using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
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

        public InspectorViewModel(ITargetingService targetingService, IWorldService worldService, ILanguageService languageService)
        {
            _targetingService = targetingService;
            _worldService = worldService;
            _languageService = languageService;

            _statusMessage = _languageService.GetString("Inspector.Status.ClickInspect");
            _targetingService.TargetReceived += OnTargetReceived;
        }

        [RelayCommand]
        private void StartInspect()
        {
            IsWaitingForTarget = true;
            StatusMessage = _languageService.GetString("Inspector.Status.WaitingTarget");
            _targetingService.RequestTarget();
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
