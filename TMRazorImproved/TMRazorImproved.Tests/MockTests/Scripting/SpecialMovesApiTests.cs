using Xunit;
using Moq;
using TMRazorImproved.Core.Services.Scripting.Api;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Core.Services.Scripting;

namespace TMRazorImproved.Tests.MockTests.Scripting
{
    public class SpecialMovesApiTests
    {
        [Fact]
        public void SetPrimaryAbility_SendsCorrectPacket()
        {
            // ARRANGE
            var worldMock = new Mock<IWorldService>();
            var packetMock = new Mock<IPacketService>();
            var player = new Mobile(0x123);
            worldMock.Setup(w => w.Player).Returns(player);
            
            var api = new SpecialMovesApi(worldMock.Object, packetMock.Object, new ScriptCancellationController(System.Threading.CancellationToken.None));

            // ACT
            api.SetPrimaryAbility();

            // ASSERT
            packetMock.Verify(p => p.SendToServer(It.Is<byte[]>(data => 
                data.Length == 9 &&
                data[0] == 0xD7 &&
                data[8] == 0x01 &&
                (data[3] == 0x00 && data[4] == 0x00 && data[5] == 0x01 && data[6] == 0x23) // Serial 0x123
            )), Times.Once);
        }

        [Fact]
        public void HasPrimary_ReturnsPlayerState()
        {
            // ARRANGE
            var worldMock = new Mock<IWorldService>();
            var player = new Mobile(0x123) { PrimaryAbilityActive = true };
            worldMock.Setup(w => w.Player).Returns(player);
            
            var api = new SpecialMovesApi(worldMock.Object, new Mock<IPacketService>().Object, new ScriptCancellationController(System.Threading.CancellationToken.None));

            // ASSERT
            Assert.True(api.HasPrimary);
            
            player.PrimaryAbilityActive = false;
            Assert.False(api.HasPrimary);
        }
    }
}
