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
        private readonly Mock<IHotkeyService> _hotkeyServiceMock = new();
        private readonly Mock<ILogger<ScavengerService>> _loggerMock = new();
        private readonly IMessenger _messenger = new StrongReferenceMessenger();
        private readonly UserProfile _profile = new();
        private readonly Mobile _player = new Mobile(0x123) { X = 100, Y = 100 };

        public ScavengerServiceTests()
        {
            _configServiceMock.Setup(c => c.CurrentProfile).Returns(_profile);
            _worldServiceMock.Setup(w => w.Player).Returns(_player);
        }

        [Fact]
        public async Task HandleWorldItem_ShouldEnqueueAndScavenge_WhenInRange()
        {
            // Arrange
            var service = new ScavengerService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _messenger,
                _hotkeyServiceMock.Object,
                _loggerMock.Object);

            _profile.ScavengerLists[0].Enabled = true;
            _profile.ScavengerLists[0].Container = 0x44444444;
            _profile.ScavengerLists[0].Range = 2;
            _profile.ScavengerLists[0].ItemList.Add(new LootItem(0x0EED, -1, "Gold")); // Gold

            service.Start();

            // 0x1A Packet (World Item)
            byte[] packet = new byte[20];
            packet[0] = 0x1A;
            // Length
            packet[1] = 0; packet[2] = 20;
            // Serial = 0x41111111 (Item bit set)
            packet[3] = 0x41; packet[4] = 11; packet[5] = 11; packet[6] = 11;
            // Graphic = 0x0EED
            packet[7] = 0x0E; packet[8] = 0xED;
            // X = 101, Y = 101 (Dist 1)
            packet[9] = 0; packet[10] = 101;
            packet[11] = 0; packet[12] = 101;
            // Z = 0
            packet[13] = 0;

            // Act
            _messenger.Send(new UOPacketMessage(PacketPath.ServerToClient, new UOPacket(packet)));
            await Task.Delay(200);

            // Assert
            _packetServiceMock.Verify(p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0x07)), Times.Once);
            _packetServiceMock.Verify(p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0x08)), Times.AtLeastOnce);
            
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
                _messenger,
                _hotkeyServiceMock.Object,
                _loggerMock.Object);

            _profile.ScavengerLists[0].Enabled = true;
            _profile.ScavengerLists[0].Container = 0x44444444;
            _profile.ScavengerLists[0].Range = 2;
            _profile.ScavengerLists[0].ItemList.Add(new LootItem(0x0EED, -1, "Gold"));

            service.Start();

            // 0x1A Packet (World Item) - X=110, Y=110 (Dist 10)
            byte[] packet = new byte[20];
            packet[0] = 0x1A;
            packet[1] = 0; packet[2] = 20;
            packet[3] = 0x41; packet[4] = 11; packet[5] = 11; packet[6] = 11;
            packet[7] = 0x0E; packet[8] = 0xED;
            packet[9] = 0; packet[10] = 110;
            packet[11] = 0; packet[12] = 110;

            // Act
            _messenger.Send(new UOPacketMessage(PacketPath.ServerToClient, new UOPacket(packet)));
            await Task.Delay(200);

            // Assert
            _packetServiceMock.Verify(p => p.SendToServer(It.IsAny<byte[]>()), Times.Never);
            
            service.Stop();
        }
    }
}
