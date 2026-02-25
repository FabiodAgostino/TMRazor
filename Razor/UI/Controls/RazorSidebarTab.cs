using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Assistant.UI.Controls
{
    public class RazorSidebarTab : Button
    {
        private bool _isActive = false;
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; Invalidate(); }
        }

        public string IconText { get; set; } = "";

        public RazorSidebarTab()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = RazorTheme.Colors.BackgroundDark;
            this.ForeColor = Color.White;
            this.Font = RazorTheme.Fonts.DisplayFont(10F, FontStyle.Regular); 
            this.Cursor = Cursors.Hand;
            this.Size = new Size(200, 42); 
            this.Margin = new Padding(0);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            pevent.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using (SolidBrush baseBrush = new SolidBrush(RazorTheme.Colors.BackgroundDark))
            {
                pevent.Graphics.FillRectangle(baseBrush, this.ClientRectangle);
            }

            if (_isActive) 
            {
                // Active dark orange overlay
                using (SolidBrush overlayBrush = new SolidBrush(Color.FromArgb(30, RazorTheme.Colors.Primary)))
                {
                    pevent.Graphics.FillRectangle(overlayBrush, this.ClientRectangle);
                }
                
                // Left accent border
                using (SolidBrush accentBrush = new SolidBrush(RazorTheme.Colors.Primary))
                {
                    pevent.Graphics.FillRectangle(accentBrush, 0, 0, 3, this.Height);
                }
            }
            else if (this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
            {
                // Hover overlay (light white hint)
                using (SolidBrush hoverBrush = new SolidBrush(Color.FromArgb(15, 255, 255, 255)))
                {
                    pevent.Graphics.FillRectangle(hoverBrush, this.ClientRectangle);
                }
            }

            Color textColor = _isActive ? Color.White : Color.FromArgb(200, 255, 255, 255);

            int currentX = 20;
            if (!string.IsNullOrEmpty(IconText))
            {
                using (Font iconFont = new Font("Segoe MDL2 Assets", 11.5F, FontStyle.Regular))
                {
                    TextRenderer.DrawText(pevent.Graphics, IconText, iconFont, new Rectangle(currentX, 0, 30, this.Height), textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
                }
                currentX += 35;
            }

            Rectangle textRect = new Rectangle(currentX, 0, this.Width - currentX, this.Height);
            TextRenderer.DrawText(pevent.Graphics, this.Text, this.Font, textRect, textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (!_isActive) Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (!_isActive) Invalidate();
        }
    }
}
