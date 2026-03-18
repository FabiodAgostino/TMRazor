using System.Collections.Generic;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IWeaponService
    {
        bool IsTwoHanded(ushort graphic);
        WeaponInfo? GetWeaponInfo(ushort graphic);
    }
}
