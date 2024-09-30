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
using static System.Net.WebRequestMethods;

namespace wpf_nested_notify
{
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();
    }
    partial class MainPageViewModel : ObservableObject
    {
        [ObservableProperty]
        private SettingsClass _settings;

        [ObservableProperty]
        Brush _pingServerIndicatorColor = Brushes.Gray;

        public MainPageViewModel()
        {
            Settings = new SettingsClass();
            NewSettingsTestCommand = new RelayCommand(() =>
            {
                Settings = new SettingsClass();
            });
        }

        // Respond to a new instance of Settings e.g. user profile changed.
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(Settings):
                    Settings.PropertyChanged -= OnSettingsPropertyChanged;
                    Settings.PropertyChanged += OnSettingsPropertyChanged;
                    break;
            }
        }
        private void OnSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.ServerPath):
                    _ = PingServer();
                    break;
            }
        }
        private async Task PingServer()
        {
            var url = 
                Settings
                .ServerPath
                .Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase);
            if( new[] { ".com", ".net", "org" }.Contains(
                Path.GetExtension(url.Replace("www.", string.Empty))))
            {
                PingServerIndicatorColor = Brushes.Yellow;
                using (Ping ping = new Ping())
                {
                    try
                    {
                        PingServerIndicatorColor = (
                            await ping.SendPingAsync(url))
                            .Status == IPStatus.Success ?
                                Brushes.LightGreen : Brushes.Red;
                    }
                    catch
                    {  
                        PingServerIndicatorColor = Brushes.Red;
                    }
                }
            }
            else PingServerIndicatorColor = Brushes.Salmon;
        }
        public ICommand NewSettingsTestCommand { get; }
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