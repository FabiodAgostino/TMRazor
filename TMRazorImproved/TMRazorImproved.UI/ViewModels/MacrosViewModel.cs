using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using Wpf.Ui;

using Microsoft.Win32;
using System.IO;
using TMRazorImproved.Core.Utilities;

namespace TMRazorImproved.UI.ViewModels
{
    public partial class MacrosViewModel : ViewModelBase
    {
        private readonly IMacrosService _macrosService;
        private readonly IContentDialogService _dialogService;

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

        public MacrosViewModel(IMacrosService macrosService, IContentDialogService dialogService)
        {
            _macrosService = macrosService;
            _dialogService = dialogService;
            _isRecording = _macrosService.IsRecording;
            _isPlaying = _macrosService.IsPlaying;
            
            // Inizializza la lista
            _macrosService.LoadMacros();
        }

        [RelayCommand]
        private void ImportLegacyMacro()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Legacy Macro Files (*.macro)|*.macro|All files (*.*)|*.*",
                Title = "Seleziona Macro Legacy da Importare"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string content = File.ReadAllText(openFileDialog.FileName);
                    var migratedCommands = LegacyMacroMigrator.Migrate(content);
                    
                    if (migratedCommands.Count > 0)
                    {
                        string macroName = Path.GetFileNameWithoutExtension(openFileDialog.FileName) + "_migrated";
                        var steps = migratedCommands.Select(c => new MacroStep(c, c)).ToList();
                        _macrosService.Save(macroName, steps);
                        
                        _macrosService.LoadMacros();
                        SelectedMacro = macroName;
                    }
                }
                catch (System.Exception ex)
                {
                    // Log error or show snackbar
                }
            }
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
        private async Task StopRecording()
        {
            bool wasRecordingWithoutName = IsRecording && _macrosService.ActiveMacro != null && _macrosService.ActiveMacro.StartsWith("macro_");
            string? tempName = _macrosService.ActiveMacro;

            _macrosService.StopRecording();
            IsPlaying = _macrosService.IsPlaying;
            IsRecording = _macrosService.IsRecording;

            if (wasRecordingWithoutName && tempName != null)
            {
                await PromptForMacroName(tempName);
            }
        }

        private async Task PromptForMacroName(string tempName)
        {
            var input = new Wpf.Ui.Controls.TextBox
            {
                PlaceholderText = "Inserisci il nome della macro",
                Text = string.Empty
            };

            var dialog = new Wpf.Ui.Controls.ContentDialog(_dialogService.GetDialogHost())
            {
                Title = "Salva Macro",
                Content = input,
                PrimaryButtonText = "Salva",
                CloseButtonText = "Elimina"
            };

            var result = await dialog.ShowAsync();

            if (result == Wpf.Ui.Controls.ContentDialogResult.Primary && !string.IsNullOrWhiteSpace(input.Text))
            {
                string newName = input.Text.Trim();
                _macrosService.Rename(tempName, newName);
                SelectedMacro = newName;
            }
            else if (result == Wpf.Ui.Controls.ContentDialogResult.Secondary || result == Wpf.Ui.Controls.ContentDialogResult.None)
            {
                // Se annullato o chiusa, eliminiamo la macro temporanea
                _macrosService.Delete(tempName);
            }
        }

        [RelayCommand]
        private void StartRecording()
        {
            if (!string.IsNullOrEmpty(NewMacroName))
            {
                _macrosService.StartRecording(NewMacroName);
                IsRecording = _macrosService.IsRecording;
                NewMacroName = string.Empty;
            }
            else
            {
                _macrosService.StartRecording(); // Usa nome automatico
                IsRecording = _macrosService.IsRecording;
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

        [RelayCommand]
        private void MoveStepUp(MacroStep? step)
        {
            if (step == null) return;
            int index = MacroSteps.IndexOf(step);
            if (index > 0)
            {
                MacroSteps.Move(index, index - 1);
            }
        }

        [RelayCommand]
        private void MoveStepDown(MacroStep? step)
        {
            if (step == null) return;
            int index = MacroSteps.IndexOf(step);
            if (index < MacroSteps.Count - 1)
            {
                MacroSteps.Move(index, index + 1);
            }
        }

        [RelayCommand]
        private void ClearSteps()
        {
            MacroSteps.Clear();
        }
    }
}
