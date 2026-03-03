using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    /// <summary>
    /// API esposta agli script Python come variabile <c>Player</c>.
    ///
    /// NOTA BINDER DLR: Le proprietà sono <c>virtual</c> per assicurare
    /// che IronPython possa accedervi tramite il DispatchSlot mechanism su .NET 10.
    /// Se una proprietà non è virtual e la classe è sealed, il binder può
    /// non riuscire a generare il CallSite corretto e restituire AttributeError.
    /// </summary>
    public class PlayerApi
    {
        private readonly IWorldService _world;
        private readonly IPacketService _packet;
        private readonly ITargetingService _targeting;
        private readonly ScriptCancellationController _cancel;

        public PlayerApi(IWorldService world, IPacketService packet, ITargetingService targeting, ScriptCancellationController cancel)
        {
            _world = world;
            _packet = packet;
            _targeting = targeting;
            _cancel = cancel;
        }

        private Mobile? P => _world.Player;

        // ------------------------------------------------------------------
        // Stats vitali
        // ------------------------------------------------------------------
        public virtual int Hits    => P?.Hits    ?? 0;
        public virtual int HitsMax => P?.HitsMax ?? 0;
        public virtual int Mana    => P?.Mana    ?? 0;
        public virtual int ManaMax => P?.ManaMax ?? 0;
        public virtual int Stam    => P?.Stam    ?? 0;
        public virtual int StamMax => P?.StamMax ?? 0;

        // ------------------------------------------------------------------
        // Attributi base
        // ------------------------------------------------------------------
        public virtual int Str => P?.Str ?? 0;
        public virtual int Dex => P?.Dex ?? 0;
        public virtual int Int => P?.Int ?? 0;

        // ------------------------------------------------------------------
        // Identificazione
        // ------------------------------------------------------------------
        public virtual uint   Serial => P?.Serial ?? 0;
        public virtual string Name   => P?.Name   ?? string.Empty;

        // ------------------------------------------------------------------
        // Stato
        // ------------------------------------------------------------------
        public virtual bool IsConnected  => P != null;
        public virtual bool IsPoisoned   => P?.IsPoisoned  ?? false;
        public virtual bool IsYellowHits => P?.IsYellowHits ?? false;

        // ------------------------------------------------------------------
        // Posizione
        // ------------------------------------------------------------------
        public virtual int X => P?.X ?? 0;
        public virtual int Y => P?.Y ?? 0;
        public virtual int Z => P?.Z ?? 0;

        // ------------------------------------------------------------------
        // Inventario
        // ------------------------------------------------------------------
        /// <summary>Item che rappresenta il backpack del giocatore.</summary>
        public virtual Item? Backpack
        {
            get
            {
                _cancel.ThrowIfCancelled();
                return P?.Backpack;
            }
        }

        // ------------------------------------------------------------------
        // Percentuali di utilità per script
        // ------------------------------------------------------------------
        public virtual double HitsPct => HitsMax > 0 ? (double)Hits / HitsMax * 100 : 0;
        public virtual double ManaPct  => ManaMax > 0 ? (double)Mana / ManaMax * 100 : 0;
        public virtual double StamPct  => StamMax > 0 ? (double)Stam / StamMax * 100 : 0;

        // ------------------------------------------------------------------
        // Azioni
        // ------------------------------------------------------------------

        public virtual void Chat(string message, int hue = 0)
        {
            _cancel.ThrowIfCancelled();
            System.Text.Encoding enc = System.Text.Encoding.BigEndianUnicode;
            byte[] msgBytes = enc.GetBytes(message);
            byte[] packet = new byte[12 + msgBytes.Length + 2];
            packet[0] = 0xAD;
            ushort len = (ushort)packet.Length;
            packet[1] = (byte)(len >> 8);
            packet[2] = (byte)(len & 0xff);
            packet[3] = 0x00; // Regular
            packet[4] = (byte)(hue >> 8);
            packet[5] = (byte)(hue & 0xff);
            packet[6] = 0x00; packet[7] = 0x03; // Font
            packet[8] = (byte)'e'; packet[9] = (byte)'n'; packet[10] = (byte)'u'; packet[11] = 0x00; // Lang
            System.Array.Copy(msgBytes, 0, packet, 12, msgBytes.Length);
            _packet.SendToServer(packet);
        }

        public virtual void Attack(uint serial)
        {
            _cancel.ThrowIfCancelled();
            byte[] packet = new byte[5];
            packet[0] = 0x05;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(1), serial);
            _packet.SendToServer(packet);
        }

        public virtual void HeadMsg(string message, int hue = 945)
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return;
            // Invia al client un messaggio locale sopra la testa del player
            // Pacchetto 0x1C (ASCII) o 0xAE (Unicode)
        }

        public virtual void UseSkill(string skillName)
        {
            _cancel.ThrowIfCancelled();
            // TODO: Mappare skillName a ID ed inviare pacchetto 0x12
        }

        public virtual void Cast(string spellName)
        {
            _cancel.ThrowIfCancelled();
            // TODO: Inviare comando cast
        }

        public virtual void TargetSelf() => _targeting.TargetSelf();
        public virtual void TargetLast() => _targeting.SendTarget(_targeting.LastTarget);
    }
}
