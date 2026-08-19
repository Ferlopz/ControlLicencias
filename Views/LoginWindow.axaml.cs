using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ControlLicencias.Helpers;
using ControlLicencias.Models;
using ControlLicencias.Services;

namespace ControlLicencias.Views;

public partial class LoginWindow : Window
{
    private bool _verificacionCorrida;
    private string? _tagActualizacion;

    public LoginWindow()
    {
        InitializeComponent();
        Opened += (_, _) => this.FindControl<TextBox>("TxtUsuario")?.Focus();
        _ = VerificarActualizacionAsync();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private async Task VerificarActualizacionAsync()
    {
        if (_verificacionCorrida) return;
        _verificacionCorrida = true;
        try
        {
            var tag = await UpdateService.HayActualizacionAsync();
            if (string.IsNullOrEmpty(tag)) return;

            _tagActualizacion = tag;
            var panel = this.FindControl<Border>("PanelActualizacion");
            var txt = this.FindControl<TextBlock>("TxtNuevaVersion");
            if (panel != null) panel.IsVisible = true;
            if (txt != null) txt.Text = $"Nueva versión disponible ({tag})";
        }
        catch { }
    }

    public async void OnActualizar_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_tagActualizacion)) return;

        var btn = this.FindControl<Button>("BtnActualizar");
        if (btn != null) { btn.IsEnabled = false; btn.Content = "Descargando..."; }

        var (exito, error) = await UpdateService.DescargarEInstalarAsync(_tagActualizacion);
        if (!exito)
        {
            var err = this.FindControl<TextBlock>("LblError");
            if (err != null) { err.Text = $"No se pudo descargar la actualización. {error}"; err.IsVisible = true; }
            if (btn != null) { btn.IsEnabled = true; btn.Content = "Actualizar"; }
        }
    }

    public void OnClose_Click(object? sender, RoutedEventArgs e) => Close();

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            Login_Click(this, new RoutedEventArgs());
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private async void Login_Click(object? sender, RoutedEventArgs e)
    {
        var userBox = this.FindControl<TextBox>("TxtUsuario");
        var passBox = this.FindControl<TextBox>("TxtPassword");
        var error = this.FindControl<TextBlock>("LblError");
        var btn = this.FindControl<Button>("BtnLogin");
        var loading = this.FindControl<ProgressBar>("LoadingBar");

        if (userBox == null || passBox == null || error == null || btn == null || loading == null) return;

        var user = userBox.Text?.Trim() ?? "";
        var pass = passBox.Text ?? "";

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            MostrarError(error, userBox, "Ingrese usuario y contraseña.");
            return;
        }

        error.IsVisible = false;
        btn.IsEnabled = false;
        loading.IsVisible = true;

        try
        {
            var resp = await ApiService.PostAsync<LoginResponse>("/api/auth/login", new { userName = user, password = pass });
            if (resp == null || string.IsNullOrEmpty(resp.Token))
                throw new ApiException("Respuesta inválida del servidor.");

            ApiService.SetToken(resp.Token);
            Sesion.Token = resp.Token;
            Sesion.UserName = resp.UserName;
            Sesion.Nombre = resp.Nombre;

            new MainWindow().Show();
            Close();
        }
        catch (ApiException ex)
        {
            MostrarError(error, passBox, ex.Message);
        }
        catch (Exception)
        {
            MostrarError(error, passBox, "No se pudo conectar con el servidor.");
        }
        finally
        {
            btn.IsEnabled = true;
            loading.IsVisible = false;
        }
    }

    private void MostrarError(TextBlock error, Control c, string msg)
    {
        error.Text = msg;
        UIHelper.AnimarError(c, error);
        c.Focus();
    }
}
