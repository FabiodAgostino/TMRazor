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
- [x] Integrazione delle librerie native `Crypt.dll`, `Loader.dll` e `cuoapi.dll`.
- [x] Porting di `DLLImport.cs` e funzioni Win32 native.
- [x] Creazione `IClientInteropService` (avvio client OSI / ClassicUO, patch in memoria).
- [x] Porting dell'engine di Rete: creazione `IPacketService`.
- [x] Implementazione del routing dei pacchetti (dal client al server e viceversa).
- [x] Creazione sistema ad eventi asincroni tramite `IMessenger` per la notifica alla UI.

### Mondo di Gioco (World State)
- [x] Creazione `IWorldService` (porting da `Core/World.cs`).
- [x] Definizione modelli `UOEntity`, `Mobile`, `Item` nel progetto Shared.
- [x] Sistema di aggiornamento thread-safe dello stato del giocatore (HP, Mana, Stamina).
- [x] Porting sistema gump (`Gumps.cs`) e object property list (`ObjectPropertyList.cs`).

### Configurazione & Localizzazione
- [x] Creazione `IConfigService` per la gestione di Profili e Impostazioni globali (JSON).
- [x] Creazione `ILanguageService` per gestione stringhe UI e Cliloc UO.
- [x] LanguageHelper con risorse attuali (.iso files).
- [x] Implementazione caricamento/salvataggio automatico del file di profilo attivo.

## ⚙️ Fase 2.5: Sincronizzazione Threading e Sicurezza UI (Cruciale)
*Questa fase è obbligatoria prima di procedere con la UI per prevenire Cross-Thread Exceptions e memory leaks in .NET 10.*
- [x] Refactoring dei vecchi `System.Threading.Thread` in `Task` asincroni (TAP).
- [x] Implementazione di `CancellationTokenSource` per tutti i servizi di background (Agenti, Scripting, Macro).
- [x] Rimozione totale di `Thread.Abort()` (non supportato in .NET 10) e sostituzione con check cooperativi (`token.ThrowIfCancellationRequested()`).
- [x] Isolamento dei Modelli di stato (`UOEntity`, `Mobile`, `Item`) per renderli thread-safe (rimozione o protezione di `INotifyPropertyChanged` diretto da thread di rete).
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
- [ ] Creazione `OptionsViewModel`.
- [ ] Creazione `OptionsPage.xaml`.
- [ ] Sotto-sezione Filtri (Light, Weather, Sound, Death, Staff Npcs).
- [ ] Binding delle opzioni direttamente ai flag di `IConfigService`.

### 3.3 Scheda: Display & Counters
- [ ] Creazione `DisplayViewModel`.
- [ ] Creazione `DisplayPage.xaml`.
- [ ] UI per definire i Counter (Titlebar, FPS, formato testo spell).
- [ ] Sincronizzazione in tempo reale con i dati di gioco.

### 3.4 Scheda: Hotkeys (Tasti Rapidi)
- [ ] Creazione `HotkeysViewModel`.
- [ ] Creazione `HotkeysPage.xaml`.
- [ ] Implementazione componente albero gerarchico (`TreeView` stile WPF) per categorie.
- [ ] Gestione cattura input tastiera (hook di sistema) e binding del key selezionato.

### 3.5 Agenti (AutoLoot, Scavenger, Organizer)
- [ ] **AutoLoot**: `AutoLootViewModel` e `AutoLootPage.xaml`. ListView per lista item, bottone "Set Container".
- [ ] **Scavenger**: `ScavengerViewModel` e `ScavengerPage.xaml`.
- [ ] **Organizer**: `OrganizerViewModel` e `OrganizerPage.xaml`. Configurazione hotbag e target bag.
- [ ] **Vendor Buy/Sell**: Porting interfacce di compravendita e lista priorità.
- [ ] Componente UI riutilizzabile: *Object Inspector* (Sostituzione di `EnhancedItemInspector` WinForms).

### 3.6 Agenti: Combat & Dress
- [ ] **Bandage/Heal**: Configurazione timer e trigger HP.
- [ ] **Dress**: `DressViewModel` e `DressPage.xaml`.
- [ ] Implementazione griglia visuale (simil-paperdoll) per gli slot equipaggiamento.

### 3.7 Agenti: Target & Friends
- [ ] **Targeting**: Liste Friends ed Enemies.
- [ ] UI per inserimento rapido ID/Nomi e configurazione priorità (Health, Distance).

## 🧑‍💻 Fase 4: Scripting ed Editor (Il Cuore Avanzato)
- [ ] Creazione `ScriptingViewModel`.
- [ ] Creazione `ScriptingPage.xaml`.
- [ ] Sostituzione di `FastColoredTextBox` con `AvalonEdit` di ICSharpCode.
- [ ] Scrittura file di definizione sintassi `.xshd` per le keyword di UOSteam (es. `msg`, `cast`, `waitfortarget`).
- [ ] Scrittura file `.xshd` per Python (IronPython).
- [ ] Implementazione pulsanti Start, Stop, Record, Step-by-Step.
- [ ] Log Panel (Console di output) integrato con MVVM per aggiornamenti thread-safe.
- [x] Integrazione del motore `IronPython` all'interno dello `IScriptingService`. — Architettura completa: cancellazione via `sys.settrace` + `ScriptCancellationController`, API virtuali (ItemsApi/MobilesApi/PlayerApi/MiscApi), output redirect via `ScriptOutputWriter`.
- [ ] Integrazione dell'interprete sintassi C#/UOSteam nativo.

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
- [ ] Setup progetto `TMRazorImproved.Tests` (xUnit / NUnit).
- [ ] Unit Test per il parsing dei pacchetti (`UOBufferReader` validation).
- [ ] Unit Test per la logica di filtraggio e routing del `PacketService`.
- [ ] Mocking del client UO per testare gli Agenti (AutoLoot, Scavenger) in ambiente isolato.
- [ ] Test di regressione per il motore di scripting (UOSteam/Python syntax).

---
**Istruzioni d'uso per l'Assistente AI:**
Ogni volta che si riprende il lavoro o si cambia contesto, l'agente deve consultare questo file, marcare con una `[x]` i task completati e pianificare il prossimo micro-step basato sulla struttura definita.
