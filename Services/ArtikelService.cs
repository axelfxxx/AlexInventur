using InventurApp.Models;
using InventurApp.Persistence;
using System.ComponentModel;

namespace InventurApp.Services
{
    public class ArtikelService
    {
        private readonly SqliteRepository _repo = new();

        private readonly BindingList<Artikel> _artikel;

        private readonly BindingList<string> _bezeichnungen = new()
        {
            "Computer",
            "Laptop",
            "Smartboard",
            "Hotspot",
            "Bildschirm",
            "Sonstiges"
        };

        public ArtikelService()
        {
            var geladen = _repo.LadeArtikel() ?? new List<Artikel>();
            _artikel = new BindingList<Artikel>(geladen);

            foreach (var artikel in _artikel)
                EnsureBezeichnung(artikel.Bezeichnung);
        }

        public List<Artikel> GetAlle()
        {
            return _artikel.ToList();
        }

        public BindingList<string> GetBezeichnungen()
        {
            return _bezeichnungen;
        }

        public void EnsureBezeichnung(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (!_bezeichnungen.Contains(value))
                _bezeichnungen.Add(value);
        }

        public void Upsert(Artikel artikel)
        {
            var fehler = Validate(artikel);
            if (fehler.Count > 0)
                throw new ArgumentException(string.Join(Environment.NewLine, fehler));

            artikel.Artikelnummer = artikel.Artikelnummer.Trim();
            artikel.Bezeichnung = artikel.Bezeichnung.Trim();
            artikel.Lagerort = artikel.Lagerort?.Trim() ?? string.Empty;

            EnsureBezeichnung(artikel.Bezeichnung);

            var existing = _artikel.FirstOrDefault(a =>
                a.Id == artikel.Id || a.Artikelnummer == artikel.Artikelnummer);

            if (existing != null)
            {
                existing.Artikelnummer = artikel.Artikelnummer;
                existing.Bezeichnung = artikel.Bezeichnung;
                existing.Lagerort = artikel.Lagerort;
                existing.SollMenge = artikel.SollMenge;
                existing.CustomFields = artikel.CustomFields ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                _artikel.Add(artikel);
            }

            Speichern();
        }


        public List<string> Validate(Artikel? artikel, bool pruefeDuplikate = false)
        {
            var fehler = new List<string>();

            if (artikel == null)
            {
                fehler.Add("Der Artikel ist leer.");
                return fehler;
            }

            if (string.IsNullOrWhiteSpace(artikel.Artikelnummer))
                fehler.Add("Die Artikelnummer darf nicht leer sein.");

            if (string.IsNullOrWhiteSpace(artikel.Bezeichnung))
                fehler.Add("Die Bezeichnung darf nicht leer sein.");

            if (artikel.SollMenge < 0)
                fehler.Add("Die Soll-Menge darf nicht negativ sein.");

            if (pruefeDuplikate && ArtikelnummerExistiert(artikel.Artikelnummer, artikel.Id))
                fehler.Add("Diese Artikelnummer existiert bereits.");

            return fehler;
        }

        public void Löschen(Guid id)
        {
            var item = _artikel.FirstOrDefault(a => a.Id == id);
            if (item == null) return;

            _artikel.Remove(item);
            Speichern();
        }

        public void Speichern()
        {
            _repo.SpeichereArtikel(_artikel.ToList());
        }

        public string GetNaechsteArtikelnummer()
        {
            if (_artikel.Count == 0)
                return "A-001";

            var max = _artikel
                .Select(a => a.Artikelnummer)
                .Where(n => n?.StartsWith("A-") == true)
                .Select(n => int.TryParse(n![2..], out var x) ? x : 0)
                .DefaultIfEmpty(0)
                .Max();

            return $"A-{(max + 1):000}";
        }

        public bool ArtikelnummerExistiert(string artikelnummer, Guid ausgenommeneId)
        {
            if (string.IsNullOrWhiteSpace(artikelnummer))
                return false;

            return _artikel.Any(a =>
                string.Equals(a.Artikelnummer, artikelnummer, StringComparison.OrdinalIgnoreCase) &&
                a.Id != ausgenommeneId);
        }
    }
}
