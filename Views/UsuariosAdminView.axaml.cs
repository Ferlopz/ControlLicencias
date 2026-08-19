using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ControlLicencias.Helpers;
using ControlLicencias.Models;
using ControlLicencias.Services;

namespace ControlLicencias.Views;

public partial class UsuariosAdminView : UserControl
{
    public UsuariosAdminView()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            UIHelper.FadeIn(this);
            await CargarAsync();
        };
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private async Task CargarAsync()
    {
        var grid = this.FindControl<DataGrid>("LstUsuarios");
        var estado = this.FindControl<TextBlock>("LblEstado");
        var vacio = this.FindControl<StackPanel>("PanelVacio");
        var loading = this.FindControl<StackPanel>("PanelLoading");

        if (loading != null) loading.IsVisible = true;

        try
        {
            var items = await ApiService.GetAsync<List<UsuarioAdmin>>("/api/admin/usuarios-admin") ?? new List<UsuarioAdmin>();
            if (grid != null) grid.ItemsSource = items;
            if (vacio != null) vacio.IsVisible = items.Count == 0;
            if (estado != null) estado.Text = $"{items.Count} usuario(s)";
        }
        catch (ApiException ex)
        {
            ToastService.Show(ex.Message, ToastType.Error);
            if (estado != null) estado.Text = ex.Message;
        }
        catch (Exception)
        {
            ToastService.Show("No se pudo conectar con el servidor.", ToastType.Error);
            if (estado != null) estado.Text = "No se pudo conectar con el servidor.";
        }
        finally
        {
            if (loading != null) loading.IsVisible = false;
        }
    }

    private async void Nuevo_Click(object? sender, RoutedEventArgs e)
    {
        var w = new UsuarioAdminNuevoWindow();
        var owner = TopLevel.GetTopLevel(this) as Window;
        if (owner != null) await w.ShowDialog(owner); else w.Show();
        await CargarAsync();
    }

    private async void EditarItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is UsuarioAdmin u)
        {
            var w = new UsuarioAdminEditarWindow { UsuarioId = u.Id };
            var owner = TopLevel.GetTopLevel(this) as Window;
            if (owner != null) await w.ShowDialog(owner); else w.Show();
            await CargarAsync();
        }
    }

    private async void ToggleItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is UsuarioAdmin u)
        {
            if (u.Activo)
            {
                var dlg = new ConfirmDialog("Desactivar usuario",
                    $"¿Deseas desactivar el acceso de {u.UserName}? No podrá iniciar sesión.",
                    "Desactivar");
                var owner = TopLevel.GetTopLevel(this) as Window;
                if (owner != null) await dlg.ShowDialog(owner);
                if (!dlg.Resultado) return;
            }

            try
            {
                await ApiService.PutAsync<object>($"/api/admin/usuarios-admin/{u.Id}", new { userName = u.UserName, nombre = u.Nombre, activo = !u.Activo, password = "" });
                ToastService.Show(u.Activo ? $"Acceso de {u.UserName} desactivado." : $"Acceso de {u.UserName} activado.", ToastType.Success);
                await CargarAsync();
            }
            catch (ApiException ex)
            {
                ToastService.Show(ex.Message, ToastType.Error);
            }
            catch (Exception)
            {
                ToastService.Show("No se pudo conectar con el servidor.", ToastType.Error);
            }
        }
    }
}
