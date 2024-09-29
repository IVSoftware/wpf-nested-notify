Looking at your code, the intention seems to be having a `Settings` property that is an `ObservableObject` nested inside your `MainPageViewModel` which is also an `ObservableObject`. In xaml, the way you would bind to this is by referencing the nested object, e.g. `Button <FontAttributes="{Binding Settings.FontSetting}">`.


#### Main Page VM

``` 
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
```

___

#### Settings Class

```
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
```

___

#### Xaml

Here, the `Text` of the button is a property of `MainPageViewModel`, but the _style_ of the text on the button comes from `MainPageViewModel.Settings.FontSetting`.

```
<Window x:Class="wpf_nested_notify.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:wpf_nested_notify"
        mc:Ignorable="d"
        Title="MainWindow" Height="450" Width="800">
    <Window.DataContext>
        <local:MainPageViewModel/>
    </Window.DataContext>

    <Grid 
        VerticalAlignment="Stretch">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="2*" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>
        <StackPanel
            Grid.Column="1"
            Orientation="Vertical"
            VerticalAlignment="Center">

            <!-- Button -->
            <Button
                Content="{Binding Text}" 
                FontStyle="{Binding Settings.FontSetting}" 
                ToolTip="Counts the number of times you click"
                Command="{Binding ButtonClickedCommand}"
                HorizontalAlignment="Stretch"
                Padding="5"
                FontSize="14"/>

            <!-- Frame with Grid and CheckBox -->
            <Grid Height="100" Margin="0,20,0,0">
                <Border BorderBrush="Gray" BorderThickness="1" CornerRadius="5" Margin="0,20,0,0" Background="White">
                    <Grid 
                        Margin="5,15" >
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="25" />
                            <ColumnDefinition Width="Auto" />
                        </Grid.ColumnDefinitions>
                        <CheckBox Grid.Column="0" IsChecked="{Binding Settings.UseItalic}"  VerticalAlignment="Center"/>
                        <Label Grid.Column="1" Padding="5,0" Content="Use Italics" VerticalAlignment="Center" VerticalContentAlignment="Center"/>
                    </Grid>
                </Border>

                <!-- Label for Settings -->
                <Label
                    Margin="10,5,0,0"
                    Background="White"
                    Padding="5"
                    Content="Settings"
                    Height="30"
                    VerticalAlignment="Top"
                    HorizontalAlignment="Left"/>
            </Grid>
        </StackPanel>
    </Grid>
</Window>
```

[![button styled using a settings property][2]][2]

___

##### Binding (in general)

Finally, based on your comment:

> I don’t have a deep understanding of how everything works yet

In essence, all that's really being said when something is an observable object is that it implements `INotifyPropertyChanged`, and it's trivial to do this directly. Perhaps it will demystify things to see how one might implement an observable `Text` property without the toolkit.

```
class ObservableObjectFromScratch : INotifyPropertyChanged
{
    public string Text
    {
        get => _text;
        set
        {
            if (!Equals(_text, value))
            {
                _text = value;
                OnPropertyChanged();
            }
        }
    }
    string _text = string.Empty;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public event PropertyChangedEventHandler? PropertyChanged;
}
```

  [2]: https://i.sstatic.net/EnbidmZP.png