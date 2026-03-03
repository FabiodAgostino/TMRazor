using System.Linq;
using Moq;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.UI.ViewModels.Agents;
using Xunit;

namespace TMRazorImproved.Tests.MockTests
{
    public class ValidationTests
    {
        [Fact]
        public void AutoLootViewModel_ShouldHaveErrors_WhenDelayIsOutOfRange()
        {
            // Arrange
            var config = new Mock<IConfigService>();
            var target = new Mock<ITargetingService>();
            var log = new Mock<ILogService>();
            var vm = new AutoLootViewModel(config.Object, target.Object, log.Object);

            // Act
            vm.Delay = 50; // Min is 100

            // Assert
            Assert.True(vm.HasErrors);
            var errors = vm.GetErrors(nameof(vm.Delay)).ToList();
            Assert.Single(errors);
            Assert.Contains("Delay must be between 100 and 5000ms", errors[0].ErrorMessage);
        }

        [Fact]
        public void AutoLootViewModel_ShouldNotHaveErrors_WhenDelayIsInRange()
        {
            // Arrange
            var config = new Mock<IConfigService>();
            var target = new Mock<ITargetingService>();
            var log = new Mock<ILogService>();
            var vm = new AutoLootViewModel(config.Object, target.Object, log.Object);

            // Act
            vm.Delay = 1000; 

            // Assert
            Assert.False(vm.HasErrors);
        }
    }
}
