using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net.Http;
using CommandLine;

namespace Nonogram
{
    public abstract class Options : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public static Options? Option { get; set; }

        public abstract bool IsValidState { get; }

        protected abstract Game<T> Execute<T>(Func<string, int, T> converterRGB, Func<ROSpan2D<Color>, T> converterColor, Func<IEnumerator<byte>, T> loader, T ignoredColor = default!)
            where T : notnull;

        public static Game<T> Generate<T>(Func<string, int, T> converterRGB, Func<ROSpan2D<Color>, T> converterColor, Func<IEnumerator<byte>, T> loader, T ignoredColor = default!)
            where T : notnull
            => (Option ??= new WebPbn()).Execute(converterRGB, converterColor, loader, ignoredColor);

        public static void ParseArgs(string[] args)
        {
            if (args is null or { Length: 0 })
                Option = default;
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
            public override bool IsValidState => MinWidth <= MaxWidth && MinHeight <= MaxHeight && MinColors <= MaxColors;

            private int? _webPbnIndex = null;
            [Option('i', "id", HelpText = "Index pattern from 'Webpbn.com'")]
            public int? WebPbnIndex
            {
                get => _webPbnIndex;
                set => this.OnPropertyChanged(ref _webPbnIndex, in value, PropertyChanged);
            }

            private int _minWidth = 0;
            [Option('w', "minWidth")]
            public int MinWidth
{
                get => _minWidth;
                init => this.OnPropertyChanged(ref _minWidth, in value, PropertyChanged);
            }

            private int _maxWidth = 1000;
            [Option('W', "maxWidth")]
            public int MaxWidth
{
                get => _maxWidth;
                init => this.OnPropertyChanged(ref _maxWidth, in value, PropertyChanged);
            }

            private int _minHeight = 0;
            [Option('h', "minHeight")]
            public int MinHeight
{
                get => _minHeight;
                init => this.OnPropertyChanged(ref _minHeight, in value, PropertyChanged);
            }

            private int _maxHeight = 1000;
            [Option('H', "maxHeight")]
            public int MaxHeight
{
                get => _maxHeight;
                init => this.OnPropertyChanged(ref _maxHeight, in value, PropertyChanged);
            }

            private int _minColors = 1;
            [Option('c', "minColors")]
            public int MinColors
{
                get => _minColors;
                init => this.OnPropertyChanged(ref _minColors, in value, PropertyChanged);
            }

            private int _maxColors = 1000;
            [Option('C', "maxColors")]
            public int MaxColors
{
                get => _maxColors;
                init => this.OnPropertyChanged(ref _maxColors, in value, PropertyChanged);
            }

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
            public override bool IsValidState => !string.IsNullOrWhiteSpace(File) && (FileInfo.Exists || URI.Scheme.StartsWith("http", StringComparison.InvariantCultureIgnoreCase));

            private string _file = default!;
            [Option("file", Required = true)]
            public string File
            {
                get => _file;
                init
                {
                    if (this.OnPropertyChanged(ref _file, in value, PropertyChanged) && IsValidState && FileInfo.Exists)
                    {
                        var bitmap = new Bitmap(FileInfo.OpenRead());
                        (Width, Height) = (bitmap.Width, bitmap.Height);
                        this.NotifyProperty(PropertyChanged, nameof(Width));
                        this.NotifyProperty(PropertyChanged, nameof(Height));
                    }
                }
            }
            public FileInfo FileInfo => new(File);
            public Uri URI => new(File);

            [Option('w', "width", Required = true)]
            public int Width { get; init; } = 10;

            [Option('h', "height", Required = true)]
            public int Height { get; init; } = 10;

            [Option('f', "factor", Default = 100)]
            public int FactorReduction { get; init; } = 100;

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

                var pattern = Extensions.ReduceArray(array, Width, Height, converterColor);
                return Game.Create(pattern, ignoredColor);
            }
        }

        [Verb("load", false)]
        public class Load : Options
        {
            public override bool IsValidState => !string.IsNullOrWhiteSpace(File) && FileInfo.Exists;

            private bool _loadGame;
            [Option('l', "all", Default = false)]
            public bool LoadGame
            {
                get => _loadGame;
                init => this.OnPropertyChanged(ref _loadGame, in value, PropertyChanged);
            }

            private string _file = "";
            [Option("file", Required = true)]
            public string File
            {
                get => _file;
                init => this.OnPropertyChanged(ref _file, in value, PropertyChanged);
            }
            public FileInfo FileInfo => new(File);

            protected override Game<T> Execute<T>(Func<string, int, T> converterRGB, Func<ROSpan2D<Color>, T> converterColor, Func<IEnumerator<byte>, T> loader, T ignoredColor = default!)
            {
                using var stream = FileInfo.OpenRead();
                return Game.Load(stream, loader, LoadGame);
            }
        }
    }
}
