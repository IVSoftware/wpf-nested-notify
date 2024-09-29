Your understanding of the nested binding seems to be correct! It _should_ work that way, and the minimal reproducible example below shows that it _does_ work that way.

There might be a nuanced distinction to be made between `ObservableProperty` and `ObservableObject`. The `Settings` property doesn't need to be observable unless the settings instance itself is going to change. If you have multiple users, and they have individual profile settings, _then_ you would need to notify but otherwise it just points to the "one and only" instance of `SettingsClass`.

The code below is tested and it works. The hope is that you might be able to spot a discrepancy between this implementation and your own.
___

##### VM

```
class MainPageViewModel : ObservableObject
{
    // Settings 'is' an ObservableObject because it provides INotifyPropertyChanged.
    // It 'is not' an ObservableProperty however, unless you're swapping out settings
    // en masse e.g. because you have Profiles with their own individual Settings.
    // ==================================================================================
    public SettingsClass Settings { get; } = new SettingsClass(); // The "one and only" settings object.
}
```

##### Settings Class

In this sample, we'll react to changes of `ServerPath` to trigger UI updates of `UpdatePath` and text color.

```
partial class SettingsClass : ObservableObject
{
    [ObservableProperty]
    private string _serverPath = String.Empty;
}
```

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
        Title="MainWindow" Width="500" Height="300"
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
        </StackPanel>
    </Grid>
</Window>
```

  [1]: https://i.sstatic.net/f5SAKu46.png