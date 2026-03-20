using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public class TargetApi
    {
        private readonly ITargetingService _targeting;
        private readonly IWorldService _world;
        private readonly IConfigService _config;
        private readonly IPacketService _packet;
        private readonly ScriptCancellationController _cancel;

        public TargetApi(ITargetingService targeting, IWorldService world, IConfigService config, IPacketService packet, ScriptCancellationController cancel)
        {
            _targeting = targeting;
            _world = world;
            _config = config;
            _packet = packet;
            _cancel = cancel;
        }

        private ScriptMobile? Wrap(Mobile? m) => m == null ? null : new ScriptMobile(m, _world, _packet, _targeting);
        private ScriptItem? Wrap(Item? i) => i == null ? null : new ScriptItem(i, _world, _packet, _targeting);

        public virtual void Self()
        {
            _cancel.ThrowIfCancelled();
            _targeting.TargetSelf();
        }

        public virtual void Last()
        {
            _cancel.ThrowIfCancelled();
            _targeting.SendTarget(_targeting.LastTarget);
        }

        public virtual void Cancel()
        {
            _cancel.ThrowIfCancelled();
            _targeting.CancelTarget();
        }

        /// <summary>
        /// Attende che il server invii un target cursor (0x6C S2C) fino a <paramref name="timeout"/> ms.
        /// Ritorna true se il cursor è arrivato, false se scaduto il timeout.
        /// </summary>
        public virtual bool WaitForTarget(int timeout = 5000)
        {
            _cancel.ThrowIfCancelled();

            // Il cursore è già pendente — ritorna subito
            if (_targeting.HasTargetCursor) return true;

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Action<uint> handler = _ => tcs.TrySetResult(true);

            _targeting.TargetCursorRequested += handler;
            try
            {
                var deadline = Environment.TickCount64 + timeout;
                while (Environment.TickCount64 < deadline)
                {
                    _cancel.ThrowIfCancelled();
                    if (_targeting.HasTargetCursor || tcs.Task.IsCompleted) return true;
                    Thread.Sleep(50);
                }
                return _targeting.HasTargetCursor;
            }
            finally
            {
                _targeting.TargetCursorRequested -= handler;
            }
        }

        /// <summary>Ritorna true se c'è un target cursor S2C attivo inviato dal server e non ancora consumato, filtrabile per tipo.</summary>
        public virtual bool HasTarget(string targetFlag = "Any")
        {
            if (!_targeting.HasTargetCursor) return false;

            return targetFlag.ToLowerInvariant() switch
            {
                "beneficial" => _targeting.PendingCursorType == 2,
                "harmful" => _targeting.PendingCursorType == 1,
                "neutral" => _targeting.PendingCursorType == 0,
                "any" => true,
                _ => throw new ArgumentOutOfRangeException(nameof(targetFlag), "Valid flags: Beneficial, Harmful, Neutral, Any")
            };
        }

        public virtual int GetLast() => (int)_targeting.LastTarget;
        public virtual void SetLastTarget(uint serial) => _targeting.LastTarget = serial;

        public virtual void TargetExecute(uint serial)
        {
            _cancel.ThrowIfCancelled();
            _targeting.SendTarget(serial);
        }

        public virtual void Target(uint serial) => TargetExecute(serial);

        public virtual void TargetExecute(int x, int y, int z, int graphic)
        {
            _cancel.ThrowIfCancelled();
            _targeting.SendTarget(0, (ushort)x, (ushort)y, (sbyte)z, (ushort)graphic);
        }

        // FR-024: 3-parameter ground target overload (graphic defaults to 0)
        /// <summary>Targets a ground tile at the specified coordinates (no static graphic).</summary>
        public virtual void TargetExecute(int x, int y, int z) => TargetExecute(x, y, z, 0);

        public virtual bool HasPrompt() => _targeting.HasPrompt;

        public virtual bool WaitForPrompt(int timeout = 5000)
        {
            if (_targeting.HasPrompt) return true;

            var tcs = new TaskCompletionSource<bool>();
            Action<bool> handler = (hasPrompt) => { if (hasPrompt) tcs.TrySetResult(true); };

            _targeting.PromptChanged += handler;
            try
            {
                var deadline = Environment.TickCount64 + timeout;
                while (Environment.TickCount64 < deadline)
                {
                    _cancel.ThrowIfCancelled();
                    if (_targeting.HasPrompt) return true;
                    Thread.Sleep(50);
                }
                return _targeting.HasPrompt;
            }
            finally
            {
                _targeting.PromptChanged -= handler;
            }
        }

        public virtual void SendPrompt(string text)
        {
            _cancel.ThrowIfCancelled();
            _targeting.SendPrompt(text);
        }

        /// <summary>Invia un target di tipo land/ground alle coordinate specificate (z=0 se non noto).</summary>
        public virtual void TargetXYZ(int x, int y, int z = 0, int graphic = 0)
        {
            _cancel.ThrowIfCancelled();
            _targeting.SendTarget(0, (ushort)x, (ushort)y, (sbyte)z, (ushort)graphic);
        }

        /// <summary>
        /// Attende un target cursor poi lo invia alla tile ground specificata.
        /// </summary>
        public virtual bool TargetGround(int x, int y, int z = 0, int timeoutMs = 5000)
        {
            if (!WaitForTarget(timeoutMs)) return false;
            TargetXYZ(x, y, z);
            return true;
        }

        /// <summary>
        /// Avanza al prossimo target nella coda. Ritorna il cursore inviato (0 se nessun target in coda).
        /// </summary>
        public virtual void TargetNext()
        {
            _cancel.ThrowIfCancelled();
            _targeting.TargetNext();
        }

        // --- Migrated Missing APIs ---

        public virtual void AttackTargetFromList(string target_name)
        {
            _cancel.ThrowIfCancelled();
            var mob = GetTargetFromList(target_name) as ScriptMobile;
            if (mob != null)
            {
                _targeting.SendTarget(mob.Serial);
            }
        }
        
        public virtual void ClearLast()
        {
            _cancel.ThrowIfCancelled();
            _targeting.LastTarget = 0;
        }

        public virtual void ClearLastandQueue()
        {
            _cancel.ThrowIfCancelled();
            _targeting.LastTarget = 0;
            _targeting.Clear();
        }

        public virtual void ClearLastAttack()
        {
            _cancel.ThrowIfCancelled();
            if (_world.Player != null)
                lock (_world.Player.SyncRoot) { _world.Player.AttackTarget = 0; }
        }

        public virtual void ClearQueue()
        {
            _cancel.ThrowIfCancelled();
            _targeting.Clear();
        }

        public virtual int GetLastAttack()
        {
            _cancel.ThrowIfCancelled();
            return (int)(_world.Player?.AttackTarget ?? 0);
        }

        public virtual object? GetTargetFromList(string target_name)
        {
            _cancel.ThrowIfCancelled();
            var profile = _config.CurrentProfile;
            if (profile == null) return null;

            var filter = profile.TargetLists.FirstOrDefault(f => f.Name.Equals(target_name, StringComparison.OrdinalIgnoreCase));
            if (filter == null) return null;

            var player = _world.Player;
            var list = _world.Mobiles.Where(m =>
                m.Serial != player?.Serial &&
                (filter.BodyIDs.Count == 0 || filter.BodyIDs.Contains(m.Graphic)) &&
                (filter.Hues.Count == 0 || filter.Hues.Contains(m.Hue)) &&
                (filter.Notorieties.Count == 0 || filter.Notorieties.Contains(m.Notoriety)) &&
                (player == null || m.DistanceTo(player) <= filter.Range)
            ).ToList();

            if (!list.Any()) return null;

            var target = list.OrderBy(m => player != null ? m.DistanceTo(player) : 0).First();
            return Wrap(target);
        }

        public virtual void LastQueued()
        {
            _cancel.ThrowIfCancelled();
            _targeting.SendTarget(_targeting.LastTarget);
        }

        public virtual int LastUsedObject()
        {
            _cancel.ThrowIfCancelled();
            return (int)(_world.Player?.LastObject ?? 0);
        }

        public virtual void PerformTargetFromList(string target_name)
        {
            _cancel.ThrowIfCancelled();
            var target = GetTargetFromList(target_name);
            if (target is ScriptMobile m) _targeting.SendTarget(m.Serial);
            else if (target is ScriptItem i) _targeting.SendTarget(i.Serial);
        }

        public virtual void TargetResource(uint item_serial, int resource_number)
        {
            _cancel.ThrowIfCancelled();
            var item = _world.FindItem(item_serial);
            if (item == null) return;
            
            _packet.SendToServer(TMRazorImproved.Core.Utilities.PacketBuilder.TargetByResource(item_serial, resource_number));
        }

        public virtual void TargetResource(uint item_serial, string resource_name)
        {
            _cancel.ThrowIfCancelled();
            int number;
            switch (resource_name.ToLowerInvariant())
            {
                case "ore": number = 0; break;
                case "sand": number = 1; break;
                case "wood": number = 2; break;
                case "graves": number = 3; break;
                case "red_mushroom": number = 4; break;
                default:
                    if (!int.TryParse(resource_name, out number))
                        return;
                    break;
            }
            TargetResource(item_serial, number);
        }

        public virtual void TargetResource(ScriptItem item, string resource_name)
        {
            if (item != null)
                TargetResource(item.Serial, resource_name);
        }

        public virtual void TargetResource(ScriptItem item, int resource_number)
        {
            if (item != null)
                TargetResource(item.Serial, resource_number);
        }

        public virtual Point3D? PromptGroundTarget(string message = "Select Ground Position", int color = 945)
        {
            _cancel.ThrowIfCancelled();
            if (_world.Player != null)
            {
                // _world.Player.HeadMsg(message, (ushort)color);
            }

            var task = _targeting.AcquireTargetAsync();
            while (!task.IsCompleted)
            {
                _cancel.ThrowIfCancelled();
                Thread.Sleep(50);
            }

            var info = task.GetAwaiter().GetResult();
            // If it's an object target (serial != 0), try to use the object's position
            // But usually this is called for ground.
            if (info.Serial != 0)
            {
                var ent = _world.FindEntity(info.Serial);
                if (ent != null) return new Point3D(ent.X, ent.Y, ent.Z);
            }

            // If it's a ground click, return the parsed X, Y, Z
            if (info.X != 0 || info.Y != 0)
            {
                return new Point3D(info.X, info.Y, info.Z);
            }

            return null;
        }

        public virtual int PromptTarget(string message = "Select Item or Mobile", int color = 945)
        {
            _cancel.ThrowIfCancelled();
            if (_world.Player != null)
            {
                // _world.Player.HeadMsg(message, (ushort)color);
            }

            var task = _targeting.AcquireTargetAsync();
            while (!task.IsCompleted)
            {
                _cancel.ThrowIfCancelled();
                Thread.Sleep(50);
            }

            var info = task.GetAwaiter().GetResult();
            if (info.Serial == 0 && info.X == 0 && info.Y == 0) return -1;
            return (int)info.Serial;
        }

        public virtual void SelfQueued()
        {
            _cancel.ThrowIfCancelled();
            _targeting.TargetSelf();
        }

        public virtual void SetLast(uint serial, bool wait = true)
        {
            _cancel.ThrowIfCancelled();
            _targeting.LastTarget = serial;
            
            // official UO highlight packet (0x73)
            if (serial != 0)
            {
                var highlightPkt = new byte[] { 
                    0x73, 
                    (byte)(serial >> 24), 
                    (byte)(serial >> 16), 
                    (byte)(serial >> 8), 
                    (byte)serial 
                };
                _packet.SendToClient(highlightPkt);
            }

            // Visual highlight (legacy style)
            var mob = _world.FindMobile(serial);
            if (mob != null)
            {
                var scriptMob = new ScriptMobile(mob, _world, _packet, _targeting);
                scriptMob.OverheadMsg("* Target *", 10);
            }
        }

        public virtual void SetLast(object mob)
        {
            if (mob is ScriptMobile m) _targeting.LastTarget = m.Serial;
            else if (mob is uint u) _targeting.LastTarget = u;
        }

        public virtual void SetLastTargetFromList(string target_name)
        {
            var target = GetTargetFromList(target_name);
            if (target is ScriptMobile m) _targeting.LastTarget = m.Serial;
            else if (target is ScriptItem i) _targeting.LastTarget = i.Serial;
        }

        private static readonly System.Collections.Generic.HashSet<int> CaveTiles = new() { 0xae, 0x5, 0x3, 0xc1, 0xc2, 0xc3, 0xbd, 0x0016, 0x0017, 0x0018, 0x0019,
            0x244, 0x245, 0x246, 0x247, 0x248, 0x249, 0x22b, 0x22c, 0x22d, 0x22e, 0x22f,
            0x053B, 0x053C, 0x053D, 0x053E, 0x053f };

        private static Ultima.Map? GetUltimaMap(int mapId) => mapId switch
        {
            0 => Ultima.Map.Felucca,
            1 => Ultima.Map.Trammel,
            2 => Ultima.Map.Ilshenar,
            3 => Ultima.Map.Malas,
            4 => Ultima.Map.Tokuno,
            5 => Ultima.Map.TerMur,
            _ => Ultima.Map.Felucca
        };

        public virtual void TargetExecuteRelative(uint serial, int offset)
        {
            _cancel.ThrowIfCancelled();
            var ent = _world.FindEntity(serial);
            if (ent == null) return;
            var player = _world.Player;
            if (player == null) return;

            int x = ent.X;
            int y = ent.Y;
            int dir = (ent is Mobile m) ? (m.Direction & 0x07) : 0;
            switch (dir)
            {
                case 0: /* North */ y -= offset; break;
                case 1: /* Right */ x += offset; y -= offset; break;
                case 2: /* East  */ x += offset; break;
                case 3: /* Down  */ x += offset; y += offset; break;
                case 4: /* South */ y += offset; break;
                case 5: /* Left  */ x -= offset; y += offset; break;
                case 6: /* West  */ x -= offset; break;
                case 7: /* Up    */ x -= offset; y -= offset; break;
            }

            var umap = GetUltimaMap(player.MapId);
            if (umap != null)
            {
                var tileinfo = umap.Tiles.GetLandTile(x, y, true);
                var staticTiles = umap.Tiles.GetStaticTiles(x, y, true);

                if (CaveTiles.Contains(tileinfo.Id))
                {
                    if (staticTiles != null && staticTiles.Length > 0)
                    {
                        TargetExecute(x, y, staticTiles[0].Z, staticTiles[0].Id);
                        return;
                    }

                    var items = _world.Items.Where(i => i.X == x && i.Y == y && i.Z == tileinfo.Z);
                    foreach (var item in items)
                    {
                        if (CaveTiles.Contains(item.Graphic))
                        {
                            TargetExecute(item.Serial);
                            return;
                        }
                    }
                }

                TargetExecute(x, y, tileinfo.Z, 0);
            }
            else
            {
                _targeting.SendTarget(0, (ushort)x, (ushort)y, (sbyte)ent.Z, ent.Graphic);
            }
        }

        public virtual bool TargetType(int graphic, int color = -1, int range = 20, string selector = "Nearest", System.Collections.Generic.List<byte>? notoriety = null)
        {
            _cancel.ThrowIfCancelled();
            var player = _world.Player;
            if (player == null) return false;

            // 1. Backpack items
            var bpSerial = player.Backpack?.Serial ?? 0;
            var bpItems = _world.Items.Where(i =>
                i.Graphic == (ushort)graphic &&
                (color == -1 || i.Hue == (ushort)color) &&
                (i.RootContainer == player.Serial || i.RootContainer == bpSerial || i.Container == player.Serial || i.Container == bpSerial)
            ).ToList();

            if (bpItems.Any())
            {
                _targeting.SendTarget(bpItems.First().Serial);
                return true;
            }

            // 2. Ground items
            var groundItems = _world.Items.Where(i =>
                i.Graphic == (ushort)graphic &&
                (color == -1 || i.Hue == (ushort)color) &&
                i.OnGround &&
                i.DistanceTo(player) <= range
            ).ToList();

            if (groundItems.Any())
            {
                var itemTarget = selector.ToLower() switch
                {
                    "nearest" => groundItems.OrderBy(i => i.DistanceTo(player)).First(),
                    "farthest" => groundItems.OrderByDescending(i => i.DistanceTo(player)).First(),
                    _ => groundItems.First()
                };
                _targeting.SendTarget(itemTarget.Serial);
                return true;
            }

            // 3. Mobiles
            var mobiles = _world.Mobiles.Where(m =>
                m.Graphic == (ushort)graphic &&
                (color == -1 || m.Hue == (ushort)color) &&
                m.DistanceTo(player) <= range &&
                (notoriety == null || notoriety.Count == 0 || notoriety.Contains(m.Notoriety))
            ).ToList();

            if (mobiles.Any())
            {
                var target = selector.ToLower() switch
                {
                    "nearest" => mobiles.OrderBy(m => m.DistanceTo(player)).First(),
                    "farthest" => mobiles.OrderByDescending(m => m.DistanceTo(player)).First(),
                    _ => mobiles.First()
                };
                _targeting.SendTarget(target.Serial);
                return true;
            }

            return false;
        }

        #region int-serial overloads — RazorEnhanced compatibility (TASK-FR-012)
        public virtual void SetLastTarget(int serial) => SetLastTarget((uint)serial);
        public virtual void TargetExecute(int serial) => TargetExecute((uint)serial);
        public virtual void Target(int serial) => Target((uint)serial);
        public virtual void SetLast(int serial, bool wait = true) => SetLast((uint)serial, wait);
        public virtual void TargetExecuteRelative(int serial, int offset) => TargetExecuteRelative((uint)serial, offset);
        public virtual void TargetResource(int item_serial, int resource_number) => TargetResource((uint)item_serial, resource_number);
        public virtual void TargetResource(int item_serial, string resource_name) => TargetResource((uint)item_serial, resource_name);
        #endregion

        public virtual bool WaitForTargetOrFizzle(int delay = 5000, bool noshow = false)
        {
            _cancel.ThrowIfCancelled();

            if (_targeting.HasTargetCursor) return true;

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Action<uint> targetHandler = _ => tcs.TrySetResult(true);

            bool fizzled = false;
            Action<byte[]> fizzleHandler = data =>
            {
                if (data.Length >= 5)
                {
                    ushort soundId = (ushort)((data[2] << 8) | data[3]);
                    if (soundId == 0x5c)
                    {
                        fizzled = true;
                        tcs.TrySetResult(false);
                    }
                }
            };

            _targeting.TargetCursorRequested += targetHandler;
            _packet.RegisterViewer(TMRazorImproved.Shared.Enums.PacketPath.ServerToClient, 0x54, fizzleHandler);

            try
            {
                var deadline = Environment.TickCount64 + delay;
                while (Environment.TickCount64 < deadline)
                {
                    _cancel.ThrowIfCancelled();
                    if (_targeting.HasTargetCursor || fizzled || tcs.Task.IsCompleted)
                    {
                        return _targeting.HasTargetCursor;
                    }
                    Thread.Sleep(50);
                }
                return _targeting.HasTargetCursor;
            }
            finally
            {
                _targeting.TargetCursorRequested -= targetHandler;
                _packet.UnregisterViewer(TMRazorImproved.Shared.Enums.PacketPath.ServerToClient, 0x54, fizzleHandler);
            }
        }
    }
}
