using Xunit;
using TMRazorImproved.Core.Services.Scripting;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using CommunityToolkit.Mvvm.Messaging;

namespace TMRazorImproved.Tests.Integration
{
    public class ScriptingIntegrationTests
    {
        [Fact]
        public async Task ExecutePython_RealEngine_CapturePrintOutput()
        {
            // ARRANGE: Usiamo il servizio REALE con il motore IronPython
            var logger = NullLogger<ScriptingService>.Instance;
            var scriptingService = new ScriptingService(
                new Mock<IWorldService>().Object,
                new Mock<IPacketService>().Object,
                new Mock<ITargetingService>().Object,
                new Mock<IJournalService>().Object,
                new Mock<ISkillsService>().Object,
                new Mock<IFriendsService>().Object,
                new Mock<IConfigService>().Object,
                WeakReferenceMessenger.Default,
                logger,
                new NullLoggerFactory()
            );

            string capturedOutput = "";
            scriptingService.OutputReceived += (line) => {
                capturedOutput += line;
            };

            // Codice Python che verifica anche l'override di time.sleep (tramite Preamble)
            string pythonCode = 
                "import time\n" +
                "print('START')\n" +
                "time.sleep(0.01)\n" +
                "print('END')";

            // ACT
            await scriptingService.RunAsync(pythonCode, ScriptLanguage.Python, "integration_test");

            // ASSERT
            Assert.Contains("START", capturedOutput);
            Assert.Contains("END", capturedOutput);
        }

        [Fact]
        public async Task ExecutePython_Arithmetic_CalculationWorks()
        {
            var scriptingService = CreateRealScriptingService();
            string output = "";
            scriptingService.OutputReceived += (s) => output += s;

            await scriptingService.RunAsync("print(10 + 20 * 2)", ScriptLanguage.Python);

            Assert.Contains("50", output);
        }

        private ScriptingService CreateRealScriptingService()
        {
            return new ScriptingService(
                new Mock<IWorldService>().Object,
                new Mock<IPacketService>().Object,
                new Mock<ITargetingService>().Object,
                new Mock<IJournalService>().Object,
                new Mock<ISkillsService>().Object,
                new Mock<IFriendsService>().Object,
                new Mock<IConfigService>().Object,
                WeakReferenceMessenger.Default,
                NullLogger<ScriptingService>.Instance,
                new NullLoggerFactory()
            );
        }
    }
}
