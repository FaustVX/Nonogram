﻿using System;
using System.Windows;

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
            Options.ParseArgs(args);
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }
    }
}
