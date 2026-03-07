using System.ComponentModel;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.UI.Utilities
{
    public class TranslationSource : INotifyPropertyChanged
    {
        private static readonly TranslationSource _instance = new TranslationSource();
        public static TranslationSource Instance => _instance;

        private ILanguageService? _languageService;

        public event PropertyChangedEventHandler? PropertyChanged;

        private TranslationSource() { }

        public void Initialize(ILanguageService languageService)
        {
            _languageService = languageService;
            Refresh();
        }

        public string this[string key]
        {
            get
            {
                if (string.IsNullOrEmpty(key)) return string.Empty;
                return _languageService?.GetString(key) ?? $"[{key}]";
            }
        }

        public void Refresh()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }
    }
}