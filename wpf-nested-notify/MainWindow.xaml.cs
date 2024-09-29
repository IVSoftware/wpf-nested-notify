using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace wpf_nested_notify
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();
    }
    partial class MainPageViewModel : ObservableObject
    {
        public MainPageViewModel()
        {
            ButtonClickedCommand = new RelayCommand(() =>
            {
                _count++;
                string plural = _count > 1 ? "s" : string.Empty;
                Text = $"You clicked {_count} time{plural}";
            });
        }
        int _count = 0;
        public ICommand ButtonClickedCommand { get; }

        [ObservableProperty]
        string _text = "Click Me";

        // This 'is' an ObservableObject because it provides INotifyPropertyChanged.
        // It 'is not' an ObservableProperty however, unless you're swapping out settings
        // en masse e.g. because you have Profiles with their own individual Settings.
        // ==================================================================================
        public SettingsClass Settings { get; } = new SettingsClass(); // The "one and only" settings object.
    }
    partial class SettingsClass : ObservableObject
    {
        [ObservableProperty]
        bool _useItalic;

        [ObservableProperty]
        FontStyle _fontSetting;

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(UseItalic):
                    FontSetting = UseItalic ? FontStyles.Italic : FontStyles.Normal;
                    break;
            }
        }
    }
}