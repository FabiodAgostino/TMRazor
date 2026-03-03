using System;
using System.Threading;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    /// <summary>
    /// API esposta agli script Python come variabile <c>Misc</c>.
    /// Contiene funzioni di utilità: pause, output, timing.
    ///
    /// Pause() usa uno sleep chunked (10ms slice) per permettere la cancellazione
    /// anche durante attese lunghe da script (il sys.settrace non si attiva
    /// mentre l'esecuzione è bloccata in codice .NET nativo).
    /// </summary>
    public class MiscApi
    {
        private readonly IWorldService _world;
        private readonly ScriptCancellationController _cancel;
        private readonly Action<string>? _outputCallback;

        public MiscApi(IWorldService world, ScriptCancellationController cancel, Action<string>? outputCallback = null)
        {
            _world = world;
            _cancel = cancel;
            _outputCallback = outputCallback;
        }

        /// <summary>
        /// Attende per il numero di millisecondi specificato rispettando la cancellazione.
        /// Equivalente a time.sleep() ma interrompibile da StopAsync().
        /// </summary>
        public virtual void Pause(int milliseconds)
        {
            if (milliseconds <= 0) return;

            var deadline = Environment.TickCount64 + milliseconds;
            while (Environment.TickCount64 < deadline)
            {
                _cancel.ThrowIfCancelled();
                var remaining = (int)(deadline - Environment.TickCount64);
                Thread.Sleep(Math.Min(10, Math.Max(0, remaining)));
            }
        }

        /// <summary>
        /// Attende finché la condizione non è vera, con timeout.
        /// </summary>
        /// <param name="condition">Funzione che ritorna true quando l'attesa deve terminare.</param>
        /// <param name="timeoutMs">Timeout massimo in millisecondi.</param>
        /// <param name="pollMs">Intervallo di polling in millisecondi (default 50ms).</param>
        /// <returns>True se la condizione è diventata vera, False se è scattato il timeout.</returns>
        public virtual bool WaitFor(Func<bool> condition, int timeoutMs = 5000, int pollMs = 50)
        {
            var deadline = Environment.TickCount64 + timeoutMs;
            while (Environment.TickCount64 < deadline)
            {
                _cancel.ThrowIfCancelled();
                if (condition()) return true;
                Thread.Sleep(Math.Max(1, pollMs));
            }
            return false;
        }

        public virtual bool WaitForTarget(int timeoutMs = 5000)
        {
            // TODO: Implementare flag HasTarget in WorldService o PacketService
            return WaitFor(() => false, timeoutMs); 
        }

        public virtual bool WaitGump(uint gumpId, int timeoutMs = 5000)
        {
            return WaitFor(() => _world.CurrentGump?.GumpId == gumpId, timeoutMs);
        }

        /// <summary>Scrive una riga sul pannello output della UI (non sulla chat UO).</summary>
        public virtual void Log(string message)
        {
            _outputCallback?.Invoke($"[Script] {message}");
        }

        /// <summary>Ritorna il timestamp Unix corrente in millisecondi.</summary>
        public virtual long Timestamp() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
