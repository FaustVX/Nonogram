using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;

namespace Nonogram.Services
{
    public static class WebPbn
    {
        public static Game<T> Get<T>(int id, Func<string, int, T> converter)
        where T : notnull
        {
            var xml = XDocument.Load(Post(id));
            var puzzle = xml.Element("puzzleset")!.Element("puzzle")!;
            var defaultColor = puzzle.Attribute("defaultcolor")!.Value;
            T ignoredColor = default!;
            var dict = puzzle.Elements("color")
                .Select(elem => (@char: elem.Attribute("char")!.Value[0], name: elem.Attribute("name")!.Value, color: int.Parse(elem.Value, NumberStyles.AllowHexSpecifier)))
                .ToDictionary(t => t.@char, t =>
                {
                    var conv = converter(t.name, t.color);
                    if (t.name == defaultColor)
                        ignoredColor = conv;
                    return conv;
                });

            var pattern = puzzle.Element("solution")!
                .Element("image")!
                .Value
                .Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.ToCharArray()).ToArray()
                .To2DArray(c => dict[c]);
            return new(pattern, ignoredColor);

            static Stream Post(int id)
            {
                //$"go=1&id={id}&fmt=xml&xml_soln=on"
                using var client = new HttpClient();
                return client.PostAsync($"https://webpbn.com/export.cgi/webpbn{id:000000}.xml",
                    new FormUrlEncodedContent(new KeyValuePair<string?, string?>[]
                    {
                        new("go", "1"),
                        new("id", id.ToString()),
                        new("fmt", "xml"),
                        new("xml_soln", "on")
                    }))
                    .GetAwaiter().GetResult()
                    .Content
                    .ReadAsStream();
            }
        }
    }
}