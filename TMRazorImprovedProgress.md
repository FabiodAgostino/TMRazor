# TMRazor Improved - Piano di Sviluppo e Checklist di Migrazione

Questo documento traccia il progresso granulare della riscrittura di TMRazor in WPF, MVVM e .NET 10. Serve come punto di ripristino del contesto per l'agente IA.

## 🏗️ Fase 1: Setup dell'Infrastruttura (Architettura)
- [x] Creazione Solution `.sln` in .NET 10.
- [x] Setup progetto `TMRazorImproved.Core` (Logica di business pura).
- [x] Setup progetto `TMRazorImproved.Shared` (Interfacce, DTO, Enums comuni).
- [x] Setup progetto `TMRazorImproved.UI` (WPF UI).
- [x] Porting e modernizzazione `UltimaSDK` a .NET 10.
- [x] Configurazione architettura x86 (32-bit) cross-solution per compatibilità client UO.
- [x] Installazione pacchetti essenziali (`WPF-UI`, `CommunityToolkit.Mvvm`, `Microsoft.Extensions.DependencyInjection`, `AvalonEdit`, `Newtonsoft.Json`).
- [x] Setup `App.xaml` con risorse WPF-UI 3.0.5.
- [x] Costruzione scheletro `MainWindow.xaml` con `ui:FluentWindow` e navigazione.
- [x] Configurazione dell'`IHost` in `App.xaml.cs` (Bootstrapper per Dependency Injection).
- [x] Registrazione nel DI container di `NavigationService`, `PageService` e `IMessenger`.
- [x] Creazione sistema di navigazione per le sotto-pagine nel `RootFrame`.

## 🧠 Fase 2: Estrazione Motore e Servizi (TMRazorImproved.Core)
*L'obiettivo qui è isolare la logica da qualsiasi dipendenza visiva.*

### Client & Rete
- [x] Integrazione delle librerie native `Crypt.dll`, `Loader.dll` e `cuoapi.dll`. — Marshalling sicuro per .NET 10 (CharSet.Ansi, UnmanagedType.LPStr).
- [x] Porting di `DLLImport.cs` e funzioni Win32 native.
- [x] Creazione `IClientInteropService` (avvio client OSI / ClassicUO, patch in memoria). — Accesso a Shared Memory, CommMutex e GetPacketLength.
- [x] Porting dell'engine di Rete: creazione `IPacketService`. — Implementazione `OnMessage` per WndProc hook.
- [x] Implementazione del routing dei pacchetti (dal client al server e viceversa). — Accesso diretto ai buffer circolari `SharedBuffer` nativi, estrazione pacchetti e re-iniezione se non filtrati.
- [x] Creazione sistema ad eventi asincroni tramite `IMessenger` per la notifica alla UI.

### Mondo di Gioco (World State)
- [x] Creazione `IWorldService` (porting da `Core/World.cs`). — Gestione concorrente di Mobiles e Items, stato del Player e dei Gump.
- [x] Definizione modelli `UOEntity`, `Mobile`, `Item` nel progetto Shared. — Strutture dati base con supporto a coordinate, grafiche, hue e statistiche.
- [x] Sistema di aggiornamento thread-safe dello stato del giocatore (HP, Mana, Stamina). — Implementazione `WorldPacketHandler` con supporto a 0x11, 0x1B, 0xBF.08, 0x78, 0x1A, 0x1D e 0x20.
- [x] **Sprint 2 — Packet Handler Completi (Classic UO)**: Esteso `WorldPacketHandler` con 25+ handler per copertura quasi totale del protocollo UO:
  - **Movimento**: 0x77 MobileMoving, 0x21 WalkReject
  - **Inventory**: 0x3C ContainerContent, 0x25 AddItemToContainer, 0x2E EquipUpdate, 0x89 CorpseEquipment
  - **Item SA**: 0xF3 SAWorldItem (Stygian Abyss / High Seas)
  - **Combat**: 0x0B Damage, 0x72 WarMode, 0xAA AttackOK
  - **Messages**: 0xC1 UnicodeClocMessage, 0xC2 UnicodeClocMessageAffix
  - **Session**: 0x55 LoginComplete, 0x8C RelayServer, 0x73 Ping, 0xBD ClientVersion
  - **Misc**: 0x88 OpenPaperdoll, 0x98 MobileName, 0x6D PlayMusic, 0xC0 GraphicalEffect, 0xDF BuffDebuff, 0x7C OpenMenu, 0x90 MapDisplay, 0x6C TargetCursor(S2C)
  - **Extended 0xBF**: sub 0x14 CloseGump, 0x19 ExtendedStats (AOS resistenze/luck/danni), 0x25 MapChange
  - **Mobile model**: aggiunte proprietà WarMode, AttackTarget, Gold, Armor, Weight, MaxWeight, StatCap, Followers, FollowersMax, FireResist, ColdResist, PoisonResist, EnergyResist, Luck, MinDamage, MaxDamage, TithingPoints, MapId
  - **Nuovi Message types**: `WorldStateMessages.cs` (ContainerContent, ContainerItemAdded, EquipmentChanged, LoginComplete, MobileName), `CombatMessages.cs` (WarMode, Damage, AttackTarget), `EffectMessages.cs` (TargetCursor, MobileMoving, GraphicalEffect, BuffDebuff, Music)
  - **IWorldService**: aggiunto `LastOpenedContainer` + `SetLastOpenedContainer()`
- [x] Porting sistema gump (`Gumps.cs`) e object property list (`ObjectPropertyList.cs`). — Parsing di 0xB0 (Gump) e 0xD6 (OPL) integrato nel WorldPacketHandler.

### Configurazione & Localizzazione
- [x] Creazione `IConfigService` per la gestione di Profili e Impostazioni globali (JSON).
- [x] Creazione `ILanguageService` per gestione stringhe UI e Cliloc UO.
- [x] Migrazione file `.resx` originali da TMRazor e integrazione via `ResourceManager`.
- [x] Implementazione estensione XAML `{loc:Loc}` per localizzazione dichiarativa delle View.
- [x] Localizzazione completa di tutte le pagine UI (General, Options, Hotkeys, Inspector, Scripting).
- [x] Sostituzione stringhe hardcoded nei ViewModel con chiavi di localizzazione.
- [x] LanguageHelper con risorse attuali (.iso files e .resx).
- [x] Implementazione caricamento/salvajaggio automatico del file di profilo attivo.

## ⚙️ Fase 2.5: Sincronizzazione Threading e Sicurezza UI (Cruciale)
*Questa fase è obbligatoria prima di procedere con la UI per prevenire Cross-Thread Exceptions e memory leaks in .NET 10.*
- [x] Refactoring dei vecchi `System.Threading.Thread` in `Task` asincroni (TAP).
- [x] Implementazione di `CancellationTokenSource` per tutti i servizi di background (Agenti, Scripting, Macro).
- [x] Rimozione totale di `Thread.Abort()` (non supportato in .NET 10) e sostituzione con check cooperativi (`token.ThrowIfCancellationRequested()`).
- [x] Marshalling Sicuro P/Invoke: Configurazione esplicita di CharSet e MarshalAs per compatibilità con le protezioni di memoria di .NET 10.
- [x] Isolamento dei Modelli di stato (`UOEntity`, `Mobile`, `Item`) per renderli thread-safe tramite `SyncRoot` e `Snapshots` immutabili.
- [x] Configurazione di `BindingOperations.EnableCollectionSynchronization` per tutte le `ObservableCollection` usate nei ViewModel.
- [x] Configurazione del Dispatching esplicito (`Application.Current.Dispatcher.Invoke`) per l'aggiornamento sicuro delle property singole da parte di eventi `IMessenger`.
- [x] Implementazione meccanismi di Throttling/Debouncing per l'aggiornamento UI ad alta frequenza (es. HP, Status). — `UiThrottler` (DispatcherTimer 100ms), `PlayerStatusMessage`, `PlayerStatusViewModel` con flag volatile.

## 🎨 Fase 3: Interfacce e ViewModel (TMRazorImproved.UI)
*Sostituzione completa del "MainForm" monolitico con View separate collegate via Data Binding.*

### 3.1 Scheda: General (Login e Status)
- [x] Creazione `GeneralViewModel`.
- [x] Creazione `GeneralPage.xaml`.
- [x] Binding input: Percorso Client, Percorso Data, IP Server, Porta.
- [ ] Checkbox per Patch (Encryption, Multi, ecc.).
- [x] Pulsante "Launch UO" con logica collegata a `IClientInteropService`.
- [x] Status message e integrazione DI per il lancio.

### 3.2 Scheda: Options (Filtri e Tweaks)
- [x] Creazione `OptionsViewModel`.
- [x] Creazione `OptionsPage.xaml`.
- [x] Sotto-sezione Filtri (Light, Weather, Sound, Death, Staff Npcs).
- [x] Binding delle opzioni direttamente ai flag di `IConfigService`.
- [x] Implementazione `FilterHandler` per intercettazione pacchetti basata su config.

### 3.3 Scheda: Display & Counters
- [ ] Creazione `DisplayViewModel`.
- [ ] Creazione `DisplayPage.xaml`.
- [ ] UI per definire i Counter (Titlebar, FPS, formato testo spell).
- [x] Implementazione `TitleBarService` per l'aggiornamento del titolo della finestra UO.
- [ ] Sincronizzazione in tempo reale con i dati di gioco.

### 3.4 Scheda: Hotkeys (Tasti Rapidi)
- [x] Creazione `HotkeysViewModel`.
- [x] Creazione `HotkeysPage.xaml`.
- [x] Implementazione componente albero gerarchico (`TreeView` stile WPF) per categorie.
- [x] Gestione cattura input tastiera (hook di sistema) e binding del key selezionato.
- [x] Implementazione `HotkeyService` con Low-Level Keyboard Hook (WH_KEYBOARD_LL).
- [x] Supporto per modificatori (Ctrl, Alt, Shift) e mappatura azioni dal profilo.
- [x] Registrazione azioni per AutoLoot e Scavenger (Start/Stop/Toggle).

### 3.5 Agenti (AutoLoot, Scavenger, Organizer)
- [x] Implementazione logica `AutoLootService` (intercettazione 0x3C/0x25, invio 0x07/0x08).
- [x] Implementazione logica `ScavengerService` (intercettazione 0x1A, calcolo distanza).
- [x] Implementazione logica `OrganizerService` (scansione container sorgente, spostamento item a destinazione).
- [x] **AutoLoot**: `AutoLootViewModel` e `AutoLootPage.xaml`. ListView per lista item, bottone "Set Container".
- [x] **Scavenger**: `ScavengerViewModel` e `ScavengerPage.xaml`.
- [x] **Organizer**: `OrganizerViewModel` e `OrganizerPage.xaml`. Configurazione hotbag e target bag.
- [ ] **Vendor Buy/Sell**: Porting interfacce di compravendita e lista priorità.
- [ ] Componente UI riutilizzabile: *Object Inspector* (Sostituzione di `EnhancedItemInspector` WinForms).

### 3.6 Agenti: Combat & Dress
- [x] **Bandage/Heal**: Implementazione logica `BandageHealService` con calcolo delay DEX-based e iniezione pacchetti (0x06, 0x6C).
- [x] **Bandage/Heal UI**: `BandageHealViewModel` e `BandageHealPage.xaml`. Configurazione soglie e bende.
- [x] **Dress**: Implementazione logica `DressService` con gestione liste di equipaggiamento, code di azioni e delay di sicurezza.
- [ ] Implementazione griglia visuale (simil-paperdoll) per gli slot equipaggiamento.

### 3.7 Agenti: Target & Friends
- [x] **Targeting**: Logica `TargetingService` (Next, Closest, Self, Last Target).
- [x] Gestione liste Friends (Filtri nel targeting).
- [x] **Targeting & Friends UI**: `TargetingViewModel` e `TargetingPage.xaml`. Inserimento rapido ID/Nomi e configurazione priorità.

## 🧑‍💻 Fase 4: Scripting ed Editor (Il Cuore Avanzato)
- [x] Creazione `ScriptingViewModel`. — comandi Run/Stop/New/Open/Save, ObservableCollection log thread-safe, subscribe eventi IScriptingService.
- [x] Creazione `ScriptingPage.xaml`. — layout toolbar + AvalonEdit editor (2/3) + console output (1/3) con GridSplitter.
- [x] Sostituzione di `FastColoredTextBox` con `AvalonEdit` di ICSharpCode. — sync bidirezionale ViewModel↔editor in code-behind, tab→spaces, line numbers.
- [x] Scrittura file di definizione sintassi `.xshd` per le keyword di UOSteam (es. `msg`, `cast`, `waitfortarget`).
- [x] Scrittura file `.xshd` per Python (IronPython). — `Python.xshd` EmbeddedResource, colori VSCode-like (keyword, UO API objects, builtin, commenti, stringhe multiline, numeri).
- [x] Implementazione pulsanti Start, Stop, New, Open, Save. — stato abilitato/disabilitato basato su `IsRunning`.
- [x] Log Panel (Console di output) integrato con MVVM per aggiornamenti thread-safe. — auto-scroll, colori per tipo (Output=grigio, Error=rosso, System=giallo), timestamp per riga.
- [x] Integrazione del motore `IronPython` all'interno dello `IScriptingService`. — Architettura completa: cancellazione via `sys.settrace`, API virtuali, output redirect via `scope.__stdout__`/`__stderr__` → `sys.stdout/stderr` nel preamble (fix API IronPython 3.4.2).
- [x] Integrazione dell'interprete sintassi UOSteam minimale.

## 🛠️ Fase 5: Strumenti Aggiuntivi (Griglie e Radar)
- [ ] Ricreazione `SpellGrid.cs` (Griglia floating su schermo) usando una Window WPF semi-trasparente senza bordi (`AllowsTransparency="True"`).
- [ ] Ricreazione `ToolBar.cs` (Barra HP/Mana a schermo) come Window "Topmost" WPF.
- [x] Porting logica della Map/Radar (richiede conversione da `System.Drawing` a WPF `WriteableBitmap` per alte prestazioni). — *COMPLETATO: Estirpato `System.Drawing.Common` da `UltimaSDK` tramite il mock `Ultima.Data.Bitmap`, emette byte[] grezzi pronti per WPF `BitmapSource.Create`.*

## 🚀 Fase 6: Polish, Testing e Rilascio
- [ ] Sostituzione delle icone legacy `.ico/.png` con icone vettoriali o `SymbolIcon` di WPF-UI dove possibile.
- [ ] Implementazione Dark Mode / Light Mode toggle (Theme Service).
- [ ] Implementazione sistema di Dialog moderni (Sostituzione di tutte le `MessageBox.Show()` con dialoghi non bloccanti nella UI).
- [ ] Test di stabilità memoria (WPF ha garbage collection diversa rispetto alla gestione GDI+).
- [ ] Setup processo di Build e Publish stand-alone (.NET 10 Single File).

## 🧪 Fase 7: Unit Testing & Quality Assurance (Post-Logic)
*Da eseguire dopo il completamento dei layer di business e logica.*
- [x] Setup progetto `TMRazorImproved.Tests` (xUnit / Moq).
- [x] Unit Test per il parsing dei pacchetti (`UOBufferReader` e `WorldPacketHandler`).
- [x] Unit Test per la logica di filtraggio e routing del `PacketService`.
- [x] Mocking del client UO per testare gli Agenti (AutoLoot, Scavenger, BandageHeal) in ambiente isolato.
- [x] Test di regressione per il motore di scripting (UOSteam/Python syntax).
- [x] **Stress & Concurrency Testing**: Implementazione di test multithread per validare la thread-safety di `WorldService` e `PacketService`.
- [x] **Fuzz Testing**: Validazione della robustezza del parser contro pacchetti malformati o casuali.
- [ ] **Integrazione "Non-Mock"**: Creazione di un layer di test che utilizzi il client reale o un simulatore di protocollo binario per test end-to-end.

---
**Istruzioni d'uso per l'Assistente AI:**
Ogni volta che si riprende il lavoro o si cambia contesto, l'agente deve consultare questo file, marcare con una `[x]` i task completati e pianificare il prossimo micro-step basato sulla struttura definita.
