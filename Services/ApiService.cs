using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ControlLicencias.Helpers;

namespace ControlLicencias.Services;

public class ApiException : Exception
{
    public ApiException(string message) : base(message) { }
}

public static class ApiService
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(30) };

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static void SetToken(string token)
    {
        Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static string Url(string path) => Config.ApiUrl + path;

    private static async Task<T?> SendAsync<T>(HttpMethod method, string path, object? body)
    {
        using var req = new HttpRequestMessage(method, Url(path));
        if (body != null)
            req.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");

        var resp = await Http.SendAsync(req);
        var json = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
        {
            var error = "Error al comunicarse con el servidor.";
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("error", out var e) && e.ValueKind == JsonValueKind.String)
                    error = e.GetString() ?? error;
            }
            catch { }
            throw new ApiException(error);
        }

        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    public static Task<T?> GetAsync<T>(string path) => SendAsync<T>(HttpMethod.Get, path, null);
    public static Task<T?> PostAsync<T>(string path, object body) => SendAsync<T>(HttpMethod.Post, path, body);
    public static Task<T?> PutAsync<T>(string path, object body) => SendAsync<T>(HttpMethod.Put, path, body);
    public static Task<T?> DeleteAsync<T>(string path) => SendAsync<T>(HttpMethod.Delete, path, null);
}
