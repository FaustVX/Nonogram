using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Nonogram.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Game<Brush> Nonogram { get; }
        public Brush CurrentColor { get; set; } = default!;
        private double Size
            => (double)Resources["Size"];
        private readonly Border[,] _borders;

        public MainWindow()
        {
            Nonogram = Services.WebPbn.Get<Brush>(2, (_, rgb) => new SolidColorBrush(Color.FromRgb((byte)rgb, (byte)(rgb >> 8), (byte)(rgb >> 16))));
            _borders = new Border[Nonogram.Width, Nonogram.Height];

            InitializeComponent();

            for (var x = 0; x < Nonogram.RowHints.Length; x++)
                cells.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Star) });
            foreach (var col in Nonogram.ColHints)
                cells.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });

            foreach (var (x, y) in Nonogram.GenerateCoord())
            {
                var text = new TextBlock()
                {
                    Text = "X",
                    TextAlignment = TextAlignment.Center,
                    Foreground = Brushes.Transparent,
                    Background = Brushes.Transparent,
                };
                var border = _borders[x, y] = new()
                {
                    BorderThickness = new(1),
                    BorderBrush = Brushes.Gray,
                    CornerRadius = new(0),
                    Background = Convert(x, y),
                    Width = Size,
                    Height = Size,
                    Tag = (x, y),
                    Child = text,
                };
                border.MouseDown += CellMouseDown;
                border.MouseEnter += CellMouseEnter;
                Grid.SetRow(border, y);
                Grid.SetColumn(border, x);
                cells.Children.Add(border);
            }

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
                foreach (var (x, y) in Nonogram.GenerateCoord())
                    ResetSeals(x, y);
            }
        }

        private void ResetHints(int x, int y)
            => ResetSeals(x, y);

        private void ResetSeals(int x, int y)
        {
            ((TextBlock)_borders[x, y].Child).Foreground = Nonogram[x, y] switch
            {
                SealedCell<Brush> { Seals: var seals } when seals.Contains(CurrentColor) => CurrentColor,
                AllColoredSealCell => Nonogram.IgnoredColor,
                _ => Brushes.Transparent
            };
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

            var text = (TextBlock)border.Child;

            border.Background = Convert(x, y);

            ResetHints(x, y);

            if (Nonogram.IsCorrect)
                foreach (var item in _borders)
                {
                    item.BorderThickness = new(0);
                    item.Width = item.Height = Size * 2;
                }
        }

        private static (int x, int y) GetXYFromTag(FrameworkElement element)
            => ((int, int))element.Tag;

        private Brush Convert(int x, int y)
           => Nonogram[x, y] switch
           {
               EmptyCell => Nonogram.IgnoredColor,
               ColoredCell<Brush> c => c.Color,
               _ => Brushes.Gray,
           };

        private void This_KeyUp(object sender, KeyEventArgs e)
        {
            switch((e.KeyboardDevice.Modifiers, e.Key))
            {
                case (ModifierKeys.Control, Key.Z) when !Nonogram.IsCorrect:
                    {
                        if (Nonogram.Undo() is (var x, var y))
                        {
                            _borders[x, y].Background = Convert(x, y);
                            ResetHints(x, y);
                        }
                        break;
                    }
                case (ModifierKeys.Control, Key.Y):
                    {
                        if (Nonogram.Redo() is (var x, var y))
                        {
                            _borders[x, y].Background = Convert(x, y);
                            ResetHints(x, y);
                        }
                        break;
                    }
                case (_, >= Key.D1 and <= Key.D9 and var key) when (key - Key.D1) < colors.Children.Count:
                    ((RadioButton)colors.Children[key - Key.D1]).IsChecked = true;
                    break;
            }
        }
    }
}
