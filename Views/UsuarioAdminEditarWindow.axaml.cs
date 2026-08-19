using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ControlLicencias.Helpers;
using ControlLicencias.Models;
using ControlLicencias.Services;

namespace ControlLicencias.Views;

public partial class UsuarioAdminEditarWindow : Window
{
    public int UsuarioId { get; set; }

    public UsuarioAdminEditarWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await InicializarAsync();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private async Task InicializarAsync()
    {
        try
        {
            var users = await ApiService.GetAsync<List<UsuarioAdmin>>("/api/admin/usuarios-admin");
            var u = users?.FirstOrDefault(x => x.Id == UsuarioId);
            if (u == null) return;

            var lbl = this.FindControl<TextBlock>("LblUserName");
            if (lbl != null) lbl.Text = u.UserName;

            var nombre = this.FindControl<TextBox>("TxtNombre");
            if (nombre != null) nombre.Text = u.Nombre;

            var chk = this.FindControl<CheckBox>("ChkActivo");
            if (chk != null) chk.IsChecked = u.Activo;
        }
        catch (Exception) { }
    }

    private async void Guardar_Click(object? sender, RoutedEventArgs e)
    {
        var lbl = this.FindControl<TextBlock>("LblUserName");
        var nombre = UIHelper.Capitalizar(this.FindControl<TextBox>("TxtNombre")?.Text);
        var pass = this.FindControl<TextBox>("TxtPassword")?.Text ?? "";
        var activo = this.FindControl<CheckBox>("ChkActivo")?.IsChecked == true;
        var error = this.FindControl<TextBlock>("LblError");

        var userName = lbl?.Text ?? "";

        try
        {
            await ApiService.PutAsync<object>($"/api/admin/usuarios-admin/{UsuarioId}", new { userName, nombre, activo, password = pass });
            Close();
        }
        catch (ApiException ex)
        {
            if (error != null) { error.Text = ex.Message; error.IsVisible = true; }
        }
        catch (Exception)
        {
            if (error != null) { error.Text = "No se pudo guardar."; error.IsVisible = true; }
        }
    }

    private void Cancelar_Click(object? sender, RoutedEventArgs e) => Close();
}
