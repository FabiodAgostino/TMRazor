using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Assistant.UI.Controls
{
    public class RazorButton : Button
    {
        private Color _primaryColor = RazorTheme.Colors.Primary;
        private Color _hoverColor = ColorTranslator.FromHtml("#EA580C"); // orange-600

        public RazorButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = _primaryColor;
            this.ForeColor = Color.White;
            this.Font = new Font("Inter", 9F, FontStyle.Bold);
            this.Cursor = Cursors.Hand;
            this.Size = new Size(130, 36);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Define colors
            Color backColor = this.Enabled ? (this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)) ? _hoverColor : _primaryColor) : Color.Gray;
            
            // Draw background
            using (GraphicsPath path = new GraphicsPath())
            {
                int radius = 8;
                path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                path.AddArc(this.Width - (radius * 2), 0, radius * 2, radius * 2, 270, 90);
                path.AddArc(this.Width - (radius * 2), this.Height - (radius * 2), radius * 2, radius * 2, 0, 90);
                path.AddArc(0, this.Height - (radius * 2), radius * 2, radius * 2, 90, 90);
                path.CloseFigure();

                using (SolidBrush brush = new SolidBrush(backColor))
                {
                    pevent.Graphics.FillPath(brush, path);
                }
            }

            // Draw text
            TextRenderer.DrawText(pevent.Graphics, this.Text, this.Font, this.ClientRectangle, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            this.Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.Invalidate();
        }
    }

    public class RazorOutlineButton : RazorButton
    {
        public RazorOutlineButton()
        {
            this.BackColor = RazorTheme.IsDark ? RazorTheme.Colors.CardDark : Color.White;
            this.ForeColor = RazorTheme.IsDark ? RazorTheme.Colors.TextDarkMode : RazorTheme.Colors.TextLightMode;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Color borderColor = RazorTheme.IsDark ? ColorTranslator.FromHtml("#4B5563") : ColorTranslator.FromHtml("#D1D5DB");
            Color hoverBackColor = RazorTheme.IsDark ? ColorTranslator.FromHtml("#374151") : ColorTranslator.FromHtml("#F9FAFB");
            Color backColor = this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)) ? hoverBackColor : (RazorTheme.IsDark ? RazorTheme.Colors.CardDark : Color.White);

            using (GraphicsPath path = new GraphicsPath())
            {
                int radius = 8;
                path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                path.AddArc(this.Width - (radius * 2) - 1, 0, radius * 2, radius * 2, 270, 90);
                path.AddArc(this.Width - (radius * 2) - 1, this.Height - (radius * 2) - 1, radius * 2, radius * 2, 0, 90);
                path.AddArc(0, this.Height - (radius * 2) - 1, radius * 2, radius * 2, 90, 90);
                path.CloseFigure();

                using (SolidBrush brush = new SolidBrush(backColor))
                {
                    pevent.Graphics.FillPath(brush, path);
                }

                using (Pen pen = new Pen(borderColor, 1))
                {
                    pevent.Graphics.DrawPath(pen, path);
                }
            }

            TextRenderer.DrawText(pevent.Graphics, this.Text, this.Font, this.ClientRectangle, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
