using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ControlLicencias.Helpers;

public static class UIHelper
{
    public static async void AnimarError(Control c, TextBlock? labelError = null)
    {
        if (labelError != null) labelError.IsVisible = true;
        var orig = c.Margin;
        if (c is TextBox tb) { tb.BorderBrush = Brushes.Red; tb.BorderThickness = new Thickness(1.5); }

        for (int i = 0; i < 3; i++)
        {
            c.Margin = new Thickness(orig.Left + 5, orig.Top, orig.Right, orig.Bottom); await Task.Delay(40);
            c.Margin = new Thickness(orig.Left - 5, orig.Top, orig.Right, orig.Bottom); await Task.Delay(40);
        }

        c.Margin = orig;
        await Task.Delay(2000);

        if (c is TextBox tb2)
        {
            tb2.ClearValue(TextBox.BorderBrushProperty);
            tb2.ClearValue(TextBox.BorderThicknessProperty);
        }
    }

    public static async void FadeIn(Control c)
    {
        c.Opacity = 0;
        for (int i = 0; i <= 10; i++)
        {
            c.Opacity = i / 10.0;
            await Task.Delay(14);
        }
    }

    public static string Capitalizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return texto ?? "";

        var palabras = texto.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < palabras.Length; i++)
        {
            palabras[i] = char.ToUpper(palabras[i][0]) + palabras[i][1..].ToLower();
        }
        return string.Join(' ', palabras);
    }
}
