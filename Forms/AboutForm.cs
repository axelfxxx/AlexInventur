using InventurApp.Services;
using InventurApp.UI;
using System.Diagnostics;

namespace InventurApp.Forms
{
    public class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            ModernTheme.ApplyForm(this);
            Text = $"Über {AppInfoService.ProductName}";
            ClientSize = new Size(720, 430);
            MinimumSize = new Size(680, 400);
            ShowInTaskbar = false;
            MinimizeBox = false;
            MaximizeBox = false;

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(24),
                BackColor = ModernTheme.Background
            };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 134));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            Controls.Add(root);

            var header = ModernTheme.CreateCardPanel();
            header.Dock = DockStyle.Fill;
            header.Padding = new Padding(20);
            root.Controls.Add(header, 0, 0);

            var headerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ModernTheme.Surface
            };
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            header.Controls.Add(headerLayout);

            var iconBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = ModernTheme.Surface
            };
            try
            {
                iconBox.Image = Icon?.ToBitmap();
            }
            catch
            {
                // Fallback: kein Bild nötig.
            }
            headerLayout.Controls.Add(iconBox, 0, 0);

            var title = new Label
            {
                Text = $"{AppInfoService.ProductName}\nInventur-, Geräte- und Dokumentenverwaltung",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = ModernTheme.TitleFont,
                ForeColor = ModernTheme.Text,
                BackColor = ModernTheme.Surface
            };
            headerLayout.Controls.Add(title, 1, 0);

            var card = ModernTheme.CreateCardPanel();
            card.Dock = DockStyle.Fill;
            card.Padding = new Padding(22);
            root.Controls.Add(card, 0, 1);

            var info = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                RowCount = 6,
                Height = 252,
                BackColor = ModernTheme.Surface
            };
            info.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
            info.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (var i = 0; i < 6; i++)
                info.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            card.Controls.Add(info);

            AddInfoRow(info, "Version", AppInfoService.CurrentVersionText, 0);
            AddInfoRow(info, "Build", AppInfoService.BuildConfiguration, 1);
            AddInfoRow(info, "Produkt", AppInfoService.ProductName, 2);
            AddInfoRow(info, "Beschreibung", AppInfoService.Description, 3);
            AddInfoRow(info, "Datenordner", AppInfoService.DataDirectory, 4);
            AddInfoRow(info, "Installation", AppInfoService.InstallDirectory, 5);

            var actions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = ModernTheme.Background
            };
            root.Controls.Add(actions, 0, 2);

            var btnClose = new Button { Text = "Schließen", Width = 118, Height = 38, DialogResult = DialogResult.OK };
            var btnData = new Button { Text = "📁 Datenordner", Width = 136, Height = 38 };
            btnData.Click += (_, _) => AppInfoService.OpenDataDirectory();
            ModernTheme.ApplyPrimaryButton(btnClose);
            ModernTheme.ApplySecondaryButton(btnData);
            actions.Controls.Add(btnClose);
            actions.Controls.Add(btnData);
            AcceptButton = btnClose;
        }

        private static void AddInfoRow(TableLayoutPanel table, string labelText, string valueText, int row)
        {
            var label = new Label { Text = labelText, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            var value = new TextBox { Text = valueText, Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None };
            ModernTheme.ApplyLabel(label, muted: true);
            ModernTheme.ApplyInput(value);
            table.Controls.Add(label, 0, row);
            table.Controls.Add(value, 1, row);
        }
    }
}
