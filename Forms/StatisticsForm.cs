using InventurApp.Models;
using InventurApp.Services;
using InventurApp.UI;
using System.Drawing.Drawing2D;

namespace InventurApp.Forms
{
    public class StatisticsForm : Form
    {
        private readonly StatisticsSnapshot _snapshot;
        private readonly TableLayoutPanel _root = new();

        public StatisticsForm(IEnumerable<Artikel> artikel)
        {
            _snapshot = new StatisticsService().CreateSnapshot(artikel);

            Text = "Statistik-Dashboard 2.0";
            ClientSize = new Size(1180, 760);
            MinimumSize = new Size(960, 640);
            ModernTheme.ApplyForm(this);

            BuildUi();
            Resize += (_, _) => ApplyResponsiveLayout();
            ApplyResponsiveLayout();
        }

        private void BuildUi()
        {
            Controls.Clear();

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 86,
                BackColor = ModernTheme.Background,
                Padding = new Padding(24, 18, 24, 8)
            };

            var title = new Label
            {
                Text = "Statistik-Dashboard 2.0\nBestände, Dokumente, Benutzeraktivität und Warnungen",
                Dock = DockStyle.Left,
                Width = 720,
                Font = ModernTheme.TitleFont,
                ForeColor = ModernTheme.Text,
                BackColor = Color.Transparent
            };

            var btnRefresh = new Button
            {
                Text = "↻ Aktualisieren",
                Dock = DockStyle.Right,
                Width = 140,
                Height = 38,
                Margin = new Padding(8)
            };
            btnRefresh.Click += (_, _) => DialogResult = DialogResult.Retry;
            ModernTheme.ApplySecondaryButton(btnRefresh);

            header.Controls.Add(btnRefresh);
            header.Controls.Add(title);
            Controls.Add(header);

            _root.Dock = DockStyle.Fill;
            _root.BackColor = ModernTheme.Background;
            _root.Padding = new Padding(24, 6, 24, 24);
            _root.ColumnCount = 4;
            _root.RowCount = 4;
            Controls.Add(_root);
            _root.BringToFront();

            AddMetricCard(0, 0, "📦 Artikel", _snapshot.ArtikelGesamt.ToString("N0"), $"{_snapshot.LagerorteGesamt:N0} Lagerorte");
            AddMetricCard(1, 0, "Σ Soll-Menge", _snapshot.GesamtMenge.ToString("N0"), "Gesamter Bestand");
            AddMetricCard(2, 0, "🧾 Dokumente", _snapshot.DokumenteGesamt.ToString("N0"), $"{_snapshot.ScansGesamt:N0} Scans");
            AddMetricCard(3, 0, "👥 Benutzer", _snapshot.AktiveBenutzer.ToString("N0"), $"{_snapshot.InaktiveBenutzer:N0} deaktiviert");

            AddChart(0, 1, 2, "Bestand nach Lagerort", _snapshot.Lagerorte, ChartKind.Bar);
            AddChart(2, 1, 2, "Dokumente nach Kategorie", _snapshot.DokumentKategorien, ChartKind.Donut);
            AddChart(0, 2, 2, "Top-Artikel nach Soll-Menge", _snapshot.TopArtikel, ChartKind.Bar);
            AddChart(2, 2, 2, "Dokumente letzte 14 Tage", _snapshot.DokumenteProTag, ChartKind.Line);

            AddListCard(0, 3, 2, "⚠ Hinweise", _snapshot.Warnungen);
            AddListCard(2, 3, 2, "🕘 Letzte Aktivitäten", _snapshot.Aktivitaeten);
        }

        private void AddMetricCard(int column, int row, string title, string value, string subtitle)
        {
            var panel = ModernTheme.CreateCardPanel();
            panel.Margin = new Padding(0, 0, 12, 12);
            panel.Padding = new Padding(18, 14, 18, 14);

            var lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 24,
                ForeColor = ModernTheme.MutedText,
                Font = ModernTheme.SubtitleFont,
                BackColor = Color.Transparent
            };

            var lblValue = new Label
            {
                Text = value,
                Dock = DockStyle.Top,
                Height = 36,
                ForeColor = ModernTheme.Text,
                Font = ModernTheme.MetricFont,
                BackColor = Color.Transparent
            };

            var lblSubtitle = new Label
            {
                Text = subtitle,
                Dock = DockStyle.Fill,
                ForeColor = ModernTheme.MutedText,
                Font = ModernTheme.SubtitleFont,
                BackColor = Color.Transparent
            };

            panel.Controls.Add(lblSubtitle);
            panel.Controls.Add(lblValue);
            panel.Controls.Add(lblTitle);
            _root.Controls.Add(panel, column, row);
        }

        private void AddChart(int column, int row, int span, string title, List<StatisticItem> data, ChartKind kind)
        {
            var chart = new ChartCard(title, data, kind)
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 12, 12)
            };
            _root.Controls.Add(chart, column, row);
            _root.SetColumnSpan(chart, span);
        }

        private void AddListCard(int column, int row, int span, string title, List<string> items)
        {
            var list = new ListBox
            {
                BorderStyle = BorderStyle.None,
                IntegralHeight = false,
                Dock = DockStyle.Fill
            };
            ModernTheme.ApplyInput(list);
            foreach (var item in items)
                list.Items.Add(item);

            var titleLabel = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 32,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = ModernTheme.Text,
                BackColor = Color.Transparent
            };

            var panel = ModernTheme.CreateCardPanel();
            panel.Margin = new Padding(0, 0, 12, 0);
            panel.Padding = new Padding(18, 14, 18, 14);
            panel.Controls.Add(list);
            panel.Controls.Add(titleLabel);
            _root.Controls.Add(panel, column, row);
            _root.SetColumnSpan(panel, span);
        }

        private void ApplyResponsiveLayout()
        {
            _root.SuspendLayout();
            _root.ColumnStyles.Clear();
            _root.RowStyles.Clear();

            var narrow = ClientSize.Width < 1050;
            _root.ColumnCount = narrow ? 2 : 4;
            _root.RowCount = 4;

            for (var i = 0; i < _root.ColumnCount; i++)
                _root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / _root.ColumnCount));

            _root.RowStyles.Add(new RowStyle(SizeType.Absolute, narrow ? 150 : 104));
            _root.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            _root.RowStyles.Add(new RowStyle(SizeType.Percent, 33));
            _root.RowStyles.Add(new RowStyle(SizeType.Percent, 34));

            _root.ResumeLayout();
        }

        private enum ChartKind { Bar, Donut, Line }

        private class ChartCard : Panel
        {
            private readonly string _title;
            private readonly List<StatisticItem> _data;
            private readonly ChartKind _kind;

            public ChartCard(string title, List<StatisticItem> data, ChartKind kind)
            {
                _title = title;
                _data = data.Count > 0 ? data : new List<StatisticItem> { new() { Name = "Keine Daten", Wert = 0 } };
                _kind = kind;
                BackColor = ModernTheme.Surface;
                Padding = new Padding(18, 14, 18, 14);
                Resize += (_, _) => Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                DrawCardBorder(e.Graphics);
                DrawTitle(e.Graphics);

                var plot = new Rectangle(22, 52, Width - 44, Height - 74);
                if (plot.Width < 50 || plot.Height < 50)
                    return;

                switch (_kind)
                {
                    case ChartKind.Bar:
                        DrawBars(e.Graphics, plot);
                        break;
                    case ChartKind.Donut:
                        DrawDonut(e.Graphics, plot);
                        break;
                    case ChartKind.Line:
                        DrawLine(e.Graphics, plot);
                        break;
                }
            }

            private void DrawCardBorder(Graphics graphics)
            {
                using var pen = new Pen(ModernTheme.Border, 1);
                using var path = RoundedRectangle(new Rectangle(0, 0, Width - 1, Height - 1), 16);
                graphics.DrawPath(pen, path);
            }

            private void DrawTitle(Graphics graphics)
            {
                using var titleBrush = new SolidBrush(ModernTheme.Text);
                using var subBrush = new SolidBrush(ModernTheme.MutedText);
                using var titleFont = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
                using var subFont = new Font("Segoe UI", 8.5F);
                graphics.DrawString(_title, titleFont, titleBrush, new PointF(18, 14));
                graphics.DrawString($"Aktualisiert: {DateTime.Now:dd.MM.yyyy HH:mm}", subFont, subBrush, new PointF(18, 34));
            }

            private void DrawBars(Graphics graphics, Rectangle plot)
            {
                var max = Math.Max(1, _data.Max(x => x.Wert));
                var barHeight = Math.Max(18, Math.Min(34, plot.Height / Math.Max(1, _data.Count) - 8));
                var y = plot.Top;
                using var labelBrush = new SolidBrush(ModernTheme.Text);
                using var mutedBrush = new SolidBrush(ModernTheme.MutedText);
                using var barBrush = new SolidBrush(ModernTheme.Primary);
                using var backBrush = new SolidBrush(ModernTheme.SurfaceAlt);
                using var font = new Font("Segoe UI", 8.5F);

                foreach (var item in _data.Take(8))
                {
                    var labelWidth = Math.Min(150, plot.Width / 3);
                    var barLeft = plot.Left + labelWidth + 8;
                    var barWidth = Math.Max(20, plot.Width - labelWidth - 72);
                    var filled = (int)(barWidth * (item.Wert / (double)max));
                    var barRect = new Rectangle(barLeft, y + 4, barWidth, barHeight);
                    var fillRect = new Rectangle(barLeft, y + 4, Math.Max(4, filled), barHeight);

                    graphics.DrawString(Trim(item.Name, 24), font, labelBrush, new RectangleF(plot.Left, y + 3, labelWidth, barHeight + 4));
                    graphics.FillRoundedRectangle(backBrush, barRect, 8);
                    graphics.FillRoundedRectangle(barBrush, fillRect, 8);
                    graphics.DrawString(item.Wert.ToString("N0"), font, mutedBrush, new PointF(barLeft + barWidth + 8, y + 4));
                    y += barHeight + 10;
                }
            }

            private void DrawDonut(Graphics graphics, Rectangle plot)
            {
                var total = _data.Sum(x => Math.Max(0, x.Wert));
                if (total <= 0)
                {
                    DrawEmpty(graphics, plot);
                    return;
                }

                var size = Math.Min(plot.Height - 10, plot.Width / 2);
                var donut = new Rectangle(plot.Left + 6, plot.Top + 8, size, size);
                var start = -90f;
                var palette = CreatePalette();
                using var holeBrush = new SolidBrush(ModernTheme.Surface);

                for (var i = 0; i < _data.Count; i++)
                {
                    var sweep = (float)(360.0 * _data[i].Wert / total);
                    using var brush = new SolidBrush(palette[i % palette.Count]);
                    graphics.FillPie(brush, donut, start, sweep);
                    start += sweep;
                }

                var hole = Rectangle.Inflate(donut, -donut.Width / 4, -donut.Height / 4);
                graphics.FillEllipse(holeBrush, hole);

                using var labelBrush = new SolidBrush(ModernTheme.Text);
                using var mutedBrush = new SolidBrush(ModernTheme.MutedText);
                using var font = new Font("Segoe UI", 8.5F);
                var x = donut.Right + 22;
                var y = plot.Top + 8;
                for (var i = 0; i < _data.Take(7).Count(); i++)
                {
                    var item = _data[i];
                    using var brush = new SolidBrush(palette[i % palette.Count]);
                    graphics.FillRoundedRectangle(brush, new Rectangle(x, y + 3, 12, 12), 4);
                    graphics.DrawString($"{Trim(item.Name, 20)} · {item.Wert:N0}", font, labelBrush, new PointF(x + 18, y));
                    y += 22;
                }
            }

            private void DrawLine(Graphics graphics, Rectangle plot)
            {
                var data = _data.TakeLast(14).ToList();
                var max = Math.Max(1, data.Max(x => x.Wert));
                using var gridPen = new Pen(ModernTheme.Border, 1);
                using var linePen = new Pen(ModernTheme.Primary, 3);
                using var pointBrush = new SolidBrush(ModernTheme.Primary);
                using var textBrush = new SolidBrush(ModernTheme.MutedText);
                using var font = new Font("Segoe UI", 8F);

                for (var i = 0; i < 4; i++)
                {
                    var y = plot.Top + i * plot.Height / 3;
                    graphics.DrawLine(gridPen, plot.Left, y, plot.Right, y);
                }

                if (data.Count == 0)
                {
                    DrawEmpty(graphics, plot);
                    return;
                }

                var points = data.Select((item, index) =>
                {
                    var x = data.Count == 1 ? plot.Left + plot.Width / 2 : plot.Left + index * plot.Width / (data.Count - 1);
                    var y = plot.Bottom - (int)(plot.Height * (item.Wert / (double)max));
                    return new Point(x, y);
                }).ToArray();

                if (points.Length > 1)
                    graphics.DrawLines(linePen, points);

                for (var i = 0; i < points.Length; i++)
                {
                    graphics.FillEllipse(pointBrush, points[i].X - 4, points[i].Y - 4, 8, 8);
                    if (i == 0 || i == points.Length - 1 || i % 3 == 0)
                        graphics.DrawString(data[i].Name, font, textBrush, new PointF(points[i].X - 16, plot.Bottom + 4));
                }
            }

            private void DrawEmpty(Graphics graphics, Rectangle plot)
            {
                using var brush = new SolidBrush(ModernTheme.MutedText);
                using var font = new Font("Segoe UI", 10F);
                graphics.DrawString("Keine Daten vorhanden", font, brush, plot);
            }

            private static List<Color> CreatePalette() => new()
            {
                ModernTheme.Primary,
                ModernTheme.Success,
                ModernTheme.Warning,
                Color.FromArgb(139, 92, 246),
                Color.FromArgb(14, 165, 233),
                Color.FromArgb(236, 72, 153),
                Color.FromArgb(100, 116, 139)
            };

            private static string Trim(string value, int max)
            {
                if (string.IsNullOrWhiteSpace(value)) return "-";
                return value.Length <= max ? value : value[..Math.Max(0, max - 1)] + "…";
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

    internal static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int radius)
        {
            using var path = RoundedRectangle(bounds, radius);
            graphics.FillPath(brush, path);
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
