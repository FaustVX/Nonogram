using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Nonogram.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Game<SolidColorBrush> Nonogram { get; }
        public MainWindow()
        {
            Nonogram = new(new[,]
            {
                {Brushes.AliceBlue, Brushes.Beige},
                {Brushes.Beige, Brushes.AliceBlue},
            }, Brushes.AliceBlue);
            Nonogram.ValidateHints(0, 0, Brushes.AliceBlue, seal: false);
            InitializeComponent();
        }
    }
}
