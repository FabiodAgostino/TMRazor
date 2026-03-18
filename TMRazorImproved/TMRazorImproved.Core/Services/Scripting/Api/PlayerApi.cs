using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMRazorImproved.Core.Utilities;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class Point3D
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        public Point3D(int x, int y, int z) { X = x; Y = y; Z = z; }
        public override string ToString() => $"({X}, {Y}, {Z})";
    }

    public class BuffInfo
    {
        public string Name { get; }
        public int Remaining { get; }
        public BuffInfo(string name, int remaining) { Name = name; Remaining = remaining; }
    }

    public class PlayerApi
    {
        private readonly IWorldService _world;
        private readonly IPacketService _packet;
        private readonly ITargetingService _targeting;
        private readonly ISkillsService _skills;
        private readonly IPathFindingService? _pathfinding;
        private readonly ScriptCancellationController _cancel;
        private readonly ILogger<PlayerApi>? _logger;
        private readonly IConfigService? _config;

        public PlayerApi(
            IWorldService world,
            IPacketService packet,
            ITargetingService targeting,
            ISkillsService skills,
            ScriptCancellationController cancel,
            IPathFindingService? pathfinding = null,
            ILogger<PlayerApi>? logger = null,
            IConfigService? config = null)
        {
            _world = world;
            _packet = packet;
            _targeting = targeting;
            _skills = skills;
            _cancel = cancel;
            _pathfinding = pathfinding;
            _logger = logger;
            _config = config;
        }

        private Mobile? P => _world.Player;
        private ScriptMobile? Wrap(Mobile? m) => m == null ? null : new ScriptMobile(m, _world, _packet, _targeting);
        private ScriptItem? Wrap(Item? i) => i == null ? null : new ScriptItem(i, _world, _packet, _targeting);

        public virtual int Hits    => P?.Hits    ?? -1;
        public virtual int HitsMax => P?.HitsMax ?? -1;
        public virtual int Mana    => P?.Mana    ?? -1;
        public virtual int ManaMax => P?.ManaMax ?? -1;
        public virtual int Stam    => P?.Stam    ?? -1;
        public virtual int StamMax => P?.StamMax ?? -1;

        public virtual int Str => P?.Str ?? 0;
        public virtual int Dex => P?.Dex ?? 0;
        public virtual int Int => P?.Int ?? 0;
        public virtual int StatCap => P?.StatCap ?? 0;

        public virtual int FireResist    => P?.FireResist    ?? 0;
        public virtual int ColdResist    => P?.ColdResist    ?? 0;
        public virtual int PoisonResist  => P?.PoisonResist  ?? 0;
        public virtual int EnergyResist  => P?.EnergyResist  ?? 0;
        public virtual int FireResistance   => FireResist;
        public virtual int ColdResistance   => ColdResist;
        public virtual int PoisonResistance => PoisonResist;
        public virtual int EnergyResistance => EnergyResist;
        public virtual int AR => P?.AR ?? 0;

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

        public virtual int    Karma      => P?.Karma      ?? 0;
        public virtual int    Fame       => P?.Fame       ?? 0;
        public virtual string KarmaTitle => P?.KarmaTitle ?? string.Empty;

        public virtual bool UpdateKarma()
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return false;
            int oldKarma = P.Karma;
            _packet.SendToServer(PacketBuilder.RequestProfile(P.Serial));
            var deadline = Environment.TickCount64 + 2000;
            while (Environment.TickCount64 < deadline)
            {
                _cancel.ThrowIfCancelled();
                if (P.Karma != oldKarma) return true;
                Thread.Sleep(50);
            }
            return true;
        }

        public virtual bool HasSpecial          => (P?.PrimaryAbilityActive   ?? false) || (P?.SecondaryAbilityActive ?? false);
        public virtual bool HasPrimarySpecial   => P?.PrimaryAbilityActive   ?? false;
        public virtual bool HasSecondarySpecial => P?.SecondaryAbilityActive ?? false;
        public virtual int  PrimarySpecialId    => P?.PrimaryAbilityId       ?? 0;
        public virtual int  SecondarySpecialId  => P?.SecondaryAbilityId     ?? 0;
        public virtual bool InParty => _world.PartyMembers.Count > 0;

        public virtual List<ScriptMobile> Pets
        {
            get
            {
                _cancel.ThrowIfCancelled();
                if (P == null) return new List<ScriptMobile>();
                lock (P.Pets)
                {
                    return P.Pets
                        .Select(serial => _world.FindMobile(serial))
                        .Where(m => m != null)
                        .Select(m => new ScriptMobile(m!, _world, _packet, _targeting))
                        .ToList();
                }
            }
        }

        public virtual uint StaticMount
        {
            get => _config?.CurrentProfile?.RemountSerial ?? 0;
            set { if (_config?.CurrentProfile != null) _config.CurrentProfile.RemountSerial = value; }
        }

        public virtual uint   Serial   => P?.Serial  ?? 0;
        public virtual string Name     => P?.Name    ?? string.Empty;
        public virtual int    Graphic  => P?.Graphic ?? 0;
        public virtual int    Hue      => P?.Hue     ?? 0;
        public virtual int    Body     => Graphic;
        public virtual int    MobileID => Graphic;

        public virtual bool IsConnected  => P != null;
        public virtual bool Connected    => IsConnected;
        public virtual bool IsPoisoned   => P?.IsPoisoned   ?? false;
        public virtual bool Poisoned     => IsPoisoned;
        public virtual bool IsYellowHits => P?.IsYellowHits ?? false;
        public virtual bool YellowHits   => IsYellowHits;
        public virtual bool IsHidden     => P?.IsHidden     ?? false;
        public virtual bool Visible      => !IsHidden;
        public virtual bool WarMode      => P?.WarMode      ?? false;
        public virtual bool IsGhost      => P?.IsGhost      ?? false;
        public virtual bool Paralized    => P?.Paralyzed    ?? false;
        public virtual bool Female       => P?.Female       ?? false;

        public virtual void TrackingArrow(int x, int y, bool display, uint target = 0)
        {
            _cancel.ThrowIfCancelled();
            if (target == 0) target = Serial;
            byte[] pkt = { 0xBA, 0, 0, 0, 0, 0, 0, 0, 0 };
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(1), target);
            pkt[5] = (byte)(display ? 0x01 : 0x00);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(6), (ushort)x);
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(pkt.AsSpan(8), (ushort)y);
            _packet.SendToClient(pkt);
        }

        public virtual int X     => P?.X     ?? 0;
        public virtual int Y     => P?.Y     ?? 0;
        public virtual int Z     => P?.Z     ?? 0;
        public virtual int MapId => P?.MapId ?? 0;
        public virtual int Map   => MapId;
        public virtual Point3D Position => new(X, Y, Z);

        public virtual string Direction
        {
            get
            {
                if (P == null) return "Undefined";
                return (P.Direction & 0x07) switch
                {
                    0 => "North", 1 => "Right", 2 => "East", 3 => "Down",
                    4 => "South", 5 => "Left", 6 => "West", 7 => "Up",
                    _ => "Undefined"
                };
            }
        }

        public virtual int DirectionNum => P?.Direction ?? 0;

        private static bool _regionsLoaded = false;
        private static readonly Dictionary<int, List<(int X, int Y, int W, int H, string Name, string Zone)>> _mapRegions = new();
        private static readonly object _regionsLock = new();

        private static void LoadRegions()
        {
            if (_regionsLoaded) return;
            lock (_regionsLock)
            {
                if (_regionsLoaded) return;
                try
                {
                    var path = System.IO.Path.Combine(AppContext.BaseDirectory, "Config", "regions.json");
                    if (System.IO.File.Exists(path))
                    {
                        var json = System.IO.File.ReadAllText(path);
                        using var doc = System.Text.Json.JsonDocument.Parse(json);

                        string[] maps = { "Felucca", "Trammel", "Ilshenar", "Malas", "Tokuno", "TerMur" };
                        for (int i = 0; i < maps.Length; i++)
                        {
                            if (doc.RootElement.TryGetProperty(maps[i], out var mapElem))
                            {
                                var regions = new List<(int X, int Y, int W, int H, string Name, string Zone)>();
                                _mapRegions[i] = regions;

                                string[] zones = { "Towns", "Dungeons", "Guarded", "Forest" };
                                foreach (var zoneName in zones)
                                {
                                    if (mapElem.TryGetProperty(zoneName, out var zoneElem) && zoneElem.ValueKind == System.Text.Json.JsonValueKind.Object)
                                    {
                                        foreach (var areaProp in zoneElem.EnumerateObject())
                                        {
                                            string areaName = areaProp.Name;
                                            if (areaProp.Value.ValueKind == System.Text.Json.JsonValueKind.Array)
                                            {
                                                foreach (var rectElem in areaProp.Value.EnumerateArray())
                                                {
                                                    if (rectElem.ValueKind == System.Text.Json.JsonValueKind.Object)
                                                    {
                                                        int x = int.Parse(rectElem.GetProperty("X").GetString() ?? "0");
                                                        int y = int.Parse(rectElem.GetProperty("Y").GetString() ?? "0");
                                                        int w = int.Parse(rectElem.GetProperty("Width").GetString() ?? "0");
                                                        int h = int.Parse(rectElem.GetProperty("Height").GetString() ?? "0");
                                                        regions.Add((x, y, w, h, areaName, zoneName));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore loading errors for fallback
                }
                finally
                {
                    _regionsLoaded = true;
                }
            }
        }

        private (string Area, string Zone) GetCurrentRegion()
        {
            LoadRegions();
            var p = P;
            if (p == null) return ("Unknown", "Unknown");

            int map = p.MapId;
            if (_mapRegions.TryGetValue(map, out var regions))
            {
                foreach (var rect in regions)
                {
                    if (p.X >= rect.X && p.X <= rect.X + rect.W &&
                        p.Y >= rect.Y && p.Y <= rect.Y + rect.H)
                    {
                        return (rect.Name, rect.Zone);
                    }
                }
            }

            return ("Unknown", "Unknown");
        }

        public virtual string Area() => GetCurrentRegion().Area;
        public virtual string Zone() => GetCurrentRegion().Zone;

        public virtual int Notoriety => P?.Notoriety ?? 0;

        public virtual int DistanceTo(uint serial)
        {
            var entity = _world.FindEntity(serial);
            return entity != null ? (int)Math.Max(Math.Abs(X - entity.X), Math.Abs(Y - entity.Y)) : -1;
        }

        public virtual int DistanceTo(ScriptMobile mobile) => DistanceTo(mobile.Serial);
        public virtual int DistanceTo(ScriptItem item) => DistanceTo(item.Serial);

        public virtual bool InRange(uint serial, int range)
        {
            int dist = DistanceTo(serial);
            return dist != -1 && dist <= range;
        }

        public virtual bool InRangeMobile(uint serial, int range) => InRange(serial, range);
        public virtual bool InRangeItem(uint serial, int range) => InRange(serial, range);

        public virtual double HitsPct => HitsMax > 0 ? (double)Hits / HitsMax * 100 : 0;
        public virtual double ManaPct  => ManaMax > 0 ? (double)Mana / ManaMax * 100 : 0;
        public virtual double StamPct  => StamMax > 0 ? (double)Stam / StamMax * 100 : 0;

        public virtual ScriptItem? Backpack => Wrap(P?.Backpack);
        public virtual ScriptItem? Bank => FindLayer(0x1D);
        public virtual ScriptItem? Quiver => FindLayer(0x16);
        public virtual ScriptItem? Mount => FindLayer(0x19);

        public virtual bool IsOnMount
        {
            get
            {
                _cancel.ThrowIfCancelled();
                return P != null && _world.Items.Any(i => i.Container == P.Serial && i.Layer == 0x19);
            }
        }

        public virtual ScriptItem? GetItemOnLayer(string layerName)
        {
            _cancel.ThrowIfCancelled();
            if (P == null || !Enum.TryParse<Layer>(layerName, true, out var layer)) return null;
            return Wrap(_world.Items.FirstOrDefault(i => i.Container == P.Serial && i.Layer == (byte)layer));
        }

        public virtual ScriptItem? FindLayer(byte layer)
        {
            _cancel.ThrowIfCancelled();
            return P == null ? null : Wrap(_world.Items.FirstOrDefault(i => i.Container == P.Serial && i.Layer == layer));
        }

        public virtual uint GetLayer(byte layer) => FindLayer(layer)?.Serial ?? 0;
        public virtual bool CheckLayer(string layerName) => GetItemOnLayer(layerName) != null;

        public virtual void Chat(string message, int hue = 0) => SendSpeech(message, 0x00, hue == 0 ? 0x0034 : hue);
        public virtual void ChatSay(string message, int hue = 0x0451) => SendSpeech(message, 0x00, hue);
        public virtual void ChatWhisper(string message)  => SendSpeech(message, 0x02, 0x3B2);
        public virtual void ChatYell(string message)     => SendSpeech(message, 0x04, 0x021);
        public virtual void ChatEmote(string message)    => SendSpeech(message, 0x03, 0x024);
        public virtual void ChatGuild(string message)    => SendSpeech(message, 0x0D, 0x044);
        public virtual void ChatAlliance(string message) => SendSpeech(message, 0x0E, 0x057);
        public virtual void ChatChannel(string message)  => SendSpeech(message, 0x0C, 0x034);
        public virtual void MapSay(string message)       => ChatSay(message);

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

        public virtual void HeadMessage(int color, string msg) => HeadMsg(msg, color);

        public virtual void Attack(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(PacketBuilder.Attack(serial));
        }

        public virtual void AttackLast()
        {
            _cancel.ThrowIfCancelled();
            if (_targeting.LastTarget != 0) _packet.SendToServer(PacketBuilder.Attack(_targeting.LastTarget));
        }

        public virtual void ChatParty(string message, uint recipientSerial = 0)
        {
            _cancel.ThrowIfCancelled();
            if (string.IsNullOrEmpty(message)) return;
            var textBytes = System.Text.Encoding.BigEndianUnicode.GetBytes(message);
            if (recipientSerial != 0)
            {
                var pkt = new byte[10 + textBytes.Length + 2];
                pkt[0] = 0xBF; pkt[1] = (byte)(pkt.Length >> 8); pkt[2] = (byte)pkt.Length;
                pkt[3] = 0x00; pkt[4] = 0x06; pkt[5] = 0x02;
                pkt[6] = (byte)(recipientSerial >> 24); pkt[7] = (byte)(recipientSerial >> 16); pkt[8] = (byte)(recipientSerial >> 8); pkt[9] = (byte)recipientSerial;
                Array.Copy(textBytes, 0, pkt, 10, textBytes.Length);
                _packet.SendToServer(pkt);
            }
            else
            {
                var pkt = new byte[10 + textBytes.Length + 2];
                pkt[0] = 0xBF; pkt[1] = (byte)(pkt.Length >> 8); pkt[2] = (byte)pkt.Length;
                pkt[3] = 0x00; pkt[4] = 0x06; pkt[5] = 0x01;
                Array.Copy(textBytes, 0, pkt, 10, textBytes.Length);
                _packet.SendToServer(pkt);
            }
        }

        public virtual void PartyInvite()
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x06, 0x00, 0x06, 0x01 });
        }

        public virtual void PartyAccept(uint fromSerial = 0)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x0A, 0x00, 0x06, 0x02, (byte)(fromSerial >> 24), (byte)(fromSerial >> 16), (byte)(fromSerial >> 8), (byte)fromSerial });
        }

        public virtual void LeaveParty()
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x0A, 0x00, 0x06, 0x03, (byte)(Serial >> 24), (byte)(Serial >> 16), (byte)(Serial >> 8), (byte)Serial });
        }

        public virtual void Run(string direction)
        {
            _cancel.ThrowIfCancelled();
            if (Enum.TryParse<TMRazorImproved.Shared.Enums.Direction>(direction, true, out var dir))
            {
                byte d = (byte)((byte)dir | (byte)TMRazorImproved.Shared.Enums.Direction.Running);
                _packet.SendToServer(new byte[] { 0x02, d, 0x00 });
            }
        }

        public virtual void Walk(string direction)
        {
            _cancel.ThrowIfCancelled();
            if (Enum.TryParse<TMRazorImproved.Shared.Enums.Direction>(direction, true, out var dir))
                _packet.SendToServer(new byte[] { 0x02, (byte)dir, 0x00 });
        }

        public virtual void Turn(string direction)
        {
            _cancel.ThrowIfCancelled();
            if (Enum.TryParse<TMRazorImproved.Shared.Enums.Direction>(direction, true, out var dir))
            {
                var currentDir = P != null ? (TMRazorImproved.Shared.Enums.Direction)(P.Direction & 0x07) : TMRazorImproved.Shared.Enums.Direction.North;
                if (currentDir != dir)
                    _packet.SendToServer(new byte[] { 0x02, (byte)dir, 0x00 });
            }
        }

        public virtual void PathFindTo(int x, int y, int z)
        {
            _cancel.ThrowIfCancelled();
            // Invia al client il pacchetto 0x38 ripetuto per forzare il pathfinding alla coordinata
            byte[] packet = new byte[7 * 20];
            for (int i = 0; i < 20; i++)
            {
                packet[i * 7] = 0x38;
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(i * 7 + 1), (ushort)x);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(i * 7 + 3), (ushort)y);
                System.Buffers.Binary.BinaryPrimitives.WriteInt16BigEndian(packet.AsSpan(i * 7 + 5), (short)z);
            }
            _packet.SendToClient(packet);
        }

        public virtual void Fly(bool status)
        {
            _cancel.ThrowIfCancelled();
            if ((P?.Flying ?? false) != status) _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x05, 0x00, 0x32 });
        }

        public virtual void InvokeVirtue(string virtueName)
        {
            _cancel.ThrowIfCancelled();
            byte virtueId = virtueName.ToLowerInvariant() switch
            {
                "honor" => 1, "sacrifice" => 2, "valor" => 3, "compassion" => 4,
                "honesty" => 5, "humility" => 6, "justice" => 7, "spirituality" => 8, _ => 0
            };
            if (virtueId != 0) _packet.SendToServer(new byte[] { 0xB2, virtueId });
        }

        public virtual bool CheckBuffs(string buffName)
        {
            _cancel.ThrowIfCancelled();
            if (P == null || string.IsNullOrEmpty(buffName)) return false;
            if (P.ActiveBuffs.ContainsKey(buffName)) return true;
            return P.OPL?.Properties.Any(p => p.Arguments.Contains(buffName, StringComparison.OrdinalIgnoreCase)) ?? false;
        }

        public virtual bool HasBuff(string buffName) => CheckBuffs(buffName);
        public virtual bool BuffsExist(string buffName) => CheckBuffs(buffName);

        public virtual void AttackType(string type)
        {
            _cancel.ThrowIfCancelled();
            byte val = type.ToLowerInvariant() switch { "grapple" => 1, "disarm" => 2, _ => 0 };
            if (val != 0) _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x05, 0x00, val });
        }

        public virtual void WeaponPrimarySA() => SetAbility("primary");
        public virtual void WeaponSecondarySA() => SetAbility("secondary");
        public virtual void WeaponClearSA() => SetAbility("clear");
        public virtual void WeaponDisarmSA() => AttackType("disarm");
        public virtual void WeaponStunSA() => AttackType("grapple");

        public virtual void EquipLastWeapon()
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x06, 0x00, 0x16 });
        }

        public virtual void ToggleAlwaysRun()
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x05, 0x00, 0x24 });
        }

        public virtual void GuildButton()
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x05, 0x00, 0x02 });
        }

        public virtual void QuestButton()
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x05, 0x00, 0x01 });
        }

        public virtual void ClearCorpseList() { _cancel.ThrowIfCancelled(); }

        public virtual void EquipUO3D(List<uint> serials)
        {
            _cancel.ThrowIfCancelled();
            foreach (var serial in serials) EquipItem(serial);
        }

        public virtual void UnEquipUO3D(List<string> layers)
        {
            _cancel.ThrowIfCancelled();
            foreach (var layer in layers) UnEquipItemByLayer(layer);
        }

        public virtual void PartyCanLoot(bool canLoot)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x06, 0x00, 0x06, (byte)(canLoot ? 0x05 : 0x06) });
        }

        public virtual void KickMember(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x0A, 0x00, 0x06, 0x04, (byte)(serial >> 24), (byte)(serial >> 16), (byte)(serial >> 8), (byte)serial });
        }

        public virtual void SetStaticMount(uint serial) => StaticMount = serial;
        public virtual bool SpellIsEnabled(string spellName)
        {
            _cancel.ThrowIfCancelled();
            var disabled = _config?.CurrentProfile?.DisabledSpells;
            if (disabled == null || disabled.Count == 0) return true;
            return !disabled.Contains(spellName, StringComparer.OrdinalIgnoreCase);
        }

        public virtual int SumAttribute(string name)
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return 0;
            return _world.Items.Where(i => i.Container == P.Serial).Sum(item => Wrap(item)?.GetPropValue(name) ?? 0);
        }

        public virtual void UseSkillOnly(string skillName, bool wait = true)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
            if (skill != null) _packet.SendToServer(PacketBuilder.UseSkill(skill.ID));
        }

        public virtual void HashSet(string key, object value) { }

        public virtual List<string> Buffs
        {
            get
            {
                _cancel.ThrowIfCancelled();
                return P?.ActiveBuffs.Keys.ToList() ?? new List<string>();
            }
        }

        public virtual List<BuffInfo> BuffsInfo
        {
            get
            {
                _cancel.ThrowIfCancelled();
                return P?.ActiveBuffs.Select(kv => new BuffInfo(kv.Key, kv.Value)).ToList() ?? new List<BuffInfo>();
            }
        }

        public virtual BuffInfo? GetBuffInfo(string buffName)
        {
            _cancel.ThrowIfCancelled();
            if (P == null || string.IsNullOrEmpty(buffName)) return null;
            if (P.ActiveBuffs.TryGetValue(buffName, out int remaining)) return new BuffInfo(buffName, remaining);
            var match = P.ActiveBuffs.FirstOrDefault(kv => kv.Key.Contains(buffName, StringComparison.OrdinalIgnoreCase));
            return match.Key != null ? new BuffInfo(match.Key, match.Value) : null;
        }

        public virtual int BuffTime(string buffName)
        {
            _cancel.ThrowIfCancelled();
            return (P != null && P.ActiveBuffs.TryGetValue(buffName, out int v)) ? v : 0;
        }

        public virtual void BandageSelf() => Bandage(Serial);
        public virtual void Bandage(uint targetSerial)
        {
            _cancel.ThrowIfCancelled();
            var bp = P?.Backpack;
            if (bp == null) return;
            var bandage = _world.Items.FirstOrDefault(i => i.Container == bp.Serial && i.Graphic == 0x0E21);
            if (bandage != null) _packet.SendToServer(PacketBuilder.DoubleClick(bandage.Serial));
        }

        public virtual void OpenBackpack() => OpenContainer(P?.Backpack?.Serial ?? 0);
        public virtual void OpenContainer(uint serial)
        {
            _cancel.ThrowIfCancelled();
            if (serial != 0) _packet.SendToServer(PacketBuilder.DoubleClick(serial));
        }

        public virtual void OpenPaperDoll()
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return;
            byte[] pkt = { 0xBF, 0x00, 0x09, 0x00, 0x0F, 0, 0, 0, 0 };
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5), P.Serial);
            _packet.SendToServer(pkt);
        }

        public virtual void Resync()
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0x22, 0xFF, 0x00 });
        }

        public virtual void ToggleWarMode()
        {
            _cancel.ThrowIfCancelled();
            bool current = P?.WarMode ?? false;
            _packet.SendToServer(new byte[] { 0x72, (byte)(current ? 0x00 : 0x01), 0x00, 0x32, 0x00 });
        }

        public virtual void SetWarMode(bool warMode)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0x72, (byte)(warMode ? 0x01 : 0x00), 0x00, 0x32, 0x00 });
        }

        public virtual void UnEquipItemByLayer(string layerName, bool wait = true)
        {
            _cancel.ThrowIfCancelled();
            if (!Enum.TryParse<Layer>(layerName, true, out var layer)) return;
            var item = _world.Items.FirstOrDefault(i => i.Container == Serial && i.Layer == (byte)layer);
            if (item == null || Backpack == null) return;
            _packet.SendToServer(PacketBuilder.LiftItem(item.Serial, item.Amount));
            _packet.SendToServer(PacketBuilder.DropToContainer(item.Serial, Backpack.Serial));
        }

        public virtual void EquipItem(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var item = _world.FindItem(serial);
            if (item == null) return;
            _packet.SendToServer(PacketBuilder.LiftItem(item.Serial, item.Amount));
            _packet.SendToServer(PacketBuilder.WearItem(item.Serial, item.Layer, Serial));
        }

        public virtual void EquipItem(ScriptItem item) => EquipItem(item.Serial);

        public virtual double GetSkillValue(string skillName)
        {
            _cancel.ThrowIfCancelled();
            return _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase))?.Value ?? 0;
        }

        public virtual double GetRealSkillValue(string skillName)
        {
            _cancel.ThrowIfCancelled();
            return _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase))?.BaseValue ?? 0;
        }

        public virtual double GetSkillCap(string skillName)
        {
            _cancel.ThrowIfCancelled();
            return _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase))?.Cap ?? 0;
        }

        public virtual int GetSkillStatus(string skillName)
        {
            _cancel.ThrowIfCancelled();
            return (int)(_skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase))?.Lock ?? 0);
        }

        public virtual void SetSkillStatus(string skillName, int status)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
            if (skill != null)
            {
                _packet.SendToServer(PacketBuilder.SetSkillLock(skill.ID, (byte)status));
                skill.Lock = (TMRazorImproved.Shared.Models.SkillLock)status;
            }
        }

        public virtual int GetStatStatus(string statName)
        {
            _cancel.ThrowIfCancelled();
            return statName.ToLowerInvariant() switch
            {
                "strength"     => (int)(P?.StrLock ?? 0),
                "dexterity"    => (int)(P?.DexLock ?? 0),
                "intelligence" => (int)(P?.IntLock ?? 0),
                _              => 0
            };
        }

        public virtual void SetStatStatus(string statName, int status)
        {
            _cancel.ThrowIfCancelled();
            byte statId = statName.ToLowerInvariant() switch { "strength" => 0, "dexterity" => 1, "intelligence" => 2, _ => 255 };
            if (statId != 255)
            {
                _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x07, 0x00, 0x19, 0x02, (byte)statId, (byte)status });
                if (P != null) {
                    if (statId == 0) P.StrLock = (byte)status;
                    else if (statId == 1) P.DexLock = (byte)status;
                    else if (statId == 2) P.IntLock = (byte)status;
                }
            }
        }

        public virtual void TargetSelf() => _targeting.TargetSelf();

        public virtual void UseSkill(string skillName, uint targetSerial = 0)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
            if (skill != null) { _packet.SendToServer(PacketBuilder.UseSkill(skill.ID)); _targeting.SendTarget(targetSerial); }
        }

        public virtual void SetAbility(string abilityName)
        {
            _cancel.ThrowIfCancelled();
            int ability = abilityName?.ToLowerInvariant() switch
            {
                "primary" => 1, "secondary" => 2, "clear" => 0,
                _ => int.TryParse(abilityName, out int n) ? n : 0
            };
            
            // Format for The Miracle shard (D7)
            byte[] pkt = new byte[9];
            pkt[0] = 0xD7;
            pkt[1] = 0x00; pkt[2] = 0x09;
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), Serial);
            pkt[7] = 0x00; pkt[8] = (byte)ability;
            _packet.SendToServer(pkt);
        }

        public virtual void Cast(string spellName)
        {
            _cancel.ThrowIfCancelled();
            if (SpellsApi.TryGetSpellId(spellName, out int id)) _packet.SendToServer(PacketBuilder.CastSpell(id));
        }

        public virtual List<string> GetPropStringList() => P?.Properties ?? new List<string>();
        public virtual string GetPropStringByIndex(int index) => (P != null && index >= 0 && index < P.Properties.Count) ? P.Properties[index] : string.Empty;

        public virtual int GetPropValue(string name)
        {
            _cancel.ThrowIfCancelled();
            if (P?.OPL == null || string.IsNullOrEmpty(name)) return 0;
            foreach (var prop in P.OPL.Properties)
            {
                string text = prop.Arguments;
                if (!text.Contains(name, StringComparison.OrdinalIgnoreCase)) continue;
                var match = System.Text.RegularExpressions.Regex.Match(text, @"[-+]?\d+");
                if (match.Success && int.TryParse(match.Value, out int val)) return val;
                return 1;
            }
            return 0;
        }
    }
}
