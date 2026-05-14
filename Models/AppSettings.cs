namespace InventurApp.Models
{
    public class AppSettings
    {
        public bool DarkMode { get; set; }
        public bool ScannerEnabled { get; set; } = true;
        public bool MultiWindowEnabled { get; set; } = true;
        public DateTime? LastBackupAt { get; set; }
        public string TwainSourceName { get; set; } = string.Empty;
        public bool TwainEnabled { get; set; } = true;

        // Komfort-Anmeldung: nur für Einzelplatz-/Werkstattbetrieb gedacht.
        // Es wird kein Passwort gespeichert; der konfigurierte aktive Benutzer wird beim Start gesetzt.
        public bool AutoLoginEnabled { get; set; } = false;
        public string AutoLoginUsername { get; set; } = string.Empty;

        // Update-/Release-Informationen. Der Manifest-Pfad kann später auf eine HTTPS-URL
        // oder auf eine lokale Datei zeigen, z. B. \\Server\Releases\update.json.
        public bool AutoUpdateCheckEnabled { get; set; } = true;
        public string UpdateManifestUrl { get; set; } = string.Empty;
        public DateTime? LastUpdateCheckAt { get; set; }

        // Frei änderbare Anzeigenamen für Artikel-Felder. Die internen Schlüssel bleiben stabil,
        // damit Datenbank, Services und Importe weiterhin zuverlässig funktionieren.
        public Dictionary<string, string> FieldDisplayNames { get; set; } = new()
        {
            ["Artikelnummer"] = "Artikelnummer",
            ["Bezeichnung"] = "Bezeichnung",
            ["Lagerort"] = "Lagerort",
            ["SollMenge"] = "Soll-Menge"
        };

        // Zusätzliche CSV-Importnamen pro internem Feld, getrennt durch Semikolon in der UI.
        public Dictionary<string, List<string>> FieldImportAliases { get; set; } = new()
        {
            ["Artikelnummer"] = new List<string> { "Artikelnummer", "Artikelnr", "Artikel-Nr", "SKU", "Nummer", "Gerät", "Geraet", "Hostname" },
            ["Bezeichnung"] = new List<string> { "Bezeichnung", "Name", "Artikelname", "Beschreibung", "System Modell", "Modell", "Device Model" },
            ["Lagerort"] = new List<string> { "Lagerort", "Lager", "Standort", "Ort", "Raum", "Location" },
            ["SollMenge"] = new List<string> { "SollMenge", "Soll-Menge", "Menge", "Bestand", "Anzahl", "Quantity", "Qty" }
        };

        // Frei definierbare Importfelder. Diese werden beim CSV-Import als Zusatzfelder
        // am Artikel gespeichert, ohne das feste Artikelschema zu erzwingen.
        public List<string> CustomImportFields { get; set; } = new();

        public string GetFieldName(string key)
        {
            EnsureFieldDefaults();
            return FieldDisplayNames.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : key;
        }

        public void EnsureFieldDefaults()
        {
            FieldDisplayNames ??= new Dictionary<string, string>();
            FieldImportAliases ??= new Dictionary<string, List<string>>();
            CustomImportFields ??= new List<string>();

            EnsureDisplayName("Artikelnummer", "Artikelnummer");
            EnsureDisplayName("Bezeichnung", "Bezeichnung");
            EnsureDisplayName("Lagerort", "Lagerort");
            EnsureDisplayName("SollMenge", "Soll-Menge");

            EnsureAliases("Artikelnummer", new[] { "Artikelnummer", "Artikelnr", "Artikel-Nr", "SKU", "Nummer", "Gerät", "Geraet", "Hostname" });
            EnsureAliases("Bezeichnung", new[] { "Bezeichnung", "Name", "Artikelname", "Beschreibung", "System Modell", "Modell", "Device Model" });
            EnsureAliases("Lagerort", new[] { "Lagerort", "Lager", "Standort", "Ort", "Raum", "Location" });
            EnsureAliases("SollMenge", new[] { "SollMenge", "Soll-Menge", "Menge", "Bestand", "Anzahl", "Quantity", "Qty" });

            CustomImportFields = CustomImportFields
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Select(f => f.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(f => f)
                .ToList();
        }

        private void EnsureDisplayName(string key, string fallback)
        {
            if (!FieldDisplayNames.ContainsKey(key) || string.IsNullOrWhiteSpace(FieldDisplayNames[key]))
                FieldDisplayNames[key] = fallback;
        }

        private void EnsureAliases(string key, IEnumerable<string> aliases)
        {
            if (!FieldImportAliases.ContainsKey(key) || FieldImportAliases[key] == null || FieldImportAliases[key].Count == 0)
                FieldImportAliases[key] = aliases.ToList();
        }
    }
}
