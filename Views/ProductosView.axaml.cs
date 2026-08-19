using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ControlLicencias.Helpers;
using ControlLicencias.Models;
using ControlLicencias.Services;

namespace ControlLicencias.Views;

public partial class ProductosView : UserControl
{
    public ProductosView()
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
        var grid = this.FindControl<DataGrid>("LstProductos");
        var estado = this.FindControl<TextBlock>("LblEstado");
        var vacio = this.FindControl<StackPanel>("PanelVacio");
        var loading = this.FindControl<StackPanel>("PanelLoading");

        if (loading != null) loading.IsVisible = true;

        try
        {
            var items = await ApiService.GetAsync<List<Producto>>("/api/admin/productos") ?? new List<Producto>();
            if (grid != null) grid.ItemsSource = items;
            if (vacio != null) vacio.IsVisible = items.Count == 0;
            if (estado != null) estado.Text = $"{items.Count} producto(s)";
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

    private async void Nuevo_Click(object? sender, RoutedEventArgs e) => await AbrirEditorAsync(null);

    private async void EditarItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Producto p)
            await AbrirEditorAsync(p);
    }

    private async void EliminarItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Producto p)
        {
            var dlg = new ConfirmDialog("Eliminar producto",
                $"¿Deseas eliminar el producto \"{p.Nombre}\"?\nNo se podrá si tiene licencias asociadas.",
                "Eliminar");
            var owner = TopLevel.GetTopLevel(this) as Window;
            if (owner != null) await dlg.ShowDialog(owner);
            if (!dlg.Resultado) return;

            try
            {
                await ApiService.DeleteAsync<object>($"/api/admin/productos/{p.Id}");
                ToastService.Show("Producto eliminado.", ToastType.Success);
                await CargarAsync();
            }
            catch (ApiException ex)
            {
                ToastService.Show(ex.Message, ToastType.Error);
            }
            catch (Exception)
            {
                ToastService.Show("No se pudo eliminar.", ToastType.Error);
            }
        }
    }

    private async Task AbrirEditorAsync(Producto? producto)
    {
        var w = new ProductoEditarWindow { ProductoActual = producto };
        var owner = TopLevel.GetTopLevel(this) as Window;
        if (owner != null)
            await w.ShowDialog(owner);
        else
            w.Show();

        await CargarAsync();
    }
}
