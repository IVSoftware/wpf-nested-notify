When you say this, it's a true statement:

> `ViewModel` does not get notified about these [changes in Settings...] 

When you say this, it's a _partially_ true statement:

> I expected the [ObservableProperty] attribute to handle this automatically.

True in the sense that if there are bindings like `{Binding Server.ServerPath}` in the xaml, and you swap to a new instance of `Settings`, the bindings in the UI _will_ now respond to changes in the new instance. So it _is_ automatic in that respect.
___

That said, it sounds as though you want **internal logic in the view model** to respond directly to `PropertyChanged` of the `Settings`, like in the sample below where we `Ping()` in response to a new value for `ServerPath`. Nothing about that is going to be automatic. Your code is showing exactly the right approach, unhooking the old instance's `PropertyChanged` event and setting a hook to the new instance, calling an internal handler so that the business logic in the VM will respond. 

In many cases, though, the wiring for "complex interactions" can be done in xaml using `IValueConverter` as is the case for the generated update path and text color in this UI.

I've reread your post about a hundred times now to see why _the UI doesn't update correctly in some cases_. A slightly modified approach to the VM is shown here, but there doesn't seem to be anything fundamentally wrong with how you're looking at this.
___

##### VM

```
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
```
___

##### Settings Class

In this sample, we'll react to changes of `ServerPath` to trigger UI updates of `UpdatePath` and text color. What this demonstrates is that the property change notifications of the nested class are definitely available and functional as binding targets in the xaml scheme.

```
partial class SettingsClass : ObservableObject
{
    [ObservableProperty]
    private string _serverPath = String.Empty;
}
```
___

#### Converted property values

There is a textbox on the UI that generates an `UpdatePath` based on changes to `ServerPath`.

##### UpdatePath

This `IValueConverter` sample class derives `UpdatePath` from changes to `ServerPath`.

```
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
```

##### Text Color

This `IValueConverter` sample class derives `Forecolor` from changes to `ServerPath`.

```
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
```

[![ui update example][1]][1]

___

#### Xaml

```
<Window x:Class="wpf_nested_notify.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:wpf_nested_notify"
        mc:Ignorable="d"
        Title="MainWindow" Width="500" Height="350"
        FontSize="18">
    <Window.DataContext>
        <local:MainPageViewModel/>
    </Window.DataContext>
    <Window.Resources>
        <local:UpdatePathFromServerPath x:Key="UpdatePathFromServerPath"/>
        <local:EmptyToColor x:Key="EmptyToColor"/>
    </Window.Resources>
    <Grid 
        VerticalAlignment="Stretch">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*" />
            <ColumnDefinition Width="4*" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>
        <StackPanel
            Grid.Column="1"
            Orientation="Vertical"
            VerticalAlignment="Center">
            <TextBox 
                Text="{Binding Settings.ServerPath, UpdateSourceTrigger=PropertyChanged}"
                Margin="0,20"
                MinHeight="30"
                VerticalContentAlignment="Bottom"/>            
            <Grid Height="175" Margin="0,0,0,0">
                <Border
                    BorderBrush="Gray" 
                    BorderThickness="1" 
                    CornerRadius="5" 
                    Margin="0,20"
                    Background="White">
                    <StackPanel 
                        Margin="10,10" >
                        <Label 
                            Content="{Binding Settings.ServerPath, UpdateSourceTrigger=PropertyChanged}"
                            Margin="0,10" 
                            Background="Azure"
                            MinHeight="25"/>
                        <TextBox 
                            Text="{Binding Settings.ServerPath, Converter={StaticResource UpdatePathFromServerPath} }" 
                            Margin="0,10" MinHeight="25" 
                            IsReadOnly="True"
                            Background="Azure"
                            FontSize="12"
                            Foreground="{Binding Settings.ServerPath, Converter={StaticResource EmptyToColor}  }"
                            VerticalContentAlignment="Center">
                        </TextBox>
                    </StackPanel>
                </Border>
                <Label
                    Margin="5,2,0,0"
                    Background="White"
                    Padding="5"
                    Content="Loopback"
                    VerticalAlignment="Top"
                    HorizontalAlignment="Left"/>
            </Grid>
            <Button 
                Height="30"
                Padding="5"
                Width="100"
                FontSize="11"
                Content="New Settings"
                Command="{Binding NewSettingsTestCommand}"
                Background="LightGreen" />
        </StackPanel>
    </Grid>
</Window>
```


  [1]: https://i.sstatic.net/M6LbzAwp.png