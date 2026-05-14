using InventurApp.Models;
using InventurApp.Services;
using InventurApp.UI;

namespace InventurApp.Forms
{
    public class SettingsForm : Form
    {
        private readonly BenutzerService _benutzerService;
        private readonly TwainService _twainService = new();

        private readonly CheckBox _chkScanner = new();
        private readonly CheckBox _chkDarkMode = new();
        private readonly CheckBox _chkMultiWindow = new();
        private readonly CheckBox _chkTwainEnabled = new();
        private readonly CheckBox _chkAutoLogin = new();
        private readonly ComboBox _cmbTwainSources = new();
        private readonly ComboBox _cmbAutoLoginUser = new();
        private readonly Label _lblHint = new();
        private readonly Label _lblTwainInfo = new();

        private readonly Dictionary<string, TextBox> _fieldNameBoxes = new();
        private readonly Dictionary<string, TextBox> _aliasBoxes = new();

        public AppSettings Settings { get; }

        public SettingsForm(AppSettings currentSettings, BenutzerService? benutzerService = null)
        {
            _benutzerService = benutzerService ?? new BenutzerService();
            currentSettings.EnsureFieldDefaults();
            Settings = new AppSettings
            {
                DarkMode = currentSettings.DarkMode,
                ScannerEnabled = currentSettings.ScannerEnabled,
                MultiWindowEnabled = currentSettings.MultiWindowEnabled,
                LastBackupAt = currentSettings.LastBackupAt,
                TwainSourceName = currentSettings.TwainSourceName,
                TwainEnabled = currentSettings.TwainEnabled,
                AutoLoginEnabled = currentSettings.AutoLoginEnabled,
                AutoLoginUsername = currentSettings.AutoLoginUsername,
                AutoUpdateCheckEnabled = currentSettings.AutoUpdateCheckEnabled,
                UpdateManifestUrl = currentSettings.UpdateManifestUrl,
                LastUpdateCheckAt = currentSettings.LastUpdateCheckAt,
                FieldDisplayNames = currentSettings.FieldDisplayNames.ToDictionary(k => k.Key, v => v.Value),
                FieldImportAliases = currentSettings.FieldImportAliases.ToDictionary(k => k.Key, v => v.Value.ToList())
            };
            Settings.EnsureFieldDefaults();

            InitializeLayout();
            LoadValues();
        }

        private void InitializeLayout()
        {
            ModernTheme.ApplyForm(this);
            Text = UserPermissionService.CanManageUsers(_benutzerService.CurrentUser) ? "Einstellungen" : "Einstellungen – eingeschränkt";
            ClientSize = new Size(840, 650);
            MinimumSize = new Size(760, 590);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = ModernTheme.Background;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(24),
                BackColor = ModernTheme.Background
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            Controls.Add(root);

            var title = ModernTheme.CreateTitleLabel("Einstellungen", "Scanner, WIA/TWAIN, Benutzer, Felder, Design und Fensterverhalten anpassen");
            root.Controls.Add(title, 0, 0);

            var tabs = new TabControl { Dock = DockStyle.Fill, Font = ModernTheme.BaseFont };
            tabs.TabPages.Add(CreateGeneralTab());
            tabs.TabPages.Add(CreateUpdateTab());
            tabs.TabPages.Add(CreateScannerTab());
            tabs.TabPages.Add(CreateFieldsTab());
            if (UserPermissionService.CanManageUsers(_benutzerService.CurrentUser))
                tabs.TabPages.Add(CreateUsersTab());
            root.Controls.Add(tabs, 0, 1);

            _lblHint.Text = "Die Einstellungen werden in deinem lokalen App-Datenordner gespeichert.";
            _lblHint.Dock = DockStyle.Fill;
            _lblHint.TextAlign = ContentAlignment.MiddleLeft;
            ModernTheme.ApplyLabel(_lblHint, muted: true);
            root.Controls.Add(_lblHint, 0, 2);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = ModernTheme.Background
            };
            root.Controls.Add(buttons, 0, 4);

            var btnSave = new Button { Text = "Speichern", Width = 118, Height = 38, DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Abbrechen", Width = 118, Height = 38, DialogResult = DialogResult.Cancel };
            btnSave.Click += (_, _) => SaveValues();

            ModernTheme.ApplyPrimaryButton(btnSave);
            ModernTheme.ApplySecondaryButton(btnCancel);
            buttons.Controls.Add(btnSave);
            buttons.Controls.Add(btnCancel);

            AcceptButton = btnSave;
            CancelButton = btnCancel;
        }

        private TabPage CreateGeneralTab()
        {
            var tab = CreateTabPage("Allgemein");
            var card = ModernTheme.CreateCardPanel();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(22);
            tab.Controls.Add(card);

            var options = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 1,
                RowCount = 2,
                Height = 132,
                BackColor = ModernTheme.Surface
            };
            options.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            options.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            card.Controls.Add(options);

            ConfigureOption(_chkDarkMode, "Dark Mode verwenden", "Schaltet die Oberfläche dauerhaft auf das dunkle Design um.");
            ConfigureOption(_chkMultiWindow, "Mehrfenster-Modus aktivieren", "Dateimanager und Statistik bleiben parallel zum Hauptfenster geöffnet.");
            options.Controls.Add(_chkDarkMode, 0, 0);
            options.Controls.Add(_chkMultiWindow, 0, 1);
            return tab;
        }


        private TabPage CreateUpdateTab()
        {
            var tab = CreateTabPage("Version / Update");
            var card = ModernTheme.CreateCardPanel();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(22);
            tab.Controls.Add(card);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 5,
                Height = 250,
                BackColor = ModernTheme.Surface
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 5; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            card.Controls.Add(layout);

            var lblVersion = new Label { Text = "Aktuelle Version", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            var txtVersion = new TextBox { Text = AppInfoService.CurrentVersionText, Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None };
            ModernTheme.ApplyLabel(lblVersion, muted: true);
            ModernTheme.ApplyInput(txtVersion);
            layout.Controls.Add(lblVersion, 0, 0);
            layout.Controls.Add(txtVersion, 1, 0);

            var lblData = new Label { Text = "Datenordner", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            var txtData = new TextBox { Text = AppInfoService.DataDirectory, Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None };
            ModernTheme.ApplyLabel(lblData, muted: true);
            ModernTheme.ApplyInput(txtData);
            layout.Controls.Add(lblData, 0, 1);
            layout.Controls.Add(txtData, 1, 1);

            var lblLast = new Label { Text = "Letzte Prüfung", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            var txtLast = new TextBox { Text = Settings.LastUpdateCheckAt?.ToString("dd.MM.yyyy HH:mm") ?? "Noch nicht geprüft", Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None };
            ModernTheme.ApplyLabel(lblLast, muted: true);
            ModernTheme.ApplyInput(txtLast);
            layout.Controls.Add(lblLast, 0, 2);
            layout.Controls.Add(txtLast, 1, 2);

            var hint = new Label
            {
                Text = "Update-Quelle und automatische Prüfung werden im separaten Update-Fenster gepflegt.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            ModernTheme.ApplyLabel(hint, muted: true);
            layout.SetColumnSpan(hint, 2);
            layout.Controls.Add(hint, 0, 3);

            var actionPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                BackColor = ModernTheme.Surface
            };

            var btnUpdates = new Button { Text = "🔄 Version & Updates", Width = 190, Height = 40 };
            btnUpdates.Click += (_, _) =>
            {
                using var updateForm = new UpdateInfoForm(Settings);
                updateForm.ShowDialog(this);
            };

            var btnAbout = new Button { Text = "ℹ Über Alex Inventur", Width = 190, Height = 40 };
            btnAbout.Click += (_, _) =>
            {
                using var aboutForm = new AboutForm();
                aboutForm.ShowDialog(this);
            };

            ModernTheme.ApplyPrimaryButton(btnUpdates);
            ModernTheme.ApplySecondaryButton(btnAbout);
            actionPanel.Controls.Add(btnUpdates);
            actionPanel.Controls.Add(btnAbout);
            layout.Controls.Add(actionPanel, 1, 4);
            return tab;
        }

        private TabPage CreateScannerTab()
        {
            var tab = CreateTabPage("Scanner / WIA-TWAIN");
            var card = ModernTheme.CreateCardPanel();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(22);
            tab.Controls.Add(card);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 5,
                Height = 250,
                BackColor = ModernTheme.Surface
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            card.Controls.Add(layout);

            ConfigureOption(_chkScanner, "Barcode-Scanner aktivieren", "Zeigt das Barcode-Eingabefeld und erlaubt Enter-Scan-Suche.");
            layout.SetColumnSpan(_chkScanner, 2);
            layout.Controls.Add(_chkScanner, 0, 0);

            ConfigureOption(_chkTwainEnabled, "Scanner-Schnittstelle aktivieren", "Erlaubt den direkten Scan über WIA. TWAIN-Quellen werden erkannt, benötigen für echte TWAIN-Scans aber später eine externe Bibliothek.");
            layout.SetColumnSpan(_chkTwainEnabled, 2);
            layout.Controls.Add(_chkTwainEnabled, 0, 1);

            var lblSource = new Label { Text = "Scanner-Quelle", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            ModernTheme.ApplyLabel(lblSource, muted: true);
            layout.Controls.Add(lblSource, 0, 2);

            _cmbTwainSources.Dock = DockStyle.Fill;
            _cmbTwainSources.DropDownStyle = ComboBoxStyle.DropDownList;
            ModernTheme.ApplyInput(_cmbTwainSources);
            layout.Controls.Add(_cmbTwainSources, 1, 2);

            var btnRefresh = new Button { Text = "Quellen neu suchen", Height = 36, Width = 180, Dock = DockStyle.Left };
            btnRefresh.Click += (_, _) => LoadTwainSources();
            ModernTheme.ApplySecondaryButton(btnRefresh);
            layout.Controls.Add(btnRefresh, 1, 3);

            _lblTwainInfo.Dock = DockStyle.Fill;
            _lblTwainInfo.TextAlign = ContentAlignment.MiddleLeft;
            ModernTheme.ApplyLabel(_lblTwainInfo, muted: true);
            layout.SetColumnSpan(_lblTwainInfo, 2);
            layout.Controls.Add(_lblTwainInfo, 0, 4);
            return tab;
        }

        private TabPage CreateFieldsTab()
        {
            var tab = CreateTabPage("Felder / Import");
            var card = ModernTheme.CreateCardPanel();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(22);
            tab.Controls.Add(card);

            var description = new Label
            {
                Text = "Hier änderst du die sichtbaren Feldnamen. Import-Aliase erlauben zusätzliche CSV-Spaltennamen, z. B. SKU oder Artikel-Nr.",
                Dock = DockStyle.Top,
                Height = 54,
                TextAlign = ContentAlignment.MiddleLeft
            };
            ModernTheme.ApplyLabel(description, muted: true);
            card.Controls.Add(description);

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = FieldMappingService.CanonicalFields.Length + 1,
                BackColor = ModernTheme.Surface,
                Padding = new Padding(0, 10, 0, 0)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            card.Controls.Add(grid);
            grid.BringToFront();

            AddHeader(grid, "Internes Feld", 0, 0);
            AddHeader(grid, "Anzeigename", 1, 0);
            AddHeader(grid, "Import-Aliase ; getrennt", 2, 0);

            var row = 1;
            foreach (var field in FieldMappingService.CanonicalFields)
            {
                var internalLabel = new Label { Text = field, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
                ModernTheme.ApplyLabel(internalLabel, muted: true);
                grid.Controls.Add(internalLabel, 0, row);

                var nameBox = new TextBox { Dock = DockStyle.Fill };
                ModernTheme.ApplyInput(nameBox);
                _fieldNameBoxes[field] = nameBox;
                grid.Controls.Add(nameBox, 1, row);

                var aliasBox = new TextBox { Dock = DockStyle.Fill };
                ModernTheme.ApplyInput(aliasBox);
                _aliasBoxes[field] = aliasBox;
                grid.Controls.Add(aliasBox, 2, row);
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
                row++;
            }

            return tab;
        }

        private TabPage CreateUsersTab()
        {
            var tab = CreateTabPage("Benutzer");
            var card = ModernTheme.CreateCardPanel();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(22);
            tab.Controls.Add(card);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 4,
                Height = 210,
                BackColor = ModernTheme.Surface
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            card.Controls.Add(layout);

            var description = new Label
            {
                Text = "Lege Benutzer an und aktiviere bei Bedarf die automatische Anmeldung für einen festen Arbeitsplatz.",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            ModernTheme.ApplyLabel(description, muted: true);
            layout.SetColumnSpan(description, 2);
            layout.Controls.Add(description, 0, 0);

            ConfigureOption(_chkAutoLogin, "Automatische Anmeldung aktivieren", "Nur auf vertrauenswürdigen Einzelplatz-Rechnern verwenden.");
            layout.SetColumnSpan(_chkAutoLogin, 2);
            layout.Controls.Add(_chkAutoLogin, 0, 1);

            var lblUser = new Label { Text = "Auto-Login Benutzer", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            ModernTheme.ApplyLabel(lblUser, muted: true);
            layout.Controls.Add(lblUser, 0, 2);

            _cmbAutoLoginUser.Dock = DockStyle.Fill;
            _cmbAutoLoginUser.DropDownStyle = ComboBoxStyle.DropDownList;
            ModernTheme.ApplyInput(_cmbAutoLoginUser);
            layout.Controls.Add(_cmbAutoLoginUser, 1, 2);

            var btnUsers = new Button { Text = "👤 Benutzerverwaltung öffnen", Dock = DockStyle.Left, Height = 42, Width = 260 };
            btnUsers.Click += (_, _) =>
            {
                using var usersForm = new UserManagementForm(_benutzerService);
                usersForm.ShowDialog(this);
                LoadUsersForAutoLogin();
            };
            ModernTheme.ApplyPrimaryButton(btnUsers);
            layout.Controls.Add(btnUsers, 1, 3);

            return tab;
        }

        private static void AddHeader(TableLayoutPanel grid, string text, int column, int row)
        {
            var label = new Label { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = ModernTheme.SubtitleFont };
            ModernTheme.ApplyLabel(label, muted: true);
            grid.Controls.Add(label, column, row);
            if (grid.RowStyles.Count == 0)
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        }

        private static TabPage CreateTabPage(string text) => new()
        {
            Text = text,
            BackColor = ModernTheme.Background,
            Padding = new Padding(6)
        };

        private static void ConfigureOption(CheckBox checkBox, string title, string subtitle)
        {
            checkBox.Text = $"{title}\n{subtitle}";
            checkBox.Dock = DockStyle.Fill;
            checkBox.AutoSize = false;
            checkBox.TextAlign = ContentAlignment.MiddleLeft;
            checkBox.CheckAlign = ContentAlignment.MiddleLeft;
            checkBox.Padding = new Padding(8, 0, 0, 0);
            checkBox.Font = ModernTheme.BaseFont;
            checkBox.ForeColor = ModernTheme.Text;
            checkBox.BackColor = ModernTheme.Surface;
        }

        private void LoadValues()
        {
            Settings.EnsureFieldDefaults();
            _chkScanner.Checked = Settings.ScannerEnabled;
            _chkDarkMode.Checked = Settings.DarkMode;
            _chkMultiWindow.Checked = Settings.MultiWindowEnabled;
            _chkTwainEnabled.Checked = Settings.TwainEnabled;
            _chkAutoLogin.Checked = Settings.AutoLoginEnabled;

            foreach (var field in FieldMappingService.CanonicalFields)
            {
                _fieldNameBoxes[field].Text = Settings.GetFieldName(field);
                _aliasBoxes[field].Text = Settings.FieldImportAliases.TryGetValue(field, out var aliases)
                    ? string.Join("; ", aliases)
                    : string.Empty;
            }

            LoadUsersForAutoLogin();
            LoadTwainSources();
        }

        private void LoadUsersForAutoLogin()
        {
            var selected = Settings.AutoLoginUsername;
            _cmbAutoLoginUser.Items.Clear();
            foreach (var user in _benutzerService.GetUsers().Where(u => u.IsActive).OrderBy(u => u.Username))
                _cmbAutoLoginUser.Items.Add(user.Username);

            if (!string.IsNullOrWhiteSpace(selected) && _cmbAutoLoginUser.Items.Contains(selected))
                _cmbAutoLoginUser.SelectedItem = selected;
            else if (_cmbAutoLoginUser.Items.Count > 0)
                _cmbAutoLoginUser.SelectedIndex = 0;
        }

        private void LoadTwainSources()
        {
            var selected = Settings.TwainSourceName;
            _cmbTwainSources.Items.Clear();
            foreach (var source in _twainService.GetAvailableSources())
                _cmbTwainSources.Items.Add(source.Name);

            if (!string.IsNullOrWhiteSpace(selected) && !_cmbTwainSources.Items.Contains(selected))
                _cmbTwainSources.Items.Add(selected);

            if (!string.IsNullOrWhiteSpace(selected) && _cmbTwainSources.Items.Contains(selected))
                _cmbTwainSources.SelectedItem = selected;
            else if (_cmbTwainSources.Items.Count > 0)
                _cmbTwainSources.SelectedIndex = 0;

            _lblTwainInfo.Text = _cmbTwainSources.Items.Count == 1 && _cmbTwainSources.Items[0]?.ToString() == "Standard-WIA-Dialog"
                ? "Es wurde keine konkrete Scanner-Quelle erkannt. Der Standard-WIA-Dialog bleibt auswählbar."
                : $"{_cmbTwainSources.Items.Count} Scanner-Quelle(n) gefunden.";
        }

        private void SaveValues()
        {
            Settings.ScannerEnabled = _chkScanner.Checked;
            Settings.DarkMode = _chkDarkMode.Checked;
            Settings.MultiWindowEnabled = _chkMultiWindow.Checked;
            Settings.TwainEnabled = _chkTwainEnabled.Checked;
            Settings.TwainSourceName = _cmbTwainSources.SelectedItem?.ToString() ?? string.Empty;
            Settings.AutoLoginEnabled = _chkAutoLogin.Checked;
            Settings.AutoLoginUsername = _cmbAutoLoginUser.SelectedItem?.ToString() ?? string.Empty;

            foreach (var field in FieldMappingService.CanonicalFields)
            {
                Settings.FieldDisplayNames[field] = string.IsNullOrWhiteSpace(_fieldNameBoxes[field].Text)
                    ? field
                    : _fieldNameBoxes[field].Text.Trim();

                Settings.FieldImportAliases[field] = _aliasBoxes[field].Text
                    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            Settings.EnsureFieldDefaults();
        }
    }
}
