using System;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Core.Utilities;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class SpellsApi
    {
        private readonly IWorldService _world;
        private readonly IPacketService _packet;
        private readonly ScriptCancellationController _cancel;
        private readonly ITargetingService _targeting;
        private readonly ILogger<SpellsApi>? _logger;

        public SpellsApi(IWorldService world, IPacketService packet, ScriptCancellationController cancel, ITargetingService targeting, ILogger<SpellsApi>? logger = null)
        {
            _world = world;
            _packet = packet;
            _cancel = cancel;
            _targeting = targeting;
            _logger = logger;
        }

        /// <summary>Ritorna vero se il giocatore sta castando una spell (rilevato dai pacchetti C2S 0x12).</summary>
        public virtual bool IsCasting => _world.IsCasting;

        /// <summary>Attende che il cast corrente finisca (fino a timeoutMs).</summary>
        public virtual void WaitCast(int timeoutMs = 10000)
        {
            var deadline = Environment.TickCount64 + timeoutMs;
            while (Environment.TickCount64 < deadline)
            {
                _cancel.ThrowIfCancelled();
                if (!IsCasting) return;
                System.Threading.Thread.Sleep(50);
            }
        }

        public virtual void Cast(int spellId)
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogDebug("Cast: spellId={SpellId}", spellId);
            _packet.SendToServer(PacketBuilder.CastSpell(spellId));
        }

        public virtual void Cast(string name)
        {
            _cancel.ThrowIfCancelled();
            if (SpellDefinitions.TryGetSpellId(name, out int spellId))
            {
                _logger?.LogDebug("Cast: '{SpellName}' → id={SpellId}", name, spellId);
                Cast(spellId);
            }
            else
            {
                _logger?.LogWarning("Cast: spell '{SpellName}' not found", name);
            }
        }

        public virtual void CastMagery(string name) => Cast(name);
        public virtual void CastNecro(string name) => Cast(name);
        public virtual void CastChivalry(string name) => Cast(name);
        public virtual void CastBushido(string name) => Cast(name);
        public virtual void CastNinjitsu(string name) => Cast(name);
        public virtual void CastSpellweaving(string name) => Cast(name);
        public virtual void CastMysticism(string name) => Cast(name);
        public virtual void CastCleric(string name) => Cast(name);
        public virtual void CastDruid(string name) => Cast(name);

        public virtual void CastLastSpell()
        {
            _cancel.ThrowIfCancelled();
            if (!string.IsNullOrEmpty(_lastSpellName))
            {
                Cast(_lastSpellName);
            }
            else
            {
                _logger?.LogWarning("CastLastSpell: No last spell recorded.");
            }
        }

        public virtual void CastLastSpellLastTarget()
        {
            _cancel.ThrowIfCancelled();
            if (!string.IsNullOrEmpty(_lastSpellName))
            {
                Cast(_lastSpellName);
                
                var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
                Action<uint> handler = _ => tcs.TrySetResult(true);
                _targeting.TargetCursorRequested += handler;
                try
                {
                    var deadline = Environment.TickCount64 + 10000;
                    while (Environment.TickCount64 < deadline)
                    {
                        _cancel.ThrowIfCancelled();
                        if (_targeting.HasTargetCursor || tcs.Task.IsCompleted)
                        {
                            _targeting.SendTarget(_targeting.LastTarget);
                            return;
                        }
                        System.Threading.Thread.Sleep(50);
                    }
                }
                finally
                {
                    _targeting.TargetCursorRequested -= handler;
                }
            }
            else
            {
                _logger?.LogWarning("CastLastSpellLastTarget: No last spell recorded.");
            }
        }

        public virtual void ExportSpellsToJson()
        {
            _cancel.ThrowIfCancelled();
            try
            {
                var basePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                if (!System.IO.Directory.Exists(basePath))
                {
                    System.IO.Directory.CreateDirectory(basePath);
                }
                
                var customSpellsPath = System.IO.Path.Combine(basePath, "CustomSpells.json");
                var spellsDict = new System.Collections.Generic.Dictionary<string, int>();
                
                foreach (var spell in SpellDefinitions.All)
                {
                    spellsDict[spell.Name] = spell.ID;
                }
                
                string jsonContent = System.Text.Json.JsonSerializer.Serialize(spellsDict, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(customSpellsPath, jsonContent);
                _logger?.LogInformation("Exported {Count} spells to {Path}", spellsDict.Count, customSpellsPath);
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error exporting spells to JSON: {Message}", ex.Message);
            }
        }

        /// <summary>Ritorna l'ID di un incantesimo dal suo nome. 0 se non trovato.</summary>
        public virtual int GetSpellId(string name)
        {
            _cancel.ThrowIfCancelled();
            return SpellDefinitions.TryGetSpellId(name, out int id) ? id : 0;
        }

        /// <summary>API statica per uso interno (es. PlayerApi.Cast) senza dipendenza circolare.</summary>
        internal static bool TryGetSpellId(string name, out int id)
        {
            return SpellDefinitions.TryGetSpellId(name, out id);
        }

        // ------------------------------------------------------------------
        // Cerchie aggiuntive
        // ------------------------------------------------------------------

        /// <summary>Lancia un incantesimo di Imbuing.</summary>
        public virtual void CastImbuing(string name) => Cast(name);

        /// <summary>Lancia un Bard Mastery spell.</summary>
        public virtual void CastMastery(string name) => Cast(name);

        // ------------------------------------------------------------------
        // Utility spell
        // ------------------------------------------------------------------

        /// <summary>
        /// Attende che la mana del player raggiunga almeno il valore specificato.
        /// Controlla ogni 100ms. Ritorna true se la mana è sufficiente entro il timeout.
        /// </summary>
        public virtual bool WaitForMana(int mana, int timeoutMs = 10000)
        {
            _cancel.ThrowIfCancelled();
            var deadline = Environment.TickCount64 + timeoutMs;
            while (Environment.TickCount64 < deadline)
            {
                _cancel.ThrowIfCancelled();
                if ((_world.Player?.Mana ?? 0) >= mana) return true;
                System.Threading.Thread.Sleep(100);
            }
            return false;
        }

        /// <summary>
        /// True se il player ha abbastanza mana per l'incantesimo specificato.
        /// Il costo minimo è stimato in base al circolo (id / 8 + 1) * 4.
        /// </summary>
        public virtual bool HasManaToCast(string spellName)
        {
            _cancel.ThrowIfCancelled();
            if (!SpellDefinitions.TryGetSpellId(spellName, out int id)) return false;
            // Stima: cerchio = (id-1)/8+1, mana = cerchio * 4  (magery standard)
            int circle = (id > 0) ? ((id - 1) / 8 + 1) : 1;
            int estimatedCost = circle * 4;
            return (_world.Player?.Mana ?? 0) >= estimatedCost;
        }

        // ------------------------------------------------------------------
        // Controllo del cast
        // ------------------------------------------------------------------

        private volatile string _lastSpellName = string.Empty;

        /// <summary>
        /// Interrompe il cast corrente inviando un movimento (tecnica standard UO per cancellare il cast).
        /// </summary>
        public virtual void Interrupt()
        {
            _cancel.ThrowIfCancelled();
            // Il modo standard per interrompere un cast in UO è muoversi o eseguire un'azione
            // Inviamo 0x02 (MoveRequest) con la direzione corrente per "scuotere" il cast
            var player = _world.Player;
            if (player == null) return;
            byte dir = player.Direction;
            byte[] pkt = { 0x02, dir, 0x01, 0x00, 0x00, 0x00, 0x00 };
            _packet.SendToServer(pkt);
        }

        /// <summary>Nome dell'ultimo incantesimo castato (vuoto se nessuno).</summary>
        public virtual string GetLastSpell() => _lastSpellName;

        /// <summary>Casta e registra il nome dell'ultimo incantesimo.</summary>
        public virtual void CastAndRecord(string name)
        {
            _cancel.ThrowIfCancelled();
            if (SpellDefinitions.TryGetSpellId(name, out int spellId))
            {
                _lastSpellName = name;
                Cast(spellId);
            }
        }

        /// <summary>
        /// Attende che il flag IsCasting diventi false (cast completato o interrotto),
        /// poi ritorna true. False se scade il timeout.
        /// </summary>
        public virtual bool WaitCastComplete(int timeoutMs = 10000)
        {
            var deadline = Environment.TickCount64 + timeoutMs;
            while (Environment.TickCount64 < deadline)
            {
                _cancel.ThrowIfCancelled();
                if (!IsCasting) return true;
                System.Threading.Thread.Sleep(50);
            }
            return !IsCasting;
        }
    }
}
