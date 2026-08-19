using ControlLicencias.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace ControlLicencias.Services
{
    public static class UpdateService
    {
        private const string GitHubApiUrl = "https://api.github.com/repos/Ferlopz/ControlLicencias/releases/latest";
        private static readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };

        private static void Log(string msg)
        {
            try
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ControlLicencias");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var ruta = Path.Combine(dir, "update_log.txt");
                File.AppendAllText(ruta, $"[{DateTime.Now:HH:mm:ss}] {msg}{Environment.NewLine}");
            }
            catch { }
        }

        public static string ObtenerGitHubToken()
        {
            string[] rutas = {
                Path.Combine(AppContext.BaseDirectory, "configuracion.txt"),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "configuracion.txt")
            };
            foreach (var ruta in rutas)
            {
                if (File.Exists(ruta))
                {
                    try
                    {
                        foreach (var linea in File.ReadAllLines(ruta))
                        {
                            var t = linea.Trim();
                            if (t.StartsWith("GITHUB_TOKEN="))
                                return t.Substring(13).Trim();
                        }
                    }
                    catch { }
                    break;
                }
            }
            return "";
        }

        public static string VersionActual
        {
            get
            {
                var attr = Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                var version = attr?.InformationalVersion ?? "1.0";
                var idx = version.IndexOf('+');
                if (idx > 0) version = version.Substring(0, idx);
                return version;
            }
        }

        public static async Task<string?> HayActualizacionAsync()
        {
            try
            {
                var token = ObtenerGitHubToken();
                if (string.IsNullOrEmpty(token))
                {
                    Log("Sin GITHUB_TOKEN configurado. Update check omitido.");
                    return null;
                }

                Log($"Iniciando check. Version local: {VersionActual}");
                _http.DefaultRequestHeaders.Clear();
                _http.DefaultRequestHeaders.Add("User-Agent", "ControlLicencias-Updater");
                _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                var json = await _http.GetStringAsync(GitHubApiUrl);
                var release = JsonSerializer.Deserialize<GitHubRelease>(json);
                if (release == null || string.IsNullOrEmpty(release.TagName))
                {
                    Log("Release nula o sin tag name.");
                    return null;
                }

                string tag = release.TagName.TrimStart('v');
                Log($"GitHub latest: tag={release.TagName}, limpio={tag}");
                if (Version.TryParse(tag, out var remote) && Version.TryParse(VersionActual, out var local))
                {
                    Log($"Comparando local={local} vs remote={remote} -> {remote > local}");
                    if (remote > local) return release.TagName;
                    Log("No hay version nueva.");
                }
                else
                {
                    Log($"No se pudo parsear. Tag={tag}, Local={VersionActual}");
                }
            }
            catch (HttpRequestException ex)
            {
                Log($"ERROR HTTP: {ex.StatusCode} - {ex.Message}");
            }
            catch (Exception ex)
            {
                Log($"ERROR: {ex.GetType().Name} - {ex.Message}");
            }
            return null;
        }

        public static async Task<(bool exito, string? error)> DescargarEInstalarAsync(string tag)
        {
            try
            {
                var token = ObtenerGitHubToken();
                Log($"Descargando update: {tag}");
                _http.DefaultRequestHeaders.Clear();
                _http.DefaultRequestHeaders.Add("User-Agent", "ControlLicencias-Updater");
                if (!string.IsNullOrEmpty(token))
                    _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                var json = await _http.GetStringAsync(GitHubApiUrl);
                var release = JsonSerializer.Deserialize<GitHubRelease>(json);
                var zipAsset = release?.Assets.FirstOrDefault(a => a.Name.EndsWith(".zip"));
                if (zipAsset == null) { Log("No se encontro .zip en assets."); return (false, "No se encontró el archivo .zip en la release de GitHub."); }

                string downloadUrl = zipAsset.ApiUrl;
                Log($"Descargando desde API: {downloadUrl}");
                string tempZip = Path.Combine(Path.GetTempPath(), $"ControlLicencias-{tag}.zip");
                string tempExtract = Path.Combine(Path.GetTempPath(), $"ControlLicencias-{tag}");

                using var httpDescarga = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
                httpDescarga.DefaultRequestHeaders.Clear();
                httpDescarga.DefaultRequestHeaders.Add("User-Agent", "ControlLicencias-Updater");
                if (!string.IsNullOrEmpty(token))
                    httpDescarga.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                httpDescarga.DefaultRequestHeaders.Add("Accept", "application/octet-stream");
                var zipBytes = await httpDescarga.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(tempZip, zipBytes);

                if (Directory.Exists(tempExtract))
                    Directory.Delete(tempExtract, true);
                ZipFile.ExtractToDirectory(tempZip, tempExtract);
                Log("Zip descargado y extraido.");

                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string scriptPath = Path.Combine(tempExtract, "update.bat");

                await File.WriteAllLinesAsync(scriptPath, new[]
                {
                    "@echo off",
                    ">nul 2>&1 net session || (powershell -Command \"Start-Process '%~f0' -Verb RunAs\" & exit /b)",
                    "taskkill /F /IM ControlLicencias.exe >nul 2>&1",
                    $"xcopy \"{tempExtract}\\*\" \"{appDir}\" /E /Y /I >nul",
                    $"cd /d \"{appDir}\"",
                    $"start \"\" \"ControlLicencias.exe\"",
                    "timeout /t 3 /nobreak >nul",
                    $"rmdir /s /q \"{tempExtract}\"",
                    $"del \"{tempZip}\"",
                    "del \"%~f0\""
                });

                Log($"Ejecutando script: {scriptPath}");
                Process.Start(new ProcessStartInfo
                {
                    FileName = scriptPath,
                    UseShellExecute = true,
                    CreateNoWindow = true
                });

                return (true, null);
            }
            catch (Exception ex)
            {
                Log($"ERROR descarga: {ex.GetType().Name} - {ex.Message}");
                return (false, $"{ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
