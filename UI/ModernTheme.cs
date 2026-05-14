using System.Drawing.Drawing2D;

namespace InventurApp.UI
{
    internal static class ModernTheme
    {
        public static bool IsDarkMode { get; private set; }

        public static Color Background { get; private set; } = Color.FromArgb(244, 247, 251);
        public static Color Surface { get; private set; } = Color.White;
        public static Color SurfaceAlt { get; private set; } = Color.FromArgb(238, 243, 249);
        public static Color Sidebar { get; private set; } = Color.FromArgb(17, 24, 39);
        public static Color SidebarHover { get; private set; } = Color.FromArgb(31, 41, 55);
        public static Color Border { get; private set; } = Color.FromArgb(213, 222, 234);
        public static Color Text { get; private set; } = Color.FromArgb(31, 41, 55);
        public static Color MutedText { get; private set; } = Color.FromArgb(107, 114, 128);
        public static Color Primary { get; private set; } = Color.FromArgb(37, 99, 235);
        public static Color PrimaryHover { get; private set; } = Color.FromArgb(29, 78, 216);
        public static Color Danger { get; private set; } = Color.FromArgb(220, 38, 38);
        public static Color DangerHover { get; private set; } = Color.FromArgb(185, 28, 28);
        public static Color Success { get; private set; } = Color.FromArgb(22, 163, 74);
        public static Color Warning { get; private set; } = Color.FromArgb(217, 119, 6);

        public static void SetDarkMode(bool enabled)
        {
            IsDarkMode = enabled;
            if (enabled)
            {
                Background = Color.FromArgb(15, 23, 42);
                Surface = Color.FromArgb(30, 41, 59);
                SurfaceAlt = Color.FromArgb(51, 65, 85);
                Sidebar = Color.FromArgb(2, 6, 23);
                SidebarHover = Color.FromArgb(30, 41, 59);
                Border = Color.FromArgb(71, 85, 105);
                Text = Color.FromArgb(226, 232, 240);
                MutedText = Color.FromArgb(148, 163, 184);
                Primary = Color.FromArgb(59, 130, 246);
                PrimaryHover = Color.FromArgb(37, 99, 235);
            }
            else
            {
                Background = Color.FromArgb(244, 247, 251);
                Surface = Color.White;
                SurfaceAlt = Color.FromArgb(238, 243, 249);
                Sidebar = Color.FromArgb(17, 24, 39);
                SidebarHover = Color.FromArgb(31, 41, 55);
                Border = Color.FromArgb(213, 222, 234);
                Text = Color.FromArgb(31, 41, 55);
                MutedText = Color.FromArgb(107, 114, 128);
                Primary = Color.FromArgb(37, 99, 235);
                PrimaryHover = Color.FromArgb(29, 78, 216);
            }
        }

        public static readonly Font BaseFont = new("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font TitleFont = new("Segoe UI Semibold", 20F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font SubtitleFont = new("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        public static readonly Font ButtonFont = new("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font GridHeaderFont = new("Segoe UI Semibold", 9.5F, FontStyle.Bold, GraphicsUnit.Point);
        public static readonly Font MetricFont = new("Segoe UI Semibold", 18F, FontStyle.Bold, GraphicsUnit.Point);

        public static void ApplyForm(Form form)
        {
            form.BackColor = Background;
            form.Font = BaseFont;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimumSize = new Size(Math.Max(form.MinimumSize.Width, 820), Math.Max(form.MinimumSize.Height, 600));

            try
            {
                var appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (appIcon != null)
                    form.Icon = appIcon;
            }
            catch
            {
                // Icon ist rein kosmetisch. Bei Designer-/Testläufen darf das niemals blockieren.
            }
        }

        public static Label CreateTitleLabel(string title, string subtitle)
        {
            return new Label
            {
                AutoSize = false,
                Text = $"{title}\n{subtitle}",
                Font = TitleFont,
                ForeColor = Text,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 0, 0),
                Height = 72,
                Dock = DockStyle.Top
            };
        }

        public static Panel CreateCard(Control child, Padding padding)
        {
            var card = new Panel
            {
                BackColor = Surface,
                Padding = padding
            };

            child.Dock = DockStyle.Fill;
            card.Controls.Add(child);
            card.Paint += (_, e) => DrawBorder(card, e.Graphics, 16, Border);
            card.Resize += (_, _) => card.Invalidate();
            return card;
        }

        public static Panel CreateCardPanel()
        {
            var card = new Panel
            {
                BackColor = Surface,
                Padding = new Padding(18)
            };
            card.Paint += (_, e) => DrawBorder(card, e.Graphics, 16, Border);
            card.Resize += (_, _) => card.Invalidate();
            return card;
        }

        public static void ApplyPrimaryButton(Button button) => ApplyButton(button, Primary, PrimaryHover, Color.White);
        public static void ApplySecondaryButton(Button button) => ApplyButton(button, SurfaceAlt, Border, Text);
        public static void ApplyDangerButton(Button button) => ApplyButton(button, Danger, DangerHover, Color.White);
        public static void ApplySidebarButton(Button button) => ApplyButton(button, Sidebar, SidebarHover, Color.White);

        public static void ApplyButton(Button button, Color backColor, Color hoverColor, Color foreColor)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.Font = ButtonFont;
            button.Height = Math.Max(button.Height, 38);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
            button.FlatAppearance.BorderSize = backColor == SurfaceAlt ? 1 : 0;
            button.FlatAppearance.BorderColor = Border;
            button.FlatAppearance.MouseOverBackColor = hoverColor;
            button.FlatAppearance.MouseDownBackColor = hoverColor;
        }

        public static void ApplyGrid(DataGridView grid)
        {
            grid.BackgroundColor = Surface;
            grid.BorderStyle = BorderStyle.None;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.GridColor = Border;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersDefaultCellStyle.BackColor = SurfaceAlt;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Text;
            grid.ColumnHeadersDefaultCellStyle.Font = GridHeaderFont;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            grid.ColumnHeadersHeight = 42;
            grid.RowTemplate.Height = 38;
            grid.DefaultCellStyle.BackColor = Surface;
            grid.DefaultCellStyle.ForeColor = Text;
            grid.DefaultCellStyle.SelectionBackColor = IsDarkMode ? Color.FromArgb(30, 64, 175) : Color.FromArgb(219, 234, 254);
            grid.DefaultCellStyle.SelectionForeColor = IsDarkMode ? Color.White : Text;
            grid.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
            grid.AlternatingRowsDefaultCellStyle.BackColor = IsDarkMode ? Color.FromArgb(15, 23, 42) : Color.FromArgb(249, 250, 251);
            grid.RowHeadersVisible = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.AllowUserToResizeRows = false;
        }

        public static void ApplyInput(Control control)
        {
            control.BackColor = Surface;
            control.ForeColor = Text;
            control.Font = BaseFont;
        }

        public static void ApplyLabel(Label label, bool muted = false)
        {
            label.ForeColor = muted ? MutedText : Text;
            label.BackColor = Color.Transparent;
            label.Font = muted ? SubtitleFont : BaseFont;
        }

        public static void ApplyStatus(Label label)
        {
            label.BackColor = Surface;
            label.ForeColor = MutedText;
            label.BorderStyle = BorderStyle.None;
            label.Padding = new Padding(12, 6, 12, 6);
        }

        private static void DrawBorder(Control control, Graphics graphics, int radius, Color borderColor)
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var pen = new Pen(borderColor, 1);
            using var path = RoundedRectangle(new Rectangle(0, 0, control.Width - 1, control.Height - 1), radius);
            graphics.DrawPath(pen, path);
        }

        private static GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
        {
            var diameter = Math.Max(2, radius * 2);
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
