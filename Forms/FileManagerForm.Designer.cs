namespace InventurApp.Forms
{
    partial class FileManagerForm
    {
        private System.ComponentModel.IContainer components = null;

        private DataGridView dgvFiles;
        private Button btnOpen;
        private Button btnDelete;
        private Button btnRefresh;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel lblStatus;
        private TextBox txtSearch;
        private Label lblSearch;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            dgvFiles = new DataGridView();
            btnOpen = new Button();
            btnDelete = new Button();
            btnRefresh = new Button();
            statusStrip1 = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            txtSearch = new TextBox();
            lblSearch = new Label();

            ((System.ComponentModel.ISupportInitialize)dgvFiles).BeginInit();
            statusStrip1.SuspendLayout();
            SuspendLayout();

            // GRID
            dgvFiles.Location = new Point(12, 40);
            dgvFiles.Size = new Size(560, 180);
            dgvFiles.Name = "dgvFiles";

            // SUCHLABEL
            lblSearch.Text = "Suche:";
            lblSearch.Location = new Point(12, 12);

            // SUCHFELD
            txtSearch.Location = new Point(70, 10);
            txtSearch.Size = new Size(200, 23);
            txtSearch.TextChanged += txtSearch_TextChanged;

            // BUTTONS
            btnOpen.Text = "Öffnen";
            btnOpen.Location = new Point(12, 230);
            btnOpen.Click += btnOpen_Click;

            btnDelete.Text = "Löschen";
            btnDelete.Location = new Point(100, 230);
            btnDelete.Click += btnDelete_Click;

            btnRefresh.Text = "Aktualisieren";
            btnRefresh.Location = new Point(200, 230);
            btnRefresh.Click += btnRefresh_Click;

            // STATUS
            statusStrip1.Items.Add(lblStatus);
            statusStrip1.Location = new Point(0, 270);
            lblStatus.Text = "...";

            // FORM
            ClientSize = new Size(600, 300);
            Controls.Add(dgvFiles);
            Controls.Add(btnOpen);
            Controls.Add(btnDelete);
            Controls.Add(btnRefresh);
            Controls.Add(statusStrip1);
            Controls.Add(txtSearch);
            Controls.Add(lblSearch);

            Text = "File Manager";

            ((System.ComponentModel.ISupportInitialize)dgvFiles).EndInit();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
