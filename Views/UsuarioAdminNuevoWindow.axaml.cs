using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ControlLicencias.Helpers;
using ControlLicencias.Services;

namespace ControlLicencias.Views;

public partial class UsuarioAdminNuevoWindow : Window
{
    public UsuarioAdminNuevoWindow()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private async void Crear_Click(object? sender, RoutedEventArgs e)
    {
        var user = this.FindControl<TextBox>("TxtUserName")?.Text?.Trim() ?? "";
        var nombre = UIHelper.Capitalizar(this.FindControl<TextBox>("TxtNombre")?.Text);
        var pass = this.FindControl<TextBox>("TxtPassword")?.Text ?? "";
        var error = this.FindControl<TextBlock>("LblError");

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            if (error != null) { error.Text = "Usuario y contraseña son obligatorios."; error.IsVisible = true; }
            return;
        }

        try
        {
            await ApiService.PostAsync<object>("/api/admin/usuarios-admin", new { userName = user, nombre, password = pass });
            Close();
        }
        catch (ApiException ex)
        {
            if (error != null) { error.Text = ex.Message; error.IsVisible = true; }
        }
        catch (Exception)
        {
            if (error != null) { error.Text = "No se pudo crear."; error.IsVisible = true; }
        }
    }

    private void Cancelar_Click(object? sender, RoutedEventArgs e) => Close();
}
