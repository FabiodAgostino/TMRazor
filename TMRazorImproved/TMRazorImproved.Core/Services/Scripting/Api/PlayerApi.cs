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
    /// <summary>Represents a three-dimensional coordinate in the Ultima Online world.</summary>
    public class Point3D
    {
        /// <summary>The X (east-west) coordinate.</summary>
        public int X { get; }
        /// <summary>The Y (north-south) coordinate.</summary>
        public int Y { get; }
        /// <summary>The Z (altitude) coordinate.</summary>
        public int Z { get; }
        public Point3D(int x, int y, int z) { X = x; Y = y; Z = z; }
        public override string ToString() => $"({X}, {Y}, {Z})";
    }

    /// <summary>Holds information about a single active buff or debuff on the player character.</summary>
    public class BuffInfo
    {
        /// <summary>The display name of the buff or debuff.</summary>
        public string Name { get; }
        /// <summary>The remaining duration of the buff in milliseconds, or 0 if permanent/unknown.</summary>
        public int Remaining { get; }
        public BuffInfo(string name, int remaining) { Name = name; Remaining = remaining; }
    }

    /// <summary>
    /// Provides access to the player character's attributes, position, equipment, and actions.
    /// Exposed to scripts as the <c>Player</c> variable.
    /// </summary>
    public class PlayerApi
    {
        private readonly IWorldService _world;
        private readonly IPacketService _packet;
        private readonly ITargetingService _targeting;
        private readonly ISkillsService _skills;
        private readonly IPathFindingService? _pathfinding;
        private readonly IClientInteropService _interop;
        private readonly ScriptCancellationController _cancel;
        private readonly ILogger<PlayerApi>? _logger;
        private readonly IConfigService? _config;

        public PlayerApi(
            IWorldService world,
            IPacketService packet,
            ITargetingService targeting,
            ISkillsService skills,
            ScriptCancellationController cancel,
            IClientInteropService interop,
            IPathFindingService? pathfinding = null,
            ILogger<PlayerApi>? logger = null,
            IConfigService? config = null)
        {
            _world = world;
            _packet = packet;
            _targeting = targeting;
            _skills = skills;
            _cancel = cancel;
            _interop = interop;
            _pathfinding = pathfinding;
            _logger = logger;
            _config = config;
        }

        private Mobile? P => _world.Player;
        private ScriptMobile? Wrap(Mobile? m) => m == null ? null : new ScriptMobile(m, _world, _packet, _targeting);
        private ScriptItem? Wrap(Item? i) => i == null ? null : new ScriptItem(i, _world, _packet, _targeting);

        /// <summary>Current hit points. Returns -1 if not connected.</summary>
        public virtual int Hits    => P?.Hits    ?? -1;
        /// <summary>Maximum hit points. Returns -1 if not connected.</summary>
        public virtual int HitsMax => P?.HitsMax ?? -1;
        /// <summary>Current mana. Returns -1 if not connected.</summary>
        public virtual int Mana    => P?.Mana    ?? -1;
        /// <summary>Maximum mana. Returns -1 if not connected.</summary>
        public virtual int ManaMax => P?.ManaMax ?? -1;
        /// <summary>Current stamina. Returns -1 if not connected.</summary>
        public virtual int Stam    => P?.Stam    ?? -1;
        /// <summary>Maximum stamina. Returns -1 if not connected.</summary>
        public virtual int StamMax => P?.StamMax ?? -1;

        /// <summary>Base Strength attribute of the player.</summary>
        public virtual int Str => P?.Str ?? 0;
        /// <summary>Base Dexterity attribute of the player.</summary>
        public virtual int Dex => P?.Dex ?? 0;
        /// <summary>Base Intelligence attribute of the player.</summary>
        public virtual int Int => P?.Int ?? 0;
        /// <summary>Total stat cap (maximum combined stats) of the player.</summary>
        public virtual int StatCap => P?.StatCap ?? 0;

        /// <summary>Player's fire resistance value.</summary>
        public virtual int FireResist    => P?.FireResist    ?? 0;
        /// <summary>Player's cold resistance value.</summary>
        public virtual int ColdResist    => P?.ColdResist    ?? 0;
        /// <summary>Player's poison resistance value.</summary>
        public virtual int PoisonResist  => P?.PoisonResist  ?? 0;
        /// <summary>Player's energy resistance value.</summary>
        public virtual int EnergyResist  => P?.EnergyResist  ?? 0;
        /// <summary>Alias for <see cref="FireResist"/>.</summary>
        public virtual int FireResistance   => FireResist;
        /// <summary>Alias for <see cref="ColdResist"/>.</summary>
        public virtual int ColdResistance   => ColdResist;
        /// <summary>Alias for <see cref="PoisonResist"/>.</summary>
        public virtual int PoisonResistance => PoisonResist;
        /// <summary>Alias for <see cref="EnergyResist"/>.</summary>
        public virtual int EnergyResistance => EnergyResist;
        /// <summary>Player's physical armor rating.</summary>
        public virtual int AR => P?.AR ?? 0;

        /// <summary>Hit Chance Increase bonus from equipment and buffs.</summary>
        public virtual int HitChanceIncrease       => P?.HitChanceIncrease       ?? 0;
        /// <summary>Swing Speed Increase bonus, reducing weapon swing delay.</summary>
        public virtual int SwingSpeedIncrease       => P?.SwingSpeedIncrease       ?? 0;
        /// <summary>Damage Chance Increase bonus for additional damage rolls.</summary>
        public virtual int DamageChanceIncrease     => P?.DamageChanceIncrease     ?? 0;
        /// <summary>Lower Reagent Cost percentage, reducing or eliminating reagent consumption when casting.</summary>
        public virtual int LowerReagentCost         => P?.LowerReagentCost         ?? 0;
        /// <summary>Hit Points Regeneration rate from equipment and skills.</summary>
        public virtual int HitPointsRegeneration    => P?.HitPointsRegeneration    ?? 0;
        /// <summary>Stamina Regeneration rate from equipment and skills.</summary>
        public virtual int StaminaRegeneration      => P?.StaminaRegeneration      ?? 0;
        /// <summary>Mana Regeneration rate from equipment and skills.</summary>
        public virtual int ManaRegeneration         => P?.ManaRegeneration         ?? 0;
        /// <summary>Reflect Physical Damage percentage reflected back to attacker.</summary>
        public virtual int ReflectPhysicalDamage    => P?.ReflectPhysicalDamage    ?? 0;
        /// <summary>Enhance Potions percentage bonus applied to potion effects.</summary>
        public virtual int EnhancePotions           => P?.EnhancePotions           ?? 0;
        /// <summary>Defense Chance Increase bonus for blocking incoming attacks.</summary>
        public virtual int DefenseChanceIncrease    => P?.DefenseChanceIncrease    ?? 0;
        /// <summary>Spell Damage Increase percentage bonus applied to spell damage.</summary>
        public virtual int SpellDamageIncrease      => P?.SpellDamageIncrease      ?? 0;
        /// <summary>Faster Cast Recovery bonus reducing post-cast delay.</summary>
        public virtual int FasterCastRecovery       => P?.FasterCastRecovery       ?? 0;
        /// <summary>Faster Casting bonus reducing spell cast time.</summary>
        public virtual int FasterCasting            => P?.FasterCasting            ?? 0;
        /// <summary>Lower Mana Cost percentage reduction on spell mana usage.</summary>
        public virtual int LowerManaCost            => P?.LowerManaCost            ?? 0;
        /// <summary>Strength Increase bonus from equipment or buffs.</summary>
        public virtual int StrengthIncrease         => P?.StrengthIncrease         ?? 0;
        /// <summary>Dexterity Increase bonus from equipment or buffs.</summary>
        public virtual int DexterityIncrease        => P?.DexterityIncrease        ?? 0;
        /// <summary>Intelligence Increase bonus from equipment or buffs.</summary>
        public virtual int IntelligenceIncrease     => P?.IntelligenceIncrease     ?? 0;
        /// <summary>Hit Points Increase bonus raising maximum HP.</summary>
        public virtual int HitPointsIncrease        => P?.HitPointsIncrease        ?? 0;
        /// <summary>Stamina Increase bonus raising maximum stamina.</summary>
        public virtual int StaminaIncrease          => P?.StaminaIncrease          ?? 0;
        /// <summary>Mana Increase bonus raising maximum mana.</summary>
        public virtual int ManaIncrease             => P?.ManaIncrease             ?? 0;
        /// <summary>Maximum Hit Points Increase from item properties.</summary>
        public virtual int MaximumHitPointsIncrease => P?.MaximumHitPointsIncrease ?? 0;
        /// <summary>Maximum Stamina Increase from item properties.</summary>
        public virtual int MaximumStaminaIncrease   => P?.MaximumStaminaIncrease   ?? 0;

        /// <summary>Minimum weapon damage value.</summary>
        public virtual int MinDamage    => P?.MinDamage    ?? 0;
        /// <summary>Maximum weapon damage value.</summary>
        public virtual int MaxDamage    => P?.MaxDamage    ?? 0;
        /// <summary>Player's luck stat, influencing loot quality and other random outcomes.</summary>
        public virtual int Luck         => P?.Luck         ?? 0;
        /// <summary>Tithe points used by the Chivalry skill.</summary>
        public virtual int Tithe        => P?.Tithe        ?? 0;
        /// <summary>Current number of followers (pets/summoned creatures).</summary>
        public virtual int Followers    => P?.Followers    ?? 0;
        /// <summary>Maximum allowed number of followers.</summary>
        public virtual int FollowersMax => P?.FollowersMax ?? 0;
        /// <summary>Current total weight carried by the player.</summary>
        public virtual int Weight       => P?.Weight       ?? 0;
        /// <summary>Maximum weight the player can carry before becoming overloaded.</summary>
        public virtual int MaxWeight    => P?.MaxWeight    ?? 0;
        /// <summary>Amount of gold in the player's backpack.</summary>
        public virtual int Gold         => P?.Gold         ?? 0;
        /// <summary>Player's armor rating (physical damage reduction).</summary>
        public virtual int Armor        => P?.Armor        ?? 0;

        /// <summary>Player's karma value; negative is murderer, positive is virtuous.</summary>
        public virtual int    Karma      => P?.Karma      ?? 0;
        /// <summary>Player's fame value representing renown in the world.</summary>
        public virtual int    Fame       => P?.Fame       ?? 0;
        /// <summary>Title string derived from the player's karma level.</summary>
        public virtual string KarmaTitle => P?.KarmaTitle ?? string.Empty;

        /// <summary>
        /// Requests an updated karma/fame value from the server and waits up to 2 seconds for it to change.
        /// </summary>
        /// <returns>True when the request was sent (karma may or may not have changed).</returns>
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

        /// <summary>True if either the primary or secondary weapon special ability is currently active.</summary>
        public virtual bool HasSpecial          => (P?.PrimaryAbilityActive   ?? false) || (P?.SecondaryAbilityActive ?? false);
        /// <summary>True if the primary weapon special ability is currently active.</summary>
        public virtual bool HasPrimarySpecial   => P?.PrimaryAbilityActive   ?? false;
        /// <summary>True if the secondary weapon special ability is currently active.</summary>
        public virtual bool HasSecondarySpecial => P?.SecondaryAbilityActive ?? false;
        /// <summary>The ID of the primary weapon special ability.</summary>
        public virtual int  PrimarySpecialId    => P?.PrimaryAbilityId       ?? 0;
        /// <summary>The ID of the secondary weapon special ability.</summary>
        public virtual int  SecondarySpecialId  => P?.SecondaryAbilityId     ?? 0;
        /// <summary>True if the player is currently in a party with at least one other member.</summary>
        public virtual bool InParty => _world.PartyMembers.Count > 0;

        /// <summary>Returns a list of all pets currently owned and tracked by the player.</summary>
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

        /// <summary>Gets or sets the serial of the mount used by the auto-remount feature.</summary>
        public virtual uint StaticMount
        {
            get => _config?.CurrentProfile?.RemountSerial ?? 0;
            set { if (_config?.CurrentProfile != null) _config.CurrentProfile.RemountSerial = value; }
        }

        /// <summary>The player's unique serial number used to identify them in packets.</summary>
        public virtual uint   Serial   => P?.Serial  ?? 0;
        /// <summary>The player character's name.</summary>
        public virtual string Name     => P?.Name    ?? string.Empty;
        /// <summary>The graphic (body) ID of the player's character model.</summary>
        public virtual int    Graphic  => P?.Graphic ?? 0;
        /// <summary>The hue (color) applied to the player's character model.</summary>
        public virtual int    Hue      => P?.Hue     ?? 0;
        /// <summary>Alias for <see cref="Graphic"/> — the body graphic ID.</summary>
        public virtual int    Body     => Graphic;
        /// <summary>Alias for <see cref="Graphic"/> — the mobile body ID.</summary>
        public virtual int    MobileID => Graphic;

        /// <summary>True if the player is currently connected to the server and in world.</summary>
        public virtual bool IsConnected  => P != null;
        /// <summary>Alias for <see cref="IsConnected"/>.</summary>
        public virtual bool Connected    => IsConnected;
        /// <summary>True if the player is currently poisoned.</summary>
        public virtual bool IsPoisoned   => P?.IsPoisoned   ?? false;
        /// <summary>Alias for <see cref="IsPoisoned"/>.</summary>
        public virtual bool Poisoned     => IsPoisoned;
        /// <summary>True if the player's HP bar is displayed in yellow (low HP warning or criminal status).</summary>
        public virtual bool IsYellowHits => P?.IsYellowHits ?? false;
        /// <summary>Alias for <see cref="IsYellowHits"/>.</summary>
        public virtual bool YellowHits   => IsYellowHits;
        /// <summary>True if the player is currently hidden (invisible to other players).</summary>
        public virtual bool IsHidden     => P?.IsHidden     ?? false;
        /// <summary>True if the player is visible (not hidden).</summary>
        public virtual bool Visible      => !IsHidden;
        /// <summary>True if the player is currently in war mode.</summary>
        public virtual bool WarMode      => P?.WarMode      ?? false;
        /// <summary>True if the player's character is in ghost/dead state.</summary>
        public virtual bool IsGhost      => P?.IsGhost      ?? false;
        /// <summary>True if the player is currently paralyzed and unable to move.</summary>
        public virtual bool Paralized    => P?.Paralyzed    ?? false;
        /// <summary>True if the player's character is female.</summary>
        public virtual bool Female       => P?.Female       ?? false;

        /// <summary>
        /// Sends a tracking arrow overlay to the UO client pointing at the given world coordinates.
        /// </summary>
        /// <param name="x">World X coordinate for the arrow target.</param>
        /// <param name="y">World Y coordinate for the arrow target.</param>
        /// <param name="display">True to show the arrow, false to hide it.</param>
        /// <param name="target">Optional serial of the entity being tracked; defaults to the player's own serial.</param>
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

        /// <summary>Player's current X (east-west) coordinate in the world.</summary>
        public virtual int X     => P?.X     ?? 0;
        /// <summary>Player's current Y (north-south) coordinate in the world.</summary>
        public virtual int Y     => P?.Y     ?? 0;
        /// <summary>Player's current Z (altitude) coordinate in the world.</summary>
        public virtual int Z     => P?.Z     ?? 0;
        /// <summary>The numeric ID of the current map facet (0=Felucca, 1=Trammel, etc.).</summary>
        public virtual int MapId => P?.MapId ?? 0;
        /// <summary>Alias for <see cref="MapId"/>.</summary>
        public virtual int Map   => MapId;
        /// <summary>Player's current 3D world position as a Point3D object.</summary>
        public virtual Point3D Position => new(X, Y, Z);

        /// <summary>Player's current facing direction as a string (e.g. "North", "East", "South", "West").</summary>
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

        /// <summary>Player's current facing direction as a raw numeric value (0=North, 2=East, 4=South, 6=West).</summary>
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

        /// <summary>Returns the name of the area (town, dungeon, etc.) the player is currently located in.</summary>
        /// <returns>Area name from regions.json, or "Unknown" if no region data is found.</returns>
        public virtual string Area() => GetCurrentRegion().Area;

        /// <summary>Returns the zone type of the player's current location (e.g., "Towns", "Dungeons", "Forest").</summary>
        /// <returns>Zone category string, or "Unknown" if no region data is found.</returns>
        public virtual string Zone() => GetCurrentRegion().Zone;

        /// <summary>Player's notoriety value (1=innocent/blue, 3=gray, 4=criminal, 5=enemy, 6=murderer/red).</summary>
        public virtual int Notoriety => P?.Notoriety ?? 0;

        /// <summary>
        /// Returns the Chebyshev (UO tile-based) distance from the player to the entity with the given serial.
        /// </summary>
        /// <param name="serial">The serial of the target entity.</param>
        /// <returns>Distance in tiles, or -1 if the entity is not found.</returns>
        public virtual int DistanceTo(uint serial)
        {
            var entity = _world.FindEntity(serial);
            return entity != null ? (int)Math.Max(Math.Abs(X - entity.X), Math.Abs(Y - entity.Y)) : -1;
        }

        /// <summary>Returns the tile distance from the player to the given mobile.</summary>
        public virtual int DistanceTo(ScriptMobile mobile) => DistanceTo(mobile.Serial);
        /// <summary>Returns the tile distance from the player to the given item.</summary>
        public virtual int DistanceTo(ScriptItem item) => DistanceTo(item.Serial);

        /// <summary>
        /// Returns true if the entity with the given serial is within the specified tile range of the player.
        /// </summary>
        /// <param name="serial">The serial of the entity to check.</param>
        /// <param name="range">Maximum allowable distance in tiles.</param>
        public virtual bool InRange(uint serial, int range)
        {
            int dist = DistanceTo(serial);
            return dist != -1 && dist <= range;
        }

        /// <summary>Returns true if the mobile with the given serial is within the specified tile range.</summary>
        public virtual bool InRangeMobile(uint serial, int range) => InRange(serial, range);
        /// <summary>Returns true if the item with the given serial is within the specified tile range.</summary>
        public virtual bool InRangeItem(uint serial, int range) => InRange(serial, range);

        /// <summary>Hit points as a percentage (0–100) of maximum.</summary>
        public virtual double HitsPct => HitsMax > 0 ? (double)Hits / HitsMax * 100 : 0;
        /// <summary>Mana as a percentage (0–100) of maximum.</summary>
        public virtual double ManaPct  => ManaMax > 0 ? (double)Mana / ManaMax * 100 : 0;
        /// <summary>Stamina as a percentage (0–100) of maximum.</summary>
        public virtual double StamPct  => StamMax > 0 ? (double)Stam / StamMax * 100 : 0;

        /// <summary>The player's backpack container item.</summary>
        public virtual ScriptItem? Backpack => Wrap(P?.Backpack);
        /// <summary>The player's bank box container (layer 0x1D).</summary>
        public virtual ScriptItem? Bank => FindLayer(0x1D);
        /// <summary>The quiver equipped by the player (layer 0x16), or null if not equipped.</summary>
        public virtual ScriptItem? Quiver => FindLayer(0x16);
        /// <summary>The mount item currently worn in the mount layer (layer 0x19), or null if not mounted.</summary>
        public virtual ScriptItem? Mount => FindLayer(0x19);

        /// <summary>True if the player is currently mounted (has an item in the mount layer).</summary>
        public virtual bool IsOnMount
        {
            get
            {
                _cancel.ThrowIfCancelled();
                return P != null && _world.Items.Any(i => i.Container == P.Serial && i.Layer == 0x19);
            }
        }

        /// <summary>
        /// Returns the item equipped in the named layer slot, or null if the slot is empty.
        /// </summary>
        /// <param name="layerName">Layer name as defined in the <c>Layer</c> enum (e.g., "OneHanded", "Neck").</param>
        public virtual ScriptItem? GetItemOnLayer(string layerName)
        {
            _cancel.ThrowIfCancelled();
            if (P == null || !Enum.TryParse<Layer>(layerName, true, out var layer)) return null;
            return Wrap(_world.Items.FirstOrDefault(i => i.Container == P.Serial && i.Layer == (byte)layer));
        }

        /// <summary>
        /// Returns the item currently occupying the given equipment layer, or null if the layer is empty.
        /// </summary>
        /// <param name="layer">Numeric layer ID (e.g., 0x19 for mount, 0x1D for bank).</param>
        public virtual ScriptItem? FindLayer(byte layer)
        {
            _cancel.ThrowIfCancelled();
            return P == null ? null : Wrap(_world.Items.FirstOrDefault(i => i.Container == P.Serial && i.Layer == layer));
        }

        /// <summary>Returns the serial of the item in the given layer, or 0 if the slot is empty.</summary>
        public virtual uint GetLayer(byte layer) => FindLayer(layer)?.Serial ?? 0;
        /// <summary>Returns true if the player has an item equipped in the named layer slot.</summary>
        public virtual bool CheckLayer(string layerName) => GetItemOnLayer(layerName) != null;

        /// <summary>Sends a normal public speech message from the player character.</summary>
        public virtual void Chat(string message, int hue = 0) => SendSpeech(message, 0x00, hue == 0 ? 0x0034 : hue);
        /// <summary>Sends a normal say speech message with the specified hue.</summary>
        public virtual void ChatSay(string message, int hue = 0x0451) => SendSpeech(message, 0x00, hue);
        /// <summary>Sends a whisper message audible only to nearby players.</summary>
        public virtual void ChatWhisper(string message)  => ChatWhisper(message, 0x3B2);
        /// <summary>Sends a whisper message with the specified hue.</summary>
        public virtual void ChatWhisper(string message, int hue) => SendSpeech(message, 0x02, hue);
        /// <summary>Sends a yell message audible to players farther away.</summary>
        public virtual void ChatYell(string message)     => ChatYell(message, 0x021);
        /// <summary>Sends a yell message with the specified hue.</summary>
        public virtual void ChatYell(string message, int hue) => SendSpeech(message, 0x04, hue);
        /// <summary>Sends an emote message displayed in italics in the UO client.</summary>
        public virtual void ChatEmote(string message)    => ChatEmote(message, 0x024);
        /// <summary>Sends an emote message with the specified hue.</summary>
        public virtual void ChatEmote(string message, int hue) => SendSpeech(message, 0x03, hue);
        /// <summary>Sends a message to the guild channel.</summary>
        public virtual void ChatGuild(string message)    => SendSpeech(message, 0x0D, 0x044);
        /// <summary>Sends a message to the alliance channel.</summary>
        public virtual void ChatAlliance(string message) => SendSpeech(message, 0x0E, 0x057);
        /// <summary>Sends a message to the current UO chat channel.</summary>
        public virtual void ChatChannel(string message)  => SendSpeech(message, 0x0C, 0x034);
        /// <summary>Alias for <see cref="ChatSay"/> — sends a normal public speech message.</summary>
        public virtual void MapSay(string message)       => ChatSay(message);

        /// <summary>Sends a client emote action command (packet 0x12) to trigger a predefined animation on the character.</summary>
        /// <param name="action">The action string name as accepted by the UO server.</param>
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

        /// <summary>Displays a local overhead message above the player's character visible only to the local client.</summary>
        /// <param name="message">The text to display overhead.</param>
        /// <param name="hue">The text color hue (default 945).</param>
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

        /// <summary>Displays a local overhead message above the player (color parameter first for RazorEnhanced compatibility).</summary>
        public virtual void HeadMessage(int color, string msg) => HeadMsg(msg, color);

        /// <summary>Sends an attack request for the entity with the given serial.</summary>
        /// <param name="serial">Serial of the mobile to attack.</param>
        public virtual void Attack(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(PacketBuilder.Attack(serial));
        }

        /// <summary>Attacks the last target stored by the targeting service.</summary>
        public virtual void AttackLast()
        {
            _cancel.ThrowIfCancelled();
            if (_targeting.LastTarget != 0) _packet.SendToServer(PacketBuilder.Attack(_targeting.LastTarget));
        }

        /// <summary>
        /// Sends a message to the party chat channel, optionally addressing it to a specific party member.
        /// </summary>
        /// <param name="message">The message text to send.</param>
        /// <param name="recipientSerial">If non-zero, sends a private message to this party member's serial.</param>
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

        /// <summary>Sends a party invitation request to the server, opening the invitation prompt on the client.</summary>
        public virtual void PartyInvite()
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x06, 0x00, 0x06, 0x01 });
        }

        /// <summary>Accepts a party invitation from the given serial (or the pending invite if serial is 0).</summary>
        public virtual void PartyAccept(uint fromSerial = 0)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x0A, 0x00, 0x06, 0x02, (byte)(fromSerial >> 24), (byte)(fromSerial >> 16), (byte)(fromSerial >> 8), (byte)fromSerial });
        }

        /// <summary>Removes the player from the current party by sending a leave packet.</summary>
        public virtual void LeaveParty()
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x0A, 0x00, 0x06, 0x03, (byte)(Serial >> 24), (byte)(Serial >> 16), (byte)(Serial >> 8), (byte)Serial });
        }

        /// <summary>Sends a run movement request in the specified direction (includes the run flag in the direction byte).</summary>
        /// <param name="direction">Direction name (e.g., "North", "East", "South", "West").</param>
        public virtual void Run(string direction)
        {
            _cancel.ThrowIfCancelled();
            if (Enum.TryParse<TMRazorImproved.Shared.Enums.Direction>(direction, true, out var dir))
            {
                byte d = (byte)((byte)dir | (byte)TMRazorImproved.Shared.Enums.Direction.Running);
                _packet.SendToServer(new byte[] { 0x02, d, 0x00 });
            }
        }

        /// <summary>Sends a walk movement request one tile in the specified direction.</summary>
        /// <param name="direction">Direction name (e.g., "North", "East", "South", "West").</param>
        public virtual void Walk(string direction)
        {
            _cancel.ThrowIfCancelled();
            if (Enum.TryParse<TMRazorImproved.Shared.Enums.Direction>(direction, true, out var dir))
                _packet.SendToServer(new byte[] { 0x02, (byte)dir, 0x00 });
        }

        /// <summary>Turns the player character to face the given direction without moving.</summary>
        /// <param name="direction">Direction name to face (e.g., "North", "East", "South", "West").</param>
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

        /// <summary>
        /// Instructs the client to pathfind to the given world coordinates using both interop and packet 0x38.
        /// </summary>
        /// <param name="x">Target X coordinate.</param>
        /// <param name="y">Target Y coordinate.</param>
        /// <param name="z">Target Z coordinate.</param>
        public virtual void PathFindTo(int x, int y, int z)
        {
            _cancel.ThrowIfCancelled();
            // Prova via interop (macro client)
            _interop.Pathfind(x, y, z);

            // Invia anche al client il pacchetto 0x38 ripetuto per forzare il pathfinding alla coordinata
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

        /// <summary>Activates the primary weapon special ability via the D7 packet.</summary>
        public virtual void WeaponPrimarySA() => SetAbility("primary");
        /// <summary>Activates the secondary weapon special ability via the D7 packet.</summary>
        public virtual void WeaponSecondarySA() => SetAbility("secondary");
        /// <summary>Clears (deactivates) the currently active weapon special ability.</summary>
        public virtual void WeaponClearSA() => SetAbility("clear");
        /// <summary>Triggers the disarm attack type special action.</summary>
        public virtual void WeaponDisarmSA() => AttackType("disarm");
        /// <summary>Triggers the stun/grapple attack type special action.</summary>
        public virtual void WeaponStunSA() => AttackType("grapple");

        /// <summary>Triggers the primary weapon special ability via the client interop service.</summary>
        public virtual void WeaponPrimary()
        {
            _cancel.ThrowIfCancelled();
            _interop.WeaponPrimary();
        }

        /// <summary>Triggers the secondary weapon special ability via the client interop service.</summary>
        public virtual void WeaponSecondary()
        {
            _cancel.ThrowIfCancelled();
            _interop.WeaponSecondary();
        }

        /// <summary>Toggles the player's flying state (Gargoyle race ability) if the current state differs from the desired.</summary>
        /// <param name="status">True to fly, false to land.</param>
        public virtual void Fly(bool status)
        {
            _cancel.ThrowIfCancelled();
            if ((P?.Flying ?? false) != status) _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x05, 0x00, 0x32 });
        }

        /// <summary>Invokes one of the UO virtues (e.g., "Honor", "Sacrifice", "Valor") by sending packet 0xB2.</summary>
        /// <param name="virtueName">The name of the virtue to invoke (case-insensitive).</param>
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

        /// <summary>Returns true if the player currently has the named buff/debuff active.</summary>
        /// <param name="buffName">The buff name to search for (case-insensitive).</param>
        public virtual bool CheckBuffs(string buffName)
        {
            _cancel.ThrowIfCancelled();
            if (P == null || string.IsNullOrEmpty(buffName)) return false;
            if (P.ActiveBuffs.ContainsKey(buffName)) return true;
            return P.OPL?.Properties.Any(p => p.Arguments.Contains(buffName, StringComparison.OrdinalIgnoreCase)) ?? false;
        }

        /// <summary>Alias for <see cref="CheckBuffs"/> — returns true if the named buff is active.</summary>
        public virtual bool HasBuff(string buffName) => CheckBuffs(buffName);
        /// <summary>Alias for <see cref="CheckBuffs"/> — returns true if the named buff is active.</summary>
        public virtual bool BuffsExist(string buffName) => CheckBuffs(buffName);

        /// <summary>Sends a combat attack type command ("grapple" or "disarm") via packet 0xBF.</summary>
        /// <param name="type">The attack type: "grapple" or "disarm".</param>
        public virtual void AttackType(string type)
        {
            _cancel.ThrowIfCancelled();
            byte val = type.ToLowerInvariant() switch { "grapple" => 1, "disarm" => 2, _ => 0 };
            if (val != 0) _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x05, 0x00, val });
        }

        /// <summary>Sends the "equip last weapon" command (packet 0xBF sub 0x16) to re-equip the previously held weapon.</summary>
        public virtual void EquipLastWeapon()
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x06, 0x00, 0x16 });
        }

        /// <summary>Toggles the Always Run movement mode via the client interop service.</summary>
        public virtual void ToggleAlwaysRun()
        {
            _cancel.ThrowIfCancelled();
            _interop.ToggleAlwaysRun();
        }

        /// <summary>Opens the guild management gump by sending the guild button packet.</summary>
        public virtual void GuildButton()
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x05, 0x00, 0x02 });
        }

        /// <summary>Opens the quest log gump by sending the quest button packet.</summary>
        public virtual void QuestButton()
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x05, 0x00, 0x01 });
        }

        /// <summary>Clears the internal corpse tracking list. No-op in current implementation.</summary>
        public virtual void ClearCorpseList() { _cancel.ThrowIfCancelled(); }

        /// <summary>Equips a list of items by their serials, one by one (UO3D-style bulk equip).</summary>
        /// <param name="serials">List of item serials to equip.</param>
        public virtual void EquipUO3D(List<uint> serials)
        {
            _cancel.ThrowIfCancelled();
            foreach (var serial in serials) EquipItem(serial);
        }

        /// <summary>Unequips items from a list of named layer slots, moving them to the backpack (UO3D-style bulk undress).</summary>
        /// <param name="layers">List of layer names to unequip (e.g., "OneHanded", "Neck").</param>
        public virtual void UnEquipUO3D(List<string> layers)
        {
            _cancel.ThrowIfCancelled();
            foreach (var layer in layers) UnEquipItemByLayer(layer);
        }

        /// <summary>Sets whether party members are allowed to loot the player's corpse.</summary>
        /// <param name="canLoot">True to allow party looting, false to disallow.</param>
        public virtual void PartyCanLoot(bool canLoot)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x06, 0x00, 0x06, (byte)(canLoot ? 0x05 : 0x06) });
        }

        /// <summary>Kicks the party member with the given serial from the party.</summary>
        /// <param name="serial">Serial of the party member to remove.</param>
        public virtual void KickMember(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0xBF, 0x00, 0x0A, 0x00, 0x06, 0x04, (byte)(serial >> 24), (byte)(serial >> 16), (byte)(serial >> 8), (byte)serial });
        }

        /// <summary>Sets the static mount serial used by the auto-remount feature.</summary>
        public virtual void SetStaticMount(uint serial) => StaticMount = serial;

        /// <summary>Returns true if the named spell is not in the disabled spells list for the current profile.</summary>
        /// <param name="spellName">The spell name to check (case-insensitive).</param>
        public virtual bool SpellIsEnabled(string spellName)
        {
            _cancel.ThrowIfCancelled();
            var disabled = _config?.CurrentProfile?.DisabledSpells;
            if (disabled == null || disabled.Count == 0) return true;
            return !disabled.Contains(spellName, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Sums the named property value across all items currently equipped on the player.</summary>
        /// <param name="name">The property name to aggregate (e.g., "Hit Chance Increase").</param>
        /// <returns>Total numeric value of the property found across all equipped items.</returns>
        public virtual int SumAttribute(string name)
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return 0;
            return _world.Items.Where(i => i.Container == P.Serial).Sum(item => Wrap(item)?.GetPropValue(name) ?? 0);
        }

        /// <summary>Uses the named skill without sending a target, useful for skills that do not require a target (e.g., Meditation).</summary>
        /// <param name="skillName">The skill name to use (case-insensitive).</param>
        /// <param name="wait">Unused parameter kept for API compatibility.</param>
        public virtual void UseSkillOnly(string skillName, bool wait = true)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
            if (skill != null) _packet.SendToServer(PacketBuilder.UseSkill(skill.ID));
        }

        /// <summary>Stores a key-value pair. No-op in current implementation; reserved for future shared state.</summary>
        /// <remarks>⚠️ Stub: returns placeholder value.</remarks>
        public virtual void HashSet(string key, object value) { }

        /// <summary>Returns a list of all currently active buff/debuff names on the player.</summary>
        public virtual List<string> Buffs
        {
            get
            {
                _cancel.ThrowIfCancelled();
                return P?.ActiveBuffs.Keys.ToList() ?? new List<string>();
            }
        }

        /// <summary>Returns detailed buff information (name and remaining time) for all active buffs on the player.</summary>
        public virtual List<BuffInfo> BuffsInfo
        {
            get
            {
                _cancel.ThrowIfCancelled();
                return P?.ActiveBuffs.Select(kv => new BuffInfo(kv.Key, kv.Value)).ToList() ?? new List<BuffInfo>();
            }
        }

        /// <summary>Returns the <see cref="BuffInfo"/> for the named buff, or null if the buff is not active.</summary>
        /// <param name="buffName">The buff name to look up (partial case-insensitive match supported).</param>
        public virtual BuffInfo? GetBuffInfo(string buffName)
        {
            _cancel.ThrowIfCancelled();
            if (P == null || string.IsNullOrEmpty(buffName)) return null;
            if (P.ActiveBuffs.TryGetValue(buffName, out int remaining)) return new BuffInfo(buffName, remaining);
            var match = P.ActiveBuffs.FirstOrDefault(kv => kv.Key.Contains(buffName, StringComparison.OrdinalIgnoreCase));
            return match.Key != null ? new BuffInfo(match.Key, match.Value) : null;
        }

        /// <summary>Returns the remaining time in milliseconds for the named buff, or 0 if the buff is not active.</summary>
        /// <param name="buffName">The buff name to query.</param>
        public virtual int BuffTime(string buffName)
        {
            _cancel.ThrowIfCancelled();
            return (P != null && P.ActiveBuffs.TryGetValue(buffName, out int v)) ? v : 0;
        }

        /// <summary>Uses a bandage on the player character (finds a bandage in the backpack and double-clicks it).</summary>
        public virtual void BandageSelf() => Bandage(Serial);

        /// <summary>Finds a bandage in the backpack and double-clicks it to start bandaging the target serial.</summary>
        /// <param name="targetSerial">The serial of the mobile to bandage.</param>
        public virtual void Bandage(uint targetSerial)
        {
            _cancel.ThrowIfCancelled();
            var bp = P?.Backpack;
            if (bp == null) return;
            var bandage = _world.Items.FirstOrDefault(i => i.Container == bp.Serial && i.Graphic == 0x0E21);
            if (bandage != null) _packet.SendToServer(PacketBuilder.DoubleClick(bandage.Serial));
        }

        /// <summary>Opens the player's backpack container by double-clicking it.</summary>
        public virtual void OpenBackpack() => OpenContainer(P?.Backpack?.Serial ?? 0);

        /// <summary>Opens the container with the given serial by sending a double-click packet.</summary>
        /// <param name="serial">The serial of the container to open.</param>
        public virtual void OpenContainer(uint serial)
        {
            _cancel.ThrowIfCancelled();
            if (serial != 0) _packet.SendToServer(PacketBuilder.DoubleClick(serial));
        }

        /// <summary>Opens the player's paper doll (character window) by sending packet 0xBF sub 0x0F.</summary>
        public virtual void OpenPaperDoll()
        {
            _cancel.ThrowIfCancelled();
            if (P == null) return;
            byte[] pkt = { 0xBF, 0x00, 0x09, 0x00, 0x0F, 0, 0, 0, 0 };
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(5), P.Serial);
            _packet.SendToServer(pkt);
        }

        /// <summary>Sends a resync request (packet 0x22) to re-synchronize world state with the server.</summary>
        public virtual void Resync()
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0x22, 0xFF, 0x00 });
        }

        /// <summary>Toggles the player's war mode between combat and peaceful.</summary>
        public virtual void ToggleWarMode()
        {
            _cancel.ThrowIfCancelled();
            bool current = P?.WarMode ?? false;
            _packet.SendToServer(new byte[] { 0x72, (byte)(current ? 0x00 : 0x01), 0x00, 0x32, 0x00 });
        }

        /// <summary>Sets the player's war mode to the specified state.</summary>
        /// <param name="warMode">True to enter war mode, false to return to peaceful mode.</param>
        public virtual void SetWarMode(bool warMode)
        {
            _cancel.ThrowIfCancelled();
            _packet.SendToServer(new byte[] { 0x72, (byte)(warMode ? 0x01 : 0x00), 0x00, 0x32, 0x00 });
        }

        /// <summary>Unequips the item currently worn in the named layer slot and moves it to the backpack.</summary>
        /// <param name="layerName">Layer name (e.g., "OneHanded", "Neck", "Head").</param>
        /// <param name="wait">Unused parameter kept for API compatibility.</param>
        public virtual void UnEquipItemByLayer(string layerName, bool wait = true)
        {
            _cancel.ThrowIfCancelled();
            if (!Enum.TryParse<Layer>(layerName, true, out var layer)) return;
            var item = _world.Items.FirstOrDefault(i => i.Container == Serial && i.Layer == (byte)layer);
            if (item == null || Backpack == null) return;
            _packet.SendToServer(PacketBuilder.LiftItem(item.Serial, item.Amount));
            _packet.SendToServer(PacketBuilder.DropToContainer(item.Serial, Backpack.Serial));
        }

        /// <summary>Equips the item with the given serial by lifting it and wearing it on its designated layer.</summary>
        /// <param name="serial">Serial of the item to equip.</param>
        public virtual void EquipItem(uint serial)
        {
            _cancel.ThrowIfCancelled();
            var item = _world.FindItem(serial);
            if (item == null) return;
            _packet.SendToServer(PacketBuilder.LiftItem(item.Serial, item.Amount));
            _packet.SendToServer(PacketBuilder.WearItem(item.Serial, item.Layer, Serial));
        }

        /// <summary>Equips the given ScriptItem by calling <see cref="EquipItem(uint)"/> with its serial.</summary>
        public virtual void EquipItem(ScriptItem item) => EquipItem(item.Serial);

        /// <summary>Returns the current (modified) value of the named skill, or 0 if not found.</summary>
        /// <param name="skillName">Skill name to look up (case-insensitive).</param>
        public virtual double GetSkillValue(string skillName)
        {
            _cancel.ThrowIfCancelled();
            return _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase))?.Value ?? 0;
        }

        /// <summary>Returns the base (unmodified) value of the named skill, or 0 if not found.</summary>
        /// <param name="skillName">Skill name to look up (case-insensitive).</param>
        public virtual double GetRealSkillValue(string skillName)
        {
            _cancel.ThrowIfCancelled();
            return _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase))?.BaseValue ?? 0;
        }

        /// <summary>Returns the skill cap (maximum achievable value) for the named skill.</summary>
        /// <param name="skillName">Skill name to look up (case-insensitive).</param>
        public virtual double GetSkillCap(string skillName)
        {
            _cancel.ThrowIfCancelled();
            return _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase))?.Cap ?? 0;
        }

        /// <summary>Returns the lock status of the named skill as an integer (0=Up, 1=Down, 2=Locked).</summary>
        /// <param name="skillName">Skill name to look up (case-insensitive).</param>
        public virtual int GetSkillStatus(string skillName)
        {
            _cancel.ThrowIfCancelled();
            return (int)(_skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase))?.Lock ?? 0);
        }

        /// <summary>Sets the lock status of the named skill and sends the change to the server.</summary>
        /// <param name="skillName">Skill name to modify (case-insensitive).</param>
        /// <param name="status">Lock status: 0=Up, 1=Down, 2=Locked.</param>
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

        /// <summary>Returns the lock status of the named primary stat as an integer (0=Up, 1=Down, 2=Locked).</summary>
        /// <param name="statName">Stat name: "strength", "dexterity", or "intelligence".</param>
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

        /// <summary>Sets the lock status of the named primary stat and sends the change to the server.</summary>
        /// <param name="statName">Stat name: "strength", "dexterity", or "intelligence".</param>
        /// <param name="status">Lock status: 0=Up, 1=Down, 2=Locked.</param>
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

        /// <summary>Sends a self-target response, targeting the player character.</summary>
        public virtual void TargetSelf() => _targeting.TargetSelf();

        /// <summary>Uses the named skill, optionally targeting a specific entity afterwards.</summary>
        /// <param name="skillName">Skill name to use (case-insensitive).</param>
        /// <param name="targetSerial">If non-zero, sends this serial as the skill target.</param>
        public virtual void UseSkill(string skillName, uint targetSerial = 0)
        {
            _cancel.ThrowIfCancelled();
            var skill = _skills.Skills.FirstOrDefault(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));
            if (skill != null) { _packet.SendToServer(PacketBuilder.UseSkill(skill.ID)); _targeting.SendTarget(targetSerial); }
        }

        /// <summary>
        /// Sets the weapon special ability via packet 0xD7 ("primary", "secondary", or "clear").
        /// </summary>
        /// <param name="abilityName">The ability to set: "primary" (1), "secondary" (2), or "clear" (0).</param>
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

        /// <summary>Casts the named spell by resolving its ID and sending a CastSpell packet to the server.</summary>
        /// <param name="spellName">The spell name to cast (case-insensitive).</param>
        public virtual void Cast(string spellName)
        {
            _cancel.ThrowIfCancelled();
            if (SpellsApi.TryGetSpellId(spellName, out int id)) _packet.SendToServer(PacketBuilder.CastSpell(id));
        }

        /// <summary>Returns all property strings from the player's object property list (OPL).</summary>
        public virtual List<string> GetPropStringList() => P?.Properties ?? new List<string>();
        /// <summary>Returns the property string at the given index from the player's OPL, or empty string if out of range.</summary>
        public virtual string GetPropStringByIndex(int index) => (P != null && index >= 0 && index < P.Properties.Count) ? P.Properties[index] : string.Empty;

        /// <summary>
        /// Returns the numeric value of the named property from the player's OPL, parsing the first integer found.
        /// </summary>
        /// <param name="name">Property name to search for in the OPL (case-insensitive).</param>
        /// <returns>The first numeric value found in the matching property, or 0 if not found.</returns>
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
