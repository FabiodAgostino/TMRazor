using System.Threading;

namespace TMRazorImproved.Core.Services.Scripting
{
    /// <summary>
    /// Oggetto esposto allo script Python come <c>__cancel__</c>.
    /// La proprietà IsCancelled viene letta dal trace handler Python (sys.settrace)
    /// ad ogni statement per interrompere cooperativamente l'esecuzione.
    ///
    /// Uso dalla parte Python (iniettato come preamble):
    /// <code>
    ///   def _t_(f,e,a):
    ///       if __cancel__.IsCancelled: raise SystemExit('stopped')
    ///       return _t_
    ///   import sys; sys.settrace(_t_)
    /// </code>
    ///
    /// Lettura di <c>IsCancelled</c> è thread-safe (volatile read).
    /// </summary>
    public sealed class ScriptCancellationController
    {
        private volatile bool _cancelled;

        public ScriptCancellationController(CancellationToken token)
        {
            // Collega la cancellazione .NET a questo controller
            token.Register(() => _cancelled = true);
        }

        /// <summary>True quando lo script deve fermarsi. Letto dal trace Python.</summary>
        public bool IsCancelled => _cancelled;

        /// <summary>Forza la cancellazione indipendentemente dal token.</summary>
        public void Cancel() => _cancelled = true;

        /// <summary>
        /// Lancia OperationCanceledException se la cancellazione è stata richiesta.
        /// Usato dai metodi delle API C# (es. Pause, WaitFor) chiamati durante l'esecuzione dello script.
        /// </summary>
        public void ThrowIfCancelled()
        {
            if (_cancelled)
                throw new System.OperationCanceledException("Script stopped by user.");
        }
    }
}
