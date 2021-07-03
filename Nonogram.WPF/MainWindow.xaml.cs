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
        public Brush CurrentColor { get; set; } = default!;
        private readonly double _size = 15;
        private readonly Border[,] _borders;

        public MainWindow()
        {
            Nonogram = Services.WebPbn.Get<Brush>(741, (_, rgb) => new SolidColorBrush(Color.FromRgb((byte)rgb, (byte)(rgb >> 8), (byte)(rgb >> 16))));
            _borders = new Border[Nonogram.Height, Nonogram.Width];

            InitializeComponent();

            for (var x = 0; x < Nonogram.RowHints.Length; x++)
            {
                rowHints.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Star) });
                cells.RowDefinitions.Add(new() { Height = new(1, GridUnitType.Star) });
            }
            foreach (var col in Nonogram.ColHints)
            {
                colHints.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
                cells.ColumnDefinitions.Add(new() { Width = new(1, GridUnitType.Star) });
            }

            Create(Nonogram.RowHints, rowHints, Orientation.Horizontal, Grid.SetRow, _size, Brushes.LightGray);
            Create(Nonogram.ColHints, colHints, Orientation.Vertical, Grid.SetColumn, _size, Brushes.LightGray);

            for (var x = 0; x < Nonogram.Width; x++)
                for (var y = 0; y < Nonogram.Height; y++)
                {
                    var border = _borders[y, x] = new()
                    {
                        BorderThickness = new(1),
                        BorderBrush = Brushes.Gray,
                        CornerRadius = new(0),
                        Background = Convert(x, y),
                        Width = _size,
                        Height = _size,
                        Tag = (x, y),
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
                    Width = _size,
                };
                radio.Checked += RadioSelected;
                colors.Children.Add(radio);
            }
            ((RadioButton)colors.Children[0]).IsChecked = true;

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
                            ToolTip = qty.ToString(),
                        };
                        if (validated)
                            text.Foreground = validatedBrush;
                        sp.Children.Add(text);
                    }
                }
            }

            void RadioSelected(object sender, RoutedEventArgs e)
                => CurrentColor = ((Control)sender).Background;
        }

        private void ResetHints(int x, int y)
        {
            Create(Nonogram.RowHints, y, rowHints, Orientation.Horizontal, Grid.SetRow, _size, Brushes.LightGray);
            Create(Nonogram.ColHints, x, colHints, Orientation.Vertical, Grid.SetColumn, _size, Brushes.LightGray);

            static void Create((Brush color, int qty, bool validated)[][] hints, int i, Grid grid, Orientation orientation, Action<UIElement, int> setPos, double size, Brush validatedBrush)
            {
                var sp = (StackPanel)grid.Children[i];
                sp.Children.Clear();
                foreach (var (color, qty, validated) in hints[i])
                {
                    var text = new TextBlock()
                    {
                        Text = qty.ToString(),
                        Background = color,
                        Width = size,
                        Height = size,
                        TextAlignment = TextAlignment.Center,
                        ToolTip = qty.ToString(),
                    };
                    text.Foreground = (validated ? validatedBrush : (Brush)TextBlock.ForegroundProperty.DefaultMetadata.DefaultValue);
                    sp.Children.Add(text);
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

            ResetHints(x, y);

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

        private static (int x, int y) GetXYFromTag(FrameworkElement element)
            => ((int, int))element.Tag;

        private Brush Convert(int x, int y)
           => Nonogram[x, y] switch
           {
               EmptyCell => Nonogram.IgnoredColor,
               ColoredCell<Brush> c => c.Color,
               AllColoredSealCell => CurrentColor,
               //SealedCell<Brush> seal when seal.Seals.Contains(window.CurrentColor) => CurrentColor,
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
                            _borders[y, x].Background = Convert(x, y);
                            ResetHints(x, y);
                        }
                        break;
                    }
                case (ModifierKeys.Control, Key.Y):
                    {
                        if (Nonogram.Redo() is (var x, var y))
                        {
                            _borders[y, x].Background = Convert(x, y);
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
