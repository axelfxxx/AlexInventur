using InventurApp.Models;
using System.Text.Json;

namespace InventurApp.Services
{
    public class SettingsService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public AppSettings Load()
        {
            AppPaths.EnsureAll();

            if (!File.Exists(AppPaths.SettingsFile))
            {
                var defaults = new AppSettings();
                defaults.EnsureFieldDefaults();
                return defaults;
            }

            try
            {
                var json = File.ReadAllText(AppPaths.SettingsFile);
                var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                settings.EnsureFieldDefaults();
                return settings;
            }
            catch
            {
                TryBackupBrokenSettingsFile();
                var defaults = new AppSettings();
                defaults.EnsureFieldDefaults();
                return defaults;
            }
        }

        public void Save(AppSettings settings)
        {
            AppPaths.EnsureAll();
            settings.EnsureFieldDefaults();
            var tmpFile = AppPaths.SettingsFile + ".tmp";
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(tmpFile, json);

            if (File.Exists(AppPaths.SettingsFile))
                File.Replace(tmpFile, AppPaths.SettingsFile, AppPaths.SettingsFile + ".bak");
            else
                File.Move(tmpFile, AppPaths.SettingsFile);
        }

        private static void TryBackupBrokenSettingsFile()
        {
            try
            {
                if (!File.Exists(AppPaths.SettingsFile)) return;
                var brokenFile = Path.Combine(
                    AppPaths.DataDirectory,
                    $"settings_defekt_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                File.Copy(AppPaths.SettingsFile, brokenFile, overwrite: false);
            }
            catch
            {
                // Einstellungen sind Komfort. Fehler beim Sichern dürfen die App nicht abbrechen.
            }
        }
    }
}
