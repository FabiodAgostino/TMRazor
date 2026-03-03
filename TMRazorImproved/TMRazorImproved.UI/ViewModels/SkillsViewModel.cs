using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.UI.ViewModels
{
    public sealed partial class SkillsViewModel : ViewModelBase
    {
        private readonly ISkillsService _skillsService;
        private readonly object _skillsLock = new();

        [ObservableProperty]
        private double _totalReal;

        [ObservableProperty]
        private double _totalBase;

        [ObservableProperty]
        private bool _displayChanges = true;

        [ObservableProperty]
        private SkillLock _selectedLockAll = SkillLock.Lock;

        public ObservableCollection<SkillInfo> Skills { get; } = new();
        public ObservableCollection<SkillLock> LockOptions { get; } = new() { SkillLock.Up, SkillLock.Down, SkillLock.Lock };

        public IRelayCommand ResetDeltaCommand { get; }
        public IRelayCommand SetAllLocksCommand { get; }
        public IRelayCommand CopyAllCommand { get; }

        public SkillsViewModel(ISkillsService skillsService)
        {
            _skillsService = skillsService;
            ResetDeltaCommand = new RelayCommand(() => _skillsService.ResetDelta());
            SetAllLocksCommand = new RelayCommand(SetAllLocks);
            CopyAllCommand = new RelayCommand(CopyAll);

            EnableThreadSafeCollection(Skills, _skillsLock);
            RefreshSkills();

            UpdateTotals();
        }

        private void RefreshSkills()
        {
            SyncCollection(Skills, _skillsService.Skills, _skillsLock);
        }

        private void SetAllLocks()
        {
            foreach (var skill in Skills)
            {
                skill.Lock = SelectedLockAll;
            }
            StatusText = $"All skills set to {SelectedLockAll}";
        }

        private void CopyAll()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Skill Name\tReal\tBase\tCap");
            foreach (var s in Skills)
            {
                sb.AppendLine($"{s.Name}\t{s.Value:F1}\t{s.BaseValue:F1}\t{s.Cap:F1}");
            }
            System.Windows.Clipboard.SetText(sb.ToString());
            StatusText = "Skill list copied to clipboard.";
        }

        private void UpdateTotals()
        {
            TotalReal = _skillsService.TotalReal;
            TotalBase = _skillsService.TotalBase;
        }
    }
}
