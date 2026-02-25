using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Assistant.UI.Controls
{
    public class RazorTextBox : TextBox
    {
        private Color _borderColor = RazorTheme.IsDark ? ColorTranslator.FromHtml("#4B5563") : ColorTranslator.FromHtml("#D1D5DB");
        private Color _focusedBorderColor = RazorTheme.Colors.Primary;

        public RazorTextBox()
        {
            this.BorderStyle = BorderStyle.FixedSingle;
            this.BackColor = RazorTheme.IsDark ? RazorTheme.Colors.SurfaceDark : ColorTranslator.FromHtml("#F9FAFB");
            this.ForeColor = RazorTheme.Colors.CurrentText;
            this.Font = RazorTheme.Fonts.DisplayFont(9F);
        }

    }
}
