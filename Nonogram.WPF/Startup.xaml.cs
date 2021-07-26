using Microsoft.Win32;
using Nonogram.WPF.DependencyProperties;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Nonogram.WPF
{
    /// <summary>
    /// Interaction logic for Startup.xaml
    /// </summary>
    public partial class Startup : Window, INotifyPropertyChanged
    {
        public Startup()
        {
            App.StartupWindow = this;
            InitializeComponent();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _webPbnScope;
        public bool WebPbnScope
        {
            get => _webPbnScope;
            set => OnPropertyChanged(ref _webPbnScope, in value, WebPbnOption);
        }

        private bool _webPbnIndex;
        public bool WebPbnIndex
        {
            get => _webPbnIndex;
            set => OnPropertyChanged(ref _webPbnIndex, in value, WebPbnIndexOption);
        }

        private bool _load;
        public bool Load
        {
            get => _load;
            set => OnPropertyChanged(ref _load, in value, LoadOption);
        }

        private bool _resize;
        public bool Resize
        {
            get => _resize;
            set => OnPropertyChanged(ref _resize, in value, ResizeOption);
        }

        public bool CanStart
            => (WebPbnScope && WebPbnOption.IsValidState)
               || (WebPbnIndex && (WebPbnRandomOption.IsValidState || WebPbnIndexOption.IsValidState))
               || (Load && LoadOption.IsValidState)
               || (Resize && ResizeOption.IsValidState);

        public Options.WebPbn WebPbnOption { get; } = new();
        public Options.WebPbn WebPbnIndexOption { get; } = new() { WebPbnIndex = 0 };
        public Options.WebPbn WebPbnRandomOption { get; } = new() { WebPbnIndex = 0 };

        public Options.Resize ResizeOption { get; } = new();

        public Options.Load LoadOption { get; } = new();

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Hide();
        }

        private void OnPropertyChanged(ref bool storage, in bool value, Options option, [CallerMemberName]string propertyName = default!)
        {
            if (this.OnPropertyChanged(ref storage, in value, PropertyChanged, propertyName) && storage)
                Options.Option = option;
            PropertyChanged?.Invoke(this, new(nameof(CanStart)));
        }

        private void IsRandom_Checked(object sender, RoutedEventArgs e)
            => Options.Option = ((CheckBox)sender).IsChecked is true ? WebPbnRandomOption : WebPbnIndexOption;

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var parent = ((TextBox)((FrameworkElement)sender).TemplatedParent);

            var openDialog = new OpenFileDialog()
            {
                AddExtension = true,
                Filter = FileExtension.GetExtension(parent),
            };
            if (openDialog.ShowDialog(this) is not true)
                return;

            parent.Text = openDialog.FileName;
        }
    }
}
