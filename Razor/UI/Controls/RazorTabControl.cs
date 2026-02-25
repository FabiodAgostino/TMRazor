using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Assistant.UI.Controls
{
    public class RazorTabControl : TabControl
    {
        private int hoveredIndex = -1;

        public RazorTabControl()
        {
            SetStyle(ControlStyles.UserPaint | 
                     ControlStyles.AllPaintingInWmPaint | 
                     ControlStyles.OptimizedDoubleBuffer | 
                     ControlStyles.ResizeRedraw, true);
            
            this.DoubleBuffered = true;
            this.SizeMode = TabSizeMode.Fixed;
            this.ItemSize = new Size(110, 32); 
            this.Padding = new Point(10, 0);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int prevHovered = hoveredIndex;
            hoveredIndex = -1;
            for (int i = 0; i < this.TabCount; i++)
            {
                if (this.GetTabRect(i).Contains(e.Location))
                {
                    hoveredIndex = i;
                    break;
                }
            }
            if (prevHovered != hoveredIndex)
                this.Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (hoveredIndex != -1)
            {
                hoveredIndex = -1;
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 1. Sfondo scuro dell'area tab
            using (SolidBrush bgBrush = new SolidBrush(RazorTheme.Colors.BackgroundDark))
            {
                g.FillRectangle(bgBrush, this.ClientRectangle);
            }

            // 2. Disegna le linguette
            for (int i = 0; i < this.TabCount; i++)
            {
                Rectangle tabRect = GetTabRect(i);
                bool isSelected = (this.SelectedIndex == i);
                bool isHover = (i == hoveredIndex) && !isSelected;

                // Solo un leggero feedback al passaggio del mouse, nessuno sfondo per il selezionato
                if (isHover)
                {
                    using (SolidBrush sb = new SolidBrush(Color.FromArgb(15, Color.White)))
                    {
                        g.FillRectangle(sb, tabRect);
                    }
                }

                // Disegna la riga arancione solo per il selezionato
                if (isSelected)
                {
                    using (SolidBrush accentBrush = new SolidBrush(RazorTheme.Colors.Primary))
                    {
                        // Riga arancione più definita
                        g.FillRectangle(accentBrush, tabRect.X + 15, tabRect.Bottom - 3, tabRect.Width - 30, 3);
                    }
                }

                // Testo più piccolo (8.5F)
                string text = this.TabPages[i].Text;
                Color textColor = isSelected ? RazorTheme.Colors.Primary : (isHover ? Color.White : RazorTheme.Colors.TextSecondaryDark);
                
                using (Font font = RazorTheme.Fonts.DisplayFont(8.5F, isSelected ? FontStyle.Bold : FontStyle.Regular))
                {
                    TextRenderer.DrawText(g, text, font, tabRect, textColor, 
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            }

            // 3. Separatore sottile orizzontale sotto i tab
            using (Pen p = new Pen(Color.FromArgb(30, Color.White), 1))
            {
                g.DrawLine(p, 0, ItemSize.Height, this.Width, ItemSize.Height);
            }
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (e.Control is TabPage page)
            {
                page.BackColor = RazorTheme.Colors.BackgroundDark;
                page.ForeColor = RazorTheme.Colors.CurrentText;
            }
        }
    }
}
