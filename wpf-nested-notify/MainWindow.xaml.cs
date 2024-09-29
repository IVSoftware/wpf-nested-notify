using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
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
    class MainPageViewModel : ObservableObject
    {
        // This 'is' an ObservableObject because it provides INotifyPropertyChanged.
        // It 'is not' an ObservableProperty however, unless you're swapping out settings
        // en masse e.g. because you have Profiles with their own individual Settings.
        // ==================================================================================
        public SettingsClass Settings { get; } = new SettingsClass(); // The "one and only" settings object.
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