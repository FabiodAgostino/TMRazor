using Moq;
using Xunit;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Messages;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using System;

namespace TMRazorImproved.Tests.MockTests.Networking
{
    public class PacketServiceTests
    {
        private readonly Mock<IMessenger> _messengerMock = new();
        private readonly Mock<IClientInteropService> _interopMock = new();
        private readonly Mock<ILogger<PacketService>> _loggerMock = new();

        [Fact]
        public void RegisterViewer_ShouldInvokeCallback_WhenPacketReceived()
        {
            // Arrange
            var service = new PacketService(_messengerMock.Object, _interopMock.Object, _loggerMock.Object);
            bool called = false;
            byte[] packet = { 0x1A, 0x00, 0x05, 0x11, 0x22 };
            
            service.RegisterViewer(PacketPath.ServerToClient, 0x1A, data => 
            {
                called = true;
                Assert.Equal(packet, data);
            });

            // Act
            bool result = service.OnPacketReceived(PacketPath.ServerToClient, packet);

            // Assert
            Assert.True(result);
            Assert.True(called);
            _messengerMock.Verify(m => m.Send(It.IsAny<UOPacketMessage>(), It.IsAny<int>()), Times.AtMostOnce());
        }

        [Fact]
        public void RegisterFilter_ShouldBlockPacket_WhenReturnsFalse()
        {
            // Arrange
            var service = new PacketService(_messengerMock.Object, _interopMock.Object, _loggerMock.Object);
            byte[] packet = { 0xBC, 0x00, 0x01 }; // Season packet
            
            service.RegisterFilter(PacketPath.ServerToClient, 0xBC, data => false); // Block it

            // Act
            bool result = service.OnPacketReceived(PacketPath.ServerToClient, packet);

            // Assert
            Assert.False(result);
            // Se filtrato, il messenger NON deve essere notificato
            _messengerMock.Verify(m => m.Send(It.IsAny<UOPacketMessage>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void MultipleFilters_ShouldAllBeExecuted_UnlessOneReturnsFalse()
        {
            // Arrange
            var service = new PacketService(_messengerMock.Object, _interopMock.Object, _loggerMock.Object);
            byte[] packet = { 0x1A, 0x00, 0x05 };
            int filter1Called = 0;
            int filter2Called = 0;

            service.RegisterFilter(PacketPath.ServerToClient, 0x1A, data => { filter1Called++; return true; });
            service.RegisterFilter(PacketPath.ServerToClient, 0x1A, data => { filter2Called++; return false; }); // This one blocks
            service.RegisterFilter(PacketPath.ServerToClient, 0x1A, data => { throw new Exception("Should not be called"); });

            // Act
            bool result = service.OnPacketReceived(PacketPath.ServerToClient, packet);

            // Assert
            Assert.False(result);
            Assert.Equal(1, filter1Called);
            Assert.Equal(1, filter2Called);
        }

        [Fact]
        public void UnregisterViewer_ShouldStopInvokingCallback()
        {
            // Arrange
            var service = new PacketService(_messengerMock.Object, _interopMock.Object, _loggerMock.Object);
            int callCount = 0;
            byte[] packet = { 0x1A, 0x00, 0x05 };
            Action<byte[]> callback = data => callCount++;
            
            service.RegisterViewer(PacketPath.ServerToClient, 0x1A, callback);
            service.OnPacketReceived(PacketPath.ServerToClient, packet);
            Assert.Equal(1, callCount);

            // Act
            service.UnregisterViewer(PacketPath.ServerToClient, 0x1A, callback);
            service.OnPacketReceived(PacketPath.ServerToClient, packet);

            // Assert
            Assert.Equal(1, callCount); // Rimane a 1
        }
    }
}
