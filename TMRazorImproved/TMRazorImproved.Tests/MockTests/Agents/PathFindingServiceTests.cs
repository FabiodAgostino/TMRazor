using Moq;
using Xunit;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using Microsoft.Extensions.Logging;
using System.Linq;
using Ultima;

namespace TMRazorImproved.Tests.MockTests.Agents
{
    /// <summary>
    /// Sprint Fix-4 — T24: unit test di PathFindingService con IMapDataProvider mockato.
    /// Usano una griglia in memoria (5×5) senza dipendere da file UO reali.
    ///
    /// Convenzione mappa in-memory:
    ///   - Land tile Id = 1 (passabile, Z = 0) per default
    ///   - Land tile Id = 2 (ignorato da Ignored()) — forza check via statici/items
    ///   - LandData.Flags = TileFlag.None  → passabile
    ///   - LandData.Flags = TileFlag.Impassable → bloccato
    ///   - No static tiles per default (array vuoto)
    ///   - No world items per default
    /// </summary>
    public class PathFindingServiceTests
    {
        private readonly Mock<IWorldService> _worldMock = new();
        private readonly Mock<IMapDataProvider> _mapMock = new();
        private readonly Mock<IConfigService> _configMock = new();
        private readonly Mock<ILogger<PathFindingService>> _loggerMock = new();
        private readonly Mobile _player = new Mobile(0x01) { X = 0, Y = 0, Z = 0 };

        public PathFindingServiceTests()
        {
            _worldMock.Setup(w => w.Player).Returns(_player);
            _worldMock.Setup(w => w.Items).Returns(Enumerable.Empty<Item>());
            _worldMock.Setup(w => w.Mobiles).Returns(Enumerable.Empty<Mobile>());

            // Default config
            _configMock.Setup(c => c.CurrentProfile).Returns(new UserProfile());

            // Default map setup: available, flat passable land (Id=1, Z=0)
            _mapMock.Setup(m => m.IsMapAvailable(It.IsAny<int>())).Returns(true);
            _mapMock.Setup(m => m.GetLandTile(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(new Tile(1, 0)); // Id=1 (not ignored), Z=0
            _mapMock.Setup(m => m.GetStaticTiles(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                    .Returns(System.Array.Empty<HuedTile>());
            // LandData: non-impassable, no surface flag (land is handled via considerLand path)
            _mapMock.Setup(m => m.GetLandData(It.IsAny<int>()))
                    .Returns(new LandData { Flags = TileFlag.None });
            // ItemData: no flags, Height=0, CalcHeight=0
            _mapMock.Setup(m => m.GetItemData(It.IsAny<int>()))
                    .Returns(new ItemData("", TileFlag.None, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0));
        }

        private PathFindingService CreateService()
            => new PathFindingService(_worldMock.Object, _mapMock.Object, _configMock.Object, _loggerMock.Object);

        // ---------------------------------------------------------------
        // T24-01: mappa non disponibile → null
        // ---------------------------------------------------------------

        [Fact]
        public void GetPath_ReturnsNull_WhenMapNotAvailable()
        {
            _mapMock.Setup(m => m.IsMapAvailable(99)).Returns(false);
            var svc = CreateService();

            var result = svc.GetPath(0, 0, 0, 3, 0, 99);

            Assert.Null(result);
        }

        // ---------------------------------------------------------------
        // T24-02: start == goal → lista vuota (zero passi richiesti)
        // ---------------------------------------------------------------

        [Fact]
        public void GetPath_ReturnsEmptyList_WhenStartEqualsGoal()
        {
            var svc = CreateService();

            var result = svc.GetPath(5, 5, 0, 5, 5, 0);

            Assert.NotNull(result);
            Assert.Empty(result!);
        }

        // ---------------------------------------------------------------
        // T24-03: percorso rettilineo su terreno pianeggiante
        // ---------------------------------------------------------------

        [Fact]
        public void GetPath_ReturnsStraightPath_OnFlatPassableLand()
        {
            // Land tile Id=1 → not ignored (Ignored checks Id==2 || Id==0x1DB || ...)
            // LandData.Flags = TileFlag.None → not impassable
            // considerLand=true, landBlocks=false → passable via land tile path
            var svc = CreateService();

            var result = svc.GetPath(0, 0, 0, 3, 0, 0);

            Assert.NotNull(result);
            Assert.True(result!.Count > 0, "Path should have steps");
            Assert.Equal((3, 0), result.Last()); // last step = goal
            Assert.Equal((0, 0), result.First()); // first step = start
        }

        // ---------------------------------------------------------------
        // T24-04: percorso bloccato da terra impassabile → null
        // ---------------------------------------------------------------

        [Fact]
        public void GetPath_ReturnsNull_WhenAllNeighborsImpassable()
        {
            // Make land impassable everywhere except start tile
            _mapMock.Setup(m => m.GetLandData(It.IsAny<int>()))
                    .Returns(new LandData { Flags = TileFlag.Impassable });

            // Start tile's land is also impassable, but start has Z=0
            // No static tiles → no alternative surface → Check() returns false → BigCost
            var svc = CreateService();

            var result = svc.GetPath(0, 0, 0, 5, 5, 0);

            Assert.Null(result);
        }

        // ---------------------------------------------------------------
        // T24-05: verifica BUG-P1-01 — goal NON duplicato nel path
        // ---------------------------------------------------------------

        [Fact]
        public void GetPath_GoalAppearsExactlyOnce_NoBugP101Regression()
        {
            var svc = CreateService();
            var goal = (X: 2, Y: 0);

            var result = svc.GetPath(0, 0, 0, goal.X, goal.Y, 0);

            Assert.NotNull(result);
            int goalCount = result!.Count(p => p.X == goal.X && p.Y == goal.Y);
            Assert.Equal(1, goalCount);
        }

        // ---------------------------------------------------------------
        // T24-06: ignoreDoors=true — item con TileFlag.Door è attraversabile
        // ---------------------------------------------------------------

        [Fact]
        public void GetPath_PassesThroughDoor_WhenIgnoreDoorsIsTrue()
        {
            // Aggiungi un "item porta" alla posizione (1,0)
            var doorItem = new Item(0xDEAD)
            {
                Graphic = 0x692, // grafica porta tipica UO
                X = 1, Y = 0, Z = 0, Container = 0, Layer = 0
            };
            _worldMock.Setup(w => w.Items).Returns(new[] { doorItem });

            // ItemData per la porta: Impassable + Surface + Door
            var doorData = new ItemData("door", TileFlag.Impassable | TileFlag.Surface | TileFlag.Door, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0);
            _mapMock.Setup(m => m.GetItemData((int)0x692)).Returns(doorData);

            var svc = CreateService();

            // ignoreDoors=true: la porta non blocca il percorso
            var resultIgnore = svc.GetPath(0, 0, 0, 2, 0, 0, ignoreDoors: true);
            Assert.NotNull(resultIgnore);
        }
    }
}
