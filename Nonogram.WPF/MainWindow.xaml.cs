using Nonogram.WPF.Converters;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Nonogram.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public Game<Brush> Nonogram { get; }
        private Brush _currentColor = default!;
        public Brush CurrentColor
        {
            get => _currentColor;
            set
            {
                if (_currentColor == value)
                    return;
                _currentColor = value;
                PropertyChanged?.Invoke(this, new(nameof(CurrentColor)));
            }
        }
        private double Size
            => (double)Resources["Size"];
        private ICellToForegroundConverter ICellToForegroundConverter
            => (ICellToForegroundConverter)Resources["ICellToForegroundConverter"];
        private ICellToBackgroundConverter ICellToBackgroundConverter
            => (ICellToBackgroundConverter)Resources["ICellToBackgroundConverter"];

        private readonly Border[,] _borders;

        public MainWindow()
        {
            Nonogram = Services.WebPbn.Get<Brush>(2, (_, rgb) => new SolidColorBrush(Color.FromRgb((byte)rgb, (byte)(rgb >> 8), (byte)(rgb >> 16))));
            _borders = new Border[Nonogram.Width, Nonogram.Height];

            InitializeComponent();
            ICellToForegroundConverter.IgnoredBrush = ICellToBackgroundConverter.IgnoredBrush = Nonogram.IgnoredColor;

            foreach (var c in Nonogram.PossibleColors)
            {
                var radio = new RadioButton()
                {
                    GroupName = "Color",
                    Background = c,
                    Width = Size,
                };
                radio.Checked += RadioSelected;
                colors.Children.Add(radio);
            }
            ((RadioButton)colors.Children[0]).IsChecked = true;

            void RadioSelected(object sender, RoutedEventArgs e)
            {
                CurrentColor = ((Control)sender).Background;
            }
        }

        private ICell? _selectedColor;
        private void CellMouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton is MouseButtonState.Pressed || e.RightButton is MouseButtonState.Pressed)
            {
                var (x, y) = GetXYFromTag((FrameworkElement)sender);
                if ((_selectedColor?.Equals(Nonogram[x, y]) ?? false) || Nonogram[x, y] is EmptyCell || (Nonogram[x, y] is SealedCell<Brush> { Seals: var seals } && !seals.Contains(CurrentColor)))
                    Change((Border)sender, e.RightButton is MouseButtonState.Pressed);
            }
        }

        private void CellMouseDown(object sender, MouseButtonEventArgs e)
        {
            var (x, y) = GetXYFromTag((FrameworkElement)sender);
            _selectedColor = Nonogram[x, y];
            Change((Border)sender, e.ChangedButton is MouseButton.Right);
        }

        private void Change(Border border, bool isSealed)
        {
            if (Nonogram.IsCorrect)
                return;

            var (x, y) = GetXYFromTag(border);

            Nonogram.ValidateHints(x, y, Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? Nonogram.IgnoredColor : CurrentColor, seal: isSealed);
        }

        private static (int x, int y) GetXYFromTag(FrameworkElement element)
            => ((int, int))element.Tag;

        private void This_KeyUp(object sender, KeyEventArgs e)
        {
            switch ((e.KeyboardDevice.Modifiers, e.Key))
            {
                case (ModifierKeys.Control, Key.Z) when !Nonogram.IsCorrect:
                    Nonogram.Undo();
                    break;
                case (ModifierKeys.Control, Key.Y):
                    Nonogram.Redo();
                    break;
                case (_, >= Key.D1 and <= Key.D9 and var key) when (key - Key.D1) < colors.Children.Count:
                    ((RadioButton)colors.Children[key - Key.D1]).IsChecked = true;
                    break;
            }
        }

        private void CellInitialized(object sender, System.EventArgs e)
        {
            var border = (Border)sender;
            var (x, y) = Nonogram.GetCoord((ICell)border.DataContext);
            border.Tag = (x, y);
            _borders[x, y] = border;
        }
    }
}
