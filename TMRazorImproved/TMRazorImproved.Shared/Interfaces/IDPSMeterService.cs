using System;
using System.Collections.Generic;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IDPSMeterService
    {
        double CurrentDPS { get; }
        double MaxDPS { get; }
        long TotalDamage { get; }
        TimeSpan CombatTime { get; }
        bool IsActive { get; }

        IReadOnlyDictionary<uint, long> TargetDamage { get; }

        bool IsPaused { get; }

        void Start();
        void Stop();
        void Pause();
        void Reset();

        long GetDamage(uint serial);

        event Action? Updated;
    }
}
