using System;
using System;
using System.Collections.Generic;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Shared.Interfaces
{
    public class SkillGainRecord
    {
        public DateTime Timestamp { get; }
        public string SkillName { get; }
        public double OldValue { get; }
        public double NewValue { get; }
        public double Gain => NewValue - OldValue;

        public SkillGainRecord(DateTime timestamp, string skillName, double oldValue, double newValue)
        {
            Timestamp = timestamp;
            SkillName = skillName;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public interface ISkillsService
    {
        IReadOnlyList<SkillInfo> Skills { get; }
        IReadOnlyList<SkillGainRecord> GainHistory { get; }

        double TotalReal { get; }
        double TotalBase { get; }

        void ResetDelta();
        void SetLock(int skillId, SkillLock lockType);
        void LoadNamesFromDataPath(string dataPath);
    }
}
