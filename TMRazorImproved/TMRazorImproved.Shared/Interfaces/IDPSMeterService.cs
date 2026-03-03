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

        void Start();
        void Stop();
        void Reset();

        event Action? Updated;
    }
}
