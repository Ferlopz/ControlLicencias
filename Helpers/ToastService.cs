namespace ControlLicencias.Helpers;

public enum ToastType
{
    Info,
    Success,
    Error
}

public static class ToastService
{
    private static Views.ToastHost? _host;

    public static void Register(Views.ToastHost host) => _host = host;

    public static void Show(string message, ToastType type = ToastType.Info)
        => _host?.Show(message, type);
}
