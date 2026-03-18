using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        public SoundApi(
            ISoundService soundService, 
            IWorldService worldService, 
            ITargetingService targetingService,
            ScriptCancellationController cancel)
        {
            _soundService = soundService;
            _worldService = worldService;
            _targetingService = targetingService;
            _cancel = cancel;
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
    }
}
