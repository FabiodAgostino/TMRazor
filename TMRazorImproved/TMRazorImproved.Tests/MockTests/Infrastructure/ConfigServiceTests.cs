using Moq;
using Xunit;
using TMRazorImproved.Core.Services;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Models.Config;
using System.IO;
using System;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;

namespace TMRazorImproved.Tests.MockTests.Infrastructure
{
    public class ConfigServiceTests : IDisposable
    {
        private readonly Mock<ILogger<ConfigService>> _loggerMock = new();
        private readonly Mock<IMessenger> _messengerMock = new();
        private readonly string _configPath;

        public ConfigServiceTests()
        {
            _configPath = Path.Combine(AppContext.BaseDirectory, "Config");
            // Pulizia iniziale per isolamento test
            if (Directory.Exists(_configPath))
            {
                try { Directory.Delete(_configPath, true); } catch { }
            }
        }

        public void Dispose()
        {
            // Pulizia finale
            if (Directory.Exists(_configPath))
            {
                try { Directory.Delete(_configPath, true); } catch { }
            }
        }

        [Fact]
        public void Load_ShouldCreateDefaultConfig_WhenMissing()
        {
            // Act
            var service = new ConfigService(_loggerMock.Object, _messengerMock.Object);

            // Assert
            Assert.NotNull(service.Global);
            Assert.Equal("Default", service.Global.LastProfile);
            Assert.NotNull(service.CurrentProfile);
            Assert.Equal("Default", service.CurrentProfile.Name);
            Assert.True(File.Exists(Path.Combine(_configPath, "global.json")));
        }

        [Fact]
        public void SwitchProfile_ShouldLoadCorrectProfile()
        {
            // Arrange
            var service = new ConfigService(_loggerMock.Object, _messengerMock.Object);
            service.CreateProfile("TestProfile");
            
            // Act
            service.SwitchProfile("TestProfile");
            
            // Assert
            Assert.Equal("TestProfile", service.CurrentProfile.Name);
            Assert.Equal("TestProfile", service.Global.LastProfile);
        }

        [Fact]
        public void CreateProfile_ShouldGenerateNewJsonFile()
        {
            // Arrange
            var service = new ConfigService(_loggerMock.Object, _messengerMock.Object);
            
            // Act
            service.CreateProfile("NewTestProfile");
            
            // Assert
            string profileFile = Path.Combine(_configPath, "Profiles", "NewTestProfile.json");
            Assert.True(File.Exists(profileFile));
        }

        [Fact]
        public void CloneProfile_ShouldCopyData()
        {
            // Arrange
            var service = new ConfigService(_loggerMock.Object, _messengerMock.Object);
            service.CurrentProfile.FiltersEnabled = false;
            service.Save();
            
            // Act
            service.CloneProfile("Default", "ClonedProfile");
            service.SwitchProfile("ClonedProfile");
            
            // Assert
            Assert.False(service.CurrentProfile.FiltersEnabled);
        }

        [Fact]
        public void RenameProfile_ShouldDeleteOldAndCreateNew()
        {
            // Arrange
            var service = new ConfigService(_loggerMock.Object, _messengerMock.Object);
            service.CreateProfile("OldName");
            
            // Act
            service.RenameProfile("OldName", "NewName");
            
            // Assert
            Assert.True(File.Exists(Path.Combine(_configPath, "Profiles", "NewName.json")));
            Assert.False(File.Exists(Path.Combine(_configPath, "Profiles", "OldName.json")));
        }

        [Fact]
        public void DeleteProfile_ShouldRemoveJsonFile()
        {
            // Arrange
            var service = new ConfigService(_loggerMock.Object, _messengerMock.Object);
            service.CreateProfile("ToDelete");
            
            // Act
            service.DeleteProfile("ToDelete");
            
            // Assert
            Assert.False(File.Exists(Path.Combine(_configPath, "Profiles", "ToDelete.json")));
        }
    }
}
