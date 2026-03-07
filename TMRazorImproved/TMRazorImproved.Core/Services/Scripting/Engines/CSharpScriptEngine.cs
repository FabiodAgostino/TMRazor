using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using TMRazorImproved.Core.Services.Scripting.Api;
using TMRazorImproved.Shared.Enums;

namespace TMRazorImproved.Core.Services.Scripting.Engines
{
    /// <summary>
    /// Motore di scripting C# basato su Roslyn (Microsoft.CodeAnalysis.CSharp.Scripting).
    ///
    /// ===========================================================================
    /// CANCELLAZIONE COOPERATIVA per script C# (Roslyn)
    /// ===========================================================================
    ///
    /// A differenza di IronPython (sys.settrace) e UOSteamInterpreter (ThrowIfCancelled
    /// per ogni statement), Roslyn non offre un meccanismo di interruzione involontaria
    /// di uno script già avviato: il CancellationToken passato a RunAsync serve solo per
    /// annullare la compilazione/avvio, NON per interrompere il codice utente in esecuzione.
    ///
    /// SOLUZIONE — Cancellazione cooperativa a due livelli:
    ///
    /// LIVELLO 1 — ScriptToken (cooperativo, zero overhead se non usato):
    ///   Il <see cref="ScriptGlobals.ScriptToken"/> è il token di cancellazione dello script.
    ///   Script ben scritti chiamano <c>ScriptToken.ThrowIfCancellationRequested()</c> nei
    ///   propri loop. Questo è il meccanismo principale e più efficiente.
    ///
    /// LIVELLO 2 — Thread.Interrupt() (fallback per chiamate bloccanti):
    ///   <see cref="Execute"/> salva il thread corrente. Lo ScriptingService chiama
    ///   Thread.Interrupt() tramite _scriptThread per sbloccare attese native
    ///   (Task.Wait, Thread.Sleep, WaitHandle, ecc.).
    ///
    /// LIVELLO 3 — API interne (ThrowIfCancelled in ogni loop):
    ///   Tutti i metodi API (Misc.Pause, Items.FindAll, ecc.) controllano il
    ///   <see cref="ScriptCancellationController"/> ad ogni iterazione.
    ///
    /// ===========================================================================
    /// RIFERIMENTI ASSEMBLY
    /// ===========================================================================
    /// Roslyn carica i tipi dei globali tramite reflection. Le assembly dei tipi
    /// esposti in <see cref="ScriptGlobals"/> (Core + Shared) devono essere incluse
    /// in <see cref="ScriptOptions.WithReferences"/>. Sono aggiunte automaticamente
    /// nel costruttore tramite <c>typeof(T).Assembly</c>.
    ///
    /// ===========================================================================
    /// </summary>
    public sealed class CSharpScriptEngine
    {
        private readonly ScriptOptions _options;
        private readonly Action<string> _output;
        private readonly Action<string> _error;

        public CSharpScriptEngine(Action<string> output, Action<string> error)
        {
            _output = output;
            _error = error;

            _options = ScriptOptions.Default
                .WithReferences(
                    typeof(ScriptGlobals).Assembly,           // TMRazorImproved.Core
                    typeof(TMRazorImproved.Shared.Enums.ScriptLanguage).Assembly,  // TMRazorImproved.Shared
                    typeof(object).Assembly,                   // System.Runtime
                    typeof(System.Linq.Enumerable).Assembly,  // System.Linq
                    typeof(System.Collections.Generic.List<>).Assembly,
                    typeof(System.Threading.Tasks.Task).Assembly)
                .WithImports(
                    "System",
                    "System.Collections.Generic",
                    "System.Linq",
                    "System.Threading",
                    "System.Threading.Tasks",
                    "TMRazorImproved.Core.Services.Scripting.Api",
                    "TMRazorImproved.Shared.Models");
        }

        /// <summary>
        /// Esegue uno script C# in modo sincrono nel thread chiamante.
        /// Il chiamante è responsabile di impostare <see cref="ScriptGlobals.ScriptToken"/>
        /// prima di chiamare questo metodo.
        /// </summary>
        /// <param name="code">Codice C# sorgente dello script.</param>
        /// <param name="globals">Oggetto globale iniettato nello script.</param>
        /// <exception cref="OperationCanceledException">Propagata se lo script viene annullato.</exception>
        public void Execute(string code, ScriptGlobals globals)
        {
            var token = globals.ScriptToken;

            try
            {
                // Roslyn compila e avvia lo script. Il token viene usato per cancellare
                // la compilazione se il token è già cancellato prima dell'avvio.
                // Durante l'esecuzione, la cancellazione avviene cooperativamente tramite
                // ScriptToken.ThrowIfCancellationRequested() nello script utente.
                CSharpScript
                    .RunAsync(code, _options, globals, typeof(ScriptGlobals), token)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (AggregateException agg) when (agg.InnerException != null)
            {
                // Roslyn wrappa le eccezioni in AggregateException
                throw agg.InnerException;
            }
            catch (CompilationErrorException compEx)
            {
                // Errori di compilazione: formattali e propagali come stringa leggibile
                var msg = string.Join(Environment.NewLine, compEx.Diagnostics);
                _error($"[C# Script] Errore di compilazione:{Environment.NewLine}{msg}");
                throw new InvalidOperationException($"Compilazione fallita: {msg}", compEx);
            }
        }
    }
}
