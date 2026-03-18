using Moq;
using Xunit;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using TMRazorImproved.Shared.Messages;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Enums;
using System.Threading.Tasks;
using System.Linq;

namespace TMRazorImproved.Tests.MockTests.Agents
{
    public class ScavengerServiceTests
    {
        private readonly Mock<IPacketService> _packetServiceMock = new();
        private readonly Mock<IConfigService> _configServiceMock = new();
        private readonly Mock<IWorldService> _worldServiceMock = new();
        private readonly Mock<IDragDropCoordinator> _dragDropCoordinatorMock = new();
        private readonly Mock<IHotkeyService> _hotkeyServiceMock = new();
        private readonly Mock<ILogger<ScavengerService>> _loggerMock = new();
        private readonly IMessenger _messenger = new StrongReferenceMessenger();
        private readonly UserProfile _profile = new();
        private readonly Mobile _player = new Mobile(0x123) { X = 100, Y = 100 };

        public ScavengerServiceTests()
        {
            _configServiceMock.Setup(c => c.CurrentProfile).Returns(_profile);
            _worldServiceMock.Setup(w => w.Player).Returns(_player);
            
            _dragDropCoordinatorMock.Setup(d => d.RequestDragDrop(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<ushort>()))
                                    .ReturnsAsync(true);
        }

        [Fact]
        public async Task HandleWorldItem_ShouldEnqueueAndScavenge_WhenInRange()
        {
            // Arrange
            var service = new ScavengerService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _dragDropCoordinatorMock.Object,
                _messenger,
                _hotkeyServiceMock.Object,
                _loggerMock.Object);

            _profile.ScavengerLists[0].Enabled = true;
            _profile.ScavengerLists[0].Container = 0x44444444;
            _profile.ScavengerLists[0].Range = 2;
            _profile.ScavengerLists[0].ItemList.Add(new LootItem(0x0EED, -1, "Gold")); // Gold

            var item = new Item(0x41111111) { Graphic = 0x0EED, X = 101, Y = 101 };
            var message = new WorldItemMessage(item);

            service.Start();

            // Act
            _messenger.Send(message);
            await Task.Delay(500);

            // Assert
            _dragDropCoordinatorMock.Verify(d => d.RequestDragDrop(0x41111111, 0x44444444, It.IsAny<ushort>()), Times.Once);
            
            service.Stop();
        }

        [Fact]
        public async Task ShouldNotScavenge_WhenOutOfRange()
        {
            // Arrange
            var service = new ScavengerService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _dragDropCoordinatorMock.Object,
                _messenger,
                _hotkeyServiceMock.Object,
                _loggerMock.Object);

            _profile.ScavengerLists[0].Enabled = true;
            _profile.ScavengerLists[0].Container = 0x44444444;
            _profile.ScavengerLists[0].Range = 2;
            _profile.ScavengerLists[0].ItemList.Add(new LootItem(0x0EED, -1, "Gold"));

            var item = new Item(0x41111111) { Graphic = 0x0EED, X = 110, Y = 110 };
            var message = new WorldItemMessage(item);

            service.Start();

            // Act
            _messenger.Send(message);
            await Task.Delay(500);

            // Assert
            _dragDropCoordinatorMock.Verify(d => d.RequestDragDrop(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<ushort>()), Times.Never);
            
            service.Stop();
        }
    }
}
