using System;
using System.Buffers.Binary;
using TMRazorImproved.Core.Utilities;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    /// <summary>
    /// API esposta agli script Python come variabile <c>SpecialMoves</c>.
    /// Gestisce le abilità speciali di combattimento (Weapon Abilities).
    /// </summary>
    public class SpecialMovesApi
    {
        private readonly IWorldService _world;
        private readonly IPacketService _packet;
        private readonly ScriptCancellationController _cancel;

        public SpecialMovesApi(IWorldService world, IPacketService packet, ScriptCancellationController cancel)
        {
            _world  = world;
            _packet = packet;
            _cancel = cancel;
        }

        /// <summary>Attiva o disattiva l'abilità primaria dell'arma corrente.</summary>
        public virtual void SetPrimaryAbility()
        {
            _cancel.ThrowIfCancelled();
            SendToggleAbility(0x01);
        }

        /// <summary>Attiva o disattiva l'abilità secondaria dell'arma corrente.</summary>
        public virtual void SetSecondaryAbility()
        {
            _cancel.ThrowIfCancelled();
            SendToggleAbility(0x02);
        }

        /// <summary>Disattiva entrambe le abilità speciali.</summary>
        public virtual void ClearCurrentAbility()
        {
            _cancel.ThrowIfCancelled();
            SendToggleAbility(0x00);
        }

        public virtual bool HasPrimary   => _world.Player?.PrimaryAbilityActive   ?? false;
        public virtual bool HasSecondary => _world.Player?.SecondaryAbilityActive ?? false;
        public virtual int  PrimaryId    => _world.Player?.PrimaryAbilityId       ?? 0;
        public virtual int  SecondaryId  => _world.Player?.SecondaryAbilityId     ?? 0;

        private void SendToggleAbility(byte abilityIndex)
        {
            // Pacchetto 0xD7 (Extended Command)
            // Formato richiesto per lo shard The Miracle:
            // [0] 0xD7, [1-2] len (9), [3-6] serial, [7-8] sub (0x00XX where XX=abilityIndex)
            byte[] pkt = new byte[9];
            pkt[0] = 0xD7;
            pkt[1] = 0x00; pkt[2] = 0x09;
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), _world.Player?.Serial ?? 0);
            pkt[7] = 0x00; pkt[8] = abilityIndex;
            _packet.SendToServer(pkt);
        }
    }
}
