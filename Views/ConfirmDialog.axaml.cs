using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace ControlLicencias.Views;

public partial class ConfirmDialog : Window
{
    public bool Resultado { get; private set; }

    public ConfirmDialog()
    {
        InitializeComponent();
    }

    public ConfirmDialog(string titulo, string mensaje, string botonTexto, bool esPeligro = true) : this()
    {
        var t = this.FindControl<TextBlock>("TxtTitulo");
        if (t != null) t.Text = titulo;

        var m = this.FindControl<TextBlock>("TxtMensaje");
        if (m != null) m.Text = mensaje;

        var b = this.FindControl<Button>("BtnAccion");
        if (b != null)
        {
            b.Content = botonTexto;
            if (!esPeligro)
            {
                b.Classes.Remove("DangerBtn");
                b.Classes.Add("PrimaryBtn");
            }
        }
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void Accion_Click(object? sender, RoutedEventArgs e)
    {
        Resultado = true;
        Close();
    }

    private void Cancelar_Click(object? sender, RoutedEventArgs e)
    {
        Resultado = false;
        Close();
    }
}
