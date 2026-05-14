using InventurApp.Models;

namespace InventurApp.Services
{
    public class ExportService
    {
        private readonly CsvService _csvService = new();

        public void ExportArtikelCsv(List<Artikel> artikel, string dateiPfad)
        {
            _csvService.ExportArtikelCsv(artikel, dateiPfad);
        }
    }
}
