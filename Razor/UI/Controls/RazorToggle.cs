using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Assistant.UI.Controls
{
    public class RazorToggle : CheckBox
    {
        private Color _onBackColor = RazorTheme.Colors.Primary;
        private Color _offBackColor = ColorTranslator.FromHtml("#D1D5DB"); // gray-300
        private Color _offBackColorDark = ColorTranslator.FromHtml("#374151"); // gray-700
        private Color _toggleColor = Color.White;

        public RazorToggle()
        {
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.Cursor = Cursors.Hand;
        }

        // Suppress the focus-rectangle glow that Windows draws around the control
        protected override bool ShowFocusCues => false;
        protected override bool ShowKeyboardCues => false;

        protected override void WndProc(ref Message m)
        {
            // Suppress WM_SETFOCUS and WM_KILLFOCUS repaints from the native checkbox
            const int WM_SETFOCUS   = 0x0007;
            const int WM_KILLFOCUS  = 0x0008;
            if (m.Msg == WM_SETFOCUS || m.Msg == WM_KILLFOCUS)
            {
                this.Invalidate();
                return;
            }
            base.WndProc(ref m);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            Size baseSize = base.GetPreferredSize(proposedSize);
            return new Size(baseSize.Width + 28, Math.Max(baseSize.Height, 20));
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // Prevent clipping from strictly dragged bounds in legacy designer
            int minW = string.IsNullOrWhiteSpace(this.Text) ? 40 : 60;
            base.SetBoundsCore(x, y, Math.Max(width, minW), Math.Max(height, 20), specified);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Force opaque background paint to avoid black/white corner artifacts
            Color bgClearColor = this.BackColor == Color.Transparent ? (this.Parent?.BackColor ?? RazorTheme.Colors.CurrentBackground) : this.BackColor;
            using (SolidBrush bgBrush = new SolidBrush(bgClearColor))
            {
                pevent.Graphics.FillRectangle(bgBrush, this.ClientRectangle);
            }

            Color currentOffBack = RazorTheme.IsDark ? _offBackColorDark : _offBackColor;
            Color backColor = this.Checked ? _onBackColor : currentOffBack;

            int toggleW = 36;
            int toggleH = 18;
            int toggleY = (this.Height - toggleH) / 2;
            int toggleX = Math.Max(2, this.Width - toggleW - 2);

            if (string.IsNullOrWhiteSpace(this.Text))
            {
                // Standalone toggle, center it
                toggleX = (this.Width - toggleW) / 2;
            }

            // Draw track (background)
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(toggleX, toggleY, toggleH, toggleH, 90, 180);
                path.AddArc(toggleX + toggleW - toggleH, toggleY, toggleH, toggleH, 270, 180);
                path.CloseFigure();

                using (SolidBrush brush = new SolidBrush(backColor))
                {
                    pevent.Graphics.FillPath(brush, path);
                }
                
                // Draw border for track when off
                if (!this.Checked)
                {
                    using (Pen pen = new Pen(RazorTheme.IsDark ? ColorTranslator.FromHtml("#4B5563") : ColorTranslator.FromHtml("#D1D5DB"), 1.5f))
                    {
                        pevent.Graphics.DrawPath(pen, path);
                    }
                }
            }

            // Draw toggle (circle)
            int d = toggleH - 4; // Margin of 2px
            int circleX = this.Checked ? toggleX + toggleW - d - 2 : toggleX + 2;
            int circleY = toggleY + 2;

            using (SolidBrush brush = new SolidBrush(_toggleColor))
            {
                pevent.Graphics.FillEllipse(brush, circleX, circleY, d, d);
                
                if(!this.Checked)
                {
                     using(Pen pen = new Pen(RazorTheme.IsDark ? ColorTranslator.FromHtml("#4B5563") : ColorTranslator.FromHtml("#D1D5DB"), 1.5f))
                     {
                          pevent.Graphics.DrawEllipse(pen, circleX, circleY, d, d);
                     }
                }
            }

            // Draw text
            if (!string.IsNullOrEmpty(this.Text))
            {
                Rectangle textRect = new Rectangle(0, 0, toggleX - 4, this.Height);
                TextRenderer.DrawText(pevent.Graphics, this.Text, this.Font, textRect, this.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }
        }
    }
}
