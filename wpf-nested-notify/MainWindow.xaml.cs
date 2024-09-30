using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace wpf_nested_notify
{
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();
    }
    partial class MainPageViewModel : ObservableObject
    {
        // This 'is' an ObservableObject because it provides INotifyPropertyChanged.
        // It 'is not' an ObservableProperty however, unless you're swapping out settings
        // en masse e.g. because you have Profiles with their own individual Settings.
        // ==================================================================================
        public SettingsClass Settings { get; } = new SettingsClass(); // The "one and only" settings object.

        [ObservableProperty]
        Brush _pingServerIndicatorColor = Brushes.Gray;

        public MainPageViewModel()
        {
            Settings.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(Settings.ServerPath): 
                        _ = PingServer(); 
                        break;
                }
            };
        }

        private async Task PingServer()
        {
            var url = 
                Settings
                .ServerPath
                .Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase);
            if(url.Replace("www.", string.Empty).Count(_ => _ == '.') >= 1 &&
                Path.GetExtension(url).Length >= 3)
            {
                PingServerIndicatorColor = Brushes.Yellow;
                using (Ping ping = new Ping())
                {
                    try
                    {
                        PingServerIndicatorColor = (
                            await ping.SendPingAsync(
                                hostNameOrAddress: url, 
                                timeout: 5000,
                                buffer: Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"),
                                options: new PingOptions { DontFragment = true }
                             ))
                            .Status == IPStatus.Success ?
                                Brushes.LightGreen : Brushes.Red;
                    }
                    catch (PingException pex)
                    {
                        // Handle Ping specific exceptions
                        PingServerIndicatorColor = Brushes.Red; // Connection failed
                    }
                    catch (Exception ex)
                    {  
                        PingServerIndicatorColor = Brushes.Red;
                    }
                }
            }
            else PingServerIndicatorColor = Brushes.Salmon;
        }
    }
    partial class SettingsClass : ObservableObject
    {
        [ObservableProperty]
        private string _serverPath = String.Empty;
    }
    class UpdatePathFromServerPath : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string serverPath && !string.IsNullOrWhiteSpace(serverPath))
            {
                return $"{serverPath}/Updates/OneClick/package.htm";
            }
            else
            {
                return "Waiting for server path...";
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 
            throw new NotImplementedException();
    }
    class EmptyToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string @string)
            {
                return string.IsNullOrEmpty(@string) ? Brushes.Green: Brushes.Red;
            }
            else return Colors.Black;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}