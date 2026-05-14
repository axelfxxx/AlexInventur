using InventurApp.Models;
using InventurApp.Services;
using InventurApp.UI;
using System.Diagnostics;

namespace InventurApp.Forms
{
    public partial class FileManagerForm : Form
    {
        private readonly FileService _fileService = new();
        private readonly DocumentService _documentService = new();
        private readonly ArtikelService? _artikelService;
        private readonly AppSettings? _settings;
        private readonly string _currentUser;
        private readonly bool _canModifyDocuments;

        private readonly List<FileEntry> _allEntries = new();
        private TreeView? _navigationTree;
        private TableLayoutPanel? _rootLayout;
        private TableLayoutPanel? _mainLayout;
        private Panel? _listCard;
        private Panel? _detailsCard;
        private FlowLayoutPanel? _actionPanel;
        private Label? _title;
        private Label? _detailsTitle;
        private Label? _detailsInfo;
        private string _selectedCategory = "all";

        public FileManagerForm()
            : this(null, null, Environment.UserName, false)
        {
        }

        public FileManagerForm(ArtikelService? artikelService, AppSettings? settings, string currentUser, bool canModifyDocuments)
        {
            _artikelService = artikelService;
            _settings = settings;
            _currentUser = currentUser;
            _canModifyDocuments = canModifyDocuments;

            InitializeComponent();
            ApplyModernDesign();
            SetupGrid();
            SetupContextMenu();
            SetupDragDrop();
            LoadEntries();

            dgvFiles.CellDoubleClick += (s, e) => OpenSelectedEntry();
            dgvFiles.SelectionChanged += (_, _) => UpdateDetailsPanel();
            dgvFiles.KeyDown += DgvFiles_KeyDown;
        }

        private void ApplyModernDesign()
        {
            ModernTheme.ApplyForm(this);
            Text = "Alex Inventur – Dateimanager";
            ClientSize = new Size(1180, 760);
            MinimumSize = new Size(860, 560);
            Controls.Clear();

            _rootLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = ModernTheme.Background,
                Padding = new Padding(22, 18, 22, 12),
                ColumnCount = 2,
                RowCount = 2
            };
            _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 245));
            _rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
            _rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(_rootLayout);

            _title = new Label
            {
                Text = "Dateimanager 2.0\nDateien, Exporte, Backups und Dokumente zentral verwalten",
                Dock = DockStyle.Fill,
                ForeColor = ModernTheme.Text,
                BackColor = Color.Transparent,
                Font = ModernTheme.TitleFont
            };
            _rootLayout.Controls.Add(_title, 0, 0);
            _rootLayout.SetColumnSpan(_title, 2);

            var navCard = ModernTheme.CreateCardPanel();
            navCard.Dock = DockStyle.Fill;
            navCard.Padding = new Padding(10);
            _rootLayout.Controls.Add(navCard, 0, 1);

            _navigationTree = new TreeView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                HideSelection = false,
                FullRowSelect = true,
                ShowLines = false,
                ShowPlusMinus = true,
                BackColor = ModernTheme.Surface,
                ForeColor = ModernTheme.Text,
                Font = ModernTheme.BaseFont,
                ItemHeight = 31
            };
            _navigationTree.Nodes.Add(CreateNode("Dateien", "files-root",
                CreateNode("Alle Dateien", "all"),
                CreateNode("CSV-Exporte", "csv"),
                CreateNode("PDF-Exporte", "pdf"),
                CreateNode("Backups", "backup")));
            _navigationTree.Nodes.Add(CreateNode("Dokumente", "documents-root",
                CreateNode("Alle Dokumente", "documents"),
                CreateNode("Scans", "doc-scan"),
                CreateNode("Rechnungen", "doc-invoice"),
                CreateNode("Lieferscheine", "doc-delivery"),
                CreateNode("Fotos", "doc-photo")));
            _navigationTree.ExpandAll();
            _navigationTree.AfterSelect += NavigationTree_AfterSelect;
            navCard.Controls.Add(_navigationTree);

            _mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = ModernTheme.Background,
                Padding = new Padding(14, 0, 0, 0),
                ColumnCount = 2,
                RowCount = 2
            };
            _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _rootLayout.Controls.Add(_mainLayout, 1, 1);

            BuildTopBar();
            BuildListAndDetails();

            statusStrip1.BackColor = ModernTheme.Surface;
            statusStrip1.ForeColor = ModernTheme.MutedText;
            statusStrip1.SizingGrip = false;
            statusStrip1.Dock = DockStyle.Bottom;
            Controls.Add(statusStrip1);
            statusStrip1.BringToFront();

            Resize -= FileManagerForm_Resize;
            Resize += FileManagerForm_Resize;
            ApplyResponsiveLayout();
            _navigationTree.SelectedNode = FindNodeByTag(_navigationTree.Nodes, "all");
        }

        private void BuildTopBar()
        {
            if (_mainLayout == null) return;

            var topBar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                BackColor = ModernTheme.Background
            };
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 285));
            _mainLayout.Controls.Add(topBar, 0, 0);
            _mainLayout.SetColumnSpan(topBar, 2);

            _actionPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = ModernTheme.Background,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0, 3, 0, 9)
            };

            btnOpen.Text = "↗ Öffnen";
            btnDelete.Text = "🗑 Löschen";
            btnRefresh.Text = "⟳ Aktualisieren";

            foreach (var button in new[] { btnOpen, btnDelete, btnRefresh })
            {
                button.Width = button == btnRefresh ? 136 : 112;
                button.Height = 38;
                button.Margin = new Padding(0, 0, 10, 0);
                _actionPanel.Controls.Add(button);
            }

            ModernTheme.ApplyPrimaryButton(btnOpen);
            ModernTheme.ApplyDangerButton(btnDelete);
            ModernTheme.ApplySecondaryButton(btnRefresh);
            topBar.Controls.Add(_actionPanel, 0, 0);

            txtSearch.PlaceholderText = "Datei oder Dokument suchen...";
            txtSearch.Dock = DockStyle.Fill;
            txtSearch.Margin = new Padding(8, 6, 0, 10);
            ModernTheme.ApplyInput(txtSearch);
            lblSearch.Visible = false;
            topBar.Controls.Add(txtSearch, 1, 0);
        }

        private void BuildListAndDetails()
        {
            if (_mainLayout == null) return;

            _listCard = ModernTheme.CreateCardPanel();
            _listCard.Dock = DockStyle.Fill;
            _listCard.Padding = new Padding(14);
            _mainLayout.Controls.Add(_listCard, 0, 1);

            dgvFiles.Dock = DockStyle.Fill;
            ModernTheme.ApplyGrid(dgvFiles);
            _listCard.Controls.Add(dgvFiles);

            _detailsCard = ModernTheme.CreateCardPanel();
            _detailsCard.Dock = DockStyle.Fill;
            _detailsCard.Padding = new Padding(18);
            _mainLayout.Controls.Add(_detailsCard, 1, 1);

            var details = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = ModernTheme.Surface
            };
            details.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            details.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            details.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            _detailsCard.Controls.Add(details);

            _detailsTitle = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Details",
                ForeColor = ModernTheme.Text,
                Font = ModernTheme.SubtitleFont,
                TextAlign = ContentAlignment.MiddleLeft
            };
            details.Controls.Add(_detailsTitle, 0, 0);

            _detailsInfo = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Wähle links eine Datei oder ein Dokument aus.",
                ForeColor = ModernTheme.MutedText,
                Font = ModernTheme.BaseFont,
                AutoEllipsis = true
            };
            details.Controls.Add(_detailsInfo, 0, 1);

            var hint = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Tipp: Dateien per Drag & Drop hinzufügen. Rechtsklick öffnet Aktionen.",
                ForeColor = ModernTheme.MutedText,
                Font = ModernTheme.BaseFont,
                TextAlign = ContentAlignment.MiddleLeft
            };
            details.Controls.Add(hint, 0, 2);
        }

        private static TreeNode CreateNode(string text, string tag, params TreeNode[] children)
        {
            var node = new TreeNode(text) { Tag = tag };
            node.Nodes.AddRange(children);
            return node;
        }

        private static TreeNode? FindNodeByTag(TreeNodeCollection nodes, string tag)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Tag?.ToString() == tag) return node;
                var child = FindNodeByTag(node.Nodes, tag);
                if (child != null) return child;
            }
            return null;
        }

        private void NavigationTree_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            var tag = e.Node?.Tag?.ToString() ?? "all";
            if (tag.EndsWith("-root", StringComparison.OrdinalIgnoreCase))
            {
                _navigationTree!.SelectedNode = FindNodeByTag(_navigationTree.Nodes, tag == "documents-root" ? "documents" : "all");
                return;
            }

            _selectedCategory = tag;
            ApplyFilter();
        }

        private void FileManagerForm_Resize(object? sender, EventArgs e) => ApplyResponsiveLayout();

        private void ApplyResponsiveLayout()
        {
            var compact = ClientSize.Width < 1040;
            var veryCompact = ClientSize.Width < 900;

            if (_rootLayout != null)
                _rootLayout.ColumnStyles[0].Width = veryCompact ? 185 : compact ? 215 : 245;

            if (_mainLayout != null)
            {
                _mainLayout.ColumnStyles[1].Width = compact ? 0 : 310;
                if (_detailsCard != null) _detailsCard.Visible = !compact;
            }

            if (_actionPanel != null)
            {
                _actionPanel.WrapContents = compact;
                _actionPanel.AutoScroll = compact;
            }

            dgvFiles.AutoSizeColumnsMode = compact
                ? DataGridViewAutoSizeColumnsMode.DisplayedCells
                : DataGridViewAutoSizeColumnsMode.Fill;
            dgvFiles.ScrollBars = ScrollBars.Both;
        }

        private void SetupGrid()
        {
            dgvFiles.AutoGenerateColumns = false;
            dgvFiles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFiles.MultiSelect = false;
            dgvFiles.ReadOnly = true;
            dgvFiles.AllowUserToAddRows = false;
            dgvFiles.AllowUserToDeleteRows = false;
            dgvFiles.RowHeadersVisible = false;
            dgvFiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvFiles.Columns.Clear();
            dgvFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Icon", HeaderText = "", MinimumWidth = 42, FillWeight = 25 });
            dgvFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name", MinimumWidth = 230, FillWeight = 185 });
            dgvFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "Typ", MinimumWidth = 95, FillWeight = 65 });
            dgvFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "SizeKB", HeaderText = "Größe", MinimumWidth = 90, FillWeight = 70 });
            dgvFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Artikel", HeaderText = "Artikel", MinimumWidth = 100, FillWeight = 80 });
            dgvFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "Datum", MinimumWidth = 145, FillWeight = 110 });
            dgvFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Source", HeaderText = "Quelle", MinimumWidth = 110, FillWeight = 80 });
            dgvFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Kind", HeaderText = "Kind", Visible = false });
            dgvFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Path", HeaderText = "Pfad", Visible = false });
            dgvFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "DocumentId", HeaderText = "DocumentId", Visible = false });
        }

        private void LoadEntries()
        {
            try
            {
                AppPaths.EnsureAll();
                _allEntries.Clear();
                AddFilesFromDirectory(AppPaths.ExportDirectory, "Export");
                AddFilesFromDirectory(AppPaths.BackupDirectory, "Backup");

                // Dokumente werden ausschließlich über die Dokumentenverwaltung geladen.
                // So tauchen importierte/scannte Dateien nicht doppelt als Datei + Dokument auf.
                foreach (var document in _documentService.GetAll())
                {
                    _allEntries.Add(FileEntry.FromDocument(document));
                }

                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden:\n{ex.Message}", "Dateimanager", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddFilesFromDirectory(string path, string source, bool recursive = false)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var file in Directory.GetFiles(path, "*.*", option))
            {
                _allEntries.Add(FileEntry.FromFile(new FileInfo(file), source));
            }
        }

        private void ApplyFilter()
        {
            var search = txtSearch.Text.Trim().ToLowerInvariant();
            var filtered = _allEntries
                .Where(e => MatchesCategory(e, _selectedCategory))
                .Where(e => string.IsNullOrWhiteSpace(search) || e.SearchText.Contains(search))
                .OrderByDescending(e => e.Date)
                .ToList();

            dgvFiles.Rows.Clear();
            foreach (var e in filtered)
            {
                dgvFiles.Rows.Add(
                    e.Icon,
                    e.DisplayName,
                    e.TypeLabel,
                    e.SizeLabel,
                    e.Artikelnummer ?? string.Empty,
                    e.Date.ToString("dd.MM.yyyy HH:mm"),
                    e.Source,
                    e.Kind,
                    e.Path,
                    e.DocumentId?.ToString() ?? string.Empty);
            }

            lblStatus.Text = $"{GetCategoryName(_selectedCategory)} | {_allEntries.Count} gesamt | {filtered.Count} angezeigt";
            if (dgvFiles.Rows.Count > 0) dgvFiles.Rows[0].Selected = true;
            UpdateDetailsPanel();
        }

        private static bool MatchesCategory(FileEntry entry, string category)
        {
            var ext = Path.GetExtension(entry.Path).ToLowerInvariant();
            var categoryName = entry.Category.ToLowerInvariant();

            return category switch
            {
                "csv" => entry.Kind == "file" && ext == ".csv",
                "pdf" => entry.Kind == "file" && ext == ".pdf",
                "backup" => ext == ".zip" || ext == ".bak" || categoryName.Contains("backup") || entry.DisplayName.ToLowerInvariant().Contains("backup"),
                "documents" => entry.Kind == "document",
                "doc-scan" => entry.Kind == "document" && entry.Category.Contains("scan", StringComparison.OrdinalIgnoreCase),
                "doc-invoice" => entry.Kind == "document" && entry.Category.Contains("rechnung", StringComparison.OrdinalIgnoreCase),
                "doc-delivery" => entry.Kind == "document" && entry.Category.Contains("lieferschein", StringComparison.OrdinalIgnoreCase),
                "doc-photo" => entry.Kind == "document" && (entry.Category.Contains("foto", StringComparison.OrdinalIgnoreCase) || IsImage(entry.Path)),
                _ => true
            };
        }

        private static bool IsImage(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext is ".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif";
        }

        private static string GetCategoryName(string category) => category switch
        {
            "csv" => "CSV-Exporte",
            "pdf" => "PDF-Exporte",
            "backup" => "Backups",
            "documents" => "Dokumente",
            "doc-scan" => "Scans",
            "doc-invoice" => "Rechnungen",
            "doc-delivery" => "Lieferscheine",
            "doc-photo" => "Fotos",
            _ => "Alle Dateien"
        };

        private void UpdateDetailsPanel()
        {
            if (_detailsTitle == null || _detailsInfo == null) return;
            var entry = GetSelectedEntry(false);
            if (entry == null)
            {
                _detailsTitle.Text = "Details";
                _detailsInfo.Text = "Keine Auswahl.";
                return;
            }

            _detailsTitle.Text = entry.DisplayName;
            _detailsInfo.Text =
                $"Typ: {entry.TypeLabel}\n" +
                $"Kategorie: {entry.Category}\n" +
                $"Größe: {entry.SizeLabel}\n" +
                $"Datum: {entry.Date:dd.MM.yyyy HH:mm}\n" +
                $"Quelle: {entry.Source}\n" +
                $"Artikel: {(string.IsNullOrWhiteSpace(entry.Artikelnummer) ? "-" : entry.Artikelnummer)}\n\n" +
                $"Pfad:\n{entry.Path}";
        }

        private void txtSearch_TextChanged(object sender, EventArgs e) => ApplyFilter();
        private void btnOpen_Click(object sender, EventArgs e) => OpenSelectedEntry();
        private void btnRefresh_Click(object sender, EventArgs e) => LoadEntries();

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var entry = GetSelectedEntry();
            if (entry == null) return;

            var result = MessageBox.Show("Eintrag wirklich löschen?", "Bestätigung", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;

            try
            {
                if (entry.DocumentId.HasValue)
                {
                    var document = _documentService.GetAll().FirstOrDefault(d => d.Id == entry.DocumentId.Value);
                    if (document != null)
                        _documentService.Delete(document, deletePhysicalFile: true);
                }
                else
                {
                    _fileService.DeleteFile(entry.Path);
                }
                LoadEntries();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Löschen:\n{ex.Message}", "Dateimanager", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Öffnen", null, (_, _) => OpenSelectedEntry());
            menu.Items.Add("CSV importieren", null, (_, _) => ImportSelectedCsv());
            menu.Items.Add("Dokument importieren", null, (_, _) => ImportExternalFiles());
            menu.Items.Add("Artikel zuordnen", null, (_, _) => AssignSelectedDocumentToArticle());
            menu.Items.Add("Umbenennen", null, (_, _) => RenameSelectedFile());
            menu.Items.Add("Löschen", null, (s, e) => btnDelete_Click(s, e));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Im Explorer anzeigen", null, (_, _) => ShowInExplorer());
            dgvFiles.ContextMenuStrip = menu;
        }

        private void SetupDragDrop()
        {
            AllowDrop = true;
            dgvFiles.AllowDrop = true;
            DragEnter += FileManagerForm_DragEnter;
            DragDrop += FileManagerForm_DragDrop;
            dgvFiles.DragEnter += FileManagerForm_DragEnter;
            dgvFiles.DragDrop += FileManagerForm_DragDrop;
        }

        private void FileManagerForm_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                e.Effect = DragDropEffects.Copy;
        }

        private void FileManagerForm_DragDrop(object? sender, DragEventArgs e)
        {
            var files = e.Data?.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length == 0) return;
            ImportFilesAsDocuments(files);
        }

        private void ImportExternalFiles()
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Dateien als Dokument importieren",
                Multiselect = true,
                Filter = "Alle unterstützten Dateien|*.pdf;*.jpg;*.jpeg;*.png;*.bmp;*.csv;*.txt;*.doc;*.docx;*.xls;*.xlsx|Alle Dateien|*.*"
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
                ImportFilesAsDocuments(dialog.FileNames);
        }

        private void ImportFilesAsDocuments(IEnumerable<string> files)
        {
            if (!_canModifyDocuments)
            {
                MessageBox.Show("Du hast keine Berechtigung zum Importieren von Dokumenten.", "Dateimanager", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var count = 0;
                foreach (var file in files.Where(File.Exists))
                {
                    var category = Path.GetExtension(file).Equals(".pdf", StringComparison.OrdinalIgnoreCase) ? "Dokument" : IsImage(file) ? "Foto" : "Allgemein";
                    _documentService.ImportFile(file, Path.GetFileNameWithoutExtension(file), category, null, _currentUser);
                    count++;
                }
                LoadEntries();
                lblStatus.Text = $"{count} Datei(en) als Dokument importiert";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Importieren:\n{ex.Message}", "Dateimanager", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowInExplorer()
        {
            var entry = GetSelectedEntry();
            if (entry == null) return;
            if (File.Exists(entry.Path))
                Process.Start("explorer.exe", $"/select,\"{entry.Path}\"");
        }

        private void RenameSelectedFile()
        {
            var entry = GetSelectedEntry();
            if (entry == null || !File.Exists(entry.Path)) return;
            if (entry.DocumentId.HasValue)
            {
                MessageBox.Show("Dokumenteinträge werden über die Dokumentenverwaltung umbenannt.", "Dateimanager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var newName = PromptDialog.Show("Neuer Dateiname", Path.GetFileName(entry.Path));
            if (string.IsNullOrWhiteSpace(newName)) return;

            try
            {
                var target = Path.Combine(Path.GetDirectoryName(entry.Path)!, newName);
                if (!Path.HasExtension(target)) target += Path.GetExtension(entry.Path);
                File.Move(entry.Path, target);
                LoadEntries();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Umbenennen:\n{ex.Message}", "Dateimanager", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AssignSelectedDocumentToArticle()
        {
            var entry = GetSelectedEntry();
            if (entry == null || !entry.DocumentId.HasValue) return;
            if (!_canModifyDocuments || _artikelService == null)
            {
                MessageBox.Show("Artikelzuordnung ist hier nicht verfügbar.", "Dateimanager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var document = _documentService.GetAll().FirstOrDefault(d => d.Id == entry.DocumentId.Value);
            if (document == null) return;

            var choices = _artikelService.GetAlle()
                .OrderBy(a => a.Artikelnummer)
                .Select(a => $"{a.Artikelnummer} – {a.Bezeichnung}")
                .ToList();
            choices.Insert(0, "(keinem Artikel zuordnen)");

            using var dialog = new SelectionDialog("Artikel zuordnen", "Artikel auswählen", choices);
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            var selected = dialog.SelectedValue;
            document.Artikelnummer = selected.StartsWith("(") ? null : selected.Split('–')[0].Trim();
            _documentService.UpdateDocument(document);
            LoadEntries();
        }

        private void OpenSelectedEntry()
        {
            var entry = GetSelectedEntry();
            if (entry == null) return;

            if (Path.GetExtension(entry.Path).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                var result = MessageBox.Show(
                    "CSV-Datei importieren?\n\nJa = In die Artikelliste importieren\nNein = Datei normal öffnen",
                    "CSV-Datei",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Cancel) return;
                if (result == DialogResult.Yes)
                {
                    ImportSelectedCsv();
                    return;
                }
            }

            try
            {
                _fileService.OpenFile(entry.Path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Öffnen:\n{ex.Message}", "Dateimanager", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImportSelectedCsv()
        {
            var entry = GetSelectedEntry();
            if (entry == null || !Path.GetExtension(entry.Path).Equals(".csv", StringComparison.OrdinalIgnoreCase)) return;

            var mainForm = Application.OpenForms.OfType<ArtikelForm>().FirstOrDefault();
            if (mainForm == null)
            {
                MessageBox.Show("Die CSV kann nur importiert werden, wenn das Hauptfenster geöffnet ist.", "Dateimanager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            mainForm.LoadCsvFromFileManager(entry.Path);
            Close();
        }

        private FileEntry? GetSelectedEntry(bool showMessage = true)
        {
            if (dgvFiles.CurrentRow == null)
            {
                if (showMessage) MessageBox.Show("Bitte zuerst einen Eintrag auswählen.");
                return null;
            }

            var path = dgvFiles.CurrentRow.Cells["Path"].Value?.ToString() ?? string.Empty;
            var docIdText = dgvFiles.CurrentRow.Cells["DocumentId"].Value?.ToString();
            Guid? docId = Guid.TryParse(docIdText, out var parsed) ? parsed : null;

            return _allEntries.FirstOrDefault(e =>
                string.Equals(e.Path, path, StringComparison.OrdinalIgnoreCase) &&
                ((!docId.HasValue && !e.DocumentId.HasValue) || e.DocumentId == docId));
        }

        private void DgvFiles_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) OpenSelectedEntry();
            if (e.KeyCode == Keys.Delete) btnDelete_Click(sender, e);
            if (e.Control && e.KeyCode == Keys.R) RenameSelectedFile();
        }

        private sealed class FileEntry
        {
            public string Kind { get; set; } = "file";
            public Guid? DocumentId { get; set; }
            public string Path { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string TypeLabel { get; set; } = string.Empty;
            public string Source { get; set; } = string.Empty;
            public string? Artikelnummer { get; set; }
            public long SizeBytes { get; set; }
            public DateTime Date { get; set; }
            public string Icon => Kind == "document" ? "🧾" : TypeLabel switch { "CSV" => "📄", "PDF" => "📕", "Backup" or "ZIP" => "🗄", _ => "📁" };
            public string SizeLabel => SizeBytes <= 0 ? "-" : SizeBytes < 1024 * 1024 ? $"{Math.Max(1, SizeBytes / 1024)} KB" : $"{SizeBytes / 1024d / 1024d:0.0} MB";
            public string SearchText => $"{DisplayName} {Category} {TypeLabel} {Source} {Artikelnummer}".ToLowerInvariant();

            public static FileEntry FromFile(FileInfo file, string source)
            {
                var ext = file.Extension.ToLowerInvariant();
                return new FileEntry
                {
                    Kind = "file",
                    Path = file.FullName,
                    DisplayName = file.Name,
                    Category = source,
                    TypeLabel = ext switch
                    {
                        ".csv" => "CSV",
                        ".pdf" => "PDF",
                        ".zip" => "ZIP",
                        ".bak" => "Backup",
                        _ => string.IsNullOrWhiteSpace(ext) ? "Datei" : ext.Trim('.').ToUpperInvariant()
                    },
                    Source = source,
                    SizeBytes = file.Length,
                    Date = file.LastWriteTime
                };
            }

            public static FileEntry FromDocument(DocumentRecord document)
            {
                var info = File.Exists(document.DateiPfad) ? new FileInfo(document.DateiPfad) : null;
                return new FileEntry
                {
                    Kind = "document",
                    DocumentId = document.Id,
                    Path = document.DateiPfad,
                    DisplayName = document.Titel,
                    Category = document.Kategorie,
                    TypeLabel = string.IsNullOrWhiteSpace(document.Kategorie) ? "Dokument" : document.Kategorie,
                    Source = document.Quelle,
                    Artikelnummer = document.Artikelnummer,
                    SizeBytes = info?.Length ?? 0,
                    Date = document.ErstelltAm
                };
            }
        }
    }

    internal sealed class PromptDialog : Form
    {
        private readonly TextBox _textBox = new();
        public string Result => _textBox.Text.Trim();

        private PromptDialog(string title, string value)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(420, 145);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(14), RowCount = 2 };
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            Controls.Add(layout);

            _textBox.Dock = DockStyle.Fill;
            _textBox.Text = value;
            layout.Controls.Add(_textBox, 0, 0);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 90 };
            var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, Width = 100 };
            buttons.Controls.Add(ok);
            buttons.Controls.Add(cancel);
            layout.Controls.Add(buttons, 0, 1);
            AcceptButton = ok;
            CancelButton = cancel;
        }

        public static string? Show(string title, string value)
        {
            using var dialog = new PromptDialog(title, value);
            return dialog.ShowDialog() == DialogResult.OK ? dialog.Result : null;
        }
    }

    internal sealed class SelectionDialog : Form
    {
        private readonly ComboBox _combo = new();
        public string SelectedValue => _combo.Text;

        public SelectionDialog(string title, string labelText, IEnumerable<string> values)
        {
            Text = title;
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(520, 170);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(14), RowCount = 3 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            Controls.Add(layout);

            layout.Controls.Add(new Label { Text = labelText, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            _combo.Dock = DockStyle.Top;
            _combo.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (var value in values) _combo.Items.Add(value);
            if (_combo.Items.Count > 0) _combo.SelectedIndex = 0;
            layout.Controls.Add(_combo, 0, 1);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 90 };
            var cancel = new Button { Text = "Abbrechen", DialogResult = DialogResult.Cancel, Width = 100 };
            buttons.Controls.Add(ok);
            buttons.Controls.Add(cancel);
            layout.Controls.Add(buttons, 0, 2);
            AcceptButton = ok;
            CancelButton = cancel;
        }
    }
}
