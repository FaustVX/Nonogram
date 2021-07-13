using Nonogram.WPF.Converters;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

        private Game<Brush> _nonogram = default!;
        public Game<Brush> Nonogram
        {
            get => _nonogram;
            set
            {
                var autoSeal = Nonogram?.AutoSeal;
                OnPropertyChanged(ref _nonogram, in value);
                Controls.ColRow.Reset();
                CurrentColorIndex = 0;
                ICellToForegroundConverter.IgnoredBrush = ICellToBackgroundConverter.IgnoredBrush = Nonogram!.IgnoredColor;
                if (autoSeal is bool seal)
                    Nonogram.AutoSeal = seal;
                if (WindowState is WindowState.Normal)
                    SizeToContent = SizeToContent.WidthAndHeight;
            }
        }

        private Brush _currentColor = default!;
        public Brush CurrentColor
        {
            get => _currentColor;
            set => OnPropertyChanged(ref _currentColor, in value, otherPropertyNames: nameof(CurrentColorIndex));
        }

        private int _currentColorIndex;
        public int CurrentColorIndex
        {
            get => _currentColorIndex;
            set => OnPropertyChanged(ref _currentColorIndex, in value, otherPropertyNames: nameof(CurrentColor));
        }

        private int _hoverX;
        public int HoverX
        {
            get => _hoverX;
            set => OnPropertyChanged(ref _hoverX, in value);
        }

        private int _hoverY;
        public int HoverY
        {
            get => _hoverY;
            set => OnPropertyChanged(ref _hoverY, in value);
        }

        private int _size = 150;
        public double Size
        {
            get => _size / 10d;
            set => OnPropertyChanged(ref _size, in value, v => (int)(v * 10));
        }

        protected void OnPropertyChanged<T, U>(ref T storage, in U value, Func<U, T> converter, [CallerMemberName] string propertyName = default!)
            => OnPropertyChanged(ref storage, converter(value), propertyName: propertyName);

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

        public MainWindow()
        {
            InitializeComponent();
            Nonogram = TryGetRandomId();
        }

        private (ICell cell, int x, int y)? _selectedColor;
        private void CellMouseEnter(object sender, MouseEventArgs e)
        {
            var (x, y) = (HoverX, HoverY) = GetXYFromTag((FrameworkElement)sender);
            if (e.LeftButton is MouseButtonState.Pressed || e.RightButton is MouseButtonState.Pressed)
            {
                if (((_selectedColor?.x ?? -1) == x) || ((_selectedColor?.y ?? 1) == y))
                    if ((_selectedColor?.cell?.Equals(Nonogram[x, y]) ?? false) || Nonogram[x, y] is EmptyCell || (Nonogram[x, y] is SealedCell<Brush> { Seals: var seals } && !seals.Contains(CurrentColor)))
                        Change((Border)sender, e.RightButton is MouseButtonState.Pressed);
            }
        }

        private void CellMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton is not (MouseButton.Left or MouseButton.Right))
                return;
            var (x, y) = GetXYFromTag((FrameworkElement)sender);
            _selectedColor = (Nonogram[x, y], x, y);
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
                case (ModifierKeys.Control, Key.OemComma):
                    Nonogram.Tips();
                    break;
                case (_, >= Key.D1 and <= Key.D9 and var key) when (key - Key.D1) < Nonogram.PossibleColors.Length:
                    CurrentColorIndex = key - Key.D1;
                    break;
                case (ModifierKeys.Control, Key.Add or Key.OemPlus):
                    Size += 0.1;
                    break;
                case (ModifierKeys.Control, Key.Subtract or Key.OemMinus):
                    Size -= 0.1;
                    break;
            }
        }

        private void This_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.XButton1 when !Nonogram.IsCorrect:
                    Nonogram.Undo();
                    break;
                case MouseButton.XButton2:
                    Nonogram.Redo();
                    break;
            }
        }

        private void This_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var offset = e.Delta switch
            {
                > 0 => Nonogram.PossibleColors.Length - 1,
                0 => 0,
                < 0 => +1,
            };
            CurrentColorIndex = (CurrentColorIndex + offset) % Nonogram.PossibleColors.Length;
        }

        private void TipsButtonClick(object sender, RoutedEventArgs e)
            => Nonogram.Tips();

        private void CellInitialized(object sender, System.EventArgs e)
        {
            var border = (Border)sender;
            var (x, y) = Nonogram.GetCoord((ICell)border.DataContext);
            border.Tag = (x, y);
        }

        private void NewClick(object sender, RoutedEventArgs e)
            => Nonogram = TryGetRandomId();

        private static Game<Brush> TryGetRandomId()
            => Services.WebPbn.TryGetRandomId<Brush>(new(), (_, rgb) => new SolidColorBrush(Color.FromRgb((byte)rgb, (byte)(rgb >> 8), (byte)(rgb >> 16))));

        private void BoxClick(object sender, RoutedEventArgs e)
            => Nonogram.BoxSeal();

        private void This_StateChanged(object sender, EventArgs e)
            => SizeToContent = SizeToContent.WidthAndHeight;
    }
}
