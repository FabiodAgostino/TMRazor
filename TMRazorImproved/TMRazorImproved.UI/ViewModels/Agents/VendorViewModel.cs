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
    public sealed partial class VendorViewModel : ViewModelBase
    {
        private readonly IConfigService _config;
        private readonly ITargetingService _targeting;
        private readonly ILogService _log;
        private readonly ILanguageService _lang;
        private readonly object _buyLock = new();
        private readonly object _sellLock = new();
        private readonly object _listLock = new();

        [ObservableProperty]
        private bool _buyEnabled;

        [ObservableProperty]
        private bool _sellEnabled;

        [ObservableProperty]
        private uint _buyBagSerial;

        [ObservableProperty]
        private string _buyBagName = string.Empty;

        [ObservableProperty]
        private uint _sellBagSerial;

        [ObservableProperty]
        private string _sellBagName = string.Empty;

        [ObservableProperty]
        private bool _logPurchases;

        [ObservableProperty]
        private bool _compareName;

        [ObservableProperty]
        private int _maxSellAmount = 500;

        [ObservableProperty]
        private VendorConfig? _selectedList;

        public ObservableCollection<VendorConfig> Lists { get; } = new();
        public ObservableCollection<LootItem> BuyList { get; } = new();
        public ObservableCollection<LootItem> SellList { get; } = new();
        public ObservableCollection<LogEntry> Logs { get; } = new();

        public IRelayCommand AddBuyItemCommand { get; }
        public IRelayCommand<LootItem> RemoveBuyItemCommand { get; }
        public IRelayCommand AddSellItemCommand { get; }
        public IRelayCommand<LootItem> RemoveSellItemCommand { get; }
        public IAsyncRelayCommand SetBuyBagCommand { get; }
        public IAsyncRelayCommand SetSellBagCommand { get; }
        
        public IRelayCommand AddListCommand { get; }
        public IRelayCommand RemoveListCommand { get; }
        public IRelayCommand CloneListCommand { get; }

        public VendorViewModel(IConfigService config, ITargetingService targeting, ILogService log, ILanguageService languageService)
        {
            _config = config;
            _targeting = targeting;
            _log = log;
            _lang = languageService;

            _buyBagName = _lang.GetString("Agents.General.NotSet");
            _sellBagName = _lang.GetString("Agents.General.NotSet");

            EnableThreadSafeCollection(Lists, _listLock);
            EnableThreadSafeCollection(BuyList, _buyLock);
            EnableThreadSafeCollection(SellList, _sellLock);
            EnableThreadSafeCollection(Logs, _listLock);

            _log.OnNewLog += entry =>
            {
                if (entry.Source == "Vendor")
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Logs.Insert(0, entry);
                        if (Logs.Count > 50) Logs.RemoveAt(50);
                    });
                }
            };

            AddBuyItemCommand = new RelayCommand(AddBuyItem);
            RemoveBuyItemCommand = new RelayCommand<LootItem>(RemoveBuyItem);
            AddSellItemCommand = new RelayCommand(AddSellItem);
            RemoveSellItemCommand = new RelayCommand<LootItem>(RemoveSellItem);
            SetBuyBagCommand = new AsyncRelayCommand(SetBuyBagAsync);
            SetSellBagCommand = new AsyncRelayCommand(SetSellBagAsync);
            
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
            foreach (var list in profile.VendorLists)
                Lists.Add(list);

            SelectedList = Lists.FirstOrDefault(l => l.Name == profile.ActiveVendorList) ?? Lists.FirstOrDefault();
        }

        partial void OnSelectedListChanged(VendorConfig? value)
        {
            if (value != null && _config.CurrentProfile != null)
            {
                _config.CurrentProfile.ActiveVendorList = value.Name;
                LoadActiveListConfig();
                _config.Save();
            }
        }

        private void LoadActiveListConfig()
        {
            if (SelectedList == null) return;

            BuyEnabled = SelectedList.BuyEnabled;
            SellEnabled = SelectedList.SellEnabled;
            BuyBagSerial = SelectedList.BuyBag;
            BuyBagName = BuyBagSerial != 0 ? $"0x{BuyBagSerial:X8}" : _lang.GetString("Agents.General.NotSet");
            SellBagSerial = SelectedList.SellBag;
            SellBagName = SellBagSerial != 0 ? $"0x{SellBagSerial:X8}" : _lang.GetString("Agents.General.NotSet");
            LogPurchases = SelectedList.LogPurchases;
            CompareName = SelectedList.CompareName;
            MaxSellAmount = SelectedList.MaxSellAmount;

            BuyList.Clear();
            foreach (var item in SelectedList.BuyList)
                BuyList.Add(item);

            SellList.Clear();
            foreach (var item in SelectedList.SellList)
                SellList.Add(item);
        }

        private async Task SetBuyBagAsync()
        {
            StatusText = _lang.GetString("Agents.General.SelectContainer");
            var serial = await _targeting.AcquireTargetAsync();
            if (serial != 0)
            {
                BuyBagSerial = serial;
                BuyBagName = $"0x{serial:X8}";
                if (SelectedList != null)
                {
                    SelectedList.BuyBag = serial;
                    _config.Save();
                }
                StatusText = $"{_lang.GetString("Agents.General.ContainerSet")} {BuyBagName}";
            }
        }

        private async Task SetSellBagAsync()
        {
            StatusText = _lang.GetString("Agents.General.SelectContainer");
            var serial = await _targeting.AcquireTargetAsync();
            if (serial != 0)
            {
                SellBagSerial = serial;
                SellBagName = $"0x{serial:X8}";
                if (SelectedList != null)
                {
                    SelectedList.SellBag = serial;
                    _config.Save();
                }
                StatusText = $"{_lang.GetString("Agents.General.ContainerSet")} {SellBagName}";
            }
        }

        private void AddList()
        {
            var name = $"{_lang.GetString("Agents.General.NewList")} {Lists.Count + 1}";
            var newList = new VendorConfig { Name = name };
            _config.CurrentProfile?.VendorLists.Add(newList);
            Lists.Add(newList);
            SelectedList = newList;
        }

        private void RemoveList()
        {
            if (SelectedList == null || Lists.Count <= 1) return;
            var toRemove = SelectedList;
            _config.CurrentProfile?.VendorLists.Remove(toRemove);
            Lists.Remove(toRemove);
            SelectedList = Lists.FirstOrDefault();
        }

        private void CloneList()
        {
            if (SelectedList == null) return;
            var clone = new VendorConfig
            {
                Name = $"{SelectedList.Name} ({_lang.GetString("Agents.General.Copy")})",
                BuyEnabled = SelectedList.BuyEnabled,
                SellEnabled = SelectedList.SellEnabled,
                BuyBag = SelectedList.BuyBag,
                SellBag = SelectedList.SellBag,
                LogPurchases = SelectedList.LogPurchases,
                CompareName = SelectedList.CompareName,
                MaxSellAmount = SelectedList.MaxSellAmount,
                BuyList = new List<LootItem>(SelectedList.BuyList.Select(i => new LootItem { Graphic = i.Graphic, Color = i.Color, Amount = i.Amount, Name = i.Name, IsEnabled = i.IsEnabled })),
                SellList = new List<LootItem>(SelectedList.SellList.Select(i => new LootItem { Graphic = i.Graphic, Color = i.Color, Amount = i.Amount, Name = i.Name, IsEnabled = i.IsEnabled }))
            };
            _config.CurrentProfile?.VendorLists.Add(clone);
            Lists.Add(clone);
            SelectedList = clone;
        }

        private void AddBuyItem()
        {
            if (SelectedList != null)
            {
                var newItem = new LootItem(0x0F3F, -1, "Arrows") { Amount = 999 };
                SelectedList.BuyList.Add(newItem);
                BuyList.Add(newItem);
                _config.Save();
            }
        }

        private void RemoveBuyItem(LootItem? item)
        {
            if (item != null && SelectedList != null)
            {
                SelectedList.BuyList.Remove(item);
                BuyList.Remove(item);
                _config.Save();
            }
        }

        private void AddSellItem()
        {
            if (SelectedList != null)
            {
                var newItem = new LootItem(0x0EED, -1, "Gold Coin") { Amount = 0 }; // 0 = sell all
                SelectedList.SellList.Add(newItem);
                SellList.Add(newItem);
                _config.Save();
            }
        }

        private void RemoveSellItem(LootItem? item)
        {
            if (item != null && SelectedList != null)
            {
                SelectedList.SellList.Remove(item);
                SellList.Remove(item);
                _config.Save();
            }
        }

        partial void OnBuyEnabledChanged(bool value) => SaveConfig();
        partial void OnSellEnabledChanged(bool value) => SaveConfig();
        partial void OnLogPurchasesChanged(bool value) => SaveConfig();
        partial void OnCompareNameChanged(bool value) => SaveConfig();
        partial void OnMaxSellAmountChanged(int value) => SaveConfig();

        private void SaveConfig()
        {
            if (SelectedList != null)
            {
                SelectedList.BuyEnabled = BuyEnabled;
                SelectedList.SellEnabled = SellEnabled;
                SelectedList.LogPurchases = LogPurchases;
                SelectedList.CompareName = CompareName;
                SelectedList.MaxSellAmount = MaxSellAmount;
                _config.Save();
            }
        }
    }
}
