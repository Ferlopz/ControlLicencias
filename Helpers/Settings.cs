using System.Text.Json;

namespace ControlLicencias.Helpers;

public static class Settings
{
    private static readonly string Dir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ControlLicencias");

    private static readonly string FilePath = Path.Combine(Dir, "settings.json");

    private static string _theme = "dark";

    public static string Theme
    {
        get => _theme;
        set
        {
            _theme = value;
            Save();
        }
    }

    public static void Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            using var doc = JsonDocument.Parse(File.ReadAllText(FilePath));
            if (doc.RootElement.TryGetProperty("theme", out var t) && t.ValueKind == JsonValueKind.String)
                _theme = t.GetString() ?? "dark";
        }
        catch
        {
            _theme = "dark";
        }
    }

    private static void Save()
    {
        try
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(new { theme = _theme }));
        }
        catch
        {
        }
    }
}
