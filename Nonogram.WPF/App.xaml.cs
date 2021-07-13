using System;
using System.Windows;
using CommandLine;

namespace Nonogram.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var parsedPbn = Parser.Default.ParseArguments<Options.WebPbn>(args).WithParsed(o => Options.Option = o);
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }
    }
}
