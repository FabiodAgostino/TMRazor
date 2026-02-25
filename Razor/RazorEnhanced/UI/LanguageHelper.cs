using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Windows.Forms;
using RazorEnhanced;

namespace RazorEnhanced.UI
{
    internal static class LanguageHelper
    {
        private static ResourceManager _resourceManager;
        private static string _currentLanguage;

        static LanguageHelper()
        {
            // Resource name matches the expected location Assistant.RazorEnhanced.UI.Strings
            _resourceManager = new ResourceManager("Assistant.RazorEnhanced.UI.Strings", typeof(LanguageHelper).Assembly);
            _currentLanguage = Shards.allShards.Language ?? "it";
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
                string value = _resourceManager.GetString(key, new CultureInfo(_currentLanguage == "it" ? "it-IT" : "en-US"));
                return value ?? key;
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
