using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.UI.ViewModels.Agents
{
    public sealed partial class DressViewModel : ViewModelBase
    {
        private readonly IConfigService _config;
        private readonly IDressService _dressService;
        private readonly ITargetingService _targeting;
        private readonly IWorldService _world;
        private readonly ILogService _log;
        private readonly ILanguageService _lang;
        private readonly object _lock = new();

        [ObservableProperty]
        private DressList? _selectedList;

        [ObservableProperty]
        private int _dragDelay = 600;

        [ObservableProperty]
        private bool _removeConflict = true;

        [ObservableProperty]
        private bool _use3D;

        [ObservableProperty]
        private uint _undressBagSerial;

        [ObservableProperty]
        private string _undressBagName = string.Empty;

        public ObservableCollection<DressList> Lists { get; } = new();
        public ObservableCollection<DressSlotViewModel> CurrentSlots { get; } = new();
        public ObservableCollection<LogEntry> Logs { get; } = new();

        public IRelayCommand AddListCommand { get; }
        public IRelayCommand RemoveListCommand { get; }
        public IRelayCommand DressNowCommand { get; }
        public IRelayCommand UndressNowCommand { get; }
        public IAsyncRelayCommand StopCommand { get; }
        public IRelayCommand ReadCurrentCommand { get; }
        public IAsyncRelayCommand SetUndressBagCommand { get; }
        public IAsyncRelayCommand<DressSlotViewModel> SetSlotCommand { get; }
        public IRelayCommand<DressSlotViewModel> ClearSlotCommand { get; }
        public IRelayCommand ClearAllSlotsCommand { get; }

        public DressViewModel(IConfigService config, IDressService dressService, ITargetingService targeting, IWorldService world, ILogService log, ILanguageService languageService)
        {
            _config = config;
            _dressService = dressService;
            _targeting = targeting;
            _world = world;
            _log = log;
            _lang = languageService;

            _undressBagName = _lang.GetString("Agents.General.NotSet");

            EnableThreadSafeCollection(Logs, _lock);

            _log.OnNewLog += entry =>
            {
                if (entry.Source == "Dress")
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Logs.Insert(0, entry);
                        if (Logs.Count > 50) Logs.RemoveAt(50);
                    });
                }
            };

            AddListCommand = new RelayCommand(AddList);
            RemoveListCommand = new RelayCommand(RemoveList);
            DressNowCommand = new RelayCommand(() => { if (SelectedList != null) { _dressService.Dress(SelectedList.Name); StatusText = _lang.GetString("Agents.Dress.Dressing"); } });
            UndressNowCommand = new RelayCommand(() => { if (SelectedList != null) { _dressService.Undress(SelectedList.Name); StatusText = _lang.GetString("Agents.Dress.Undressing"); } });
            StopCommand = new AsyncRelayCommand(() => _dressService.StopAsync());
            ReadCurrentCommand = new RelayCommand(ReadCurrent);
            SetUndressBagCommand = new AsyncRelayCommand(SetUndressBagAsync);
            SetSlotCommand = new AsyncRelayCommand<DressSlotViewModel>(SetSlotAsync);
            ClearSlotCommand = new RelayCommand<DressSlotViewModel>(ClearSlot);
            ClearAllSlotsCommand = new RelayCommand(ClearAllSlots);

            LoadConfig();
        }

        private void LoadConfig()
        {
            Lists.Clear();
            foreach (var list in _config.CurrentProfile.DressLists)
            {
                Lists.Add(list);
            }
            SelectedList = Lists.FirstOrDefault(l => l.Name == _config.CurrentProfile.ActiveDressList) ?? Lists.FirstOrDefault();
        }

        private void AddList()
        {
            var name = $"{_lang.GetString("Agents.General.NewList")} {Lists.Count + 1}";
            var newList = new DressList { Name = name };
            _config.CurrentProfile.DressLists.Add(newList);
            Lists.Add(newList);
            SelectedList = newList;
            _config.Save();
        }

        private void RemoveList()
        {
            if (SelectedList != null && Lists.Count > 1)
            {
                _config.CurrentProfile.DressLists.Remove(SelectedList);
                Lists.Remove(SelectedList);
                SelectedList = Lists.FirstOrDefault();
                _config.Save();
            }
        }

        private async Task SetUndressBagAsync()
        {
            StatusText = _lang.GetString("Agents.General.SelectContainer");
            var targetInfo = await _targeting.AcquireTargetAsync(); var serial = targetInfo.Serial;
            if (serial != 0)
            {
                UndressBagSerial = serial;
                UndressBagName = $"0x{serial:X8}";
                if (SelectedList != null)
                {
                    SelectedList.UndressBag = serial;
                    _config.Save();
                }
                StatusText = $"{_lang.GetString("Agents.General.ContainerSet")} {UndressBagName}";
            }
        }

        private void ReadCurrent()
        {
            if (SelectedList == null || _world.Player == null) return;

            SelectedList.LayerItems.Clear();
            var equippedItems = _world.GetItemsInContainer(_world.Player.Serial);
            foreach (var item in equippedItems)
            {
                if (item.Layer != 0 && item.Layer != 21) // Evitiamo backpack e altri layer di sistema
                {
                    SelectedList.LayerItems[item.Layer] = item.Serial;
                }
            }
            
            UpdateSlots();
            _config.Save();
            StatusText = _lang.GetString("Agents.Dress.Complete");
        }

        private async Task SetSlotAsync(DressSlotViewModel? slot)
        {
            if (slot == null || SelectedList == null) return;

            StatusText = _lang.GetString("Agents.General.SelectItem");
            var targetInfo = await _targeting.AcquireTargetAsync(); var serial = targetInfo.Serial;
            if (serial != 0)
            {
                SelectedList.LayerItems[(byte)slot.Layer] = serial;
                slot.Serial = serial;
                slot.ItemName = _world.FindItem(serial)?.Name ?? $"0x{serial:X8}";
                _config.Save();
                StatusText = $"{slot.Layer} set to {slot.ItemName}";
            }
        }

        private void ClearSlot(DressSlotViewModel? slot)
        {
            if (slot == null || SelectedList == null) return;

            SelectedList.LayerItems.Remove((byte)slot.Layer);
            slot.Serial = 0;
            slot.ItemName = _lang.GetString("Agents.General.NotSet");
            _config.Save();
            StatusText = $"{slot.Layer} cleared.";
        }

        private void ClearAllSlots()
        {
            if (SelectedList == null) return;

            SelectedList.LayerItems.Clear();
            foreach (var slot in CurrentSlots)
            {
                slot.Serial = 0;
                slot.ItemName = _lang.GetString("Agents.General.NotSet");
            }
            _config.Save();
            StatusText = _lang.GetString("Agents.General.ClearList");
        }

        partial void OnSelectedListChanged(DressList? value)
        {
            if (value != null)
            {
                _config.CurrentProfile.ActiveDressList = value.Name;
                DragDelay = value.DragDelay;
                RemoveConflict = value.RemoveConflict;
                Use3D = value.Use3D;
                UndressBagSerial = value.UndressBag;
                UndressBagName = UndressBagSerial != 0 ? $"0x{UndressBagSerial:X8}" : _lang.GetString("Agents.General.NotSet");
                _config.Save();
            }
            UpdateSlots();
        }

        partial void OnDragDelayChanged(int value) { if (SelectedList != null) { SelectedList.DragDelay = value; _config.Save(); } }
        partial void OnRemoveConflictChanged(bool value) { if (SelectedList != null) { SelectedList.RemoveConflict = value; _config.Save(); } }
        partial void OnUse3DChanged(bool value) { if (SelectedList != null) { SelectedList.Use3D = value; _config.Save(); } }

        private void UpdateSlots()
        {
            CurrentSlots.Clear();
            if (SelectedList == null) return;

            // Definiamo i layer che vogliamo mostrare nella UI (quelli principali da combattimento/vestizione)
            var displayLayers = new[] 
            { 
                Layer.RightHand, Layer.LeftHand, Layer.Head, Layer.Neck, 
                Layer.InnerTorso, Layer.MiddleTorso, Layer.OuterTorso, 
                Layer.Gloves, Layer.Arms, Layer.Cloak, Layer.Shoes, 
                Layer.Pants, Layer.OuterLegs, Layer.Ring, Layer.Bracelet, 
                Layer.Earrings, Layer.Talisman, Layer.Waist 
            };

            foreach (var layer in displayLayers)
            {
                uint serial = 0;
                SelectedList.LayerItems.TryGetValue((byte)layer, out serial);
                
                CurrentSlots.Add(new DressSlotViewModel 
                { 
                    Layer = layer, 
                    Serial = serial,
                    ItemName = serial != 0 ? (_world.FindItem(serial)?.Name ?? $"0x{serial:X8}") : _lang.GetString("Agents.General.NotSet")
                });
            }
        }
    }

    public partial class DressSlotViewModel : ObservableObject
    {
        [ObservableProperty]
        private Layer _layer;

        [ObservableProperty]
        private uint _serial;

        [ObservableProperty]
        private string _itemName = string.Empty;
    }
}
