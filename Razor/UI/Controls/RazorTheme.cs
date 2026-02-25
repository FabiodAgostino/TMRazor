using System.Drawing;

namespace Assistant.UI.Controls
{
    public static class RazorTheme
    {
        public static bool IsDark { get; set; } = true; // Default to true based on prompt instruction

        public static class Colors
        {
            public static Color Primary => ColorTranslator.FromHtml("#F97316"); // Orange
            public static Color BackgroundLight => ColorTranslator.FromHtml("#F3F4F6");
            public static Color BackgroundDark => ColorTranslator.FromHtml("#2D2424");
            public static Color CardDark => ColorTranslator.FromHtml("#382E2E");
            public static Color CardLight => ColorTranslator.FromHtml("#FFFFFF");
            public static Color GlowViolet => ColorTranslator.FromHtml("#8B5CF6");
            public static Color SurfaceDark => ColorTranslator.FromHtml("#1E1818");

            // Text colors
            public static Color TextLightMode => ColorTranslator.FromHtml("#1F2937"); // gray-800
            public static Color TextDarkMode => ColorTranslator.FromHtml("#E5E7EB"); // gray-200
            public static Color TextSecondaryLight => ColorTranslator.FromHtml("#6B7280"); // gray-500
            public static Color TextSecondaryDark => ColorTranslator.FromHtml("#9CA3AF"); // gray-400

            // Helper for current theme
            public static Color CurrentBackground => IsDark ? BackgroundDark : BackgroundLight;
            public static Color CurrentCard => IsDark ? CardDark : CardLight;
            public static Color CurrentSurface => IsDark ? SurfaceDark : Color.White;
            public static Color CurrentText => IsDark ? TextDarkMode : TextLightMode;
            public static Color CurrentTextSecondary => IsDark ? TextSecondaryDark : TextSecondaryLight;
        }

        public static class Fonts
        {
            // Fallback to Segoe UI or Arial if Inter is not installed
            public static Font DisplayFont(float size, FontStyle style = FontStyle.Regular)
            {
                return new Font("Inter", size, style) ?? new Font("Segoe UI", size, style);
            }
        }

        public static void ApplyThemeToForm(System.Windows.Forms.Form form)
        {
            form.BackColor = Colors.CurrentBackground;
            form.ForeColor = Colors.CurrentText;
            ApplyThemeToControls(form.Controls);
        }

        private static void ApplyThemeToControls(System.Windows.Forms.Control.ControlCollection controls)
        {
            foreach (System.Windows.Forms.Control control in controls)
            {
                // Default handling
                control.ForeColor = Colors.CurrentText;

                if (control is System.Windows.Forms.Panel || control is System.Windows.Forms.TabPage || control is System.Windows.Forms.TabControl)
                {
                    control.BackColor = Colors.CurrentBackground;
                }
                else if (control is System.Windows.Forms.TextBox || control is System.Windows.Forms.ComboBox || control is Assistant.UI.Controls.RazorTextBox || control is Assistant.UI.Controls.RazorComboBox)
                {
                    control.BackColor = Colors.CurrentSurface;
                    control.ForeColor = Colors.CurrentText;
                    
                    if (control is System.Windows.Forms.TextBox tb)
                    {
                        tb.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                    }
                    else if (control is System.Windows.Forms.ComboBox cb)
                    {
                        cb.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                    }
                }
                else if (control is Assistant.UI.Controls.RazorButton rb)
                {
                    // Preserved because RazorButton manages its own background rendering
                }
                else if (control is System.Windows.Forms.Button btn)
                {
                    btn.BackColor = Colors.Primary;
                    btn.ForeColor = Color.White;
                    btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                }

                if (control.Controls.Count > 0)
                {
                    ApplyThemeToControls(control.Controls);
                }
            }
        }
    }
}
