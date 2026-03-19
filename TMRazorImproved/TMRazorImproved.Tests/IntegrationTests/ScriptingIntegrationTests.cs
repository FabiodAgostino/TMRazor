using Xunit;
using TMRazorImproved.Core.Services.Scripting;
using TMRazorImproved.Shared.Enums;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
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
                new Mock<IClientInteropService>().Object,
                new Mock<ITargetingService>().Object,
                new Mock<IJournalService>().Object,
                new Mock<ISkillsService>().Object,
                new Mock<IFriendsService>().Object,
                new Mock<IHotkeyService>().Object,
                new Mock<IConfigService>().Object,
                new Mock<IAutoLootService>().Object,
                new Mock<IScavengerService>().Object,
                new Mock<IOrganizerService>().Object,
                new Mock<IBandageHealService>().Object,
                new Mock<IDressService>().Object,
                new Mock<IRestockService>().Object,
                new Mock<IVendorService>().Object,
                new Mock<ISoundService>().Object,
                new Mock<IMacrosService>().Object,
                new Mock<IPathFindingService>().Object,
                new Mock<ICounterService>().Object,
                new Mock<IDPSMeterService>().Object,
                new Mock<IPacketLoggerService>().Object,
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
        public async Task ExecutePython_ScriptingApi_WorksCorrectly()
        {
            // ARRANGE
            var worldMock = new Mock<IWorldService>();
            var player = new Mobile(0x123) { Name = "TestPlayer", Hits = 100, HitsMax = 110 };
            worldMock.Setup(w => w.Player).Returns(player);
            
            var scriptingService = CreateRealScriptingService(worldMock.Object);
            
            var outputs = new List<string>();
            scriptingService.OutputReceived += (s) => outputs.Add(s);

            string pythonCode = 
                "print(f'NAME: {Player.Name}')\n" +
                "print(f'HITS: {Player.Hits}')\n" +
                "Misc.SendMessage('Hello from Python')\n" +
                "print(f'TARGET_EXISTS: {Target.HasTarget()}')\n" +
                "print(f'ITEMS_COUNT: {len(Items.ApplyFilter(Items.Filter()))}')";

            // ACT
            await scriptingService.RunAsync(pythonCode, ScriptLanguage.Python, "api_test");

            // ASSERT
            Assert.Contains("NAME: TestPlayer", outputs);
            Assert.Contains("HITS: 100", outputs);
            Assert.Contains("TARGET_EXISTS: False", outputs);
            Assert.Contains("ITEMS_COUNT: 0", outputs);
        }

        [Fact]
        public async Task ExecutePython_MobilesApi_FindsMobiles()
        {
            // ARRANGE
            var worldMock = new Mock<IWorldService>();
            var enemy = new Mobile(0x456) { Name = "EnemyNPC", Hits = 50 };
            worldMock.Setup(w => w.Mobiles).Returns(new List<Mobile> { enemy });
            
            var scriptingService = CreateRealScriptingService(worldMock.Object);
            var outputs = new List<string>();
            scriptingService.OutputReceived += (s) => outputs.Add(s);

            string pythonCode = 
                "enemies = Mobiles.ApplyFilter(Mobiles.Filter())\n" +
                "print(f'FOUND: {len(enemies)}')\n" +
                "if len(enemies) > 0:\n" +
                "    print(f'FIRST: {enemies[0].Name}')";

            // ACT
            await scriptingService.RunAsync(pythonCode, ScriptLanguage.Python, "mobiles_test");

            // ASSERT
            Assert.Contains("FOUND: 1", outputs);
            Assert.Contains("FIRST: EnemyNPC", outputs);
        }

        private ScriptingService CreateRealScriptingService(IWorldService? world = null)
        {
            var logger = NullLogger<ScriptingService>.Instance;
            return new ScriptingService(
                world ?? new Mock<IWorldService>().Object,
                new Mock<IPacketService>().Object,
                new Mock<IClientInteropService>().Object,
                new Mock<ITargetingService>().Object,
                new Mock<IJournalService>().Object,
                new Mock<ISkillsService>().Object,
                new Mock<IFriendsService>().Object,
                new Mock<IHotkeyService>().Object,
                new Mock<IConfigService>().Object,
                new Mock<IAutoLootService>().Object,
                new Mock<IScavengerService>().Object,
                new Mock<IOrganizerService>().Object,
                new Mock<IBandageHealService>().Object,
                new Mock<IDressService>().Object,
                new Mock<IRestockService>().Object,
                new Mock<IVendorService>().Object,
                new Mock<ISoundService>().Object,
                new Mock<IMacrosService>().Object,
                new Mock<IPathFindingService>().Object,
                new Mock<ICounterService>().Object,
                new Mock<IDPSMeterService>().Object,
                new Mock<IPacketLoggerService>().Object,
                WeakReferenceMessenger.Default,
                logger,
                new NullLoggerFactory()
            );
        }
    }
}
