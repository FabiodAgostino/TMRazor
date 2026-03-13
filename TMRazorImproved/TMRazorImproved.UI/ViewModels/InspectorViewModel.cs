using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class InspectorViewModel : ViewModelBase
    {
        private readonly ITargetingService _targetingService;
        private readonly IWorldService _worldService;
        private readonly ILanguageService _languageService;
        private readonly IPacketService _packetService;

        [ObservableProperty]
        private UOEntity? _inspectedEntity;

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

        public InspectorViewModel(ITargetingService targetingService, IWorldService worldService, ILanguageService languageService, IPacketService packetService)
        {
            _targetingService = targetingService;
            _worldService = worldService;
            _languageService = languageService;
            _packetService = packetService;

            _statusMessage = _languageService.GetString("Inspector.Status.ClickInspect");
            _gumpInfo = _languageService.GetString("Inspector.Gump.NoGumpInspected");
            _mapInfo = _languageService.GetString("Inspector.Map.NoMapData");

            _targetingService.TargetReceived += OnTargetReceived;
            
            EnableThreadSafeCollection(RecentSerials, new object());
            EnableThreadSafeCollection(GumpControls, new object());
            EnableThreadSafeCollection(OpenGumps, new object());

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
            // TODO: Implementare targeting specifico per locazione/terreno nel targetingService
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
    }
}
