using System;
using System.Collections.Generic;

namespace TMRazorImproved.Core.Services.Scripting
{
    /// <summary>
    /// Controlla l'esecuzione in modalità debug per script Python.
    /// Esposto allo script come variabile __debug__ nel preamble sys.settrace.
    /// Il trace handler Python chiama ShouldPause(lineno) ad ogni statement 'line';
    /// se restituisce true, chiama PauseHere() e loop su IsPaused.
    /// </summary>
    public sealed class ScriptDebugController
    {
        private readonly HashSet<int> _breakpoints = new();
        private readonly object _lock = new();
        private volatile bool _paused;
        private volatile bool _stepMode;
        private int _currentLine;

        // ----------------------------------------------------------------
        // Stato osservabile dalla UI
        // ----------------------------------------------------------------

        /// <summary>True mentre lo script è in pausa su un breakpoint o step.</summary>
        public bool IsPaused => _paused;

        /// <summary>Numero di riga corrente durante l'esecuzione (1-based).</summary>
        public int CurrentLine => _currentLine;

        /// <summary>Scattato quando l'esecuzione si ferma (breakpoint o step).</summary>
        public event Action<int>? DebugPaused;

        // ----------------------------------------------------------------
        // API chiamate dal trace handler Python
        // ----------------------------------------------------------------

        /// <summary>
        /// Chiamato dal trace handler Python ad ogni evento 'line'.
        /// Aggiorna CurrentLine; ritorna true se l'esecuzione deve pausarsi.
        /// </summary>
        public bool ShouldPause(int lineNumber)
        {
            _currentLine = lineNumber;
            if (_stepMode)
            {
                _stepMode = false;
                return true;
            }
            lock (_lock) { return _breakpoints.Contains(lineNumber); }
        }

        /// <summary>
        /// Chiamato dal trace handler Python dopo ShouldPause == true.
        /// Setta IsPaused e notifica la UI.
        /// </summary>
        public void PauseHere()
        {
            _paused = true;
            DebugPaused?.Invoke(_currentLine);
        }

        // ----------------------------------------------------------------
        // Comandi dalla UI
        // ----------------------------------------------------------------

        /// <summary>Riprende l'esecuzione fino al prossimo breakpoint.</summary>
        public void Continue() => _paused = false;

        /// <summary>Esegue una singola riga e si ferma di nuovo.</summary>
        public void StepInto()
        {
            _stepMode = true;
            _paused = false;
        }

        // ----------------------------------------------------------------
        // Gestione breakpoint
        // ----------------------------------------------------------------

        public void SetBreakpoint(int line) { lock (_lock) { _breakpoints.Add(line); } }
        public void ClearBreakpoint(int line) { lock (_lock) { _breakpoints.Remove(line); } }
        public void ClearAll() { lock (_lock) { _breakpoints.Clear(); } }
        public bool HasBreakpoint(int line) { lock (_lock) { return _breakpoints.Contains(line); } }

        public IEnumerable<int> Breakpoints
        {
            get { lock (_lock) { return new List<int>(_breakpoints); } }
        }

        /// <summary>Reset completo: rimuove breakpoint e azzera stato pausa/step.</summary>
        public void Reset()
        {
            _paused = false;
            _stepMode = false;
            _currentLine = 0;
            lock (_lock) { _breakpoints.Clear(); }
        }
    }
}
