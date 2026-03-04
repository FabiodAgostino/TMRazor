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
        private readonly ILogger<SpellsApi>? _logger;

        public SpellsApi(IWorldService world, IPacketService packet, ScriptCancellationController cancel, ILogger<SpellsApi>? logger = null)
        {
            _world = world;
            _packet = packet;
            _cancel = cancel;
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
    }
}
