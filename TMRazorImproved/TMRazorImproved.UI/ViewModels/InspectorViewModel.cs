using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class InspectorViewModel : ViewModelBase,
        CommunityToolkit.Mvvm.Messaging.IRecipient<TMRazorImproved.Shared.Messages.NavigateToInspectorMessage>,
        CommunityToolkit.Mvvm.Messaging.IRecipient<TMRazorImproved.Shared.Messages.GumpResponseLogMessage>
    {
        private readonly ITargetingService _targetingService;
        private readonly IWorldService _worldService;
        private readonly ILanguageService _languageService;
        private readonly IPacketService _packetService;
        private readonly IMapService _mapService;
        private readonly IMapDataProvider _mapDataProvider;
        private readonly CommunityToolkit.Mvvm.Messaging.IMessenger _messenger;

        public MapViewModel Map { get; }

        [ObservableProperty]
        private UOEntity? _inspectedEntity;

        partial void OnInspectedEntityChanged(UOEntity? value)
        {
            OnPropertyChanged(nameof(InspectedLayer));
            OnPropertyChanged(nameof(InspectedContainer));
            OnPropertyChanged(nameof(InspectedAmount));
            OnPropertyChanged(nameof(InspectedRawFlags));
            OnPropertyChanged(nameof(IsItemInspected));
            OnPropertyChanged(nameof(IsMobileInspected));
            OnPropertyChanged(nameof(MobileHitsText));
            OnPropertyChanged(nameof(MobileManaText));
            OnPropertyChanged(nameof(MobileStamText));
            OnPropertyChanged(nameof(MobileHitsPercent));
            OnPropertyChanged(nameof(MobileManaPercent));
            OnPropertyChanged(nameof(MobileStamPercent));
            OnPropertyChanged(nameof(MobileNotoriety));
            OnPropertyChanged(nameof(MobileDirection));
            OnPropertyChanged(nameof(MobileStats));
            RefreshItemFlags(value);
            RefreshMobileFlags(value);
        }

        private void RefreshItemFlags(UOEntity? entity)
        {
            RunOnUIThread(() =>
            {
                ItemFlags.Clear();
                if (entity is not Item it) return;
                ItemFlags.Add(new ItemFlagEntry("Visible",      it.Visible));
                ItemFlags.Add(new ItemFlagEntry("Movable",      it.Movable));
                ItemFlags.Add(new ItemFlagEntry("OnGround",     it.OnGround));
                ItemFlags.Add(new ItemFlagEntry("IsContainer",  it.IsContainer));
                ItemFlags.Add(new ItemFlagEntry("IsCorpse",     it.IsCorpse));
                ItemFlags.Add(new ItemFlagEntry("IsDoor",       it.IsDoor));
                ItemFlags.Add(new ItemFlagEntry("IsPotion",     it.IsPotion));
                ItemFlags.Add(new ItemFlagEntry("IsTwoHanded",  it.IsTwoHanded));
                ItemFlags.Add(new ItemFlagEntry("IsSearchable", it.IsSearchable));
                ItemFlags.Add(new ItemFlagEntry("IsResource",   it.IsResource));
            });
        }

        private void RefreshMobileFlags(UOEntity? entity)
        {
            RunOnUIThread(() =>
            {
                MobileFlags.Clear();
                if (entity is not Mobile m) return;
                MobileFlags.Add(new ItemFlagEntry("Visible",    m.Visible));
                MobileFlags.Add(new ItemFlagEntry("Poisoned",   m.IsPoisoned));
                MobileFlags.Add(new ItemFlagEntry("YellowHits", m.IsYellowHits));
                MobileFlags.Add(new ItemFlagEntry("Paralyzed",  m.Paralyzed));
                MobileFlags.Add(new ItemFlagEntry("Flying",     m.Flying));
                MobileFlags.Add(new ItemFlagEntry("Ghost",      m.IsGhost));
                MobileFlags.Add(new ItemFlagEntry("WarMode",    m.WarMode));
                MobileFlags.Add(new ItemFlagEntry("Mounted",    m.Mounted));
                MobileFlags.Add(new ItemFlagEntry("CanRename",  m.CanRename));
                MobileFlags.Add(new ItemFlagEntry("Female",     m.Female));
            });
        }

        [ObservableProperty]
        private string _statusMessage;

        [ObservableProperty]
        private bool _isWaitingForTarget;

        [ObservableProperty]
        private string _gumpInfo;

        [ObservableProperty]
        private UOGump? _inspectedGump;

        [ObservableProperty]
        private string _mapInfo;

        [ObservableProperty]
        private int _playerX;

        [ObservableProperty]
        private int _playerY;

        [ObservableProperty]
        private int _mapId;

        public ObservableCollection<string> RecentSerials { get; } = new();

        public ObservableCollection<GumpControl> GumpControls { get; } = new();

        public ObservableCollection<UOGump> OpenGumps { get; } = new();

        public ObservableCollection<string> GumpResponseHistory { get; } = new();

        public string InspectedLayer => (InspectedEntity is Item item) ? item.Layer.ToString() : "N/A";
        public string InspectedContainer => (InspectedEntity is Item item2) ? $"0x{item2.Container:X8}" : "N/A";
        public string InspectedAmount => (InspectedEntity is Item item3) ? item3.Amount.ToString() : "N/A";
        public string InspectedRawFlags => InspectedEntity != null ? $"0x{InspectedEntity.Flags:X2}" : "N/A";
        public bool IsItemInspected => InspectedEntity is Item;
        public bool IsMobileInspected => InspectedEntity is Mobile;

        // Mobile inspector computed properties
        public string MobileHitsText => InspectedEntity is Mobile m ? $"{m.Hits} / {m.HitsMax}" : "N/A";
        public string MobileManaText => InspectedEntity is Mobile m2 ? $"{m2.Mana} / {m2.ManaMax}" : "N/A";
        public string MobileStamText => InspectedEntity is Mobile m3 ? $"{m3.Stam} / {m3.StamMax}" : "N/A";
        public double MobileHitsPercent => InspectedEntity is Mobile m4 && m4.HitsMax > 0 ? (double)m4.Hits / m4.HitsMax * 100 : 0;
        public double MobileManaPercent => InspectedEntity is Mobile m5 && m5.ManaMax > 0 ? (double)m5.Mana / m5.ManaMax * 100 : 0;
        public double MobileStamPercent => InspectedEntity is Mobile m6 && m6.StamMax > 0 ? (double)m6.Stam / m6.StamMax * 100 : 0;

        public string MobileNotoriety => (InspectedEntity is Mobile mn) ? mn.Notoriety switch
        {
            1 => "1 - Innocent (Blue)",
            2 => "2 - Friend (Green)",
            3 => "3 - Neutral (Gray)",
            4 => "4 - Criminal (Gray)",
            5 => "5 - Enemy (Orange)",
            6 => "6 - Murderer (Red)",
            7 => "7 - Invulnerable (Yellow)",
            _ => $"{mn.Notoriety} - Unknown"
        } : "N/A";

        public string MobileDirection => (InspectedEntity is Mobile md) ? (md.Direction & 0x07) switch
        {
            0 => "North",
            1 => "North-East",
            2 => "East",
            3 => "South-East",
            4 => "South",
            5 => "South-West",
            6 => "West",
            7 => "North-West",
            _ => md.Direction.ToString()
        } : "N/A";

        public string MobileStats => InspectedEntity is Mobile ms
            ? $"STR: {ms.Str}  DEX: {ms.Dex}  INT: {ms.Int}  |  AR: {ms.AR}  Fire: {ms.FireResist}  Cold: {ms.ColdResist}  Poison: {ms.PoisonResist}  Energy: {ms.EnergyResist}"
            : "N/A";

        public ObservableCollection<ItemFlagEntry> ItemFlags { get; } = new();
        public ObservableCollection<ItemFlagEntry> MobileFlags { get; } = new();

        // Tile Inspector
        [ObservableProperty] private int _tileX;
        [ObservableProperty] private int _tileY;
        [ObservableProperty] private int _tileMapId;
        [ObservableProperty] private string _landTileInfo = "N/A";
        public ObservableCollection<TileDisplayEntry> StaticTiles { get; } = new();

        // FR-079: Script Debug Inspector
        private IScriptingService? _scriptingService;
        public ObservableCollection<SharedDataEntry> SharedScriptData { get; } = new();
        public ObservableCollection<ScriptTimerEntry> ActiveTimers      { get; } = new();

        public InspectorViewModel(ITargetingService targetingService, IWorldService worldService, ILanguageService languageService, IPacketService packetService, IMapService mapService, IMapDataProvider mapDataProvider, CommunityToolkit.Mvvm.Messaging.IMessenger messenger, IScriptingService? scriptingService = null)
        {
            _targetingService = targetingService;
            _worldService = worldService;
            _languageService = languageService;
            _packetService = packetService;
            _mapService = mapService;
            _mapDataProvider = mapDataProvider;
            _messenger = messenger;
            _scriptingService = scriptingService;

            Map = new MapViewModel(_worldService, _mapService);

            _statusMessage = _languageService.GetString("Inspector.Status.ClickInspect");
            _gumpInfo = _languageService.GetString("Inspector.Gump.NoGumpInspected");
            _mapInfo = _languageService.GetString("Inspector.Map.NoMapData");

            _targetingService.TargetReceived += OnTargetReceived;
            _messenger.RegisterAll(this);
            
            EnableThreadSafeCollection(RecentSerials, new object());
            EnableThreadSafeCollection(GumpControls, new object());
            EnableThreadSafeCollection(OpenGumps, new object());
            EnableThreadSafeCollection(GumpResponseHistory, new object());
            EnableThreadSafeCollection(MobileFlags, new object());
            EnableThreadSafeCollection(StaticTiles, new object());

            // Inizializza posizione iniziale
            UpdatePlayerPosition();
            RefreshGumpsList();
        }

        private void UpdatePlayerPosition()
        {
            if (_worldService.Player != null)
            {
                PlayerX = _worldService.Player.X;
                PlayerY = _worldService.Player.Y;
                MapId = _worldService.Player.MapId;
                MapInfo = $"Map: {MapId} | X: {PlayerX}, Y: {PlayerY}";
            }
        }

        [RelayCommand]
        private void CopySerial()
        {
            if (InspectedEntity == null) return;
            Clipboard.SetText($"0x{InspectedEntity.Serial:X8}");
            StatusMessage = _languageService.GetString("Inspector.Status.SerialCopied");
        }

        [RelayCommand]
        private void RefreshMap()
        {
            UpdatePlayerPosition();
            StatusMessage = _languageService.GetString("Inspector.Status.MapUpdated");
        }

        [RelayCommand]
        private void RefreshGumpsList()
        {
            RunOnUIThread(() => {
                OpenGumps.Clear();
                foreach (var gump in _worldService.OpenGumps.Values)
                {
                    OpenGumps.Add(gump);
                }
                StatusMessage = string.Format(_languageService.GetString("Inspector.Status.GumpListUpdated"), OpenGumps.Count);
            });
        }

        [RelayCommand]
        private void ClearGumpHistory()
        {
            GumpResponseHistory.Clear();
        }

        [RelayCommand]
        private void InspectSpecificGump(UOGump? gump)
        {
            if (gump == null) return;
            
            RunOnUIThread(() => {
                InspectedGump = gump;
                GumpControls.Clear();
                
                GumpInfo = $"Gump ID: 0x{gump.GumpId:X8} | Serial: 0x{gump.Serial:X8}";
                foreach (var control in gump.Controls)
                {
                    GumpControls.Add(control);
                }
                StatusMessage = string.Format(_languageService.GetString("Inspector.Status.GumpInspected"), gump.GumpId);
            });

            // Cambia la selezione del tab corrente se necessario (potrebbe richiedere binding sul TabControl, 
            // ma l'utente può cliccarlo manualmente)
        }

        [RelayCommand]
        private void StartInspect()
        {
            IsWaitingForTarget = true;
            StatusMessage = _languageService.GetString("Inspector.Status.WaitingTarget");
            _targetingService.RequestTarget();
        }

        [RelayCommand]
        private void InspectGump()
        {
            // Logica per ispezionare l'ultimo gump aperto
            StatusMessage = _languageService.GetString("Inspector.Status.InspectingGump");
            var lastGump = _worldService.CurrentGump;
            
            RunOnUIThread(() => {
                InspectedGump = lastGump;
                GumpControls.Clear();
                
                if (lastGump != null)
                {
                    GumpInfo = $"Gump ID: 0x{lastGump.GumpId:X8} | Serial: 0x{lastGump.Serial:X8}";
                    foreach (var control in lastGump.Controls)
                    {
                        GumpControls.Add(control);
                    }
                    StatusMessage = _languageService.GetString("Inspector.Status.GumpSuccess");
                }
                else
                {
                    GumpInfo = _languageService.GetString("Inspector.Status.NoGumpFound");
                    StatusMessage = _languageService.GetString("Inspector.Status.NoGumpFound");
                }
            });
        }

        [RelayCommand]
        private void ExecuteGumpAction(GumpControl? control)
        {
            if (InspectedGump == null || control == null) return;

            if (control is GumpButton btn)
            {
                byte[] pkt = TMRazorImproved.Core.Utilities.PacketBuilder.RespondGump(InspectedGump.Serial, InspectedGump.GumpId, btn.ButtonId);
                _packetService.SendToServer(pkt);
                StatusMessage = string.Format(_languageService.GetString("Inspector.Status.GumpResponse"), btn.ButtonId);
            }
            else
            {
                StatusMessage = _languageService.GetString("Inspector.Status.ActionNotSupported");
            }
        }

        [RelayCommand]
        private void CopyAsPython(GumpControl? control)
        {
            if (InspectedGump == null || control == null) return;

            string code = "";
            if (control is GumpButton btn)
            {
                code = $"Gumps.SendAction(0x{InspectedGump.Serial:X8}, {btn.ButtonId})";
            }
            else if (control is GumpText txt)
            {
                code = $"# Text: {txt.Text}\n# ID: {txt.StringId}";
            }

            if (!string.IsNullOrEmpty(code))
            {
                Clipboard.SetText(code);
                StatusMessage = _languageService.GetString("Inspector.Status.PythonCopied");
            }
        }

        [RelayCommand]
        private void CopyAsCSharp(GumpControl? control)
        {
            if (InspectedGump == null || control == null) return;

            string code = "";
            if (control is GumpButton btn)
            {
                code = $"Gumps.SendAction(0x{InspectedGump.Serial:X8}u, {btn.ButtonId});";
            }

            if (!string.IsNullOrEmpty(code))
            {
                Clipboard.SetText(code);
                StatusMessage = _languageService.GetString("Inspector.Status.CSharpCopied");
            }
        }

        [RelayCommand]
        private void InspectMap()
        {
            IsWaitingForTarget = true;
            StatusMessage = _languageService.GetString("Inspector.Status.SelectMapLocation");
            _targetingService.RequestLocationTarget();
        }

        [RelayCommand]
        private void InspectTile()
        {
            RunOnUIThread(() =>
            {
                StaticTiles.Clear();
                if (!_mapDataProvider.IsMapAvailable(TileMapId))
                {
                    LandTileInfo = "Map not available (SDK not initialized or invalid map ID)";
                    return;
                }

                try
                {
                    var land = _mapDataProvider.GetLandTile(TileX, TileY, TileMapId);
                    var landData = _mapDataProvider.GetLandData(land.Id);
                    LandTileInfo = $"ID: 0x{land.Id:X4} ({land.Id})  Z: {land.Z}  Name: {(string.IsNullOrEmpty(landData.Name) ? "unknown" : landData.Name)}  Flags: 0x{(long)landData.Flags:X}";
                }
                catch (Exception ex)
                {
                    LandTileInfo = $"Error: {ex.Message}";
                }

                try
                {
                    var statics = _mapDataProvider.GetStaticTiles(TileX, TileY, TileMapId);
                    foreach (var t in statics)
                    {
                        string name = "unknown";
                        try { name = _mapDataProvider.GetItemData(t.Id).Name; } catch { }
                        StaticTiles.Add(new TileDisplayEntry(
                            $"0x{t.Id:X4} ({t.Id})", t.Hue, t.Z, name));
                    }
                }
                catch (Exception ex)
                {
                    StaticTiles.Add(new TileDisplayEntry("Error", 0, 0, ex.Message));
                }

                StatusMessage = $"Tile inspected at X={TileX} Y={TileY} Map={TileMapId} — {StaticTiles.Count} static(s)";
            });
        }

        [RelayCommand]
        private void FillTileCoordsFromPlayer()
        {
            if (_worldService.Player != null)
            {
                TileX = _worldService.Player.X;
                TileY = _worldService.Player.Y;
                TileMapId = _worldService.Player.MapId;
            }
        }

        // FR-079: Refresh shared script data and active timers
        [RelayCommand]
        private void RefreshScriptDebug()
        {
            RunOnUIThread(() =>
            {
                SharedScriptData.Clear();
                if (_scriptingService != null)
                {
                    foreach (var kv in _scriptingService.GetSharedScriptData())
                        SharedScriptData.Add(new SharedDataEntry(kv.Key, kv.Value?.ToString() ?? "null", kv.Value?.GetType().Name ?? "null"));
                }

                ActiveTimers.Clear();
                if (_scriptingService != null)
                {
                    foreach (var t in _scriptingService.GetActiveTimers())
                        ActiveTimers.Add(new ScriptTimerEntry(t.Name, t.IntervalMs, t.TimeLeftMs, t.IsRunning));
                }

                StatusMessage = $"Script debug refreshed — {SharedScriptData.Count} shared value(s), {ActiveTimers.Count} timer(s)";
            });
        }

        public void Receive(TMRazorImproved.Shared.Messages.NavigateToInspectorMessage message)
        {
            if (message.Value is UOGump gump)
            {
                InspectSpecificGump(gump);
            }
            else if (message.Value is UOEntity entity)
            {
                InspectedEntity = entity;
            }
            else if (message.Value is uint serial)
            {
                var ent = _worldService.FindEntity(serial);
                if (ent != null) InspectedEntity = ent;
            }
        }

        public void Receive(TMRazorImproved.Shared.Messages.GumpResponseLogMessage message)
        {
            var (serial, gumpId, buttonId, switches, textEntries) = message.Value;

            string log = $"[{DateTime.Now:HH:mm:ss}] Gump 0x{gumpId:X8} - Button: {buttonId}";
            if (switches.Count > 0)
                log += $" | Switches: {string.Join(", ", switches)}";
            if (textEntries.Count > 0)
                log += $" | Texts: {string.Join(", ", textEntries.Select(kv => $"{kv.Key}='{kv.Value}'"))}";

            RunOnUIThread(() => {
                GumpResponseHistory.Insert(0, log);
                if (GumpResponseHistory.Count > 50) GumpResponseHistory.RemoveAt(50);
            });
        }

        private void OnTargetReceived(TMRazorImproved.Shared.Models.TargetInfo info)
        {
            if (!IsWaitingForTarget) return;

            RunOnUIThread(() =>
            {
                var entity = _worldService.FindEntity(info.Serial);
                if (entity != null)
                {
                    InspectedEntity = entity;
                    StatusMessage = string.Format(_languageService.GetString("Inspector.Status.Inspected"), info.Serial);

                    if (!RecentSerials.Contains($"0x{info.Serial:X8}"))
                    {
                        RecentSerials.Insert(0, $"0x{info.Serial:X8}");
                        if (RecentSerials.Count > 10) RecentSerials.RemoveAt(10);
                    }
                }
                else if (info.Serial == 0)
                {
                    // Ground target
                    StatusMessage = string.Format("Location Selected: X={0}, Y={1}, Z={2}", info.X, info.Y, info.Z);
                }
                else
                {
                    StatusMessage = string.Format(_languageService.GetString("Inspector.Status.NotFound"), info.Serial);
                }
                IsWaitingForTarget = false;
            });
        }
    }

    public record ItemFlagEntry(string FlagName, bool IsSet);
    public record TileDisplayEntry(string Graphic, int Hue, int Z, string Name);

    // FR-079
    public record SharedDataEntry(string Key, string ValueStr, string TypeName);
    public record ScriptTimerEntry(string Name, double IntervalMs, double TimeLeftMs, bool IsRunning);
}