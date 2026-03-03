using Moq;
using TMRazorImproved.Core.Services.Scripting;
using TMRazorImproved.Core.Services.Scripting.Api;
using TMRazorImproved.Core.Services.Scripting.Engines;
using TMRazorImproved.Shared.Interfaces;
using Xunit;

namespace TMRazorImproved.Tests.MockTests.Scripting
{
    public class UOSteamInterpreterTests
    {
        private readonly Mock<IWorldService> _worldMock = new();
        private readonly Mock<IPacketService> _packetMock = new();
        private readonly Mock<ITargetingService> _targetingMock = new();
        private readonly ScriptCancellationController _cancel;
        private readonly List<string> _outputLog = new();

        public UOSteamInterpreterTests()
        {
            _cancel = new ScriptCancellationController(new CancellationToken());
        }

        private UOSteamInterpreter CreateInterpreter(PlayerApi playerApi)
        {
            var misc = new MiscApi(_worldMock.Object, _cancel);
            var items = new ItemsApi(_worldMock.Object, _packetMock.Object, _cancel);
            var mobiles = new MobilesApi(_worldMock.Object, _cancel);
            
            return new UOSteamInterpreter(misc, playerApi, items, mobiles, _cancel, s => _outputLog.Add(s));
        }

        [Fact]
        public void Execute_SimpleMsg_ShouldCallPlayerChatSay()
        {
            // Arrange
            var playerMock = new Mock<PlayerApi>(_worldMock.Object, _packetMock.Object, _targetingMock.Object, _cancel);
            var interpreter = CreateInterpreter(playerMock.Object);
            string code = "msg 'hello world'";

            // Act
            interpreter.Execute(code);

            // Assert
            playerMock.Verify(p => p.ChatSay("hello world", It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void Execute_IfCondition_ShouldWork()
        {
            // Arrange
            var playerMock = new Mock<PlayerApi>(_worldMock.Object, _packetMock.Object, _targetingMock.Object, _cancel);
            playerMock.Setup(p => p.Hits).Returns(50);
            var interpreter = CreateInterpreter(playerMock.Object);
            
            string code = @"
                if hits == 50
                    msg 'correct'
                else
                    msg 'wrong'
                endif";

            // Act
            interpreter.Execute(code);

            // Assert
            playerMock.Verify(p => p.ChatSay("correct", It.IsAny<int>()), Times.Once);
            playerMock.Verify(p => p.ChatSay("wrong", It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Execute_WhileLoop_ShouldExecuteMultipleTimes()
        {
            // Arrange
            var playerMock = new Mock<PlayerApi>(_worldMock.Object, _packetMock.Object, _targetingMock.Object, _cancel);
            
            // Simula hits che scendono ogni volta che viene letto
            int callCount = 0;
            playerMock.Setup(p => p.Hits).Returns(() => 3 - callCount++);
            
            var interpreter = CreateInterpreter(playerMock.Object);
            
            string code = @"
                while hits > 0
                    msg 'loop'
                endwhile";

            // Act
            interpreter.Execute(code);

            // Assert
            playerMock.Verify(p => p.ChatSay("loop", It.IsAny<int>()), Times.Exactly(3));
        }

        [Fact]
        public void Execute_SetAlias_ShouldPersist()
        {
            // Arrange
            var playerMock = new Mock<PlayerApi>(_worldMock.Object, _packetMock.Object, _targetingMock.Object, _cancel);
            var interpreter = CreateInterpreter(playerMock.Object);
            
            string code = @"
                setalias 'target' 0x123
                if serial == target
                    msg 'is_target'
                endif";
            
            playerMock.Setup(p => p.Serial).Returns(0x123);

            // Act
            interpreter.Execute(code);

            // Assert
            playerMock.Verify(p => p.ChatSay("is_target", It.IsAny<int>()), Times.Once);
        }
    }
}
