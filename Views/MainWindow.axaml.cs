using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ControlLicencias.Helpers;

namespace ControlLicencias.Views;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Button> _navButtons = new();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            var nombre = Sesion.Nombre;
            if (string.IsNullOrWhiteSpace(nombre)) nombre = Sesion.UserName;

            var lblNombre = this.FindControl<TextBlock>("LblNombreUsuario");
            if (lblNombre != null) lblNombre.Text = string.IsNullOrWhiteSpace(Sesion.Nombre) ? Sesion.UserName : Sesion.Nombre;

            var lblUser = this.FindControl<TextBlock>("LblUsuario");
            if (lblUser != null) lblUser.Text = "@" + Sesion.UserName;

            var lblInicial = this.FindControl<TextBlock>("LblInicial");
            if (lblInicial != null) lblInicial.Text = (string.IsNullOrEmpty(nombre) ? "?" : nombre[..1]).ToUpper();

            _navButtons["clientes"] = this.FindControl<Button>("BtnNavClientes")!;
            _navButtons["productos"] = this.FindControl<Button>("BtnNavProductos")!;
            _navButtons["config"] = this.FindControl<Button>("BtnNavConfig")!;
            _navButtons["usuarios"] = this.FindControl<Button>("BtnNavUsuarios")!;

            var overlay = this.FindControl<Panel>("Overlay");
            if (overlay != null)
            {
                var toastHost = new ToastHost();
                ToastService.Register(toastHost);
                overlay.Children.Add(toastHost);
            }

            ActualizarThemeBtn();
            MostrarView("clientes");
        };
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void Theme_Click(object? sender, RoutedEventArgs e)
    {
        ThemeManager.Toggle();
        ActualizarThemeBtn();
    }

    private void ActualizarThemeBtn()
    {
        var dark = ThemeManager.IsDark;
        var iconMoon = this.FindControl<Control>("IconMoon");
        var iconSun = this.FindControl<Control>("IconSun");
        var lbl = this.FindControl<TextBlock>("LblTheme");

        if (iconMoon != null) iconMoon.IsVisible = !dark;
        if (iconSun != null) iconSun.IsVisible = dark;
        if (lbl != null) lbl.Text = dark ? "Tema claro" : "Tema oscuro";
    }

    private void Nav_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
            MostrarView(tag);
    }

    private void MostrarView(string tag)
    {
        foreach (var kv in _navButtons)
        {
            if (kv.Key == tag)
            {
                if (!kv.Value.Classes.Contains("NavActive")) kv.Value.Classes.Add("NavActive");
            }
            else
            {
                kv.Value.Classes.Remove("NavActive");
            }
        }

        var host = this.FindControl<ContentControl>("Contenido");
        if (host == null) return;

        host.Content = tag switch
        {
            "productos" => new ProductosView(),
            "config" => new ConfigView(),
            "usuarios" => new UsuariosAdminView(),
            _ => new ClientesView()
        };
    }

    private void Salir_Click(object? sender, RoutedEventArgs e) => Close();
}
