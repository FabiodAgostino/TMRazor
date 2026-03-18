using System.Collections.Generic;

namespace TMRazorImproved.Core.Utilities
{
    public static class ItemDataHelper
    {
        public record StaticWallData(ushort NewItemID, ushort NewHue, string Label);

        public static readonly Dictionary<ushort, StaticWallData> AnimatedWallToStatic = new()
        {
            // Wall of Stone
            { 0x0080, new StaticWallData(0x0750, 0x03B1, "[Wall Of Stone]") },
            { 0x0082, new StaticWallData(0x0750, 0x03B1, "[Wall Of Stone]") },
            
            // Fire Field
            { 0x3996, new StaticWallData(0x28A8, 0x0845, "[Fire Field]") },
            { 0x398C, new StaticWallData(0x28A8, 0x0845, "[Fire Field]") },
            
            // Poison Field
            { 0x3915, new StaticWallData(0x28A8, 0x016A, "[Poison Field]") },
            { 0x3920, new StaticWallData(0x28A8, 0x016A, "[Poison Field]") },
            { 0x3922, new StaticWallData(0x28A8, 0x016A, "[Poison Field]") },
            
            // Paralyze Field
            { 0x3967, new StaticWallData(0x28A8, 0x00DA, "[Paralyze Field]") },
            { 0x3979, new StaticWallData(0x28A8, 0x00DA, "[Paralyze Field]") },
            
            // Energy Field
            { 0x3946, new StaticWallData(0x28A8, 0x0125, "[Energy Field]") },
            { 0x3956, new StaticWallData(0x28A8, 0x0125, "[Energy Field]") }
        };
    }
}
