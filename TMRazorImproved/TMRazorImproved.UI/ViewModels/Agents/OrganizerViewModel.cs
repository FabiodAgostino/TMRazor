using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.UI.ViewModels.Agents
{
    public sealed partial class OrganizerViewModel : ViewModelBase
    {
        private readonly IConfigService _config;
        private readonly ITargetingService _targeting;
        private readonly IOrganizerService _organizer;
        private readonly ILogService _log;
        private readonly object _lock = new();

        [ObservableProperty]
        private uint _sourceSerial;

        [ObservableProperty]
        private string _sourceName = "Not Set";

        [ObservableProperty]
        private uint _destinationSerial;

        [ObservableProperty]
        private string _destinationName = "Not Set";

        [ObservableProperty]
        private bool _isRunning;

        [ObservableProperty]
        private int _delay = 600;

        [ObservableProperty]
        private bool _stack = true;

        [ObservableProperty]
        private bool _loop;

        [ObservableProperty]
        private bool _showCompleteMessage = true;

        [ObservableProperty]
        private OrganizerConfig? _selectedList;

        public ObservableCollection<OrganizerConfig> Lists { get; } = new();
        public ObservableCollection<LootItem> OrganizerItems { get; } = new();
        public ObservableCollection<LogEntry> Logs { get; } = new();

        public IAsyncRelayCommand SetSourceCommand { get; }
        public IAsyncRelayCommand SetDestinationCommand { get; }
        public IAsyncRelayCommand AddItemCommand { get; }
        public IRelayCommand AddManualItemCommand { get; }
        public IRelayCommand EditItemCommand { get; }
        public IRelayCommand RemoveItemCommand { get; }
        public IRelayCommand ClearListCommand { get; }
        public IRelayCommand StartCommand { get; }
        public IAsyncRelayCommand StopCommand { get; }
        
        public IRelayCommand AddListCommand { get; }
        public IRelayCommand RemoveListCommand { get; }
        public IRelayCommand CloneListCommand { get; }

        public OrganizerViewModel(IConfigService config, ITargetingService targeting, IOrganizerService organizer, ILogService log)
        {
            _config = config;
            _targeting = targeting;
            _organizer = organizer;
            _log = log;
            
            EnableThreadSafeCollection(Lists, _lock);
            EnableThreadSafeCollection(OrganizerItems, _lock);
            EnableThreadSafeCollection(Logs, _lock);

            _log.OnNewLog += entry =>
            {
                if (entry.Source == "Organizer")
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Logs.Insert(0, entry);
                        if (Logs.Count > 50) Logs.RemoveAt(50);
                    });
                }
            };

            SetSourceCommand = new AsyncRelayCommand(SetSourceAsync);
            SetDestinationCommand = new AsyncRelayCommand(SetDestinationAsync);
            AddItemCommand = new AsyncRelayCommand(AddItemAsync);
            AddManualItemCommand = new RelayCommand(AddManualItem);
            EditItemCommand = new RelayCommand<LootItem>(EditItem);
            RemoveItemCommand = new RelayCommand<LootItem>(RemoveItem);
            ClearListCommand = new RelayCommand(ClearList);
            StartCommand = new RelayCommand(Start);
            StopCommand = new AsyncRelayCommand(StopAsync);
            
            AddListCommand = new RelayCommand(AddList);
            RemoveListCommand = new RelayCommand(RemoveList);
            CloneListCommand = new RelayCommand(CloneList);

            LoadLists();
        }

        private void LoadLists()
        {
            var profile = _config.CurrentProfile;
            if (profile == null) return;

            Lists.Clear();
            foreach (var list in profile.OrganizerLists)
            {
                Lists.Add(list);
            }

            SelectedList = Lists.FirstOrDefault(l => l.Name == profile.ActiveOrganizerList) ?? Lists.FirstOrDefault();
        }

        partial void OnSelectedListChanged(OrganizerConfig? value)
        {
            if (value != null && _config.CurrentProfile != null)
            {
                _config.CurrentProfile.ActiveOrganizerList = value.Name;
                LoadActiveListConfig();
                _config.Save();
            }
        }

        private void LoadActiveListConfig()
        {
            if (SelectedList == null) return;

            SourceSerial = SelectedList.Source;
            SourceName = SourceSerial != 0 ? $"0x{SourceSerial:X8}" : "Not Set";
            DestinationSerial = SelectedList.Destination;
            DestinationName = DestinationSerial != 0 ? $"0x{DestinationSerial:X8}" : "Not Set";
            Delay = SelectedList.Delay;
            Stack = SelectedList.Stack;
            Loop = SelectedList.Loop;
            ShowCompleteMessage = SelectedList.ShowCompleteMessage;

            OrganizerItems.Clear();
            foreach (var item in SelectedList.ItemList)
            {
                OrganizerItems.Add(item);
            }
        }

        private void AddList()
        {
            var name = $"New List {Lists.Count + 1}";
            var newList = new OrganizerConfig { Name = name };
            _config.CurrentProfile?.OrganizerLists.Add(newList);
            Lists.Add(newList);
            SelectedList = newList;
        }

        private void RemoveList()
        {
            if (SelectedList == null || Lists.Count <= 1) return;
            var toRemove = SelectedList;
            _config.CurrentProfile?.OrganizerLists.Remove(toRemove);
            Lists.Remove(toRemove);
            SelectedList = Lists.FirstOrDefault();
        }

        private void CloneList()
        {
            if (SelectedList == null) return;
            var clone = new OrganizerConfig
            {
                Name = $"{SelectedList.Name} (Copy)",
                Source = SelectedList.Source,
                Destination = SelectedList.Destination,
                Delay = SelectedList.Delay,
                Stack = SelectedList.Stack,
                Loop = SelectedList.Loop,
                ShowCompleteMessage = SelectedList.ShowCompleteMessage,
                ItemList = new List<LootItem>(SelectedList.ItemList)
            };
            _config.CurrentProfile?.OrganizerLists.Add(clone);
            Lists.Add(clone);
            SelectedList = clone;
        }

        private OrganizerConfig? GetActiveConfig() => SelectedList;

        private async Task SetSourceAsync()
        {
            StatusText = "Seleziona il contenitore sorgente...";
            var serial = await _targeting.AcquireTargetAsync();
            if (serial != 0)
            {
                SourceSerial = serial;
                SourceName = $"0x{serial:X8}";
                var config = GetActiveConfig();
                if (config != null)
                {
                    config.Source = serial;
                    _config.Save();
                }
                StatusText = $"Sorgente impostata: {SourceName}";
            }
        }

        private async Task SetDestinationAsync()
        {
            StatusText = "Seleziona il contenitore di destinazione...";
            var serial = await _targeting.AcquireTargetAsync();
            if (serial != 0)
            {
                DestinationSerial = serial;
                DestinationName = $"0x{serial:X8}";
                var config = GetActiveConfig();
                if (config != null)
                {
                    config.Destination = serial;
                    _config.Save();
                }
                StatusText = $"Destinazione impostata: {DestinationName}";
            }
        }

        private async Task AddItemAsync()
        {
            var config = GetActiveConfig();
            if (config == null) return;

            StatusText = "Seleziona l'oggetto da aggiungere alla lista...";
            var serial = await _targeting.AcquireTargetAsync();
            if (serial != 0)
            {
                var item = new LootItem((int)0x0EED, -1, "Targeted Item");
                config.ItemList.Add(item);
                OrganizerItems.Add(item);
                _config.Save();
                StatusText = "Oggetto aggiunto via target.";
            }
        }

        private void AddManualItem()
        {
            var config = GetActiveConfig();
            if (config != null)
            {
                var item = new LootItem((int)0x0000, -1, "Manual Item");
                config.ItemList.Add(item);
                OrganizerItems.Add(item);
                _config.Save();
                StatusText = "Oggetto vuoto aggiunto. Modifica ID e Colore nella lista.";
            }
        }

        private void EditItem(LootItem? item)
        {
            if (item == null) return;
            // TODO: Aprire dialog per la modifica delle proprietà e quantità
            StatusText = $"Modifica proprietà/quantità per {item.Name} non ancora implementata.";
        }

        private void RemoveItem(LootItem? item)
        {
            var config = GetActiveConfig();
            if (item != null && config != null)
            {
                config.ItemList.Remove(item);
                OrganizerItems.Remove(item);
                _config.Save();
            }
        }

        private void ClearList()
        {
            var config = GetActiveConfig();
            if (config != null)
            {
                config.ItemList.Clear();
                OrganizerItems.Clear();
                _config.Save();
            }
        }

        private void Start()
        {
            if (SourceSerial == 0 || DestinationSerial == 0)
            {
                StatusText = "Errore: Imposta sorgente e destinazione prima di avviare.";
                return;
            }
            
            _organizer.Start();
            IsRunning = _organizer.IsRunning;
            StatusText = "Organizer avviato...";
        }

        private async Task StopAsync()
        {
            await _organizer.StopAsync();
            IsRunning = _organizer.IsRunning;
            StatusText = "Organizer fermato.";
        }

        partial void OnDelayChanged(int value) => SaveConfig();
        partial void OnStackChanged(bool value) => SaveConfig();
        partial void OnLoopChanged(bool value) => SaveConfig();
        partial void OnShowCompleteMessageChanged(bool value) => SaveConfig();

        private void SaveConfig()
        {
            var config = GetActiveConfig();
            if (config != null)
            {
                config.Delay = Delay;
                config.Stack = Stack;
                config.Loop = Loop;
                config.ShowCompleteMessage = ShowCompleteMessage;
                _config.Save();
            }
        }
    }
}
