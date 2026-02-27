using System;
using System.Drawing;
using System.Windows.Forms;

namespace Assistant.UI.Controls
{
    public static class RazorTheme
    {
        public static bool IsDark { get; set; } = true;

        public static class Colors
        {
            public static Color Primary => ColorTranslator.FromHtml("#14532d"); // Professional dark green
            public static Color BackgroundLight => ColorTranslator.FromHtml("#F3F4F6");
            public static Color BackgroundDark => ColorTranslator.FromHtml("#152331"); // Deep Blue Grey
            public static Color GradientEndDark => ColorTranslator.FromHtml("#000000"); // Pure Black
            public static Color CardDark => ColorTranslator.FromHtml("#1E2D3D"); // Adjusted card color for the new bg
            public static Color CardLight => ColorTranslator.FromHtml("#FFFFFF");
            public static Color GlowViolet => ColorTranslator.FromHtml("#8B5CF6");
            public static Color SurfaceDark => ColorTranslator.FromHtml("#050C26");

            // Text colors
            public static Color TextLightMode => ColorTranslator.FromHtml("#1F2937");
            public static Color TextDarkMode => ColorTranslator.FromHtml("#E5E7EB");
            public static Color TextSecondaryLight => ColorTranslator.FromHtml("#6B7280");
            public static Color TextSecondaryDark => ColorTranslator.FromHtml("#9CA3AF");

            // Semantic colors
            public static Color Success => ColorTranslator.FromHtml("#14532d"); // Professional dark green
            public static Color SuccessHover => ColorTranslator.FromHtml("#166534"); // Lighter hover green
            public static Color New => ColorTranslator.FromHtml("#ff3900");
            public static Color NewHover => ColorTranslator.FromHtml("#e03100");
            public static Color Warning => ColorTranslator.FromHtml("#FFD700");
            public static Color WarningHover => ColorTranslator.FromHtml("#DAA520");
            public static Color Danger => ColorTranslator.FromHtml("#B31B1B");
            public static Color DangerHover => ColorTranslator.FromHtml("#8E1515");
            public static Color FocusOutline => ColorTranslator.FromHtml("#22c55e");

            // Helper for current theme
            public static Color CurrentBackground => IsDark ? BackgroundDark : BackgroundLight;
            public static Color CurrentGradientEnd => IsDark ? GradientEndDark : BackgroundLight;
            public static Color CurrentCard => IsDark ? CardDark : CardLight;
            public static Color CurrentSurface => IsDark ? SurfaceDark : Color.White;
            public static Color CurrentText => IsDark ? TextDarkMode : TextLightMode;
            public static Color CurrentTextSecondary => IsDark ? TextSecondaryDark : TextSecondaryLight;
        }

        public static class Fonts
        {
            public static Font DisplayFont(float size, FontStyle style = FontStyle.Regular)
            {
                return new Font("Nunito", size, style) ?? new Font("Segoe UI", size, style);
            }
        }

        public static void ApplyThemeToForm(System.Windows.Forms.Form form)
        {
            form.BackColor = Colors.CurrentBackground;
            form.ForeColor = Colors.CurrentText;

            if (IsDark)
            {
                form.Paint -= Form_PaintGradient;
                form.Paint += Form_PaintGradient;

                if (form.IsHandleCreated)
                {
                    ActivateDarkMode(form.Handle);
                }
                else
                {
                    form.HandleCreated += (s, e) => ActivateDarkMode(form.Handle);
                }
            }

            ApplyThemeToControls(form.Controls);
        }

        public static Color DarkenColor(Color color, float amount)
        {
            float r = Math.Max(0, color.R * (1f - amount));
            float g = Math.Max(0, color.G * (1f - amount));
            float b = Math.Max(0, color.B * (1f - amount));
            return Color.FromArgb(color.A, (int)r, (int)g, (int)b);
        }

        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private static void ActivateDarkMode(IntPtr handle)
        {
            if (System.Environment.OSVersion.Version.Major >= 10)
            {
                int attribute = 20;
                if (System.Environment.OSVersion.Version.Build < 18985)
                    attribute = 19;

                int useDarkMode = 1;
                DwmSetWindowAttribute(handle, attribute, ref useDarkMode, sizeof(int));

                SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0020 | 0x0040);
            }
        }

        private static void Form_PaintGradient(object sender, PaintEventArgs e)
        {
            if (sender is Form form)
            {
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    form.ClientRectangle,
                    Colors.BackgroundDark,
                    Colors.GradientEndDark,
                    45f))
                {
                    e.Graphics.FillRectangle(brush, form.ClientRectangle);
                }
            }
        }

        private static void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is Assistant.UI.Controls.RazorButton rb)
                {
                    // RazorButton has its own auto-detection logic
                }
                else if (control is Assistant.UI.Controls.RazorCard)
                {
                    // Managed controls
                }
                else
                {
                    control.ForeColor = Colors.CurrentText;

                    if (control is Panel || control is TabPage || control is GroupBox || control is TabControl || control is CheckedListBox)
                    {
                        control.BackColor = Colors.CurrentBackground;
                    }
                    else if (control is Label || control is CheckBox || control is RadioButton)
                    {
                        // Standard controls that might support transparency if not nested too deep
                        control.BackColor = Color.Transparent;
                        if (control.Parent is TabControl || control.Parent is TabPage)
                            control.BackColor = Colors.CurrentBackground;
                    }
                    else if (control is TextBox || control is ComboBox || control is Assistant.UI.Controls.RazorTextBox || control is Assistant.UI.Controls.RazorComboBox)
                    {
                        control.BackColor = Colors.CurrentSurface;
                        control.ForeColor = Colors.CurrentText;
                    }
                    else if (control is Button btn)
                    {
                        string text = btn.Text.ToLower();
                        if (text.Contains("remove") || text.Contains("rimuovi") ||
                            text.Contains("close") || text.Contains("chiudi") ||
                            text.Contains("disable") || text.Contains("disabilita"))
                        {
                            btn.BackColor = Colors.Danger;
                            btn.FlatAppearance.MouseOverBackColor = Colors.DangerHover;
                            btn.FlatAppearance.MouseDownBackColor = DarkenColor(Colors.Danger, 0.25f);
                        }
                        else
                        {
                            btn.BackColor = Colors.Primary;
                            btn.FlatAppearance.MouseOverBackColor = DarkenColor(Colors.Primary, 0.15f);
                        }

                        btn.ForeColor = Color.White;
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderSize = 0;
                    }
                }

                if (control.Controls.Count > 0)
                {
                    ApplyThemeToControls(control.Controls);
                }
            }
        }
    }

}
