using Moq;
using Xunit;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using TMRazorImproved.Shared.Messages;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System;

namespace TMRazorImproved.Tests.MockTests.Agents
{
    public class VendorBuyIntegrationTests
    {
        private readonly Mock<IPacketService> _packetServiceMock = new();
        private readonly Mock<IConfigService> _configServiceMock = new();
        private readonly Mock<IWorldService> _worldServiceMock = new();
        private readonly Mock<ILogger<VendorService>> _loggerMock = new();
        private readonly IMessenger _messenger = new StrongReferenceMessenger();
        private readonly UserProfile _profile = new();

        public VendorBuyIntegrationTests()
        {
            _configServiceMock.Setup(c => c.CurrentProfile).Returns(_profile);
        }

        [Fact]
        public void VendorBuy_ShouldSendCorrectBuyRequest()
        {
            // Arrange
            var vendorSerial = 0x12345678u;
            var vendorContainerSerial = 0x87654321u;
            var itemSerial = 0x99999999u;
            var itemGraphic = (ushort)0x0EED; // Gold

            var service = new VendorService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _messenger,
                _loggerMock.Object);

            // Configure profile to buy gold
            var buyList = new VendorConfig
            {
                Name = "Test",
                BuyEnabled = true
            };
            buyList.BuyList.Add(new LootItem(itemGraphic, 100, "Gold"));
            _profile.VendorLists.Add(buyList);
            _profile.ActiveVendorList = "Test";

            // Simula che il container sia stato aperto e contenga l'item
            _worldServiceMock.Setup(w => w.LastOpenedContainer).Returns(vendorContainerSerial);
            var vendorItems = new List<Item>
            {
                new Item(itemSerial) { Graphic = itemGraphic, Amount = 500, Container = vendorContainerSerial }
            };
            _worldServiceMock.Setup(w => w.GetItemsInContainer(vendorContainerSerial)).Returns(vendorItems);

            // Act
            // Invia il messaggio che simula la ricezione del pacchetto 0x74 (Buy Window) dal server
            var buyMessage = new VendorBuyMessage(vendorSerial, new List<(uint, string)>());
            _messenger.Send(buyMessage);

            // Assert
            // Verifica che sia stato inviato il pacchetto 0x3B (Buy Response) al server
            _packetServiceMock.Verify(p => p.SendToServer(It.Is<byte[]>(b => 
                b.Length > 0 && 
                b[0] == 0x3B && 
                BitConverter.ToUInt32(b.Skip(3).Take(4).Reverse().ToArray(), 0) == vendorSerial && // Vendor Serial (Big Endian check)
                BitConverter.ToUInt32(b.Skip(9).Take(4).Reverse().ToArray(), 0) == itemSerial     // Item Serial (Big Endian check)
            )), Times.Once);
        }

        [Fact]
        public void VendorBuy_ShouldNotBuyIfDisabled()
        {
            // Arrange
            var vendorSerial = 0x12345678u;
            var service = new VendorService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _messenger,
                _loggerMock.Object);

            _profile.VendorLists.Add(new VendorConfig { Name = "Test", BuyEnabled = false });

            // Act
            _messenger.Send(new VendorBuyMessage(vendorSerial, new List<(uint, string)>()));

            // Assert
            _packetServiceMock.Verify(p => p.SendToServer(It.IsAny<byte[]>()), Times.Never);
        }
    }
}
