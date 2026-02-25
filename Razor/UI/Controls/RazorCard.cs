using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Assistant.UI.Controls
{
    public class RazorCard : GroupBox
    {
        private Color _borderColor = RazorTheme.Colors.GlowViolet;
        private int _borderRadius = 12;

        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; this.Invalidate(); }
        }

        public RazorCard()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            // Expose the actual drawn background colour so child controls
            // (e.g. RazorToggle) that read Parent.BackColor get the right value
            // and don't paint a mismatched rectangle (the "glow" effect).
            this.BackColor = RazorTheme.Colors.CurrentCard;
        }

        private bool _controlsShifted = false;
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!_controlsShifted && !this.DesignMode)
            {
                foreach (Control c in this.Controls)
                {
                    c.Left += 8;
                    c.Top += 2; // Ridotto da 6 a 2
                }
                _controlsShifted = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Emulate Border Right
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            int radius = 12;
            
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                path.AddArc(rect.Right - (radius * 2), rect.Y, radius * 2, radius * 2, 270, 90);
                path.AddArc(rect.Right - (radius * 2), rect.Bottom - (radius * 2), radius * 2, radius * 2, 0, 90);
                path.AddArc(rect.X, rect.Bottom - (radius * 2), radius * 2, radius * 2, 90, 90);
                path.CloseFigure();

                // Fill with slightly transparent card color to let gradient show through or use CurrentCard
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(180, RazorTheme.Colors.CurrentCard)))
                {
                    e.Graphics.FillPath(brush, path);
                }
            }

            // Draw glowing left border
            using (SolidBrush borderBrush = new SolidBrush(_borderColor))
            {
                // Make the glowing bar a bit shorter than the full height and rounded
                e.Graphics.FillRectangle(borderBrush, 0, _borderRadius + 8, 4, Math.Max(this.Height - (_borderRadius * 2) - 16, 12));
            }

            // Draw Text (Title) — support "icon  title" pattern where icon uses Segoe MDL2 Assets
            if (!string.IsNullOrEmpty(this.Text))
            {
                int sepIdx = this.Text.IndexOf("  "); // double-space separates icon from title
                if (sepIdx > 0)
                {
                    string iconPart  = this.Text.Substring(0, sepIdx);
                    string titlePart = this.Text.Substring(sepIdx + 2);
                    using (Font mdl2Font  = new Font("Segoe MDL2 Assets", 11f))
                    using (Font titleFont = new Font(this.Font.FontFamily, 10f, FontStyle.Bold))
                    {
                        Size iconSz = TextRenderer.MeasureText(e.Graphics, iconPart, mdl2Font,
                            new Size(40, 28), TextFormatFlags.NoPadding);
                        TextRenderer.DrawText(e.Graphics, iconPart,  mdl2Font,  new Point(14, 8), this.ForeColor, TextFormatFlags.NoPadding); // Y da 10 a 8
                        TextRenderer.DrawText(e.Graphics, titlePart, titleFont, new Point(14 + iconSz.Width + 4, 9), this.ForeColor, TextFormatFlags.NoPadding); // Y da 11 a 9
                    }
                }
                else
                {
                    using (Font titleFont = new Font(this.Font.FontFamily, 10f, FontStyle.Bold))
                    {
                        TextRenderer.DrawText(e.Graphics, this.Text, titleFont, new Point(16, 10), this.ForeColor); // Y da 12 a 10
                    }
                }
            }
        }
    }
}
