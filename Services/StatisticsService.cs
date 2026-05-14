using InventurApp.Models;
using InventurApp.Persistence;

namespace InventurApp.Services
{
    public class StatisticsService
    {
        private readonly SqliteRepository _repository = new();

        public StatisticsSnapshot CreateSnapshot(IEnumerable<Artikel> artikel)
        {
            var artikelListe = artikel.ToList();
            var dokumente = SafeLoadDocuments();
            var benutzer = SafeLoadUsers();

            var snapshot = new StatisticsSnapshot
            {
                ArtikelGesamt = artikelListe.Count,
                LagerorteGesamt = artikelListe
                    .Select(a => a.Lagerort)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count(),
                GesamtMenge = artikelListe.Sum(a => Math.Max(0, a.SollMenge)),
                NiedrigerBestand = artikelListe.Count(a => a.SollMenge <= 5),
                DokumenteGesamt = dokumente.Count,
                ScansGesamt = dokumente.Count(d => string.Equals(d.Kategorie, "Scan", StringComparison.OrdinalIgnoreCase) || d.Quelle.Contains("TWAIN", StringComparison.OrdinalIgnoreCase)),
                AktiveBenutzer = benutzer.Count(u => u.IsActive),
                InaktiveBenutzer = benutzer.Count(u => !u.IsActive),
                LogEintraegeHeute = CountLogEntriesToday()
            };

            snapshot.Lagerorte = artikelListe
                .Where(a => !string.IsNullOrWhiteSpace(a.Lagerort))
                .GroupBy(a => a.Lagerort.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => new StatisticItem { Name = g.Key, Wert = g.Sum(a => Math.Max(0, a.SollMenge)), Zusatz = $"{g.Count()} Artikel" })
                .OrderByDescending(x => x.Wert)
                .Take(8)
                .ToList();

            snapshot.TopArtikel = artikelListe
                .OrderByDescending(a => a.SollMenge)
                .ThenBy(a => a.Artikelnummer)
                .Take(8)
                .Select(a => new StatisticItem { Name = string.IsNullOrWhiteSpace(a.Bezeichnung) ? a.Artikelnummer : a.Bezeichnung, Wert = Math.Max(0, a.SollMenge), Zusatz = a.Artikelnummer })
                .ToList();

            snapshot.DokumentKategorien = dokumente
                .GroupBy(d => string.IsNullOrWhiteSpace(d.Kategorie) ? "Allgemein" : d.Kategorie.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => new StatisticItem { Name = g.Key, Wert = g.Count(), Zusatz = "Dokumente" })
                .OrderByDescending(x => x.Wert)
                .Take(8)
                .ToList();

            snapshot.BenutzerRollen = benutzer
                .GroupBy(u => string.IsNullOrWhiteSpace(u.Role) ? "Benutzer" : u.Role.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => new StatisticItem { Name = g.Key, Wert = g.Count(), Zusatz = $"{g.Count(u => u.IsActive)} aktiv" })
                .OrderByDescending(x => x.Wert)
                .ToList();

            snapshot.DokumenteProTag = dokumente
                .Where(d => d.ErstelltAm >= DateTime.Today.AddDays(-13))
                .GroupBy(d => d.ErstelltAm.Date)
                .Select(g => new StatisticItem { Name = g.Key.ToString("dd.MM."), Wert = g.Count(), Zusatz = g.Key.ToShortDateString() })
                .OrderBy(x => DateTime.ParseExact(x.Name + DateTime.Today.Year, "dd.MM.yyyy", null))
                .ToList();

            snapshot.Warnungen = CreateWarnings(artikelListe, dokumente);
            snapshot.Aktivitaeten = ReadRecentActivities();
            return snapshot;
        }

        private List<DocumentRecord> SafeLoadDocuments()
        {
            try { return _repository.LadeDokumente(); }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Statistik: Dokumente konnten nicht geladen werden.");
                return new List<DocumentRecord>();
            }
        }

        private List<UserAccount> SafeLoadUsers()
        {
            try { return _repository.LadeBenutzer(); }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Statistik: Benutzer konnten nicht geladen werden.");
                return new List<UserAccount>();
            }
        }

        private static List<string> CreateWarnings(List<Artikel> artikel, List<DocumentRecord> dokumente)
        {
            var warnings = new List<string>();
            var low = artikel.Where(a => a.SollMenge <= 5).OrderBy(a => a.SollMenge).Take(5).ToList();
            if (low.Count > 0)
                warnings.Add($"{low.Count} Artikel im kritischen Bestand: {string.Join(", ", low.Select(a => $"{a.Artikelnummer} ({a.SollMenge})"))}");

            var ohneLagerort = artikel.Count(a => string.IsNullOrWhiteSpace(a.Lagerort));
            if (ohneLagerort > 0)
                warnings.Add($"{ohneLagerort} Artikel ohne Lagerort.");

            var dokumenteOhneArtikel = dokumente.Count(d => string.IsNullOrWhiteSpace(d.Artikelnummer));
            if (dokumenteOhneArtikel > 0)
                warnings.Add($"{dokumenteOhneArtikel} Dokumente sind noch keinem Artikel zugeordnet.");

            if (warnings.Count == 0)
                warnings.Add("Keine kritischen Hinweise gefunden.");

            return warnings;
        }

        private static int CountLogEntriesToday()
        {
            try
            {
                var file = Path.Combine(AppPaths.LogDirectory, $"alexinventur_{DateTime.Now:yyyyMMdd}.log");
                return File.Exists(file) ? File.ReadLines(file).Count(line => line.Contains("[")) : 0;
            }
            catch { return 0; }
        }

        private static List<string> ReadRecentActivities()
        {
            try
            {
                if (!Directory.Exists(AppPaths.LogDirectory))
                    return new List<string> { "Noch keine Aktivitäten vorhanden." };

                return Directory.GetFiles(AppPaths.LogDirectory, "alexinventur_*.log")
                    .OrderByDescending(File.GetLastWriteTime)
                    .Take(3)
                    .SelectMany(file => File.ReadLines(file).Reverse().Take(8))
                    .Where(line => line.Contains("[AUDIT]") || line.Contains("[INFO]") || line.Contains("[WARN]"))
                    .Take(10)
                    .Select(CleanLogLine)
                    .DefaultIfEmpty("Noch keine Aktivitäten vorhanden.")
                    .ToList();
            }
            catch
            {
                return new List<string> { "Aktivitäten konnten nicht gelesen werden." };
            }
        }

        private static string CleanLogLine(string line)
        {
            if (line.Length <= 23) return line;
            return line[0..19] + " · " + line[24..].Trim();
        }
    }
}
