using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ControlLicencias.Helpers;
using ControlLicencias.Models;
using ControlLicencias.Services;

namespace ControlLicencias.Views;

public partial class LicenciaNuevaWindow : Window
{
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = "";

    private List<Producto> _productos = new();

    public LicenciaNuevaWindow()
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
        var lblCliente = this.FindControl<TextBlock>("LblCliente");
        if (lblCliente != null) lblCliente.Text = ClienteNombre;

        var lst = this.FindControl<ListBox>("LstProductos");

        try
        {
            _productos = await ApiService.GetAsync<List<Producto>>("/api/admin/productos") ?? new List<Producto>();
            if (lst != null)
            {
                lst.ItemsSource = _productos.Select(p => p.Nombre).ToList();
                if (_productos.Count > 0)
                    lst.SelectedIndex = 0;
            }
        }
        catch (Exception)
        {
            var error = this.FindControl<TextBlock>("LblError");
            if (error != null) { error.Text = "No se pudo cargar los productos."; error.IsVisible = true; }
        }
    }

    private void ToggleProductos_Click(object? sender, RoutedEventArgs e)
    {
        var lst = this.FindControl<ListBox>("LstProductos");
        if (lst != null) lst.IsVisible = !lst.IsVisible;
    }

    private void LstProductos_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var lst = this.FindControl<ListBox>("LstProductos");
        var lbl = this.FindControl<TextBlock>("LblProductoSeleccionado");

        var idx = lst?.SelectedIndex ?? -1;
        if (idx >= 0 && idx < _productos.Count && lbl != null)
        {
            lbl.Text = _productos[idx].Nombre;
            if (lst != null) lst.IsVisible = false;
        }
    }

    private async void Crear_Click(object? sender, RoutedEventArgs e)
    {
        var lst = this.FindControl<ListBox>("LstProductos");
        var error = this.FindControl<TextBlock>("LblError");

        var idx = lst?.SelectedIndex ?? -1;
        var producto = (idx >= 0 && idx < _productos.Count) ? _productos[idx] : null;

        if (producto == null)
        {
            if (error != null) { error.Text = "Selecciona un producto."; error.IsVisible = true; }
            return;
        }

        try
        {
            await ApiService.PostAsync<object>("/api/admin/licencias", new
            {
                clienteId = ClienteId,
                productoId = producto.Id,
                codigoActivacion = "",
                cantidadPCs = 3,
                cantidadUsuarios = 3,
                fechaExpiracion = (DateTime?)null,
                activa = true
            });

            ToastService.Show("Licencia creada.", ToastType.Success);
            Close();
        }
        catch (ApiException ex)
        {
            if (error != null) { error.Text = ex.Message; error.IsVisible = true; }
        }
        catch (Exception)
        {
            if (error != null) { error.Text = "No se pudo crear la licencia."; error.IsVisible = true; }
        }
    }

    private void Cancelar_Click(object? sender, RoutedEventArgs e) => Close();
}
