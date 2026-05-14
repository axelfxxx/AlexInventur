namespace InventurApp.Forms
{
    partial class ArtikelForm
    {
        private System.ComponentModel.IContainer components = null;

        private DataGridView dgvArtikel;
        private Button btnNeu;
        private Button btnLöschen;
        private Button btnExportCsv;
        private Button btnImportCsv;
        private Button btnDateimanager;
        private Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            dgvArtikel = new DataGridView();
            btnNeu = new Button();
            btnLöschen = new Button();
            btnExportCsv = new Button();
            btnImportCsv = new Button();
            btnDateimanager = new Button();
            lblStatus = new Label();

            ((System.ComponentModel.ISupportInitialize)dgvArtikel).BeginInit();
            SuspendLayout();

            // 
            // dgvArtikel
            // 
            dgvArtikel.Location = new Point(12, 60);
            dgvArtikel.Name = "dgvArtikel";
            dgvArtikel.Size = new Size(760, 350);
            dgvArtikel.TabIndex = 0;
            dgvArtikel.ReadOnly = false;

            // 
            // btnNeu
            // 
            btnNeu.Location = new Point(12, 12);
            btnNeu.Name = "btnNeu";
            btnNeu.Size = new Size(100, 30);
            btnNeu.Text = "Neu";
            btnNeu.UseVisualStyleBackColor = true;
            btnNeu.Click += btnNeu_Click;

            // 
            // btnLöschen
            // 
            btnLöschen.Location = new Point(118, 12);
            btnLöschen.Name = "btnLöschen";
            btnLöschen.Size = new Size(100, 30);
            btnLöschen.Text = "Löschen";
            btnLöschen.UseVisualStyleBackColor = true;
            btnLöschen.Click += btnLöschen_Click;

            // 
            // btnExportCsv
            // 
            btnExportCsv.Location = new Point(224, 12);
            btnExportCsv.Name = "btnExportCsv";
            btnExportCsv.Size = new Size(100, 30);
            btnExportCsv.Text = "Export";
            btnExportCsv.UseVisualStyleBackColor = true;
            btnExportCsv.Click += btnExportCsv_Click;

            // 
            // btnImportCsv
            // 
            btnImportCsv.Location = new Point(330, 12);
            btnImportCsv.Name = "btnImportCsv";
            btnImportCsv.Size = new Size(100, 30);
            btnImportCsv.Text = "Import";
            btnImportCsv.UseVisualStyleBackColor = true;
            btnImportCsv.Click += btnImportCsv_Click;

            // 
            // btnDateimanager
            // 
            btnDateimanager.Location = new Point(436, 12);
            btnDateimanager.Name = "btnDateimanager";
            btnDateimanager.Size = new Size(120, 30);
            btnDateimanager.Text = "Dateimanager";
            btnDateimanager.UseVisualStyleBackColor = true;
            btnDateimanager.Click += btnDateimanager_Click;

            // 
            // lblStatus
            // 
            lblStatus.Location = new Point(12, 420);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(760, 23);
            lblStatus.Text = "Bereit";

            // 
            // ArtikelForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 461);
            Controls.Add(lblStatus);
            Controls.Add(btnDateimanager);
            Controls.Add(btnImportCsv);
            Controls.Add(btnExportCsv);
            Controls.Add(btnLöschen);
            Controls.Add(btnNeu);
            Controls.Add(dgvArtikel);
            Name = "ArtikelForm";
            Text = "InventurApp - Artikelverwaltung";

            ((System.ComponentModel.ISupportInitialize)dgvArtikel).EndInit();
            ResumeLayout(false);
        }

        #endregion
    }
}
