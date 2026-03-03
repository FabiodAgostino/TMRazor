using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Windows;
using TMRazorImproved.UI.ViewModels;
using TMRazorImproved.UI.Views;
using TMRazorImproved.UI.Views.Pages;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Core.Handlers;
using TMRazorImproved.Core.Services.Scripting;
using Wpf.Ui;
using CommunityToolkit.Mvvm.Messaging;

namespace TMRazorImproved.UI
{
    public partial class App : Application
    {
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Debug);
                logging.AddNLog();
            })
            .ConfigureServices((context, services) =>
            {
                // UI Services
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IPageService, PageService>();

                // Infrastructure (MVVM Messenger)
                services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

                // Windows
                services.AddSingleton<MainWindow>();

                // Core Services
                services.AddSingleton<IClientInteropService, ClientInteropService>();
                services.AddSingleton<IPacketService, PacketService>();
                services.AddSingleton<IWorldService, WorldService>();
                services.AddSingleton<IJournalService, JournalService>();
                services.AddSingleton<IConfigService, ConfigService>();
                services.AddSingleton<ILanguageService, LanguageService>();
                services.AddSingleton<IScriptingService, ScriptingService>();
                services.AddSingleton<IAutoLootService, AutoLootService>();
                services.AddSingleton<IScavengerService, ScavengerService>();
                services.AddSingleton<IOrganizerService, OrganizerService>();
                services.AddSingleton<IBandageHealService, BandageHealService>();
                services.AddSingleton<IDressService, DressService>();
                services.AddSingleton<IVendorService, VendorService>();
                services.AddSingleton<ITargetingService, TargetingService>();
                services.AddSingleton<ITitleBarService, TitleBarService>();
                services.AddSingleton<IHotkeyService, HotkeyService>();
                
                // Packet Handlers
                services.AddSingleton<WorldPacketHandler>();
                services.AddSingleton<FilterHandler>();

                // ViewModels
                services.AddSingleton<GeneralViewModel>();
                services.AddSingleton<PlayerStatusViewModel>();
                services.AddSingleton<ScriptingViewModel>();
                services.AddSingleton<OptionsViewModel>();
                services.AddSingleton<HotkeysViewModel>();
                services.AddSingleton<InspectorViewModel>();

                // Pages
                services.AddSingleton<DashboardPage>();
                services.AddSingleton<GeneralPage>();
                services.AddSingleton<ScriptingPage>();
                services.AddSingleton<OptionsPage>();
                services.AddSingleton<HotkeysPage>();
                services.AddSingleton<InspectorPage>();
            }).Build();

        public static T? GetService<T>() where T : class
            => _host.Services.GetService(typeof(T)) as T;

        protected override async void OnStartup(StartupEventArgs e)
        {
            try 
            {
                await _host.StartAsync();

                // Inizializzazione Servizi Core
                var config = _host.Services.GetRequiredService<IConfigService>();
                var lang = _host.Services.GetRequiredService<ILanguageService>();
                lang.Load(config.Global.Language);
                _host.Services.GetRequiredService<WorldPacketHandler>();
                _host.Services.GetRequiredService<FilterHandler>();
                _host.Services.GetRequiredService<ITitleBarService>().Start();
                _host.Services.GetRequiredService<IHotkeyService>().Start();
                
                // Forza l'inizializzazione degli agenti per registrare le hotkey
                _host.Services.GetRequiredService<IAutoLootService>();
                _host.Services.GetRequiredService<IScavengerService>();
                _host.Services.GetRequiredService<IOrganizerService>();
                _host.Services.GetRequiredService<IBandageHealService>();
                _host.Services.GetRequiredService<IDressService>();
                _host.Services.GetRequiredService<IVendorService>();
                _host.Services.GetRequiredService<ITargetingService>();

                // Attiva i ViewModel che si iscrivono a messaggi (il DI li crea lazy)
                _host.Services.GetRequiredService<PlayerStatusViewModel>();

                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                var logger = _host.Services.GetRequiredService<ILogger<App>>();
                logger.LogCritical(ex, "Startup Error: {Message}", ex.Message);
                MessageBox.Show($"Startup Error: {ex.Message}\n{ex.InnerException?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            // Disponi esplicitamente i servizi che implementano IDisposable
            (_host.Services.GetService<IScriptingService>() as IDisposable)?.Dispose();
            (_host.Services.GetService<ScriptingViewModel>() as IDisposable)?.Dispose();
            (_host.Services.GetService<PlayerStatusViewModel>() as IDisposable)?.Dispose();

            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
        }
    }
}
