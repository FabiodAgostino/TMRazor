using System;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    // FIX BUG-P2-02: FiltersApi ora delega ai filtri reali del profilo tramite IConfigService
    /// <summary>Enables and disables visual/audio filters: light, sound, weather, death animation, and others.</summary>
    public class FiltersApi
    {
        private readonly IConfigService _config;
        private readonly ScriptCancellationController _cancel;

        public FiltersApi(IConfigService config, ScriptCancellationController cancel)
        {
            _config = config;
            _cancel = cancel;
        }

        // TMRazor Legacy Properties
        /// <summary>Gets or sets the delay (in ms) before the AutoRemount agent tries to remount after being dismounted.</summary>
        public virtual int AutoRemountEDelay
        { 
            get 
            {
                _cancel.ThrowIfCancelled();
                return _config.CurrentProfile?.AutoRemountEDelay ?? 0;
            }
            set
            {
                _cancel.ThrowIfCancelled();
                if (_config.CurrentProfile != null)
                    _config.CurrentProfile.AutoRemountEDelay = value;
            }
        }

        /// <summary>Gets or sets the serial of the mount used by the AutoRemount agent.</summary>
        public virtual int AutoRemountSerial
        { 
            get 
            {
                _cancel.ThrowIfCancelled();
                return (int)(_config.CurrentProfile?.RemountSerial ?? 0);
            }
            set
            {
                _cancel.ThrowIfCancelled();
                if (_config.CurrentProfile != null)
                    _config.CurrentProfile.RemountSerial = (uint)value;
            }
        }

        public class GraphChangeData
        {
            public bool Selected { get; set; }
            public int GraphReal { get; set; }
            public int GraphNew { get; set; }
            public int ColorNew { get; set; }
        }

        /// <summary>Enables a filter by name. Valid names: light, weather, sound, death, poison, snoop, bardmusic, footsteps, karmafame, season.</summary>
        public virtual void Enable(string name)
        {
            _cancel.ThrowIfCancelled();
            var p = _config.CurrentProfile;
            if (p == null) return;
            SetFilter(p, name, true);
        }

        /// <summary>Disables a filter by name. See <see cref="Enable"/> for valid names.</summary>
        public virtual void Disable(string name)
        {
            _cancel.ThrowIfCancelled();
            var p = _config.CurrentProfile;
            if (p == null) return;
            SetFilter(p, name, false);
        }

        /// <summary>Returns <c>true</c> if the specified filter is currently enabled.</summary>
        public virtual bool IsEnabled(string name)
        {
            _cancel.ThrowIfCancelled();
            var p = _config.CurrentProfile;
            if (p == null) return false;
            return name.ToLowerInvariant() switch
            {
                "light"        => p.FilterLight,
                "weather"      => p.FilterWeather,
                "sound"        => p.FilterSound,
                "death"        => p.FilterDeath,
                "poison"       => p.FilterPoison,
                "snoop"        => p.FilterSnoop,
                "bardmusic"    => p.FilterBardMusic,
                "footsteps"    => p.FilterFootsteps,
                "karmafame"    => p.FilterKarmaFame,
                "season"       => p.FilterSeason,
                _ => false
            };
        }

        private static void SetFilter(TMRazorImproved.Shared.Models.Config.UserProfile p, string name, bool value)
        {
            switch (name.ToLowerInvariant())
            {
                case "light":       p.FilterLight       = value; break;
                case "weather":     p.FilterWeather     = value; break;
                case "sound":       p.FilterSound       = value; break;
                case "death":       p.FilterDeath       = value; break;
                case "poison":      p.FilterPoison      = value; break;
                case "snoop":       p.FilterSnoop       = value; break;
                case "bardmusic":   p.FilterBardMusic   = value; break;
                case "footsteps":   p.FilterFootsteps   = value; break;
                case "karmafame":   p.FilterKarmaFame   = value; break;
                case "season":      p.FilterSeason      = value; break;
            }
        }
    }
}
