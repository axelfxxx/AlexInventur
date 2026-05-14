using InventurApp.Models;
using InventurApp.Services;
using InventurApp.UI;

namespace InventurApp.Forms
{
    public partial class ImportMappingForm : Form
    {
        private readonly AppSettings _settings;
        private readonly List<string> _csvColumns;
        public Dictionary<string, string> Mapping { get; } = new();
        public bool RememberMapping => chkRememberMapping.Checked;
        public Dictionary<string, string> LearnedAliases { get; } = new();

        public ImportMappingForm(IEnumerable<string> csvColumns, AppSettings? settings = null)
        {
            _settings = settings ?? new AppSettings();
            _settings.EnsureFieldDefaults();
            _csvColumns = csvColumns.ToList();
            InitializeComponent();
            ApplyModernDesign();
            ConfigureTargetColumn();

            foreach (var col in csvColumns)
                dgvMapping.Rows.Add(col, FieldMappingService.GuessTarget(_settings, col));
        }

        private void ConfigureTargetColumn()
        {
            if (dgvMapping.Columns["TargetField"] is DataGridViewComboBoxColumn target)
            {
                var targets = new List<string> { string.Empty };
                targets.AddRange(FieldMappingService.GetTargetDisplayNames(_settings, _csvColumns));
                target.DataSource = targets;
            }
        }

        private void ApplyModernDesign()
        {
            ModernTheme.ApplyForm(this);
            Text = "CSV-Spalten zuordnen";
            ClientSize = new Size(820, 620);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            Controls.Clear();

            var header = new Label
            {
                Text = "CSV-Import vorbereiten\nOrdne Standardfelder zu oder übernimm CSV-Spalten als Zusatzfelder. Unbekannte Spalten werden automatisch als Zusatzfelder vorgeschlagen.",
                Location = new Point(24, 20),
                Size = new Size(772, 72),
                ForeColor = ModernTheme.Text,
                BackColor = Color.Transparent,
                Font = ModernTheme.TitleFont
            };

            var card = ModernTheme.CreateCardPanel();
            card.Location = new Point(24, 104);
            card.Size = new Size(772, 400);
            card.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            card.Padding = new Padding(12);

            dgvMapping.Dock = DockStyle.Fill;
            ModernTheme.ApplyGrid(dgvMapping);
            card.Controls.Add(dgvMapping);

            chkRememberMapping.Text = "Diese Zuordnung für zukünftige Importe merken";
            chkRememberMapping.Checked = true;
            chkRememberMapping.Location = new Point(32, 518);
            chkRememberMapping.Size = new Size(420, 28);
            chkRememberMapping.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            chkRememberMapping.BackColor = ModernTheme.Background;
            chkRememberMapping.ForeColor = ModernTheme.Text;
            chkRememberMapping.Font = ModernTheme.BaseFont;

            btnOk.Text = "Importieren";
            btnAbbrechen.Text = "Abbrechen";
            btnOk.Location = new Point(578, 544);
            btnAbbrechen.Location = new Point(696, 544);
            btnOk.Size = new Size(108, 38);
            btnAbbrechen.Size = new Size(100, 38);
            btnOk.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            btnAbbrechen.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;

            ModernTheme.ApplyPrimaryButton(btnOk);
            ModernTheme.ApplySecondaryButton(btnAbbrechen);

            Controls.Add(header);
            Controls.Add(card);
            Controls.Add(chkRememberMapping);
            Controls.Add(btnOk);
            Controls.Add(btnAbbrechen);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Mapping.Clear();
            LearnedAliases.Clear();

            foreach (DataGridViewRow row in dgvMapping.Rows)
            {
                var csvCol = row.Cells[0].Value?.ToString();
                var targetDisplay = row.Cells[1].Value?.ToString();
                var targetCanonical = FieldMappingService.ToCanonical(_settings, targetDisplay);

                if (!string.IsNullOrWhiteSpace(csvCol) && !string.IsNullOrWhiteSpace(targetCanonical))
                {
                    Mapping[csvCol] = targetCanonical;

                    if (FieldMappingService.IsCustomTarget(targetCanonical))
                    {
                        FieldMappingService.AddCustomField(_settings, FieldMappingService.GetCustomFieldName(targetCanonical));
                    }
                    else
                    {
                        // Wenn der CSV-Spaltenname bisher nicht erkannt wurde oder anders benannt ist,
                        // kann er optional als Alias für spätere Importe gespeichert werden.
                        var alreadyKnown = FieldMappingService.ToCanonical(_settings, csvCol) == targetCanonical;
                        if (!alreadyKnown)
                            LearnedAliases[csvCol] = targetCanonical;
                    }
                }
            }

            if (!Mapping.ContainsValue("Artikelnummer"))
            {
                MessageBox.Show($"{_settings.GetFieldName("Artikelnummer")} muss zugeordnet werden!", "CSV-Import", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}
