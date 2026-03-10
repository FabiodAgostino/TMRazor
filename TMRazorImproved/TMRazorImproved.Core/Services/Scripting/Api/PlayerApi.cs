using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMRazorImproved.Core.Utilities;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Lightweight equivalente di RazorEnhanced.Point3D per gli script
    // ─────────────────────────────────────────────────────────────────────────────
    public class Point3D
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        public Point3D(int x, int y, int z) { X = x; Y = y; Z = z; }
        public override string ToString() => $"({X}, {Y}, {Z})";
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Equivalente di RazorEnhanced.BuffInfo per gli script
    // ─────────────────────────────────────────────────────────────────────────────
    public class BuffInfo
    {
        public string Name { get; }
        /// <summary>Secondi rimanenti. -1 = senza durata.</summary>
        public int Remaining { get; }
        public BuffInfo(string name, int remaining) { Name = name; Remaining = remaining; }
    }

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
        private readonly IPathFindingService? _pathfinding;
        private readonly ScriptCancellationController _cancel;
        private readonly ILogger<PlayerApi>? _logger;

        public PlayerApi(
            IWorldService world,
            IPacketService packet,
            ITargetingService targeting,
            ISkillsService skills,
            ScriptCancellationController cancel,
            IPathFindingService? pathfinding = null,
            ILogger<PlayerApi>? logger = null)
        {
            _world = world;
            _packet = packet;
            _targeting = targeting;
            _skills = skills;
            _cancel = cancel;
            _pathfinding = pathfinding;
            _logger = logger;
        }

        private Mobile? P => _world.Player;

        // ──────────────────────────────────────────────────────────────────────
        // Stats vitali
        // ──────────────────────────────────────────────────────────────────────
        public virtual int Hits    => P?.Hits    ?? 0;
        public virtual int HitsMax => P?.HitsMax ?? 0;
        public virtual int Mana    => P?.Mana    ?? 0;
        public virtual int ManaMax => P?.ManaMax ?? 0;
        public virtual int Stam    => P?.Stam    ?? 0;
        public virtual int StamMax => P?.StamMax ?? 0;

        // ──────────────────────────────────────────────────────────────────────
        // Attributi base
        // ──────────────────────────────────────────────────────────────────────
        public virtual int Str => P?.Str ?? 0;
        public virtual int Dex => P?.Dex ?? 0;
        public virtual int Int => P?.Int ?? 0;

        public virtual int StatCap => P?.StatCap ?? 0;

        // ──────────────────────────────────────────────────────────────────────
        // Resistenze elementali
        // ──────────────────────────────────────────────────────────────────────
        public virtual int FireResist    => P?.FireResist    ?? 0;
        public virtual int ColdResist    => P?.ColdResist    ?? 0;
        public virtual int PoisonResist  => P?.PoisonResist  ?? 0;
        public virtual int EnergyResist  => P?.EnergyResist  ?? 0;

        /// <summary>Alias RazorEnhanced: FireResistance</summary>
        public virtual int FireResistance   => FireResist;
        /// <summary>Alias RazorEnhanced: ColdResistance</summary>
        public virtual int ColdResistance   => ColdResist;
        /// <summary>Alias RazorEnhanced: PoisonResistance</summary>
        public virtual int PoisonResistance => PoisonResist;
        /// <summary>Alias RazorEnhanced: EnergyResistance</summary>
        public virtual int EnergyResistance => EnergyResist;
        /// <summary>Physical Resist (Armour Rating) — aggiornato via extended status packet 0x11.</summary>
        public virtual int AR => P?.AR ?? 0;

        // ──────────────────────────────────────────────────────────────────────
        // Extended stats AOS+ (aggiornati via 0x11 type>=5/6)
        // ──────────────────────────────────────────────────────────────────────
        public virtual int HitChanceIncrease       => P?.HitChanceIncrease       ?? 0;
        public virtual int SwingSpeedIncrease       => P?.SwingSpeedIncrease       ?? 0;
        public virtual int DamageChanceIncrease     => P?.DamageChanceIncrease     ?? 0;
        public virtual int LowerReagentCost         => P?.LowerReagentCost         ?? 0;
        public virtual int HitPointsRegeneration    => P?.HitPointsRegeneration    ?? 0;
        public virtual int StaminaRegeneration      => P?.StaminaRegeneration      ?? 0;
        public virtual int ManaRegeneration         => P?.ManaRegeneration         ?? 0;
        public virtual int ReflectPhysicalDamage    => P?.ReflectPhysicalDamage    ?? 0;
        public virtual int EnhancePotions           => P?.EnhancePotions           ?? 0;
        public virtual int DefenseChanceIncrease    => P?.DefenseChanceIncrease    ?? 0;
        public virtual int SpellDamageIncrease      => P?.SpellDamageIncrease      ?? 0;
        public virtual int FasterCastRecovery       => P?.FasterCastRecovery       ?? 0;
        public virtual int FasterCasting            => P?.FasterCasting            ?? 0;
        public virtual int LowerManaCost            => P?.LowerManaCost            ?? 0;
        public virtual int StrengthIncrease         => P?.StrengthIncrease         ?? 0;
        public virtual int DexterityIncrease        => P?.DexterityIncrease        ?? 0;
        public virtual int IntelligenceIncrease     => P?.IntelligenceIncrease     ?? 0;
        public virtual int HitPointsIncrease        => P?.HitPointsIncrease        ?? 0;
        public virtual int StaminaIncrease          => P?.StaminaIncrease          ?? 0;
        public virtual int ManaIncrease             => P?.ManaIncrease             ?? 0;
        public virtual int MaximumHitPointsIncrease => P?.MaximumHitPointsIncrease ?? 0;
        public virtual int MaximumStaminaIncrease   => P?.MaximumStaminaIncrease   ?? 0;

        // ──────────────────────────────────────────────────────────────────────
        // Danno e misc
        // ──────────────────────────────────────────────────────────────────────
        public virtual int MinDamage    => P?.MinDamage    ?? 0;
        public virtual int MaxDamage    => P?.MaxDamage    ?? 0;
        public virtual int Luck         => P?.Luck         ?? 0;
        public virtual int Tithe        => P?.Tithe        ?? 0;
        public virtual int Followers    => P?.Followers    ?? 0;
        public virtual int FollowersMax => P?.FollowersMax ?? 0;
        public virtual int Weight       => P?.Weight       ?? 0;
        public virtual int MaxWeight    => P?.MaxWeight    ?? 0;
        public virtual int Gold         => P?.Gold         ?? 0;
        public virtual int Armor        => P?.Armor        ?? 0;

        // ──────────────────────────────────────────────────────────────────────
        // Fame e Karma
        // ──────────────────────────────────────────────────────────────────────
        public virtual int    Karma      => P?.Karma      ?? 0;
        public virtual int    Fame       => P?.Fame       ?? 0;
        public virtual string KarmaTitle => P?.KarmaTitle ?? string.Empty;

        /// <summary>
        /// Richiede aggiornamento Fame/Karma al server (stub — sempre false in questa implementazione).
        /// </summary>
        public virtual bool UpdateKarma() => false;

        // ──────────────────────────────────────────────────────────────────────
        // Identificazione
        // ──────────────────────────────────────────────────────────────────────
        public virtual uint   Serial   => P?.Serial  ?? 0;
        public virtual string Name     => P?.Name    ?? string.Empty;
        public virtual int    Graphic  => P?.Graphic ?? 0;
        public virtual int    Hue      => P?.Hue     ?? 0;
        /// <summary>Alias RazorEnhanced: Body</summary>
        public virtual int Body     => Graphic;
        /// <summary>Alias RazorEnhanced: MobileID</summary>
        public virtual int MobileID => Graphic;

        // ──────────────────────────────────────────────────────────────────────
        // Flags stato
        // ──────────────────────────────────────────────────────────────────────
        public virtual bool IsConnected  => P != null;
        /// <summary>Alias RazorEnhanced: Connected</summary>
        public virtual bool Connected    => IsConnected;
        public virtual bool IsPoisoned   => P?.IsPoisoned   ?? false;
        /// <summary>Alias RazorEnhanced: Poisoned</summary>
        public virtual bool Poisoned     => IsPoisoned;
        public virtual bool IsYellowHits => P?.IsYellowHits ?? false;
        /// <summary>Alias RazorEnhanced: YellowHits</summary>
        public virtual bool YellowHits   => IsYellowHits;
        public virtual bool IsHidden     => P?.IsHidden     ?? false;
        /// <summary>Player è visibile (non nascosto).</summary>
        public virtual bool Visible      => !IsHidden;
        public virtual bool WarMode      => P?.WarMode      ?? false;
        public virtual bool IsGhost      => P?.IsGhost      ?? false;
        public virtual bool Paralized    => P?.Paralyzed    ?? false;
        public virtual bool Female       => P?.Female       ?? false;

        // Abilità speciali (tracciato lato client via pacchetti 0xBF)
        public virtual bool HasSpecial          { get; protected set; }
        public virtual bool HasPrimarySpecial   { get; protected set; }
        public virtual bool HasSecondarySpecial { get; protected set; }
        public virtual uint PrimarySpecial      { get; protected set; }
        public virtual uint SecondarySpecial    { get; protected set; }

        // ──────────────────────────────────────────────────────────────────────
        // Posizione e mappa
        // ──────────────────────────────────────────────────────────────────────
        public virtual int X     => P?.X     ?? 0;
        public virtual int Y     => P?.Y     ?? 0;
        public virtual int Z     => P?.Z     ?? 0;
        public virtual int MapId => P?.MapId ?? 0;
        /// <summary>Alias RazorEnhanced: Map</summary>
        public virtual int Map   => MapId;

        /// <summary>Posizione corrente come oggetto Point3D.</summary>
        public virtual Point3D Position => new(X, Y, Z);

        /// <summary>Direzione corrente come stringa (North, South, East, West…).</summary>
        public virtual string Direction
        {
            get
            {
                if (P == null) return "Undefined";
                return (P.Direction & 0x07) switch
                {
                    0 => "North",
                    1 => "Right",
                    2 => "East",
                    3 => "Down",
                    4 => "South",
                    5 => "Left",
                    6 => "West",
                    7 => "Up",
                    _ => "Undefined"
                };
            }
        }

        /// <summary>Valore numerico della direzione (0-7).</summary>
        public virtual int DirectionNum => P?.Direction ?? 0;

        // ──────────────────────────────────────────────────────────────────────
        // Area e zona (non implementate — richiede file regions.json)
        // ──────────────────────────────────────────────────────────────────────
        public virtual string Area() => "Unknown";
        public virtual string Zone() => "Unknown";

        // ──────────────────────────────────────────────────────────────────────
        // Notorietà
        // ──────────────────────────────────────────────────────────────────────
        public virtual int Notoriety => P?.Notoriety ?? 0;

        // ──────────────────────────────────────────────────────────────────────
        // Percentuali di utilità per script
        // ──────────────────────────────────────────────────────────────────────
        public virtual double HitsPct => HitsMax > 0 ? (double)Hits / HitsMax * 100 : 0;
        public virtual double ManaPct  => ManaMax > 0 ? (double)Mana / ManaMax * 100 : 0;
        public virtual double StamPct  => StamMax > 0 ? (double)Stam / StamMax * 100 : 0;

        // ──────────────────────────────────────────────────────────────────────
        // Inventario e layer
        // ──────────────────────────────────────────────────────────────────────
        /// <summary>Backpack del player.</summary>
        public virtual Item? Backpack
        {
            get { _cancel.ThrowIfCancelled(); return P?.Backpack; }
        }

        /// <summary>Bank box (layer 0x1D).</summary>
        public virtual Item? Bank => FindLayer(0x1D);

        /// <summary>Faretra/Quiver (layer 0x16).</summary>
        public virtual Item? Quiver => FindLayer(0x16);

        /// <summary>Mount corrente (layer 0x19).</summary>
        public virtual Item? Mount => FindLayer(0x19);

        /// <summary>True se il player è in sella (layer riding 0x19 occupato).</summary>
        public virtual bool IsOnMount
        {
            get
            {
                _cancel.ThrowIfCancelled();
                if (P == null) return false;
                return _world.Items.Any(i => i.Container == P.Serial && i.Layer == 0x19);
            }
        }

        /// <summary>Ritorna l'item equipaggiato nel layer (stringa).</summary>
        public virtual Item? GetItemOnLayer(string layerName)
        {
            _cancel.ThrowIfCancelled();
            if (P == null || !Enum.TryParse<Layer>(layerName, true, out var layer))
                return null;
            return _world.Items.FirstOrDefault(i => i.Container == P.Serial && i.Layer == (byte)layer);
        }

        /// <summary>Ritorna l'item equipaggiato nel layer (byte).</summary>
        public virtual Item? FindLayer(byte layer)
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return null;
            return _world.Items.FirstOrDefault(i => i.Container == P.Serial && i.Layer == layer);
        }

        /// <summary>Ritorna il serial dell'item nel layer specificato, 0 se assente.</summary>
        public virtual uint GetLayer(byte layer) => FindLayer(layer)?.Serial ?? 0;

        /// <summary>True se il layer specificato è occupato da un item.</summary>
        public virtual bool CheckLayer(string layerName)
        {
            _cancel.ThrowIfCancelled();
            return GetItemOnLayer(layerName) != null;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Pets e Follower
        // ──────────────────────────────────────────────────────────────────────
        /// <summary>Lista approssimata di pet/follower amichevoli nel range visivo.</summary>
        public virtual List<uint> AllFollowers
        {
            get
            {
                _cancel.ThrowIfCancelled();
                return _world.Mobiles
                    .Where(m => m.Notoriety == 1 && m.Serial != (P?.Serial ?? 0))
                    .Select(m => m.Serial)
                    .ToList();
            }
        }

        /// <summary>Lista di mobili rinominabili (approssimazione di Pet).</summary>
        public virtual List<uint> Pets
        {
            get
            {
                _cancel.ThrowIfCancelled();
                return _world.Mobiles
                    .Where(m => m.Notoriety == 1 && m.Serial != (P?.Serial ?? 0))
                    .Select(m => m.Serial)
                    .ToList();
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // Party
        // ──────────────────────────────────────────────────────────────────────
        public virtual List<uint> AllParty
        {
            get { _cancel.ThrowIfCancelled(); return _world.PartyMembers.ToList(); }
        }

        public virtual bool InParty(uint serial)
        {
            _cancel.ThrowIfCancelled();
            return _world.PartyMembers.Contains(serial);
        }

        /// <summary>Invia un invito al party (richiede target ingame).</summary>
        public virtual void PartyInvite()
        {
            _cancel.ThrowIfCancelled();
            // 0xBF sub 0x06: party invite (target selection)
            byte[] pkt = { 0xBF, 0x00, 0x05, 0x00, 0x06 };
            _packet.SendToServer(pkt);
        }

        /// <summary>Accetta un invito al party.</summary>
        public virtual bool PartyAccept(uint fromSerial = 0)
        {
            _cancel.ThrowIfCancelled();
            // 0xBF sub 0x08: accept party
            byte[] pkt = new byte[10];
            pkt[0] = 0xBF;
            pkt[1] = 0x00; pkt[2] = 0x0A;
            pkt[3] = 0x00; pkt[4] = 0x08;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5), fromSerial);
            _packet.SendToServer(pkt);
            return _world.PartyMembers.Count > 0;
        }

        /// <summary>Lascia il party corrente.</summary>
        public virtual void LeaveParty()
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return;
            // 0xBF sub 0x02: remove member (self)
            byte[] pkt = new byte[10];
            pkt[0] = 0xBF;
            pkt[1] = 0x00; pkt[2] = 0x0A;
            pkt[3] = 0x00; pkt[4] = 0x02;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5), P.Serial);
            _packet.SendToServer(pkt);
        }

        /// <summary>Espelle un membro dal party (solo party leader).</summary>
        public virtual void KickMember(uint serial)
        {
            _cancel.ThrowIfCancelled();
            byte[] pkt = new byte[10];
            pkt[0] = 0xBF;
            pkt[1] = 0x00; pkt[2] = 0x0A;
            pkt[3] = 0x00; pkt[4] = 0x02;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5), serial);
            _packet.SendToServer(pkt);
        }

        /// <summary>Imposta il permesso di saccheggio nel party.</summary>
        public virtual void PartyCanLoot(bool canLoot)
        {
            _cancel.ThrowIfCancelled();
            // 0xBF sub 0x07: party loot permission
            byte[] pkt = { 0xBF, 0x00, 0x06, 0x00, 0x07, (byte)(canLoot ? 0x01 : 0x00) };
            _packet.SendToServer(pkt);
        }

        /// <summary>Invia messaggio al party.</summary>
        public virtual void ChatParty(string msg, uint recipientSerial = 0)
        {
            _cancel.ThrowIfCancelled();
            byte[] msgBytes = Encoding.BigEndianUnicode.GetBytes(msg + "\0");
            if (recipientSerial != 0)
            {
                // Private party message: sub 0x03
                byte[] pkt = new byte[10 + msgBytes.Length];
                pkt[0] = 0xBF;
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)pkt.Length);
                pkt[3] = 0x00; pkt[4] = 0x03;
                System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5), recipientSerial);
                Array.Copy(msgBytes, 0, pkt, 9, msgBytes.Length);
                _packet.SendToServer(pkt);
            }
            else
            {
                // Party message: sub 0x04
                byte[] pkt = new byte[5 + msgBytes.Length];
                pkt[0] = 0xBF;
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)pkt.Length);
                pkt[3] = 0x00; pkt[4] = 0x04;
                Array.Copy(msgBytes, 0, pkt, 5, msgBytes.Length);
                _packet.SendToServer(pkt);
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // Chat
        // ──────────────────────────────────────────────────────────────────────
        public virtual void Chat(string message, int hue = 0) => SendSpeech(message, 0x00, hue == 0 ? 0x0034 : hue);

        public virtual void ChatSay(string message, int hue = 0x0451) => SendSpeech(message, 0x00, hue);

        public virtual void ChatWhisper(string message)  => SendSpeech(message, 0x02, 0x3B2);
        public virtual void ChatYell(string message)     => SendSpeech(message, 0x04, 0x021);
        public virtual void ChatEmote(string message)    => SendSpeech(message, 0x03, 0x024);

        /// <summary>Invia messaggio alla chat di gilda.</summary>
        public virtual void ChatGuild(string message)
        {
            _cancel.ThrowIfCancelled();
            SendSpeech(message, 0x0D, 0x044); // MessageType.Guild = 0x0D
        }

        /// <summary>Invia messaggio alla chat di alleanza.</summary>
        public virtual void ChatAlliance(string message)
        {
            _cancel.ThrowIfCancelled();
            SendSpeech(message, 0x0E, 0x057); // MessageType.Alliance = 0x0E
        }

        /// <summary>Invia messaggio al canale chat (ChatAction 0x61).</summary>
        public virtual void ChatChannel(string message)
        {
            _cancel.ThrowIfCancelled();
            // 0x98 packet or ChatAction: use regular speech type 0x0C
            SendSpeech(message, 0x0C, 0x034);
        }

        /// <summary>MapSay — invia il messaggio come speech regolare (UOAssist non disponibile).</summary>
        public virtual void MapSay(string message) => ChatSay(message);

        /// <summary>Emote action (animazione testo).</summary>
        public virtual void EmoteAction(string action)
        {
            _cancel.ThrowIfCancelled();
            if (string.IsNullOrEmpty(action)) return;
            byte[] data = Encoding.ASCII.GetBytes(action);
            byte[] pkt = new byte[3 + data.Length];
            pkt[0] = 0x12;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)pkt.Length);
            Array.Copy(data, 0, pkt, 2, data.Length);
            _packet.SendToServer(pkt);
        }

        private void SendSpeech(string message, byte type, int hue)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(PacketBuilder.UnicodeSpeech(message, type, (ushort)hue));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Messaggi sopra la testa
        // ──────────────────────────────────────────────────────────────────────
        public virtual void HeadMsg(string message, int hue = 945)
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return;
            byte[] msgBytes = Encoding.BigEndianUnicode.GetBytes(message + "\0");
            int size = 48 + msgBytes.Length;
            byte[] packet = new byte[size];
            packet[0] = 0xAE;
            packet[1] = (byte)(size >> 8); packet[2] = (byte)size;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(3), Serial);
            packet[7] = (byte)(P.Graphic >> 8); packet[8] = (byte)(P.Graphic & 0xff);
            packet[9] = 0x00;
            packet[10] = (byte)(hue >> 8); packet[11] = (byte)hue;
            packet[12] = 0x00; packet[13] = 0x03;
            packet[14] = (byte)'e'; packet[15] = (byte)'n'; packet[16] = (byte)'u'; packet[17] = 0;
            string sysName = "System";
            for (int i = 0; i < sysName.Length && i < 30; i++) packet[18 + i] = (byte)sysName[i];
            Array.Copy(msgBytes, 0, packet, 48, msgBytes.Length);
            _packet.SendToClient(packet);
        }

        /// <summary>Alias RazorEnhanced: HeadMessage(color, msg)</summary>
        public virtual void HeadMessage(int color, string msg) => HeadMsg(msg, color);

        // ──────────────────────────────────────────────────────────────────────
        // Combat
        // ──────────────────────────────────────────────────────────────────────
        public virtual void Attack(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _logger?.LogDebug("Attack: target=0x{Serial:X}", serial);
            _packet.SendToServer(PacketBuilder.Attack(serial));
        }

        public virtual void TargetSelf() => _targeting.TargetSelf();
        public virtual void TargetLast() => _targeting.SendTarget(_targeting.LastTarget);

        /// <summary>Attacca l'ultimo target noto.</summary>
        public virtual void AttackLast()
        {
            _cancel.ThrowIfCancelled();
            if (_targeting.LastTarget != 0)
                _packet.SendToServer(PacketBuilder.Attack(_targeting.LastTarget));
        }

        /// <summary>
        /// Attacca il mobile più vicino con il graphic specificato nel range dato.
        /// selector: "Nearest" | "Random" | "Weakest" | "Strongest".
        /// </summary>
        public virtual bool AttackType(int graphic, int rangeMax, string selector = "Nearest",
            List<int>? color = null, List<byte>? notoriety = null)
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return false;
            var candidates = _world.Mobiles
                .Where(m => m.Serial != P.Serial && m.DistanceTo(P) <= rangeMax)
                .Where(m => graphic <= 0 || m.Graphic == graphic)
                .Where(m => color == null || color.Count == 0 || color.Contains(m.Hue))
                .Where(m => notoriety == null || notoriety.Count == 0 || notoriety.Contains(m.Notoriety));

            Mobile? target = selector?.ToLowerInvariant() switch
            {
                "random"    => candidates.OrderBy(_ => Guid.NewGuid()).FirstOrDefault(),
                "weakest"   => candidates.OrderBy(m => m.Hits).FirstOrDefault(),
                "strongest" => candidates.OrderByDescending(m => m.Hits).FirstOrDefault(),
                _           => candidates.OrderBy(m => m.DistanceTo(P)).FirstOrDefault()
            };

            if (target == null) return false;
            _targeting.SetLastTarget(target.Serial);
            _packet.SendToServer(PacketBuilder.Attack(target.Serial));
            return true;
        }

        // ──────────────────────────────────────────────────────────────────────
        // War mode e resync
        // ──────────────────────────────────────────────────────────────────────
        public virtual void ToggleWarMode()
        {
            _cancel.ThrowIfCancelled();
            bool current = P?.WarMode ?? false;
            byte[] pkt = { 0x72, (byte)(current ? 0x00 : 0x01), 0x00, 0x32, 0x00 };
            _packet.SendToServer(pkt);
        }

        public virtual void SetWarMode(bool warMode)
        {
            _cancel.ThrowIfCancelled();
            byte[] pkt = { 0x72, (byte)(warMode ? 0x01 : 0x00), 0x00, 0x32, 0x00 };
            _packet.SendToServer(pkt);
        }

        public virtual void Resync()
        {
            _cancel.ThrowIfCancelled();
            byte[] pkt = { 0x22, 0xFF, 0x00 };
            _packet.SendToServer(pkt);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Virtù
        // ──────────────────────────────────────────────────────────────────────
        private static readonly Dictionary<string, byte> _virtueIds = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Honor"]        = 0x01,
            ["Sacrifice"]    = 0x02,
            ["Valor"]        = 0x03,
            ["Compassion"]   = 0x04,
            ["Honesty"]      = 0x05,
            ["Humility"]     = 0x06,
            ["Justice"]      = 0x07,
            ["Spirituality"] = 0x08,
        };

        public virtual void InvokeVirtue(string virtue)
        {
            _cancel.ThrowIfCancelled();
            if (!_virtueIds.TryGetValue(virtue, out byte id))
            {
                _logger?.LogWarning("InvokeVirtue: virtue '{V}' not recognized", virtue);
                return;
            }
            InvokeVirtue(id);
        }

        public virtual void InvokeVirtue(int virtueId)
        {
            _cancel.ThrowIfCancelled();
            // Packet 0x12, type 0xF4: InvokeVirtue
            byte[] pkt = { 0x12, 0x00, 0x05, 0xF4, (byte)virtueId };
            _packet.SendToServer(pkt);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Movimento
        // ──────────────────────────────────────────────────────────────────────
        private static readonly Dictionary<string, byte> _directions = new(StringComparer.OrdinalIgnoreCase)
        {
            ["North"] = 0, ["Right"] = 1, ["East"] = 2, ["Down"] = 3,
            ["South"] = 4, ["Left"]  = 5, ["West"] = 6, ["Up"]   = 7,
        };

        /// <summary>Esegue un passo in corsa nella direzione specificata.</summary>
        public virtual bool Run(string direction) => Move(direction, running: true);

        /// <summary>Esegue un passo a piedi nella direzione specificata.</summary>
        public virtual bool Walk(string direction) => Move(direction, running: false);

        private bool Move(string direction, bool running)
        {
            _cancel.ThrowIfCancelled();
            if (!_directions.TryGetValue(direction, out byte dir))
            {
                _logger?.LogWarning("Move: direction '{D}' not recognized", direction);
                return false;
            }
            byte flag = running ? (byte)(dir | 0x80) : dir;
            // 0x02 WalkRequest: cmd(1) dir(1) seqNum(1) fastwalk(4) — seqNum gestito dal client
            byte[] pkt = { 0x02, flag, 0x00, 0x00, 0x00, 0x00, 0x00 };
            _packet.SendToServer(pkt);
            return true;
        }

        /// <summary>Pathfinding verso coordinate x/y/z.</summary>
        public virtual void PathFindTo(int x, int y, int z)
        {
            _cancel.ThrowIfCancelled();
            if (_pathfinding == null || P == null)
            {
                _logger?.LogDebug("PathFindTo: no pathfinding service available");
                return;
            }
            var path = _pathfinding.GetPath(P.X, P.Y, P.Z, x, y, P.MapId);
            if (path == null)
            {
                _logger?.LogDebug("PathFindTo: no path found to ({X},{Y},{Z})", x, y, z);
                return;
            }
            foreach (var step in path)
            {
                _cancel.ThrowIfCancelled();
                int dx = step.X - P.X, dy = step.Y - P.Y;
                string dir = (dx, dy) switch
                {
                    (0, -1) => "North", (1, -1) => "Right",  (1, 0) => "East",  (1, 1) => "Down",
                    (0, 1)  => "South", (-1, 1) => "Left",  (-1, 0) => "West", (-1,-1) => "Up",
                    _       => "North"
                };
                Move(dir, running: false);
            }
        }

        /// <summary>Pathfinding verso un Point3D.</summary>
        public virtual void PathFindTo(Point3D location) => PathFindTo(location.X, location.Y, location.Z);

        // ──────────────────────────────────────────────────────────────────────
        // AlwaysRun e StaticMount (stub — richiede integrazione client)
        // ──────────────────────────────────────────────────────────────────────
        public virtual void ToggleAlwaysRun() { /* stub: non supportato */ }
        public virtual int  StaticMount    => 0;
        public virtual void SetStaticMount(int serial) { /* stub */ }

        // ──────────────────────────────────────────────────────────────────────
        // Mount e Volo
        // ──────────────────────────────────────────────────────────────────────
        public virtual void MountAnimal(uint serial = 0)
        {
            _cancel.ThrowIfCancelled();
            if (serial != 0)
            {
                _packet.SendToServer(PacketBuilder.DoubleClick(serial));
                return;
            }
            if (P == null) return;
            var nearest = _world.Mobiles
                .Where(m => m.Serial != P.Serial && m.DistanceTo(P) <= 2)
                .OrderBy(m => m.DistanceTo(P)).FirstOrDefault();
            if (nearest != null) _packet.SendToServer(PacketBuilder.DoubleClick(nearest.Serial));
        }

        public virtual void Dismount()
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return;
            var mountItem = FindLayer(0x19);
            _packet.SendToServer(PacketBuilder.DoubleClick(mountItem?.Serial ?? P.Serial));
        }

        /// <summary>Toggle volo Gargoyle.</summary>
        public virtual void Fly()
        {
            _cancel.ThrowIfCancelled();
            byte[] data = { 0x12, 0x00, 0x06, 0x56, 0x00, 0x00 };
            _packet.SendToServer(data);
        }

        /// <summary>Fly con stato esplicito. True = attiva volo, False = disattiva.</summary>
        public virtual void Fly(bool status)
        {
            _cancel.ThrowIfCancelled();
            bool flying = P?.Flying ?? false;
            if (status && !flying) Fly();
            else if (!status && flying) Fly();
        }

        /// <summary>Alias di Fly() per coerenza con RazorEnhanced.</summary>
        public virtual void Land() => Fly();

        // ──────────────────────────────────────────────────────────────────────
        // Abilità Speciali
        // ──────────────────────────────────────────────────────────────────────
        public virtual void WeaponPrimary() => SendSpecialMove(1);
        public virtual void WeaponSecondary() => SendSpecialMove(2);
        public virtual void Stun()  => SendSpecialMove(3);
        public virtual void Disarm() => SendSpecialMove(4);

        public virtual void WeaponPrimarySA()   => SendSpecialMoveSA(primary: true);
        public virtual void WeaponSecondarySA()  => SendSpecialMoveSA(primary: false);
        public virtual void WeaponClearSA()      => ClearSpecialMove();
        public virtual void WeaponDisarmSA()     => SendSpecialMove(4);
        public virtual void WeaponStunSA()       => SendSpecialMove(3);

        private void SendSpecialMove(int id)
        {
            _cancel.ThrowIfCancelled();
            byte[] pkt = { 0xBF, 0x00, 0x07, 0x00, 0x25, 0x00, (byte)id };
            _packet.SendToServer(pkt);
        }

        private void SendSpecialMoveSA(bool primary)
        {
            _cancel.ThrowIfCancelled();
            // 0xD7 packet abilityType
            byte type = primary ? (byte)0x01 : (byte)0x02;
            byte[] pkt = new byte[10];
            pkt[0] = 0xD7;
            pkt[1] = 0x00; pkt[2] = 0x0A;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), Serial);
            pkt[7] = 0x00; pkt[8] = 0x61; // sub = 0x0061 (UseSpecialAbility)
            pkt[9] = type;
            _packet.SendToServer(pkt);
        }

        private void ClearSpecialMove()
        {
            _cancel.ThrowIfCancelled();
            byte[] pkt = { 0xBF, 0x00, 0x07, 0x00, 0x25, 0x00, 0x00 };
            _packet.SendToServer(pkt);
        }

        public virtual void SetAbility(string abilityName)
        {
            _cancel.ThrowIfCancelled();
            int ability = abilityName?.ToLowerInvariant() switch
            {
                "primary"   => 1,
                "secondary" => 2,
                "clear"     => 0,
                _           => int.TryParse(abilityName, out int n) ? n : 0
            };
            byte[] data = { 0xBF, 0x00, 0x07, 0x00, 0x14, (byte)(ability >> 8), (byte)ability };
            _packet.SendToServer(data);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Equip / Unequip
        // ──────────────────────────────────────────────────────────────────────
        public virtual void EquipItem(uint serial)
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return;
            var item = _world.FindItem(serial);
            if (item == null) { _logger?.LogWarning("EquipItem: serial 0x{S:X} not found", serial); return; }
            _packet.SendToServer(PacketBuilder.LiftItem(item.Serial, item.Amount));
            _packet.SendToServer(PacketBuilder.WearItem(item.Serial, item.Layer, P.Serial));
        }

        public virtual void UnEquipItemByLayer(string layerName)
        {
            _cancel.ThrowIfCancelled();
            if (P == null || P.Backpack == null) return;
            var item = GetItemOnLayer(layerName);
            if (item == null) return;
            _packet.SendToServer(PacketBuilder.LiftItem(item.Serial, item.Amount));
            _packet.SendToServer(PacketBuilder.DropToContainer(item.Serial, P.Backpack.Serial));
        }

        public virtual void EquipLastWeapon()
        {
            _cancel.ThrowIfCancelled();
            // 0x89: EquipLastWeapon
            byte[] pkt = { 0x89, 0x00, 0x00, 0x00, 0x00 };
            if (P != null) System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), P.Serial);
            _packet.SendToServer(pkt);
        }

        /// <summary>Equipaggia una lista di serials con il pacchetto UO3D (0xEC).</summary>
        public virtual void EquipUO3D(List<uint> serials)
        {
            _cancel.ThrowIfCancelled();
            if (serials == null || serials.Count == 0) return;
            int len = 5 + serials.Count * 4;
            byte[] pkt = new byte[len];
            pkt[0] = 0xEC;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)len);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(3), (ushort)serials.Count);
            for (int i = 0; i < serials.Count; i++)
                System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5 + i * 4), serials[i]);
            _packet.SendToServer(pkt);
        }

        /// <summary>De-equipaggia una lista di layer con il pacchetto UO3D (0xEE).</summary>
        public virtual void UnEquipUO3D(List<string> layers)
        {
            _cancel.ThrowIfCancelled();
            if (layers == null || layers.Count == 0) return;
            var layerBytes = new List<byte>();
            foreach (var ln in layers)
                if (Enum.TryParse<Layer>(ln, true, out var l)) layerBytes.Add((byte)l);
            if (layerBytes.Count == 0) return;
            int len = 5 + layerBytes.Count * 2;
            byte[] pkt = new byte[len];
            pkt[0] = 0xEE;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), (ushort)len);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(3), (ushort)layerBytes.Count);
            for (int i = 0; i < layerBytes.Count; i++)
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(5 + i * 2), layerBytes[i]);
            _packet.SendToServer(pkt);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Skills
        // ──────────────────────────────────────────────────────────────────────
        public virtual double GetSkillValue(string skillName)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
            return skill?.Value ?? 0;
        }

        public virtual double GetRealSkillValue(string skillName)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
            return skill?.BaseValue ?? 0;
        }

        public virtual double GetSkillCap(string skillName)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
            return skill?.Cap ?? 0;
        }

        /// <summary>Lock status: 0=Up, 1=Down, 2=Locked. -1 se skill non trovata.</summary>
        public virtual int GetSkillStatus(string skillName)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
            return skill == null ? -1 : (int)skill.Lock;
        }

        /// <summary>Imposta il lock di una skill. status: 0=Up, 1=Down, 2=Locked.</summary>
        public virtual void SetSkillStatus(string skillName, int status)
        {
            _cancel.ThrowIfCancelled();
            if (status < 0 || status > 2) return;
            var skill = _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
            if (skill == null) return;
            _skills.SetLock(skill.ID, (SkillLock)status);
            _packet.SendToServer(PacketBuilder.SetSkillLock(skill.ID, (byte)status));
            _packet.SendToClient(PacketBuilder.SkillUpdate(skill.ID, skill.Value, skill.BaseValue, skill.Cap, (byte)status));
        }

        /// <summary>GetStatStatus — stub (non supportato).</summary>
        public virtual int  GetStatStatus(string statName) => -1;
        /// <summary>SetStatStatus — invia 0x56 stat lock packet.</summary>
        public virtual void SetStatStatus(string statName, int status)
        {
            _cancel.ThrowIfCancelled();
            if (status < 0 || status > 2) return;
            byte statId = statName?.ToLowerInvariant() switch
            {
                "strength" or "str"      => 0,
                "dexterity" or "dex"     => 1,
                "intelligence" or "int"  => 2,
                _ => 0xFF
            };
            if (statId == 0xFF) return;
            // 0x56: LockStats C→S
            byte[] pkt = { 0x56, statId, (byte)status };
            _packet.SendToServer(pkt);
        }

        public virtual void UseSkill(string skillName)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
            if (skill == null) return;
            _packet.SendToServer(PacketBuilder.UseSkill(skill.ID));
        }

        /// <summary>Usa la skill su un target specifico (serial).</summary>
        public virtual void UseSkill(string skillName, uint targetSerial)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
            if (skill == null) return;
            // Use Targeted Skill 0x12 type 0x2C
            byte[] pkt = new byte[9];
            pkt[0] = 0x12;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(1), 9);
            pkt[3] = 0x2C;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(4), (ushort)skill.ID);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5), targetSerial);
            _packet.SendToServer(pkt);
        }

        public virtual void UseSkillOnly(string skillName)    => UseSkill(skillName);

        /// <summary>Ritorna tutti gli skill come dizionario nome→valore corrente.</summary>
        public virtual Dictionary<string, double> GetAllSkillsDict()
        {
            _cancel.ThrowIfCancelled();
            var dict = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in _skills.Skills) dict[s.Name] = s.Value;
            return dict;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Spell e buff
        // ──────────────────────────────────────────────────────────────────────
        public virtual void Cast(string spellName)
        {
            _cancel.ThrowIfCancelled();
            if (!SpellsApi.TryGetSpellId(spellName, out int spellId))
            {
                _logger?.LogWarning("Cast: spell '{SpellName}' not found", spellName);
                return;
            }
            _packet.SendToServer(PacketBuilder.CastSpell(spellId));
        }

        /// <summary>SpellIsEnabled — stub (richiede SkillEnabled tracking server-side).</summary>
        public virtual bool SpellIsEnabled(string spellName) => false;

        public virtual bool CheckBuffs(string buffName)
        {
            _cancel.ThrowIfCancelled();
            if (P == null || string.IsNullOrEmpty(buffName)) return false;
            if (P.ActiveBuffs.ContainsKey(buffName)) return true;
            if (P.Properties == null) return false;
            return P.Properties.Properties.Any(p =>
                p.Arguments.Contains(buffName, StringComparison.OrdinalIgnoreCase));
        }

        public virtual bool HasBuff(string buffName) => CheckBuffs(buffName);

        /// <summary>Alias RazorEnhanced: BuffsExist</summary>
        public virtual bool BuffsExist(string buffName) => CheckBuffs(buffName);

        /// <summary>Lista dei buff attivi come stringhe.</summary>
        public virtual List<string> Buffs
        {
            get
            {
                _cancel.ThrowIfCancelled();
                if (P == null) return new List<string>();
                return P.ActiveBuffs.Keys.ToList();
            }
        }

        /// <summary>Lista dei buff attivi come oggetti BuffInfo.</summary>
        public virtual List<BuffInfo> BuffsInfo
        {
            get
            {
                _cancel.ThrowIfCancelled();
                if (P == null) return new List<BuffInfo>();
                return P.ActiveBuffs
                    .Select(kv => new BuffInfo(kv.Key, kv.Value))
                    .ToList();
            }
        }

        /// <summary>Ritorna le info di un buff attivo per nome.</summary>
        public virtual BuffInfo? GetBuffInfo(string buffName)
        {
            _cancel.ThrowIfCancelled();
            if (P == null || string.IsNullOrEmpty(buffName)) return null;
            if (P.ActiveBuffs.TryGetValue(buffName, out int remaining))
                return new BuffInfo(buffName, remaining);
            var match = P.ActiveBuffs.FirstOrDefault(kv =>
                kv.Key.Contains(buffName, StringComparison.OrdinalIgnoreCase));
            return match.Key != null ? new BuffInfo(match.Key, match.Value) : null;
        }

        /// <summary>Ritorna i secondi rimanenti di un buff (0 se non attivo).</summary>
        public virtual int BuffTime(string buffName)
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return 0;
            return P.ActiveBuffs.TryGetValue(buffName, out int v) ? v : 0;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Bandage
        // ──────────────────────────────────────────────────────────────────────
        public virtual void BandageSelf()
        {
            _cancel.ThrowIfCancelled();
            Bandage(P?.Serial ?? 0);
        }

        public virtual void Bandage(uint targetSerial)
        {
            _cancel.ThrowIfCancelled();
            if (targetSerial == 0) return;
            var bp = P?.Backpack;
            if (bp == null) return;
            var bandage = _world.Items.FirstOrDefault(i => i.Container == bp.Serial && i.Graphic == 0x0E75);
            if (bandage == null) return;
            _packet.SendToServer(PacketBuilder.DoubleClick(bandage.Serial));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Contenitori
        // ──────────────────────────────────────────────────────────────────────
        public virtual void OpenBackpack()
        {
            _cancel.ThrowIfCancelled();
            var bp = P?.Backpack;
            if (bp != null) _packet.SendToServer(PacketBuilder.DoubleClick(bp.Serial));
        }

        public virtual void OpenContainer(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(PacketBuilder.DoubleClick(serial));
        }

        /// <summary>Apre il paperdoll del player (0xBF sub 0x0F).</summary>
        public virtual void OpenPaperDoll()
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return;
            byte[] pkt = new byte[9];
            pkt[0] = 0xBF;
            pkt[1] = 0x00; pkt[2] = 0x09;
            pkt[3] = 0x00; pkt[4] = 0x0F;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5), P.Serial);
            _packet.SendToServer(pkt);
        }

        public virtual void QuestButton()
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return;
            // 0xD7 sub 0x00B5: open quests
            byte[] pkt = new byte[10];
            pkt[0] = 0xD7;
            pkt[1] = 0x00; pkt[2] = 0x0A;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), P.Serial);
            pkt[7] = 0x00; pkt[8] = 0xB5; pkt[9] = 0x00;
            _packet.SendToServer(pkt);
        }

        public virtual void GuildButton()
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return;
            // 0xD7 sub 0x00B4: open guild
            byte[] pkt = new byte[10];
            pkt[0] = 0xD7;
            pkt[1] = 0x00; pkt[2] = 0x0A;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), P.Serial);
            pkt[7] = 0x00; pkt[8] = 0xB4; pkt[9] = 0x00;
            _packet.SendToServer(pkt);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Range
        // ──────────────────────────────────────────────────────────────────────
        public virtual bool InRange(uint serial, int range)
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return false;
            var entity = _world.FindEntity(serial);
            return entity != null && entity.DistanceTo(P) <= range;
        }

        public virtual int DistanceTo(uint serial)
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return 999;
            var entity = _world.FindEntity(serial);
            return entity?.DistanceTo(P) ?? 999;
        }

        public virtual bool InRangeMobile(uint serial, int range) => InRange(serial, range);
        public virtual bool InRangeItem(uint serial, int range)   => InRange(serial, range);

        // ──────────────────────────────────────────────────────────────────────
        // Item usage
        // ──────────────────────────────────────────────────────────────────────
        public virtual void UseItem(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(PacketBuilder.DoubleClick(serial));
        }

        public virtual void UseType(int graphic)
        {
            _cancel.ThrowIfCancelled();
            var bp = P?.Backpack;
            Item? found = null;
            if (bp != null)
                found = _world.GetItemsInContainer(bp.Serial).FirstOrDefault(i => i.Graphic == graphic);
            found ??= _world.Items.FirstOrDefault(i => i.Graphic == graphic);
            if (found != null) _packet.SendToServer(PacketBuilder.DoubleClick(found.Serial));
        }

        // ──────────────────────────────────────────────────────────────────────
        // Corpses
        // ──────────────────────────────────────────────────────────────────────
        public virtual HashSet<uint> Corpses => P?.CorpseSerials ?? new HashSet<uint>();
        public virtual void ClearCorpseList() => P?.CorpseSerials.Clear();

        // ──────────────────────────────────────────────────────────────────────
        // Tracking Arrow
        // ──────────────────────────────────────────────────────────────────────
        public virtual void TrackingArrow(int x, int y, bool display, uint target = 0)
        {
            _cancel.ThrowIfCancelled();
            if (target == 0) target = Serial;
            // 0xBA: Display Tracking Arrow
            byte[] pkt = new byte[11];
            pkt[0] = 0xBA;
            pkt[1] = (byte)(display ? 0x01 : 0x00);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(2), (ushort)x);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(4), (ushort)y);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(6), target);
            pkt[10] = 0x00;
            _packet.SendToClient(pkt);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Polymorphism
        // ──────────────────────────────────────────────────────────────────────
        public virtual bool IsPolymorph
        {
            get
            {
                var p = P;
                if (p == null) return false;
                return p.Graphic != 0x190 && p.Graphic != 0x191 &&
                       p.Graphic != 0x25D && p.Graphic != 0x25E &&
                       p.Graphic != 0x29A && p.Graphic != 0x29B;
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // OPL / Properties
        // ──────────────────────────────────────────────────────────────────────
        public virtual List<string> GetPropStringList()
        {
            _cancel.ThrowIfCancelled();
            if (P?.Properties == null) return new List<string>();
            return P.Properties.Properties.Select(p => p.Arguments).ToList();
        }

        public virtual string GetPropStringByIndex(int index)
        {
            _cancel.ThrowIfCancelled();
            if (P?.Properties == null) return string.Empty;
            var props = P.Properties.Properties;
            return index >= 0 && index < props.Count ? props[index].Arguments : string.Empty;
        }

        public virtual int GetPropValue(string name)
        {
            _cancel.ThrowIfCancelled();
            if (P?.Properties == null || string.IsNullOrEmpty(name)) return 0;
            foreach (var prop in P.Properties.Properties)
            {
                string text = prop.Arguments;
                if (!text.Contains(name, StringComparison.OrdinalIgnoreCase)) continue;
                // Estrai numero dalla stringa (es. "Lower Reagent Cost: 10" → 10)
                var match = System.Text.RegularExpressions.Regex.Match(text, @"[-+]?\d+");
                if (match.Success && int.TryParse(match.Value, out int val)) return val;
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// Scansiona tutti gli item equipaggiati e somma il valore di una proprietà OPL.
        /// </summary>
        public virtual float SumAttribute(string attributeName)
        {
            _cancel.ThrowIfCancelled();
            if (P == null || string.IsNullOrEmpty(attributeName)) return 0;
            float total = 0;
            byte[] equipLayers = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                                   0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
                                   0x11, 0x12, 0x13, 0x15, 0x16 };
            foreach (byte layer in equipLayers)
            {
                var item = FindLayer(layer);
                if (item?.Properties == null) continue;
                foreach (var prop in item.Properties.Properties)
                {
                    string text = prop.Arguments;
                    if (!text.Contains(attributeName, StringComparison.OrdinalIgnoreCase)) continue;
                    var match = System.Text.RegularExpressions.Regex.Match(text, @"[-+]?\d+");
                    if (match.Success && float.TryParse(match.Value, out float val)) total += val;
                    else total += 1;
                    break;
                }
            }
            return total;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Targeting
        // ──────────────────────────────────────────────────────────────────────
        public virtual Item? GetItemOnLayer(string layerName, uint mobileSerial)
        {
            _cancel.ThrowIfCancelled();
            if (!Enum.TryParse<Layer>(layerName, true, out var layer)) return null;
            return _world.Items.FirstOrDefault(i => i.Container == mobileSerial && i.Layer == (byte)layer);
        }

        public virtual uint GetLayer(string layerName) => GetItemOnLayer(layerName)?.Serial ?? 0;

        // ──────────────────────────────────────────────────────────────────────
        // Skill helpers (dict)
        // ──────────────────────────────────────────────────────────────────────
        public virtual Dictionary<string, double> GetAllSkills()
        {
            _cancel.ThrowIfCancelled();
            var dict = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in _skills.Skills) dict[s.Name] = s.Value;
            return dict;
        }
    }
}
