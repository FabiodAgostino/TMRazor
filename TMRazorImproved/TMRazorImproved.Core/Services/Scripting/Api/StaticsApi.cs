using System.Collections.Generic;
using TMRazorImproved.Shared.Interfaces;

namespace TMRazorImproved.Core.Services.Scripting.Api
{
    public record StaticTile(int X, int Y, int Z, int Graphic, int Hue);

    public class StaticsApi
    {
        private readonly ScriptCancellationController _cancel;

        public StaticsApi(ScriptCancellationController cancel)
        {
            _cancel = cancel;
        }

        public virtual int GetStaticsGraphic(int x, int y, int map) => 0;
        public virtual List<StaticTile> GetStaticsTileInfo(int x, int y, int map) 
            => new List<StaticTile>();
            
        public virtual int GetLandGraphic(int x, int y, int map) => 0;
        public virtual int GetLandZ(int x, int y, int map) => 0;
    }
}
