using Moq;
using Xunit;
using TMRazorImproved.Core.Handlers;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Models;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace TMRazorImproved.Tests.MockTests.Networking
{
    public class WorldPacketHandlerTests
    {
        private readonly Mock<IPacketService> _packetServiceMock = new();
        private readonly Mock<IWorldService> _worldServiceMock = new();
        private readonly Mock<IJournalService> _journalServiceMock = new();
        private readonly Mock<ILanguageService> _languageServiceMock = new();
        private readonly Mock<IFriendsService> _friendsServiceMock = new();
        private readonly Mock<IConfigService> _configServiceMock = new();
        private readonly Mock<IScreenCaptureService> _screenCaptureMock = new();
        private readonly Mock<ITargetingService> _targetingServiceMock = new();
        private readonly Mock<IMultiService> _multiServiceMock = new();
        private readonly Mock<IMessenger> _messengerMock = new();
        private readonly WorldPacketHandler _handler;

        public WorldPacketHandlerTests()
        {
            _handler = new WorldPacketHandler(
                _packetServiceMock.Object,
                _worldServiceMock.Object,
                _journalServiceMock.Object,
                _languageServiceMock.Object,
                _friendsServiceMock.Object,
                _configServiceMock.Object,
                _screenCaptureMock.Object,
                _targetingServiceMock.Object,
                _multiServiceMock.Object,
                _messengerMock.Object);
        }

        [Fact]
        public void HandleMobileIncoming_ShouldAddMobileToWorld()
        {
            // Arrange
            byte[] packet = new byte[30];
            packet[0] = 0x78;
            // length
            packet[1] = 0; packet[2] = 30;
            // serial 0x12345678
            packet[3] = 0x12; packet[4] = 0x34; packet[5] = 0x56; packet[6] = 0x78;
            // body 0x0190 (Male)
            packet[7] = 0x01; packet[8] = 0x90;
            // X=1000, Y=2000
            packet[9] = 0x03; packet[10] = 0xE8;
            packet[11] = 0x07; packet[12] = 0xD0;

            Mobile? capturedMobile = null;
            _worldServiceMock.Setup(w => w.AddMobile(It.IsAny<Mobile>()))
                             .Callback<Mobile>(m => capturedMobile = m);

            // Act
            // Troviamo la callback registrata per 0x78 ed eseguiamola
            var callback = ExtractFilterCallback(0x78);
            callback(packet);

            // Assert
            Assert.NotNull(capturedMobile);
            Assert.Equal(0x12345678u, capturedMobile.Serial);
            Assert.Equal(0x0190, capturedMobile.Graphic);
            Assert.Equal(1000, capturedMobile.X);
            Assert.Equal(2000, capturedMobile.Y);
        }

        [Fact]
        public void HandleMobileStatus_ShouldUpdatePlayerStats()
        {
            // Arrange
            uint playerSerial = 0x11223344;
            var player = new Mobile(playerSerial);
            _worldServiceMock.Setup(w => w.Player).Returns(player);
            _worldServiceMock.Setup(w => w.FindMobile(playerSerial)).Returns(player);

            byte[] packet = new byte[66];
            packet[0] = 0x11;
            packet[1] = 0; packet[2] = 66;
            // serial
            packet[3] = 0x11; packet[4] = 0x22; packet[5] = 0x33; packet[6] = 0x44;
            // name (30 bytes)
            // hits=100, max=100
            packet[37] = 0; packet[38] = 100;
            packet[39] = 0; packet[40] = 100;
            // type=1 (stats complete)
            packet[42] = 1;
            // offset 43: isFemale (byte)
            packet[43] = 0;
            // offset 44: Str=120 (ushort)
            packet[44] = 0; packet[45] = 120;
            // offset 46: Dex=100 (ushort)
            packet[46] = 0; packet[47] = 100;
            // offset 48: Int=100 (ushort)
            packet[48] = 0; packet[49] = 100;

            // Act
            var callback = ExtractCallback(0x11);
            callback(packet);

            // Assert
            Assert.Equal(100, player.Hits);
            Assert.Equal(120, player.Str);
            Assert.Equal(100, player.Dex);
        }

        private Action<byte[]> ExtractCallback(int packetId)
        {
            _packetServiceMock.Verify(p => p.RegisterViewer(PacketPath.ServerToClient, packetId, It.IsAny<Action<byte[]>>()));
            
            // Dobbiamo estrarre l'argomento passato alla chiamata RegisterViewer
            var invocation = _packetServiceMock.Invocations.First(i => i.Method.Name == "RegisterViewer" && (int)i.Arguments[1] == packetId);
            return (Action<byte[]>)invocation.Arguments[2];
        }

        private Func<byte[], bool> ExtractFilterCallback(int packetId)
        {
            _packetServiceMock.Verify(p => p.RegisterFilter(PacketPath.ServerToClient, packetId, It.IsAny<Func<byte[], bool>>()));
            
            var invocation = _packetServiceMock.Invocations.First(i => i.Method.Name == "RegisterFilter" && (int)i.Arguments[1] == packetId);
            return (Func<byte[], bool>)invocation.Arguments[2];
        }
    }
}
