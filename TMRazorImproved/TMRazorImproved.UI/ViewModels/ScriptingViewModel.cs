using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.UI.ViewModels
{
    public enum ScriptLogType { Output, Error, System }

    public record ScriptLogEntry(string Text, ScriptLogType Type, DateTime Timestamp)
    {
        public string FormattedTime => Timestamp.ToString("HH:mm:ss");
    }

    public sealed partial class ScriptingViewModel : ViewModelBase, IDisposable
    {
        private readonly IScriptingService _scriptingService;
        private readonly IScriptRecorderService? _recorder;
        private readonly object _logLock = new();

        [ObservableProperty]
        private string _scriptCode =
            "# Script Python per TMRazor\n" +
            "# Oggetti disponibili: Player, Items, Mobiles, Misc\n\n" +
            "print('Ciao da TMRazor!')\n" +
            "print('HP:', Player.Hits, '/', Player.HitsMax)\n";

        [ObservableProperty]
        private string _scriptName = "nuovo_script.py";

        [ObservableProperty]
        private bool _isRunning;

        [ObservableProperty]
        private string _executionStatus = string.Empty;

        [ObservableProperty]
        private ScriptLanguage _selectedLanguage = ScriptLanguage.Python;

        [ObservableProperty]
        private bool _isRecordingScript;

        /// <summary>Inverso di IsRecordingScript — usato per la visibilità del pulsante "Registra".</summary>
        public bool IsNotRecordingScript => !IsRecordingScript;

        partial void OnIsRecordingScriptChanged(bool value)
            => OnPropertyChanged(nameof(IsNotRecordingScript));

        public IEnumerable<ScriptLanguage> AvailableLanguages =>
            Enum.GetValues(typeof(ScriptLanguage)).Cast<ScriptLanguage>();

        public ObservableCollection<ScriptLogEntry> LogEntries { get; } = new();

        public IAsyncRelayCommand RunScriptCommand      { get; }
        public IAsyncRelayCommand StopScriptCommand     { get; }
        public IRelayCommand      ClearLogCommand       { get; }
        public IRelayCommand      NewScriptCommand      { get; }
        public IRelayCommand      OpenScriptCommand     { get; }
        public IRelayCommand      SaveScriptCommand     { get; }
        public IRelayCommand      StartRecordCommand    { get; }
        public IRelayCommand      StopRecordCommand     { get; }

        public ScriptingViewModel(IScriptingService scriptingService, IScriptRecorderService? recorder = null)
        {
            _scriptingService = scriptingService;
            _recorder         = recorder;
            EnableThreadSafeCollection(LogEntries, _logLock);

            RunScriptCommand   = new AsyncRelayCommand(RunScriptAsync,    () => !IsRunning);
            StopScriptCommand  = new AsyncRelayCommand(StopScriptAsync,   () => IsRunning);
            ClearLogCommand    = new RelayCommand(ClearLog);
            NewScriptCommand   = new RelayCommand(NewScript,   () => !IsRunning);
            OpenScriptCommand  = new RelayCommand(OpenScript,  () => !IsRunning);
            SaveScriptCommand  = new RelayCommand(SaveScript);
            StartRecordCommand = new RelayCommand(StartRecording, () => !IsRecordingScript && !IsRunning);
            StopRecordCommand  = new RelayCommand(StopRecording,  () => IsRecordingScript);

            _scriptingService.OutputReceived  += OnOutputReceived;
            _scriptingService.ErrorReceived   += OnErrorReceived;
            _scriptingService.ScriptCompleted += OnScriptCompleted;
        }

        partial void OnSelectedLanguageChanged(ScriptLanguage value)
        {
            // Aggiorna l'estensione suggerita se è uno script nuovo
            if (ScriptName.StartsWith("nuovo_script"))
            {
                ScriptName = value switch
                {
                    ScriptLanguage.Python => "nuovo_script.py",
                    ScriptLanguage.UOSteam => "nuovo_script.uos",
                    ScriptLanguage.CSharp => "nuovo_script.cs",
                    _ => ScriptName
                };
            }
        }

        // ------------------------------------------------------------------
        // Commands
        // ------------------------------------------------------------------

        private async Task RunScriptAsync()
        {
            if (string.IsNullOrWhiteSpace(ScriptCode)) return;

            AddLog($"--- Avvio '{ScriptName}' [{SelectedLanguage}] ---", ScriptLogType.System);
            IsRunning = true;
            ExecutionStatus = $"Esecuzione: {ScriptName}...";
            NotifyCommandsCanExecuteChanged();

            await _scriptingService.RunAsync(ScriptCode, SelectedLanguage, ScriptName);
        }

        private async Task StopScriptAsync()
        {
            AddLog("--- Interruzione richiesta ---", ScriptLogType.System);
            await _scriptingService.StopAsync();
        }

        private void ClearLog() => RunOnUIThread(() =>
        {
            lock (_logLock) { LogEntries.Clear(); }
        });

        private void NewScript()
        {
            ScriptCode = SelectedLanguage switch
            {
                ScriptLanguage.Python => "# Nuovo script Python\n",
                ScriptLanguage.UOSteam => "// Nuovo script UOSteam\n",
                ScriptLanguage.CSharp => "// Nuovo script C#\n",
                _ => ""
            };
            
            ScriptName = SelectedLanguage switch
            {
                ScriptLanguage.Python => "nuovo_script.py",
                ScriptLanguage.UOSteam => "nuovo_script.uos",
                ScriptLanguage.CSharp => "nuovo_script.cs",
                _ => "nuovo_script"
            };

            ClearLog();
            ExecutionStatus = string.Empty;
        }

        private void OpenScript()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Script Python (*.py)|*.py|Script UOSteam (*.uos)|*.uos|Script C# (*.cs)|*.cs|Tutti i file (*.*)|*.*",
                Title  = "Apri Script"
            };
            
            // Suggerisce il filtro basato sul linguaggio selezionato
            dialog.FilterIndex = SelectedLanguage switch
            {
                ScriptLanguage.Python => 1,
                ScriptLanguage.UOSteam => 2,
                ScriptLanguage.CSharp => 3,
                _ => 4
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                ScriptCode = File.ReadAllText(dialog.FileName);
                ScriptName = Path.GetFileName(dialog.FileName);
                
                // Tenta di auto-selezionare il linguaggio dall'estensione
                var ext = Path.GetExtension(dialog.FileName).ToLower();
                SelectedLanguage = ext switch
                {
                    ".py" => ScriptLanguage.Python,
                    ".uos" or ".txt" => ScriptLanguage.UOSteam,
                    ".cs" => ScriptLanguage.CSharp,
                    _ => SelectedLanguage
                };

                AddLog($"Caricato: {dialog.FileName} ({SelectedLanguage})", ScriptLogType.System);
            }
            catch (Exception ex)
            {
                AddLog($"Errore caricamento: {ex.Message}", ScriptLogType.Error);
            }
        }

        private void SaveScript()
        {
            var dialog = new SaveFileDialog
            {
                Filter   = "Script Python (*.py)|*.py|Script UOSteam (*.uos)|*.uos|Script C# (*.cs)|*.cs|Tutti i file (*.*)|*.*",
                FileName = ScriptName,
                Title    = "Salva Script"
            };

            dialog.FilterIndex = SelectedLanguage switch
            {
                ScriptLanguage.Python => 1,
                ScriptLanguage.UOSteam => 2,
                ScriptLanguage.CSharp => 3,
                _ => 4
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                File.WriteAllText(dialog.FileName, ScriptCode);
                ScriptName = Path.GetFileName(dialog.FileName);
                AddLog($"Salvato: {dialog.FileName}", ScriptLogType.System);
            }
            catch (Exception ex)
            {
                AddLog($"Errore salvataggio: {ex.Message}", ScriptLogType.Error);
            }
        }

        // ------------------------------------------------------------------
        // Service event handlers
        // ------------------------------------------------------------------

        private void OnOutputReceived(string line)
            => AddLog(line, ScriptLogType.Output);

        private void OnErrorReceived(string line)
            => AddLog(line, ScriptLogType.Error);

        private void OnScriptCompleted(ScriptCompletionInfo info)
        {
            RunOnUIThread(() =>
            {
                IsRunning = false;
                NotifyCommandsCanExecuteChanged();

                string status;
                if (info.WasCancelled)
                    status = $"Interrotto — {info.ScriptName} ({info.Elapsed.TotalSeconds:F1}s)";
                else if (info.Error != null)
                    status = $"Errore — {info.ScriptName} ({info.Elapsed.TotalSeconds:F1}s)";
                else
                    status = $"Completato — {info.ScriptName} ({info.Elapsed.TotalSeconds:F1}s)";

                ExecutionStatus = status;
                AddLog($"--- {status} ---", ScriptLogType.System);
            });
        }

        // ------------------------------------------------------------------
        // Log helper
        // ------------------------------------------------------------------

        private void AddLog(string text, ScriptLogType type)
        {
            var entry = new ScriptLogEntry(text, type, DateTime.Now);
            RunOnUIThread(() =>
            {
                lock (_logLock) { LogEntries.Add(entry); }
            });
        }

        // ------------------------------------------------------------------
        // Recording commands
        // ------------------------------------------------------------------

        private void StartRecording()
        {
            if (_recorder == null)
            {
                AddLog("ScriptRecorderService non disponibile.", ScriptLogType.System);
                return;
            }
            _recorder.StartRecording(SelectedLanguage);
            IsRecordingScript = true;
            NotifyCommandsCanExecuteChanged();
            AddLog($"--- Registrazione avviata ({SelectedLanguage}) ---", ScriptLogType.System);
        }

        private void StopRecording()
        {
            if (_recorder == null) return;
            _recorder.StopRecording();
            IsRecordingScript = false;

            string recorded = _recorder.GetRecordedScript();
            if (!string.IsNullOrWhiteSpace(recorded))
            {
                // Appende il codice registrato all'editor
                ScriptCode = string.IsNullOrWhiteSpace(ScriptCode)
                    ? recorded
                    : ScriptCode + Environment.NewLine + Environment.NewLine + recorded;
                AddLog("--- Registrazione inserita nell'editor ---", ScriptLogType.System);
            }
            else
            {
                AddLog("--- Registrazione vuota ---", ScriptLogType.System);
            }
            NotifyCommandsCanExecuteChanged();
        }

        private void NotifyCommandsCanExecuteChanged()
        {
            RunScriptCommand.NotifyCanExecuteChanged();
            StopScriptCommand.NotifyCanExecuteChanged();
            NewScriptCommand.NotifyCanExecuteChanged();
            OpenScriptCommand.NotifyCanExecuteChanged();
            StartRecordCommand.NotifyCanExecuteChanged();
            StopRecordCommand.NotifyCanExecuteChanged();
        }

        public void Dispose()
        {
            _scriptingService.OutputReceived  -= OnOutputReceived;
            _scriptingService.ErrorReceived   -= OnErrorReceived;
            _scriptingService.ScriptCompleted -= OnScriptCompleted;
        }
    }
}
