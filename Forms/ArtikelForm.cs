using InventurApp.Models;
using InventurApp.Services;
using InventurApp.UI;
using System.ComponentModel;

namespace InventurApp.Forms
{
    public partial class ArtikelForm : Form
    {
        private readonly ArtikelService _service = new();
        private readonly CsvService _csvService = new();
        private readonly PdfExportService _pdfExportService = new();
        private readonly BackupService _backupService = new();
        private readonly SettingsService _settingsService = new();
        private readonly TwainService _twainService = new();
        private readonly DocumentService _documentService = new();
        private readonly BenutzerService? _benutzerService;
        private AppSettings _settings = new();

        private BindingList<string> Bezeichnungen => _service.GetBezeichnungen();
        private readonly BindingSource _artikelBindingSource = new();
        private readonly BindingList<Artikel> _artikelView = new();

        private bool _columnsSetup = false;
        private bool _isRefreshingGrid = false;
        private bool _suppressFilterEvents = false;
        private bool _layoutReady = false;
        private string? _sortField;
        private bool _sortDescending = false;


        private Panel? _sidebar;
        private Panel? _contentPanel;
        private Panel? _headerPanel;
        private Label? _brandLabel;
        private Label? _titleLabel;
        private Panel? _dashboardPanel;
        private TableLayoutPanel? _metricsLayout;
        private readonly List<Control> _metricCards = new();
        private Panel? _gridCard;
        private FlowLayoutPanel? _toolbar;
        private TextBox? _txtSearch;
        private ComboBox? _cmbLagerortFilter;
        private Label? _lblMetricArtikel;
        private Label? _lblMetricLagerorte;
        private Label? _lblMetricMenge;
        private Label? _lblMetricLetzterExport;
        private TextBox? _txtBarcode;
        private SplitContainer? _mainSplit;
        private Panel? _detailCard;
        private Label? _detailTitle;
        private Label? _detailSubtitle;
        private FlowLayoutPanel? _detailFields;
        private Label? _detailEmptyLabel;

        public ArtikelForm(BenutzerService? benutzerService = null)
        {
            _benutzerService = benutzerService;
            _settings = _settingsService.Load();
            ModernTheme.SetDarkMode(_settings.DarkMode);
            InitializeComponent();

            ApplyModernDesign();
            SetupGrid();

            dgvArtikel.DataError += DgvArtikel_DataError;
            dgvArtikel.CellEndEdit += DgvArtikel_CellEndEdit;
            dgvArtikel.CellValidating += DgvArtikel_CellValidating;
            dgvArtikel.CellFormatting += DgvArtikel_CellFormatting;
            dgvArtikel.SelectionChanged += DgvArtikel_SelectionChanged;
            dgvArtikel.ColumnHeaderMouseClick += DgvArtikel_ColumnHeaderMouseClick;

            LoadData();
        }

        private void ApplyModernDesign()
        {
            ModernTheme.ApplyForm(this);
            Text = "Alex Inventur – Dashboard";
            if (ClientSize.Width < 900 || ClientSize.Height < 620)
                ClientSize = new Size(1180, 760);
            MinimumSize = new Size(760, 560);

            Controls.Clear();

            _sidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = ModernTheme.Sidebar,
                Padding = new Padding(18, 20, 18, 18)
            };

            _brandLabel = new Label
            {
                Text = "Alex\nInventur",
                Dock = DockStyle.Top,
                Height = 76,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var nav = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = ModernTheme.Sidebar,
                Padding = new Padding(0, 12, 0, 0)
            };

            var btnDashboard = CreateNavButton("📊  Dashboard");
            var btnArtikelNav = CreateNavButton("📦  Artikel");
            var btnImportNav = CreateNavButton("⬇  Import");
            var btnExportNav = CreateNavButton("⬆  Export");
            var btnFilesNav = CreateNavButton("📁  Dateien");
            var btnStatsNav = CreateNavButton("📈  Statistik");
            var btnSettingsNav = CreateNavButton("⚙  Einstellungen");
            var btnScanNav = CreateNavButton("🖨  Scan");
            var btnInfoNav = CreateNavButton("ℹ  Info");
            var btnDarkNav = CreateNavButton(_settings.DarkMode ? "☀  Light Mode" : "🌙  Dark Mode");

            btnDashboard.Click += (_, _) => dgvArtikel.Focus();
            btnArtikelNav.Click += (_, _) => btnNeu_Click(btnNeu, EventArgs.Empty);
            btnImportNav.Click += (_, _) => btnImportCsv_Click(btnImportCsv, EventArgs.Empty);
            btnExportNav.Click += (_, _) => btnExportCsv_Click(btnExportCsv, EventArgs.Empty);
            btnFilesNav.Click += (_, _) => btnDateimanager_Click(btnDateimanager, EventArgs.Empty);
            btnStatsNav.Click += (_, _) => ShowStatistics();
            btnSettingsNav.Click += (_, _) => ShowSettings();
            btnScanNav.Click += (_, _) => StartTwainScan();
            btnInfoNav.Click += (_, _) => ShowAbout();
            btnDarkNav.Click += (_, _) => ToggleDarkMode();

            nav.Controls.AddRange(new Control[] { btnDashboard, btnArtikelNav, btnImportNav, btnExportNav, btnFilesNav, btnStatsNav, btnScanNav, btnSettingsNav, btnInfoNav, btnDarkNav });

            var sidebarFooter = new Label
            {
                Text = $"{(_benutzerService?.CurrentUser?.Username ?? "Benutzer")}\n{(_benutzerService?.CurrentUser?.Role ?? "Benutzer")} · SQLite",
                Dock = DockStyle.Bottom,
                Height = 58,
                ForeColor = Color.FromArgb(156, 163, 175),
                BackColor = Color.Transparent,
                Font = ModernTheme.SubtitleFont
            };

            _sidebar.Controls.Add(sidebarFooter);
            _sidebar.Controls.Add(nav);
            _sidebar.Controls.Add(_brandLabel);
            Controls.Add(_sidebar);

            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ModernTheme.Background,
                Padding = new Padding(28, 24, 28, 18)
            };
            Controls.Add(_contentPanel);
            _contentPanel.BringToFront();

            _headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 92,
                BackColor = ModernTheme.Background
            };

            _titleLabel = new Label
            {
                Text = "Dashboard\nArtikelbestand, Schnellaktionen und CSV-Verwaltung",
                Dock = DockStyle.Left,
                Width = 560,
                ForeColor = ModernTheme.Text,
                BackColor = Color.Transparent,
                Font = ModernTheme.TitleFont
            };

            _txtSearch = new TextBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(590, 18),
                Size = new Size(250, 28),
                PlaceholderText = "Artikel suchen..."
            };
            _txtSearch.TextChanged += (_, _) => ApplyArticleFilterSafe();
            ModernTheme.ApplyInput(_txtSearch);

            _cmbLagerortFilter = new ComboBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(850, 18),
                Size = new Size(190, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbLagerortFilter.SelectedIndexChanged += (_, _) => { if (!_suppressFilterEvents) ApplyArticleFilterSafe(); };
            ModernTheme.ApplyInput(_cmbLagerortFilter);

            _txtBarcode = new TextBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(590, 50),
                Size = new Size(450, 28),
                PlaceholderText = _settings.ScannerEnabled ? GetScannerPlaceholder() : "Scanner ist deaktiviert – in Einstellungen aktivieren",
                Enabled = _settings.ScannerEnabled,
                Visible = _settings.ScannerEnabled
            };
            _txtBarcode.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    FocusBarcodeResult();
                    e.SuppressKeyPress = true;
                }
            };
            ModernTheme.ApplyInput(_txtBarcode);

            _headerPanel.Resize += (_, _) => ApplyResponsiveLayout();

            _headerPanel.Controls.Add(_titleLabel);
            _headerPanel.Controls.Add(_txtSearch);
            _headerPanel.Controls.Add(_cmbLagerortFilter);
            _headerPanel.Controls.Add(_txtBarcode);
            _contentPanel.Controls.Add(_headerPanel);

            _dashboardPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 104,
                BackColor = ModernTheme.Background,
                Padding = new Padding(0, 0, 0, 14)
            };
            _contentPanel.Controls.Add(_dashboardPanel);
            _dashboardPanel.BringToFront();

            _metricCards.Clear();
            _metricsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = ModernTheme.Background
            };

            _lblMetricArtikel = CreateMetricCard(_metricsLayout, 0, "Artikel", "0", "Aktiver Datenbestand");
            _lblMetricLagerorte = CreateMetricCard(_metricsLayout, 1, "Lagerorte", "0", "Eindeutige Standorte");
            _lblMetricMenge = CreateMetricCard(_metricsLayout, 2, "Soll-Menge", "0", "Gesamtsumme");
            _lblMetricLetzterExport = CreateMetricCard(_metricsLayout, 3, "Status", "Bereit", "Letzte Aktion");

            _dashboardPanel.Controls.Add(_metricsLayout);

            _toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 54,
                BackColor = ModernTheme.Background,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(0, 3, 0, 9)
            };

            btnNeu.Text = "＋ Neuer Artikel";
            var btnEdit = new Button { Text = "✏ Bearbeiten" };
            var btnDetails = new Button { Text = "🔎 Details" };
            btnLöschen.Text = "🗑 Löschen";
            btnImportCsv.Text = "⬇ CSV importieren";
            btnExportCsv.Text = "⬆ CSV exportieren";
            btnDateimanager.Text = "📁 Dateien";
            var btnPdf = new Button { Text = "📄 PDF" };
            var btnBackup = new Button { Text = "💾 Backup" };
            var btnStats = new Button { Text = "📈 Statistik" };
            var btnSettings = new Button { Text = "⚙ Einstellungen" };
            var btnScan = new Button { Text = "🖨 Scan" };
            btnEdit.Click += (_, _) => EditSelectedArtikel();
            btnDetails.Click += (_, _) => ShowDeviceDetailWindow();
            btnPdf.Click += (_, _) => ExportPdf();
            btnBackup.Click += (_, _) => CreateBackup();
            btnStats.Click += (_, _) => ShowStatistics();
            btnSettings.Click += (_, _) => ShowSettings();
            btnScan.Click += (_, _) => StartTwainScan();

            foreach (var button in new[] { btnNeu, btnEdit, btnDetails, btnLöschen, btnImportCsv, btnExportCsv, btnDateimanager, btnPdf, btnBackup, btnStats, btnScan, btnSettings })
            {
                button.Tag = button.Text;
                button.Width = button == btnNeu ? 142 : button == btnDateimanager ? 130 : 122;
                button.Height = 38;
                button.Margin = new Padding(0, 0, 10, 0);
                _toolbar.Controls.Add(button);
            }

            ModernTheme.ApplyPrimaryButton(btnNeu);
            ModernTheme.ApplySecondaryButton(btnEdit);
            ModernTheme.ApplySecondaryButton(btnDetails);
            ModernTheme.ApplyDangerButton(btnLöschen);
            ModernTheme.ApplySecondaryButton(btnImportCsv);
            ModernTheme.ApplySecondaryButton(btnExportCsv);
            ModernTheme.ApplySecondaryButton(btnDateimanager);
            ModernTheme.ApplySecondaryButton(btnPdf);
            ModernTheme.ApplySecondaryButton(btnBackup);
            ModernTheme.ApplySecondaryButton(btnStats);
            ModernTheme.ApplySecondaryButton(btnScan);
            ModernTheme.ApplySecondaryButton(btnSettings);

            ApplyPermissionState(btnImportNav, btnExportNav, btnStatsNav, btnScanNav, btnSettingsNav, btnNeu, btnLöschen, btnImportCsv, btnExportCsv, btnPdf, btnBackup, btnStats, btnScan, btnSettings);
            btnEdit.Enabled = UserPermissionService.CanEditArticle(_benutzerService?.CurrentUser);
            btnDetails.Enabled = true;

            _contentPanel.Controls.Add(_toolbar);
            _toolbar.BringToFront();

            _mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 8,
                BackColor = ModernTheme.Background,
                // MinSizes werden erst im Responsive-Layout gesetzt.
                // Beim Start kann der SplitContainer noch sehr schmal sein; feste MinSizes
                // lösen sonst InvalidOperationException bei SplitterDistance/Panel2MinSize aus.
                Panel1MinSize = 0,
                Panel2MinSize = 0
            };
            _contentPanel.Controls.Add(_mainSplit);
            _mainSplit.BringToFront();

            _gridCard = ModernTheme.CreateCardPanel();
            _gridCard.Dock = DockStyle.Fill;
            _gridCard.Padding = new Padding(14);
            _mainSplit.Panel1.Controls.Add(_gridCard);

            dgvArtikel.Dock = DockStyle.Fill;
            dgvArtikel.ReadOnly = !UserPermissionService.CanEditArticle(_benutzerService?.CurrentUser);
            ModernTheme.ApplyGrid(dgvArtikel);
            _gridCard.Controls.Add(dgvArtikel);

            CreateDeviceDetailPanel();

            lblStatus.Dock = DockStyle.Bottom;
            lblStatus.Height = 38;
            ModernTheme.ApplyStatus(lblStatus);
            _contentPanel.Controls.Add(lblStatus);
            lblStatus.BringToFront();

            Resize -= ArtikelForm_Resize;
            Resize += ArtikelForm_Resize;
            Shown -= ArtikelForm_Shown;
            Shown += ArtikelForm_Shown;
        }

        private void ArtikelForm_Resize(object? sender, EventArgs e)
        {
            if (!_layoutReady || !IsHandleCreated)
                return;

            ApplyResponsiveLayout();
        }

        private void ArtikelForm_Shown(object? sender, EventArgs e)
        {
            _layoutReady = true;
            ApplyResponsiveLayout();
        }

        private void CreateDeviceDetailPanel()
        {
            if (_mainSplit == null)
                return;

            _detailCard = ModernTheme.CreateCardPanel();
            _detailCard.Dock = DockStyle.Fill;
            _detailCard.Padding = new Padding(18);
            _mainSplit.Panel2.Controls.Add(_detailCard);

            _detailTitle = new Label
            {
                Text = "Gerätedetails",
                Dock = DockStyle.Top,
                Height = 34,
                ForeColor = ModernTheme.Text,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold),
                AutoEllipsis = true
            };

            _detailSubtitle = new Label
            {
                Text = "Wähle einen Artikel aus.",
                Dock = DockStyle.Top,
                Height = 44,
                ForeColor = ModernTheme.MutedText,
                BackColor = Color.Transparent,
                Font = ModernTheme.SubtitleFont,
                AutoEllipsis = true
            };

            _detailFields = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 8, 0, 0)
            };

            _detailEmptyLabel = new Label
            {
                Text = "Keine Gerätedetails vorhanden.",
                Dock = DockStyle.Top,
                Height = 70,
                ForeColor = ModernTheme.MutedText,
                BackColor = Color.Transparent,
                Font = ModernTheme.SubtitleFont,
                TextAlign = ContentAlignment.MiddleCenter
            };

            _detailCard.Controls.Add(_detailFields);
            _detailCard.Controls.Add(_detailSubtitle);
            _detailCard.Controls.Add(_detailTitle);
            UpdateDeviceDetailPanel(null);
        }

        private void DgvArtikel_SelectionChanged(object? sender, EventArgs e)
        {
            if (_isRefreshingGrid)
                return;

            UpdateDeviceDetailPanel(GetSelectedArtikel());
        }

        private Artikel? GetSelectedArtikel()
        {
            return dgvArtikel.CurrentRow?.DataBoundItem as Artikel;
        }

        private void UpdateDeviceDetailPanel(Artikel? artikel)
        {
            if (_detailFields == null || _detailTitle == null || _detailSubtitle == null)
                return;

            _detailFields.SuspendLayout();
            try
            {
                _detailFields.Controls.Clear();

                if (artikel == null)
                {
                    _detailTitle.Text = "Gerätedetails";
                    _detailSubtitle.Text = "Wähle ein Gerät oder einen Artikel aus.";
                    AddDetailEmpty("Keine Auswahl vorhanden.");
                    return;
                }

                _detailTitle.Text = string.IsNullOrWhiteSpace(artikel.Artikelnummer)
                    ? "Gerätedetails"
                    : artikel.Artikelnummer;
                _detailSubtitle.Text = string.Join("  ·  ", new[] { artikel.Bezeichnung, artikel.Lagerort }
                    .Where(v => !string.IsNullOrWhiteSpace(v)));

                AddDetailSection("Basisdaten", GetBaseDetailItems(artikel));

                var customFields = artikel.CustomFields ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                AddGroupedCustomFields(customFields);

                if (_detailFields.Controls.Count == 0)
                    AddDetailEmpty("Keine Gerätedetails vorhanden.");
            }
            finally
            {
                _detailFields.ResumeLayout();
            }
        }

        private IEnumerable<KeyValuePair<string, string>> GetBaseDetailItems(Artikel artikel)
        {
            yield return new KeyValuePair<string, string>(_settings.GetFieldName("Artikelnummer"), artikel.Artikelnummer ?? string.Empty);
            yield return new KeyValuePair<string, string>(_settings.GetFieldName("Bezeichnung"), artikel.Bezeichnung ?? string.Empty);
            yield return new KeyValuePair<string, string>(_settings.GetFieldName("Lagerort"), artikel.Lagerort ?? string.Empty);
            yield return new KeyValuePair<string, string>(_settings.GetFieldName("SollMenge"), artikel.SollMenge.ToString());
        }

        private void AddGroupedCustomFields(Dictionary<string, string> fields)
        {
            if (fields.Count == 0)
                return;

            var nonEmptyFields = fields
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                .OrderBy(kv => GetDetailGroupOrder(kv.Key))
                .ThenBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var group in nonEmptyFields.GroupBy(kv => GetDetailGroupName(kv.Key)))
                AddDetailSection(group.Key, group);
        }

        private static int GetDetailGroupOrder(string fieldName)
        {
            var group = GetDetailGroupName(fieldName);
            return group switch
            {
                "System" => 10,
                "Identifikation" => 20,
                "BIOS" => 30,
                "CPU" => 40,
                "Arbeitsspeicher" => 50,
                "Datenträger" => 60,
                "Netzwerk" => 70,
                "Sonstige Felder" => 99,
                _ => 99
            };
        }

        private static string GetDetailGroupName(string fieldName)
        {
            var name = fieldName.ToLowerInvariant();

            if (name.Contains("system") || name.Contains("hersteller") || name.Contains("modell") || name.Contains("manufacturer") || name.Contains("model"))
                return "System";

            if (name.Contains("serien") || name.Contains("serial") || name.Contains("inventar") || name.Contains("asset") || name.Contains("gerät"))
                return "Identifikation";

            if (name.Contains("bios"))
                return "BIOS";

            if (name.Contains("cpu") || name.Contains("prozessor") || name.Contains("processor"))
                return "CPU";

            if (name.Contains("ram") || name.Contains("speicher") || name.Contains("memory"))
                return "Arbeitsspeicher";

            if (name.Contains("disk") || name.Contains("drive") || name.Contains("hdd") || name.Contains("ssd") || name.Contains("datenträger"))
                return "Datenträger";

            if (name.Contains("nic") || name.Contains("netz") || name.Contains("mac") || name.Contains("ip") || name.Contains("adapter"))
                return "Netzwerk";

            return "Sonstige Felder";
        }

        private void AddDetailSection(string title, IEnumerable<KeyValuePair<string, string>> items)
        {
            if (_detailFields == null)
                return;

            var cleanedItems = items
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                .ToList();

            if (cleanedItems.Count == 0)
                return;

            var sectionLabel = new Label
            {
                Text = title,
                Width = Math.Max(220, _detailFields.ClientSize.Width - 24),
                Height = 28,
                Margin = new Padding(0, _detailFields.Controls.Count == 0 ? 0 : 14, 0, 4),
                ForeColor = ModernTheme.Text,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _detailFields.Controls.Add(sectionLabel);

            foreach (var item in cleanedItems)
                _detailFields.Controls.Add(CreateDetailRow(item.Key, item.Value));
        }

        private Control CreateDetailRow(string label, string value)
        {
            var row = new Panel
            {
                Width = Math.Max(220, (_detailFields?.ClientSize.Width ?? 300) - 24),
                Height = 48,
                Margin = new Padding(0, 0, 0, 6),
                BackColor = ModernTheme.SurfaceAlt,
                Padding = new Padding(10, 5, 10, 5)
            };

            var lblKey = new Label
            {
                Text = label,
                Dock = DockStyle.Top,
                Height = 18,
                ForeColor = ModernTheme.MutedText,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.5F),
                AutoEllipsis = true
            };

            var lblValue = new Label
            {
                Text = value,
                Dock = DockStyle.Fill,
                ForeColor = ModernTheme.Text,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9.5F),
                AutoEllipsis = true
            };

            row.Controls.Add(lblValue);
            row.Controls.Add(lblKey);
            return row;
        }

        private void AddDetailEmpty(string text)
        {
            if (_detailFields == null)
                return;

            _detailFields.Controls.Add(new Label
            {
                Text = text,
                Width = Math.Max(220, _detailFields.ClientSize.Width - 24),
                Height = 72,
                ForeColor = ModernTheme.MutedText,
                BackColor = Color.Transparent,
                Font = ModernTheme.SubtitleFont,
                TextAlign = ContentAlignment.MiddleCenter
            });
        }

        private void ApplyResponsiveLayout()
        {
            if (!_layoutReady || !IsHandleCreated || _contentPanel == null || _sidebar == null)
                return;

            var compactSidebar = ClientSize.Width < 1080;
            var narrow = ClientSize.Width < 1100;
            var veryNarrow = ClientSize.Width < 920;

            _sidebar.Width = compactSidebar ? 78 : 220;
            _sidebar.Padding = compactSidebar ? new Padding(10, 18, 10, 14) : new Padding(18, 20, 18, 18);

            if (_brandLabel != null)
            {
                _brandLabel.Text = compactSidebar ? "AI" : "Alex\nInventur";
                _brandLabel.Height = compactSidebar ? 54 : 76;
                _brandLabel.TextAlign = compactSidebar ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;
                _brandLabel.Font = compactSidebar
                    ? new Font("Segoe UI Semibold", 17F, FontStyle.Bold)
                    : new Font("Segoe UI Semibold", 20F, FontStyle.Bold);
            }

            foreach (var button in GetAllControls(_sidebar).OfType<Button>())
            {
                var fullText = button.Tag?.ToString() ?? button.Text;
                button.Text = compactSidebar ? ToCompactLabel(fullText) : fullText;
                button.Width = compactSidebar ? 56 : 184;
                button.TextAlign = compactSidebar ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;
                button.Padding = compactSidebar ? Padding.Empty : new Padding(14, 0, 0, 0);
            }

            _contentPanel.Padding = veryNarrow
                ? new Padding(14, 14, 14, 12)
                : narrow
                    ? new Padding(20, 18, 20, 14)
                    : new Padding(28, 24, 28, 18);

            LayoutHeader(narrow, veryNarrow);
            LayoutMetrics(narrow);
            LayoutToolbar(narrow);

            if (dgvArtikel != null)
            {
                dgvArtikel.RowTemplate.Height = narrow ? 34 : 38;
                dgvArtikel.ColumnHeadersHeight = narrow ? 38 : 42;
            }

            LayoutDeviceDetails(narrow, veryNarrow);
        }

        private void LayoutDeviceDetails(bool narrow, bool veryNarrow)
        {
            if (_mainSplit == null || _mainSplit.IsDisposed)
                return;

            var available = _mainSplit.ClientSize.Width;
            if (available <= 0)
                return;

            var collapseDetails = veryNarrow || available < 820;

            // Wichtig: Vor dem Kollabieren/Einblenden MinSizes dynamisch an die echte Breite anpassen.
            // Sonst kann WinForms beim Start oder bei kleinen Fenstern eine ungültige SplitterDistance auslösen.
            var safePanel1Min = Math.Min(420, Math.Max(0, available - _mainSplit.SplitterWidth - 260));
            var safePanel2Min = collapseDetails ? 0 : Math.Min(260, Math.Max(0, available - _mainSplit.SplitterWidth - safePanel1Min));

            _mainSplit.Panel1MinSize = safePanel1Min;
            _mainSplit.Panel2MinSize = safePanel2Min;
            _mainSplit.Panel2Collapsed = collapseDetails;

            if (!_mainSplit.Panel2Collapsed)
            {
                var preferredDetailWidth = narrow ? 280 : 340;
                var maxDetailWidth = Math.Max(safePanel2Min, available - _mainSplit.SplitterWidth - safePanel1Min);
                var detailWidth = Math.Min(preferredDetailWidth, maxDetailWidth);
                detailWidth = Math.Max(safePanel2Min, detailWidth);

                var minDistance = safePanel1Min;
                var maxDistance = available - _mainSplit.SplitterWidth - safePanel2Min;
                var desiredDistance = available - _mainSplit.SplitterWidth - detailWidth;
                var safeDistance = Math.Max(minDistance, Math.Min(maxDistance, desiredDistance));

                if (maxDistance >= minDistance && _mainSplit.SplitterDistance != safeDistance)
                    _mainSplit.SplitterDistance = safeDistance;
            }

            if (_detailFields != null)
            {
                foreach (Control control in _detailFields.Controls)
                    control.Width = Math.Max(220, _detailFields.ClientSize.Width - 24);
            }
        }

        private void LayoutHeader(bool narrow, bool veryNarrow)
        {
            if (_headerPanel == null || _titleLabel == null || _txtSearch == null || _cmbLagerortFilter == null)
                return;

            _headerPanel.Height = narrow ? (_txtBarcode?.Visible == true ? 150 : 118) : 92;
            _titleLabel.Dock = DockStyle.None;
            _titleLabel.Location = new Point(0, 0);
            _titleLabel.Height = narrow ? 58 : 78;
            _titleLabel.Width = narrow ? _headerPanel.Width : Math.Min(560, Math.Max(360, _headerPanel.Width - 520));
            _titleLabel.Font = narrow ? new Font("Segoe UI Semibold", 17F, FontStyle.Bold) : ModernTheme.TitleFont;

            if (narrow)
            {
                var gap = 10;
                var top = veryNarrow ? 66 : 70;
                var halfWidth = Math.Max(160, (_headerPanel.Width - gap) / 2);

                _txtSearch.Location = new Point(0, top);
                _txtSearch.Size = new Size(halfWidth, 30);
                _cmbLagerortFilter.Location = new Point(halfWidth + gap, top);
                _cmbLagerortFilter.Size = new Size(Math.Max(150, _headerPanel.Width - halfWidth - gap), 30);

                if (_txtBarcode != null)
                {
                    _txtBarcode.Location = new Point(0, top + 38);
                    _txtBarcode.Size = new Size(_headerPanel.Width, 30);
                }
            }
            else
            {
                var filterWidth = 190;
                var searchWidth = 250;
                var gap = 10;
                var filterLeft = Math.Max(650, _headerPanel.Width - filterWidth);
                var searchLeft = Math.Max(390, filterLeft - gap - searchWidth);

                _txtSearch.Location = new Point(searchLeft, 18);
                _txtSearch.Size = new Size(searchWidth, 28);
                _cmbLagerortFilter.Location = new Point(filterLeft, 18);
                _cmbLagerortFilter.Size = new Size(filterWidth, 28);

                if (_txtBarcode != null)
                {
                    _txtBarcode.Location = new Point(searchLeft, 50);
                    _txtBarcode.Size = new Size(Math.Max(250, _cmbLagerortFilter.Right - searchLeft), 28);
                }
            }
        }

        private void LayoutMetrics(bool narrow)
        {
            if (_dashboardPanel == null || _metricsLayout == null || _metricCards.Count < 4)
                return;

            _dashboardPanel.Height = narrow ? 206 : 104;
            _metricsLayout.SuspendLayout();
            _metricsLayout.ColumnStyles.Clear();
            _metricsLayout.RowStyles.Clear();
            _metricsLayout.ColumnCount = narrow ? 2 : 4;
            _metricsLayout.RowCount = narrow ? 2 : 1;

            if (narrow)
            {
                _metricsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                _metricsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                _metricsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
                _metricsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

                for (var i = 0; i < _metricCards.Count; i++)
                {
                    _metricsLayout.SetCellPosition(_metricCards[i], new TableLayoutPanelCellPosition(i % 2, i / 2));
                    _metricCards[i].Margin = new Padding(i % 2 == 0 ? 0 : 8, i < 2 ? 0 : 8, i % 2 == 1 ? 0 : 8, i >= 2 ? 0 : 8);
                }
            }
            else
            {
                for (var i = 0; i < 4; i++)
                    _metricsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
                _metricsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

                for (var i = 0; i < _metricCards.Count; i++)
                {
                    _metricsLayout.SetCellPosition(_metricCards[i], new TableLayoutPanelCellPosition(i, 0));
                    _metricCards[i].Margin = new Padding(i == 0 ? 0 : 8, 0, i == 3 ? 0 : 8, 0);
                }
            }

            _metricsLayout.ResumeLayout();
        }

        private void LayoutToolbar(bool narrow)
        {
            if (_toolbar == null)
                return;

            _toolbar.WrapContents = true;
            _toolbar.AutoScroll = true;
            _toolbar.Height = narrow ? 116 : 62;

            foreach (var button in _toolbar.Controls.OfType<Button>())
            {
                var fullText = button.Tag?.ToString() ?? button.Text;
                button.Text = narrow ? ToCompactLabel(fullText) : fullText;
                button.Width = narrow ? 76 : fullText.Contains("Dateien") ? 130 : fullText.Contains("Neuer") ? 142 : 122;
                button.Height = 38;
            }
        }

        private static IEnumerable<Control> GetAllControls(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;
                foreach (var nested in GetAllControls(child))
                    yield return nested;
            }
        }

        private static string ToCompactLabel(string text)
        {
            var trimmed = text.Trim();
            var splitIndex = trimmed.IndexOf(' ');
            return splitIndex > 0 ? trimmed[..splitIndex] : trimmed;
        }


        private void ApplyPermissionState(
            Button btnImportNav,
            Button btnExportNav,
            Button btnStatsNav,
            Button btnScanNav,
            Button btnSettingsNav,
            Button btnNeuToolbar,
            Button btnDeleteToolbar,
            Button btnImportToolbar,
            Button btnExportToolbar,
            Button btnPdfToolbar,
            Button btnBackupToolbar,
            Button btnStatsToolbar,
            Button btnScanToolbar,
            Button btnSettingsToolbar)
        {
            var user = _benutzerService?.CurrentUser;

            btnNeuToolbar.Enabled = UserPermissionService.CanCreateArticle(user);
            btnDeleteToolbar.Enabled = UserPermissionService.CanDeleteArticle(user);
            btnImportToolbar.Enabled = UserPermissionService.CanImport(user);
            btnExportToolbar.Enabled = UserPermissionService.CanExport(user);
            btnPdfToolbar.Enabled = UserPermissionService.CanExport(user);
            btnBackupToolbar.Enabled = UserPermissionService.CanCreateBackup(user);
            btnStatsToolbar.Enabled = UserPermissionService.CanOpenStatistics(user);
            btnScanToolbar.Enabled = UserPermissionService.CanUseScanner(user) && _settings.ScannerEnabled;
            btnSettingsToolbar.Enabled = UserPermissionService.CanOpenSettings(user);

            btnImportNav.Enabled = UserPermissionService.CanImport(user);
            btnExportNav.Enabled = UserPermissionService.CanExport(user);
            btnStatsNav.Enabled = UserPermissionService.CanOpenStatistics(user);
            btnScanNav.Enabled = UserPermissionService.CanUseScanner(user) && _settings.ScannerEnabled;
            btnSettingsNav.Enabled = UserPermissionService.CanOpenSettings(user);
        }

        private bool RequirePermission(bool allowed, string action)
        {
            if (allowed)
                return true;

            MessageBox.Show(
                $"Dein Benutzerkonto darf diese Aktion nicht ausführen.\n\nAktion: {action}",
                "Keine Berechtigung",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        private Button CreateNavButton(string text)
        {
            var button = new Button
            {
                Text = text,
                Tag = text,
                Width = 184,
                Height = 42,
                Margin = new Padding(0, 0, 0, 9),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(14, 0, 0, 0)
            };
            ModernTheme.ApplySidebarButton(button);
            return button;
        }

        private Label CreateMetricCard(TableLayoutPanel parent, int column, string title, string value, string subtitle)
        {
            var card = ModernTheme.CreateCardPanel();
            card.Margin = new Padding(column == 0 ? 0 : 8, 0, column == 3 ? 0 : 8, 0);
            card.Dock = DockStyle.Fill;

            var titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 22,
                ForeColor = ModernTheme.MutedText,
                BackColor = Color.Transparent,
                Font = ModernTheme.SubtitleFont
            };

            var valueLabel = new Label
            {
                Text = value,
                Dock = DockStyle.Top,
                Height = 34,
                ForeColor = ModernTheme.Text,
                BackColor = Color.Transparent,
                Font = ModernTheme.MetricFont
            };

            var subtitleLabel = new Label
            {
                Text = subtitle,
                Dock = DockStyle.Top,
                Height = 22,
                ForeColor = ModernTheme.MutedText,
                BackColor = Color.Transparent,
                Font = ModernTheme.SubtitleFont
            };

            card.Controls.Add(subtitleLabel);
            card.Controls.Add(valueLabel);
            card.Controls.Add(titleLabel);
            _metricCards.Add(card);
            parent.Controls.Add(card, column, 0);
            return valueLabel;
        }

        // =========================
        // GRID
        // =========================
        private void SetupGrid()
        {
            if (_columnsSetup) return;
            _columnsSetup = true;

            dgvArtikel.AutoGenerateColumns = false;
            dgvArtikel.Columns.Clear();

            dgvArtikel.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvArtikel.MultiSelect = false;
            dgvArtikel.RowHeadersVisible = false;
            dgvArtikel.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvArtikel.ScrollBars = ScrollBars.Both;
            _artikelBindingSource.DataSource = _artikelView;
            dgvArtikel.DataSource = _artikelBindingSource;
            dgvArtikel.AllowUserToOrderColumns = true;

            dgvArtikel.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = _settings.GetFieldName("Artikelnummer"),
                DataPropertyName = "Artikelnummer",
                ReadOnly = false,
                MinimumWidth = 120,
                FillWeight = 90
            });

            dgvArtikel.Columns.Add(new DataGridViewComboBoxColumn
            {
                HeaderText = _settings.GetFieldName("Bezeichnung"),
                DataPropertyName = "Bezeichnung",
                DataSource = Bezeichnungen,
                FlatStyle = FlatStyle.Flat,
                MinimumWidth = 180,
                FillWeight = 180
            });

            dgvArtikel.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = _settings.GetFieldName("Lagerort"),
                DataPropertyName = "Lagerort",
                MinimumWidth = 130,
                FillWeight = 110
            });

            dgvArtikel.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = _settings.GetFieldName("SollMenge"),
                DataPropertyName = "SollMenge",
                ReadOnly = false,
                MinimumWidth = 90,
                FillWeight = 70
            });

            foreach (var field in _settings.CustomImportFields.Where(f => !string.IsNullOrWhiteSpace(f)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                dgvArtikel.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = field,
                    Name = "Custom__" + field,
                    DataPropertyName = string.Empty,
                    ReadOnly = false,
                    MinimumWidth = 140,
                    FillWeight = 120
                });
            }

            foreach (DataGridViewColumn column in dgvArtikel.Columns)
                column.SortMode = DataGridViewColumnSortMode.Programmatic;
        }


        private void DgvArtikel_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.ColumnIndex >= dgvArtikel.Columns.Count)
                return;

            var column = dgvArtikel.Columns[e.ColumnIndex];
            var field = GetSortField(column);
            if (string.IsNullOrWhiteSpace(field))
                return;

            if (string.Equals(_sortField, field, StringComparison.OrdinalIgnoreCase))
                _sortDescending = !_sortDescending;
            else
            {
                _sortField = field;
                _sortDescending = false;
            }

            ApplyArticleFilterSafe();
            UpdateSortGlyphs();
        }

        private string? GetSortField(DataGridViewColumn column)
        {
            if (column.Name.StartsWith("Custom__", StringComparison.OrdinalIgnoreCase))
                return column.Name["Custom__".Length..];

            return column.DataPropertyName switch
            {
                "Artikelnummer" => "Artikelnummer",
                "Bezeichnung" => "Bezeichnung",
                "Lagerort" => "Lagerort",
                "SollMenge" => "SollMenge",
                _ => null
            };
        }

        private IEnumerable<Artikel> ApplyCurrentSort(IEnumerable<Artikel> items)
        {
            if (string.IsNullOrWhiteSpace(_sortField))
                return items;

            if (string.Equals(_sortField, "SollMenge", StringComparison.OrdinalIgnoreCase))
                return _sortDescending ? items.OrderByDescending(a => a.SollMenge) : items.OrderBy(a => a.SollMenge);

            Func<Artikel, string> selector = _sortField switch
            {
                "Artikelnummer" => a => a.Artikelnummer ?? string.Empty,
                "Bezeichnung" => a => a.Bezeichnung ?? string.Empty,
                "Lagerort" => a => a.Lagerort ?? string.Empty,
                _ => a => a.CustomFields != null && a.CustomFields.TryGetValue(_sortField, out var value) ? value ?? string.Empty : string.Empty
            };

            return _sortDescending
                ? items.OrderByDescending(selector, StringComparer.OrdinalIgnoreCase)
                : items.OrderBy(selector, StringComparer.OrdinalIgnoreCase);
        }

        private void UpdateSortGlyphs()
        {
            foreach (DataGridViewColumn column in dgvArtikel.Columns)
            {
                var field = GetSortField(column);
                column.HeaderCell.SortGlyphDirection = string.Equals(field, _sortField, StringComparison.OrdinalIgnoreCase)
                    ? (_sortDescending ? SortOrder.Descending : SortOrder.Ascending)
                    : SortOrder.None;
            }
        }

        // =========================
        // DATA ERROR SAFE MODE
        // =========================

        private void DgvArtikel_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var column = dgvArtikel.Columns[e.ColumnIndex];
            if (!column.Name.StartsWith("Custom__", StringComparison.OrdinalIgnoreCase))
                return;

            if (dgvArtikel.Rows[e.RowIndex].DataBoundItem is not Artikel artikel)
                return;

            var fieldName = column.Name["Custom__".Length..];
            e.Value = artikel.CustomFields != null && artikel.CustomFields.TryGetValue(fieldName, out var value) ? value : string.Empty;
            e.FormattingApplied = true;
        }

        private void DgvArtikel_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
            e.Cancel = true;
        }

        private void DgvArtikel_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0 || dgvArtikel.Columns[e.ColumnIndex].DataPropertyName != "SollMenge")
                return;

            if (!int.TryParse(e.FormattedValue?.ToString(), out var value) || value < 0)
            {
                e.Cancel = true;
                dgvArtikel.Rows[e.RowIndex].ErrorText = $"{_settings.GetFieldName("SollMenge")} muss eine Zahl ab 0 sein.";
                MessageBox.Show($"{_settings.GetFieldName("SollMenge")} muss eine Zahl ab 0 sein.", "Artikel bearbeiten", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                dgvArtikel.Rows[e.RowIndex].ErrorText = string.Empty;
            }
        }

        private void DgvArtikel_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            if (_isRefreshingGrid || e.RowIndex < 0)
                return;

            if (!UserPermissionService.CanEditArticle(_benutzerService?.CurrentUser))
                return;

            dgvArtikel.Rows[e.RowIndex].ErrorText = string.Empty;

            if (dgvArtikel.Rows[e.RowIndex].DataBoundItem is not Artikel artikel)
                return;

            var column = dgvArtikel.Columns[e.ColumnIndex];
            if (column.Name.StartsWith("Custom__", StringComparison.OrdinalIgnoreCase))
            {
                artikel.CustomFields ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var fieldName = column.Name["Custom__".Length..];
                artikel.CustomFields[fieldName] = dgvArtikel.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString() ?? string.Empty;
            }

            var validation = _service.Validate(artikel).ToList();
            var duplicate = _service.GetAlle().FirstOrDefault(a =>
                a.Id != artikel.Id &&
                string.Equals(a.Artikelnummer, artikel.Artikelnummer, StringComparison.OrdinalIgnoreCase));

            if (duplicate != null)
                validation.Add($"{_settings.GetFieldName("Artikelnummer")} existiert bereits.");

            if (validation.Count > 0)
            {
                MessageBox.Show(string.Join(Environment.NewLine, validation), "Artikel bearbeiten", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ReloadDataDeferred();
                return;
            }

            _service.Upsert(artikel);
            _service.EnsureBezeichnung(artikel.Bezeichnung);

            // Wichtig: Keine DataSource-/Filter-Aktualisierung direkt innerhalb von CellEndEdit.
            // Das kann bei DataGridView einen Wiedereintritt in SetCurrentCellAddressCore auslösen.
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed) return;
                RefreshLagerortFilter(_service.GetAlle());
                UpdateDeviceDetailPanel(artikel);
                UpdateDashboard(_service.GetAlle(), "Artikel gespeichert");
                SetStatus("Artikeländerung gespeichert");
            }));
        }

        // =========================
        // LOAD
        // =========================
        private void LoadData()
        {
            var data = _service.GetAlle();

            foreach (var a in data)
                _service.EnsureBezeichnung(a.Bezeichnung);

            RefreshLagerortFilter(data);
            ApplyArticleFilter(setStatus: false);
            UpdateDashboard(data, "Daten geladen");
            SetStatus($"{data.Count} Artikel geladen");
        }

        private void RefreshLagerortFilter(List<Artikel> data)
        {
            if (_cmbLagerortFilter == null) return;

            var selected = _cmbLagerortFilter.SelectedItem?.ToString();
            _suppressFilterEvents = true;
            try
            {
                _cmbLagerortFilter.BeginUpdate();
                _cmbLagerortFilter.Items.Clear();
                _cmbLagerortFilter.Items.Add("Alle Lagerorte");

                foreach (var lagerort in data
                    .Select(a => a.Lagerort)
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(l => l))
                {
                    _cmbLagerortFilter.Items.Add(lagerort);
                }

                var index = !string.IsNullOrWhiteSpace(selected) && _cmbLagerortFilter.Items.Contains(selected)
                    ? _cmbLagerortFilter.Items.IndexOf(selected)
                    : 0;

                _cmbLagerortFilter.SelectedIndex = Math.Max(0, index);
            }
            finally
            {
                _cmbLagerortFilter.EndUpdate();
                _suppressFilterEvents = false;
            }
        }

        private void ApplyArticleFilterSafe(bool setStatus = true)
        {
            if (_isRefreshingGrid || dgvArtikel.IsCurrentCellInEditMode)
            {
                BeginInvoke(new Action(() => ApplyArticleFilter(setStatus)));
                return;
            }

            ApplyArticleFilter(setStatus);
        }

        private void ApplyArticleFilter(bool setStatus = true)
        {
            if (_isRefreshingGrid) return;

            var data = _service.GetAlle();
            var search = _txtSearch?.Text.Trim() ?? string.Empty;
            var lagerort = _cmbLagerortFilter?.SelectedItem?.ToString();

            IEnumerable<Artikel> filtered = data;

            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(a =>
                    (a.Artikelnummer?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (a.Bezeichnung?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (a.Lagerort?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (!string.IsNullOrWhiteSpace(lagerort) && lagerort != "Alle Lagerorte")
            {
                filtered = filtered.Where(a => string.Equals(a.Lagerort, lagerort, StringComparison.OrdinalIgnoreCase));
            }

            var result = ApplyCurrentSort(filtered).ToList();
            _isRefreshingGrid = true;
            try
            {
                dgvArtikel.SuspendLayout();
                _artikelView.RaiseListChangedEvents = false;
                _artikelView.Clear();
                foreach (var artikel in result)
                    _artikelView.Add(artikel);
                _artikelView.RaiseListChangedEvents = true;
                _artikelBindingSource.ResetBindings(false);
                UpdateSortGlyphs();
                if (_artikelView.Count > 0 && dgvArtikel.Rows.Count > 0)
                    dgvArtikel.Rows[0].Selected = true;
            }
            finally
            {
                _artikelView.RaiseListChangedEvents = true;
                dgvArtikel.ResumeLayout();
                _isRefreshingGrid = false;
            }

            UpdateDeviceDetailPanel(GetSelectedArtikel());
            UpdateDashboard(data, result.Count == data.Count ? "Bereit" : $"{result.Count} Treffer");

            if (setStatus)
                SetStatus($"{result.Count} von {data.Count} Artikeln angezeigt");
        }

        private void ReloadDataDeferred()
        {
            BeginInvoke(new Action(() =>
            {
                if (!IsDisposed)
                    LoadData();
            }));
        }

        private void UpdateDashboard(List<Artikel> data, string status)
        {
            if (_lblMetricArtikel != null) _lblMetricArtikel.Text = data.Count.ToString();
            if (_lblMetricLagerorte != null) _lblMetricLagerorte.Text = data
                .Select(a => a.Lagerort)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count()
                .ToString();
            if (_lblMetricMenge != null) _lblMetricMenge.Text = data.Sum(a => a.SollMenge).ToString("N0");
            if (_lblMetricLetzterExport != null) _lblMetricLetzterExport.Text = status;
        }

        // =========================
        // NEU
        // =========================
        private void btnNeu_Click(object sender, EventArgs e)
        {
            if (!RequirePermission(UserPermissionService.CanCreateArticle(_benutzerService?.CurrentUser), "Artikel anlegen")) return;

            using var dlg = new ArtikelEditForm(_service);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var existing = _service
                    .GetAlle()
                    .FirstOrDefault(a => a.Artikelnummer == dlg.Artikel.Artikelnummer);

                if (existing != null)
                {
                    MessageBox.Show("Artikelnummer existiert bereits!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _service.Upsert(dlg.Artikel);
                LoadData();
            }
        }


        // =========================
        // LÖSCHEN
        // =========================
        private void btnLöschen_Click(object sender, EventArgs e)
        {
            if (!RequirePermission(UserPermissionService.CanDeleteArticle(_benutzerService?.CurrentUser), "Artikel löschen")) return;

            if (dgvArtikel.CurrentRow?.DataBoundItem is Artikel artikel)
            {
                _service.Löschen(artikel.Id);
                LoadData();
                SetStatus("Artikel gelöscht");
            }
        }

        // =========================
        // EDIT
        // =========================
        private void dgvArtikel_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            EditSelectedArtikel();
        }

        private void EditSelectedArtikel()
        {
            if (!RequirePermission(UserPermissionService.CanEditArticle(_benutzerService?.CurrentUser), "Artikel bearbeiten")) return;

            if (dgvArtikel.CurrentRow?.DataBoundItem is Artikel artikel)
            {
                using var dlg = new ArtikelEditForm(_service, artikel);

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _service.Upsert(dlg.Artikel);
                    LoadData();
                    SetStatus("Artikel gespeichert");
                }
            }
        }


        private void ShowDeviceDetailWindow()
        {
            var artikel = GetSelectedArtikel();
            if (artikel == null)
            {
                MessageBox.Show("Bitte zuerst ein Gerät oder einen Artikel auswählen.", "Gerätedetails", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var documents = _documentService.GetByArtikelnummer(artikel.Artikelnummer);
            var form = new DeviceDetailForm(artikel, documents, _settings);
            if (_settings.MultiWindowEnabled)
                form.Show(this);
            else
                using (form)
                    form.ShowDialog(this);
        }

        // =========================
        // EXPORT
        // =========================
        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            if (!RequirePermission(UserPermissionService.CanExport(_benutzerService?.CurrentUser), "CSV exportieren")) return;

            var exportDir = AppPaths.ExportDirectory;
            Directory.CreateDirectory(exportDir);

            using var dlg = new SaveFileDialog
            {
                Filter = "CSV-Dateien (*.csv)|*.csv",
                InitialDirectory = exportDir,
                FileName = $"Inventur_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _csvService.ExportArtikelCsv(
                    _service.GetAlle(),
                    dlg.FileName,
                    _settings);

                SetStatus("CSV exportiert");
            }
        }

        // =========================
        // IMPORT
        // =========================
        private void btnImportCsv_Click(object sender, EventArgs e)
        {
            if (!RequirePermission(UserPermissionService.CanImport(_benutzerService?.CurrentUser), "CSV importieren")) return;

            using var dlg = new OpenFileDialog
            {
                Filter = "CSV-Dateien (*.csv)|*.csv"
            };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            ImportCsvDatei(dlg.FileName);
        }

        private void ImportCsvDatei(string dateiPfad)
        {
            if (!RequirePermission(UserPermissionService.CanImport(_benutzerService?.CurrentUser), "CSV importieren")) return;

            try
            {
                var rows = _csvService.ReadRows(dateiPfad);

                if (rows.Count < 2)
                {
                    MessageBox.Show("Die CSV-Datei enthält keine importierbaren Daten.", "CSV-Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var header = rows[0];

                using var mappingForm = new ImportMappingForm(header, _settings);
                if (mappingForm.ShowDialog() != DialogResult.OK)
                    return;

                var mapping = mappingForm.Mapping;
                if (mappingForm.RememberMapping)
                {
                    var changed = false;
                    foreach (var alias in mappingForm.LearnedAliases)
                        changed |= FieldMappingService.AddAlias(_settings, alias.Value, alias.Key);

                    foreach (var target in mapping.Values.Where(FieldMappingService.IsCustomTarget))
                        changed |= FieldMappingService.AddCustomField(_settings, FieldMappingService.GetCustomFieldName(target));

                    if (changed)
                    {
                        _settingsService.Save(_settings);
                        _columnsSetup = false;
                        SetupGrid();
                    }
                }

                var count = 0;
                var fehler = new List<string>();

                foreach (var rowInfo in rows.Skip(1).Select((values, index) => new { values, zeile = index + 2 }))
                {
                    var artikel = new Artikel { CustomFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) };
                    var rowFehler = new List<string>();

                    for (int i = 0; i < header.Length && i < rowInfo.values.Length; i++)
                    {
                        if (!mapping.TryGetValue(header[i], out var ziel))
                            continue;

                        var value = rowInfo.values[i]?.Trim() ?? string.Empty;

                        switch (ziel)
                        {
                            case "Artikelnummer":
                                artikel.Artikelnummer = value;
                                break;

                            case "Bezeichnung":
                                artikel.Bezeichnung = value;
                                break;

                            case "Lagerort":
                                artikel.Lagerort = value;
                                break;

                            case "SollMenge":
                                if (string.IsNullOrWhiteSpace(value))
                                {
                                    artikel.SollMenge = 0;
                                }
                                else if (int.TryParse(value, out var m))
                                {
                                    artikel.SollMenge = m;
                                }
                                else
                                {
                                    rowFehler.Add("Soll-Menge ist keine gültige Zahl.");
                                }
                                break;

                            default:
                                if (FieldMappingService.IsCustomTarget(ziel))
                                {
                                    var fieldName = FieldMappingService.GetCustomFieldName(ziel);
                                    if (!string.IsNullOrWhiteSpace(fieldName))
                                        artikel.CustomFields[fieldName] = value;
                                }
                                break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(artikel.Bezeichnung))
                        artikel.Bezeichnung = artikel.CustomFields.TryGetValue("System Modell", out var model) && !string.IsNullOrWhiteSpace(model)
                            ? model
                            : "Gerät";

                    var validierungsFehler = _service.Validate(artikel);
                    rowFehler.AddRange(validierungsFehler);

                    if (rowFehler.Count > 0)
                    {
                        fehler.Add($"Zeile {rowInfo.zeile}: {string.Join(" ", rowFehler)}");
                        continue;
                    }

                    _service.Upsert(artikel);
                    count++;
                }

                LoadData();

                if (fehler.Count > 0)
                {
                    MessageBox.Show(
                        string.Join(Environment.NewLine, fehler.Take(20)) + (fehler.Count > 20 ? Environment.NewLine + "..." : string.Empty),
                        "CSV-Import mit Hinweisen",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                SetStatus($"{count} Artikel importiert/aktualisiert ✔");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fehler beim CSV-Import", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void ExportPdf()
        {
            if (!RequirePermission(UserPermissionService.CanExport(_benutzerService?.CurrentUser), "PDF exportieren")) return;

            try
            {
                AppPaths.EnsureAll();
                var fileName = Path.Combine(AppPaths.ExportDirectory, $"Inventur_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
                _pdfExportService.ExportArtikelPdf(_service.GetAlle(), fileName);
                SetStatus("PDF exportiert");
                MessageBox.Show($"PDF wurde erstellt:\n{fileName}", "PDF-Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "PDF-Export", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateBackup()
        {
            if (!RequirePermission(UserPermissionService.CanCreateBackup(_benutzerService?.CurrentUser), "Backup erstellen")) return;

            try
            {
                var file = _backupService.CreateBackup();
                SetStatus("Backup erstellt");
                MessageBox.Show($"Backup wurde erstellt:\n{file}", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Backup", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowStatistics()
        {
            var form = new StatisticsForm(_service.GetAlle());
            if (_settings.MultiWindowEnabled)
                form.Show(this);
            else
                form.ShowDialog(this);
        }

        private void ShowAbout()
        {
            using var about = new AboutForm();
            about.ShowDialog(this);
        }

        private void ShowSettings()
        {
            if (!RequirePermission(UserPermissionService.CanOpenSettings(_benutzerService?.CurrentUser), "Einstellungen öffnen")) return;

            using var dlg = new SettingsForm(_settings, _benutzerService);
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            _settings = dlg.Settings;
            _settingsService.Save(_settings);
            ModernTheme.SetDarkMode(_settings.DarkMode);
            _columnsSetup = false;
            ApplyModernDesign();
            SetupGrid();
            LoadData();
            SetStatus("Einstellungen gespeichert");
        }

        private void ToggleDarkMode()
        {
            _settings.DarkMode = !_settings.DarkMode;
            _settingsService.Save(_settings);
            ModernTheme.SetDarkMode(_settings.DarkMode);
            _columnsSetup = false;
            ApplyModernDesign();
            SetupGrid();
            LoadData();
            SetStatus(_settings.DarkMode ? "Dark Mode aktiv" : "Light Mode aktiv");
        }

        private string GetScannerPlaceholder()
        {
            if (_settings.TwainEnabled && !string.IsNullOrWhiteSpace(_settings.TwainSourceName))
                return $"Barcode/Scanner: {_settings.TwainSourceName} – scannen oder Artikelnummer eingeben...";

            return "Barcode scannen oder Artikelnummer eingeben...";
        }


        private void ShowDocuments()
        {
            if (!RequirePermission(UserPermissionService.CanOpenDocuments(_benutzerService?.CurrentUser), "Dokumentenverwaltung öffnen")) return;

            var form = new DocumentManagerForm(
                _service,
                _settings,
                _benutzerService?.CurrentUser?.Username ?? Environment.UserName,
                UserPermissionService.CanManageDocuments(_benutzerService?.CurrentUser));

            if (_settings.MultiWindowEnabled)
                form.Show(this);
            else
                form.ShowDialog(this);
        }


        private void StartTwainScan()
        {
            if (!RequirePermission(UserPermissionService.CanUseScanner(_benutzerService?.CurrentUser), "Scan starten")) return;

            if (!_settings.ScannerEnabled)
            {
                MessageBox.Show("Scanner ist deaktiviert. Aktiviere ihn zuerst in den Einstellungen.", "Scanner", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!_settings.TwainEnabled)
            {
                MessageBox.Show("Die Scanner-Schnittstelle ist deaktiviert. Aktiviere sie zuerst in den Einstellungen.", "Scanner", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = _twainService.Scan(_settings.TwainSourceName);
            if (result.Success)
            {
                if (!string.IsNullOrWhiteSpace(result.FilePath))
                {
                    _documentService.RegisterScan(
                        result.FilePath,
                        "Schnellscan",
                        "Scan",
                        null,
                        string.IsNullOrWhiteSpace(_settings.TwainSourceName) ? "Scanner" : _settings.TwainSourceName,
                        _benutzerService?.CurrentUser?.Username ?? Environment.UserName);
                }

                SetStatus("Scan abgeschlossen und dokumentiert");
                MessageBox.Show(result.Message + "\n\nDer Scan wurde in der Dokumentenverwaltung abgelegt.", "Scan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SetStatus("Scan konnte nicht gestartet werden");
            MessageBox.Show(result.Message, "Scan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void FocusBarcodeResult()
        {
            if (!RequirePermission(UserPermissionService.CanUseScanner(_benutzerService?.CurrentUser), "Scanner verwenden")) return;
            if (_txtBarcode == null) return;
            var code = BarcodeService.Normalize(_txtBarcode.Text);
            if (!BarcodeService.LooksLikeBarcode(code))
            {
                SetStatus("Barcode ungültig oder zu kurz");
                return;
            }

            _txtSearch!.Text = code;
            var match = _service.GetAlle().FirstOrDefault(a =>
                string.Equals(a.Artikelnummer, code, StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                if (MessageBox.Show("Artikel nicht gefunden. Neu anlegen?", "Barcode", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using var dlg = new ArtikelEditForm(_service, new Artikel { Artikelnummer = code });
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        _service.Upsert(dlg.Artikel);
                        LoadData();
                    }
                }
                return;
            }

            SetStatus($"Barcode gefunden: {match.Artikelnummer}");
        }

        // =========================
        // DATEIMANAGER 🔥 NEU
        // =========================

        private void btnDateimanager_Click(object sender, EventArgs e)
        {
            var dlg = new FileManagerForm(_service, _settings, _benutzerService?.CurrentUser?.Username ?? Environment.UserName, UserPermissionService.CanManageDocuments(_benutzerService?.CurrentUser));
            if (_settings.MultiWindowEnabled)
                dlg.Show(this);
            else
                dlg.ShowDialog(this);
        }

        // =========================
        // STATUS
        // =========================
        private void SetStatus(string text)
        {
            lblStatus.Text = $"{DateTime.Now:HH:mm:ss} – {text}";
            if (_lblMetricLetzterExport != null)
                _lblMetricLetzterExport.Text = text.Length > 16 ? text[..16] + "…" : text;
        }

        public void LoadCsvFromFileManager(string path)
        {
            ImportCsvDatei(path);
        }
    }


}