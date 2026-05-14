namespace InventurApp.Models
{
    public class Artikel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Artikelnummer { get; set; }
        public string Bezeichnung { get; set; }
        public string Lagerort { get; set; }
        public int SollMenge { get; set; }
        public Dictionary<string, string> CustomFields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
