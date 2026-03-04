using Moq;
using Xunit;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TMRazorImproved.Tests.MockTests.Agents
{
    public class DressServiceTests
    {
        private readonly Mock<IPacketService>           _packetMock = new();
        private readonly Mock<IConfigService>           _configMock = new();
        private readonly Mock<IWorldService>            _worldMock  = new();
        private readonly Mock<IHotkeyService>           _hotkeyMock = new();
        private readonly Mock<ILogger<DressService>>    _loggerMock = new();

        private readonly UserProfile _profile = new();
        private readonly Mobile      _player  = new Mobile(0xAA) { Name = "Tester" };

        public DressServiceTests()
        {
            _configMock.Setup(c => c.CurrentProfile).Returns(_profile);
            _worldMock.Setup(w => w.Player).Returns(_player);
            _player.Backpack = new Item(0xBB) { Graphic = 0x0E75 };
        }

        private DressService CreateService() =>
            new DressService(
                _packetMock.Object,
                _configMock.Object,
                _worldMock.Object,
                _hotkeyMock.Object,
                _loggerMock.Object);

        private static DressList MakeList(string name, uint itemSerial, byte layer = 0x01)
        {
            var list = new DressList { Name = name, DragDelay = 0 };
            list.LayerItems[layer] = itemSerial;
            return list;
        }

        // ---------------------------------------------------------------
        // FIX BUG-C01: pacchetto equip corretto = 0x13 (WearItem), non 0x05 (Attack)
        // ---------------------------------------------------------------

        [Fact]
        public async Task Dress_ShouldSendLiftAndWearItemPackets()
        {
            // Arrange
            _profile.DressLists.Clear();
            _profile.DressLists.Add(MakeList("Default", 0xDDDD, layer: 0x01));
            _profile.ActiveDressList = "Default";

            var item = new Item(0xDDDD) { Graphic = 0x1515, Layer = 0 };
            _worldMock.Setup(w => w.FindItem(0xDDDD)).Returns(item);

            var service = CreateService();

            // Act — Dress() popola la coda e avvia il loop
            service.Dress("Default");
            await Task.Delay(400);
            await service.StopAsync();

            // Assert: LiftItem (0x07) poi WearItem (0x13)
            _packetMock.Verify(
                p => p.SendToServer(It.Is<byte[]>(b => b.Length == 7 && b[0] == 0x07)),
                Times.AtLeastOnce, "Dovrebbe inviare LiftItem 0x07");

            _packetMock.Verify(
                p => p.SendToServer(It.Is<byte[]>(b => b.Length == 10 && b[0] == 0x13)),
                Times.AtLeastOnce, "Dovrebbe inviare WearItem 0x13");

            _packetMock.Verify(
                p => p.SendToServer(It.Is<byte[]>(b => b.Length == 5 && b[0] == 0x05)),
                Times.Never, "NON deve inviare Attack Request 0x05");
        }

        // ---------------------------------------------------------------
        // Undress → LiftItem (0x07) + DropToContainer (0x08)
        // ---------------------------------------------------------------

        [Fact]
        public async Task Undress_ShouldDropItemToBackpack()
        {
            // Arrange
            _profile.DressLists.Clear();
            _profile.DressLists.Add(MakeList("Default", 0xCCCC, layer: 0x02));
            _profile.ActiveDressList = "Default";

            var item = new Item(0xCCCC) { Graphic = 0x1414, Layer = 2 };
            _worldMock.Setup(w => w.FindItem(0xCCCC)).Returns(item);

            var service = CreateService();

            // Act
            service.Undress("Default");
            await Task.Delay(400);
            await service.StopAsync();

            // Assert: LiftItem (0x07) + DropToContainer (0x08)
            _packetMock.Verify(
                p => p.SendToServer(It.Is<byte[]>(b => b.Length == 7 && b[0] == 0x07)),
                Times.AtLeastOnce, "Dovrebbe inviare LiftItem 0x07");

            _packetMock.Verify(
                p => p.SendToServer(It.Is<byte[]>(b => b.Length == 15 && b[0] == 0x08)),
                Times.AtLeastOnce, "Dovrebbe inviare DropToContainer 0x08");
        }

        // ---------------------------------------------------------------
        // Se la lista è null, nessun pacchetto inviato
        // ---------------------------------------------------------------

        [Fact]
        public void Dress_ShouldDoNothing_WhenListNotFound()
        {
            // Arrange
            _profile.DressLists.Clear();
            var service = CreateService();

            // Act
            service.Dress("NonExistent");

            // Assert: senza lista, la coda rimane vuota — nessun pacchetto
            _packetMock.Verify(p => p.SendToServer(It.IsAny<byte[]>()), Times.Never);
        }
    }
}
