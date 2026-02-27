using System.Drawing;
using System.Windows.Forms;
using Assistant.UI.Controls;
using RazorEnhanced.UI;

namespace Assistant
{
    public partial class MainForm : System.Windows.Forms.Form
    {
        private void InitializeBandageHealTab2()
        {
            bandageheal.BackColor = RazorTheme.Colors.BackgroundDark;

            // groupBox5 è già un RazorCard (Log), aggiusta solo il ListBox interno
            bandagehealLogBox.BackColor = RazorTheme.Colors.CurrentCard;
            bandagehealLogBox.ForeColor = RazorTheme.Colors.CurrentText;
            bandagehealLogBox.Font = RazorTheme.Fonts.DisplayFont(8.5F);
            bandagehealLogBox.BorderStyle = BorderStyle.None;

            // BandageHealSettingsBox: ridimensiona e posiziona per non traboccare
            // Tab usabile: 677 - 6 (padding) = 671
            // Log card (groupBox5): X=6, Width=297, right edge=303
            // Settings box: partiamo a 310, larghezza 355 → right edge=665 (OK)
            BandageHealSettingsBox.Location = new Point(310, 6);
            BandageHealSettingsBox.Size = new Size(355, 333);
            BandageHealSettingsBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Tema il GroupBox
            BandageHealSettingsBox.BackColor = RazorTheme.Colors.CurrentCard;
            BandageHealSettingsBox.ForeColor = RazorTheme.Colors.CurrentText;
            BandageHealSettingsBox.Font = RazorTheme.Fonts.DisplayFont(9F);

            // Tema tutti i controlli figli del GroupBox
            ApplyDarkThemeToGroupBox(BandageHealSettingsBox);

            // Allarga le checkbox con testo lungo che venivano troncate
            foreach (Control c in BandageHealSettingsBox.Controls)
            {
                if (c is CheckBox cb && cb.Right > BandageHealSettingsBox.Width - 10)
                    cb.Width = BandageHealSettingsBox.Width - cb.Left - 10;
            }
        }

        private void ApplyDarkThemeToGroupBox(Control container)
        {
            foreach (Control c in container.Controls)
            {
                c.ForeColor = RazorTheme.Colors.CurrentText;

                if (c is Label || c is CheckBox || c is RadioButton)
                {
                    c.BackColor = Color.Transparent;
                    if (c is Label lbl) lbl.Font = RazorTheme.Fonts.DisplayFont(8.5F);
                }
                else if (c is TextBox tb)
                {
                    tb.BackColor = RazorTheme.Colors.CurrentCard;
                    tb.ForeColor = RazorTheme.Colors.CurrentText;
                    tb.Font = RazorTheme.Fonts.DisplayFont(9F);
                }
                else if (c is ComboBox cb)
                {
                    cb.BackColor = RazorTheme.Colors.CurrentCard;
                    cb.ForeColor = RazorTheme.Colors.CurrentText;
                    cb.Font = RazorTheme.Fonts.DisplayFont(9F);
                }
                else if (c is Button btn && !(c is RazorButton))
                {
                    btn.BackColor = RazorTheme.Colors.Primary;
                    btn.ForeColor = Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                }

                if (c.Controls.Count > 0)
                    ApplyDarkThemeToGroupBox(c);
            }
        }
    }
}
