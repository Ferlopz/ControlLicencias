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
    public LoginWindow()
    {
        InitializeComponent();
        Opened += (_, _) => this.FindControl<TextBox>("TxtUsuario")?.Focus();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

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
