using System.Collections.Generic;

namespace TMRazorImproved.Shared.Interfaces
{
    /// <summary>
    /// Tracks multi structures (houses and boats) in the game world.
    /// Used to determine if a tile coordinate is inside a placed multi structure.
    /// </summary>
    public interface IMultiService
    {
        /// <summary>Adds or updates a multi entry computed from its graphic bounding box.</summary>
        void AddMulti(uint serial, int x, int y, ushort graphic);

        /// <summary>Removes a multi from tracking.</summary>
        void RemoveMulti(uint serial);

        /// <summary>Returns true if the tile (x, y) falls within any tracked multi's bounding box.</summary>
        bool CheckDeedHouse(int x, int y);

        /// <summary>Clears all tracked multis (e.g. on logout).</summary>
        void Clear();
    }
}
