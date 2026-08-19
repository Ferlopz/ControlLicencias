using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ControlLicencias.Helpers;
using ControlLicencias.Models;
using ControlLicencias.Services;

namespace ControlLicencias.Views;

public partial class LicenciaEditarWindow : Window
{
    public int LicenciaId { get; set; }

    public LicenciaEditarWindow()
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
        try
        {
            var l = await ApiService.GetAsync<Licencia>($"/api/admin/licencias/{LicenciaId}");
            if (l == null) return;

            var subtitulo = this.FindControl<TextBlock>("LblSubtitulo");
            if (subtitulo != null) subtitulo.Text = $"{l.ClienteNombre} — {l.ProductoNombre}";

            SetText("LblCliente", l.ClienteNombre);
            SetText("LblProducto", l.ProductoNombre);
            Set("TxtCodigo", l.CodigoActivacion);
            Set("TxtPCs", l.CantidadPCs.ToString());
            Set("TxtUsuarios", l.CantidadUsuarios.ToString());
            Set("TxtExpiracion", l.FechaExpiracion?.ToString("yyyy-MM-dd") ?? "");
            SetCheck("ChkActiva", l.Activa);

            await CargarDispositivosAsync();
            await CargarUsuariosAsync();
        }
        catch (ApiException ex)
        {
            MostrarEstado(ex.Message);
        }
        catch (Exception)
        {
            MostrarEstado("No se pudo cargar la licencia.");
        }
    }

    private async Task CargarDispositivosAsync()
    {
        var grid = this.FindControl<DataGrid>("LstDispositivos");
        if (grid == null) return;
        try
        {
            grid.ItemsSource = await ApiService.GetAsync<List<Dispositivo>>($"/api/admin/licencias/{LicenciaId}/dispositivos") ?? new List<Dispositivo>();
        }
        catch { }
    }

    private async Task CargarUsuariosAsync()
    {
        var grid = this.FindControl<DataGrid>("LstUsuarios");
        if (grid == null) return;
        try
        {
            grid.ItemsSource = await ApiService.GetAsync<List<UsuarioAutorizado>>($"/api/admin/licencias/{LicenciaId}/usuarios") ?? new List<UsuarioAutorizado>();
        }
        catch { }
    }

    private async void Guardar_Click(object? sender, RoutedEventArgs e)
    {
        DateTime? fecha = null;
        var fechaTxt = Get("TxtExpiracion");
        if (!string.IsNullOrWhiteSpace(fechaTxt) && DateTime.TryParse(fechaTxt, out var d))
            fecha = d;

        var body = new
        {
            clienteId = 0,
            productoId = 0,
            codigoActivacion = Get("TxtCodigo"),
            cantidadPCs = ParseInt("TxtPCs", 3),
            cantidadUsuarios = ParseInt("TxtUsuarios", 3),
            fechaExpiracion = fecha,
            activa = GetCheck("ChkActiva")
        };

        try
        {
            await ApiService.PutAsync<object>($"/api/admin/licencias/{LicenciaId}", body);
            ToastService.Show("Licencia guardada.", ToastType.Success);
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

    private async void ToggleDispositivoItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is Dispositivo d)
        {
            if (d.Activo)
            {
                var dlg = new ConfirmDialog("Desactivar dispositivo",
                    $"¿Deseas desactivar el dispositivo {d.NombrePC}?",
                    "Desactivar");
                var owner = TopLevel.GetTopLevel(this) as Window;
                if (owner != null) await dlg.ShowDialog(owner);
                if (!dlg.Resultado) return;
            }

            try
            {
                await ApiService.PutAsync<object>($"/api/admin/dispositivos/{d.Id}/activar", new { activo = !d.Activo });
                await CargarDispositivosAsync();
            }
            catch (ApiException ex) { MostrarEstado(ex.Message); }
        }
    }

    private async void ToggleUsuarioItem_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is UsuarioAutorizado u)
        {
            if (u.Activo)
            {
                var dlg = new ConfirmDialog("Desactivar usuario",
                    $"¿Deseas desactivar el acceso de {u.UserName}?",
                    "Desactivar");
                var owner = TopLevel.GetTopLevel(this) as Window;
                if (owner != null) await dlg.ShowDialog(owner);
                if (!dlg.Resultado) return;
            }

            try
            {
                await ApiService.PutAsync<object>($"/api/admin/usuarios/{u.Id}/activar", new { activo = !u.Activo });
                await CargarUsuariosAsync();
            }
            catch (ApiException ex) { MostrarEstado(ex.Message); }
        }
    }

    private async void AgregarUsuario_Click(object? sender, RoutedEventArgs e)
    {
        var txt = this.FindControl<TextBox>("TxtNuevoUsuario");
        var name = txt?.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(name)) return;

        try
        {
            await ApiService.PostAsync<object>($"/api/admin/licencias/{LicenciaId}/usuarios", new { userName = name });
            if (txt != null) txt.Text = "";
            await CargarUsuariosAsync();
        }
        catch (ApiException ex) { MostrarEstado(ex.Message); }
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

    private void SetText(string name, string? value)
    {
        var tb = this.FindControl<TextBlock>(name);
        if (tb != null) tb.Text = value ?? "";
    }

    private int ParseInt(string name, int def)
    {
        var tb = this.FindControl<TextBox>(name);
        return int.TryParse(tb?.Text, out var v) ? v : def;
    }

    private bool GetCheck(string name) => this.FindControl<CheckBox>(name)?.IsChecked == true;

    private void SetCheck(string name, bool value)
    {
        var cb = this.FindControl<CheckBox>(name);
        if (cb != null) cb.IsChecked = value;
    }
}
