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

        public RazorCard()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
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
                    c.Top += 6;
                }
                _controlsShifted = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(this.Parent?.BackColor ?? RazorTheme.Colors.CurrentBackground);

            // Emulate Border Right
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, _borderRadius * 2, _borderRadius * 2, 180, 90);
                path.AddArc(rect.Right - (_borderRadius * 2), rect.Y, _borderRadius * 2, _borderRadius * 2, 270, 90);
                path.AddArc(rect.Right - (_borderRadius * 2), rect.Bottom - (_borderRadius * 2), _borderRadius * 2, _borderRadius * 2, 0, 90);
                path.AddArc(rect.X, rect.Bottom - (_borderRadius * 2), _borderRadius * 2, _borderRadius * 2, 90, 90);
                path.CloseFigure();

                using (SolidBrush brush = new SolidBrush(RazorTheme.Colors.CurrentCard))
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

            // Draw Text (Title)
            if (!string.IsNullOrEmpty(this.Text))
            {
                using (Font titleFont = new Font(this.Font.FontFamily, 10f, FontStyle.Bold))
                {
                    TextRenderer.DrawText(e.Graphics, this.Text, titleFont, new Point(16, 12), this.ForeColor);
                }
            }
        }
    }
}
