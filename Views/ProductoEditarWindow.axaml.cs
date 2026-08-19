using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ControlLicencias.Helpers;
using ControlLicencias.Models;
using ControlLicencias.Services;

namespace ControlLicencias.Views;

public partial class ProductoEditarWindow : Window
{
    public Producto? ProductoActual { get; set; }

    public ProductoEditarWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => Inicializar();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void Inicializar()
    {
        var titulo = this.FindControl<TextBlock>("TxtTitulo");
        if (titulo != null) titulo.Text = ProductoActual == null ? "Nuevo producto" : "Editar producto";

        if (ProductoActual == null) return;

        Set("TxtNombre", ProductoActual.Nombre);
        Set("TxtDescripcion", ProductoActual.Descripcion);
        SetCheck("ChkActivo", ProductoActual.Activo);
    }

    private async void Guardar_Click(object? sender, RoutedEventArgs e)
    {
        var nombre = UIHelper.Capitalizar(Get("TxtNombre"));
        var error = this.FindControl<TextBlock>("LblError");

        if (string.IsNullOrEmpty(nombre))
        {
            if (error != null) { error.Text = "El nombre es obligatorio."; error.IsVisible = true; }
            return;
        }

        var body = new
        {
            nombre,
            descripcion = Get("TxtDescripcion"),
            activo = GetCheck("ChkActivo")
        };

        try
        {
            if (ProductoActual == null)
                await ApiService.PostAsync<object>("/api/admin/productos", body);
            else
                await ApiService.PutAsync<object>($"/api/admin/productos/{ProductoActual.Id}", body);

            ToastService.Show(ProductoActual == null ? "Producto creado." : "Producto guardado.", ToastType.Success);
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

    private string Get(string name) => this.FindControl<TextBox>(name)?.Text ?? "";

    private void Set(string name, string? value)
    {
        var tb = this.FindControl<TextBox>(name);
        if (tb != null) tb.Text = value ?? "";
    }

    private bool GetCheck(string name) => this.FindControl<CheckBox>(name)?.IsChecked == true;

    private void SetCheck(string name, bool value)
    {
        var cb = this.FindControl<CheckBox>(name);
        if (cb != null) cb.IsChecked = value;
    }
}
