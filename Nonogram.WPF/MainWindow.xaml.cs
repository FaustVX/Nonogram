using System;
using System.Collections.Generic;
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
        public Brush CurrentColor { get; set; }
        private readonly double _size = 15;
        private readonly List<Border> _borders;

        public MainWindow()
        {
            Nonogram = Services.WebPbn.Get<Brush>(3, (_, rgb) => new SolidColorBrush(Color.FromRgb((byte)rgb, (byte)(rgb >> 8), (byte)(rgb >> 16))));
            CurrentColor = Nonogram.PossibleColors[0];
            _borders = new(Nonogram.Width * Nonogram.Height);

            InitializeComponent();

            for (var x = 0; x < Nonogram.RowHints.Length; x++)
                rowHints.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Star) });
            foreach (var col in Nonogram.ColHints)
                colHints.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });

            ResetHints();
        }

        private void ResetHints()
        {
            Create(Nonogram.RowHints, rowHints, Orientation.Horizontal, Grid.SetRow, _size, Brushes.LightGray);
            Create(Nonogram.ColHints, colHints, Orientation.Vertical, Grid.SetColumn, _size, Brushes.LightGray);

            static void Create((Brush color, int qty, bool validated)[][] hints, Grid grid, Orientation orientation, Action<UIElement, int> setPos, double size, Brush validatedBrush)
            {
                grid.Children.Clear();
                for (var x = 0; x < hints.Length; x++)
                {
                    var sp = new StackPanel()
                    {
                        Orientation = orientation,
                    };

                    if (orientation is Orientation.Vertical)
                        sp.VerticalAlignment = VerticalAlignment.Bottom;
                    else
                        sp.HorizontalAlignment = HorizontalAlignment.Right;

                    grid.Children.Add(sp);
                    setPos(sp, x);
                    foreach (var (color, qty, validated) in hints[x])
                    {
                        var text = new TextBlock()
                        {
                            Text = qty.ToString(),
                            Background = color,
                            Width = size,
                            Height = size,
                            TextAlignment = TextAlignment.Center,
                        };
                        if (validated)
                            text.Foreground = validatedBrush;
                        sp.Children.Add(text);
                    }
                }
            }
        }

        private void CellMouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton is MouseButtonState.Pressed || e.RightButton is MouseButtonState.Pressed)
                Change((Border)sender, e.RightButton is MouseButtonState.Pressed);
        }

        private void CellMouseDown(object sender, MouseButtonEventArgs e)
            => Change((Border)sender, e.ChangedButton is MouseButton.Right);

        private void Change(Border border, bool isSealed)
        {
            if (Nonogram.IsCorrect)
                return;

            var (x, y) = GetXYFromTag(border);

            Nonogram.ValidateHints(x, y, CurrentColor, seal: isSealed);

            border.Background = Convert(x, y);

            ResetHints();

            if (Nonogram.IsCorrect)
            {
                rowHints.Children.Clear();
                colHints.Children.Clear();
                foreach (var item in _borders)
                {
                    item.BorderThickness = new(0);
                    item.Width = item.Height = _size * 2;
                }
            }
        }

        private void CellInitialize(object sender, EventArgs e)
        {
            var element = (Border)sender;
            element.Tag = _borders.Count; // element index
            _borders.Add(element);
            var (x, y) = GetXYFromTag(element);
            element.Background = Convert(x, y);
            element.Width = element.Height = _size;
        }

        private (int x, int y) GetXYFromTag(FrameworkElement element)
        {
            var tag = (int)element.Tag;
            var x = tag % Nonogram.Width;
            var y = tag / Nonogram.Height;

            return (x, y);
        }

        private Brush Convert(int x, int y)
           => Nonogram[x, y] switch
           {
               EmptyCell => Nonogram.IgnoredColor,
               ColoredCell<Brush> c => c.Color,
               AllColoredSealCell => CurrentColor,
               //SealedCell<Brush> seal when seal.Seals.Contains(window.CurrentColor) => CurrentColor,
               _ => Brushes.LightGray,
           };
    }
}
