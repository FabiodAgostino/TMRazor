using System;
using System.Linq;
using System.Text;
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
        // FIX BUG-P2-01: iniettato ISkillsService per implementare GetSkillValue e UseSkill
        private readonly ISkillsService _skills;
        private readonly ScriptCancellationController _cancel;

        public PlayerApi(
            IWorldService world,
            IPacketService packet,
            ITargetingService targeting,
            ISkillsService skills,
            ScriptCancellationController cancel)
        {
            _world = world;
            _packet = packet;
            _targeting = targeting;
            _skills = skills;
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
        // Attributi base e avanzati
        // ------------------------------------------------------------------
        public virtual int Str => P?.Str ?? 0;
        public virtual int Dex => P?.Dex ?? 0;
        public virtual int Int => P?.Int ?? 0;

        public virtual int FireResist   => P?.FireResist   ?? 0;
        public virtual int ColdResist   => P?.ColdResist   ?? 0;
        public virtual int PoisonResist => P?.PoisonResist ?? 0;
        public virtual int EnergyResist => P?.EnergyResist ?? 0;

        public virtual int MinDamage => P?.MinDamage ?? 0;
        public virtual int MaxDamage => P?.MaxDamage ?? 0;
        public virtual int Luck      => P?.Luck      ?? 0;
        public virtual int Tithe     => P?.Tithe     ?? 0;
        public virtual int Followers    => P?.Followers    ?? 0;
        public virtual int FollowersMax => P?.FollowersMax ?? 0;
        
        public virtual int Weight    => P?.Weight    ?? 0;
        public virtual int MaxWeight => P?.MaxWeight ?? 0;
        public virtual int Gold      => P?.Gold      ?? 0;
        public virtual int Armor     => P?.Armor     ?? 0;

        // Fame e Karma (TODO: Da estrarre tramite OPL o messaggi server)
        public virtual int Karma => 0;
        public virtual int Fame  => 0;

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
            byte[] msgBytes = System.Text.Encoding.Unicode.GetBytes(message);
            byte[] packet = new byte[12 + msgBytes.Length + 2];
            packet[0] = 0xAE; // Unicode message
            ushort len = (ushort)packet.Length;
            packet[1] = (byte)(len >> 8);
            packet[2] = (byte)(len & 0xff);
            // Serial (Player)
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(3), Serial);
            packet[7] = (byte)(P.Graphic >> 8); packet[8] = (byte)(P.Graphic & 0xff);
            packet[9] = 0x00; // Type
            packet[10] = (byte)(hue >> 8); packet[11] = (byte)(hue & 0xff);
            // Font, Lang non necessari per 0xAE locale?
            System.Array.Copy(msgBytes, 0, packet, 12, msgBytes.Length);
            // Notifica al client (non al server)
            _packet.SendToClient(packet);
        }

        public virtual void ChatSay(string message, int hue = 0)
        {
            _cancel.ThrowIfCancelled();
            // Pacchetto 0xAD (Unicode Speech)
            byte[] msgBytes = System.Text.Encoding.BigEndianUnicode.GetBytes(message);
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

        // FIX BUG-P2-01: delega a ISkillsService (stessa logica di SkillsApi)
        public virtual double GetSkillValue(string skillName)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
            return skill?.Value ?? 0;
        }

        public virtual void UseSkill(string skillName)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
            if (skill == null) return;
            string cmd = $"{skill.ID + 1} 0";
            byte[] cmdBytes = Encoding.ASCII.GetBytes(cmd);
            byte[] packet = new byte[3 + 1 + cmdBytes.Length + 1];
            packet[0] = 0x12;
            ushort len = (ushort)packet.Length;
            packet[1] = (byte)(len >> 8);
            packet[2] = (byte)(len & 0xff);
            packet[3] = 0x24; // UseSkill
            Array.Copy(cmdBytes, 0, packet, 4, cmdBytes.Length);
            packet[packet.Length - 1] = 0x00;
            _packet.SendToServer(packet);
        }

        public virtual void Cast(string spellName)
        {
            _cancel.ThrowIfCancelled();
            // Delega a SpellsApi (via Cast int tramite il dictionary di SpellsApi)
            // Costruiamo il pacchetto direttamente per evitare dipendenza circolare
            if (!SpellsApi.TryGetSpellId(spellName, out int spellId)) return;
            string cmd = spellId.ToString();
            byte[] cmdBytes = Encoding.ASCII.GetBytes(cmd);
            byte[] packet = new byte[3 + 1 + cmdBytes.Length + 1];
            packet[0] = 0x12;
            ushort len = (ushort)packet.Length;
            packet[1] = (byte)(len >> 8);
            packet[2] = (byte)(len & 0xff);
            packet[3] = 0x56; // CastSpell
            Array.Copy(cmdBytes, 0, packet, 4, cmdBytes.Length);
            packet[packet.Length - 1] = 0x00;
            _packet.SendToServer(packet);
        }

        public virtual void UseItem(uint serial)
        {
            _cancel.ThrowIfCancelled();
            byte[] packet = new byte[5];
            packet[0] = 0x06; // Double Click
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(1), serial);
            _packet.SendToServer(packet);
        }

        public virtual void TargetSelf() => _targeting.TargetSelf();
        public virtual void TargetLast() => _targeting.SendTarget(_targeting.LastTarget);

        // ------------------------------------------------------------------
        // Mosse Speciali e Abilità
        // ------------------------------------------------------------------
        
        public virtual void WeaponPrimary()
        {
            _cancel.ThrowIfCancelled();
            // Client macro per Secondary Weapon / Primary Weapon
            // Usa 0xD7 per mandare command o pacchetto specifico (es. 0xBF sub 0x24)
            byte[] packet = new byte[7];
            packet[0] = 0xBF;
            packet[1] = 0x00; packet[2] = 0x07;
            packet[3] = 0x00; packet[4] = 0x25; // Toggle Special Move
            packet[5] = 0x00; packet[6] = 0x01; // Primary
            _packet.SendToServer(packet);
        }

        public virtual void WeaponSecondary()
        {
            _cancel.ThrowIfCancelled();
            byte[] packet = new byte[7];
            packet[0] = 0xBF;
            packet[1] = 0x00; packet[2] = 0x07;
            packet[3] = 0x00; packet[4] = 0x25; // Toggle Special Move
            packet[5] = 0x00; packet[6] = 0x02; // Secondary
            _packet.SendToServer(packet);
        }

        public virtual void Stun()
        {
            _cancel.ThrowIfCancelled();
            byte[] packet = new byte[7];
            packet[0] = 0xBF;
            packet[1] = 0x00; packet[2] = 0x07;
            packet[3] = 0x00; packet[4] = 0x25;
            packet[5] = 0x00; packet[6] = 0x03; // Stun (id arbitrario se mapta su special move stun)
            _packet.SendToServer(packet);
        }

        public virtual void Disarm()
        {
            _cancel.ThrowIfCancelled();
            byte[] packet = new byte[7];
            packet[0] = 0xBF;
            packet[1] = 0x00; packet[2] = 0x07;
            packet[3] = 0x00; packet[4] = 0x25;
            packet[5] = 0x00; packet[6] = 0x04; // Disarm
            _packet.SendToServer(packet);
        }
    }
}
