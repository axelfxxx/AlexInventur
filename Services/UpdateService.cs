using System.Diagnostics;
using System.Text.Json;
using InventurApp.Models;

namespace InventurApp.Services
{
    public sealed class UpdateInfo
    {
        public string LatestVersion { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
        public DateTime? PublishedAt { get; set; }
        public bool IsUpdateAvailable { get; set; }
        public string StatusMessage { get; set; } = string.Empty;
    }

    public class UpdateService
    {
        private readonly SettingsService _settingsService = new();

        public async Task<UpdateInfo> CheckForUpdatesAsync(AppSettings settings, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(settings.UpdateManifestUrl))
            {
                return new UpdateInfo
                {
                    StatusMessage = "Keine Update-Quelle konfiguriert. Du kannst später eine HTTPS-URL oder einen Netzwerkpfad zu update.json eintragen."
                };
            }

            try
            {
                var json = await ReadManifestAsync(settings.UpdateManifestUrl.Trim(), cancellationToken);
                var info = JsonSerializer.Deserialize<UpdateInfo>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new UpdateInfo();

                info.IsUpdateAvailable = IsNewer(info.LatestVersion, AppInfoService.CurrentVersion);
                info.StatusMessage = info.IsUpdateAvailable
                    ? $"Update {info.LatestVersion} ist verfügbar."
                    : $"Du verwendest die aktuelle Version {AppInfoService.CurrentVersionText}.";

                settings.LastUpdateCheckAt = DateTime.Now;
                _settingsService.Save(settings);
                return info;
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Update-Prüfung fehlgeschlagen");
                return new UpdateInfo
                {
                    StatusMessage = "Update-Prüfung fehlgeschlagen: " + ex.Message
                };
            }
        }

        private static async Task<string> ReadManifestAsync(string source, CancellationToken cancellationToken)
        {
            if (Uri.TryCreate(source, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                return await client.GetStringAsync(uri, cancellationToken);
            }

            var path = Environment.ExpandEnvironmentVariables(source);
            return await File.ReadAllTextAsync(path, cancellationToken);
        }

        private static bool IsNewer(string latestVersionText, Version currentVersion)
        {
            if (!Version.TryParse(latestVersionText, out var latest))
                return false;

            return latest > currentVersion;
        }

        public static void OpenDownload(UpdateInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.DownloadUrl))
                return;

            Process.Start(new ProcessStartInfo(info.DownloadUrl) { UseShellExecute = true });
        }
    }
}
