# Review Architetturale — TMRazor Improved
**Data**: 2026-03-04
**Revisore**: Senior Software Architect
**Framework**: .NET 10 / WPF / MVVM / IronPython 3.4.2
**Build target**: x86 (compatibilità DLL native UO)

---

## 1. Executive Summary

Il progetto è strutturato in modo solido e dimostra una chiara comprensione dell'architettura a microservizi con DI, pattern MVVM e separazione delle responsabilità. La base infrastrutturale (Fasi 1–2) è completa e di buona qualità. Il core di scripting (IronPython + UOSteam + Roslyn) è particolarmente maturo, con un sistema di cancellazione a due livelli ben documentato.

Sono stati identificati **4 bug P0 critici** che possono causare crash o comportamento scorretto in produzione, **4 bug P1 importanti**, e **5 issues P2**. Circa il 35% della feature parity con TMRazor originale è ancora da migrare.

---

## 2. Architecture Diagram

```
╔══════════════════════════════════════════════════════════════════════════════════╗
║                        TMRazor Improved — Architecture                          ║
╚══════════════════════════════════════════════════════════════════════════════════╝

┌─────────────────────────────── UI LAYER (WPF) ──────────────────────────────────┐
│                                                                                  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐│
│  │  MainWindow  │  │ FloatingToolbar│ │ DPSMeterWindow│  │  SpellGridWindow ✅  ││
│  │     ✅       │  │    ⚠️ stub   │  │    ✅        │  │                      ││
│  └──────┬───────┘  └──────────────┘  └──────────────┘  └──────────────────────┘│
│         │                                                                        │
│  ┌──────▼──────────────────────────────────────────────────────────────────────┐│
│  │                         PAGES / VIEWMODELS                                  ││
│  │                                                                              ││
│  │  CORE PAGES                          AGENT PAGES                            ││
│  │  ✅ DashboardPage/VM                 ✅ AutoLootPage/VM                     ││
│  │  ✅ JournalPage/VM                   ✅ ScavengerPage/VM                    ││
│  │  ✅ SkillsPage/VM                    ✅ BandageHealPage/VM                  ││
│  │  ✅ MacrosPage/VM (con editor)       ✅ OrganizerPage/VM                    ││
│  │  ✅ ScriptingPage/VM (AvalonEdit)    ✅ DressPage/VM                        ││
│  │  ✅ PacketLoggerPage/VM              ✅ RestockPage/VM                       ││
│  │  ✅ InspectorPage/VM                 ✅ VendorPage/VM                       ││
│  │  ✅ FiltersPage/VM                   ✅ FriendsPage/VM                      ││
│  │  ✅ HotkeysPage/VM                   ⚠️ TargetingPage/VM (parziale)        ││
│  │  ✅ GeneralPage/VM                                                           ││
│  │  ✅ OptionsPage/VM                   FLOATING WINDOWS                       ││
│  │  ⚠️ DisplayPage/VM (placeholder)    ✅ HuePickerWindow                     ││
│  │  ⚠️ SoundPage/VM (placeholder)      ✅ MapWindow                            ││
│  │  ⚠️ CountersPage/VM (parziale)      ✅ TargetHPWindow                      ││
│  │  ❌ SecureTradePage/VM (stub)        ⚠️ FloatingToolbarWindow (stub)        ││
│  │  ❌ GalleryPage/VM (stub)            ❌ PathfindingVisualizer               ││
│  │                                      ❌ GumpListManager                     ││
│  └──────────────────────────────────────────────────────────────────────────────┘│
│                                                                                  │
│  UTILITY UI                                                                      │
│  ✅ ViewModelBase (RunOnUIThread)  ✅ UiThrottler (10fps HP/Mana)               │
│  ✅ AvalonEditBehavior             ✅ CompletionService (autocompletamento)      │
│  ✅ UltimaImageCache               ✅ ThemeService                               │
└──────────────────────────────────────────────────────────────────────────────────┘
                                    │ DI via IHost
┌─────────────────────────── CORE LAYER ──────────────────────────────────────────┐
│                                                                                  │
│  PACKET HANDLERS                                                                 │
│  ✅ WorldPacketHandler (50+ pacchetti S2C/C2S)                                  │
│  ✅ FilterHandler (light/weather/sound/footsteps/poison/karma/snoop)            │
│  ✅ FriendsHandler                                                               │
│                                                                                  │
│  INFRASTRUCTURE SERVICES                                                         │
│  ✅ ClientInteropService    ✅ PacketService    ✅ ConfigService                 │
│  ✅ LanguageService         ✅ LogService       ✅ TitleBarService               │
│  ✅ HotkeyService                                                                │
│                                                                                  │
│  GAME WORLD SERVICES                                                             │
│  ✅ WorldService (ConcurrentDictionary, volatile Player)                        │
│  ✅ TargetingService (S2C cursor tracking, TargetCursorRequested event)         │
│  ✅ SkillsService (0x3A parser, while+break terminator)   ⚠️ thread-safety     │
│  ✅ JournalService          ✅ FriendsService   ✅ MapService                   │
│  ✅ PathFindingService (A*) ✅ CounterService   ✅ DPSMeterService              │
│  ⚠️ SecureTradeService (stub)  ⚠️ SoundService (stub)                         │
│  ⚠️ ScreenCaptureService (stub) ⚠️ VideoCaptureService (stub)                  │
│  ⚠️ SearchService              ✅ MacrosService (IF/WHILE/FOR engine)           │
│                                                                                  │
│  AGENT SERVICES (AgentServiceBase → CancellationToken + Task)                   │
│  ✅ AutoLootService    ✅ ScavengerService    ✅ BandageHealService ⚠️ bug P0   │
│  ✅ OrganizerService   ✅ DressService        ✅ VendorService                  │
│  ✅ RestockService                                                               │
│  ❌ AutoCarverService   ❌ BoneCutterService   ❌ AutoRemountService            │
│                                                                                  │
│  SCRIPTING ENGINE                                                                │
│  ✅ ScriptingService (IronPython + Roslyn CSharp + UOSteam)                    │
│  ✅ ScriptCancellationController (sys.settrace + Thread.Interrupt)              │
│  ✅ ScriptOutputWriter                                                           │
│  ✅ UOSteamInterpreter (basilare)  ⚠️ comandi avanzati mancanti               │
│                                                                                  │
│  SCRIPTING API (tutte virtual per IronPython DLR binder)                        │
│  ✅ ItemsApi    ✅ MobilesApi   ✅ PlayerApi   ✅ TargetApi                     │
│  ✅ JournalApi  ✅ GumpsApi     ✅ SkillsApi   ✅ SpellsApi                     │
│  ✅ StaticsApi  ✅ FriendApi    ✅ FiltersApi  ✅ MiscApi                       │
│                                                                                  │
│  UTILITIES                                                                       │
│  ✅ PacketBuilder (centralizzato, 12+ builders)                                 │
│  ✅ IMapDataProvider / UltimaMapDataProvider (UltimaSDK)                        │
└──────────────────────────────────────────────────────────────────────────────────┘
                                    │
┌──────────────────────────── SHARED LAYER ───────────────────────────────────────┐
│                                                                                  │
│  INTERFACES (25+): IWorldService, IPacketService, ITargetingService,            │
│                    ISkillsService, IScriptingService, IAgentService...           │
│                                                                                  │
│  MODELS: UOEntity (Mobile/Item), SkillInfo, JournalEntry, UOGump,               │
│          SpellIcon, SpellDefinitions, MobileSnapshot, TradeSession...           │
│                                                                                  │
│  MESSAGES (CommunityToolkit.Mvvm.Messaging):                                    │
│  WorldStateMessages, CombatMessages, EffectMessages, PlayerStatusMessage        │
│                                                                                  │
│  ENUMS: Layer, LocString, PacketPath, ScriptLanguage                            │
│  CONFIG: GlobalSettings, UserProfile, AgentConfigs (multi-list)                 │
└──────────────────────────────────────────────────────────────────────────────────┘
                                    │
┌──────────────────────────── EXTERNAL ───────────────────────────────────────────┐
│  📦 UltimaSDK (x86)          📦 IronPython 3.4.2       📦 WPF-UI 3.0.5        │
│  📦 CommunityToolkit.MVVM     📦 AvalonEdit 6.3.1       📦 NLog                │
│  📦 Microsoft.Extensions.DI   📦 Roslyn CSharp.Scripting                       │
│  🎮 Ultima Online Client (x86, hook tramite IClientInteropService)              │
└──────────────────────────────────────────────────────────────────────────────────┘

LEGENDA: ✅ Completato  ⚠️ Parziale/Bug noto  ❌ Mancante
```

---

## 3. Analisi Dettagliata per Componente

### 3.1 WorldService ✅
Implementazione eccellente. `ConcurrentDictionary` per mobiles/items, campo `volatile` per `_player`, metodi di mutazione semplici e thread-safe.

**Unica criticità**: `IsCasting { get; set; }` non è `volatile` — viene scritto dal packet handler thread e letto dagli script Python su thread separato.

### 3.2 WorldPacketHandler ✅
Il componente più ricco del progetto. Gestisce oltre 50 pacchetti S2C/C2S organizzati in regioni tematiche. Il pattern `if (data.Length < N) return;` come guard è applicato correttamente. L'uso di `UOBufferReader` è consistente.

**Note**: La gestione di `0x78` (MobileIncoming) usa `RegisterFilter` invece di `RegisterViewer`, il che è corretto perché può modificare il grafico. La gestione di `0xBF` Extended è ben strutturata con un sub-dispatch.

### 3.3 FilterHandler ✅
Implementazione completa dei filtri classici Razor. La doppia registrazione su `0x54` (FilterSound + FilterFootsteps) è intenzionale e corretta SE `IPacketService.RegisterFilter` supporta semantica "AND" (tutti i filtri devono passare). Questo deve essere verificato nell'implementazione di `PacketService`.

### 3.4 MacrosService ✅
Engine di controllo flow notevolmente ben fatto. Il pre-building delle jump tables in O(n) è un'ottima scelta architetturale. Il sistema di recording con viewer C2S è corretto.

**Criticità** (vedi §4): `IsPlaying`/`IsRecording` non thread-safe; `MacroList.Add()` chiamata da thread background.

### 3.5 TargetingService ✅
Separazione S2C (cursor request) / C2S (target response) implementata correttamente con `TargetCursorRequested` vs `TargetReceived`. Il pattern `ClearTargetCursor()` prima di `SendTarget()` è corretto.

**Criticità**: `SendPrompt()` invia serial/promptId con valore 0, che quasi certamente il server rifiuterà.

### 3.6 SkillsService ✅
Il fix del ciclo `while+break` per il terminator della full list (skillId=0) è corretto. La gestione 1-based/0-based è giusta.

**Criticità**: Doppia ricezione del pacchetto 0x3A (vedi §4 P0-02). `List<SkillInfo>` non thread-safe.

### 3.7 ScriptingService ✅
Architettura di cancellazione a due livelli (Thread.Interrupt + sys.settrace) ben progettata e documentata. Il TraceInterval=50 è un buon compromesso. Il SemaphoreSlim(1,1) per serializzare l'esecuzione è corretto.

**Nota**: `engine.Runtime.Shutdown()` nel finally evita memory leak — ottima pratica.

### 3.8 PacketBuilder ✅
Centralizzazione del building dei pacchetti C2S ben eseguita. Elimina la duplicazione in 6+ servizi. I calcoli delle lunghezze e i big-endian sono corretti.

**Nota**: `DropToContainer` usa 15 byte. Verificare la compatibilità con il protocollo SA (che aggiunge un byte `grid`). Se il shard target è pre-SA, 15 byte è corretto; post-SA serve 16.

### 3.9 BandageHealService ⚠️
Logica corretta ma con un bug P0 sul cursorId (vedi §4).

### 3.10 Scripting API ✅
Tutte le classi API usano `public virtual` correttamente per il DLR binder di IronPython. La gestione della cancellazione con `_cancel.ThrowIfCancelled()` è consistente.

**`ItemsApi.WaitForContents`**: usa `WeakReferenceMessenger.Default` invece del `_messenger` iniettato — inconsistenza che rompe i test che usano messenger custom.

### 3.11 UOSteamInterpreter ⚠️
Implementazione basilare funzionante per i comandi principali. Mancano molti comandi UOSteam avanzati (vedi §5).

### 3.12 App.xaml.cs ✅
DI configuration completa e ben strutturata. La distinzione Singleton/Transient è corretta (pagine stateless = Transient, pagine con stato = Singleton). La pulizia delle risorse in `OnExit` è presente.

---

## 4. Criticità Emerse

### P0 — Bug Critici (causano malfunzionamenti in produzione)

#### P0-01: BandageHealService — cursorId=0 nel target response
**File**: `TMRazorImproved.Core/Services/BandageHealService.cs:96`
**Codice problematico**:
```csharp
_packetService.SendToServer(PacketBuilder.TargetObject(targetSerial)); // cursorId=0 !!!
```
**Problema**: Il server invia il 0x6C S2C con un `cursorId` univoco che il client deve rispecchiare nella risposta. Con `cursorId=0`, il server Classic UO rifiuta il target silenziosamente e la fasciatura non viene applicata.
**Fix richiesto**: Prima chiamare `_targetingService.ClearTargetCursor()` e usare il PendingCursorId, esattamente come fa `MacrosService.cs:432-433`:
```csharp
uint cursorId = _targetingService.PendingCursorId;
_targetingService.ClearTargetCursor();
_packetService.SendToServer(PacketBuilder.TargetObject(targetSerial, cursorId));
```

#### P0-02: SkillsService — doppia registrazione per pacchetto 0x3A
**File**: `WorldPacketHandler.cs:134` + `SkillsService.cs:29`
**Problema**: `WorldPacketHandler` registra un viewer su `0x3A` via `_packetService.RegisterViewer(...)`, E `SkillsService` si iscrive ai `UOPacketMessage` tramite `IMessenger`. Se `WorldPacketHandler` pubblica anche un messaggio per 0x3A, il pacchetto viene processato DUE VOLTE → i delta delle skill vengono calcolati in modo errato.
**Fix richiesto**: Verificare che `WorldPacketHandler.HandleSkillsUpdate` non pubblichi un `UOPacketMessage` e rimuovere la doppia gestione. O rimuovere il viewer in `WorldPacketHandler` e lasciare solo `SkillsService`, o viceversa.

#### P0-03: WorldService.IsCasting — race condition senza volatile
**File**: `TMRazorImproved.Shared/Interfaces/IWorldService.cs` + `WorldService.cs:18`
**Codice**:
```csharp
public bool IsCasting { get; set; } // manca volatile!
```
**Problema**: `IsCasting` viene letto dagli script Python su un thread separato e può essere scritto dall'handler dei pacchetti. Senza `volatile`, il compilatore JIT può cachearlo in un registro → lo script non vedrà mai l'aggiornamento.
**Fix richiesto**: `public volatile bool IsCasting;` (o field backing + volatile).

**Problema aggiuntivo**: Non è chiaro dove `IsCasting` venga impostato a `true`. Nessun packet handler visibile imposta questo flag quando il player inizia a castare (0x12 C2S con type 0x56). `SpellsApi.WaitCast()` potrebbe restituire immediatamente sempre.

#### P0-04: MacrosService — ObservableCollection modificata da thread background
**File**: `TMRazorImproved.Core/Services/MacrosService.cs:706,720`
**Codice**:
```csharp
// In Stop() → viene chiamato da Task.Run o da MacrosService thread
var steps = captured.Select(cmd => new MacroStep(cmd, cmd)).ToList();
Save(ActiveMacro, steps); // → Save chiama MacroList.Add(name)

// In Save():
if (!MacroList.Contains(name))
    MacroList.Add(name); // CRASH: ObservableCollection non può essere modificata da thread non-UI
```
**Problema**: `MacroList` è `ObservableCollection<string>` e WPF lancia `InvalidOperationException` se viene modificata da un thread non-UI.
**Fix richiesto**: Dispatcher.Invoke o usare il meccanismo `RunOnUIThread` del ViewModelBase, oppure fare la `MacroList.Add` nel servizio tramite un event sul thread UI.

---

### P1 — Bug Importanti

#### P1-01: ItemsApi.WaitForContents — uso di WeakReferenceMessenger.Default
**File**: `TMRazorImproved.Core/Services/Scripting/Api/ItemsApi.cs:123`
**Problema**: Il metodo usa `WeakReferenceMessenger.Default` invece del `_messenger` iniettato nel costruttore. Rompe i test unitari che usano un messenger isolato e non funziona se viene registrato un messenger non-default.

#### P1-02: TargetingService.SendPrompt — packet 0x9A incompleto
**File**: `TMRazorImproved.Core/Services/TargetingService.cs:252-268`
**Problema**: Il pacchetto 0x9A risposta prompt richiede il `serial` e `promptId` ricevuti nel 0x9A S2C. Il codice invia questi campi a 0 perché non vengono tracciati quando arriva il prompt dal server.
**Fix richiesto**: Aggiungere handler 0x9A S2C in `WorldPacketHandler`, salvare `serial` e `promptId` in `ITargetingService`, e usarli in `SendPrompt`.

#### P1-03: MacrosService.IsPlaying / IsRecording — non thread-safe
**File**: `TMRazorImproved.Core/Services/MacrosService.cs:26-28`
**Problema**: Proprietà `bool` semplici modificate da thread diversi (UI thread chiama `Play()`, task background imposta `IsPlaying=false` nel finally). Possibile race condition visibile come UI che non aggiorna correttamente lo stato.
**Fix richiesto**: Aggiungere `volatile` o usare `Interlocked.Exchange`.

#### P1-04: DropToContainer — incompatibilità protocollo SA
**File**: `TMRazorImproved.Core/Utilities/PacketBuilder.cs:33`
**Problema**: La build del pacchetto 0x08 Drop è di 15 byte (senza campo `grid`). I server SA/post-SA aspettano 16 byte con un `grid` byte a offset [10]. Con server SA, il drop in container fallisce silenziosamente.
**Fix richiesto**: Aggiungere un parametro `byte grid = 0` e costruire il pacchetto da 16 byte per default, o rilevare la versione protocollo dal server.

---

### P2 — Issues di Qualità

#### P2-01: SkillsService.Skills — List<SkillInfo> non thread-safe
La lista skills viene scritta dall'handler 0x3A (packet thread) e letta dall'UI thread tramite binding. Nessun lock. In uno scenario di heavy skill update, si può incorrere in `InvalidOperationException`.

#### P2-02: EvaluateCondition in MacrosService — condizioni limitate
Supporta solo stats del player (HP, MANA, STAM, ecc.) e stati booleani. Mancano condizioni su item (`COUNTTYPE`, `BACKPACKWEIGHT`), su mobiles nel range, e su journal. Limita fortemente la compatibilità con macro scritte per TMRazor originale.

#### P2-03: UOSteamInterpreter — nessun sistema di variabili
Lo script UOSteam supporta `setoption` e variabili. L'interprete attuale non ha un dizionario variabili, quindi script UOSteam reali che usano variabili falliranno silenziosamente.

#### P2-04: FilterHandler — 0x54 doppia registrazione non documentata
Due lambda registrate su `0x54` per `FilterSound` e `FilterFootsteps`. La correttezza dipende dall'implementazione interna di `IPacketService.RegisterFilter`. Aggiungere un commento esplicito e un test che verifica il comportamento combinato.

#### P2-05: ScriptingService.ValidateScript — sandboxing insufficiente
Il metodo `ValidateScript` usa una semplice string-contains check per rilevare keyword pericolose. Basta rinominare un import o usare `__import__('os')` per eluderlo. Per un tool professionale, considerare un sandboxing reale (AppDomain separation o restrizioni Roslyn `SecurityPermission`).

---

## 5. Elementi Ancora da Migrare

### 5.1 Funzionalità Core (Alta Priorità)

| Feature | Stato | Note |
|---------|-------|------|
| **Encryption Patch** | ❌ Non migrato | `GlobalSettings.PatchEncryption` esiste ma `ClientInteropService` non implementa la patch |
| **Multi-client injection** | ❌ Non migrato | `AllowMultiClient` configurato ma non implementato |
| **Party System** | ❌ Non migrato | Nessun handler per pacchetti party (0x6F party invite, party member updates) |
| **Secure Trade completo** | ✅ Implementato | `SecureTradeService`, `TradeData`, UI e tracking S2C/C2S |
| **IsCasting tracking** | ✅ Implementato | Tracking in `WorldPacketHandler` su 0x12 e 0xBF sub 0x1C + `volatile` |
| **Auto Carver** | ✅ Implementato | `AutoCarverService` implementato come agent indipendente |
| **Bone Cutter** | ✅ Implementato | `BoneCutterService` implementato come agent indipendente |
| **Auto Remount** | ✅ Implementato | `AutoRemountService` implementato (supporto item ethereal e mobile mount) |

### 5.2 Filtri e Display (Media Priorità)

| Feature | Stato | Note |
|---------|-------|------|
| **Light Filter — max brightness packet** | ✅ Implementato | Invia pacchetto 0x4E/0x4F con max brightness (0x00) al client |
| **Overhead messages (danni, system)** | ❌ Non migrato | Configurazione in `UserProfile.ShowIncomingDamage` ma nessun renderer overlay |
| **Mob body filter** | ⚠️ Parziale | `MobFilterEnabled` nel profilo ma nessun handler che filtra 0x78/0x20 per body ID |
| **Staff filter** | ⚠️ Parziale | `FilterStaff` nel profilo ma nessun handler |
| **Block Trade Request** | ✅ Implementato | Gestito in `FilterHandler` filtrando 0x6F Start |
| **Block Party Invite** | ✅ Implementato | Filtro in `FilterHandler` con risposta automatica `DeclineParty` (0xBF sub 0x06 type 0x09) |
| **Highlight/Colorize Flags** | ❌ Non migrato | `HighlightFlags`, `ColorizeFlags` presenti ma non implementati |
| **Auto Screenshot on Death** | ❌ Non migrato | Flag in profilo, ScreenCaptureService è stub |

### 5.3 Agenti (Media Priorità)

| Feature | Stato | Note |
|---------|-------|------|
| **Dress — Undress support** | ⚠️ Parziale | `UndressBag` è configurato ma la logica di undress potrebbe essere incompleta |
| **Restock — da bank** | ⚠️ Da verificare | Logica di apertura bank/container non verificata |
| **AutoLoot — auto-open corpse** | ✅ Implementato | `NoOpenCorpse` configurabile |
| **Vendor Buy — confronto prezzi** | ⚠️ Da verificare | `CompareName` flag esiste ma logica di confronto prezzi non verificata |

### 5.4 UI e Tool (Bassa Priorità — Fase 3)

| Componente | Stato | Note |
|------------|-------|------|
| **FloatingToolbar overlay** | ⚠️ Stub | Window esiste, ViewModel esiste, ma posizionamento sopra client UO non implementato |
| **DisplayPage contenuto** | ⚠️ Placeholder | ViewModel e Page esistono, ma il contenuto effettivo (force width/height, scale items) non è collegato |
| **SoundPage volume/EQ** | ⚠️ Placeholder | UI esiste ma `SoundService` non ha controllo volume reale |
| **Gump List Manager** | ❌ Mancante | Nessuna UI per elencare i gump aperti |
| **Combat Strategy Editor** | ❌ Mancante | Auto-potions, priorità di cura avanzate |
| **Overhead Messages Config** | ❌ Mancante | Configurazione dei testi sopra i personaggi |
| **Skill Gain History** | ❌ Mancante | Grafico storico progressi skill |
| **Pathfinding Visualizer** | ❌ Mancante | Debug overlay per il percorso A* |
| **Static Map Inspector** | ❌ Mancante | Ispezione tile Z, flags, LOS |
| **Packet Search** | ❌ Mancante | Filtro/ricerca nel packet logger |

### 5.5 Scripting UOSteam (Media Priorità)

I seguenti comandi UOSteam NON sono implementati nell'interprete:

| Comando | Importanza |
|---------|-----------|
| `setoption`, variabili script | ✅ Implementato |
| `overhead` (messaggi sopra entità) | ✅ Implementato |
| `poplist`, `pushlist`, `removelist` | ✅ Implementato |
| `clearsysmsg`, `clearjournal` | ✅ Implementato |
| `setability` (primary/secondary ability) | ✅ Implementato |
| `interrupt` | ✅ Implementato |
| `timername`, `timer` | ✅ Implementato |
| `random` | Bassa |

---

## 6. Analisi Test Coverage

### 6.1 Copertura esistente ✅
| Area | Test File | Stato |
|------|-----------|-------|
| AutoLootService | AutoLootServiceTests | Buono |
| BandageHealService | BandageHealServiceTests | Buono (6 test) |
| MacrosService | MacrosServiceTests | Buono (IF/WHILE/FOR) |
| ScavengerService | ScavengerServiceTests | Buono |
| OrganizerService | OrganizerServiceTests | Presente |
| VendorBuy | VendorBuyIntegrationTests | Integration test |
| DressService | DressServiceTests | Presente |
| WorldPacketHandler | WorldPacketHandlerTests | Presente |
| UOSteamInterpreter | UOSteamInterpreterTests | Presente |
| PacketService | PacketServiceTests | Presente |
| UOBufferReader | UOBufferReaderTests | Presente |
| ConfigService | ConfigServiceTests | Presente |
| Concurrency | ConcurrencyStressTests | Ottimo |
| Packet Fuzz | PacketFuzzTests | Ottimo |

### 6.2 Aree senza test ❌
| Area | Priorità |
|------|---------|
| TargetingService | Alta |
| FilterHandler | Alta |
| SkillsService (0x3A parsing diretto) | Alta |
| ScriptingService (IronPython execution) | Media |
| PathFindingService (A* correttezza) | Media |
| MacrosService.MacroList thread-safety | Alta (bug P0-04) |
| DropToContainer packet format | Media |

---

## 7. Valutazione Complessiva

| Dimensione | Voto | Note |
|------------|------|------|
| Architettura e separazione layer | 9/10 | Eccellente uso di DI, MVVM, pattern observer |
| Qualità del codice (naming, stile) | 8/10 | Consistente, buona leggibilità |
| Thread safety | 6/10 | Diversi punti non protetti (vedi P0-03, P0-04, P1-03, P2-01) |
| Completezza funzionale | 5/10 | ~65% della feature parity con TMRazor originale |
| Test coverage | 7/10 | Buona base, mancano aree critiche |
| Scripting engine | 9/10 | Punto di forza del progetto |
| Packet handling | 8/10 | Molto completo, qualche gap minore |
| UI/UX | 6/10 | Base solida, molte pagine ancora stub |

---

## 8. Prossimi Sprint Consigliati (Ordine Priorità)

### Sprint Fix-10 — Bug Critici (1 settimana)
1. **Fix P0-01**: BandageHealService cursorId
2. **Fix P0-02**: Risolvere doppia gestione 0x3A tra WorldPacketHandler e SkillsService
3. **Fix P0-03**: Aggiungere `volatile` a `IsCasting` + implementare il tracking da 0x12 C2S
4. **Fix P0-04**: MacrosService — thread-safe per `MacroList` e `IsPlaying`/`IsRecording`
5. **Fix P1-02**: TargetingService — tracking 0x9A S2C per SendPrompt
6. **Fix P1-04**: PacketBuilder.DropToContainer — SA protocol compatibility
7. **Test**: Aggiungere test per TargetingService e FilterHandler

### Sprint 10 — Feature Parity Core (2 settimane)
1. Party system packet handlers
2. IsCasting tracking completo
3. Auto Carver / Bone Cutter / Auto Remount come mini-servizi
4. Light filter — invio pacchetto max brightness
5. Block Trade Request + Block Party Invite
6. SecureTradeService completo
7. UOSteamInterpreter: variabili + overhead + comandi mancanti

### Sprint 11 — UI Completamento (2 settimane)
1. DisplayPage — collegare ForceWidth/Height/ScaleItems
2. SoundPage — volume control reale
3. FloatingToolbar — posizionamento overlay sopra client
4. CountersPage — collegare CounterService al tracking grafico items
5. Overhead messages renderer

### Sprint 12 — Polish & Deploy (1 settimana)
1. Encryption patch + multi-client
2. Screen capture reale (PrintWindow Win32)
3. Gump List Manager UI
4. Skill Gain History grafico

---

*Documento generato da revisione architetturale completa del codebase — 2026-03-04*
