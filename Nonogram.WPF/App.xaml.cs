using System;
using System.Diagnostics;
using System.Windows;

namespace Nonogram.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Window? StartupWindow { get; set; }
        [STAThread]
        public static void Main(string[] args)
        {
            Options.ParseArgs(args);
            var application = new App();
            application.InitializeComponent();
            application.StartupUri = new((Options.Option is null ? nameof(Startup) : nameof(WPF.MainWindow)) + ".xaml", UriKind.Relative);
            try
            {
                application.Run();
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButton.OKCancel, MessageBoxImage.Error) is MessageBoxResult.OK)
                    Restart();
            }
        }

        private static void Restart()
        {
            var Info = new ProcessStartInfo
            {
                Arguments = "/C choice /C Y /N /D Y /T 1 & START \"\" \"" + Environment.CommandLine + "\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            };
            Process.Start(Info);
            Process.GetCurrentProcess().Kill();
        }
    }
}
