namespace InventurApp.Forms
{
    partial class ArtikelEditForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.TextBox txtArtikelnummer;
        private System.Windows.Forms.TextBox txtLagerort;
        private System.Windows.Forms.NumericUpDown numSollMenge;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnAbbrechen;
        private System.Windows.Forms.Label lblArtikelnummer;
        private System.Windows.Forms.Label lblBezeichnung;
        private System.Windows.Forms.Label lblLagerort;
        private System.Windows.Forms.Label lblSollMenge;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            txtArtikelnummer = new TextBox();
            txtLagerort = new TextBox();
            numSollMenge = new NumericUpDown();
            btnOK = new Button();
            btnAbbrechen = new Button();
            lblArtikelnummer = new Label();
            lblBezeichnung = new Label();
            lblLagerort = new Label();
            lblSollMenge = new Label();
            cmbBezeichnung = new ComboBox();
            ((System.ComponentModel.ISupportInitialize)numSollMenge).BeginInit();
            SuspendLayout();
            // 
            // txtArtikelnummer
            // 
            txtArtikelnummer.Location = new Point(120, 12);
            txtArtikelnummer.Name = "txtArtikelnummer";
            txtArtikelnummer.Size = new Size(200, 23);
            txtArtikelnummer.TabIndex = 1;
            // 
            // txtLagerort
            // 
            txtLagerort.Location = new Point(120, 82);
            txtLagerort.Name = "txtLagerort";
            txtLagerort.Size = new Size(200, 23);
            txtLagerort.TabIndex = 5;
            // 
            // numSollMenge
            // 
            numSollMenge.Location = new Point(120, 118);
            numSollMenge.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numSollMenge.Name = "numSollMenge";
            numSollMenge.Size = new Size(120, 23);
            numSollMenge.TabIndex = 7;
            // 
            // btnOK
            // 
            btnOK.Location = new Point(120, 160);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(90, 30);
            btnOK.TabIndex = 8;
            btnOK.Text = "OK";
            btnOK.Click += btnOK_Click;
            // 
            // btnAbbrechen
            // 
            btnAbbrechen.Location = new Point(230, 160);
            btnAbbrechen.Name = "btnAbbrechen";
            btnAbbrechen.Size = new Size(90, 30);
            btnAbbrechen.TabIndex = 9;
            btnAbbrechen.Text = "Abbrechen";
            btnAbbrechen.Click += btnAbbrechen_Click;
            // 
            // lblArtikelnummer
            // 
            lblArtikelnummer.AutoSize = true;
            lblArtikelnummer.Location = new Point(12, 15);
            lblArtikelnummer.Name = "lblArtikelnummer";
            lblArtikelnummer.Size = new Size(87, 15);
            lblArtikelnummer.TabIndex = 0;
            lblArtikelnummer.Text = "Artikelnummer";
            // 
            // lblBezeichnung
            // 
            lblBezeichnung.AutoSize = true;
            lblBezeichnung.Location = new Point(12, 50);
            lblBezeichnung.Name = "lblBezeichnung";
            lblBezeichnung.Size = new Size(75, 15);
            lblBezeichnung.TabIndex = 2;
            lblBezeichnung.Text = "Bezeichnung";
            // 
            // lblLagerort
            // 
            lblLagerort.AutoSize = true;
            lblLagerort.Location = new Point(12, 85);
            lblLagerort.Name = "lblLagerort";
            lblLagerort.Size = new Size(51, 15);
            lblLagerort.TabIndex = 4;
            lblLagerort.Text = "Lagerort";
            // 
            // lblSollMenge
            // 
            lblSollMenge.AutoSize = true;
            lblSollMenge.Location = new Point(12, 120);
            lblSollMenge.Name = "lblSollMenge";
            lblSollMenge.Size = new Size(68, 15);
            lblSollMenge.TabIndex = 6;
            lblSollMenge.Text = "Soll-Menge";
            // 
            // cmbBezeichnung
            // 
            cmbBezeichnung.FormattingEnabled = true;
            cmbBezeichnung.Location = new Point(120, 47);
            cmbBezeichnung.Name = "cmbBezeichnung";
            cmbBezeichnung.Size = new Size(200, 23);
            cmbBezeichnung.TabIndex = 10;
            // 
            // ArtikelEditForm
            // 
            ClientSize = new Size(350, 215);
            Controls.Add(cmbBezeichnung);
            Controls.Add(lblArtikelnummer);
            Controls.Add(txtArtikelnummer);
            Controls.Add(lblBezeichnung);
            Controls.Add(lblLagerort);
            Controls.Add(txtLagerort);
            Controls.Add(lblSollMenge);
            Controls.Add(numSollMenge);
            Controls.Add(btnOK);
            Controls.Add(btnAbbrechen);
            Name = "ArtikelEditForm";
            Text = "Artikel bearbeiten";
            ((System.ComponentModel.ISupportInitialize)numSollMenge).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private ComboBox cmbBezeichnung;
    }
}
