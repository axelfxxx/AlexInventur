using InventurApp.Models;
using System.Text;

namespace InventurApp.Services
{
    public class PdfExportService
    {
        public string ExportArtikelPdf(IEnumerable<Artikel> artikel, string fileName)
        {
            var lines = new List<string>
            {
                "Alex Inventur - Artikelliste",
                $"Erstellt am {DateTime.Now:dd.MM.yyyy HH:mm}",
                "",
                "Artikelnummer | Bezeichnung | Lagerort | Soll-Menge"
            };

            lines.AddRange(artikel.Select(a => $"{a.Artikelnummer} | {a.Bezeichnung} | {a.Lagerort} | {a.SollMenge}"));
            WriteSimplePdf(fileName, lines);
            return fileName;
        }

        private static void WriteSimplePdf(string path, IReadOnlyList<string> lines)
        {
            var objects = new List<string>();
            objects.Add("<< /Type /Catalog /Pages 2 0 R >>");
            objects.Add("<< /Type /Pages /Kids [3 0 R] /Count 1 >>");
            objects.Add("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>");
            objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");

            var content = new StringBuilder();
            content.AppendLine("BT");
            content.AppendLine("/F1 10 Tf");
            content.AppendLine("50 800 Td");
            foreach (var raw in lines.Take(42))
            {
                var line = raw.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
                content.AppendLine($"({line}) Tj");
                content.AppendLine("0 -16 Td");
            }
            content.AppendLine("ET");
            var stream = content.ToString();
            objects.Add($"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}endstream");

            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            using var writer = new StreamWriter(fs, Encoding.ASCII) { AutoFlush = true };
            writer.WriteLine("%PDF-1.4");
            var offsets = new List<long> { 0 };
            for (var i = 0; i < objects.Count; i++)
            {
                offsets.Add(fs.Position);
                writer.WriteLine($"{i + 1} 0 obj");
                writer.WriteLine(objects[i]);
                writer.WriteLine("endobj");
            }
            var xref = fs.Position;
            writer.WriteLine("xref");
            writer.WriteLine($"0 {objects.Count + 1}");
            writer.WriteLine("0000000000 65535 f ");
            foreach (var offset in offsets.Skip(1))
                writer.WriteLine($"{offset:0000000000} 00000 n ");
            writer.WriteLine("trailer");
            writer.WriteLine($"<< /Size {objects.Count + 1} /Root 1 0 R >>");
            writer.WriteLine("startxref");
            writer.WriteLine(xref);
            writer.WriteLine("%%EOF");
        }
    }
}
