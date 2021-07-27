using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace Nonogram.WPF.DependencyProperties
{
    public static class HyperLinkExtension
    {
        public static bool GetIsExternal(DependencyObject obj)
            => (bool)obj.GetValue(IsExternalProperty);

        public static void SetIsExternal(DependencyObject obj, bool value)
            => obj.SetValue(IsExternalProperty, value);
        public static readonly DependencyProperty IsExternalProperty =
            DependencyProperty.RegisterAttached("IsExternal", typeof(bool), typeof(HyperLinkExtension), new UIPropertyMetadata(false, OnIsExternalChanged));

        private static void OnIsExternalChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            var hyperlink = (Hyperlink)sender;

            if ((bool)args.NewValue)
                hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
            else
                hyperlink.RequestNavigate -= Hyperlink_RequestNavigate;
        }

        private static void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var startInfo = new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true,
            };
            Process.Start(startInfo);
            e.Handled = true;
        }
    }
}
