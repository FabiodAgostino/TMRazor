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
    public sealed partial class ScavengerViewModel : ViewModelBase
    {
        private readonly IConfigService _config;
        private readonly ITargetingService _targeting;
        private readonly ILogService _log;
        private readonly object _lock = new();

        [ObservableProperty]
        private bool _isEnabled;

        [ObservableProperty]
        private uint _containerSerial;

        [ObservableProperty]
        private string _containerName = "Not Set";

        [ObservableProperty]
        private int _range = 2;

        [ObservableProperty]
        private int _delay = 600;

        [ObservableProperty]
        private bool _autoStart;

        [ObservableProperty]
        private bool _allowHidden;

        [ObservableProperty]
        private ScavengerConfig? _selectedList;

        public ObservableCollection<ScavengerConfig> Lists { get; } = new();
        public ObservableCollection<LootItem> LootItems { get; } = new();
        public ObservableCollection<LogEntry> Logs { get; } = new();

        public IAsyncRelayCommand SetContainerCommand { get; }
        public IAsyncRelayCommand AddItemCommand { get; }
        public IRelayCommand AddManualItemCommand { get; }
        public IRelayCommand EditItemCommand { get; }
        public IRelayCommand RemoveItemCommand { get; }
        public IRelayCommand ClearListCommand { get; }
        public IRelayCommand AddListCommand { get; }
        public IRelayCommand RemoveListCommand { get; }
        public IRelayCommand CloneListCommand { get; }

        public ScavengerViewModel(IConfigService config, ITargetingService targeting, ILogService log)
        {
            _config = config;
            _targeting = targeting;
            _log = log;
            
            EnableThreadSafeCollection(Lists, _lock);
            EnableThreadSafeCollection(LootItems, _lock);
            EnableThreadSafeCollection(Logs, _lock);

            _log.OnNewLog += entry =>
            {
                if (entry.Source == "Scavenger")
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Logs.Insert(0, entry);
                        if (Logs.Count > 50) Logs.RemoveAt(50);
                    });
                }
            };

            SetContainerCommand = new AsyncRelayCommand(SetContainerAsync);
            AddItemCommand = new AsyncRelayCommand(AddItemAsync);
            AddManualItemCommand = new RelayCommand(AddManualItem);
            EditItemCommand = new RelayCommand<LootItem>(EditItem);
            RemoveItemCommand = new RelayCommand<LootItem>(RemoveItem);
            ClearListCommand = new RelayCommand(ClearList);
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
            foreach (var list in profile.ScavengerLists)
            {
                Lists.Add(list);
            }

            SelectedList = Lists.FirstOrDefault(l => l.Name == profile.ActiveScavengerList) ?? Lists.FirstOrDefault();
        }

        partial void OnSelectedListChanged(ScavengerConfig? value)
        {
            if (value != null && _config.CurrentProfile != null)
            {
                _config.CurrentProfile.ActiveScavengerList = value.Name;
                LoadActiveListConfig();
                _config.Save();
            }
        }

        private void LoadActiveListConfig()
        {
            if (SelectedList == null) return;

            IsEnabled = SelectedList.Enabled;
            ContainerSerial = SelectedList.Container;
            ContainerName = ContainerSerial != 0 ? $"0x{ContainerSerial:X8}" : "Not Set";
            Range = SelectedList.Range;
            Delay = SelectedList.Delay;
            AutoStart = SelectedList.AutoStart;
            AllowHidden = SelectedList.AllowHidden;

            LootItems.Clear();
            foreach (var item in SelectedList.ItemList)
            {
                LootItems.Add(item);
            }
        }

        private void AddList()
        {
            var name = $"New List {Lists.Count + 1}";
            var newList = new ScavengerConfig { Name = name };
            _config.CurrentProfile?.ScavengerLists.Add(newList);
            Lists.Add(newList);
            SelectedList = newList;
        }

        private void RemoveList()
        {
            if (SelectedList == null || Lists.Count <= 1) return;
            var toRemove = SelectedList;
            _config.CurrentProfile?.ScavengerLists.Remove(toRemove);
            Lists.Remove(toRemove);
            SelectedList = Lists.FirstOrDefault();
        }

        private void CloneList()
        {
            if (SelectedList == null) return;
            var clone = new ScavengerConfig
            {
                Name = $"{SelectedList.Name} (Copy)",
                Enabled = SelectedList.Enabled,
                Container = SelectedList.Container,
                Range = SelectedList.Range,
                Delay = SelectedList.Delay,
                AutoStart = SelectedList.AutoStart,
                AllowHidden = SelectedList.AllowHidden,
                ItemList = new List<LootItem>(SelectedList.ItemList)
            };
            _config.CurrentProfile?.ScavengerLists.Add(clone);
            Lists.Add(clone);
            SelectedList = clone;
        }

        private ScavengerConfig? GetActiveConfig() => SelectedList;

        private async Task SetContainerAsync()
        {
            StatusText = "Seleziona il contenitore per lo scavenger...";
            var serial = await _targeting.AcquireTargetAsync();
            if (serial != 0)
            {
                ContainerSerial = serial;
                ContainerName = $"0x{serial:X8}";
                var config = GetActiveConfig();
                if (config != null)
                {
                    config.Container = serial;
                    _config.Save();
                }
                StatusText = $"Contenitore Scavenger impostato: {ContainerName}";
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
                var item = new LootItem((int)0x0F0E, -1, "Targeted Item");
                config.ItemList.Add(item);
                LootItems.Add(item);
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
                LootItems.Add(item);
                _config.Save();
                StatusText = "Oggetto vuoto aggiunto. Modifica ID e Colore nella lista.";
            }
        }

        private void EditItem(LootItem? item)
        {
            if (item == null) return;
            // TODO: Aprire dialog per la modifica delle proprietà
            StatusText = $"Modifica proprietà per {item.Name} non ancora implementata.";
        }

        private void RemoveItem(LootItem? item)
        {
            var config = GetActiveConfig();
            if (item != null && config != null)
            {
                config.ItemList.Remove(item);
                LootItems.Remove(item);
                _config.Save();
            }
        }

        private void ClearList()
        {
            var config = GetActiveConfig();
            if (config != null)
            {
                config.ItemList.Clear();
                LootItems.Clear();
                _config.Save();
            }
        }

        partial void OnIsEnabledChanged(bool value) => SaveConfig();
        partial void OnRangeChanged(int value) => SaveConfig();
        partial void OnDelayChanged(int value) => SaveConfig();
        partial void OnAutoStartChanged(bool value) => SaveConfig();
        partial void OnAllowHiddenChanged(bool value) => SaveConfig();

        private void SaveConfig()
        {
            var config = GetActiveConfig();
            if (config != null)
            {
                config.Enabled = IsEnabled;
                config.Range = Range;
                config.Delay = Delay;
                config.AutoStart = AutoStart;
                config.AllowHidden = AllowHidden;
                _config.Save();
            }
        }
    }
}
