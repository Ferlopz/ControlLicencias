namespace ControlLicencias.Helpers;

public static class Config
{
    private static string? _apiUrl;

    public static string ApiUrl
    {
        get
        {
            if (_apiUrl == null)
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "conexion_api.txt");
                if (!File.Exists(path))
                    throw new Exception("No se encontró el archivo conexion_api.txt");

                var contenido = File.ReadAllText(path).Trim();
                if (contenido.StartsWith("API_URL=", StringComparison.OrdinalIgnoreCase))
                    contenido = contenido[8..].Trim();

                _apiUrl = contenido.TrimEnd('/');
            }
            return _apiUrl;
        }
    }
}
