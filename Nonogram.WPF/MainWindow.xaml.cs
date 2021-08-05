using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Nonogram.WPF.Converters;
using Nonogram.WPF.DependencyProperties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static Nonogram.Extensions;
using static Nonogram.WPF.Extensions;

namespace Nonogram.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly JObject _settings;

        private Game<Brush> _nonogram = default!;
        public Game<Brush> Nonogram
        {
            get => _nonogram;
            set
            {
                var autoSeal = Nonogram?.AutoSeal;
                ColRow.Helper.Reset(this);
                CanBeSelected.SetSelectedColor(this, 0);
                this.OnPropertyChanged(ref _nonogram, in value, PropertyChanged);
                ICellToForegroundConverter.IgnoredBrush = ICellToBackgroundConverter.IgnoredBrush = Nonogram!.IgnoredColor;
                if (autoSeal is bool seal)
                    Nonogram.AutoSeal = seal;
                if (AutoBox)
                    Nonogram.BoxSeal();
                if (WindowState is WindowState.Normal)
                    SizeToContent = SizeToContent.WidthAndHeight;
            }
        }

        public bool AutoSeal
        {
            get => Nonogram?.AutoSeal ?? _settings?.Value<bool>(nameof(AutoSeal)) ?? true;
            set
            {
                _settings[nameof(AutoSeal)] = Nonogram.AutoSeal = value;
                this.NotifyProperty(PropertyChanged);
            }
        }

        public bool AutoBox
            => _settings.Value<bool>(nameof(AutoBox));

        public int DragOffset
            => _settings.Value<int>(nameof(DragOffset));

        public Brush CurrentColor
            => Nonogram.PossibleColors[CurrentColorIndex].Value;

        public int CurrentColorIndex
            => CanBeSelected.GetSelectedColor(this);

        public bool IsMeasureStarted
            => DependencyProperties.Measure.GetIsStarted(this);

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
            _settings = Load<JObject>(nameof(MainWindow), autosave: true);
            _settings[nameof(DragOffset)] ??= new JValue(1);
            _settings[nameof(AutoBox)] ??= new JValue(true);
            Nonogram = Generate();
            AutoSeal = _settings.Value<bool?>(nameof(AutoSeal)) ?? true;
        }

        private (ICell cell, int x, int y, Orientation? orientation)? _selectedColor;
        private void CellMouseEnter(object sender, MouseEventArgs e)
        {
            var (x, y) = GetXYFromTag((FrameworkElement)sender);
            ColRow.SetHoverX(this, x);
            ColRow.SetHoverY(this, y);
            if (_selectedColor is not (ICell, int, int, Orientation or null) sel)
                return;
            if (IsMeasureStarted)
                return;
            if (e.LeftButton is MouseButtonState.Pressed || e.RightButton is MouseButtonState.Pressed)
                if (sel.orientation is not Orientation.Vertical && sel.y == y)
                {
                    if (sel.orientation is null)
                        _selectedColor = (sel.cell, sel.x, sel.y, Orientation.Horizontal);
                    Execute(sel.cell, x, y);
                }
                else if (sel.orientation is not Orientation.Horizontal && sel.x == x)
                {
                    if (sel.orientation is null)
                        _selectedColor = (sel.cell, sel.x, sel.y, Orientation.Vertical);
                    Execute(sel.cell, x, y);
                }
                else if (sel.orientation is Orientation.Vertical && Math.Abs(x - sel.x) <= DragOffset)
                {
                    x = sel.x;
                    ColRow.SetHoverX(this, x);
                    Execute(sel.cell, x, y);
                }
                else if (sel.orientation is Orientation.Horizontal && Math.Abs(y - sel.y) <= DragOffset)
                {
                    y = sel.y;
                    ColRow.SetHoverY(this, y);
                    Execute(sel.cell, x, y);
                }
                else
                    _selectedColor = null;

            void Execute(ICell cell, int x, int y)
            {
                if (cell.Equals(Nonogram[x, y]) || Nonogram[x, y] is EmptyCell || (Nonogram[x, y] is SealedCell<Brush> { Seals: var seals } && !seals.Contains(CurrentColor)))
                {
                    Change(x, y, e.RightButton is MouseButtonState.Pressed);
                    e.Handled = true;
                }
            }
        }

        private void CellMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMeasureStarted || e.ChangedButton is not (MouseButton.Left or MouseButton.Right))
                return;
            var (x, y) = GetXYFromTag((FrameworkElement)sender);
            _selectedColor = (Nonogram[x, y], x, y, null);
            Change(x, y, e.ChangedButton is MouseButton.Right);
            e.Handled = true;
        }

        private void Change(int x, int y, bool isSealed)
        {
            if (Nonogram.IsCorrect || Nonogram.PossibleColors[CurrentColorIndex].Validated)
                return;

            Nonogram.ValidateHints(x, y, Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? Nonogram.IgnoredColor : CurrentColor, seal: isSealed);
        }

        private void CompleteSealCell(object? parameter)
        {
            if (Nonogram.IsCorrect || Nonogram.PossibleColors[CurrentColorIndex].Validated)
                return;

            var (x, y) = GetXYFromTag((FrameworkElement)parameter!);
            Nonogram.ValidateHints(x, y, Nonogram.IgnoredColor, seal: true);
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

        private void SaveImage(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog()
            {
                AddExtension = true,
                Filter = "png Image|*.png",
                DefaultExt = "*.png",
            };
            if (saveDialog.ShowDialog(this) is not true)
                return;
            var saveFile = new FileInfo(saveDialog.FileName);
            using var stream = saveFile.OpenWrite();
            var bitmap = new System.Drawing.Bitmap(Nonogram.Width, Nonogram.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            foreach (var (x, y) in Nonogram.GenerateCoord())
                if (GetColor(Nonogram, x, y) is { A: var a, R: var r, G: var g, B: var b })
                    bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb(a, r, g, b));
            bitmap.Save(stream, ImageFormat.Png);

            static Color GetColor(Game<Brush> nonogram, int x, int y)
                => nonogram[x, y] is ColoredCell<Brush> { Color: SolidColorBrush { Color: var color } }
                    ? color
                    : ((SolidColorBrush)nonogram.IgnoredColor).Color;
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
            border.InputBindings.Add(new(new ParameteredRelayCommand(CompleteSealCell), new MouseGesture(MouseAction.RightDoubleClick)) { CommandParameter = border });
            var (x, y) = Nonogram.GetCoord((ICell)border.DataContext);
            border.Tag = (x, y);
        }

        private void NewClick(object sender, RoutedEventArgs e)
            => Close();

        private static Game<Brush> Generate()
        {
            var cache = new Dictionary<Color, Brush>();
            return Options.Generate(
                           (_, rgb) => new SolidColorBrush(Color.FromRgb((byte)rgb, (byte)(rgb >> 8), (byte)(rgb >> 16))),
                           span =>
                           {
                               var ratio = ((Options.Resize)Options.Option!).FactorReduction;
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
        {
            SaveClick(default!, default!);
            App.StartupWindow?.Show();
        }
    }
}
