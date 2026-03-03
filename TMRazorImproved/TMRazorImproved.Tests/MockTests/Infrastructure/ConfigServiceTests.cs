using Moq;
using Xunit;
using TMRazorImproved.Core.Services;
using Microsoft.Extensions.Logging;
using TMRazorImproved.Shared.Models.Config;
using System.IO;
using System;
using System.Linq;

namespace TMRazorImproved.Tests.MockTests.Infrastructure
{
    public class ConfigServiceTests : IDisposable
    {
        private readonly Mock<ILogger<ConfigService>> _loggerMock = new();
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
            var service = new ConfigService(_loggerMock.Object);

            // Assert
            Assert.NotNull(service.Global);
            Assert.Equal("Default", service.Global.LastProfile);
            Assert.NotNull(service.CurrentProfile);
            Assert.Equal("Default", service.CurrentProfile.Name);
            Assert.True(File.Exists(Path.Combine(_configPath, "global.json")));
        }

        [Fact]
        public void SwitchProfile_ShouldCreateNewProfile_WhenNotExists()
        {
            // Arrange
            var service = new ConfigService(_loggerMock.Object);

            // Act
            service.SwitchProfile("NewProfile");

            // Assert
            Assert.Equal("NewProfile", service.CurrentProfile.Name);
            Assert.Equal("NewProfile", service.Global.LastProfile);
            Assert.True(File.Exists(Path.Combine(_configPath, "Profiles", "NewProfile.json")));
        }

        [Fact]
        public void Save_ShouldPersistChanges()
        {
            // Arrange
            var service = new ConfigService(_loggerMock.Object);
            service.Global.ClientPath = @"C:\UO";
            service.CurrentProfile.AutoLootLists[0].Enabled = true;

            // Act
            service.Save();

            // Re-inizializza un nuovo servizio per verificare il caricamento da disco
            var newService = new ConfigService(_loggerMock.Object);

            // Assert
            Assert.Equal(@"C:\UO", newService.Global.ClientPath);
            Assert.True(newService.CurrentProfile.AutoLootLists[0].Enabled);
        }

        [Fact]
        public void DeleteProfile_ShouldRemoveFile()
        {
            // Arrange
            var service = new ConfigService(_loggerMock.Object);
            service.CreateProfile("ToDelete");
            string profilePath = Path.Combine(_configPath, "Profiles", "ToDelete.json");
            Assert.True(File.Exists(profilePath));

            // Act
            service.DeleteProfile("ToDelete");

            // Assert
            Assert.False(File.Exists(profilePath));
        }

        [Fact]
        public void GetAvailableProfiles_ShouldReturnAllProfiles()
        {
            // Arrange
            var service = new ConfigService(_loggerMock.Object);
            service.CreateProfile("Profile1");
            service.CreateProfile("Profile2");

            // Act
            var profiles = service.GetAvailableProfiles().ToList();

            // Assert
            Assert.Contains("Default", profiles);
            Assert.Contains("Profile1", profiles);
            Assert.Contains("Profile2", profiles);
        }
    }
}
