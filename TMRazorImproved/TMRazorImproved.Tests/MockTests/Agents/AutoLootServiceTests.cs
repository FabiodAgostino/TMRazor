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
using System.Collections.Generic;
using System.Linq;

namespace TMRazorImproved.Tests.MockTests.Agents
{
    public class AutoLootServiceTests
    {
        private readonly Mock<IPacketService> _packetServiceMock = new();
        private readonly Mock<IConfigService> _configServiceMock = new();
        private readonly Mock<IWorldService> _worldServiceMock = new();
        private readonly Mock<IHotkeyService> _hotkeyServiceMock = new();
        private readonly Mock<ILogger<AutoLootService>> _loggerMock = new();
        private readonly IMessenger _messenger = new StrongReferenceMessenger();
        private readonly UserProfile _profile = new();

        public AutoLootServiceTests()
        {
            _configServiceMock.Setup(c => c.CurrentProfile).Returns(_profile);
            _worldServiceMock.Setup(w => w.Player).Returns(new Mobile(0x123) { Name = "TestPlayer" });
        }

        [Fact]
        public void Start_ShouldSetIsRunning()
        {
            var service = new AutoLootService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _messenger,
                _hotkeyServiceMock.Object,
                _loggerMock.Object);

            service.Start();
            Assert.True(service.IsRunning);
            service.Stop();
            Assert.False(service.IsRunning);
        }

        [Fact]
        public async Task HandleContainerContent_ShouldEnqueueAndLootItems()
        {
            // Arrange
            var service = new AutoLootService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _messenger,
                _hotkeyServiceMock.Object,
                _loggerMock.Object);

            _profile.AutoLootLists[0].Enabled = true;
            _profile.AutoLootLists[0].Container = 0x44444444;
            _profile.AutoLootLists[0].ItemList.Add(new LootItem(0x0EED, -1, "Gold")); // Gold

            service.Start();

            // 0x3C Packet (Container Content)
            byte[] packet = new byte[24];
            packet[0] = 0x3C;
            // Length (2 bytes) - Big Endian
            packet[1] = 0; packet[2] = 24;
            // Count = 1 (2 bytes) - Big Endian
            packet[3] = 0; packet[4] = 1;
            // Item Serial = 0x11111111 (4 bytes)
            packet[5] = 0x11; packet[6] = 0x11; packet[7] = 0x11; packet[8] = 0x11;
            // Graphic = 0x0EED (2 bytes)
            packet[9] = 0x0E; packet[10] = 0xED;
            // Skip 1 byte (0)
            packet[11] = 0;
            // Amount = 100 (2 bytes)
            packet[12] = 0; packet[13] = 100;
            // X, Y (2+2 bytes) - ignored
            // Container Serial = 0x22222222 (4 bytes)
            packet[18] = 0x22; packet[19] = 0x22; packet[20] = 0x22; packet[21] = 0x22;
            // Hue (2 bytes) - ignored

            // Act
            _messenger.Send(new UOPacketMessage(PacketPath.ServerToClient, new UOPacket(packet)));

            // Wait for AgentLoop to process
            await Task.Delay(200);

            // Assert
            // MoveItem calls SendToServer(0x07) and SendToServer(0x08)
            _packetServiceMock.Verify(p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0x07)), Times.Once);
            _packetServiceMock.Verify(p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0x08)), Times.AtLeastOnce);
            
            service.Stop();
        }

        [Fact]
        public async Task HandleSingleItem_ShouldEnqueueAndLootItem()
        {
            // Arrange
            var service = new AutoLootService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _messenger,
                _hotkeyServiceMock.Object,
                _loggerMock.Object);

            _profile.AutoLootLists[0].Enabled = true;
            _profile.AutoLootLists[0].Container = 0x44444444;
            _profile.AutoLootLists[0].ItemList.Add(new LootItem(0x1F03, -1, "Robe")); // Robe

            service.Start();

            // 0x25 Packet (Single Item)
            byte[] packet = new byte[21];
            packet[0] = 0x25;
            // Serial = 0x99999999
            packet[1] = 0x99; packet[2] = 0x99; packet[3] = 0x99; packet[4] = 0x99;
            // Graphic = 0x1F03
            packet[5] = 0x1F; packet[6] = 0x03;
            // Amount = 1
            packet[8] = 0; packet[9] = 1;
            // Container = 0x88888888
            packet[14] = 0x88; packet[15] = 0x88; packet[16] = 0x88; packet[17] = 0x88;

            // Act
            _messenger.Send(new UOPacketMessage(PacketPath.ServerToClient, new UOPacket(packet)));

            // Wait for AgentLoop to process
            await Task.Delay(200);

            // Assert
            _packetServiceMock.Verify(p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0x07)), Times.Once);
            _packetServiceMock.Verify(p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0x08)), Times.AtLeastOnce);
            
            service.Stop();
        }

        [Fact]
        public async Task ShouldNotLootDuplicateSerials()
        {
            // Arrange
            var service = new AutoLootService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _messenger,
                _hotkeyServiceMock.Object,
                _loggerMock.Object);

            _profile.AutoLootLists[0].Enabled = true;
            _profile.AutoLootLists[0].Container = 0x44444444;
            _profile.AutoLootLists[0].ItemList.Add(new LootItem(0x0EED, -1, "Gold"));

            service.Start();

            byte[] packet = new byte[21];
            packet[0] = 0x25;
            packet[1] = 0x99; packet[2] = 0x99; packet[3] = 0x99; packet[4] = 0x99; // Same serial
            packet[5] = 0x0E; packet[6] = 0xED;
            packet[14] = 0x88; packet[15] = 0x88; packet[16] = 0x88; packet[17] = 0x88;

            // Act
            _messenger.Send(new UOPacketMessage(PacketPath.ServerToClient, new UOPacket(packet)));
            _messenger.Send(new UOPacketMessage(PacketPath.ServerToClient, new UOPacket(packet))); // Second time same serial

            // Wait
            await Task.Delay(200);

            // Assert
            _packetServiceMock.Verify(p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0x07)), Times.Once);
            
            service.Stop();
        }
    }
}
