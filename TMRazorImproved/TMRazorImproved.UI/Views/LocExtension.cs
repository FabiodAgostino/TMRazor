using System;
using System.Windows.Data;
using System.Windows.Markup;
using TMRazorImproved.UI.Utilities;

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

            // Ritorna un binding dinamico a TranslationSource
            var binding = new Binding($"[{Key}]")
            {
                Source = TranslationSource.Instance,
                Mode = BindingMode.OneWay
            };

            return binding.ProvideValue(serviceProvider);
        }
    }
}
