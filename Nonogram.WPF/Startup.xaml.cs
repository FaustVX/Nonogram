using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nonogram.WPF.DependencyProperties;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using static Nonogram.Extensions;

namespace Nonogram.WPF
{
    /// <summary>
    /// Interaction logic for Startup.xaml
    /// </summary>
    public partial class Startup : Window, INotifyPropertyChanged
    {
        private sealed class WebPbnConverter : JsonConverter<Options.WebPbn>
        {
            private readonly Options.WebPbn _webPbn;

            public WebPbnConverter(Options.WebPbn webPbn)
                => _webPbn = webPbn;

            public override Options.WebPbn? ReadJson(JsonReader reader, Type objectType, Options.WebPbn? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                existingValue = _webPbn;
                reader.Read();
                while (reader.TokenType is JsonToken.PropertyName)
                {
                    existingValue.GetType().GetProperty((string)reader.Value!)?.SetValue(existingValue, reader.ReadAsInt32());
                    if (reader.TokenType is not JsonToken.PropertyName)
                        reader.Read();
                }
                return existingValue;
            }

            public override void WriteJson(JsonWriter writer, Options.WebPbn? value, JsonSerializer serializer)
            {
                if (value is null)
                    return;
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(value.MinWidth));
                writer.WriteValue(value.MinWidth);
                writer.WritePropertyName(nameof(value.MaxWidth));
                writer.WriteValue(value.MaxWidth);
                writer.WritePropertyName(nameof(value.MinHeight));
                writer.WriteValue(value.MinHeight);
                writer.WritePropertyName(nameof(value.MaxHeight));
                writer.WriteValue(value.MaxHeight);
                writer.WritePropertyName(nameof(value.MinColors));
                writer.WriteValue(value.MinColors);
                writer.WritePropertyName(nameof(value.MaxColors));
                writer.WriteValue(value.MaxColors);
                writer.WriteEndObject();
            }
        }

        private readonly JObject _settings;

        public Startup()
        {
            App.StartupWindow = this;
            InitializeComponent();
            _settings = Load<JObject>(nameof(Startup), autosave: true);
            WebPbnScopeOption = _settings[nameof(WebPbnScope)]?.ToObject<Options.WebPbn>(JsonSerializer.CreateDefault(new() { Converters = { new WebPbnConverter(WebPbnScopeOption) } })) ?? WebPbnScopeOption;
            if (_settings[nameof(Expander)] is JValue { Type: JTokenType.String, Value: string str })
                GetType().GetProperty(str)?.SetValue(this, true);
            WebPbnScopeOption.PropertyChanged += IndexChanged;
        }

        private void IndexChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is Options.WebPbn webPbn)
            {
                if (webPbn is Options.WebPbn { WebPbnIndex: int index and > 0 and < int.MaxValue } && e.PropertyName is nameof(webPbn.WebPbnIndex))
                {
                    webPbn.WebPbnIndex = null;
                    WebPbnIndexOption.WebPbnIndex = index;
                    WebPbnIndex = true;
                }
                _settings[nameof(WebPbnScope)] = JObject.FromObject(webPbn, new() { Converters = { new WebPbnConverter(WebPbnScopeOption) } });
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _webPbnScope;
        public bool WebPbnScope
        {
            get => _webPbnScope;
            set => OnPropertyChanged(ref _webPbnScope, in value, WebPbnScopeOption);
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
            => (WebPbnScope && WebPbnScopeOption.IsValidState)
            || (WebPbnIndex && WebPbnIndexOption.IsValidState)
            || (Load && LoadOption.IsValidState)
            || (Resize && ResizeOption.IsValidState);

        public Options.WebPbn WebPbnScopeOption { get; } = new();
        public Options.WebPbn WebPbnIndexOption { get; } = new() { WebPbnIndex = 2 };

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
            {
                Options.Option = option;
                _settings[nameof(Expander)] = propertyName;
            }
            this.NotifyProperty(PropertyChanged, nameof(CanStart));
        }

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
