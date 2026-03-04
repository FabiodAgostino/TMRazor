using Moq;
using Xunit;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Shared.Models.Config;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TMRazorImproved.Tests.MockTests.Agents
{
    public class BandageHealServiceTests
    {
        private readonly Mock<IPacketService> _packetServiceMock = new();
        private readonly Mock<IConfigService> _configServiceMock = new();
        private readonly Mock<IWorldService> _worldServiceMock = new();
        private readonly Mock<IHotkeyService> _hotkeyServiceMock = new();
        private readonly Mock<ILogger<BandageHealService>> _loggerMock = new();
        private readonly UserProfile _profile = new();
        private readonly Mobile _player = new Mobile(0x123) { Name = "TestPlayer" };

        public BandageHealServiceTests()
        {
            _configServiceMock.Setup(c => c.CurrentProfile).Returns(_profile);
            _worldServiceMock.Setup(w => w.Player).Returns(_player);
            
            _profile.BandageHeal.BandageSerial = 0x44444444;
            _profile.BandageHeal.HpStart = 80;
            _profile.BandageHeal.CustomDelay = 100; // Fast delay for testing
        }

        [Fact]
        public async Task ShouldTriggerHeal_WhenHpIsLow()
        {
            // Arrange
            var service = new BandageHealService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _hotkeyServiceMock.Object,
                _loggerMock.Object);

            _player.Hits = 50;
            _player.HitsMax = 100;

            // Act
            service.Start();
            await Task.Delay(200); // Wait for at least one loop
            service.Stop();

            // Assert
            // 1. Double click on bandages (0x06)
            _packetServiceMock.Verify(p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0x06)), Times.AtLeastOnce);
            // 2. Target self (0x6C)
            _packetServiceMock.Verify(p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0x6C)), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ShouldTriggerHeal_WhenPoisonedAndPriorityActive()
        {
            // Arrange
            var service = new BandageHealService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _hotkeyServiceMock.Object,
                _loggerMock.Object);

            _player.Hits = 100;
            _player.HitsMax = 100;
            _player.IsPoisoned = true;
            _profile.BandageHeal.PoisonPriority = true;

            // Act
            service.Start();
            await Task.Delay(200);
            service.Stop();

            // Assert
            _packetServiceMock.Verify(p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0x06)), Times.AtLeastOnce);
        }

        [Fact]
        public async Task ShouldNotTriggerHeal_WhenHpIsHigh()
        {
            // Arrange
            var service = new BandageHealService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _hotkeyServiceMock.Object,
                _loggerMock.Object);

            _player.Hits = 95;
            _player.HitsMax = 100;
            _player.IsPoisoned = false;

            // Act
            service.Start();
            await Task.Delay(200);
            service.Stop();

            // Assert
            _packetServiceMock.Verify(p => p.SendToServer(It.IsAny<byte[]>()), Times.Never);
        }

        // ---------------------------------------------------------------
        // Sprint Fix-3: HiddenStop + NullRef
        // ---------------------------------------------------------------

        [Fact]
        public async Task ShouldNotThrow_WhenCurrentProfileIsNull()
        {
            // Arrange: profilo null → nessuna NullReferenceException
            _configServiceMock.Setup(c => c.CurrentProfile).Returns((UserProfile?)null);

            var service = new BandageHealService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _hotkeyServiceMock.Object,
                _loggerMock.Object);

            service.Start();
            await Task.Delay(300);
            service.Stop();

            _packetServiceMock.Verify(p => p.SendToServer(It.IsAny<byte[]>()), Times.Never);
        }

        [Fact]
        public async Task ShouldNotHeal_WhenHiddenAndHiddenStopIsActive()
        {
            // Arrange
            _player.Hits = 10;
            _player.HitsMax = 100;
            _player.IsHidden = true;
            _profile.BandageHeal.HiddenStop = true;

            var service = new BandageHealService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _hotkeyServiceMock.Object,
                _loggerMock.Object);

            service.Start();
            await Task.Delay(300);
            service.Stop();

            // HiddenStop deve bloccare la cura anche con HP basso
            _packetServiceMock.Verify(p => p.SendToServer(It.IsAny<byte[]>()), Times.Never);
        }

        [Fact]
        public async Task ShouldHeal_WhenHiddenButHiddenStopIsDisabled()
        {
            // Arrange
            _player.Hits = 10;
            _player.HitsMax = 100;
            _player.IsHidden = true;
            _profile.BandageHeal.HiddenStop = false;

            var service = new BandageHealService(
                _packetServiceMock.Object,
                _configServiceMock.Object,
                _worldServiceMock.Object,
                _hotkeyServiceMock.Object,
                _loggerMock.Object);

            service.Start();
            await Task.Delay(300);
            service.Stop();

            // Deve curare anche da hidden se HiddenStop è false
            _packetServiceMock.Verify(
                p => p.SendToServer(It.Is<byte[]>(b => b.Length > 0 && b[0] == 0x06)),
                Times.AtLeastOnce);
        }
    }
}
