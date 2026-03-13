using Moq;
using Xunit;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Messages;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace TMRazorImproved.Tests.MockTests.Stress
{
    public class ConcurrencyStressTests
    {
        private readonly Mock<IMessenger> _messengerMock = new();
        private readonly Mock<IClientInteropService> _interopMock = new();
        private readonly Mock<ILogger<PacketService>> _packetLoggerMock = new();
        
        [Fact]
        public async Task WorldService_MassiveConcurrency_ShouldNotCrash()
        {
            // Arrange
            var world = new WorldService(_messengerMock.Object);
            int numThreads = 10;
            int numEntities = 500;
            int numIterations = 1000;
            
            var tasks = new List<Task>();

            // Thread di scrittura (Aggiornano il mondo)
            for (int t = 0; t < numThreads / 2; t++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var rnd = new Random();
                    for (int i = 0; i < numIterations; i++)
                    {
                        uint serial = (uint)rnd.Next(1, numEntities);
                        var mobile = new Mobile(serial) { X = rnd.Next(5000), Y = rnd.Next(4000) };
                        world.AddMobile(mobile);
                        
                        if (i % 10 == 0) world.RemoveMobile((uint)rnd.Next(1, numEntities));
                    }
                }));
            }

            // Thread di lettura (Simulano script o UI)
            for (int t = 0; t < numThreads / 2; t++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int i = 0; i < numIterations; i++)
                    {
                        // Iterare sulle collection del WorldService è il punto più fragile
                        var mobiles = world.Mobiles.ToList(); 
                        foreach (var m in mobiles)
                        {
                            // Accede alle proprietà per forzare eventuali race conditions
                            int x = m.X;
                            int y = m.Y;
                        }
                    }
                }));
            }

            // Act & Assert
            // Se c'è una "Collection was modified" exception o un deadlock, il test fallirà qui
            var finalTask = Task.WhenAll(tasks);
            var timeoutTask = Task.Delay(10000); // 10 secondi di timeout

            if (await Task.WhenAny(finalTask, timeoutTask) == timeoutTask)
            {
                throw new Exception("Test di stress in timeout: possibile Deadlock rilevato!");
            }
        }

        [Fact]
        public async Task PacketService_FlickeringRegistration_ShouldNotCrash()
        {
            // Arrange
            var service = new PacketService(_messengerMock.Object, _interopMock.Object, _packetLoggerMock.Object);
            int numThreads = 8;
            int numIterations = 2000;
            
            var tasks = new List<Task>();

            // Thread che registrano e deregistrano viewer freneticamente
            for (int t = 0; t < numThreads / 2; t++)
            {
                tasks.Add(Task.Run(() =>
                {
                    Action<byte[]> callback = data => { };
                    for (int i = 0; i < numIterations; i++)
                    {
                        service.RegisterViewer(PacketPath.ServerToClient, 0x1A, callback);
                        if (i % 2 == 0) service.UnregisterViewer(PacketPath.ServerToClient, 0x1A, callback);
                    }
                }));
            }

            // Thread che inviano pacchetti (scatenano l'esecuzione dei viewer)
            for (int t = 0; t < numThreads / 2; t++)
            {
                tasks.Add(Task.Run(() =>
                {
                    byte[] packet = { 0x1A, 0x00, 0x05, 0x01, 0x02 };
                    for (int i = 0; i < numIterations; i++)
                    {
                        service.OnPacketReceived(PacketPath.ServerToClient, packet);
                    }
                }));
            }

            // Act & Assert
            await Task.WhenAll(tasks);
        }

        [Fact]
        public async Task MassFightSimulation_ComplexIntegration()
        {
            // Simula un intero sistema sotto stress: World, Packet e Config
            var world = new WorldService(_messengerMock.Object);
            var service = new PacketService(_messengerMock.Object, _interopMock.Object, _packetLoggerMock.Object);
            
            var cts = new CancellationTokenSource();
            var tasks = new List<Task>();

            // 1. Flusso costante di pacchetti Mobile (0x78)
            tasks.Add(Task.Run(() => 
            {
                var rnd = new Random();
                while (!cts.IsCancellationRequested)
                {
                    byte[] packet = new byte[20];
                    packet[0] = 0x78; // Mobile incoming
                    // Simula dati pacchetto...
                    service.OnPacketReceived(PacketPath.ServerToClient, packet);
                    Thread.Sleep(1); 
                }
            }));

            // 2. Viewer che aggiorna il WorldService ad ogni pacchetto
            service.RegisterViewer(PacketPath.ServerToClient, 0x78, data => 
            {
                // In uno scenario reale qui avremmo il parser, qui usiamo un mobile mock
                world.AddMobile(new Mobile(0xABC) { X = 100, Y = 100 });
            });

            // 3. Script Python fittizio che legge il world a 100fps
            tasks.Add(Task.Run(() => 
            {
                while (!cts.IsCancellationRequested)
                {
                    var count = world.Mobiles.Count();
                    var player = world.Player;
                    Thread.Sleep(10);
                }
            }));

            // Lasciamo girare per 2 secondi
            await Task.Delay(2000);
            cts.Cancel();

            try { await Task.WhenAll(tasks); } catch (OperationCanceledException) { }
        }
    }
}
