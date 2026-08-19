using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ControlLicencias.Helpers;
using ControlLicencias.Models;
using ControlLicencias.Services;

namespace ControlLicencias.Views;

public partial class LicenciasClienteWindow : Window
{
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = "";

    public LicenciasClienteWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            var titulo = this.FindControl<TextBlock>("LblTitulo");
            if (titulo != null) titulo.Text = $"Licencias de {ClienteNombre}";
            await CargarAsync();
        };
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private async Task CargarAsync()
    {
        var grid = this.FindControl<DataGrid>("LstLicencias");
        var estado = this.FindControl<TextBlock>("LblEstado");
        var vacio = this.FindControl<StackPanel>("PanelVacio");
        var loading = this.FindControl<StackPanel>("PanelLoading");

        if (loading != null) loading.IsVisible = true;

        try
        {
            var items = await ApiService.GetAsync<List<Licencia>>($"/api/admin/clientes/{ClienteId}/licencias") ?? new List<Licencia>();
            if (grid != null) grid.ItemsSource = items;
            if (vacio != null) vacio.IsVisible = items.Count == 0;
            if (estado != null) estado.Text = $"{items.Count} licencia(s)";
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

    private async void Nueva_Click(object? sender, RoutedEventArgs e)
    {
        var w = new LicenciaNuevaWindow { ClienteId = ClienteId, ClienteNombre = ClienteNombre };
        var owner = TopLevel.GetTopLevel(this) as Window;
        if (owner != null) await w.ShowDialog(owner); else w.Show();
        await CargarAsync();
    }

    private async void EditarItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Licencia l)
        {
            var w = new LicenciaEditarWindow { LicenciaId = l.Id };
            var owner = TopLevel.GetTopLevel(this) as Window;
            if (owner != null) await w.ShowDialog(owner); else w.Show();
            await CargarAsync();
        }
    }

    private async void ToggleItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Licencia l)
        {
            if (l.Activa)
            {
                var dlg = new ConfirmDialog("Desactivar licencia",
                    $"¿Deseas desactivar la licencia de {l.ProductoNombre}?",
                    "Desactivar");
                var owner = TopLevel.GetTopLevel(this) as Window;
                if (owner != null) await dlg.ShowDialog(owner);
                if (!dlg.Resultado) return;
            }

            try
            {
                await ApiService.PutAsync<object>($"/api/admin/licencias/{l.Id}/activar", new { activa = !l.Activa });
                ToastService.Show(l.Activa ? "Licencia desactivada." : "Licencia activada.", ToastType.Success);
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

    private void Cancelar_Click(object? sender, RoutedEventArgs e) => Close();
}
