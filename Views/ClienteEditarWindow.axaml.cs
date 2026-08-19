using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ControlLicencias.Helpers;
using ControlLicencias.Models;
using ControlLicencias.Services;

namespace ControlLicencias.Views;

public partial class ClienteEditarWindow : Window
{
    public int ClienteId { get; set; }

    public ClienteEditarWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            UIHelper.FadeIn(this);
            await InicializarAsync();
        };
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private async Task InicializarAsync()
    {
        var titulo = this.FindControl<TextBlock>("TxtTitulo");
        if (titulo != null) titulo.Text = ClienteId == 0 ? "Nuevo cliente" : "Editar cliente";

        if (ClienteId == 0) return;

        try
        {
            var c = await ApiService.GetAsync<Cliente>($"/api/admin/clientes/{ClienteId}");
            if (c == null) return;

            Set("TxtNombre", c.Nombre);
            Set("TxtRuc", c.RUC);
            Set("TxtEmail", c.Email);
            Set("TxtTelefono", c.Telefono);
            Set("TxtDireccion", c.Direccion);
            SetCheck("ChkActivo", c.Activo);
        }
        catch (ApiException ex)
        {
            MostrarEstado(ex.Message);
        }
        catch (Exception)
        {
            MostrarEstado("No se pudo cargar el cliente.");
        }
    }

    private async void Guardar_Click(object? sender, RoutedEventArgs e)
    {
        var body = new
        {
            nombre = UIHelper.Capitalizar(Get("TxtNombre")),
            ruc = Get("TxtRuc"),
            email = Get("TxtEmail"),
            telefono = Get("TxtTelefono"),
            direccion = Get("TxtDireccion"),
            activo = GetCheck("ChkActivo")
        };

        try
        {
            if (ClienteId == 0)
                await ApiService.PostAsync<object>("/api/admin/clientes", body);
            else
                await ApiService.PutAsync<object>($"/api/admin/clientes/{ClienteId}", body);

            ToastService.Show(ClienteId == 0 ? "Cliente creado." : "Cliente guardado.", ToastType.Success);
            Close();
        }
        catch (ApiException ex)
        {
            ToastService.Show(ex.Message, ToastType.Error);
            MostrarEstado(ex.Message);
        }
        catch (Exception)
        {
            ToastService.Show("No se pudo guardar.", ToastType.Error);
            MostrarEstado("No se pudo guardar.");
        }
    }

    private void Cancelar_Click(object? sender, RoutedEventArgs e) => Close();

    private void MostrarEstado(string msg)
    {
        var lbl = this.FindControl<TextBlock>("LblEstado");
        if (lbl != null) lbl.Text = msg;
    }

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
