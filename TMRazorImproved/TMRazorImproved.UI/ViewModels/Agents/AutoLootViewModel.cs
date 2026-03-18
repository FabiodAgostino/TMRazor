using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.UI.ViewModels.Agents
{
    public sealed partial class AutoLootViewModel : ViewModelBase
    {
        private readonly IConfigService _config;
        private readonly ITargetingService _targeting;
        private readonly ILogService _log;
        private readonly ILanguageService _lang;
        private readonly object _lock = new();

        [ObservableProperty]
        private bool _isEnabled;

        [ObservableProperty]
        private uint _containerSerial;

        [ObservableProperty]
        private string _containerName = string.Empty;

        [ObservableProperty]
        [Range(100, 5000, ErrorMessage = "Delay must be between 100 and 5000ms")]
        private int _delay = 600;

        [ObservableProperty]
        [Range(1, 20, ErrorMessage = "Max range must be between 1 and 20")]
        private int _maxRange = 2;

        [ObservableProperty]
        private bool _noOpenCorpse;

        [ObservableProperty]
        private bool _autoStart;

        [ObservableProperty]
        private bool _allowHidden;

        [ObservableProperty]
        private AutoLootConfig? _selectedList;

        public ObservableCollection<AutoLootConfig> Lists { get; } = new();
        public ObservableCollection<LootItem> LootItems { get; } = new();
        public ObservableCollection<LogEntry> Logs { get; } = new();

        public IAsyncRelayCommand SetContainerCommand { get; }
        public IRelayCommand AddItemCommand { get; }
        public IRelayCommand EditItemCommand { get; }
        public IRelayCommand RemoveItemCommand { get; }
        public IRelayCommand ClearListCommand { get; }
        
        public IRelayCommand AddListCommand { get; }
        public IRelayCommand RemoveListCommand { get; }
        public IRelayCommand CloneListCommand { get; }

        public AutoLootViewModel(IConfigService config, ITargetingService targeting, ILogService log, ILanguageService languageService)
        {
            _config = config;
            _targeting = targeting;
            _log = log;
            _lang = languageService;
            
            _containerName = _lang.GetString("Agents.General.NotSet");

            EnableThreadSafeCollection(Lists, _lock);
            EnableThreadSafeCollection(LootItems, _lock);
            EnableThreadSafeCollection(Logs, _lock);

            _log.OnNewLog += entry =>
            {
                if (entry.Source == "AutoLoot")
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Logs.Insert(0, entry);
                        if (Logs.Count > 50) Logs.RemoveAt(50);
                    });
                }
            };

            SetContainerCommand = new AsyncRelayCommand(SetContainerAsync);
            AddItemCommand = new RelayCommand(AddItem);
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
            foreach (var list in profile.AutoLootLists)
            {
                Lists.Add(list);
            }

            SelectedList = Lists.FirstOrDefault(l => l.Name == profile.ActiveAutoLootList) ?? Lists.FirstOrDefault();
        }

        partial void OnSelectedListChanged(AutoLootConfig? value)
        {
            if (value != null && _config.CurrentProfile != null)
            {
                _config.CurrentProfile.ActiveAutoLootList = value.Name;
                LoadActiveListConfig();
                _config.Save();
            }
        }

        private void LoadActiveListConfig()
        {
            if (SelectedList == null) return;

            IsEnabled = SelectedList.Enabled;
            ContainerSerial = SelectedList.Container;
            ContainerName = ContainerSerial != 0 ? $"0x{ContainerSerial:X8}" : _lang.GetString("Agents.General.NotSet");
            Delay = SelectedList.Delay;
            MaxRange = SelectedList.MaxRange;
            NoOpenCorpse = SelectedList.NoOpenCorpse;
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
            var name = $"{_lang.GetString("Agents.General.NewList")} {Lists.Count + 1}";
            var newList = new AutoLootConfig { Name = name };
            _config.CurrentProfile?.AutoLootLists.Add(newList);
            Lists.Add(newList);
            SelectedList = newList;
        }

        private void RemoveList()
        {
            if (SelectedList == null || Lists.Count <= 1) return;
            
            var toRemove = SelectedList;
            _config.CurrentProfile?.AutoLootLists.Remove(toRemove);
            Lists.Remove(toRemove);
            SelectedList = Lists.FirstOrDefault();
        }

        private void CloneList()
        {
            if (SelectedList == null) return;

            var clone = new AutoLootConfig
            {
                Name = $"{SelectedList.Name} ({_lang.GetString("Agents.General.Copy")})",
                Enabled = SelectedList.Enabled,
                Container = SelectedList.Container,
                Delay = SelectedList.Delay,
                MaxRange = SelectedList.MaxRange,
                NoOpenCorpse = SelectedList.NoOpenCorpse,
                AutoStart = SelectedList.AutoStart,
                AllowHidden = SelectedList.AllowHidden,
                ItemList = new List<LootItem>(SelectedList.ItemList)
            };

            _config.CurrentProfile?.AutoLootLists.Add(clone);
            Lists.Add(clone);
            SelectedList = clone;
        }

        private async Task SetContainerAsync()
        {
            StatusText = _lang.GetString("Agents.General.SelectContainer");
            var targetInfo = await _targeting.AcquireTargetAsync(); var serial = targetInfo.Serial;
            if (serial != 0)
            {
                ContainerSerial = serial;
                ContainerName = $"0x{serial:X8}";
                if (SelectedList != null)
                {
                    SelectedList.Container = serial;
                    _config.Save();
                }
                StatusText = $"{_lang.GetString("Agents.General.ContainerSet")} {ContainerName}";
            }
        }

        private void AddItem()
        {
            // Note: In real implementation this might use targeting too
            uint graphic = 0x0EED;
            if (SelectedList != null && !SelectedList.ItemList.Any(i => i.Graphic == (int)graphic))
            {
                var newItem = new LootItem((int)graphic, -1, "Gold Coin");
                SelectedList.ItemList.Add(newItem);
                LootItems.Add(newItem);
                _config.Save();
                StatusText = _lang.GetString("Agents.General.ItemAdded");
            }
        }

        private void EditItem(LootItem? item)
        {
            if (item == null) return;
            
            var vm = new Windows.EditLootItemViewModel(item);
            var win = new Views.Windows.EditLootItemWindow(vm);
            
            if (win.ShowDialog() == true)
            {
                _config.Save();
                StatusText = _lang.GetString("Agents.General.ItemUpdated");
                OnPropertyChanged(nameof(LootItems)); // Forza refresh UI
            }
        }

        private void RemoveItem(LootItem? item)
        {
            if (item != null && SelectedList != null) 
            {
                SelectedList.ItemList.Remove(item);
                LootItems.Remove(item);
                _config.Save();
            }
        }

        private void ClearList()
        {
            if (SelectedList != null)
            {
                SelectedList.ItemList.Clear();
                LootItems.Clear();
                _config.Save();
            }
        }

        partial void OnIsEnabledChanged(bool value) => SaveActiveListConfig();
        partial void OnDelayChanged(int value) 
        {
            ValidateProperty(value, nameof(Delay));
            SaveActiveListConfig();
        }
        partial void OnMaxRangeChanged(int value) 
        {
            ValidateProperty(value, nameof(MaxRange));
            SaveActiveListConfig();
        }
        partial void OnNoOpenCorpseChanged(bool value) => SaveActiveListConfig();
        partial void OnAutoStartChanged(bool value) => SaveActiveListConfig();
        partial void OnAllowHiddenChanged(bool value) => SaveActiveListConfig();

        private void SaveActiveListConfig()
        {
            if (SelectedList != null)
            {
                SelectedList.Enabled = IsEnabled;
                SelectedList.Delay = Delay;
                SelectedList.MaxRange = MaxRange;
                SelectedList.NoOpenCorpse = NoOpenCorpse;
                SelectedList.AutoStart = AutoStart;
                SelectedList.AllowHidden = AllowHidden;
                _config.Save();
            }
        }
    }
}
