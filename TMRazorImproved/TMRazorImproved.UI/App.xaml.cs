using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Linq;
using System.Windows;
using TMRazorImproved.UI.ViewModels;
using TMRazorImproved.UI.Views;
using TMRazorImproved.UI.Views.Pages;
using TMRazorImproved.UI.Views.Pages.Agents;
using TMRazorImproved.UI.Views.Windows;
using TMRazorImproved.Shared.Interfaces;
using TMRazorImproved.Shared.Models;
using TMRazorImproved.Core.Services;
using TMRazorImproved.Core.Handlers;
using TMRazorImproved.Core.Services.Scripting;
using TMRazorImproved.UI.Services;
using Wpf.Ui;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Input;

namespace TMRazorImproved.UI
{
    public partial class App : Application
    {
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                logging.AddNLog();
            })
            .ConfigureServices((context, services) =>
            {
                // UI Services
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IPageService, PageService>();
                services.AddSingleton<IContentDialogService, ContentDialogService>();
                services.AddSingleton<ISnackbarService, SnackbarService>();
                services.AddSingleton<TMRazorImproved.Shared.Interfaces.IThemeService, TMRazorImproved.UI.Services.ThemeService>();

                // Infrastructure (MVVM Messenger)
                services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

                // Windows
                services.AddSingleton<MainWindow>();
                services.AddSingleton<FloatingToolbarWindow>();
                services.AddSingleton<DPSMeterWindow>();
                services.AddSingleton<TargetHPWindow>();
                services.AddTransient<TMRazorImproved.UI.Views.Windows.SpellGridWindow>();

                // Core Services
                services.AddSingleton<IClientInteropService, ClientInteropService>();
                services.AddSingleton<ISearchService, SearchService>();
                services.AddSingleton<IPacketService, PacketService>();
                services.AddSingleton<IWorldService, WorldService>();
                services.AddSingleton<IMapService, MapService>();
                services.AddSingleton<IJournalService, JournalService>();
                services.AddSingleton<IConfigService, ConfigService>();
                services.AddSingleton<ILanguageService, LanguageService>();
                services.AddSingleton<ILogService, LogService>();
                services.AddSingleton<IScriptingService, ScriptingService>();
                services.AddSingleton<ITargetingService, TargetingService>();
                services.AddSingleton<IAutoLootService, AutoLootService>();
                services.AddSingleton<IScavengerService, ScavengerService>();
                services.AddSingleton<IOrganizerService, OrganizerService>();
                services.AddSingleton<IBandageHealService, BandageHealService>();
                services.AddSingleton<IDressService, DressService>();
                services.AddSingleton<IVendorService, VendorService>();
                services.AddSingleton<IRestockService, RestockService>();
                services.AddSingleton<ISkillsService, SkillsService>();
                services.AddSingleton<ITitleBarService, TitleBarService>();
                services.AddSingleton<IHotkeyService, HotkeyService>();
                services.AddSingleton<IMacrosService, MacrosService>();
                services.AddSingleton<IFriendsService, FriendsService>();
                services.AddSingleton<ISoundService, SoundService>();
                services.AddSingleton<IUltimaImageCache, UltimaImageCache>();
                services.AddSingleton<ICounterService, CounterService>();
                services.AddSingleton<IDPSMeterService, DPSMeterService>();
                services.AddSingleton<IPathFindingService, PathFindingService>();
                services.AddSingleton<ISecureTradeService, SecureTradeService>();
                services.AddSingleton<IScreenCaptureService, ScreenCaptureService>();
                services.AddSingleton<IVideoCaptureService, VideoCaptureService>();
                
                // Packet Handlers
                services.AddSingleton<WorldPacketHandler>();
                services.AddSingleton<FilterHandler>();
                services.AddSingleton<FriendsHandler>();

                // ViewModels
                services.AddSingleton<SearchViewModel>();
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<GeneralViewModel>();
                services.AddSingleton<JournalViewModel>();
                services.AddSingleton<PacketLoggerViewModel>();
                services.AddSingleton<SecureTradeViewModel>();
                services.AddSingleton<GalleryViewModel>();
                services.AddSingleton<PlayerStatusViewModel>();
                services.AddSingleton<ScriptingViewModel>();
                services.AddSingleton<MacrosViewModel>();
                services.AddSingleton<FriendsViewModel>();
                services.AddSingleton<FiltersViewModel>();
                services.AddSingleton<FloatingToolbarViewModel>();
                services.AddSingleton<DPSMeterViewModel>();
                services.AddSingleton<TargetHPViewModel>();
                services.AddSingleton<OptionsViewModel>();
                services.AddSingleton<DisplayViewModel>();
                services.AddSingleton<SoundViewModel>();
                services.AddSingleton<CountersViewModel>();
                services.AddSingleton<HotkeysViewModel>();
                services.AddSingleton<InspectorViewModel>();
                services.AddSingleton<SkillsViewModel>();
                services.AddSingleton<SpellGridViewModel>();
                services.AddSingleton<TMRazorImproved.UI.ViewModels.Agents.AutoLootViewModel>();
                services.AddSingleton<TMRazorImproved.UI.ViewModels.Agents.ScavengerViewModel>();
                services.AddSingleton<TMRazorImproved.UI.ViewModels.Agents.BandageHealViewModel>();
                services.AddSingleton<TMRazorImproved.UI.ViewModels.Agents.TargetingViewModel>();
                services.AddSingleton<TMRazorImproved.UI.ViewModels.Agents.OrganizerViewModel>();
                services.AddSingleton<TMRazorImproved.UI.ViewModels.Agents.VendorViewModel>();
                services.AddSingleton<TMRazorImproved.UI.ViewModels.Agents.RestockViewModel>();
                services.AddSingleton<TMRazorImproved.UI.ViewModels.Agents.DressViewModel>();

                // Pages
                // Sprint Fix-3: le pagine stateless sono Transient per evitare memory leak
                // e accumulo di stato visivo stale. Le pagine con stato significativo
                // (editor, log, cronologia visibile) rimangono Singleton.
                services.AddTransient<DashboardPage>();
                services.AddTransient<GeneralPage>();
                services.AddSingleton<JournalPage>();       // mantiene scroll + filtri
                services.AddSingleton<PacketLoggerPage>();  // mantiene la lista pacchetti
                services.AddTransient<SecureTradePage>();
                services.AddTransient<GalleryPage>();
                services.AddSingleton<ScriptingPage>();     // mantiene contenuto editor
                services.AddSingleton<MacrosPage>();        // mantiene selezione macro
                services.AddTransient<FiltersPage>();
                services.AddTransient<SoundPage>();
                services.AddTransient<CountersPage>();
                services.AddTransient<OptionsPage>();
                services.AddTransient<DisplayPage>();
                services.AddTransient<HotkeysPage>();
                services.AddTransient<InspectorPage>();
                services.AddTransient<TMRazorImproved.UI.Views.Pages.SkillsPage>();
                services.AddTransient<TMRazorImproved.UI.Views.Pages.Agents.AutoLootPage>();
                services.AddTransient<TMRazorImproved.UI.Views.Pages.Agents.ScavengerPage>();
                services.AddTransient<TMRazorImproved.UI.Views.Pages.Agents.BandageHealPage>();
                services.AddTransient<TMRazorImproved.UI.Views.Pages.Agents.TargetingPage>();
                services.AddTransient<TMRazorImproved.UI.Views.Pages.Agents.OrganizerPage>();
                services.AddTransient<TMRazorImproved.UI.Views.Pages.Agents.RestockPage>();
                services.AddTransient<TMRazorImproved.UI.Views.Pages.Agents.VendorPage>();
                services.AddTransient<TMRazorImproved.UI.Views.Pages.Agents.DressPage>();
                services.AddTransient<TMRazorImproved.UI.Views.Pages.Agents.FriendsPage>();
            }).Build();

        public static T? GetService<T>() where T : class
            => _host.Services.GetService(typeof(T)) as T;

        protected override async void OnStartup(StartupEventArgs e)
        {
            try 
            {
                await _host.StartAsync();

                // Inizializzazione SearchService
                InitializeSearchService();

                // Inizializzazione Servizi Core
                var config = _host.Services.GetRequiredService<IConfigService>();
                var lang = _host.Services.GetRequiredService<ILanguageService>();
                var theme = _host.Services.GetRequiredService<TMRazorImproved.Shared.Interfaces.IThemeService>();

                // Applica Tema configurato
                theme.ApplyTheme(config.Global.Theme);
                theme.ApplyAccentColor(config.Global.AccentColor);

                lang.Load(config.Global.Language);
                
                _host.Services.GetRequiredService<IMapService>().Initialize(config.Global.DataPath);
                
                _host.Services.GetRequiredService<WorldPacketHandler>();
                _host.Services.GetRequiredService<FilterHandler>();
                _host.Services.GetRequiredService<FriendsHandler>();
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

        private void InitializeSearchService()
        {
            var search = _host.Services.GetRequiredService<ISearchService>();
            var nav = _host.Services.GetRequiredService<INavigationService>();

            // Registrazione Pagine (Statiche)
            search.RegisterItem(new SearchItem("Dashboard", SearchCategory.Page, new RelayCommand(() => nav.Navigate(typeof(DashboardPage))), "Main overview", "\uE10F"));
            search.RegisterItem(new SearchItem("Journal", SearchCategory.Page, new RelayCommand(() => nav.Navigate(typeof(JournalPage))), "In-game message log", "\uE8A2"));
            search.RegisterItem(new SearchItem("Packet Logger", SearchCategory.Page, new RelayCommand(() => nav.Navigate(typeof(PacketLoggerPage))), "Network traffic monitor", "\uE945"));
            search.RegisterItem(new SearchItem("Secure Trade", SearchCategory.Page, new RelayCommand(() => nav.Navigate(typeof(SecureTradePage))), "Secure trade monitor", "\uE815"));
            search.RegisterItem(new SearchItem("Gallery", SearchCategory.Page, new RelayCommand(() => nav.Navigate(typeof(GalleryPage))), "Screenshots and videos", "\uE114"));
            search.RegisterItem(new SearchItem("General", SearchCategory.Page, new RelayCommand(() => nav.Navigate(typeof(GeneralPage))), "General settings", "\uE713"));
            search.RegisterItem(new SearchItem("Options", SearchCategory.Page, new RelayCommand(() => nav.Navigate(typeof(OptionsPage))), "Global options", "\uE713"));
            search.RegisterItem(new SearchItem("Display", SearchCategory.Page, new RelayCommand(() => nav.Navigate(typeof(DisplayPage))), "Display and Visual settings", "\uE7B3"));
            search.RegisterItem(new SearchItem("Scripting", SearchCategory.Page, new RelayCommand(() => nav.Navigate(typeof(ScriptingPage))), "Python and UOSteam editor", "\uE943"));
            search.RegisterItem(new SearchItem("Macros", SearchCategory.Page, new RelayCommand(() => nav.Navigate(typeof(MacrosPage))), "Macro recorder and player", "\uE7C8"));
            search.RegisterItem(new SearchItem("Skills", SearchCategory.Page, new RelayCommand(() => nav.Navigate(typeof(SkillsPage))), "Character skills list", "\uEADB"));
            
            // Agents
            search.RegisterItem(new SearchItem("AutoLoot", SearchCategory.Agent, new RelayCommand(() => nav.Navigate(typeof(AutoLootPage))), "Automated corpse looting", "\uE8B7"));
            search.RegisterItem(new SearchItem("Scavenger", SearchCategory.Agent, new RelayCommand(() => nav.Navigate(typeof(ScavengerPage))), "Pick up items from the ground", "\uE8B7"));
            search.RegisterItem(new SearchItem("Bandage Heal", SearchCategory.Agent, new RelayCommand(() => nav.Navigate(typeof(BandageHealPage))), "Auto bandage healing", "\uE706"));
            search.RegisterItem(new SearchItem("Organizer", SearchCategory.Agent, new RelayCommand(() => nav.Navigate(typeof(OrganizerPage))), "Move items between containers", "\uE179"));
            search.RegisterItem(new SearchItem("Restock", SearchCategory.Agent, new RelayCommand(() => nav.Navigate(typeof(RestockPage))), "Get supplies from bank/containers", "\uE896"));
            search.RegisterItem(new SearchItem("Dress", SearchCategory.Agent, new RelayCommand(() => nav.Navigate(typeof(DressPage))), "Equip/Unequip gear sets", "\uE117"));
            search.RegisterItem(new SearchItem("Friends", SearchCategory.Agent, new RelayCommand(() => nav.Navigate(typeof(FriendsPage))), "Manage friends and foes list", "\uE716"));

            // Dinamici: Script e Macro verranno registrati dai rispettivi servizi
            search.RegisterCategory(SearchCategory.Script, () => {
                var scriptService = _host.Services.GetService<IScriptingService>();
                return scriptService?.GetLoadedScripts().Select(s => new SearchItem(s, SearchCategory.Script, 
                    new RelayCommand(() => scriptService.RunScript(s)), "Run script", "\uE768")) ?? Enumerable.Empty<SearchItem>();
            });
            
            search.RegisterCategory(SearchCategory.Macro, () => {
                var macroService = _host.Services.GetService<IMacrosService>();
                return macroService?.MacroList.Select(m => new SearchItem(m, SearchCategory.Macro, 
                    new RelayCommand(() => macroService.Play(m)), "Play macro", "\uE768")) ?? Enumerable.Empty<SearchItem>();
            });
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            // Disponi esplicitamente i servizi che implementano IDisposable
            (_host.Services.GetService<IScriptingService>() as IDisposable)?.Dispose();
            (_host.Services.GetService<JournalViewModel>() as IDisposable)?.Dispose();
            (_host.Services.GetService<PacketLoggerViewModel>() as IDisposable)?.Dispose();
            (_host.Services.GetService<SecureTradeViewModel>() as IDisposable)?.Dispose();
            (_host.Services.GetService<ScriptingViewModel>() as IDisposable)?.Dispose();
            (_host.Services.GetService<PlayerStatusViewModel>() as IDisposable)?.Dispose();

            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
        }
    }
}
