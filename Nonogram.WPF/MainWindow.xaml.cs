using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Nonogram.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Game<Brush> Nonogram { get; }
        public Brush CurrentColor { get; set; }

        public MainWindow()
        {
            Nonogram = new(new[,]
            {
                {Brushes.Red, Brushes.Black},
                {Brushes.Green, Brushes.Yellow},
            }, Brushes.Black);
            CurrentColor = Nonogram.PossibleColors[0];
            Nonogram.ValidateHints(0, 0, CurrentColor, seal: false);
            InitializeComponent();
        }

        private int _btnIndex;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var tag = (int)button.Tag;
            var x = tag % Nonogram.Width;
            var y = tag / Nonogram.Height;

            Nonogram.ValidateHints(x, y, CurrentColor, seal: false);

            //Ugly but works
            var binding = button.GetBindingExpression(Button.BackgroundProperty);
            button.SetBinding(binding.TargetProperty, new Binding()
            {
                Source = Nonogram[x, y],
                Converter = binding.ParentBinding.Converter,
                ConverterParameter = binding.ParentBinding.ConverterParameter,
                Mode = binding.ParentBinding.Mode,
            });
        }

        private void Button_Initialized(object sender, EventArgs e)
        {
            var button = (Button)sender;
            button.Tag = _btnIndex++;
        }
    }
}
