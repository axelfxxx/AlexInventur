using InventurApp.Models;
using InventurApp.Services;
using InventurApp.UI;
using System.ComponentModel;
using System.Diagnostics;

namespace InventurApp.Forms
{
    public class DocumentManagerForm : Form
    {
        private readonly DocumentService _documentService = new();
        private readonly TwainService _twainService = new();
        private readonly ArtikelService _artikelService;
        private readonly AppSettings _settings;
        private readonly string _currentUser;
        private readonly bool _canModify;

        private readonly BindingList<DocumentRecord> _documents = new();
        private readonly DataGridView _grid = new();
        private readonly TextBox _txtTitle = new();
        private readonly TextBox _txtSearch = new();
        private readonly ComboBox _cmbCategory = new();
        private readonly ComboBox _cmbArtikel = new();
        private readonly Label _lblStatus = new();

        public DocumentManagerForm(ArtikelService artikelService, AppSettings settings, string currentUser, bool canModify)
        {
            _artikelService = artikelService;
            _settings = settings;
            _currentUser = currentUser;
            _canModify = canModify;

            Text = "Dokumentenverwaltung";
            Size = new Size(1040, 700);
            MinimumSize = new Size(860, 560);
            ModernTheme.ApplyForm(this);

            BuildLayout();
            LoadArticles();
            LoadDocuments();
        }

        private void BuildLayout()
        {
            Controls.Clear();

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(24),
                BackColor = ModernTheme.Background
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 118));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            Controls.Add(root);

            root.Controls.Add(ModernTheme.CreateTitleLabel("Dokumentenverwaltung", "Scans, Belege und Dateien zentral ablegen und optional einem Artikel zuordnen"), 0, 0);

            var inputCard = ModernTheme.CreateCardPanel();
            inputCard.Dock = DockStyle.Fill;
            inputCard.Padding = new Padding(16);
            root.Controls.Add(inputCard, 0, 1);

            var input = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 2,
                BackColor = ModernTheme.Surface
            };
            input.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            input.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            input.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            input.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
            input.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            input.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            input.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            input.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            inputCard.Controls.Add(input);

            AddLabel(input, "Titel", 0, 0);
            _txtTitle.Dock = DockStyle.Fill;
            _txtTitle.PlaceholderText = "z. B. Lieferschein, Rechnung, Inventurbeleg";
            ModernTheme.ApplyInput(_txtTitle);
            input.Controls.Add(_txtTitle, 1, 0);

            AddLabel(input, "Kategorie", 2, 0);
            _cmbCategory.Dock = DockStyle.Fill;
            _cmbCategory.DropDownStyle = ComboBoxStyle.DropDown;
            _cmbCategory.Items.AddRange(new object[] { "Allgemein", "Scan", "Lieferschein", "Rechnung", "Inventurbeleg", "Foto", "Sonstiges" });
            _cmbCategory.Text = "Allgemein";
            ModernTheme.ApplyInput(_cmbCategory);
            input.Controls.Add(_cmbCategory, 3, 0);

            AddLabel(input, "Artikel", 4, 0);
            _cmbArtikel.Dock = DockStyle.Fill;
            _cmbArtikel.DropDownStyle = ComboBoxStyle.DropDownList;
            ModernTheme.ApplyInput(_cmbArtikel);
            input.Controls.Add(_cmbArtikel, 5, 0);

            var btnScan = new Button { Text = "🖨 Scannen", Enabled = _canModify && _settings.ScannerEnabled && _settings.TwainEnabled };
            var btnImport = new Button { Text = "＋ Datei hinzufügen", Enabled = _canModify };
            var btnOpen = new Button { Text = "↗ Öffnen" };
            var btnDelete = new Button { Text = "🗑 Entfernen", Enabled = _canModify };
            var btnRefresh = new Button { Text = "⟳ Aktualisieren" };

            btnScan.Click += (_, _) => ScanDocument();
            btnImport.Click += (_, _) => ImportDocument();
            btnOpen.Click += (_, _) => OpenSelectedDocument();
            btnDelete.Click += (_, _) => DeleteSelectedDocument();
            btnRefresh.Click += (_, _) => LoadDocuments();

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, BackColor = ModernTheme.Surface };
            foreach (var button in new[] { btnScan, btnImport, btnOpen, btnDelete, btnRefresh })
            {
                button.Width = button.Text.Contains("hinzufügen") ? 150 : 120;
                button.Height = 36;
                button.Margin = new Padding(0, 4, 10, 0);
                ModernTheme.ApplySecondaryButton(button);
                buttons.Controls.Add(button);
            }
            input.SetColumnSpan(buttons, 4);
            input.Controls.Add(buttons, 0, 1);

            _txtSearch.Dock = DockStyle.Fill;
            _txtSearch.PlaceholderText = "Dokumente suchen...";
            _txtSearch.TextChanged += (_, _) => ApplyFilter();
            ModernTheme.ApplyInput(_txtSearch);
            input.SetColumnSpan(_txtSearch, 2);
            input.Controls.Add(_txtSearch, 4, 1);

            var gridCard = ModernTheme.CreateCardPanel();
            gridCard.Dock = DockStyle.Fill;
            gridCard.Padding = new Padding(12);
            root.Controls.Add(gridCard, 0, 2);

            _grid.Dock = DockStyle.Fill;
            _grid.ReadOnly = true;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.MultiSelect = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.AutoGenerateColumns = false;
            _grid.DoubleClick += (_, _) => OpenSelectedDocument();
            ModernTheme.ApplyGrid(_grid);
            ConfigureColumns();
            gridCard.Controls.Add(_grid);

            _lblStatus.Dock = DockStyle.Fill;
            ModernTheme.ApplyStatus(_lblStatus);
            root.Controls.Add(_lblStatus, 0, 3);
        }

        private static void AddLabel(TableLayoutPanel layout, string text, int col, int row)
        {
            var label = new Label { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            ModernTheme.ApplyLabel(label, muted: true);
            layout.Controls.Add(label, col, row);
        }

        private void ConfigureColumns()
        {
            _grid.Columns.Clear();
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DocumentRecord.Titel), HeaderText = "Titel", FillWeight = 170 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DocumentRecord.Kategorie), HeaderText = "Kategorie", FillWeight = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DocumentRecord.Artikelnummer), HeaderText = "Artikel", FillWeight = 95 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DocumentRecord.DateiName), HeaderText = "Datei", FillWeight = 150 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DocumentRecord.Quelle), HeaderText = "Quelle", FillWeight = 110 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DocumentRecord.ErstelltVon), HeaderText = "Benutzer", FillWeight = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(DocumentRecord.ErstelltAm), HeaderText = "Datum", FillWeight = 105, DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy HH:mm" } });
        }

        private void LoadArticles()
        {
            _cmbArtikel.Items.Clear();
            _cmbArtikel.Items.Add("(keinem Artikel zuordnen)");
            foreach (var artikel in _artikelService.GetAlle().OrderBy(a => a.Artikelnummer))
                _cmbArtikel.Items.Add($"{artikel.Artikelnummer} – {artikel.Bezeichnung}");
            _cmbArtikel.SelectedIndex = 0;
        }

        private void LoadDocuments()
        {
            _documents.Clear();
            foreach (var document in _documentService.GetAll())
                _documents.Add(document);
            _grid.DataSource = _documents;
            SetStatus($"{_documents.Count} Dokument(e) geladen");
        }

        private void ApplyFilter()
        {
            var q = _txtSearch.Text.Trim();
            var filtered = string.IsNullOrWhiteSpace(q)
                ? _documentService.GetAll()
                : _documentService.GetAll().Where(d =>
                    (d.Titel?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.Kategorie?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.Artikelnummer?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (d.DateiName?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

            _documents.Clear();
            foreach (var document in filtered)
                _documents.Add(document);
            SetStatus($"{_documents.Count} Treffer");
        }

        private void ScanDocument()
        {
            if (!_canModify)
            {
                MessageBox.Show("Du hast keine Berechtigung zum Scannen.", "Dokumente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = _twainService.Scan(_settings.TwainSourceName);
            if (!result.Success || string.IsNullOrWhiteSpace(result.FilePath))
            {
                MessageBox.Show(result.Message, "Scanner", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var record = _documentService.RegisterScan(
                result.FilePath,
                GetTitleOrDefault("Scan"),
                string.IsNullOrWhiteSpace(_cmbCategory.Text) ? "Scan" : _cmbCategory.Text,
                GetSelectedArtikelnummer(),
                string.IsNullOrWhiteSpace(_settings.TwainSourceName) ? "TWAIN" : _settings.TwainSourceName,
                _currentUser);

            LoadDocuments();
            SelectDocument(record.Id);
            MessageBox.Show(result.Message, "Scanner", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ImportDocument()
        {
            if (!_canModify) return;
            using var dialog = new OpenFileDialog
            {
                Title = "Dokument auswählen",
                Filter = "Dokumente|*.pdf;*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff;*.txt;*.doc;*.docx;*.xls;*.xlsx|Alle Dateien|*.*"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                var record = _documentService.ImportFile(
                    dialog.FileName,
                    GetTitleOrDefault(Path.GetFileNameWithoutExtension(dialog.FileName)),
                    _cmbCategory.Text,
                    GetSelectedArtikelnummer(),
                    _currentUser);
                LoadDocuments();
                SelectDocument(record.Id);
                SetStatus("Dokument hinzugefügt");
            }
            catch (Exception ex)
            {
                AppLogger.Error(ex, "Dokumentimport fehlgeschlagen.");
                MessageBox.Show(ex.Message, "Dokumentimport", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenSelectedDocument()
        {
            var document = GetSelectedDocument();
            if (document == null) return;
            if (!File.Exists(document.DateiPfad))
            {
                MessageBox.Show("Die Datei wurde nicht gefunden:\n" + document.DateiPfad, "Dokument öffnen", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(document.DateiPfad) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Dokument öffnen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteSelectedDocument()
        {
            if (!_canModify) return;
            var document = GetSelectedDocument();
            if (document == null) return;

            var deleteFile = MessageBox.Show(
                "Dokument aus der Verwaltung entfernen?\n\nJa = Eintrag und Datei löschen\nNein = nur Eintrag löschen\nAbbrechen = nichts tun",
                "Dokument entfernen",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (deleteFile == DialogResult.Cancel)
                return;

            try
            {
                _documentService.Delete(document, deleteFile == DialogResult.Yes);
                LoadDocuments();
                SetStatus("Dokument entfernt");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Dokument entfernen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DocumentRecord? GetSelectedDocument() => _grid.CurrentRow?.DataBoundItem as DocumentRecord;

        private string? GetSelectedArtikelnummer()
        {
            var value = _cmbArtikel.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(value) || value.StartsWith("(")) return null;
            var sep = value.IndexOf('–');
            return sep > 0 ? value[..sep].Trim() : value.Trim();
        }

        private string GetTitleOrDefault(string fallback)
        {
            var title = _txtTitle.Text.Trim();
            return string.IsNullOrWhiteSpace(title) ? fallback : title;
        }

        private void SelectDocument(Guid id)
        {
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (row.DataBoundItem is DocumentRecord doc && doc.Id == id)
                {
                    row.Selected = true;
                    _grid.CurrentCell = row.Cells[0];
                    return;
                }
            }
        }

        private void SetStatus(string text) => _lblStatus.Text = $"{DateTime.Now:HH:mm:ss} – {text}";
    }
}
