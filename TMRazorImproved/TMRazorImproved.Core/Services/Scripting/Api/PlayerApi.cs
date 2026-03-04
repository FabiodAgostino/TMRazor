using System;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Core.Utilities;
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
        private readonly ISkillsService _skills;
        private readonly ScriptCancellationController _cancel;
        private readonly ILogger<PlayerApi>? _logger;

        public PlayerApi(
            IWorldService world,
            IPacketService packet,
            ITargetingService targeting,
            ISkillsService skills,
            ScriptCancellationController cancel,
            ILogger<PlayerApi>? logger = null)
        {
            _world = world;
            _packet = packet;
            _targeting = targeting;
            _skills = skills;
            _cancel = cancel;
            _logger = logger;
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
        // Stato avanzato
        // ------------------------------------------------------------------
        public virtual bool IsHidden  => P?.IsHidden  ?? false;
        public virtual bool WarMode   => P?.WarMode   ?? false;
        public virtual int  Direction => P?.Direction ?? 0;
        public virtual int  MapId     => P?.MapId     ?? 0;
        public virtual int  Notoriety => P?.Notoriety ?? 0;

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
            _logger?.LogDebug("Attack: target=0x{Serial:X}", serial);
            _packet.SendToServer(PacketBuilder.Attack(serial));
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
            if (!SpellsApi.TryGetSpellId(spellName, out int spellId))
            {
                _logger?.LogWarning("Cast: spell '{SpellName}' not found in dictionary", spellName);
                return;
            }
            _logger?.LogDebug("Cast: '{SpellName}' → id={SpellId}", spellName, spellId);
            _packet.SendToServer(PacketBuilder.CastSpell(spellId));
        }

        public virtual void UseItem(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogDebug("UseItem: serial=0x{Serial:X}", serial);
            _packet.SendToServer(PacketBuilder.DoubleClick(serial));
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

        // ------------------------------------------------------------------
        // Sprint Fix-4: metodi di utilità mancanti
        // ------------------------------------------------------------------

        /// <summary>
        /// Usa (double-click) il primo item con il graphic specificato nel backpack.
        /// Se non trovato nel backpack, cerca nell'intero mondo.
        /// </summary>
        public virtual void UseType(int graphic)
        {
            _cancel.ThrowIfCancelled();
            var bp = P?.Backpack;
            Item? found = null;
            if (bp != null)
                found = _world.GetItemsInContainer(bp.Serial)
                              .FirstOrDefault(i => i.Graphic == graphic);
            if (found == null)
                found = _world.Items.FirstOrDefault(i => i.Graphic == graphic);
            if (found != null)
            {
                _logger?.LogDebug("UseType graphic=0x{G:X} → serial=0x{S:X}", graphic, found.Serial);
                _packet.SendToServer(PacketBuilder.DoubleClick(found.Serial));
            }
            else
                _logger?.LogDebug("UseType: no item found with graphic 0x{G:X}", graphic);
        }

        /// <summary>
        /// Ritorna l'item equipaggiato nel layer specificato, oppure null.
        /// Layer 0x19 = Riding (mount), 0x01 = One-handed weapon, ecc.
        /// </summary>
        public virtual Item? FindLayer(byte layer)
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return null;
            return _world.Items.FirstOrDefault(i => i.Container == P.Serial && i.Layer == layer);
        }

        /// <summary>Ritorna il serial dell'item equipaggiato nel layer specificato, 0 se non presente.</summary>
        public virtual uint GetLayer(byte layer) => FindLayer(layer)?.Serial ?? 0;

        /// <summary>Ritorna true se l'entità (mobile o item) con il serial dato è nel range specificato.</summary>
        public virtual bool InRange(uint serial, int range)
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return false;
            var entity = _world.FindEntity(serial);
            return entity != null && entity.DistanceTo(P) <= range;
        }

        /// <summary>Ritorna la distanza Chebyshev tra il player e l'entità con il serial dato.</summary>
        public virtual int DistanceTo(uint serial)
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return 999;
            var entity = _world.FindEntity(serial);
            return entity?.DistanceTo(P) ?? 999;
        }

        /// <summary>
        /// Tenta di montare il mobile con il serial specificato (double-click).
        /// Se il serial è 0, cerca il mobile più vicino nel raggio di 2 tile.
        /// </summary>
        public virtual void Mount(uint serial = 0)
        {
            _cancel.ThrowIfCancelled();
            if (serial != 0)
            {
                _logger?.LogDebug("Mount serial=0x{S:X}", serial);
                _packet.SendToServer(PacketBuilder.DoubleClick(serial));
                return;
            }
            if (P == null) return;
            var nearest = _world.Mobiles
                .Where(m => m.Serial != P.Serial && m.DistanceTo(P) <= 2)
                .OrderBy(m => m.DistanceTo(P))
                .FirstOrDefault();
            if (nearest != null)
            {
                _logger?.LogDebug("Mount nearest=0x{S:X}", nearest.Serial);
                _packet.SendToServer(PacketBuilder.DoubleClick(nearest.Serial));
            }
        }

        /// <summary>
        /// Smonta il personaggio: double-click sull'item nel riding layer (0x19).
        /// Se non trovato, double-click sul player stesso.
        /// </summary>
        public virtual void Dismount()
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return;
            const byte ridingLayer = 0x19;
            var mountItem = _world.Items.FirstOrDefault(i => i.Container == P.Serial && i.Layer == ridingLayer);
            if (mountItem != null)
            {
                _logger?.LogDebug("Dismount via riding layer item 0x{S:X}", mountItem.Serial);
                _packet.SendToServer(PacketBuilder.DoubleClick(mountItem.Serial));
            }
            else
            {
                _logger?.LogDebug("Dismount: fallback double-click on player");
                _packet.SendToServer(PacketBuilder.DoubleClick(P.Serial));
            }
        }

        /// <summary>
        /// Imposta la weapon ability primaria o secondaria.
        /// abilityName: "primary" | "secondary" | "clear" (o indice numerico).
        /// Invia 0xBF extended sub 0x14 (ToggleSpecialAbility) al server.
        /// </summary>
        public virtual void SetAbility(string abilityName)
        {
            _cancel.ThrowIfCancelled();
            // 0xBF extended — sub 0x14 Toggle Special Move (AOS+)
            // arg: 0 = clear, 1 = primary, 2 = secondary
            int ability = abilityName?.ToLowerInvariant() switch
            {
                "primary"   => 1,
                "secondary" => 2,
                "clear"     => 0,
                _           => int.TryParse(abilityName, out int n) ? n : 0
            };

            // Packet 0xBF sub 0x14: cmd(1) len(2) sub(2) abilityId(2)
            byte[] data = new byte[7];
            data[0] = 0xBF;
            data[1] = 0x00; data[2] = 0x07;   // length = 7
            data[3] = 0x00; data[4] = 0x14;   // sub = 0x0014
            data[5] = (byte)(ability >> 8);
            data[6] = (byte)ability;
            _packet.SendToServer(data);
            _logger?.LogDebug("SetAbility: {Name} ({Id})", abilityName, ability);
        }
    }
}
