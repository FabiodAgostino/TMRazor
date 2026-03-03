using System;
using System.Windows.Markup;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.UI.Views
{
    /// <summary>
    /// Estensione XAML per la localizzazione dinamica.
    /// Uso: Text="{loc:Loc MyKey}"
    /// </summary>
    public class LocExtension : MarkupExtension
    {
        public string Key { get; set; }

        public LocExtension(string key)
        {
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Key)) return string.Empty;

            // Evitiamo crash nel designer di Visual Studio
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
                return $"[Design:{Key}]";

            try
            {
                // Recupera il servizio di localizzazione dal DI container tramite l'App
                var langService = App.GetService<ILanguageService>();
                if (langService == null) return $"[NoService:{Key}]";

                return langService.GetString(Key);
            }
            catch
            {
                return $"[Error:{Key}]";
            }
        }
    }
}
