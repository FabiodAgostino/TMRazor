using System.Collections.Generic;
using TMRazorImproved.Shared.Models;

namespace TMRazorImproved.Shared.Interfaces
{
    public interface IPathFindingService
    {
        /// <summary>
        /// Gets the path from a starting location to a destination.
        /// </summary>
        /// <param name="startX">Starting X coordinate.</param>
        /// <param name="startY">Starting Y coordinate.</param>
        /// <param name="startZ">Starting Z coordinate.</param>
        /// <param name="destX">Destination X coordinate.</param>
        /// <param name="destY">Destination Y coordinate.</param>
        /// <param name="mapId">Map ID.</param>
        /// <param name="ignoreMobiles">Whether to ignore mobiles (living entities) during pathfinding.</param>
        /// <param name="ignoreDoors">Whether to ignore door tiles during pathfinding (useful for script navigation).</param>
        /// <returns>A list of coordinates representing the path, or null if no path could be found.</returns>
        List<(int X, int Y)>? GetPath(int startX, int startY, int startZ, int destX, int destY, int mapId, bool ignoreMobiles = false, bool ignoreDoors = false);
    }
}