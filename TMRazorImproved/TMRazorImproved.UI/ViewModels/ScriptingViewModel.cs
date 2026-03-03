using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
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
        private string _statusText = "Pronto";

        public ObservableCollection<ScriptLogEntry> LogEntries { get; } = new();

        public IAsyncRelayCommand RunScriptCommand  { get; }
        public IAsyncRelayCommand StopScriptCommand { get; }
        public IRelayCommand      ClearLogCommand   { get; }
        public IRelayCommand      NewScriptCommand  { get; }
        public IRelayCommand      OpenScriptCommand { get; }
        public IRelayCommand      SaveScriptCommand { get; }

        public ScriptingViewModel(IScriptingService scriptingService)
        {
            _scriptingService = scriptingService;
            EnableThreadSafeCollection(LogEntries, _logLock);

            RunScriptCommand  = new AsyncRelayCommand(RunScriptAsync,  () => !IsRunning);
            StopScriptCommand = new AsyncRelayCommand(StopScriptAsync, () => IsRunning);
            ClearLogCommand   = new RelayCommand(ClearLog);
            NewScriptCommand  = new RelayCommand(NewScript,   () => !IsRunning);
            OpenScriptCommand = new RelayCommand(OpenScript,  () => !IsRunning);
            SaveScriptCommand = new RelayCommand(SaveScript);

            _scriptingService.OutputReceived  += OnOutputReceived;
            _scriptingService.ErrorReceived   += OnErrorReceived;
            _scriptingService.ScriptCompleted += OnScriptCompleted;
        }

        // ------------------------------------------------------------------
        // Commands
        // ------------------------------------------------------------------

        private async Task RunScriptAsync()
        {
            if (string.IsNullOrWhiteSpace(ScriptCode)) return;

            AddLog($"--- Avvio '{ScriptName}' ---", ScriptLogType.System);
            IsRunning = true;
            StatusText = $"Esecuzione: {ScriptName}";
            NotifyCommandsCanExecuteChanged();

            await _scriptingService.RunAsync(ScriptCode, ScriptName);
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
            ScriptCode = "# Nuovo script\n";
            ScriptName = "nuovo_script.py";
            ClearLog();
            StatusText = "Pronto";
        }

        private void OpenScript()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Script Python (*.py)|*.py|Tutti i file (*.*)|*.*",
                Title  = "Apri Script"
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                ScriptCode = File.ReadAllText(dialog.FileName);
                ScriptName = Path.GetFileName(dialog.FileName);
                AddLog($"Caricato: {dialog.FileName}", ScriptLogType.System);
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
                Filter   = "Script Python (*.py)|*.py|Tutti i file (*.*)|*.*",
                FileName = ScriptName,
                Title    = "Salva Script"
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

                StatusText = status;
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

        private void NotifyCommandsCanExecuteChanged()
        {
            RunScriptCommand.NotifyCanExecuteChanged();
            StopScriptCommand.NotifyCanExecuteChanged();
            NewScriptCommand.NotifyCanExecuteChanged();
            OpenScriptCommand.NotifyCanExecuteChanged();
        }

        public void Dispose()
        {
            _scriptingService.OutputReceived  -= OnOutputReceived;
            _scriptingService.ErrorReceived   -= OnErrorReceived;
            _scriptingService.ScriptCompleted -= OnScriptCompleted;
        }
    }
}
