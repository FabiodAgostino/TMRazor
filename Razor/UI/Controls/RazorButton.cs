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
        private Color _pressedColor = ColorTranslator.FromHtml("#C2410C"); // orange-700
        private Color? _overrideCustomColor = null;
        private bool _isPressed = false;

        public Color? OverrideCustomColor
        {
            get => _overrideCustomColor;
            set { _overrideCustomColor = value; this.Invalidate(); }
        }

        public RazorButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = _primaryColor;
            this.ForeColor = Color.White;
            this.Font = RazorTheme.Fonts.DisplayFont(9F, FontStyle.Bold);
            this.Cursor = Cursors.Hand;
            this.Size = new Size(130, 40);
            this.DoubleBuffered = true;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _isPressed = true;
            this.Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _isPressed = false;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Clear background with parent color
            Color parentColor = this.Parent?.BackColor ?? RazorTheme.Colors.CurrentBackground;
            using (SolidBrush bgBrush = new SolidBrush(parentColor))
            {
                g.FillRectangle(bgBrush, this.ClientRectangle);
            }

            // Button 9 Style: 6px radius
            int radius = 6;
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            
            Color baseColor = _overrideCustomColor ?? _primaryColor;
            Color backColor;

            if (!this.Enabled)
            {
                backColor = Color.FromArgb(156, 163, 175); // Gray-400
            }
            else if (_isPressed)
            {
                backColor = _pressedColor;
            }
            else if (this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
            {
                backColor = _hoverColor;
            }
            else
            {
                backColor = baseColor;
            }

            using (GraphicsPath path = GetRoundedPath(rect, radius))
            {
                // 1. Draw Outer Shadow (Very subtle bottom depth)
                if (this.Enabled && !_isPressed)
                {
                    using (GraphicsPath shadowPath = GetRoundedPath(new Rectangle(rect.X, rect.Y + 1, rect.Width, rect.Height), radius))
                    using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(25, 0, 0, 0)))
                    {
                        g.FillPath(shadowBrush, shadowPath);
                    }
                }

                // 2. Fill Main Background
                using (SolidBrush brush = new SolidBrush(backColor))
                {
                    g.FillPath(brush, path);
                }

                // 3. Draw Inset Highlight/Border (Internal ring)
                if (this.Enabled && !_isPressed)
                {
                    using (Pen insetPen = new Pen(Color.FromArgb(35, 255, 255, 255), 1))
                    {
                        Rectangle insetRect = new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);
                        if (insetRect.Width > 0 && insetRect.Height > 0)
                        {
                            using (GraphicsPath insetPath = GetRoundedPath(insetRect, Math.Max(1, radius - 1)))
                            {
                                g.DrawPath(insetPen, insetPath);
                            }
                        }
                    }
                }

                // 4. Border (Button 9 uses a very subtle dark outline)
                Color borderColor = _isPressed ? Color.Transparent : Color.FromArgb(40, 0, 0, 0);
                using (Pen borderPen = new Pen(borderColor, 1))
                {
                    g.DrawPath(borderPen, path);
                }
            }

            // Draw text
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            TextRenderer.DrawText(g, this.Text, this.Font, rect, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
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
            this.BackColor = Color.Transparent;
            this.ForeColor = RazorTheme.IsDark ? RazorTheme.Colors.TextDarkMode : RazorTheme.Colors.TextLightMode;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            int radius = 6;

            bool isHovered = this.ClientRectangle.Contains(this.PointToClient(Cursor.Position));
            Color borderColor = isHovered ? RazorTheme.Colors.Primary : (RazorTheme.IsDark ? Color.FromArgb(75, 255, 255, 255) : Color.FromArgb(75, 0, 0, 0));

            using (GraphicsPath path = GetRoundedPath(rect, radius))
            {
                if (isHovered)
                {
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(15, RazorTheme.Colors.Primary)))
                    {
                        g.FillPath(brush, path);
                    }
                }

                using (Pen pen = new Pen(borderColor, 1.2f))
                {
                    g.DrawPath(pen, path);
                }
            }

            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            TextRenderer.DrawText(g, this.Text, this.Font, rect, isHovered ? RazorTheme.Colors.Primary : this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
