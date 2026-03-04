using Moq;
using Xunit;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TMRazorImproved.Tests.MockTests.Agents
{
    public class OrganizerServiceTests
    {
        private readonly Mock<IPacketService> _packetServiceMock = new();
        private readonly Mock<IConfigService> _configServiceMock = new();
        private readonly Mock<IWorldService> _worldServiceMock = new();
        private readonly Mock<IHotkeyService> _hotkeyServiceMock = new();
        private readonly Mock<ILogger<OrganizerService>> _loggerMock = new();
        private readonly UserProfile _profile = new();

        public OrganizerServiceTests()
        {
            _configServiceMock.Setup(c => c.CurrentProfile).Returns(_profile);
            _worldServiceMock.Setup(w => w.Player).Returns(new Mobile(0x123));
        }

        [Fact]
        public async Task Start_ShouldMoveItemsFromSourceToDest()
        {
            // Arrange
            var sourceSerial = 0x11111111u;
            var destSerial = 0x22222222u;
            var itemSerial = 0x33333333u;

            var config = _profile.OrganizerLists[0];
            config.Enabled = true;
            config.Source = sourceSerial;
            config.Destination = destSerial;
            config.Delay = 50;
            _profile.ActiveOrganizerList = config.Name;

            var items = new List<Item>
            {
                new Item(itemSerial) { Container = sourceSerial, Graphic = 0x0EED, Amount = 100 }
            };
            _worldServiceMock.Setup(w => w.Items).Returns(items);

            var service = new OrganizerService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _hotkeyServiceMock.Object,
                _loggerMock.Object);

            // Act
            service.Start();
            
            // Aspetta che il loop finisca (l'Organizer finisce da solo se non ci sono più item o viene stoppato)
            // In questo caso, AgentLoopAsync finisce dopo il foreach.
            int timeout = 0;
            while (service.IsRunning && timeout < 20)
            {
                await Task.Delay(50);
                timeout++;
            }

            // Assert
            // 0x07 = Lift, 0x08 = Drop
            _packetServiceMock.Verify(p => p.SendToServer(It.Is<byte[]>(b => b[0] == 0x07)), Times.Once);
            _packetServiceMock.Verify(p => p.SendToServer(It.Is<byte[]>(b => b[0] == 0x08)), Times.Once);
            
            service.Stop();
        }

        [Fact]
        public async Task ShouldNotMoveItemsIfDisabled()
        {
            // Arrange
            var config = _profile.OrganizerLists[0];
            config.Enabled = false;
            _profile.ActiveOrganizerList = config.Name;

            var service = new OrganizerService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _hotkeyServiceMock.Object,
                _loggerMock.Object);

            // Act
            service.Start();
            await Task.Delay(200);

            // Assert
            _packetServiceMock.Verify(p => p.SendToServer(It.IsAny<byte[]>()), Times.Never);
            service.Stop();
        }

        [Fact]
        public async Task ShouldHandleNullProfileGracefully()
        {
            // Arrange
            _configServiceMock.Setup(c => c.CurrentProfile).Returns((UserProfile)null);

            var service = new OrganizerService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _hotkeyServiceMock.Object,
                _loggerMock.Object);

            // Act
            service.Start();
            await Task.Delay(200);

            // Assert
            _packetServiceMock.Verify(p => p.SendToServer(It.IsAny<byte[]>()), Times.Never);
            service.Stop();
        }
    }
}
