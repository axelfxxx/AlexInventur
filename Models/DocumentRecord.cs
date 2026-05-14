namespace InventurApp.Models
{
    public class DocumentRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Titel { get; set; } = string.Empty;
        public string Kategorie { get; set; } = "Allgemein";
        public string DateiPfad { get; set; } = string.Empty;
        public string DateiName { get; set; } = string.Empty;
        public string? Artikelnummer { get; set; }
        public string Quelle { get; set; } = string.Empty;
        public DateTime ErstelltAm { get; set; } = DateTime.Now;
        public string ErstelltVon { get; set; } = string.Empty;
    }
}
