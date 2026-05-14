using System.Data;
using System.Text;
using InventurApp.Models;

namespace InventurApp.Services
{
    public class CsvService
    {
        private const char Separator = ';';

        public void ExportArtikelCsv(IEnumerable<Artikel> artikel, string dateiPfad)
        {
            ExportArtikelCsv(artikel, dateiPfad, new AppSettings());
        }

        public void ExportArtikelCsv(IEnumerable<Artikel> artikel, string dateiPfad, AppSettings settings)
        {
            if (string.IsNullOrWhiteSpace(dateiPfad))
                throw new ArgumentException("Es wurde kein gültiger Dateipfad angegeben.", nameof(dateiPfad));

            settings.EnsureFieldDefaults();

            var directory = Path.GetDirectoryName(dateiPfad);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            var sb = new StringBuilder();
            var customFields = settings.CustomImportFields
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(f => f)
                .ToList();

            var header = new List<string>
            {
                settings.GetFieldName("Artikelnummer"),
                settings.GetFieldName("Bezeichnung"),
                settings.GetFieldName("Lagerort"),
                settings.GetFieldName("SollMenge")
            };
            header.AddRange(customFields);
            sb.AppendLine(string.Join(Separator, header.Select(Escape)));

            foreach (var a in artikel)
            {
                var values = new List<string?>
                {
                    a.Artikelnummer,
                    a.Bezeichnung,
                    a.Lagerort,
                    a.SollMenge.ToString()
                };

                foreach (var field in customFields)
                    values.Add(a.CustomFields != null && a.CustomFields.TryGetValue(field, out var value) ? value : string.Empty);

                sb.AppendLine(string.Join(Separator, values.Select(Escape)));
            }

            File.WriteAllText(dateiPfad, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        }

        public List<string[]> ReadRows(string dateiPfad)
        {
            if (!File.Exists(dateiPfad))
                throw new FileNotFoundException("Die CSV-Datei wurde nicht gefunden.", dateiPfad);

            var result = new List<string[]>();
            using var reader = new StreamReader(dateiPfad, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            var currentRow = new List<string>();
            var currentField = new StringBuilder();
            var inQuotes = false;

            while (reader.Peek() >= 0)
            {
                var ch = (char)reader.Read();

                if (ch == '"')
                {
                    if (inQuotes && reader.Peek() == '"')
                    {
                        currentField.Append('"');
                        reader.Read();
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (ch == Separator && !inQuotes)
                {
                    currentRow.Add(currentField.ToString());
                    currentField.Clear();
                }
                else if ((ch == '\r' || ch == '\n') && !inQuotes)
                {
                    if (ch == '\r' && reader.Peek() == '\n')
                        reader.Read();

                    currentRow.Add(currentField.ToString());
                    currentField.Clear();

                    if (currentRow.Any(cell => !string.IsNullOrWhiteSpace(cell)))
                        result.Add(currentRow.ToArray());

                    currentRow.Clear();
                }
                else
                {
                    currentField.Append(ch);
                }
            }

            if (inQuotes)
                throw new InvalidDataException("Die CSV-Datei enthält ein nicht geschlossenes Anführungszeichen.");

            if (currentField.Length > 0 || currentRow.Count > 0)
            {
                currentRow.Add(currentField.ToString());
                if (currentRow.Any(cell => !string.IsNullOrWhiteSpace(cell)))
                    result.Add(currentRow.ToArray());
            }

            return result;
        }

        public DataTable ReadDataTable(string dateiPfad, int maxRows = 200)
        {
            var rows = ReadRows(dateiPfad);
            var table = new DataTable();

            if (rows.Count == 0)
                return table;

            foreach (var header in rows[0])
                table.Columns.Add(string.IsNullOrWhiteSpace(header) ? "Spalte" : header);

            foreach (var row in rows.Skip(1).Take(maxRows))
            {
                var values = new object[table.Columns.Count];
                for (var i = 0; i < values.Length; i++)
                    values[i] = i < row.Length ? row[i] : string.Empty;

                table.Rows.Add(values);
            }

            return table;
        }

        private static string Escape(string? value)
        {
            value ??= string.Empty;

            if (value.Contains(Separator) || value.Contains('"') || value.Contains('\r') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";

            return value;
        }
    }
}
