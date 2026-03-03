using System;
using System.Collections.Generic;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface ISkillsService
    {
        IReadOnlyList<SkillInfo> Skills { get; }
        double TotalReal { get; }
        double TotalBase { get; }
        void ResetDelta();
        void SetLock(int skillId, SkillLock lockType);
    }
}
