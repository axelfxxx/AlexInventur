using InventurApp.Forms;
using InventurApp.Services;

namespace AlexInventur
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, e) => HandleFatalException(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    HandleFatalException(ex);
            };

            try
            {
                AppLogger.Info("Alex Inventur startet.");
                var benutzerService = new BenutzerService();
                var settings = new SettingsService().Load();
                var loggedIn = settings.AutoLoginEnabled && benutzerService.AutoLogin(settings.AutoLoginUsername);

                if (!loggedIn)
                {
                    using var login = new LoginForm(benutzerService);
                    if (login.ShowDialog() != DialogResult.OK)
                        return;
                }

                new BackupService().AutoBackupIfNeeded();
                var mainForm = new ArtikelForm(benutzerService);
                mainForm.Shown += async (_, _) => await CheckForUpdatesOnStartAsync(settings);
                Application.Run(mainForm);
                benutzerService.Logout();
                AppLogger.Info("Alex Inventur wurde beendet.");
            }
            catch (Exception ex)
            {
                HandleFatalException(ex);
            }
        }

        private static async Task CheckForUpdatesOnStartAsync(InventurApp.Models.AppSettings settings)
        {
            if (!settings.AutoUpdateCheckEnabled || string.IsNullOrWhiteSpace(settings.UpdateManifestUrl))
                return;

            try
            {
                var info = await new UpdateService().CheckForUpdatesAsync(settings);
                if (info.IsUpdateAvailable)
                {
                    var result = MessageBox.Show(
                        $"Eine neue Version ist verfügbar: {info.LatestVersion}\n\nMöchtest du den Download öffnen?",
                        "Alex Inventur Update",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                        UpdateService.OpenDownload(info);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Automatische Update-Prüfung beim Start fehlgeschlagen");
            }
        }

        private static void HandleFatalException(Exception ex)
        {
            AppLogger.Error(ex, "Global abgefangener Fehler");
            MessageBox.Show(
                "Es ist ein unerwarteter Fehler aufgetreten. Details wurden im Logordner gespeichert.",
                "Alex Inventur",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
