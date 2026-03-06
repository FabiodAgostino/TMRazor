using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Messages;

namespace TMRazorImproved.UI.ViewModels
{
    public sealed partial class SkillsViewModel : ViewModelBase, IRecipient<SkillsUpdatedMessage>
    {
        private readonly ISkillsService _skillsService;
        private readonly IMessenger _messenger;
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
        public ObservableCollection<SkillGainRecord> GainHistory { get; } = new();

        public IRelayCommand ResetDeltaCommand { get; }
        public IRelayCommand SetAllLocksCommand { get; }
        public IRelayCommand CopyAllCommand { get; }

        public SkillsViewModel(ISkillsService skillsService, IMessenger messenger)
        {
            _skillsService = skillsService;
            _messenger = messenger;
            ResetDeltaCommand = new RelayCommand(() => _skillsService.ResetDelta());
            SetAllLocksCommand = new RelayCommand(SetAllLocks);
            CopyAllCommand = new RelayCommand(CopyAll);

            EnableThreadSafeCollection(Skills, _skillsLock);
            EnableThreadSafeCollection(GainHistory, new object());

            RefreshSkills();
            RefreshHistory();
            UpdateTotals();

            _messenger.Register<SkillsUpdatedMessage>(this);
        }

        public void Receive(SkillsUpdatedMessage message)
        {
            RunOnUIThread(() =>
            {
                // SkillInfo properties sono aggiornate da un thread di background (timer).
                // WPF non garantisce il marshaling cross-thread degli eventi PropertyChanged
                // per gli item di un DataGrid → forziamo un refresh esplicito della view.
                System.Windows.Data.CollectionViewSource.GetDefaultView(Skills).Refresh();
                UpdateTotals();
                RefreshHistory();
            });
        }

        private void RefreshSkills()
        {
            SyncCollection(Skills, _skillsService.Skills, _skillsLock);
            foreach (var skill in Skills)
                skill.PropertyChanged += OnSkillLockChanged;
        }

        private void OnSkillLockChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SkillInfo.Lock) && sender is SkillInfo skill)
                _skillsService.SetLock(skill.ID, skill.Lock);
        }

        private void RefreshHistory()
        {
            // Ottimizzazione: aggiorna solo i nuovi record
            var newRecords = _skillsService.GainHistory.Skip(GainHistory.Count).ToList();
            foreach (var rec in newRecords)
            {
                GainHistory.Insert(0, rec); // Inserisci in cima per vederli subito
            }
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
