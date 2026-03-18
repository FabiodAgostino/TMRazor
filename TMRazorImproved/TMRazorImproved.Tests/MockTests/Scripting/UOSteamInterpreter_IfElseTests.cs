using Moq;
using TMRazorImproved.Core.Services.Scripting;
using TMRazorImproved.Core.Services.Scripting.Api;
using TMRazorImproved.Core.Services.Scripting.Engines;
using TMRazorImproved.Shared.Interfaces;
using Xunit;
using System.Collections.Generic;
using System.Threading;
using CommunityToolkit.Mvvm.Messaging;

namespace TMRazorImproved.Tests.MockTests.Scripting
{
    public class UOSteamInterpreter_IfElseTests
    {
        private readonly Mock<IWorldService> _worldMock = new();
        private readonly Mock<IPacketService> _packetMock = new();
        private readonly Mock<IClientInteropService> _interopMock = new();
        private readonly Mock<ITargetingService> _targetingMock = new();
        private readonly Mock<ISkillsService> _skillsMock = new();
        private readonly Mock<IFriendsService> _friendsMock = new();
        private readonly ScriptCancellationController _cancel;
        private readonly List<string> _outputLog = new();

        public UOSteamInterpreter_IfElseTests()
        {
            _cancel = new ScriptCancellationController(new CancellationToken());
        }

        private UOSteamInterpreter CreateInterpreter(PlayerApi playerApi)
        {
            var misc    = new MiscApi(_worldMock.Object, _packetMock.Object, _interopMock.Object, _cancel);
            var items   = new ItemsApi(_worldMock.Object, _packetMock.Object, _targetingMock.Object, _cancel);
            var mobiles = new MobilesApi(_worldMock.Object, _friendsMock.Object, _packetMock.Object, _targetingMock.Object, _cancel);
            var journalMock = new Mock<IJournalService>();
            var journal = new JournalApi(journalMock.Object, _cancel);
            var configMock = new Mock<IConfigService>();
            var messengerMock = new Mock<IMessenger>();
            var targetApi = new TargetApi(_targetingMock.Object, _worldMock.Object, configMock.Object, _packetMock.Object, _cancel);
            var skillsApi = new SkillsApi(_skillsMock.Object, _packetMock.Object, _cancel);
            var gumpsApi = new GumpsApi(_worldMock.Object, _packetMock.Object, _cancel, messengerMock.Object);
            
            var autoLootApi = new AutoLootApi(new Mock<IAutoLootService>().Object, _cancel);
            var dressApi = new DressApi(new Mock<IDressService>().Object, _cancel);
            var scavengerApi = new ScavengerApi(new Mock<IScavengerService>().Object, _cancel);
            var restockApi = new RestockApi(new Mock<IRestockService>().Object, _cancel);
            var organizerApi = new OrganizerApi(new Mock<IOrganizerService>().Object, _cancel);
            var bandageHealApi = new BandageHealApi(new Mock<IBandageHealService>().Object, _cancel);
            var hotkeyApi = new HotkeyApi(new Mock<IHotkeyService>().Object, configMock.Object, _cancel);

            return new UOSteamInterpreter(misc, playerApi, items, mobiles, journal, targetApi, skillsApi, gumpsApi, 
                autoLootApi, dressApi, scavengerApi, restockApi, organizerApi, bandageHealApi, hotkeyApi,
                _cancel, s => _outputLog.Add(s));
        }

        [Fact]
        public void Execute_IfElseIf_ShouldExecuteCorrectBranch()
        {
            // Arrange
            var playerMock = new Mock<PlayerApi>(_worldMock.Object, _packetMock.Object, _targetingMock.Object, _skillsMock.Object, _cancel, null!, null!, null!);
            playerMock.Setup(p => p.Hits).Returns(30);
            var interpreter = CreateInterpreter(playerMock.Object);
            
            string code = @"
                if hits == 50
                    msg 'hits50'
                elseif hits == 30
                    msg 'hits30'
                else
                    msg 'hitsOther'
                endif";

            // Act
            interpreter.Execute(code);

            // Assert
            playerMock.Verify(p => p.ChatSay("hits50", It.IsAny<int>()), Times.Never);
            playerMock.Verify(p => p.ChatSay("hits30", It.IsAny<int>()), Times.Once);
            playerMock.Verify(p => p.ChatSay("hitsOther", It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Execute_IfTrueElseIf_ShouldOnlyExecuteFirstBranch()
        {
            // Arrange
            var playerMock = new Mock<PlayerApi>(_worldMock.Object, _packetMock.Object, _targetingMock.Object, _skillsMock.Object, _cancel, null!, null!, null!);
            playerMock.Setup(p => p.Hits).Returns(50);
            var interpreter = CreateInterpreter(playerMock.Object);
            
            string code = @"
                if hits == 50
                    msg 'hits50'
                elseif hits == 50
                    msg 'hits50_second'
                endif";

            // Act
            interpreter.Execute(code);

            // Assert
            playerMock.Verify(p => p.ChatSay("hits50", It.IsAny<int>()), Times.Once);
            playerMock.Verify(p => p.ChatSay("hits50_second", It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Execute_NestedIf_ShouldWorkCorrectly()
        {
            // Arrange
            var playerMock = new Mock<PlayerApi>(_worldMock.Object, _packetMock.Object, _targetingMock.Object, _skillsMock.Object, _cancel, null!, null!, null!);
            playerMock.Setup(p => p.Hits).Returns(50);
            playerMock.Setup(p => p.Mana).Returns(20);
            var interpreter = CreateInterpreter(playerMock.Object);
            
            string code = @"
                if hits == 50
                    if mana == 20
                        msg 'nested_true'
                    else
                        msg 'nested_false'
                    endif
                else
                    msg 'outer_false'
                endif";

            // Act
            interpreter.Execute(code);

            // Assert
            playerMock.Verify(p => p.ChatSay("nested_true", It.IsAny<int>()), Times.Once);
            playerMock.Verify(p => p.ChatSay("nested_false", It.IsAny<int>()), Times.Never);
            playerMock.Verify(p => p.ChatSay("outer_false", It.IsAny<int>()), Times.Never);
        }
    }
}
