using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    /// <summary>
    /// API per la gestione dei suoni e della musica, esposta come variabile 'Sound' negli script.
    /// </summary>
    public class SoundApi
    {
        private readonly ISoundService _soundService;
        private readonly IWorldService _worldService;
        private readonly ITargetingService _targetingService;
        private readonly ScriptCancellationController _cancel;
        private readonly IPacketService _packet;

        // --- FR-027: Static sound-filter state (shared across all SoundApi instances) ---
        private static readonly object _syncRoot = new();
        private static readonly Dictionary<string, List<int>> _namedFilters = new(StringComparer.OrdinalIgnoreCase);
        private static readonly List<(WeakReference<ManualResetEvent> MreRef, List<int> Sounds)> _soundWaiters = new();
        private static int _lastMatchX, _lastMatchY, _lastMatchZ;
        private static Action<byte[]>? _viewerCallback;
        private static IPacketService? _registeredWith;

        public SoundApi(
            ISoundService soundService,
            IWorldService worldService,
            ITargetingService targetingService,
            ScriptCancellationController cancel,
            IPacketService? packet = null)
        {
            _soundService = soundService;
            _worldService = worldService;
            _targetingService = targetingService;
            _cancel = cancel;
            _packet = packet!;
        }

        /// <summary>Riproduce un effetto sonoro alla posizione del giocatore.</summary>
        public virtual void PlaySoundEffect(int id)
        {
            _cancel.ThrowIfCancelled();
            if (_worldService.Player != null)
                _soundService.PlaySound((ushort)id, _worldService.Player.X, _worldService.Player.Y, _worldService.Player.Z);
            else
                _soundService.PlaySound((ushort)id);
        }

        /// <summary>Riproduce un brano musicale.</summary>
        public virtual void PlayMusic(int id)
        {
            _cancel.ThrowIfCancelled();
            _soundService.PlayMusic((ushort)id);
        }

        /// <summary>Interrompe la musica corrente.</summary>
        public virtual void StopSound()
        {
            _cancel.ThrowIfCancelled();
            _soundService.StopMusic();
        }

        /// <summary>Riproduce un suono alla posizione di un oggetto (Item o Mobile).</summary>
        public virtual void PlayObject(uint serial, int id)
        {
            _cancel.ThrowIfCancelled();
            var item = _worldService.FindItem(serial);
            if (item != null)
            {
                _soundService.PlaySound((ushort)id, item.X, item.Y, item.Z);
                return;
            }

            var mobile = _worldService.FindMobile(serial);
            if (mobile != null)
            {
                _soundService.PlaySound((ushort)id, mobile.X, mobile.Y, mobile.Z);
            }
        }

        /// <summary>Riproduce un suono alla posizione di un mobile.</summary>
        public virtual void PlayMobile(uint serial, int id)
        {
            _cancel.ThrowIfCancelled();
            var mobile = _worldService.FindMobile(serial);
            if (mobile != null)
            {
                _soundService.PlaySound((ushort)id, mobile.X, mobile.Y, mobile.Z);
            }
        }

        /// <summary>Richiede un target e riproduce un suono alla posizione selezionata.</summary>
        public virtual void PlayTarget(int id)
        {
            _cancel.ThrowIfCancelled();
            _targetingService.RequestTarget();
            
            var task = _targetingService.AcquireTargetAsync();
            while (!task.IsCompleted)
            {
                _cancel.ThrowIfCancelled();
                Thread.Sleep(50);
            }

            var info = task.GetAwaiter().GetResult();
            if (info.X != 0 || info.Y != 0)
            {
                _soundService.PlaySound((ushort)id, info.X, info.Y, info.Z);
            }
        }

        /// <summary>Riproduce un suono a coordinate specifiche.</summary>
        public virtual void PlayLocation(int x, int y, int z, int id)
        {
            _cancel.ThrowIfCancelled();
            _soundService.PlaySound((ushort)id, x, y, z);
        }

        /// <summary>Attende per il numero di millisecondi specificato (alias per Misc.Pause).</summary>
        public virtual void PlayDelay(int ms)
        {
            if (ms <= 0) return;
            var deadline = Environment.TickCount64 + ms;
            while (Environment.TickCount64 < deadline)
            {
                _cancel.ThrowIfCancelled();
                var remaining = (int)(deadline - Environment.TickCount64);
                Thread.Sleep(Math.Min(10, Math.Max(0, remaining)));
            }
        }

        /// <summary>Placeholder per compatibilità legacy.</summary>
        public virtual int GetMinDuration() => 0;

        /// <summary>Placeholder per compatibilità legacy.</summary>
        public virtual int GetMaxDuration() => 0;

        // --- FR-027: Sound Filtering API -----------------------------------------------

        /// <summary>
        /// Registers a named sound filter. When a sound packet with any of the given IDs arrives,
        /// it is recorded in <see cref="LastSoundMatch"/>.
        /// </summary>
        public virtual void AddFilter(string name, List<int> soundIds)
        {
            _cancel.ThrowIfCancelled();
            lock (_syncRoot)
            {
                _namedFilters[name] = new List<int>(soundIds);
                EnsureViewerRegistered();
            }
        }

        /// <summary>Removes a previously registered sound filter by name.</summary>
        public virtual void RemoveFilter(string name)
        {
            _cancel.ThrowIfCancelled();
            lock (_syncRoot)
            {
                _namedFilters.Remove(name);
                TryUnregisterViewer();
            }
        }

        /// <summary>Returns the world position where the last filter-matched sound was played.</summary>
        public virtual Point3D LastSoundMatch()
        {
            lock (_syncRoot) { return new Point3D(_lastMatchX, _lastMatchY, _lastMatchZ); }
        }

        /// <summary>
        /// Blocks until one of the specified sound IDs is received from the server, or the timeout expires.
        /// Uses <see cref="ManualResetEvent"/> signalled by the 0x54 packet viewer.
        /// </summary>
        /// <param name="soundIds">List of sound IDs to wait for.</param>
        /// <param name="timeout">Timeout in milliseconds. -1 (or 0) = 10 minutes max.</param>
        public virtual bool WaitForSound(List<int> soundIds, int timeout = -1)
        {
            _cancel.ThrowIfCancelled();
            var mre = new ManualResetEvent(false);
            lock (_syncRoot)
            {
                _soundWaiters.Add((new WeakReference<ManualResetEvent>(mre), new List<int>(soundIds)));
                EnsureViewerRegistered();
            }
            try
            {
                long deadline = Environment.TickCount64 + (timeout < 1 ? 600_000 : timeout);
                while (Environment.TickCount64 < deadline)
                {
                    _cancel.ThrowIfCancelled();
                    if (mre.WaitOne(50)) return true;
                }
                return false;
            }
            finally
            {
                lock (_syncRoot)
                {
                    _soundWaiters.RemoveAll(w =>
                    {
                        if (!w.MreRef.TryGetTarget(out var t)) return true;
                        return ReferenceEquals(t, mre);
                    });
                    TryUnregisterViewer();
                }
            }
        }

        private void EnsureViewerRegistered()
        {
            if (_viewerCallback == null && _packet != null)
            {
                _viewerCallback = OnSoundPacket;
                _registeredWith = _packet;
                _packet.RegisterViewer(PacketPath.ServerToClient, 0x54, _viewerCallback);
            }
        }

        private static void TryUnregisterViewer()
        {
            if (_viewerCallback != null && _namedFilters.Count == 0 && _soundWaiters.Count == 0)
            {
                _registeredWith?.UnregisterViewer(PacketPath.ServerToClient, 0x54, _viewerCallback);
                _viewerCallback = null;
                _registeredWith = null;
            }
        }

        // Packet 0x54: [id:1][flags:1][soundId:2][volume:2][x:2][y:2][z:2]
        private static void OnSoundPacket(byte[] data)
        {
            if (data.Length < 12) return;
            ushort soundId = (ushort)((data[2] << 8) | data[3]);
            int x = (data[6] << 8) | data[7];
            int y = (data[8] << 8) | data[9];
            int z = (data[10] << 8) | data[11];

            lock (_syncRoot)
            {
                // Check named filters
                foreach (var kv in _namedFilters)
                {
                    if (kv.Value.Contains(soundId))
                    {
                        _lastMatchX = x; _lastMatchY = y; _lastMatchZ = z;
                        break;
                    }
                }

                // Signal waiters
                bool needsCleanup = false;
                foreach (var (mreRef, sounds) in _soundWaiters)
                {
                    if (!sounds.Contains(soundId)) continue;
                    if (mreRef.TryGetTarget(out var mre))
                    {
                        _lastMatchX = x; _lastMatchY = y; _lastMatchZ = z;
                        mre.Set();
                    }
                    else
                    {
                        needsCleanup = true;
                    }
                }
                if (needsCleanup)
                    _soundWaiters.RemoveAll(w => !w.MreRef.TryGetTarget(out _));
            }
        }

        #region int-serial overloads — RazorEnhanced compatibility (TASK-FR-012)
        public virtual void PlayObject(int serial, int id) => PlayObject((uint)serial, id);
        public virtual void PlayMobile(int serial, int id) => PlayMobile((uint)serial, id);
        #endregion
    }
}
