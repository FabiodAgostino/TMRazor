using Xunit;
using Moq;
using System.Collections.Generic;
using TMRazorImproved.Core.Services.Scripting.Api;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models.Config;
using TMRazorImproved.Core.Services.Scripting;

namespace TMRazorImproved.Tests.MockTests.Scripting
{
    public class HotkeyApiTests
    {
        [Fact]
        public void Get_ReturnsAllHotkeyActions()
        {
            // ARRANGE
            var hotkeyMock = new Mock<IHotkeyService>();
            var configMock = new Mock<IConfigService>();
            var profile = new UserProfile();
            profile.Hotkeys.Add(new HotkeyDefinition { Action = "Action1" });
            profile.Hotkeys.Add(new HotkeyDefinition { Action = "Action2" });
            configMock.Setup(c => c.CurrentProfile).Returns(profile);
            
            var api = new HotkeyApi(hotkeyMock.Object, configMock.Object, new ScriptCancellationController(System.Threading.CancellationToken.None));

            // ACT
            var result = api.Get();

            // ASSERT
            Assert.Equal(2, result.Count);
            Assert.Contains("Action1", result);
            Assert.Contains("Action2", result);
        }

        [Fact]
        public void GetStatus_ReturnsCorrectStatus()
        {
            // ARRANGE
            var hotkeyMock = new Mock<IHotkeyService>();
            var configMock = new Mock<IConfigService>();
            var profile = new UserProfile();
            var hk = new HotkeyDefinition { Action = "Action1", Enabled = true };
            profile.Hotkeys.Add(hk);
            configMock.Setup(c => c.CurrentProfile).Returns(profile);
            hotkeyMock.Setup(h => h.IsEnabled).Returns(true);
            
            var api = new HotkeyApi(hotkeyMock.Object, configMock.Object, new ScriptCancellationController(System.Threading.CancellationToken.None));

            // ASSERT
            Assert.True(api.GetStatus("Action1"));
            
            hk.Enabled = false;
            Assert.False(api.GetStatus("Action1"));
            
            hk.Enabled = true;
            hotkeyMock.Setup(h => h.IsEnabled).Returns(false);
            Assert.False(api.GetStatus("Action1"));
        }

        [Fact]
        public void SetStatus_UpdatesHotkeyState()
        {
            // ARRANGE
            var hotkeyMock = new Mock<IHotkeyService>();
            var configMock = new Mock<IConfigService>();
            var profile = new UserProfile();
            var hk = new HotkeyDefinition { Action = "Action1", Enabled = true };
            profile.Hotkeys.Add(hk);
            configMock.Setup(c => c.CurrentProfile).Returns(profile);
            
            var api = new HotkeyApi(hotkeyMock.Object, configMock.Object, new ScriptCancellationController(System.Threading.CancellationToken.None));

            // ACT
            api.SetStatus("Action1", false);

            // ASSERT
            Assert.False(hk.Enabled);
            
            // Test master toggle
            api.SetStatus("Master", false);
            hotkeyMock.VerifySet(h => h.IsEnabled = false, Times.Once);
        }

        [Fact]
        public void GetKey_ReturnsCorrectKeyCode()
        {
            // ARRANGE
            var hotkeyMock = new Mock<IHotkeyService>();
            var configMock = new Mock<IConfigService>();
            var profile = new UserProfile();
            profile.Hotkeys.Add(new HotkeyDefinition { Action = "Action1", KeyCode = 65 }); // 'A'
            configMock.Setup(c => c.CurrentProfile).Returns(profile);
            
            var api = new HotkeyApi(hotkeyMock.Object, configMock.Object, new ScriptCancellationController(System.Threading.CancellationToken.None));

            // ASSERT
            Assert.Equal(65, api.GetKey("Action1"));
            Assert.Equal(0, api.GetKey("Unknown"));
        }

        [Fact]
        public void KeyString_ReturnsFormattedString()
        {
            // ARRANGE
            var hotkeyMock = new Mock<IHotkeyService>();
            var configMock = new Mock<IConfigService>();
            var profile = new UserProfile();
            profile.Hotkeys.Add(new HotkeyDefinition { Action = "Action1", KeyCode = 65, Ctrl = true, Alt = true });
            configMock.Setup(c => c.CurrentProfile).Returns(profile);
            
            var api = new HotkeyApi(hotkeyMock.Object, configMock.Object, new ScriptCancellationController(System.Threading.CancellationToken.None));

            // ASSERT
            // KeyCode 65 corresponds to Keys.A
            Assert.Equal("Ctrl+Alt+A", api.KeyString("Action1"));
            Assert.Equal("None", api.KeyString("Unknown"));
        }
    }
}
