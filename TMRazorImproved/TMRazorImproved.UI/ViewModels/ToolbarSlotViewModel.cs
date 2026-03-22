using CommunityToolkit.Mvvm.ComponentModel;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.UI.ViewModels
{
    /// <summary>
    /// Rappresenta uno slot della FloatingToolbar: mostra nome, conteggio e stato di avviso
    /// per gli slot di tipo Item; per gli altri tipi mostra solo il nome.
    /// </summary>
    public partial class ToolbarSlotViewModel : ObservableObject
    {
        private readonly int _warningCount;

        public ToolbarItemType Type { get; }
        public string Id { get; }
        public string Name { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShowCount))]
        [NotifyPropertyChangedFor(nameof(IsWarning))]
        private int _count = -1;

        /// <summary>True se lo slot è di tipo Item e il conteggio è disponibile.</summary>
        public bool ShowCount => Type == ToolbarItemType.Item && Count >= 0;

        /// <summary>True se il conteggio è inferiore alla soglia di avviso configurata.</summary>
        public bool IsWarning => ShowCount && _warningCount > 0 && Count < _warningCount;

        public ToolbarSlotViewModel(ToolbarItem config)
        {
            Type = config.Type;
            Id = config.Id;
            Name = config.Name;
            _warningCount = config.WarningCount;
        }
    }
}
