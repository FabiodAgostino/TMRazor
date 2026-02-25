using System;
using System.Drawing;
using System.Windows.Forms;

namespace Assistant.UI.Controls
{
    public class RazorComboBox : ComboBox
    {
        public RazorComboBox()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.BackColor = RazorTheme.IsDark ? RazorTheme.Colors.SurfaceDark : ColorTranslator.FromHtml("#F9FAFB");
            this.ForeColor = RazorTheme.Colors.CurrentText;
            this.Font = RazorTheme.Fonts.DisplayFont(9F);
        }
    }
}
