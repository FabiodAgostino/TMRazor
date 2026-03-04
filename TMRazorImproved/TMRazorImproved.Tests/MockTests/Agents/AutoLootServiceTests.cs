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

            var items = new List<Item> { new Item(0x11111111) { Graphic = 0x0EED, Amount = 100 } };
            var message = new ContainerContentMessage(0x22222222, items);

            service.Start();

            // Act
            _messenger.Send(message);

            // Wait for AgentLoop to process
            await Task.Delay(500);

            // Assert
            _packetServiceMock.Verify(p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0x07)), Times.Once);
            
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

            var item = new Item(0x99999999) { Graphic = 0x1F03, Amount = 1 };
            _worldServiceMock.Setup(w => w.FindItem(0x99999999)).Returns(item);
            var message = new ContainerItemAddedMessage(0x88888888, 0x99999999);

            service.Start();

            // Act
            _messenger.Send(message);

            // Wait for AgentLoop to process
            await Task.Delay(500);

            // Assert
            _packetServiceMock.Verify(p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0x07)), Times.Once);
            
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

            var item = new Item(0x99999999) { Graphic = 0x0EED, Amount = 1 };
            _worldServiceMock.Setup(w => w.FindItem(0x99999999)).Returns(item);
            var message = new ContainerItemAddedMessage(0x88888888, 0x99999999);

            service.Start();

            // Act
            _messenger.Send(message);
            _messenger.Send(message); // Send again same serial

            // Wait
            await Task.Delay(500);

            // Assert
            _packetServiceMock.Verify(p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0x07)), Times.Once);
            
            service.Stop();
        }
    }
}
