using Microsoft.Win32;
using Nonogram.WPF.Converters;
using Nonogram.WPF.DependencyProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static Nonogram.WPF.Extensions;

namespace Nonogram.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Game<Brush> _nonogram = default!;
        public Game<Brush> Nonogram
        {
            get => _nonogram;
            set
            {
                var autoSeal = Nonogram?.AutoSeal;
                ColRow.Helper.Reset(this);
                CanBeSelected.SetSelectedColor(this, 0);
                OnPropertyChanged(ref _nonogram, in value);
                ICellToForegroundConverter.IgnoredBrush = ICellToBackgroundConverter.IgnoredBrush = Nonogram!.IgnoredColor;
                if (autoSeal is bool seal)
                    Nonogram.AutoSeal = seal;
                if (WindowState is WindowState.Normal)
                    SizeToContent = SizeToContent.WidthAndHeight;
            }
        }

        public Brush CurrentColor
            => Nonogram.PossibleColors[CurrentColorIndex].Value;

        public int CurrentColorIndex
            => CanBeSelected.GetSelectedColor(this);

        public bool IsMeasureStarted
            => DependencyProperties.Measure.GetIsStarted(this);

        protected void OnPropertyChanged<T>(ref T storage, in T value, Func<T, bool> validator = default!, [CallerMemberName] string propertyName = default!, params string[] otherPropertyNames)
        {
            if ((storage is IEquatable<T> comp && !comp.Equals(value)) || (!storage?.Equals(value) ?? (value is not null)))
            {
                if (validator?.Invoke(value) ?? true)
                {
                    storage = value;
                    PropertyChanged?.Invoke(this, new(propertyName));
                    foreach (var name in otherPropertyNames)
                        PropertyChanged?.Invoke(this, new(name));
                }
            }
        }

        private ICellToForegroundConverter ICellToForegroundConverter
            => (ICellToForegroundConverter)Resources["ICellToForegroundConverter"];
        private ICellToBackgroundConverter ICellToBackgroundConverter
            => (ICellToBackgroundConverter)Resources["ICellToBackgroundConverter"];

        static MainWindow()
        {
            Game<Brush>.ColorEqualizer = (a, b) => (a, b) is (SolidColorBrush { Color: var c1 }, SolidColorBrush { Color: var c2 }) ? Color.Equals(c1, c2) : (a?.Equals(b) ?? (a, b) is (null, null));
            Game<Brush>.ColorSerializer = brush => brush is SolidColorBrush b ? new[] { b.Color.A, b.Color.R, b.Color.G, b.Color.B } : null!;
        }
        public MainWindow()
        {
            InitializeComponent();
            Nonogram = Generate();
        }

        private (ICell cell, int x, int y)? _selectedColor;
        private void CellMouseEnter(object sender, MouseEventArgs e)
        {
            var (x, y) = GetXYFromTag((FrameworkElement)sender);
            ColRow.SetHoverX(this, x);
            ColRow.SetHoverY(this, y);
            if (IsMeasureStarted)
                return;
            if (e.LeftButton is MouseButtonState.Pressed || e.RightButton is MouseButtonState.Pressed)
                if (((_selectedColor?.x ?? -1) == x) || ((_selectedColor?.y ?? 1) == y))
                    if ((_selectedColor?.cell?.Equals(Nonogram[x, y]) ?? false) || Nonogram[x, y] is EmptyCell || (Nonogram[x, y] is SealedCell<Brush> { Seals: var seals } && !seals.Contains(CurrentColor)))
                    {
                        Change((FrameworkElement)sender, e.RightButton is MouseButtonState.Pressed);
                        e.Handled = true;
                    }
        }

        private void CellMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMeasureStarted || e.ChangedButton is not (MouseButton.Left or MouseButton.Right))
                return;
            var (x, y) = GetXYFromTag((FrameworkElement)sender);
            _selectedColor = (Nonogram[x, y], x, y);
            Change((FrameworkElement)sender, e.ChangedButton is MouseButton.Right);
            e.Handled = true;
        }

        private void Change(FrameworkElement element, bool isSealed)
        {
            if (Nonogram.IsCorrect)
                return;

            var (x, y) = GetXYFromTag(element);

            Nonogram.ValidateHints(x, y, Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? Nonogram.IgnoredColor : CurrentColor, seal: isSealed);
        }

        private void This_KeyUp(object sender, KeyEventArgs e)
        {
            switch ((e.KeyboardDevice.Modifiers, e.Key))
            {
                case (ModifierKeys.Control, Key.OemComma):
                    Nonogram.Tips();
                    e.Handled = true;
                    break;
                case (ModifierKeys.Control, Key.S):
                    SaveClick(default!, default!);
                    break;
                case (ModifierKeys.Control, Key.O):
                    LoadClick(default!, default!);
                    break;
            }
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog()
            {
                AddExtension = true,
                Filter = "Picross files|*.picross",
                DefaultExt = "*.picross",
            };
            if (saveDialog.ShowDialog(this) is not true)
                return;
            var saveGame = MessageBox.Show(this, "Save also the current game ?", "Save all ?", MessageBoxButton.YesNo, MessageBoxImage.Question) is MessageBoxResult.Yes;
            var save = saveGame ? Nonogram.SaveGame() : Nonogram.SavePattern();
            var saveFile = new FileInfo(saveDialog.FileName);
            using var stream = saveFile.OpenWrite();
            stream.Write(save);
        }

        private void LoadClick(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog()
            {
                AddExtension = true,
                Filter = "Picross files|*.picross",
                DefaultExt = "*.picross",
            };
            if (openDialog.ShowDialog(this) is not true)
                return;
            var loadGame = MessageBox.Show(this, "Load also the saved game ?", "Load all ?", MessageBoxButton.YesNo, MessageBoxImage.Question) is MessageBoxResult.Yes;
            var openFile = new FileInfo(openDialog.FileName);
            using var stream = openFile.OpenRead();
            Nonogram = Game.Load(stream, ColorLoader, loadGame);
        }

        private void TipsButtonClick(object sender, RoutedEventArgs e)
            => Nonogram.Tips();

        private void CellInitialized(object sender, EventArgs e)
        {
            var border = (FrameworkElement)sender;
            var (x, y) = Nonogram.GetCoord((ICell)border.DataContext);
            border.Tag = (x, y);
        }

        private void NewClick(object sender, RoutedEventArgs e)
            => Nonogram = Generate();

        private static Game<Brush> Generate()
        {
            var cache = new Dictionary<Color, Brush>();
            return Options.Generate(
                           (_, rgb) => new SolidColorBrush(Color.FromRgb((byte)rgb, (byte)(rgb >> 8), (byte)(rgb >> 16))),
                           span =>
                           {
                               var ratio = ((Options.Resize)Options.Option).FactorReduction;
                               var count = (ulong)span.Width * (ulong)span.Height;
                               var (r, g, b) = span.Aggregate((r: 0UL, g: 0UL, b: 0UL),
                                   (acc, col) => (acc.r + col.R, acc.g + col.G, acc.b + col.B),
                                   acc => (r: (byte)(acc.r / count), g: (byte)(acc.g / count), b: (byte)(acc.b / count)));
                               return TryGet(Color.FromRgb((byte)(r / ratio * ratio), (byte)(g / ratio * ratio), (byte)(b / ratio * ratio)));
                           },
                           ColorLoader, Brushes.Black);

            Brush TryGet(Color color)
            {
                if (cache.TryGetValue(color, out var value))
                    return value;
                return cache[color] = new SolidColorBrush(color);
            }
        }

        private static Brush ColorLoader(IEnumerator<byte> enumerator)
            => new SolidColorBrush(Color.FromArgb(enumerator.GetNext(), enumerator.GetNext(), enumerator.GetNext(), enumerator.GetNext()));

        private void BoxClick(object sender, RoutedEventArgs e)
            => Nonogram.BoxSeal();

        private void This_StateChanged(object sender, EventArgs e)
            => SizeToContent = SizeToContent.WidthAndHeight;

        private void This_Closing(object sender, CancelEventArgs e)
            => SaveClick(default!, default!);
    }
}
