using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using RazorEnhanced;

namespace RazorEnhanced.UI
{
    internal static class LanguageHelper
    {
        private static ResourceManager _resourceManager;
        private static ResourceSet _italianResourceSet;
        private static string _currentLanguage;

        static LanguageHelper()
        {
            _resourceManager = new ResourceManager("Assistant.RazorEnhanced.UI.Strings", typeof(LanguageHelper).Assembly);
            _italianResourceSet = LoadItalianResourceSet();
            _currentLanguage = Shards.allShards.Language ?? "it";
        }

        // Loads the Italian satellite assembly directly from disk, handling both
        // VS MSBuild ("...Strings.resources") and dotnet SDK ("...Strings.it.resources") naming conventions.
        private static ResourceSet LoadItalianResourceSet()
        {
            try
            {
                string baseDir = Path.GetDirectoryName(typeof(LanguageHelper).Assembly.Location);
                string satPath = Path.Combine(baseDir, "it", "RazorEnhanced.resources.dll");
                if (!File.Exists(satPath))
                    return null;

                var satAssembly = Assembly.LoadFile(satPath);
                string resName = satAssembly.GetManifestResourceNames()
                    .FirstOrDefault(n => n.Contains("Strings") && n.EndsWith(".resources"));
                if (resName == null)
                    return null;

                var stream = satAssembly.GetManifestResourceStream(resName);
                if (stream == null)
                    return null;

                return new ResourceSet(stream);
            }
            catch (Exception ex)
            {
                Assistant.Utility.Logger.Debug($"LanguageHelper: failed to load Italian resources: {ex.Message}");
                return null;
            }
        }

        public static string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    Assistant.Utility.Logger.Debug($"Language changed to: {value}");
                }
            }
        }

        public static string GetString(string key)
        {
            try
            {
                if (_currentLanguage == "it" && _italianResourceSet != null)
                {
                    string val = _italianResourceSet.GetString(key);
                    if (val != null) return val;
                }
                return _resourceManager.GetString(key) ?? key;
            }
            catch (Exception ex)
            {
                Assistant.Utility.Logger.Debug($"Error getting string for key {key}: {ex.Message}");
                return key;
            }
        }

        public static void TranslateForm(Form form)
        {
            string formText = GetString(form.Name + ".Text");
            if (formText != form.Name + ".Text")
                form.Text = formText;

            TranslateControls(form.Controls, form.Name);
        }

        private static void TranslateControls(Control.ControlCollection controls, string prefix)
        {
            foreach (Control control in controls)
            {
                string key = prefix + "." + control.Name + ".Text";
                string translated = GetString(key);
                if (translated != key)
                {
                    control.Text = translated;
                }

                if (control.HasChildren)
                {
                    TranslateControls(control.Controls, prefix);
                }

                if (control is ToolStrip toolStrip)
                {
                    TranslateToolStrip(toolStrip, prefix);
                }
            }
        }

        private static void TranslateToolStrip(ToolStrip toolStrip, string prefix)
        {
            foreach (ToolStripItem item in toolStrip.Items)
            {
                string key = prefix + "." + item.Name + ".Text";
                string translated = GetString(key);
                if (translated != key)
                {
                    item.Text = translated;
                }

                if (item is ToolStripDropDownItem dropDownItem)
                {
                    TranslateToolStripDropDown(dropDownItem, prefix);
                }
            }
        }

        private static void TranslateToolStripDropDown(ToolStripDropDownItem dropDownItem, string prefix)
        {
            foreach (ToolStripItem item in dropDownItem.DropDownItems)
            {
                string key = prefix + "." + item.Name + ".Text";
                string translated = GetString(key);
                if (translated != key)
                {
                    item.Text = translated;
                }

                if (item is ToolStripDropDownItem subItem)
                {
                    TranslateToolStripDropDown(subItem, prefix);
                }
            }
        }
    }
}
