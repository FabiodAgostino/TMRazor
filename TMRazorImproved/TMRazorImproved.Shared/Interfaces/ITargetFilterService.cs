using System.Collections.Generic;
using TMRazorImproved.Shared.Models.Config;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface ITargetFilterService
    {
        void AddFilter(uint serial, string name);
        void AddAllMobiles();
        void AddAllHumanoids();
        bool IsFiltered(uint serial);
        void RemoveFilter(uint serial);
        void ClearAll();
        IReadOnlyList<TargetFilterEntry> Filters { get; }
    }
}
