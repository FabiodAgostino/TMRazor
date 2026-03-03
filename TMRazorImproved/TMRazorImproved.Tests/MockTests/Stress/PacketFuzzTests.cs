using Moq;
using Xunit;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Enums;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using System;

namespace TMRazorImproved.Tests.MockTests.Stress
{
    public class PacketFuzzTests
    {
        private readonly Mock<IMessenger> _messengerMock = new();
        private readonly Mock<IClientInteropService> _interopMock = new();
        private readonly Mock<ILogger<PacketService>> _packetLoggerMock = new();

        [Fact]
        public void OnPacketReceived_RandomGarbage_ShouldNotCrash()
        {
            // Arrange
            var service = new PacketService(_messengerMock.Object, _interopMock.Object, _packetLoggerMock.Object);
            var rnd = new Random();

            for (int i = 0; i < 1000; i++)
            {
                int len = rnd.Next(1, 500);
                byte[] garbage = new byte[len];
                rnd.NextBytes(garbage);

                // Act & Assert
                // Non deve mai lanciare eccezioni, anche con dati casuali
                service.OnPacketReceived(PacketPath.ServerToClient, garbage);
            }
        }

        [Fact]
        public void OnPacketReceived_EmptyOrNull_ShouldHandleGracefully()
        {
            // Arrange
            var service = new PacketService(_messengerMock.Object, _interopMock.Object, _packetLoggerMock.Object);

            // Act & Assert
            service.OnPacketReceived(PacketPath.ServerToClient, null!);
            service.OnPacketReceived(PacketPath.ServerToClient, new byte[0]);
        }

        [Fact]
        public void OnPacketReceived_MalformedLength_ShouldNotCrash()
        {
            // Arrange
            var service = new PacketService(_messengerMock.Object, _interopMock.Object, _packetLoggerMock.Object);
            
            // Pacchetto che dichiara un ID ma non ha dati sufficienti per un ipotetico parser
            byte[] shortPacket = { 0x3C }; 

            // Act
            service.OnPacketReceived(PacketPath.ServerToClient, shortPacket);
        }
    }
}
