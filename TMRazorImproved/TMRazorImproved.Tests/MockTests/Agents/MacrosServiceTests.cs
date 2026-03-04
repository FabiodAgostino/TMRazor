using Moq;
using Xunit;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace TMRazorImproved.Tests.MockTests.Agents
{
    public class MacrosServiceTests
    {
        private readonly Mock<IConfigService>           _configMock   = new();
        private readonly Mock<IPacketService>           _packetMock   = new();
        private readonly Mock<IWorldService>            _worldMock    = new();
        private readonly Mock<ITargetingService>        _targetMock   = new();
        private readonly Mock<ILogger<MacrosService>>   _loggerMock   = new();

        private readonly Mobile _player = new Mobile(0x01) { Name = "TestPlayer" };

        public MacrosServiceTests()
        {
            _configMock.Setup(c => c.CurrentProfile).Returns(new UserProfile());
            _worldMock.Setup(w => w.Player).Returns(_player);
            _player.Hits    = 100;
            _player.HitsMax = 100;
            _player.Mana    = 100;
            _player.ManaMax = 100;
        }

        private MacrosService CreateService() =>
            new MacrosService(
                _configMock.Object,
                _packetMock.Object,
                _worldMock.Object,
                _targetMock.Object,
                _loggerMock.Object);

        // -------------------------------------------------------------------------
        // Helper: play a list of in-memory steps without disk
        // -------------------------------------------------------------------------
        private async Task PlaySteps(MacrosService svc, List<MacroStep> steps, int waitMs = 500)
        {
            // Accediamo al metodo interno via reflection per i test (evita file disco)
            var method = typeof(MacrosService).GetMethod(
                "ExecuteWithControlFlowAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (method != null)
            {
                var cts = new System.Threading.CancellationTokenSource();
                var task = (Task)method.Invoke(svc, new object[] { steps, cts.Token })!;
                await Task.WhenAny(task, Task.Delay(waitMs));
                cts.Cancel();
                try { await task; } catch { }
            }
        }

        // -------------------------------------------------------------------------
        // Test: SAY invia pacchetto 0xAD (UnicodeSpeech)
        // -------------------------------------------------------------------------

        [Fact]
        public async Task SAY_ShouldSendUnicodeSpeechPacket()
        {
            var svc   = CreateService();
            var steps = new List<MacroStep> { new MacroStep("SAY Hello", "SAY Hello") };

            await PlaySteps(svc, steps);

            _packetMock.Verify(
                p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0xAD)),
                Times.Once);
        }

        // -------------------------------------------------------------------------
        // Test: CAST invia pacchetto 0x12 (CastSpell)
        // -------------------------------------------------------------------------

        [Fact]
        public async Task CAST_ShouldSendCastSpellPacket()
        {
            var svc   = CreateService();
            var steps = new List<MacroStep> { new MacroStep("CAST 29", "CAST 29") }; // Greater Heal

            await PlaySteps(svc, steps);

            _packetMock.Verify(
                p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0x12)),
                Times.Once);
        }

        // -------------------------------------------------------------------------
        // Test: DOUBLECLICK invia pacchetto 0x06
        // -------------------------------------------------------------------------

        [Fact]
        public async Task DOUBLECLICK_ShouldSendDoubleClickPacket()
        {
            var svc   = CreateService();
            var steps = new List<MacroStep> { new MacroStep("DOUBLECLICK 12345", "DOUBLECLICK 12345") };

            await PlaySteps(svc, steps);

            _packetMock.Verify(
                p => p.SendToServer(It.Is<byte[]>(b => b.Length == 5 && b[0] == 0x06)),
                Times.Once);
        }

        // -------------------------------------------------------------------------
        // Test: IF true → esegue SAY; IF false → non esegue
        // -------------------------------------------------------------------------

        [Fact]
        public async Task IF_ShouldExecuteBlock_WhenConditionIsTrue()
        {
            _player.Hits    = 50;
            _player.HitsMax = 100;

            var svc = CreateService();
            var steps = new List<MacroStep>
            {
                new MacroStep("IF HP < 80", "IF HP < 80"),
                new MacroStep("SAY low hp",  "SAY low hp"),
                new MacroStep("ENDIF",       "ENDIF"),
            };

            await PlaySteps(svc, steps);

            _packetMock.Verify(
                p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0xAD)),
                Times.Once);
        }

        [Fact]
        public async Task IF_ShouldSkipBlock_WhenConditionIsFalse()
        {
            _player.Hits    = 95;
            _player.HitsMax = 100;

            var svc = CreateService();
            var steps = new List<MacroStep>
            {
                new MacroStep("IF HP < 80", "IF HP < 80"),
                new MacroStep("SAY low hp",  "SAY low hp"),
                new MacroStep("ENDIF",       "ENDIF"),
            };

            await PlaySteps(svc, steps);

            _packetMock.Verify(
                p => p.SendToServer(It.IsAny<byte[]>()),
                Times.Never);
        }

        // -------------------------------------------------------------------------
        // Test: ELSE block eseguito quando IF è falso
        // -------------------------------------------------------------------------

        [Fact]
        public async Task IF_ELSE_ShouldExecuteElse_WhenConditionIsFalse()
        {
            _player.Hits    = 95;
            _player.HitsMax = 100;
            int sayCount = 0;

            _packetMock.Setup(p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0xAD)))
                       .Callback<byte[]>(_ => sayCount++);

            var svc = CreateService();
            var steps = new List<MacroStep>
            {
                new MacroStep("IF HP < 80",   "IF HP < 80"),
                new MacroStep("SAY if block", "SAY if block"),
                new MacroStep("ELSE",         "ELSE"),
                new MacroStep("SAY else",     "SAY else"),
                new MacroStep("ENDIF",        "ENDIF"),
            };

            await PlaySteps(svc, steps);

            Assert.Equal(1, sayCount); // solo l'ELSE branch
        }

        // -------------------------------------------------------------------------
        // Test: FOR 3 → esegue SAY esattamente 3 volte
        // -------------------------------------------------------------------------

        [Fact]
        public async Task FOR_ShouldExecuteBodyNTimes()
        {
            var svc = CreateService();
            var steps = new List<MacroStep>
            {
                new MacroStep("FOR 3",    "FOR 3"),
                new MacroStep("SAY loop", "SAY loop"),
                new MacroStep("ENDFOR",   "ENDFOR"),
            };

            await PlaySteps(svc, steps, waitMs: 1000);

            _packetMock.Verify(
                p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0xAD)),
                Times.Exactly(3));
        }

        // -------------------------------------------------------------------------
        // Test: WHILE loop si ferma quando condizione diventa falsa
        // -------------------------------------------------------------------------

        [Fact]
        public async Task WHILE_ShouldNotExecuteBody_WhenConditionFalseFromStart()
        {
            _player.Hits    = 90;
            _player.HitsMax = 100;

            var svc = CreateService();
            var steps = new List<MacroStep>
            {
                new MacroStep("WHILE HP < 50",  "WHILE HP < 50"),
                new MacroStep("SAY healing",    "SAY healing"),
                new MacroStep("ENDWHILE",       "ENDWHILE"),
            };

            await PlaySteps(svc, steps);

            _packetMock.Verify(
                p => p.SendToServer(It.IsAny<byte[]>()),
                Times.Never);
        }

        // -------------------------------------------------------------------------
        // Test: RESPONDGUMP invia pacchetto 0xB1
        // -------------------------------------------------------------------------

        [Fact]
        public async Task RESPONDGUMP_ShouldSendGumpMenuSelectPacket()
        {
            var svc   = CreateService();
            var steps = new List<MacroStep>
            {
                new MacroStep("RESPONDGUMP 100 200 1", "RESPONDGUMP 100 200 1"),
            };

            await PlaySteps(svc, steps);

            _packetMock.Verify(
                p => p.SendToServer(It.Is<byte[]>(b => b.Length == 23 && b[0] == 0xB1)),
                Times.Once);
        }

        // -------------------------------------------------------------------------
        // Test: TARGET invia 0x6C con il cursorId pendente del server
        // -------------------------------------------------------------------------

        [Fact]
        public async Task TARGET_ShouldSendTargetObjectWithPendingCursorId()
        {
            uint expectedCursorId = 0xDEADBEEF;
            _targetMock.Setup(t => t.PendingCursorId).Returns(expectedCursorId);

            var svc   = CreateService();
            var steps = new List<MacroStep> { new MacroStep("TARGET 12345", "TARGET 12345") };

            await PlaySteps(svc, steps);

            _packetMock.Verify(
                p => p.SendToServer(It.Is<byte[]>(b =>
                    b.Length == 19 &&
                    b[0] == 0x6C &&
                    (uint)((b[2] << 24) | (b[3] << 16) | (b[4] << 8) | b[5]) == expectedCursorId)),
                Times.Once);
            _targetMock.Verify(t => t.ClearTargetCursor(), Times.Once);
        }

        // -------------------------------------------------------------------------
        // Test: condizione NOT POISONED
        // -------------------------------------------------------------------------

        [Fact]
        public async Task IF_NOT_POISONED_ShouldExecute_WhenPlayerIsNotPoisoned()
        {
            _player.IsPoisoned = false;

            var svc = CreateService();
            var steps = new List<MacroStep>
            {
                new MacroStep("IF NOT POISONED", "IF NOT POISONED"),
                new MacroStep("SAY healthy",     "SAY healthy"),
                new MacroStep("ENDIF",           "ENDIF"),
            };

            await PlaySteps(svc, steps);

            _packetMock.Verify(
                p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0xAD)),
                Times.Once);
        }
    }
}
