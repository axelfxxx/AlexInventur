using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace InventurApp.Services
{
    public class TwainSourceInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public override string ToString() => Name;
    }

    public class TwainScanResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? FilePath { get; set; }

        public static TwainScanResult Failed(string message) => new() { Success = false, Message = message };
        public static TwainScanResult Ok(string message, string? filePath = null) => new() { Success = true, Message = message, FilePath = filePath };
    }

    /// <summary>
    /// Scanner-Service für Windows.
    ///
    /// Priorität: WIA-Geräte werden direkt verwendet, weil Windows dafür eine COM-Schnittstelle mitbringt.
    /// TWAIN-Quellen werden weiterhin erkannt und auswählbar gehalten, aber ohne externe TWAIN-Bibliothek
    /// kann nur WIA wirklich direkt scannen.
    /// </summary>
    public class TwainService
    {
        private const int ScannerDeviceType = 1;
        private const string WiaFormatJpeg = "{B96B3CAE-0728-11D3-9D7B-0000F81EF32E}";

        public TwainScanResult Scan(string sourceName)
        {
            try
            {
                AppPaths.EnsureAll();

                if (!OperatingSystem.IsWindows())
                    return TwainScanResult.Failed("Scannen ist nur unter Windows verfügbar.");

                var selectedSource = (sourceName ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(selectedSource) || selectedSource.Equals("Standard-TWAIN-Quelle", StringComparison.OrdinalIgnoreCase) || selectedSource.Equals("Standard-WIA-Dialog", StringComparison.OrdinalIgnoreCase))
                    return ScanWithWiaDialog();

                if (selectedSource.StartsWith("TWAIN:", StringComparison.OrdinalIgnoreCase))
                    return TwainScanResult.Failed("Diese Quelle ist als TWAIN-Quelle erkannt. Für echte TWAIN-Scans wird eine zusätzliche TWAIN-Bibliothek benötigt. Wähle nach Möglichkeit eine WIA-Quelle in den Einstellungen.");

                return ScanWithWiaSource(selectedSource);
            }
            catch (COMException ex)
            {
                AppLogger.Error(ex, "WIA-/Scanner-COM-Fehler.");
                return TwainScanResult.Failed("Der Scanner konnte nicht angesprochen werden. Prüfe, ob das Gerät eingeschaltet und der Treiber installiert ist. Details: " + ex.Message);
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Scan konnte nicht erstellt werden.");
                return TwainScanResult.Failed("Scan konnte nicht erstellt werden: " + ex.Message);
            }
        }

        public List<TwainSourceInfo> GetAvailableSources()
        {
            var result = new List<TwainSourceInfo>();

            AddWiaSources(result);
            AddTwainFromFolder(result, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "twain_32"));
            AddTwainFromFolder(result, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "twain_64"));
            AddTwainFromRegistry(result, Registry.CurrentUser, @"SOFTWARE\\TWAIN\\Data Sources");
            AddTwainFromRegistry(result, Registry.LocalMachine, @"SOFTWARE\\TWAIN\\Data Sources");
            AddTwainFromRegistry(result, Registry.LocalMachine, @"SOFTWARE\\WOW6432Node\\TWAIN\\Data Sources");

            if (result.Count == 0)
            {
                result.Add(new TwainSourceInfo
                {
                    Name = "Standard-WIA-Dialog",
                    Source = "WIA:Dialog"
                });
            }

            return result
                .GroupBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(s => s.Name)
                .ToList();
        }

        private static TwainScanResult ScanWithWiaDialog()
        {
            var dialogType = Type.GetTypeFromProgID("WIA.CommonDialog");
            if (dialogType == null)
                return TwainScanResult.Failed("WIA ist auf diesem System nicht verfügbar oder nicht registriert.");

            dynamic dialog = Activator.CreateInstance(dialogType)!;
            dynamic image = dialog.ShowAcquireImage(ScannerDeviceType, 1, 0, WiaFormatJpeg, true, true, false);
            if (image == null)
                return TwainScanResult.Failed("Der Scan wurde abgebrochen oder es wurde kein Bild geliefert.");

            var filePath = CreateScanFilePath("jpg");
            SaveWiaImage(image, filePath);
            return TwainScanResult.Ok("Scan erfolgreich erstellt.", filePath);
        }

        private static TwainScanResult ScanWithWiaSource(string sourceName)
        {
            var managerType = Type.GetTypeFromProgID("WIA.DeviceManager");
            if (managerType == null)
                return TwainScanResult.Failed("WIA ist auf diesem System nicht verfügbar oder nicht registriert.");

            dynamic manager = Activator.CreateInstance(managerType)!;
            dynamic? selectedInfo = null;

            foreach (dynamic info in manager.DeviceInfos)
            {
                if ((int)info.Type != ScannerDeviceType)
                    continue;

                var name = GetWiaProperty(info.Properties, "Name");
                var deviceId = GetWiaProperty(info.Properties, "DeviceID");
                var source = $"WIA: {name}";

                if (NormalizeSource(sourceName).Equals(NormalizeSource(source), StringComparison.OrdinalIgnoreCase) ||
                    sourceName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                    sourceName.Equals(deviceId, StringComparison.OrdinalIgnoreCase))
                {
                    selectedInfo = info;
                    break;
                }
            }

            if (selectedInfo == null)
                return ScanWithWiaDialog();

            dynamic device = selectedInfo.Connect();
            if (device.Items.Count < 1)
                return TwainScanResult.Failed("Der ausgewählte Scanner hat keine scanbare Quelle gemeldet.");

            dynamic item = device.Items[1];
            dynamic image = item.Transfer(WiaFormatJpeg);
            if (image == null)
                return TwainScanResult.Failed("Der Scanner hat kein Bild geliefert.");

            var filePath = CreateScanFilePath("jpg");
            SaveWiaImage(image, filePath);
            return TwainScanResult.Ok("Scan erfolgreich über WIA erstellt.", filePath);
        }

        private static string NormalizeSource(string value)
        {
            return (value ?? string.Empty).Replace("WIA:", "WIA: ", StringComparison.OrdinalIgnoreCase).Replace("TWAIN:", "TWAIN: ", StringComparison.OrdinalIgnoreCase).Trim();
        }

        private static string CreateScanFilePath(string extension)
        {
            var fileName = $"Scan_{DateTime.Now:yyyyMMdd_HHmmss}.{extension.TrimStart('.')}";
            return Path.Combine(AppPaths.ScanDirectory, fileName);
        }

        private static void SaveWiaImage(dynamic image, string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            image.SaveFile(filePath);
        }

        private static void AddWiaSources(List<TwainSourceInfo> result)
        {
            try
            {
                if (!OperatingSystem.IsWindows()) return;

                var managerType = Type.GetTypeFromProgID("WIA.DeviceManager");
                if (managerType == null) return;

                dynamic manager = Activator.CreateInstance(managerType)!;
                foreach (dynamic info in manager.DeviceInfos)
                {
                    if ((int)info.Type != ScannerDeviceType)
                        continue;

                    var name = GetWiaProperty(info.Properties, "Name");
                    var deviceId = GetWiaProperty(info.Properties, "DeviceID");
                    if (string.IsNullOrWhiteSpace(name))
                        name = string.IsNullOrWhiteSpace(deviceId) ? "WIA-Scanner" : deviceId;

                    result.Add(new TwainSourceInfo
                    {
                        Name = $"WIA: {name}",
                        Source = string.IsNullOrWhiteSpace(deviceId) ? "WIA" : deviceId
                    });
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "WIA-Scannerquellen konnten nicht vollständig ermittelt werden.");
            }
        }

        private static string GetWiaProperty(dynamic properties, string name)
        {
            try
            {
                foreach (dynamic property in properties)
                {
                    if (string.Equals((string)property.Name, name, StringComparison.OrdinalIgnoreCase))
                        return Convert.ToString(property.Value) ?? string.Empty;
                }
            }
            catch
            {
                // Einzelne Treiber liefern nicht alle WIA-Eigenschaften zuverlässig.
            }

            return string.Empty;
        }

        private static void AddTwainFromFolder(List<TwainSourceInfo> result, string folder)
        {
            try
            {
                if (!Directory.Exists(folder)) return;

                foreach (var sourceFolder in Directory.GetDirectories(folder))
                {
                    var name = Path.GetFileName(sourceFolder);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        result.Add(new TwainSourceInfo
                        {
                            Name = $"TWAIN: {name}",
                            Source = sourceFolder
                        });
                    }
                }
            }
            catch
            {
                // Scanner-Erkennung darf die Einstellungen nie blockieren.
            }
        }

        private static void AddTwainFromRegistry(List<TwainSourceInfo> result, RegistryKey root, string path)
        {
            try
            {
                using var key = root.OpenSubKey(path);
                if (key == null) return;

                foreach (var name in key.GetSubKeyNames())
                {
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        result.Add(new TwainSourceInfo
                        {
                            Name = $"TWAIN: {name}",
                            Source = $"Registry: {root.Name}\\{path}"
                        });
                    }
                }
            }
            catch
            {
                // Zugriff kann je nach Windows-/Rechtekontext fehlschlagen.
            }
        }
    }
}
