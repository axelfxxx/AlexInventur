using InventurApp.Models;
using InventurApp.Persistence;

namespace InventurApp.Services
{
    public class DocumentService
    {
        private readonly SqliteRepository _repository = new();

        public List<DocumentRecord> GetAll() => _repository.LadeDokumente();

        public List<DocumentRecord> GetByArtikelnummer(string? artikelnummer)
        {
            var normalized = (artikelnummer ?? string.Empty).Trim();
            return GetAll()
                .Where(d => string.Equals(d.Artikelnummer ?? string.Empty, normalized, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(d => d.ErstelltAm)
                .ToList();
        }

        public DocumentRecord ImportFile(string sourceFile, string title, string category, string? artikelnummer, string createdBy)
        {
            if (string.IsNullOrWhiteSpace(sourceFile) || !File.Exists(sourceFile))
                throw new FileNotFoundException("Die ausgewählte Datei wurde nicht gefunden.", sourceFile);

            AppPaths.EnsureAll();
            var extension = Path.GetExtension(sourceFile);
            var safeTitle = MakeSafeFileName(string.IsNullOrWhiteSpace(title) ? Path.GetFileNameWithoutExtension(sourceFile) : title);
            var targetFileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{safeTitle}{extension}";
            var targetPath = Path.Combine(AppPaths.AttachmentDirectory, targetFileName);
            File.Copy(sourceFile, targetPath, overwrite: false);

            var record = new DocumentRecord
            {
                Titel = string.IsNullOrWhiteSpace(title) ? Path.GetFileNameWithoutExtension(sourceFile) : title.Trim(),
                Kategorie = string.IsNullOrWhiteSpace(category) ? "Allgemein" : category.Trim(),
                DateiPfad = targetPath,
                DateiName = Path.GetFileName(targetPath),
                Artikelnummer = string.IsNullOrWhiteSpace(artikelnummer) ? null : artikelnummer.Trim(),
                Quelle = "Dateiimport",
                ErstelltVon = createdBy,
                ErstelltAm = DateTime.Now
            };

            _repository.SpeichereDokument(record);
            AppLogger.Info($"Dokument importiert: {record.DateiName}");
            return record;
        }

        public DocumentRecord RegisterScan(string scannedFile, string title, string category, string? artikelnummer, string sourceName, string createdBy)
        {
            if (string.IsNullOrWhiteSpace(scannedFile) || !File.Exists(scannedFile))
                throw new FileNotFoundException("Die Scan-Datei wurde nicht gefunden.", scannedFile);

            var record = new DocumentRecord
            {
                Titel = string.IsNullOrWhiteSpace(title) ? "Scan" : title.Trim(),
                Kategorie = string.IsNullOrWhiteSpace(category) ? "Scan" : category.Trim(),
                DateiPfad = scannedFile,
                DateiName = Path.GetFileName(scannedFile),
                Artikelnummer = string.IsNullOrWhiteSpace(artikelnummer) ? null : artikelnummer.Trim(),
                Quelle = string.IsNullOrWhiteSpace(sourceName) ? "TWAIN" : sourceName.Trim(),
                ErstelltVon = createdBy,
                ErstelltAm = DateTime.Now
            };

            _repository.SpeichereDokument(record);
            AppLogger.Info($"Scan registriert: {record.DateiName}");
            return record;
        }

        public void UpdateDocument(DocumentRecord document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            document.Titel = string.IsNullOrWhiteSpace(document.Titel) ? "Dokument" : document.Titel.Trim();
            document.Kategorie = string.IsNullOrWhiteSpace(document.Kategorie) ? "Allgemein" : document.Kategorie.Trim();
            document.Artikelnummer = string.IsNullOrWhiteSpace(document.Artikelnummer) ? null : document.Artikelnummer.Trim();
            document.DateiName = string.IsNullOrWhiteSpace(document.DateiName) ? Path.GetFileName(document.DateiPfad) : document.DateiName.Trim();

            _repository.SpeichereDokument(document);
            AppLogger.Info($"Dokument aktualisiert: {document.DateiName}");
        }

        public void Delete(DocumentRecord document, bool deletePhysicalFile = false)
        {
            _repository.LoescheDokument(document.Id);
            if (deletePhysicalFile && File.Exists(document.DateiPfad))
                File.Delete(document.DateiPfad);
        }

        private static string MakeSafeFileName(string value)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var cleaned = new string(value.Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim();
            return string.IsNullOrWhiteSpace(cleaned) ? "Dokument" : cleaned;
        }
    }
}
