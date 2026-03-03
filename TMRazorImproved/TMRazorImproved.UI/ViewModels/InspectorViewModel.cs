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

        [ObservableProperty]
        private UOEntity? _inspectedEntity;

        [ObservableProperty]
        private string _statusMessage;

        [ObservableProperty]
        private bool _isWaitingForTarget;

        [ObservableProperty]
        private string _gumpInfo = "No Gump inspected.";

        [ObservableProperty]
        private UOGump? _inspectedGump;

        [ObservableProperty]
        private string _mapInfo = "No Map data inspected.";

        [ObservableProperty]
        private int _playerX;

        [ObservableProperty]
        private int _playerY;

        [ObservableProperty]
        private int _mapId;

        public ObservableCollection<string> RecentSerials { get; } = new();

        public ObservableCollection<GumpControl> GumpControls { get; } = new();

        public InspectorViewModel(ITargetingService targetingService, IWorldService worldService, ILanguageService languageService)
        {
            _targetingService = targetingService;
            _worldService = worldService;
            _languageService = languageService;

            _statusMessage = _languageService.GetString("Inspector.Status.ClickInspect");
            _targetingService.TargetReceived += OnTargetReceived;
            
            EnableThreadSafeCollection(RecentSerials, new object());
            EnableThreadSafeCollection(GumpControls, new object());

            // Inizializza posizione iniziale
            UpdatePlayerPosition();
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
            StatusMessage = "Mappa aggiornata alla posizione attuale.";
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
            StatusMessage = "Ispezione Gump in corso...";
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
                    StatusMessage = "Gump ispezionato con successo.";
                }
                else
                {
                    GumpInfo = "Nessun Gump attivo trovato.";
                    StatusMessage = "Nessun Gump trovato.";
                }
            });
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
                StatusMessage = "Codice Python copiato negli appunti.";
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
                StatusMessage = "Codice C# copiato negli appunti.";
            }
        }

        [RelayCommand]
        private void InspectMap()
        {
            IsWaitingForTarget = true;
            StatusMessage = "Seleziona una locazione sulla mappa...";
            // TODO: Implementare targeting specifico per locazione/terreno nel targetingService
        }

        private void OnTargetReceived(uint serial)
        {
            if (!IsWaitingForTarget) return;

            RunOnUIThread(() =>
            {
                var entity = _worldService.FindEntity(serial);
                if (entity != null)
                {
                    InspectedEntity = entity;
                    StatusMessage = string.Format(_languageService.GetString("Inspector.Status.Inspected"), serial);
                    
                    if (!RecentSerials.Contains($"0x{serial:X8}"))
                    {
                        RecentSerials.Insert(0, $"0x{serial:X8}");
                        if (RecentSerials.Count > 10) RecentSerials.RemoveAt(10);
                    }
                }
                else
                {
                    StatusMessage = string.Format(_languageService.GetString("Inspector.Status.NotFound"), serial);
                    InspectedEntity = null;
                }
                IsWaitingForTarget = false;
            });
        }
    }
}
