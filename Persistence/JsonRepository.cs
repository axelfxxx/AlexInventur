using System.Text.Json;
using InventurApp.Models;

namespace InventurApp.Persistence
{
    public class JsonRepository
    {
        private static readonly string DatenOrdner = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AlexInventur");

        private static readonly string Datei = Path.Combine(DatenOrdner, "artikel.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public List<Artikel> Laden()
        {
            if (!File.Exists(Datei))
                return new List<Artikel>();

            try
            {
                var json = File.ReadAllText(Datei);

                if (string.IsNullOrWhiteSpace(json))
                    return new List<Artikel>();

                return JsonSerializer.Deserialize<List<Artikel>>(json) ?? new List<Artikel>();
            }
            catch
            {
                SichereDefekteDatei();
                return new List<Artikel>();
            }
        }

        public void Speichern(List<Artikel> artikel)
        {
            Directory.CreateDirectory(DatenOrdner);

            var json = JsonSerializer.Serialize(artikel, JsonOptions);
            var tempDatei = Datei + ".tmp";

            File.WriteAllText(tempDatei, json);

            if (File.Exists(Datei))
                File.Replace(tempDatei, Datei, null);
            else
                File.Move(tempDatei, Datei);
        }

        private static void SichereDefekteDatei()
        {
            try
            {
                if (!File.Exists(Datei))
                    return;

                var backupDatei = Path.Combine(
                    DatenOrdner,
                    $"artikel.defekt_{DateTime.Now:yyyyMMdd_HHmmss}.json");

                File.Copy(Datei, backupDatei, overwrite: false);
            }
            catch
            {
                // Absichtlich ignorieren: Eine defekte Datei darf den Programmstart nicht verhindern.
            }
        }
    }
}
