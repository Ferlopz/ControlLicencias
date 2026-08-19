using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ControlLicencias.Helpers;

namespace ControlLicencias.Views;

public partial class ToastHost : UserControl
{
    private StackPanel _root = null!;

    public ToastHost()
    {
        InitializeComponent();
        _root = this.FindControl<StackPanel>("Root")!;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    public void Show(string message, ToastType type = ToastType.Info)
    {
        var toast = BuildToast(message, type);
        _root.Children.Add(toast);
        AnimateIn(toast);
        _ = DismissAfter(toast, 3800);
    }

    private static Border BuildToast(string message, ToastType type)
    {
        var (accentKey, softKey, iconKey) = type switch
        {
            ToastType.Success => ("Success", "SuccessSoft", "IconCheck"),
            ToastType.Error => ("Danger", "DangerSoft", "IconError"),
            _ => ("PrimaryColor", "InfoSoft", "IconInfo")
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        var accent = new Border
        {
            Width = 4,
            CornerRadius = new CornerRadius(2),
            Background = Res(accentKey),
            Margin = new Thickness(0, 0, 12, 0),
            VerticalAlignment = VerticalAlignment.Stretch
        };
        Grid.SetColumn(accent, 0);

        var icon = new Border
        {
            Width = 28,
            Height = 28,
            CornerRadius = new CornerRadius(8),
            Background = Res(softKey),
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Child = new Avalonia.Controls.Shapes.Path
            {
                Data = ResGeometry(iconKey),
                Fill = Res(accentKey),
                Width = 16,
                Height = 16,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        Grid.SetColumn(icon, 1);

        var text = new TextBlock
        {
            Text = message,
            Foreground = Res("MainText"),
            TextWrapping = TextWrapping.Wrap,
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(text, 2);

        return new Border
        {
            Background = Res("Surface"),
            BorderBrush = Res("Border"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            BoxShadow = BoxShadows.Parse("0 8 24 0 #33000000"),
            MaxWidth = 380,
            Padding = new Thickness(14, 12),
            Child = grid
        };
    }

    private static IBrush? Res(string key)
    {
        if (Application.Current != null && Application.Current.TryFindResource(key, out var value))
            return value as IBrush;
        return null;
    }

    private static Geometry? ResGeometry(string key)
    {
        if (Application.Current != null && Application.Current.TryFindResource(key, out var value))
            return value as Geometry;
        return null;
    }

    private async void AnimateIn(Control toast)
    {
        var tt = new TranslateTransform(0, 24);
        toast.RenderTransform = tt;
        toast.Opacity = 0;
        for (var i = 1; i <= 8; i++)
        {
            tt.Y = 24 * (1 - i / 8.0);
            toast.Opacity = i / 8.0;
            await Task.Delay(16);
        }
        tt.Y = 0;
        toast.Opacity = 1;
    }

    private async Task DismissAfter(Control toast, int ms)
    {
        await Task.Delay(ms);
        for (var i = 1; i <= 6; i++)
        {
            toast.Opacity = 1 - i / 6.0;
            await Task.Delay(16);
        }
        _root.Children.Remove(toast);
    }
}
