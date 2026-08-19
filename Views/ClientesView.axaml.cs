using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ControlLicencias.Helpers;
using ControlLicencias.Models;
using ControlLicencias.Services;

namespace ControlLicencias.Views;

public partial class ClientesView : UserControl
{
    private readonly DispatcherTimer _searchTimer = new() { Interval = TimeSpan.FromMilliseconds(300) };

    public ClientesView()
    {
        InitializeComponent();
        _searchTimer.Tick += (_, _) =>
        {
            _searchTimer.Stop();
            _ = CargarAsync();
        };
        Loaded += async (_, _) =>
        {
            UIHelper.FadeIn(this);
            await CargarAsync();
        };
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private async Task CargarAsync()
    {
        var grid = this.FindControl<DataGrid>("LstClientes");
        var estado = this.FindControl<TextBlock>("LblEstado");
        var vacio = this.FindControl<StackPanel>("PanelVacio");
        var loading = this.FindControl<StackPanel>("PanelLoading");
        var busqueda = this.FindControl<TextBox>("TxtBusqueda")?.Text?.Trim() ?? "";

        if (loading != null) loading.IsVisible = true;

        try
        {
            var query = string.IsNullOrEmpty(busqueda) ? "" : "?busqueda=" + Uri.EscapeDataString(busqueda);
            var items = await ApiService.GetAsync<List<Cliente>>("/api/admin/clientes" + query) ?? new List<Cliente>();
            if (grid != null) grid.ItemsSource = items;
            if (vacio != null) vacio.IsVisible = items.Count == 0;
            if (estado != null) estado.Text = $"{items.Count} cliente(s)";
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

    private void TxtBusqueda_TextChanged(object? sender, TextChangedEventArgs e)
    {
        _searchTimer.Stop();
        _searchTimer.Start();
    }

    private async void Nuevo_Click(object? sender, RoutedEventArgs e) => await AbrirEditorAsync(0);

    private async void EditarItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Cliente c)
            await AbrirEditorAsync(c.Id);
    }

    private async void VerLicencias_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Cliente c)
        {
            var w = new LicenciasClienteWindow { ClienteId = c.Id, ClienteNombre = c.Nombre };
            var owner = TopLevel.GetTopLevel(this) as Window;
            if (owner != null) await w.ShowDialog(owner); else w.Show();
            await CargarAsync();
        }
    }

    private async void Lista_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (this.FindControl<DataGrid>("LstClientes")?.SelectedItem is Cliente c)
            await AbrirEditorAsync(c.Id);
    }

    private async Task AbrirEditorAsync(int clienteId)
    {
        var w = new ClienteEditarWindow { ClienteId = clienteId };
        var owner = TopLevel.GetTopLevel(this) as Window;
        if (owner != null)
            await w.ShowDialog(owner);
        else
            w.Show();

        await CargarAsync();
    }
}
