using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using CommandLine;

namespace Nonogram
{
    public abstract class Options
    {
        public static Options Option { get; set; } = default!;

        protected abstract Game<T> Execute<T>(Func<string, int, T> converterRGB, Func<ROSpan2D<Color>, T> converterColor, Func<IEnumerator<byte>, T> loader, T ignoredColor = default!)
            where T : notnull;

        public static Game<T> Generate<T>(Func<string, int, T> converterRGB, Func<ROSpan2D<Color>, T> converterColor, Func<IEnumerator<byte>, T> loader, T ignoredColor = default!)
            where T : notnull
            => (Option ??= new WebPbn()).Execute(converterRGB, converterColor, loader, ignoredColor);

        public static void ParseArgs(string[] args)
        {
            if (args is null or { Length: 0 })
                Option = new WebPbn();
            else if (args.Length is 1 && args[0].EndsWith(".picross"))
                Option = new Load() { File = args[0] };
            else if (args.Length is 1 && int.TryParse(args[0], out var index))
                Option = new WebPbn() { WebPbnIndex = index };
            else if (args.Length is 1)
                Option = new Resize() { File = args[0] };
            else
            {
                Parser.Default.ParseArguments<WebPbn>(args).WithParsed(o => Option = o);
                Parser.Default.ParseArguments<Resize>(args).WithParsed(o => Option = o);
                Parser.Default.ParseArguments<Load  >(args).WithParsed(o => Option = o);
            }
        }

        [Verb("webpbn", false, HelpText = "Retrieve pattern from 'Webpbn.com'")]
        public class WebPbn : Options
        {
            [Option('i', "id", HelpText = "Index pattern from 'Webpbn.com'")]
            public int? WebPbnIndex { get; init; }

            [Option('w', "minWidth")]
            public int MinWidth { get; init; } = 0;

            [Option('W', "maxWidth")]
            public int MaxWidth { get; init; } = int.MaxValue;

            [Option('h', "minHeight")]
            public int MinHeight { get; init; } = 0;

            [Option('H', "maxHeight")]
            public int MaxHeight { get; init; } = int.MaxValue;

            [Option('c', "minColors")]
            public int MinColors { get; init; } = 0;

            [Option('C', "maxColors")]
            public int MaxColors { get; init; } = int.MaxValue;


            protected override Game<T> Execute<T>(Func<string, int, T> converterRGB, Func<ROSpan2D<Color>, T> converterColor, Func<IEnumerator<byte>, T> loader, T ignoredColor = default!)
                => WebPbnIndex switch
                {
                    null => Services.WebPbn.TryGetRandomId(new(), (MinWidth, MaxWidth), (MinHeight, MaxHeight), (MinColors, MaxColors), converterRGB),
                    int idx => Services.WebPbn.Get(idx, converterRGB)
                };
        }

        [Verb("resize", false)]
        public class Resize : Options
        {
            [Option("file", Required = true)]
            public string File { get; init; } = default!;
            public FileInfo FileInfo => new(File);
            public Uri URI => new(File);

            [Option('w', "width", Required = true)]
            public int Width { get; init; }

            [Option('h', "height", Required = true)]
            public int Height { get; init; }

            [Option('f', "factor", Default = 100)]
            public int FactorReduction { get; init; } = 1;

            protected override Game<T> Execute<T>(Func<string, int, T> converterRGB, Func<ROSpan2D<Color>, T> converterColor, Func<IEnumerator<byte>, T> loader, T ignoredColor = default!)
            {
                using var stream = this switch
                {
                    { FileInfo: { Exists: true } file } => file.OpenRead(),
                    { URI: { Scheme: "http" or "https" } uri } => new HttpClient().GetStreamAsync(uri).GetAwaiter().GetResult(),
                    _ => throw new Exception(),
                };
                var bitmap = new Bitmap(Image.FromStream(stream));
                var array = new Color[bitmap.Height, bitmap.Width];
                for (var x = 0; x < bitmap.Width; x++)
                    for (var y = 0; y < bitmap.Height; y++)
                        array[y, x] = bitmap.GetPixel(x, y);

                var pattern = global::Nonogram.Extensions.ReduceArray<Color, T>(array, Width, Height, converterColor);
                return Game.Create(pattern, ignoredColor);
            }
        }

        [Verb("load", false)]
        public class Load : Options
        {
            [Option('l', "all", Default = false)]
            public bool LoadGame { get; init; }
            [Option("file", Required = true)]
            public string File { get; init; } = default!;
            public FileInfo FileInfo => new(File);

            protected override Game<T> Execute<T>(Func<string, int, T> converterRGB, Func<ROSpan2D<Color>, T> converterColor, Func<IEnumerator<byte>, T> loader, T ignoredColor = default!)
            {
                using var stream = FileInfo.OpenRead();
                return Game.Load(stream, loader, LoadGame);
            }
        }
    }
}
