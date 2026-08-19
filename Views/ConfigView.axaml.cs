using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ControlLicencias.Helpers;
using ControlLicencias.Models;
using ControlLicencias.Services;

namespace ControlLicencias.Views;

public partial class ConfigView : UserControl
{
    public ConfigView()
    {
        InitializeComponent();
        Loaded += async (_, _) => { UIHelper.FadeIn(this); await CargarAsync(); };
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private async Task CargarAsync()
    {
        try
        {
            var c = await ApiService.GetAsync<ConfigTelegram>("/api/admin/config");
            if (c == null) return;
            var token = this.FindControl<TextBox>("TxtToken");
            var admin = this.FindControl<TextBox>("TxtAdminId");
            if (token != null) token.Text = c.Token;
            if (admin != null) admin.Text = c.AdminId.ToString();
        }
        catch (Exception) { }
    }

    private async void Guardar_Click(object? sender, RoutedEventArgs e)
    {
        var token = this.FindControl<TextBox>("TxtToken")?.Text ?? "";
        var admin = this.FindControl<TextBox>("TxtAdminId")?.Text ?? "";
        var estado = this.FindControl<TextBlock>("LblEstado");

        if (!long.TryParse(admin, out var adminId))
        {
            if (estado != null) estado.Text = "El ID del administrador debe ser un número.";
            return;
        }

        try
        {
            await ApiService.PutAsync<object>("/api/admin/config", new { token, adminId });
            if (estado != null) estado.Text = "Guardado correctamente.";
            ToastService.Show("Configuración guardada correctamente.", ToastType.Success);
        }
        catch (ApiException ex)
        {
            if (estado != null) estado.Text = ex.Message;
            ToastService.Show(ex.Message, ToastType.Error);
        }
        catch (Exception)
        {
            if (estado != null) estado.Text = "No se pudo guardar.";
            ToastService.Show("No se pudo guardar.", ToastType.Error);
        }
    }
}
