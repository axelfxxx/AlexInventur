using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace InventurApp.Forms
{
    partial class ImportMappingForm
    {
        private System.ComponentModel.IContainer components = null;
        private DataGridView dgvMapping;
        private Button btnOk;
        private Button btnAbbrechen;
        private CheckBox chkRememberMapping;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            dgvMapping = new DataGridView();
            btnOk = new Button();
            btnAbbrechen = new Button();
            chkRememberMapping = new CheckBox();

            SuspendLayout();

            // dgvMapping
            dgvMapping.Location = new Point(12, 12);
            dgvMapping.Size = new Size(460, 250);
            dgvMapping.AllowUserToAddRows = false;
            dgvMapping.RowHeadersVisible = false;
            dgvMapping.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMapping.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgvMapping.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "CSV-Spalte",
                ReadOnly = true,
                Name = "CsvColumn"
            });

            dgvMapping.Columns.Add(new DataGridViewComboBoxColumn
            {
                HeaderText = "Ziel-Feld",
                Name = "TargetField",
                DataSource = new string[]
                {
                    "",
                    "Artikelnummer",
                    "Bezeichnung",
                    "Lagerort",
                    "Soll-Menge"
                }
            });

            // chkRememberMapping
            chkRememberMapping.Text = "Zuordnung für zukünftige Importe merken";
            chkRememberMapping.Checked = true;
            chkRememberMapping.Location = new Point(12, 270);
            chkRememberMapping.Size = new Size(250, 28);

            // btnOk
            btnOk.Text = "Importieren";
            btnOk.Location = new Point(272, 275);
            btnOk.Size = new Size(95, 30);
            btnOk.Click += btnOk_Click;

            // btnAbbrechen
            btnAbbrechen.Text = "Abbrechen";
            btnAbbrechen.Location = new Point(377, 275);
            btnAbbrechen.Size = new Size(95, 30);
            btnAbbrechen.Click += (s, e) => DialogResult = DialogResult.Cancel;

            // Form
            ClientSize = new Size(484, 321);
            Controls.Add(dgvMapping);
            Controls.Add(chkRememberMapping);
            Controls.Add(btnOk);
            Controls.Add(btnAbbrechen);
            Text = "CSV-Spalten zuordnen";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            ResumeLayout(false);
        }
    }
}
