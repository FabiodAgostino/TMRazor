using System;
using System.Collections.Generic;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface ICounterService
    {
        int GetCount(ushort graphic, ushort hue = 0);
        void RecalculateAll();
        
        event Action<ushort, ushort, int>? CounterChanged;
    }
}
