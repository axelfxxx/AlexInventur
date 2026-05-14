using InventurApp.Models;
using InventurApp.Services;
using InventurApp.UI;

namespace InventurApp.Forms
{
    public class UpdateInfoForm : Form
    {
        private readonly AppSettings _settings;
        private readonly UpdateService _updateService = new();
        private readonly Label _lblStatus = new();
        private readonly TextBox _txtManifest = new();
        private readonly TextBox _txtNotes = new();
        private readonly CheckBox _chkAutoCheck = new();
        private UpdateInfo? _lastInfo;

        public UpdateInfoForm(AppSettings settings)
        {
            _settings = settings;
            InitializeLayout();
            FormClosing += (_, _) => SaveUpdateSettings();
        }

        private void InitializeLayout()
        {
            ModernTheme.ApplyForm(this);
            Text = "Version & Updates";
            ClientSize = new Size(760, 520);
            MinimumSize = new Size(680, 460);
            ShowInTaskbar = false;
            MinimizeBox = false;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(24),
                BackColor = ModernTheme.Background
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 145));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            Controls.Add(root);

            root.Controls.Add(ModernTheme.CreateTitleLabel("Version & Updates", $"{AppInfoService.ProductName} · Version {AppInfoService.CurrentVersionText} · {AppInfoService.BuildConfiguration}"), 0, 0);

            var card = ModernTheme.CreateCardPanel();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(18);
            root.Controls.Add(card, 0, 1);

            var meta = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                BackColor = ModernTheme.Surface
            };
            meta.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160));
            meta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            meta.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            meta.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            meta.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            card.Controls.Add(meta);

            AddMeta(meta, "Datenordner", AppInfoService.DataDirectory, 0);
            AddMeta(meta, "Installationsordner", AppInfoService.InstallDirectory, 1);
            AddMeta(meta, "Letzte Prüfung", _settings.LastUpdateCheckAt?.ToString("dd.MM.yyyy HH:mm") ?? "Noch nicht geprüft", 2);

            var settingsCard = ModernTheme.CreateCardPanel();
            settingsCard.Dock = DockStyle.Fill;
            settingsCard.Padding = new Padding(18);
            root.Controls.Add(settingsCard, 0, 2);

            var inner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                BackColor = ModernTheme.Surface
            };
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            settingsCard.Controls.Add(inner);

            var lblManifest = new Label { Text = "Update-Manifest URL oder Dateipfad", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            ModernTheme.ApplyLabel(lblManifest, muted: true);
            inner.Controls.Add(lblManifest, 0, 0);

            _txtManifest.Dock = DockStyle.Fill;
            _txtManifest.Text = _settings.UpdateManifestUrl;
            ModernTheme.ApplyInput(_txtManifest);
            inner.Controls.Add(_txtManifest, 0, 1);

            _chkAutoCheck.Text = "Beim Start automatisch nach Updates suchen";
            _chkAutoCheck.Checked = _settings.AutoUpdateCheckEnabled;
            _chkAutoCheck.Dock = DockStyle.Fill;
            _chkAutoCheck.Font = ModernTheme.BaseFont;
            _chkAutoCheck.ForeColor = ModernTheme.Text;
            _chkAutoCheck.BackColor = ModernTheme.Surface;
            inner.Controls.Add(_chkAutoCheck, 0, 2);

            _lblStatus.Text = "Bereit zur Update-Prüfung.";
            _lblStatus.Dock = DockStyle.Fill;
            _lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            ModernTheme.ApplyLabel(_lblStatus, muted: true);
            inner.Controls.Add(_lblStatus, 0, 3);

            _txtNotes.Dock = DockStyle.Fill;
            _txtNotes.Multiline = true;
            _txtNotes.ReadOnly = true;
            _txtNotes.ScrollBars = ScrollBars.Vertical;
            _txtNotes.Text = "Release Notes erscheinen nach einer erfolgreichen Prüfung hier.";
            ModernTheme.ApplyInput(_txtNotes);
            inner.Controls.Add(_txtNotes, 0, 4);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                BackColor = ModernTheme.Background
            };
            root.Controls.Add(actions, 0, 3);

            var btnCheck = new Button { Text = "🔄 Jetzt prüfen", Width = 138, Height = 38 };
            var btnOpenDownload = new Button { Text = "⬇ Download öffnen", Width = 150, Height = 38 };
            var btnOpenData = new Button { Text = "📁 Datenordner", Width = 135, Height = 38 };
            btnCheck.Click += async (_, _) => await CheckForUpdatesAsync();
            btnOpenDownload.Click += (_, _) => { if (_lastInfo != null) UpdateService.OpenDownload(_lastInfo); };
            btnOpenData.Click += (_, _) => AppInfoService.OpenDataDirectory();
            ModernTheme.ApplyPrimaryButton(btnCheck);
            ModernTheme.ApplySecondaryButton(btnOpenDownload);
            ModernTheme.ApplySecondaryButton(btnOpenData);
            actions.Controls.AddRange(new Control[] { btnCheck, btnOpenDownload, btnOpenData });

            var bottom = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = ModernTheme.Background
            };
            root.Controls.Add(bottom, 0, 4);

            var btnClose = new Button { Text = "Schließen", Width = 118, Height = 38, DialogResult = DialogResult.OK };
            btnClose.Click += (_, _) => SaveUpdateSettings();
            ModernTheme.ApplyPrimaryButton(btnClose);
            bottom.Controls.Add(btnClose);
            AcceptButton = btnClose;
        }

        private static void AddMeta(TableLayoutPanel table, string labelText, string valueText, int row)
        {
            var label = new Label { Text = labelText, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            var value = new TextBox { Text = valueText, Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None };
            ModernTheme.ApplyLabel(label, muted: true);
            ModernTheme.ApplyInput(value);
            table.Controls.Add(label, 0, row);
            table.Controls.Add(value, 1, row);
        }

        private async Task CheckForUpdatesAsync()
        {
            SaveUpdateSettings();
            _lblStatus.Text = "Update-Prüfung läuft...";
            _txtNotes.Text = string.Empty;
            Enabled = false;
            try
            {
                _lastInfo = await _updateService.CheckForUpdatesAsync(_settings);
                _lblStatus.Text = _lastInfo.StatusMessage;
                _txtNotes.Text = string.IsNullOrWhiteSpace(_lastInfo.ReleaseNotes)
                    ? "Keine Release Notes im Manifest vorhanden."
                    : _lastInfo.ReleaseNotes;
            }
            finally
            {
                Enabled = true;
            }
        }

        private void SaveUpdateSettings()
        {
            _settings.UpdateManifestUrl = _txtManifest.Text.Trim();
            _settings.AutoUpdateCheckEnabled = _chkAutoCheck.Checked;
            new SettingsService().Save(_settings);
        }
    }
}
