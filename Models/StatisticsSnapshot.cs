namespace InventurApp.Models
{
    public class StatisticsSnapshot
    {
        public int ArtikelGesamt { get; set; }
        public int LagerorteGesamt { get; set; }
        public int GesamtMenge { get; set; }
        public int NiedrigerBestand { get; set; }
        public int DokumenteGesamt { get; set; }
        public int ScansGesamt { get; set; }
        public int AktiveBenutzer { get; set; }
        public int InaktiveBenutzer { get; set; }
        public int LogEintraegeHeute { get; set; }
        public List<StatisticItem> Lagerorte { get; set; } = new();
        public List<StatisticItem> TopArtikel { get; set; } = new();
        public List<StatisticItem> DokumentKategorien { get; set; } = new();
        public List<StatisticItem> BenutzerRollen { get; set; } = new();
        public List<StatisticItem> DokumenteProTag { get; set; } = new();
        public List<string> Warnungen { get; set; } = new();
        public List<string> Aktivitaeten { get; set; } = new();
    }

    public class StatisticItem
    {
        public string Name { get; set; } = string.Empty;
        public int Wert { get; set; }
        public string Zusatz { get; set; } = string.Empty;
    }
}
