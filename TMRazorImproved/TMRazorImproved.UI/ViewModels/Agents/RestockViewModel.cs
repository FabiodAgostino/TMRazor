using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.UI.ViewModels.Agents
{
    public sealed partial class RestockViewModel : ViewModelBase
    {
        private readonly IConfigService _config;
        private readonly ITargetingService _targeting;
        private readonly IRestockService _restockService;
        private readonly ILogService _log;
        private readonly ILanguageService _lang;
        private readonly object _lock = new();

        [ObservableProperty]
        private bool _isEnabled;

        [ObservableProperty]
        private string _sourceName = string.Empty;

        [ObservableProperty]
        private string _destinationName = string.Empty;

        [ObservableProperty]
        private int _delay = 600;

        [ObservableProperty]
        private RestockConfig? _selectedList;

        public ObservableCollection<RestockConfig> Lists { get; } = new();
        public ObservableCollection<LootItem> RestockItems { get; } = new();
        public ObservableCollection<LogEntry> Logs { get; } = new();

        public IAsyncRelayCommand SetSourceCommand { get; }
        public IAsyncRelayCommand SetDestinationCommand { get; }
        public IAsyncRelayCommand AddItemCommand { get; }
        public IRelayCommand AddManualItemCommand { get; }
        public IRelayCommand RemoveItemCommand { get; }
        public IRelayCommand ClearListCommand { get; }
        
        public IRelayCommand AddListCommand { get; }
        public IRelayCommand RemoveListCommand { get; }
        public IRelayCommand CloneListCommand { get; }
        public IRelayCommand StartNowCommand { get; }
        public IRelayCommand StopNowCommand { get; }

        public bool IsRunning => _restockService.IsRunning;

        public RestockViewModel(IConfigService config, ITargetingService targeting, IRestockService restockService, ILogService log, ILanguageService languageService)
        {
            _config = config;
            _targeting = targeting;
            _restockService = restockService;
            _log = log;
            _lang = languageService;
            
            _sourceName = _lang.GetString("Agents.General.NotSet");
            _destinationName = _lang.GetString("Agents.General.NotSet");

            EnableThreadSafeCollection(Lists, _lock);
            EnableThreadSafeCollection(RestockItems, _lock);
            EnableThreadSafeCollection(Logs, _lock);

            _log.OnNewLog += entry =>
            {
                if (entry.Source == "Restock")
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
            RemoveItemCommand = new RelayCommand<LootItem>(RemoveItem);
            ClearListCommand = new RelayCommand(ClearList);
            
            AddListCommand = new RelayCommand(AddList);
            RemoveListCommand = new RelayCommand(RemoveList);
            CloneListCommand = new RelayCommand(CloneList);
            StartNowCommand = new RelayCommand(StartRestock);
            StopNowCommand = new RelayCommand(StopRestock);

            LoadLists();
        }

        private void StartRestock()
        {
            _restockService.Start();
            OnPropertyChanged(nameof(IsRunning));
            StatusText = _lang.GetString("Agents.Restock.Started");
        }

        private async void StopRestock()
        {
            await _restockService.StopAsync();
            OnPropertyChanged(nameof(IsRunning));
            StatusText = _lang.GetString("Agents.Restock.Stopped");
        }

        private void LoadLists()
        {
            var profile = _config.CurrentProfile;
            if (profile == null) return;

            Lists.Clear();
            foreach (var list in profile.RestockLists)
                Lists.Add(list);

            SelectedList = Lists.FirstOrDefault(l => l.Name == profile.ActiveRestockList) ?? Lists.FirstOrDefault();
        }

        partial void OnSelectedListChanged(RestockConfig? value)
        {
            if (value != null && _config.CurrentProfile != null)
            {
                _config.CurrentProfile.ActiveRestockList = value.Name;
                LoadActiveListConfig();
                _config.Save();
            }
        }

        private void LoadActiveListConfig()
        {
            if (SelectedList == null) return;

            IsEnabled = SelectedList.Enabled;
            SourceName = SelectedList.Source != 0 ? $"0x{SelectedList.Source:X8}" : _lang.GetString("Agents.General.NotSet");
            DestinationName = SelectedList.Destination != 0 ? $"0x{SelectedList.Destination:X8}" : _lang.GetString("Agents.General.NotSet");
            Delay = SelectedList.Delay;

            RestockItems.Clear();
            foreach (var item in SelectedList.ItemList)
                RestockItems.Add(item);
        }

        private async Task SetSourceAsync()
        {
            StatusText = _lang.GetString("Agents.General.SelectContainer");
            var serial = await _targeting.AcquireTargetAsync();
            if (serial != 0)
            {
                SourceName = $"0x{serial:X8}";
                if (SelectedList != null)
                {
                    SelectedList.Source = serial;
                    _config.Save();
                }
                StatusText = $"{_lang.GetString("Agents.General.ContainerSet")} {SourceName}";
            }
        }

        private async Task SetDestinationAsync()
        {
            StatusText = _lang.GetString("Agents.General.SelectContainer");
            var serial = await _targeting.AcquireTargetAsync();
            if (serial != 0)
            {
                DestinationName = $"0x{serial:X8}";
                if (SelectedList != null)
                {
                    SelectedList.Destination = serial;
                    _config.Save();
                }
                StatusText = $"{_lang.GetString("Agents.General.ContainerSet")} {DestinationName}";
            }
        }

        private void AddList()
        {
            var name = $"{_lang.GetString("Agents.General.NewList")} {Lists.Count + 1}";
            var newList = new RestockConfig { Name = name };
            _config.CurrentProfile?.RestockLists.Add(newList);
            Lists.Add(newList);
            SelectedList = newList;
        }

        private void RemoveList()
        {
            if (SelectedList == null || Lists.Count <= 1) return;
            var toRemove = SelectedList;
            _config.CurrentProfile?.RestockLists.Remove(toRemove);
            Lists.Remove(toRemove);
            SelectedList = Lists.FirstOrDefault();
        }

        private void CloneList()
        {
            if (SelectedList == null) return;
            var clone = new RestockConfig
            {
                Name = $"{SelectedList.Name} ({_lang.GetString("Agents.General.Copy")})",
                Enabled = SelectedList.Enabled,
                Source = SelectedList.Source,
                Destination = SelectedList.Destination,
                Delay = SelectedList.Delay,
                ItemList = new List<LootItem>(SelectedList.ItemList.Select(i => new LootItem { Graphic = i.Graphic, Color = i.Color, Amount = i.Amount, Name = i.Name, IsEnabled = i.IsEnabled }))
            };
            _config.CurrentProfile?.RestockLists.Add(clone);
            Lists.Add(clone);
            SelectedList = clone;
        }

        private async Task AddItemAsync()
        {
            if (SelectedList == null) return;

            StatusText = _lang.GetString("Agents.General.SelectItem");
            var serial = await _targeting.AcquireTargetAsync();
            if (serial != 0)
            {
                var item = new LootItem((int)0x0F7A, -1, "Targeted Item") { Amount = 50 };
                SelectedList.ItemList.Add(item);
                RestockItems.Add(item);
                _config.Save();
                StatusText = _lang.GetString("Agents.General.ItemAdded");
            }
        }

        private void AddManualItem()
        {
            if (SelectedList != null)
            {
                var item = new LootItem((int)0x0000, -1, "Manual Item") { Amount = 1 };
                SelectedList.ItemList.Add(item);
                RestockItems.Add(item);
                _config.Save();
                StatusText = _lang.GetString("Agents.General.ManualItemAdded");
            }
        }

        private void RemoveItem(LootItem? item)
        {
            if (item != null && SelectedList != null)
            {
                SelectedList.ItemList.Remove(item);
                RestockItems.Remove(item);
                _config.Save();
            }
        }

        private void ClearList()
        {
            if (SelectedList != null)
            {
                SelectedList.ItemList.Clear();
                RestockItems.Clear();
                _config.Save();
            }
        }

        partial void OnDelayChanged(int value)
        {
            if (SelectedList != null)
            {
                SelectedList.Delay = value;
                _config.Save();
            }
        }

        partial void OnIsEnabledChanged(bool value)
        {
            if (SelectedList != null)
            {
                SelectedList.Enabled = value;
                _config.Save();
            }
        }
    }
}
