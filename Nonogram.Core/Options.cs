using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using CommandLine;

namespace Nonogram
{
    public abstract class Options
    {
        public static Options Option { get; set; } = default!;
        public static Game<T> Generate<T>(Func<string, int, T> converterRGB, Func<ROSpan2D<Color>, T> converterColor, T ignoredColor = default!)
        where T : notnull
        {
            switch (Options.Option)
            {
                case null or Options.WebPbn { WebPbnIndex: null }:
                    return Services.WebPbn.TryGetRandomId(new(), converterRGB);
                case Options.WebPbn { WebPbnIndex: int idx }:
                    return Services.WebPbn.Get(idx, converterRGB);
                case Options.Resize resize:
                    {
                        var stream = resize switch
                        {
                            { FileInfo: { Exists: true } file } => file.OpenRead(),
                            { URI: { Scheme: "http" or "https" } uri } => new HttpClient().GetStreamAsync(uri).GetAwaiter().GetResult(),
                            _ => throw new Exception(),
                        };
                        var bitmap = new Bitmap(Image.FromStream(stream));
                        var array = new Color[bitmap.Height, bitmap.Width];
                        for (int x = 0; x < bitmap.Width; x++)
                            for (int y = 0; y < bitmap.Height; y++)
                                array[y, x] = bitmap.GetPixel(x, y);

                        var pattern = global::Nonogram.Extensions.ReduceArray<Color, T>(array, resize.Width, resize.Height, converterColor);
                        return Game.Create(pattern, ignoredColor);
                    }
                default: throw new Exception();
            }
        }

        public static void ParseArgs(string[] args)
        {
            Parser.Default.ParseArguments<Options.WebPbn>(args).WithParsed(o => Options.Option = o);
            Parser.Default.ParseArguments<Options.Resize>(args).WithParsed(o => Options.Option = o);
        }

        [Verb("webpbn", false, HelpText = "Retrieve pattern from 'Webpbn.com'")]
        public class WebPbn : Options
        {
            [Option('i', "id", HelpText = "Index pattern from 'Webpbn.com'")]
            public int? WebPbnIndex { get; set; }
        }

        [Verb("resize", false)]
        public class Resize : Options
        {
            [Option("file", Required = true)]
            public string File { get; set; } = default!;
            public FileInfo FileInfo => new(File);
            public Uri URI => new(File);
            [Option('w', "width", Required = true)]
            public int Width { get; set; }
            [Option('h', "height", Required = true)]
            public int Height { get; set; }
            [Option('f', "factor", Default = 15)]
            public int FactorReduction { get; set; }
        }
    }
}
