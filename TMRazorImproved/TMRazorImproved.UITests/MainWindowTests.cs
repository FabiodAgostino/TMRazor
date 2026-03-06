using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using Xunit;

namespace TMRazorImproved.UITests
{
    public class MainWindowTests : BaseUITest
    {
        [Fact]
        public void AppTitle_ShouldBeCorrect()
        {
            // Verify the title bar text
            var titleBar = Session.FindElement(MobileBy.AccessibilityId("AppTitleBar"));
            Assert.NotNull(titleBar);
            
            // In WPF UI, the title might be a child or a property of the TitleBar
            // Usually, searching for the Window title is easier
            Assert.Contains("TMRazor Improved", Session.Title);
        }

        [Fact]
        public void NavigationMenu_ShouldExist()
        {
            var navView = Session.FindElement(MobileBy.AccessibilityId("RootNavigation"));
            Assert.NotNull(navView);
        }

        [Fact]
        public void NavigateToGeneral_ShouldWork()
        {
            // Find the navigation item for "General"
            var generalItem = Session.FindElement(MobileBy.Name("General"));
            generalItem.Click();

            Assert.True(generalItem.Displayed);
        }
    }
}
