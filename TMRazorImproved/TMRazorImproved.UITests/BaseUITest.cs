using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System;
using System.Diagnostics;
using System.IO;
using Xunit;

namespace TMRazorImproved.UITests
{
    public abstract class BaseUITest : IDisposable
    {
        protected WindowsDriver Session { get; private set; }
        private const string WinAppDriverUrl = "http://127.0.0.1:4723";
        
        // Path to your application executable
        private static readonly string AppPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "TMRazorImproved.UI", "bin", "Debug", "net10.0-windows", "TMRazorImproved.UI.exe"));

        protected BaseUITest()
        {
            if (Session == null)
            {
                var options = new AppiumOptions();
                options.App = AppPath;
                options.DeviceName = "WindowsPC";
                options.AutomationName = "Windows";

                try
                {
                    Session = new WindowsDriver(new Uri(WinAppDriverUrl), options);
                    Assert.NotNull(Session);
                    Assert.NotNull(Session.SessionId);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to initialize WinAppDriver session: {ex.Message}");
                    throw;
                }

                // Set implicit timeout
                Session.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            }
        }

        public virtual void Dispose()
        {
            if (Session != null)
            {
                Session.Quit();
                Session = null;
            }
        }
    }
}
