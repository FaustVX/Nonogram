using System;
using System.IO;
using CommandLine;

namespace Nonogram.WPF
{
    public abstract class Options
    {
        public static Options? Option { get; set; } = default;
        public static Game<T> Generate<T>(Func<string, int, T> converter)
        where T : notnull
        {
            switch (Options.Option)
            {
                case Options.WebPbn { WebPbnIndex: null }:
                    return Services.WebPbn.TryGetRandomId(new(), converter);
                case Options.WebPbn { WebPbnIndex: int idx }:
                    return Services.WebPbn.Get(idx, converter);
                default: throw new Exception();
            }
        }

        [Verb("webpbn", false, HelpText = "Retrieve pattern from 'Webpbn.com'")]
        public class WebPbn : Options
        {
            [Option('i', "id", HelpText = "Index pattern from 'Webpbn.com'")]
            public int? WebPbnIndex { get; set; }
        }
    }
}
