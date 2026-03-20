using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
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
        /// <param name="scriptDirectory">
        /// Directory base per la risoluzione dei percorsi relativi nelle direttive
        /// <c>//#import</c> e <c>//#assembly</c>. Se <c>null</c>, viene usata la
        /// directory di lavoro corrente.
        /// </param>
        /// <exception cref="OperationCanceledException">Propagata se lo script viene annullato.</exception>
        public void Execute(string code, ScriptGlobals globals, string scriptDirectory = null)
        {
            var token = globals.ScriptToken;

            // Pre-processing: gestisce le direttive custom //#import, //#assembly, //#forcedebug
            var processedCode = PreprocessDirectives(
                code,
                scriptDirectory ?? Directory.GetCurrentDirectory(),
                out var extraRefs,
                out bool forceDebug);

            var options = _options;
            if (extraRefs.Count > 0)
                options = options.AddReferences(extraRefs);
            if (forceDebug)
                options = options.WithOptimizationLevel(OptimizationLevel.Debug);

            try
            {
                // Roslyn compila e avvia lo script. Il token viene usato per cancellare
                // la compilazione se il token è già cancellato prima dell'avvio.
                // Durante l'esecuzione, la cancellazione avviene cooperativamente tramite
                // ScriptToken.ThrowIfCancellationRequested() nello script utente.
                CSharpScript
                    .RunAsync(processedCode, options, globals, typeof(ScriptGlobals), token)
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

        /// <summary>
        /// Pre-compila uno script C# senza eseguirlo (warm-up Roslyn JIT).
        /// Ritorna gli eventuali errori di compilazione come stringa, o null se OK.
        /// </summary>
        public string? Precompile(string code, string? scriptDirectory = null)
        {
            var processedCode = PreprocessDirectives(
                code,
                scriptDirectory ?? Directory.GetCurrentDirectory(),
                out var extraRefs,
                out bool forceDebug);

            var options = _options;
            if (extraRefs.Count > 0)
                options = options.AddReferences(extraRefs);
            if (forceDebug)
                options = options.WithOptimizationLevel(OptimizationLevel.Debug);

            try
            {
                var script = CSharpScript.Create(processedCode, options, typeof(ScriptGlobals));
                var diagnostics = script.Compile();
                var errors = System.Linq.Enumerable.Where(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
                var list = System.Linq.Enumerable.ToList(errors);
                if (list.Count > 0)
                    return string.Join(Environment.NewLine, list);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // ------------------------------------------------------------------
        // Pre-processing direttive custom
        // ------------------------------------------------------------------

        /// <summary>
        /// Analizza il codice sorgente cercando le direttive custom:
        /// <list type="bullet">
        ///   <item><c>//#import &lt;file&gt;</c> o <c>//#import "file"</c> — include un altro file .cs</item>
        ///   <item><c>//#assembly &lt;dll&gt;</c> o <c>//#assembly "dll"</c> — referenzia un assembly DLL</item>
        ///   <item><c>//#forcedebug</c> — forza la compilazione in modalità Debug</item>
        /// </list>
        /// Le direttive devono comparire prima della keyword <c>namespace</c> (come nel legacy).
        /// </summary>
        private string PreprocessDirectives(
            string code,
            string baseDirectory,
            out List<MetadataReference> extraRefs,
            out bool forceDebug)
        {
            extraRefs = new List<MetadataReference>();
            forceDebug = false;

            // Raccogliamo tutti i file da includere (risoluzione ricorsiva //#import)
            var includedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var importedBlocks = new List<string>();

            // Fase 1: parse del codice principale per trovare direttive
            ParseDirectives(
                code,
                baseDirectory,
                includedFiles,
                importedBlocks,
                extraRefs,
                ref forceDebug);

            if (importedBlocks.Count == 0 && extraRefs.Count == 0 && !forceDebug)
                return code; // nessuna direttiva: ritorna il codice invariato

            // Fase 2: costruisci il codice finale — prima gli import, poi il codice principale
            var sb = new StringBuilder();
            foreach (var block in importedBlocks)
            {
                sb.AppendLine(block);
                sb.AppendLine(); // separatore
            }
            sb.Append(code);
            return sb.ToString();
        }

        /// <summary>
        /// Legge le direttive in un blocco di codice sorgente (stringa) e le elabora
        /// ricorsivamente per i file importati.
        /// </summary>
        private void ParseDirectives(
            string sourceCode,
            string currentBaseDir,
            HashSet<string> includedFiles,
            List<string> importedBlocks,
            List<MetadataReference> extraRefs,
            ref bool forceDebug)
        {
            using var reader = new StringReader(sourceCode);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var trimmed = line.Trim();

                // Stop prima del namespace (come nel legacy)
                if (trimmed.StartsWith("namespace ", StringComparison.OrdinalIgnoreCase))
                    break;

                if (trimmed.StartsWith("//#forcedebug", StringComparison.OrdinalIgnoreCase))
                {
                    forceDebug = true;
                    continue;
                }

                if (trimmed.StartsWith("//#import", StringComparison.OrdinalIgnoreCase))
                {
                    var arg = trimmed.Substring("//#import".Length).Trim();
                    if (TryResolvePath(arg, currentBaseDir, out var resolved) &&
                        includedFiles.Add(resolved))
                    {
                        var importedCode = File.ReadAllText(resolved);
                        var importDir = Path.GetDirectoryName(resolved);
                        // Prima elabora ricorsivamente il file importato
                        ParseDirectives(importedCode, importDir, includedFiles, importedBlocks, extraRefs, ref forceDebug);
                        importedBlocks.Add(importedCode);
                    }
                    continue;
                }

                if (trimmed.StartsWith("//#assembly", StringComparison.OrdinalIgnoreCase))
                {
                    var arg = trimmed.Substring("//#assembly".Length).Trim();
                    if (TryResolvePath(arg, currentBaseDir, out var resolved))
                    {
                        try
                        {
                            extraRefs.Add(MetadataReference.CreateFromFile(resolved));
                        }
                        catch (Exception ex)
                        {
                            _error($"[C# Script] //#assembly: impossibile caricare '{resolved}': {ex.Message}");
                        }
                    }
                    continue;
                }
            }
        }

        /// <summary>
        /// Risolve un argomento direttiva in percorso file assoluto.
        /// Supporta:
        /// <list type="bullet">
        ///   <item><c>&lt;file&gt;</c> — percorso relativo rispetto a <paramref name="baseDir"/></item>
        ///   <item><c>"file"</c> — percorso assoluto o relativo rispetto a <paramref name="baseDir"/></item>
        /// </list>
        /// </summary>
        private static bool TryResolvePath(string arg, string baseDir, out string fullPath)
        {
            fullPath = null;
            if (string.IsNullOrWhiteSpace(arg))
                return false;

            string raw;
            if (arg.StartsWith("<") && arg.EndsWith(">"))
                raw = arg.Substring(1, arg.Length - 2);
            else if (arg.StartsWith("\"") && arg.EndsWith("\""))
                raw = arg.Substring(1, arg.Length - 2);
            else
                raw = arg; // bare path

            fullPath = Path.IsPathRooted(raw)
                ? Path.GetFullPath(raw)
                : Path.GetFullPath(Path.Combine(baseDir, raw));

            return File.Exists(fullPath);
        }
    }
}
