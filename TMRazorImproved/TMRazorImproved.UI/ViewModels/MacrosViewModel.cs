using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class MacrosViewModel : ViewModelBase
    {
        private readonly IMacrosService _macrosService;

        [ObservableProperty]
        private string? _selectedMacro;

        [ObservableProperty]
        private bool _isRecording;

        [ObservableProperty]
        private bool _isPlaying;

        [ObservableProperty]
        private string _newMacroName = string.Empty;

        public ObservableCollection<string> Macros => _macrosService.MacroList;

        public ObservableCollection<MacroStep> MacroSteps { get; } = new();

        public MacrosViewModel(IMacrosService macrosService)
        {
            _macrosService = macrosService;
            _isRecording = _macrosService.IsRecording;
            _isPlaying = _macrosService.IsPlaying;
            
            // Inizializza la lista
            _macrosService.LoadMacros();
        }

        partial void OnSelectedMacroChanged(string? value)
        {
            MacroSteps.Clear();
            if (!string.IsNullOrEmpty(value))
            {
                var steps = _macrosService.GetSteps(value);
                foreach (var step in steps)
                {
                    MacroSteps.Add(step);
                }
            }
        }

        [RelayCommand]
        private void Play()
        {
            if (!string.IsNullOrEmpty(SelectedMacro))
            {
                _macrosService.Play(SelectedMacro);
                IsPlaying = _macrosService.IsPlaying;
            }
        }

        [RelayCommand]
        private void Stop()
        {
            _macrosService.Stop();
            IsPlaying = _macrosService.IsPlaying;
            IsRecording = _macrosService.IsRecording;
        }

        [RelayCommand]
        private void Record()
        {
            if (!string.IsNullOrEmpty(NewMacroName))
            {
                _macrosService.Record(NewMacroName);
                IsRecording = _macrosService.IsRecording;
                NewMacroName = string.Empty;
            }
        }

        [RelayCommand]
        private void SaveSteps()
        {
            if (!string.IsNullOrEmpty(SelectedMacro))
            {
                _macrosService.Save(SelectedMacro, MacroSteps.ToList());
            }
        }

        [RelayCommand]
        private void Delete()
        {
            if (!string.IsNullOrEmpty(SelectedMacro))
            {
                _macrosService.Delete(SelectedMacro);
            }
        }

        [RelayCommand]
        private void RemoveStep(MacroStep? step)
        {
            if (step != null)
            {
                MacroSteps.Remove(step);
            }
        }
    }
}
