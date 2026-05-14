using InventurApp.Models;
using InventurApp.Services;
using InventurApp.UI;
using System.Diagnostics;

namespace InventurApp.Forms
{
    public class DeviceDetailForm : Form
    {
        private readonly Artikel _artikel;
        private readonly List<DocumentRecord> _documents;
        private readonly AppSettings _settings;

        public DeviceDetailForm(Artikel artikel, List<DocumentRecord> documents, AppSettings settings)
        {
            _artikel = artikel;
            _documents = documents ?? new List<DocumentRecord>();
            _settings = settings;

            InitializeUi();
        }

        private void InitializeUi()
        {
            ModernTheme.ApplyForm(this);
            Text = $"Gerätedetails – {_artikel.Artikelnummer}";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(820, 560);
            Size = new Size(980, 700);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = ModernTheme.Background,
                Padding = new Padding(22)
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            var header = ModernTheme.CreateCardPanel();
            header.Dock = DockStyle.Fill;
            header.Padding = new Padding(18, 12, 18, 12);

            var title = new Label
            {
                Text = string.IsNullOrWhiteSpace(_artikel.Artikelnummer) ? "Gerätedetails" : _artikel.Artikelnummer,
                Dock = DockStyle.Top,
                Height = 34,
                ForeColor = ModernTheme.Text,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
                AutoEllipsis = true
            };

            var subtitle = new Label
            {
                Text = string.Join("  ·  ", new[] { _artikel.Bezeichnung, _artikel.Lagerort }.Where(v => !string.IsNullOrWhiteSpace(v))),
                Dock = DockStyle.Top,
                Height = 26,
                ForeColor = ModernTheme.MutedText,
                BackColor = Color.Transparent,
                Font = ModernTheme.SubtitleFont,
                AutoEllipsis = true
            };

            header.Controls.Add(subtitle);
            header.Controls.Add(title);
            root.Controls.Add(header, 0, 0);

            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F),
                Padding = new Point(14, 6)
            };
            root.Controls.Add(tabs, 0, 1);

            AddTab(tabs, "Übersicht", BuildOverviewPage());
            AddTab(tabs, "Technische Daten", BuildGroupedFieldsPage());
            AddTab(tabs, "Dokumente", BuildDocumentsPage());
            AddTab(tabs, "Notizen", BuildNotesPage());
        }

        private static void AddTab(TabControl tabs, string title, Control content)
        {
            var page = new TabPage(title)
            {
                BackColor = ModernTheme.Background,
                Padding = new Padding(12)
            };
            content.Dock = DockStyle.Fill;
            page.Controls.Add(content);
            tabs.TabPages.Add(page);
        }

        private Control BuildOverviewPage()
        {
            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = ModernTheme.Background,
                Padding = new Padding(0)
            };

            AddSection(panel, "Basisdaten", new[]
            {
                new KeyValuePair<string,string>(_settings.GetFieldName("Artikelnummer"), _artikel.Artikelnummer ?? string.Empty),
                new KeyValuePair<string,string>(_settings.GetFieldName("Bezeichnung"), _artikel.Bezeichnung ?? string.Empty),
                new KeyValuePair<string,string>(_settings.GetFieldName("Lagerort"), _artikel.Lagerort ?? string.Empty),
                new KeyValuePair<string,string>(_settings.GetFieldName("SollMenge"), _artikel.SollMenge.ToString()),
                new KeyValuePair<string,string>("Dokumente", _documents.Count.ToString())
            });

            var important = (_artikel.CustomFields ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase))
                .Where(kv => IsImportantField(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value))
                .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();
            AddSection(panel, "Wichtige Gerätedaten", important);
            return panel;
        }

        private Control BuildGroupedFieldsPage()
        {
            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = ModernTheme.Background
            };

            var fields = _artikel.CustomFields ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var nonEmpty = fields
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                .OrderBy(kv => GetGroupOrder(kv.Key))
                .ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (nonEmpty.Count == 0)
            {
                AddEmpty(panel, "Keine technischen Zusatzfelder vorhanden.");
                return panel;
            }

            foreach (var group in nonEmpty.GroupBy(kv => GetGroupName(kv.Key)))
                AddSection(panel, group.Key, group);

            return panel;
        }

        private Control BuildDocumentsPage()
        {
            var list = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                Dock = DockStyle.Fill,
                BackColor = ModernTheme.Surface,
                ForeColor = ModernTheme.Text,
                Font = new Font("Segoe UI", 10F)
            };
            list.Columns.Add("Titel", 220);
            list.Columns.Add("Kategorie", 140);
            list.Columns.Add("Datei", 260);
            list.Columns.Add("Datum", 150);

            foreach (var doc in _documents.OrderByDescending(d => d.ErstelltAm))
            {
                var item = new ListViewItem(doc.Titel);
                item.SubItems.Add(doc.Kategorie);
                item.SubItems.Add(doc.DateiName);
                item.SubItems.Add(doc.ErstelltAm.ToString("dd.MM.yyyy HH:mm"));
                item.Tag = doc;
                list.Items.Add(item);
            }

            list.DoubleClick += (_, _) =>
            {
                if (list.SelectedItems.Count == 0 || list.SelectedItems[0].Tag is not DocumentRecord doc)
                    return;

                if (!File.Exists(doc.DateiPfad))
                {
                    MessageBox.Show("Die Datei wurde nicht gefunden.", "Dokument öffnen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Process.Start(new ProcessStartInfo(doc.DateiPfad) { UseShellExecute = true });
            };

            return list;
        }

        private Control BuildNotesPage()
        {
            var box = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BorderStyle = BorderStyle.None,
                BackColor = ModernTheme.Surface,
                ForeColor = ModernTheme.Text,
                Font = new Font("Segoe UI", 10F),
                Text = "Notizen sind für Geräte vorbereitet.\r\n\r\nAls nächster Schritt kann hier ein echtes Notiz-/Historienfeld mit Speicherung pro Gerät eingebaut werden."
            };
            return box;
        }

        private static void AddSection(FlowLayoutPanel panel, string title, IEnumerable<KeyValuePair<string, string>> items)
        {
            var cleaned = items.Where(kv => !string.IsNullOrWhiteSpace(kv.Value)).ToList();
            if (cleaned.Count == 0)
                return;

            var card = ModernTheme.CreateCardPanel();
            card.Width = Math.Max(720, panel.ClientSize.Width - 28);
            card.AutoSize = true;
            card.Margin = new Padding(0, 0, 0, 14);
            card.Padding = new Padding(16);

            var header = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 30,
                ForeColor = ModernTheme.Text,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold)
            };
            card.Controls.Add(header);

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 0)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            foreach (var kv in cleaned)
            {
                var row = table.RowCount++;
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

                table.Controls.Add(new Label
                {
                    Text = kv.Key,
                    Dock = DockStyle.Fill,
                    ForeColor = ModernTheme.MutedText,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9.5F),
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                }, 0, row);

                table.Controls.Add(new Label
                {
                    Text = kv.Value,
                    Dock = DockStyle.Fill,
                    ForeColor = ModernTheme.Text,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9.5F),
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                }, 1, row);
            }

            card.Controls.Add(table);
            table.BringToFront();
            panel.Controls.Add(card);
        }

        private static void AddEmpty(FlowLayoutPanel panel, string text)
        {
            panel.Controls.Add(new Label
            {
                Text = text,
                Width = 520,
                Height = 80,
                ForeColor = ModernTheme.MutedText,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = ModernTheme.SubtitleFont
            });
        }

        private static bool IsImportantField(string fieldName)
        {
            var name = fieldName.ToLowerInvariant();
            return name.Contains("hersteller") || name.Contains("modell") || name.Contains("serial") || name.Contains("serien") ||
                   name.Contains("cpu") || name.Contains("ram") || name.Contains("speicher") || name.Contains("disk") ||
                   name.Contains("mac") || name.Contains("ip") || name.Contains("bios");
        }

        private static int GetGroupOrder(string fieldName)
        {
            return GetGroupName(fieldName) switch
            {
                "System" => 10,
                "Identifikation" => 20,
                "BIOS" => 30,
                "CPU" => 40,
                "Arbeitsspeicher" => 50,
                "Datenträger" => 60,
                "Netzwerk" => 70,
                _ => 99
            };
        }

        private static string GetGroupName(string fieldName)
        {
            var name = fieldName.ToLowerInvariant();
            if (name.Contains("system") || name.Contains("hersteller") || name.Contains("modell") || name.Contains("manufacturer") || name.Contains("model")) return "System";
            if (name.Contains("serien") || name.Contains("serial") || name.Contains("inventar") || name.Contains("asset") || name.Contains("gerät")) return "Identifikation";
            if (name.Contains("bios")) return "BIOS";
            if (name.Contains("cpu") || name.Contains("prozessor") || name.Contains("processor")) return "CPU";
            if (name.Contains("ram") || name.Contains("speicher") || name.Contains("memory")) return "Arbeitsspeicher";
            if (name.Contains("disk") || name.Contains("drive") || name.Contains("hdd") || name.Contains("ssd") || name.Contains("datenträger")) return "Datenträger";
            if (name.Contains("nic") || name.Contains("netz") || name.Contains("mac") || name.Contains("ip") || name.Contains("adapter")) return "Netzwerk";
            return "Sonstige Felder";
        }
    }
}
