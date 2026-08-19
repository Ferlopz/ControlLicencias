using Avalonia;
using Avalonia.Styling;

namespace ControlLicencias.Helpers;

public static class ThemeManager
{
    public const string LightKey = "light";
    public const string DarkKey = "dark";

    public static bool IsDark => Application.Current?.RequestedThemeVariant != ThemeVariant.Light;

    public static void Initialize()
    {
        Apply(Settings.Theme == LightKey ? ThemeVariant.Light : ThemeVariant.Dark);
    }

    public static void Toggle() => Apply(IsDark ? ThemeVariant.Light : ThemeVariant.Dark);

    private static void Apply(ThemeVariant variant)
    {
        if (Application.Current != null)
            Application.Current.RequestedThemeVariant = variant;

        Settings.Theme = variant == ThemeVariant.Light ? LightKey : DarkKey;
    }
}
