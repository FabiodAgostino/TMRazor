# Final Review v2 — Analisi Architetturale Completa TMRazor -> TMRazorImproved

**Data**: 19 Marzo 2026
**Versione**: 2.0 (consolidamento di 4 parti precedenti + ricerca aggiuntiva)
**Scopo**: Documento unico e definitivo che mappa l'intera migrazione da TMRazor (legacy) a TMRazorImproved (nuovo), identifica tutti i gap, e definisce un piano Sprint con task dettagliati eseguibili da qualsiasi sviluppatore.

---

## INDICE

1. [Executive Summary](#1-executive-summary)
2. [Panoramica Architetturale](#2-panoramica-architetturale)
3. [Mappatura Completa dei Componenti](#3-mappatura-completa-dei-componenti)
   - 3.1 Servizi Core
   - 3.2 Sistema Agenti
   - 3.3 Packet Handler e Rete
   - 3.4 Filtri
   - 3.5 Sistema Macro
   - 3.6 Motori di Scripting e API
   - 3.7 UOSteam Interpreter
   - 3.8 Targeting e Hotkey
   - 3.9 UI e Finestre
   - 3.10 Infrastruttura e Utilita
4. [Inventario Completo dei Gap](#4-inventario-completo-dei-gap)
5. [Piano Sprint](#5-piano-sprint)
6. [Appendice A — Statistiche Finali](#appendice-a--statistiche-finali)
7. [Appendice B — Glossario](#appendice-b--glossario)

---

## 1. Executive Summary

### Cosa e TMRazor?
TMRazor e un **assistente per Ultima Online** (UO), un MMORPG del 1997 ancora attivo. L'assistente si aggancia al client di gioco, intercetta i pacchetti di rete tra client e server, e fornisce automazioni: macro, script, agenti automatici (auto-loot, auto-heal, ecc.), hotkey per spell/abilita, filtri visivi/audio, e strumenti di debug.

### Cosa e la migrazione?
Si sta riscrivendo TMRazor da zero:
- **Da**: Windows Forms + .NET Framework 4.8 + architettura monolitica con classi statiche
- **A**: WPF + .NET 10 + architettura MVVM con Dependency Injection

### Stato attuale della migrazione

| Asse di copertura | Percentuale | Commento |
|---|---|---|
| Servizi Core (backend dati) | ~90% | World, Skills, Journal, Config sono solidi |
| Servizi di Automazione | ~55% | Targeting 37%, HotKey 11%, DragDrop 22% |
| API Scripting | ~75% | Base solida, ma 17+ metodi stub |
| UOSteam Interpreter | ~32% | 62 comandi su 208 totali legacy — gap piu grande |
| UI/UX | ~85% | WPF moderno, tutte le pagine principali presenti |
| Multi-Shard | ~30% | Solo TmClient supportato, no shard CRUD |
| **Copertura complessiva ponderata** | **~65%** | Ponderata per importanza gameplay |

### Task aperti totali: 38

| Priorita | Conteggio | Esempi chiave |
|---|---|---|
| Critica | 10 | Targeting, Hotkey, UOSteam, Shard management |
| Alta | 1 | DragDropCoordinator |
| Media | 16 | Menu system, Command service, API scripting |
| Bassa | 11 | AutoDoc, SoundApi placeholder, template parsing |

---

## 2. Panoramica Architetturale

### 2.1 Legacy (TMRazor/Razor)

```
Razor.sln
├── Razor/                          # Progetto principale
│   ├── Core/           (29 file)   # Engine, World, Player, Targeting, Timer
│   ├── Network/        (9 file)    # PacketHandler, Handlers, PacketTable, Packets
│   ├── Client/         (4 file)    # Client.cs (abstract), ClassicUO.cs, OSIClient.cs, UOAssist.cs
│   ├── Filters/        (13 file)   # Death, Light, Sound, Season, ecc.
│   ├── RazorEnhanced/  (273 file)  # Agenti, API scripting, Macro, UOSteam, Config, UI dialoghi
│   │   ├── Macros/Actions/  (47 file)  # 43 classi azione macro + controllo flusso
│   │   ├── UI/              (16+ file) # Dialoghi modali (Inspector, Editor, Launcher)
│   │   └── Proto-Control/   (gRPC)     # Controllo remoto
│   └── UI/             (53 file)   # MainForm WinForms (12.452 righe!), Grids, Controls
├── Crypt/                          # C++ DLL: encryption/injection (x86)
├── Loader/                         # C++ DLL: caricamento processo (x86)
└── UltimaSDK/                      # Lettura file .mul/.uop di Ultima Online
```

**Caratteristiche chiave**:
- **Framework**: Windows Forms, .NET Framework 4.8
- **Pattern**: Monolitico — classi statiche, thread manuali con `Thread.Abort()`
- **File C# totali**: ~253
- **Comunicazione interna**: Callback diretti, eventi UI, variabili statiche globali
- **Configurazione**: XML + binary `.razor` files + JSON per shards
- **Client supportati**: OSI (`client.exe`), ClassicUO (`ClassicUO.exe`), qualsiasi free shard
- **Iniezione**: DLL injection via `SetWindowsHookEx` — funziona **solo x86**
- **UOSteam**: 208 comandi+espressioni registrati (il linguaggio scripting piu usato dalla community)
- **Hotkey**: 31 categorie di azioni (spell, abilita, pozioni, pet, dress, agenti, ecc.)

### 2.2 Nuovo (TMRazorImproved)

```
TMRazorImproved.slnx
├── TMRazorImproved.Shared/         # Interfacce, Modelli, Enum, Risorse (lingue)
├── TMRazorImproved.Core/           # Logica business
│   ├── Services/       (43 file)   # Tutti i servizi (PacketService, WorldService, ecc.)
│   │   ├── Scripting/              # Motori scripting
│   │   │   ├── Api/    (18 file)   # API esposte agli script (MiscApi, ItemsApi, ecc.)
│   │   │   └── Engines/(2 file)    # UOSteamInterpreter, CSharpScriptEngine
│   │   └── Adapters/   (3 file)    # ClassicUOAdapter, OsiClientAdapter, Factory (STUB)
│   ├── Handlers/       (3 file)    # WorldPacketHandler (2645 righe), FilterHandler, FriendsHandler
│   └── Utilities/                  # Helper vari
├── TMRazorImproved.UI/             # WPF UI
│   ├── ViewModels/     (38 file)   # MVVM ViewModels
│   └── Views/          (39 XAML)   # Pagine, Finestre, Controlli
│       ├── Pages/      (21 file)   # Pagine principali
│       ├── Pages/Agents/(9 file)   # Pagine agenti
│       ├── Windows/    (9 file)    # Finestre floating
│       └── Controls/   (3 file)    # Controlli custom
├── TMRazorPlugin/                  # Plugin ClassicUO (caricato dentro TmClient.exe)
├── TMRazorImproved.Tests/          # Unit test
├── Crypt/                          # C++ DLL (copia)
├── Loader/                         # C++ DLL (copia)
└── UltimaSDK/                      # Riferimento condiviso
```

**Caratteristiche chiave**:
- **Framework**: WPF, .NET 10
- **Pattern**: MVVM + Dependency Injection + Messenger (`CommunityToolkit.Mvvm`)
- **File C# totali**: ~283 (inclusi test) — piu file grazie alla separazione DI/MVVM
- **Comunicazione interna**: `IMessenger` (mediator pattern, `WeakReferenceMessenger`)
- **Configurazione**: JSON (`GlobalSettings` + `UserProfile`)
- **Client supportati**: Solo TmClient.exe (fork ClassicUO per shard "The Miracle")
- **Iniezione**: Plugin ClassicUO (`TMRazorPlugin.dll`) + shared memory + named mutex — **x86/x64**
- **DI Container**: 50+ servizi registrati, quasi tutti Singleton
- **Servizi registrati all'avvio** in `App.xaml.cs` con `Host.CreateDefaultBuilder()`

### 2.3 Cambio Architetturale Fondamentale: Da DLL Injection a Plugin

**Legacy**: Il programma inietta `Crypt.dll` nel processo del client UO usando `SetWindowsHookEx` (Win32 API). Crypt.dll si aggancia alle funzioni di rete del client e ridireziona i pacchetti verso Razor tramite shared memory. Funziona **solo con processi x86**.

**Nuovo**: `TMRazorPlugin.dll` implementa l'interfaccia plugin di ClassicUO ed e caricato nativamente da TmClient.exe. Il plugin scrive i pacchetti in una shared memory con protocollo **length-prefix** (`[4 byte lunghezza LE][dati pacchetto]`). Il processo UI li legge tramite un named mutex.

```
                Legacy Flow                              Nuovo Flow
           ┌──────────────┐                        ┌──────────────┐
           │  client.exe  │                        │ TmClient.exe │
           │   (x86)      │                        │  (x86/x64)   │
           └──────┬───────┘                        └──────┬───────┘
                  │ SetWindowsHookEx                      │ Plugin API nativa
           ┌──────▼───────┐                        ┌──────▼───────┐
           │  Crypt.dll   │                        │TMRazorPlugin │
           │  (iniettata) │                        │  (caricato)  │
           └──────┬───────┘                        └──────┬───────┘
                  │ Shared Memory                         │ Shared Memory
           ┌──────▼───────┐                        ┌──────▼───────┐
           │   Razor.exe  │                        │   UI (WPF)   │
           │  (WinForms)  │                        │   (.NET 10)  │
           └──────────────┘                        └──────────────┘
```

### 2.4 Protocollo IPC: Shared Memory con Length-Prefix

Il plugin (`Engine.cs`) e il `PacketService` comunicano cosi:

1. Il plugin crea una shared memory chiamata `UONetSharedFM_{pid:x}` e un mutex `UONetSharedCOMM_{pid:x}`
2. Per ogni pacchetto intercettato, il plugin scrive: `[4 byte LE = lunghezza pacchetto][byte del pacchetto]`
3. `PacketService.HandleComm()` legge il prefisso di 4 byte, poi legge esattamente N byte di dati
4. I buffer hanno indici: 0 = InRecv (Server→Client), 2 = InSend (Client→Server)

**Attenzione**: La costante `LENGTH_PREFIX = 4` DEVE essere identica in `Engine.cs` (plugin) e `PacketService.cs` (UI). Se non corrispondono, l'IPC si rompe.

### 2.5 Flusso di Lancio Attuale (Solo TmClient)

```
1. Utente clicca "Launch Client"
2. GeneralViewModel.LaunchClient()
   ├── DeployPlugin() → copia TMRazorPlugin.dll nella cartella plugin di TmClient
   ├── Process.Start(TmClient.exe)
   ├── WaitForWindow() → attende che la finestra del client appaia
   ├── InstallLibrary() → inietta Crypt.dll (SetWindowsHookEx)
   │   └── Imposta _discoveredGamePid
   ├── NotifyCryptReady() → abilita il timer di PacketService
   └── InjectUoMod() → patches opzionali
3. Timer PacketService (100ms)
   ├── EnsureInitialized() → apre mutex + shared memory per PID
   └── HandleComm() → legge pacchetti → OnPacketReceived → WorldPacketHandler
4. WorldPacketHandler dispatcha via IMessenger → SkillsService, JournalService, ecc.
```

---

## 3. Mappatura Completa dei Componenti

### 3.1 Servizi Core

Questi servizi gestiscono lo **stato del mondo di gioco**: entita, giocatore, skill, journal, configurazione. Sono i piu completi della migrazione.

| Componente Legacy | File Legacy | Equivalente Nuovo | Righe | Stato | Note |
|---|---|---|---|---|---|
| World (entities) | `Core/World.cs` | `WorldService.cs` | 212 | ✅ Completo | Thread-safe con snapshot |
| Mobile | `Core/Mobile.cs` | `Shared/Models/UOEntity.cs` (Mobile) | — | ✅ Completo | |
| Item | `Core/Item.cs` | `Shared/Models/UOEntity.cs` (Item) | — | ✅ Completo | |
| Player | `Core/Player.cs` | `UOEntity.cs` + `WorldService.Player` | — | ✅ Completo | |
| Skills | `Core/Player.cs` (Skill) | `SkillsService.cs` + `SkillInfo.cs` | 247 | ✅ Completo | |
| Serial | `Core/Serial.cs` | `uint` diretto | — | ✅ Semplificato | Struct rimosso |
| Spells | `Core/Spells.cs` | `SpellDefinitions.cs` | — | ✅ Completo | |
| Buffs | `Core/Buffs.cs` | In `WorldPacketHandler` (0xDF) | — | ✅ Completo | |
| ObjectPropertyList | `Core/ObjectPropertyList.cs` | Inline su Mobile/Item | — | ✅ Refactored | |
| StealthSteps | `Core/StealthSteps.cs` | In `WorldService`/`PlayerApi` | — | ✅ Integrato | |
| TitleBar | `Core/TitleBar.cs` | `TitleBarService.cs` | 130 | ✅ Completo | |
| Timer | `Core/Timer.cs` | `Task.Delay()`/`PeriodicTimer` | — | ✅ Modernizzato | |
| PasswordMemory | `Core/PasswordMemory.cs` | `ConfigService` (credentials in profile) | — | ✅ Integrato | |
| PathFinding | `RazorEnhanced/PathFinding.cs` | `PathFindingService.cs` | 360 | ✅ Completo | A* |
| Config | `RazorEnhanced/Config.cs` | `ConfigService.cs` | 257 | ✅ Completo | JSON |
| Profiles | `RazorEnhanced/Profiles.cs` | `ConfigService.Switch/Clone/Rename` | — | ✅ Completo | |
| Settings | `RazorEnhanced/Settings.cs` | `ConfigModels.cs` (GlobalSettings) | — | ✅ Completo | |
| Journal | `RazorEnhanced/Journal.cs` | `JournalService.cs` | 71 | ✅ Completo | |
| DPSMeter | `RazorEnhanced/DPSMeter.cs` | `DPSMeterService.cs` | 132 | ✅ Completo | |
| Sound | `RazorEnhanced/Sound.cs` | `SoundService.cs` | 249 | ✅ Completo | |
| SpecialMoves | `RazorEnhanced/SpecialMoves.cs` | `WeaponService.cs` | 74 | ✅ Completo | |
| EncodedSpeech | `RazorEnhanced/EncodedSpeech.cs` | `EncodedSpeechHelper.cs` | — | ✅ Completo | |
| ItemID | `RazorEnhanced/ItemID.cs` | `ItemDataHelper.cs` | — | ✅ Completo | |
| Constants | `RazorEnhanced/Constants.cs` | Sparsi nei modelli | — | ✅ Semplificato | |
| MsgQueue | `Core/MsgQueue.cs` | `IMessenger` (mediator) | — | ✅ Modernizzato | |
| Geometry | `Core/Geometry.cs` | Inline nei servizi | — | ✅ Semplificato | |
| ScreenCapture | `Core/ScreenCapture.cs` | `ScreenCaptureService.cs` | 125 | ✅ Backend ok | UI mancante (~~TASK-017~~ (COMPLETATO)) |
| VideoCapture | `Core/VideoCapture.cs` | `VideoCaptureService.cs` | 188 | ✅ Backend ok | UI mancante (~~TASK-017~~ (COMPLETATO)) |

#### Servizi con Problemi Rilevati (rating corretto dalla Parte 4)

| Componente | Rating originale | Rating reale | Copertura | Dettagli |
|---|---|---|---|---|
| **Targeting** | ✅ Completo | **⚠️ Incompleto** | ~37% | Vedi sezione 3.8 |
| **HotKey** | ✅ Completo | **❌ Gravemente Incompleto** | ~11% | Vedi sezione 3.8 |
| **DragDropCoordinator** | ✅ Completo | **⚠️ Incompleto** | ~22% | Vedi sezione 3.2.8 |
| **SecureTrade** | ✅ Completo | ✅ **COMPLETATO** | 100% | Vedi TASK-042 |
| **Commands** | Non mappato | **❌ Assente** | 0% | Vedi TASK-001 |
| **Shards** | ✅ Integrato | **❌ Solo lettura** | ~20% | Vedi TASK-027 |

---

### 3.2 Sistema Agenti

**Cos'e un agente?** Un agente e un servizio che gira in background e compie azioni automatiche nel gioco. Ad esempio, l'AutoLoot raccoglie automaticamente oggetti dai cadaveri.

**Architettura legacy**: Ogni agente e una classe statica con un metodo `Engine()` eseguito su un `Thread` separato. L'arresto avviene con il pericoloso `Thread.Abort()`.

**Architettura nuova**: Ogni agente eredita da `AgentServiceBase` (166 righe) che fornisce il pattern `AgentLoopAsync(CancellationToken)`. L'arresto e cooperativo via `CancellationToken`. Riduzione codice media: **75-80%**.

#### 3.2.1 AutoLoot — ✅ Completo (con semplificazioni)

| Aspetto | Legacy (837 righe) | Nuovo (~199 righe) |
|---|---|---|
| Trigger | Scansione ciclica corpse in range | Event-driven via `ContainerContentMessage` |
| Coda | `ConcurrentQueue<SerialToGrab>` (serial+corpse) | `ConcurrentQueue<uint>` (solo serial) |
| Filtro proprieta | `Items.WaitForProps()` manuale | `MatchProperties()` generico |
| Thread safety | `Monitor.TryEnter()` | `ConcurrentDictionary` per tracking |
| Auto-ignore | HashSet (non thread-safe) | `ConcurrentDictionary<uint, byte>` (fix bug) |

**Come funziona nel nuovo**: Il servizio si registra come `IRecipient<ContainerContentMessage>`. Quando il server invia il contenuto di un container (corpse), il messaggio arriva al servizio che filtra e accoda gli item.

**Semplificazione**: Nessun `LootBagOverride` per item singolo — un unico container target dalla config.

#### 3.2.2 Scavenger — ✅ Completo

| Aspetto | Legacy (567 righe) | Nuovo (~145 righe) |
|---|---|---|
| Trigger | Scansione ciclica items a terra | Event-driven via `WorldItemMessage` |
| Thread | `Thread` separato | `AgentLoopAsync` async |

**Come funziona**: Quando un item appare nel mondo, il messaggio viene confrontato con la lista scavenger attiva. Se corrisponde, viene accodato per il pickup.

#### 3.2.3 Organizer — ⚠️ INCOMPLETO (TASK-004)

| Aspetto | Legacy (522 righe) | Nuovo (~106 righe) |
|---|---|---|
| Supporto Amount | ✅ per-item, -1 = tutto | ❌ **MANCA** |
| Stack parziali | ✅ split stack se necessario | ❌ **MANCA** |
| Esecuzione | Loop continuo | Single-pass |
| Container scan | ✅ Ricorsivo (nested bags) | ⚠️ Flat (solo primo livello) |

**Bug**: Un utente che configura "sposta 100 bandage" sposterebbe TUTTO lo stack. **Regressione funzionale**.

#### 3.2.4 Dress — ✅ Completo (semplificazioni accettabili)

| Aspetto | Legacy (807 righe) | Nuovo (~207 righe) |
|---|---|---|
| Two-handed weapons | ✅ ~60 righe logica | ✅ ~25 righe via `_weaponService.IsTwoHanded()` |
| UO3D macro | ✅ `EquipItemMacro` | ❌ Rimosso (non rilevante per TmClient) |
| Conflict flag | ✅ Opzionale (checkbox) | ❌ Sempre attivo (piu sicuro) |

#### 3.2.5 BandageHeal — ⚠️ INCOMPLETO (TASK-006)

| Aspetto | Legacy (635 righe) | Nuovo (~158 righe) |
|---|---|---|
| Ricerca bandage | ✅ Cerca per ItemID (0x0E21) nel backpack | ❌ **Assume serial da config** |
| Conteggio bandage | ✅ Warning se poche | ❌ MANCA |
| Buff tracking | ✅ Aspetta fine buff HealingSkill | ❌ **MANCA** — usa solo delay fisso |
| Target modes | ✅ Self/Last/Friend/FriendOrSelf | ✅ Identici |

**Bug**: Se il serial della bandage diventa invalido (stack finisce, serial cambia), l'agente smette di funzionare silenziosamente.

#### 3.2.6 Restock — ✅ Completo (semplificazione minore)

| Aspetto | Legacy (501 righe) | Nuovo (~128 righe) |
|---|---|---|
| Amount limit | ✅ Per-item | ✅ Per-item |
| Container scan | ✅ Ricorsivo | ⚠️ Flat (solo primo livello) |

#### 3.2.7 Friends — ✅ Completo | VendorBuy/Sell — ✅ Completo

Parita funzionale completa per entrambi.

#### 3.2.8 DragDropCoordinator — ⚠️ INCOMPLETO (TASK-041)

Questo servizio e il **coordinatore centrale** per tutte le operazioni di trascinamento oggetti (loot, scavenge, organize, restock). Nel legacy ha code dedicate e validazioni; nel nuovo e un semplice wrapper.

| Aspetto | Legacy (237 righe) | Nuovo (53 righe) |
|---|---|---|
| Code agente | ✅ 3 code: AutoLoot, Scavenger, AutoCarver | ❌ **Nessuna coda** |
| Weight check | ✅ Verifica peso prima di lootare | ❌ MANCA |
| Z-Level validation | ✅ |diff| > 8 = skip | ❌ MANCA |
| Delay management | ✅ Configurabili per azione | ⚠️ Hardcoded 50ms/150ms |
| Corpse refresh | ✅ Gestione corpse stale | ❌ MANCA |
| Concorrenza | ✅ Mutex per serializzare | ✅ SemaphoreSlim(1,1) |

**Il nuovo ha UN SOLO metodo pubblico**: `RequestDragDrop(serial, destination, amount, timeout)`.

**Impatto**: Piu agenti che operano contemporaneamente possono collidere.

#### 3.2.9 Nuovi Agenti (non presenti nel legacy)

| Agente | File | Righe | Descrizione |
|---|---|---|---|
| AutoRemount | `AutoRemountService.cs` | 71 | Rimonta automaticamente dopo dismount |
| AutoCarver | `AutoCarverService.cs` | 83 | Taglia automaticamente corpse vicini (range 3 tile) |
| BoneCutter | `BoneCutterService.cs` | 86 | Taglia ossa (0x0ECA-0x0ED2, range 1 tile) |

---

### 3.3 Packet Handler e Rete

#### Architettura

**Legacy** (`Razor/Network/`, 9 file):
- `PacketHandler.cs`: Routing con callback (viewer/filter)
- `Handlers.cs`: ~50+ handler statici (~3.851 righe)
- `PacketTable.cs`: Tabella dimensioni statiche per ogni tipo di pacchetto
- `Packet.cs` + `Packets.cs`: 90+ definizioni classi pacchetto

**Nuovo** (`TMRazorImproved.Core/`):
- `PacketService.cs` (362 righe): Legge da shared memory, dispatcha
- `WorldPacketHandler.cs` (2.645 righe): Handler centralizzato per 70+ pacchetti
- `PacketBuilder.cs`: Costruzione pacchetti in uscita
- `UOBufferReader.cs`: Parser binario con position tracking

**Differenza chiave**: Il legacy usa `PacketTable.cs` per sapere la lunghezza di ogni pacchetto (lookup per ID). Il nuovo NON ne ha bisogno grazie al protocollo **length-prefix**: ogni pacchetto arriva gia con la sua lunghezza nei primi 4 byte.

#### Copertura Pacchetti Server → Client

**Legacy: 56 pacchetti | Nuovo: 60+ pacchetti (4 nuovi)**

| Packet ID | Nome | Legacy | Nuovo | Note |
|---|---|---|---|---|
| 0x0B | Damage | ✅ | ✅ | |
| 0x11 | MobileStatus | ✅ | ✅ | |
| 0x16 | SA MobileStatus | ✅ | ✅ | |
| 0x17 | NewMobileStatus | ✅ | ✅ | |
| 0x1A | WorldItem | ✅ | ✅ | |
| 0x1B | LoginConfirm | ✅ | ✅ | |
| 0x1C | AsciiSpeech | ✅ | ✅ | Con filtri |
| 0x1D | RemoveObject | ✅ | ✅ | |
| 0x20 | MobileUpdate | ✅ | ✅ | |
| 0x21 | MovementReject | ✅ | ✅ | |
| 0x22 | MovementAck | ✅ | ✅ | |
| 0x24 | BeginContainerContent | ✅ | ✅ | |
| 0x25 | ContainerContentUpdate | ✅ | ✅ | |
| 0x27 | LiftReject | ✅ | ✅ | |
| 0x2C | PlayerDeath | ✅ | ✅ | |
| 0x2D | MobileStatInfo | ✅ | ✅ | |
| 0x2E | EquipmentUpdate | ✅ | ✅ | |
| 0x3A | Skills | ✅ | ✅ | |
| 0x3C | ContainerContent | ✅ | ✅ | |
| 0x4E | PersonalLight | ✅ | ✅ | |
| 0x4F | GlobalLight | ✅ | ✅ | |
| 0x54 | PlaySound | ✅ | ✅ | |
| 0x55 | LoginComplete | ❌ | ✅ | **Nuovo** |
| 0x56 | PinLocation | ✅ | ✅ | |
| 0x65 | Weather | ✅ | ✅ | |
| 0x6C | TargetCursor | ❌ | ✅ | **Nuovo** |
| 0x6D | PlayMusic | ❌ | ✅ | **Nuovo** |
| 0x6E | CharAnimation | ❌ | ✅ | **Nuovo** |
| 0x6F | TradeRequest | ✅ | ✅ | |
| 0x72 | WarMode | ✅ | ✅ | |
| 0x73 | Ping | ✅ | ✅ | |
| 0x74 | VendorBuyList | ✅ | ✅ | |
| 0x76 | ServerChange | ✅ | ✅ | |
| 0x77 | MobileMoving | ✅ | ✅ | |
| 0x78 | MobileIncoming | ✅ | ✅ | |
| 0x7C | SendMenu | ✅ | ✅ | |
| 0x83 | DeleteCharacter | ❌ | ✅ | **Nuovo** |
| 0x88 | OpenPaperdoll | ✅ | ✅ | |
| 0x89 | CorpseEquipment | ❌ | ✅ | **Nuovo** |
| 0x8C | RelayServer | ❌ | ✅ | **Nuovo** |
| 0x90 | MapDetails | ✅ | ✅ | |
| 0x95 | HueResponse | ❌ | ✅ | **Nuovo** |
| 0x97 | MovementDemand | ✅ | ✅ | |
| 0x98 | MobileName | ❌ | ✅ | **Nuovo** |
| 0x9A | AsciiPrompt | ✅ | ✅ | |
| 0x9E | VendorSellList | ✅ | ✅ | |
| 0xA1-A3 | Hits/Mana/Stam Update | ✅ | ✅ | |
| 0xA8 | ServerList | ✅ | ✅ | |
| 0xAA | AttackOK | ❌ | ✅ | **Nuovo** |
| 0xAB | DisplayStringQuery | ✅ | ✅ | |
| 0xAE | UnicodeSpeech | ✅ | ✅ | Con filtri |
| 0xAF | DeathAnimation | ✅ | ✅ | |
| 0xB0 | Gump | ✅ | ✅ | |
| 0xB8-BF | Extended packets | ✅ | ✅ | |
| 0xC0 | GraphicalEffect | ❌ | ✅ | **Nuovo** |
| 0xC1 | LocalizedMessage | ✅ | ✅ | Con filtri |
| 0xC2 | UnicodePrompt | ✅ | ✅ | |
| 0xC8 | SetUpdateRange | ✅ | ✅ | |
| 0xCC | LocalizedMsgAffix | ✅ | ✅ | |
| 0xD6 | OPL (Properties) | ✅ | ✅ | |
| 0xD8 | CustomHouseInfo | ✅ | ✅ | |
| 0xDD | CompressedGump | ✅ | ✅ | |
| 0xDF | BuffDebuff | ✅ | ✅ | |
| 0xF0-F6 | Vari | ✅ | ✅ | |

#### Copertura Pacchetti Client → Server

| Packet ID | Nome | Legacy | Nuovo | Note |
|---|---|---|---|---|
| 0x02 | MovementRequest | ✅ | ✅ | |
| 0x05 | AttackRequest | ✅ | ✅ | |
| 0x06 | DoubleClick | ✅ | ✅ | |
| 0x07 | LiftRequest | ✅ | ✅ | |
| 0x08 | DropRequest | ✅ | ✅ | |
| 0x09 | SingleClick | ✅ | ✅ | |
| 0x12 | TextCommand | ✅ | ✅ | |
| 0x13 | EquipRequest | ✅ | ✅ | |
| 0x6F | TradeRequest | ✅ | ✅ | |
| 0x75 | RenameMobile | ✅ | ✅ | |
| 0xB1 | GumpResponse | ✅ | ✅ | |
| 0xBF | ExtendedCommand | ✅ | ✅ | |
| 0xD7 | EncodedPacket | ✅ | ✅ | |
| 0x7D | MenuResponse | ✅ | ✅ | ~~TASK-010~~ |
| 0xC2 | UnicodePromptSend | ✅ | ✅ | ~~TASK-011~~ |
| 0x00,5D,80,91,A0,F8 | Login/Character | ✅ | ❌ | Non necessari (TmClient gestisce) |

---

### 3.4 Filtri

**Legacy**: 13 file separati in `Razor/Filters/`, ogni filtro eredita da `Filter`.

**Nuovo**: Consolidato in `FilterHandler.cs` (351 righe) + `TargetFilterService.cs` (113 righe).

| Filtro | Legacy | Nuovo | Stato |
|---|---|---|---|
| Death (0x2C) | `Death.cs` | `FilterHandler` | ✅ |
| Light (0x4E, 0x4F) | `Light.cs` | `FilterHandler` | ✅ |
| MessageFilter | `MessageFilter.cs` | `FilterHandler` (Poison, Karma, Snoop) | ✅ |
| MobileFilter (dragon/drake) | `MobileFilter.cs` | `FilterHandler.MorphGraphic()` | ⚠️ Graphic hardcoded 0x0033 |
| Season (0xBC) | `Season.cs` | `FilterHandler` | ✅ |
| SoundFilters (0x54) | `SoundFilters.cs` | `FilterHandler` | ✅ |
| StaffItems/Npcs | 2 file | `FilterHandler` | ✅ |
| TargetFilterManager | `TargetFilterManager.cs` | `TargetFilterService.cs` | ✅ |
| VetRewardGump | `VetRewardGump.cs` | `FilterHandler` (0xB0, 0xDD) | ✅ |
| WallStaticFilter | `WallStaticFilter.cs` | `FilterHandler.MorphGraphic()` | ✅ |
| Weather (0x65) | `Weather.cs` | `FilterHandler` | ✅ |

**Filtri NUOVI in TMRazorImproved** (non presenti nel legacy):
- Bard Music Filter (0x6D)
- Party Invite Block (0xBF sub 0x06)
- Trade Request Block (0x6F)
- Footsteps Filter (0x54, sound IDs 0x12-0x1A)
- Custom Graph Filters (utente definisce graphic ID → replacement)

**Copertura filtri: 100%** — Tutti i filtri legacy sono presenti e potenziati con granularità per tipo di messaggio e grafiche configurabili. (TASK-043 COMPLETATO)

---

### 3.5 Sistema Macro

**Cos'e una macro?** Una sequenza di azioni registrata dall'utente (es: "usa skill Hiding, aspetta 5 secondi, muoviti a nord"). Il sistema macro permette anche controllo di flusso (IF/WHILE/FOR).

**Legacy** (`Razor/RazorEnhanced/Macros/`, 47 file): 43 classi dedicate, ognuna eredita da `MacroAction`. Serializzazione binaria/XML.

**Nuovo** (`MacrosService.cs`, 1018 righe): Comandi testuali unificati (switch/case in `ExecuteActionAsync`). Two-pass: `BuildJumpTables()` → `ExecuteWithControlFlowAsync()`. Persistenza come `MacroStep` (Command string + IsEnabled bool).

#### Mappatura Azioni Macro (38 implementate su 43)

| Azione Legacy | Comando Nuovo | Stato |
|---|---|---|
| AttackAction | `ATTACK` | ⚠️ Solo serial (TASK-012) |
| ArmDisarm | `ARMDISARM` | ✅ |
| Bandage | `BANDAGE` | ✅ |
| CastSpell | `CAST` | ✅ |
| ClearJournal | `CLEARJOURNAL` | ✅ |
| Comment | `// commento` | ✅ |
| DoubleClick | `DOUBLECLICK`/`DCLICK` | ✅ |
| Drop | `DROP` | ✅ |
| Fly | `FLY`/`LAND` | ✅ |
| GumpResponse | `RESPONDGUMP` | ✅ |
| IF/ELSEIF/ELSE/ENDIF | Tutti | ✅ |
| WHILE/ENDWHILE | Tutti | ✅ |
| FOR/ENDFOR | Tutti | ✅ |
| InvokeVirtue | `INVOKEVIRTUE` | ✅ |
| Messaging | `SAY`/`MSG` | ✅ |
| Mount | `MOUNT`/`DISMOUNT` | ✅ |
| MoveItem | `MOVEITEM` | ✅ |
| Pause | `PAUSE`/`WAIT` | ✅ |
| PickUp | `PICKUP` | ✅ |
| PromptResponse | `PROMPTRESPONSE` | ✅ |
| RemoveAlias | `REMOVEALIAS` | ✅ |
| RenameMobile | `RENAMEMOBILE` | ✅ |
| Resync | `RESYNC` | ✅ |
| RunOrganizer | `RUNORGANIZER` | ✅ |
| SetAlias | `SETALIAS` | ✅ |
| TargetAction | `TARGET` | ✅ |
| TargetResource | `TARGETRESOURCE` | ✅ |
| ToggleWarMode | `WARMODE` | ✅ |
| UseContextMenu | `USECONTEXTMENU` | ✅ |
| UseEmote | `EMOTE` | ✅ |
| UsePotion | `USEPOTIONTYPE` | ✅ |
| UseSkill | `USESKILL` | ✅ |
| WaitForGump | `WAITFORGUMP` | ✅ |
| WaitForTarget | `WAITFORTARGET` | ✅ |
| **SetAbility** | — | ❌ TASK-013 |
| **Disconnect** | — | ❌ Rimosso intenzionalmente |
| **MovementAction** | — | ❌ Rimosso |
| **QueryStringResponse** | — | ❌ Rimosso (raro) |

#### Condizioni IF/WHILE — Gap (TASK-022)

Il sistema condizioni (`ConditionEvaluator.cs`, 252 righe) supporta 9 categorie:

**Implementate** (✅):
- POISONED, HIDDEN, WARMODE, DEAD, PARALYZED, FLYING, YELLOWHITS
- HP, MAXHP, MANA, MAXMANA, STAM, STR, DEX, INT, WEIGHT, FOLLOWERS, GOLD, LUCK, AR, resistenze
- SKILL (per nome), FIND (per graphic), COUNT (per graphic), INRANGE (per serial)
- TARGETEXISTS, INJOURNAL, BUFFEXISTS
- Supporto prefisso `NOT` per negazione
- Operatori: `<`, `>`, `<=`, `>=`, `==`, `!=`

**Mancanti** (❌):
| Condizione | Descrizione | Impatto |
|---|---|---|
| `MOUNTED` | Verifica se player e montato | **Alto** — macro combattimento |
| `ISALIVE` (Ghost check) | `!IsGhost` (diverso da DEAD che e HP-based) | **Alto** |
| `RIGHTHANDEQUIPPED` | Item in mano destra | **Alto** — macro weapon swap |
| `LEFTHANDEQUIPPED` | Item in mano sinistra | **Alto** — macro weapon swap |
| `FIND in Container` | Cerca in container specifico, non solo backpack | **Alto** — craft macro |
| `FindStoreSerial` | Salva serial trovato per uso successivo | **Alto** — macro multi-step |
| `InRange ItemType` | Cerca item per graphic entro range | Media |
| `InRange MobileType` | Cerca mobile per graphic entro range | Media |

---

### 3.6 Motori di Scripting e API

#### Motori

| Engine Legacy | Equivalente Nuovo | Righe | Stato |
|---|---|---|---|
| PythonEngine (IronPython) | `ScriptingService.cs` (IronPython 3.4) | 595 | ✅ Migliorato |
| CSharpEngine (CodeDom) | `CSharpScriptEngine.cs` (Roslyn) | 115 | ✅ Modernizzato |
| UOSteamEngine (8.331 righe) | `UOSteamInterpreter.cs` | 716 | ⚠️ 32% (vedi 3.7) |

**Miglioramenti del nuovo**:
1. **Cancellation a 2 livelli**: Level 1 `Thread.Interrupt()` per blocking, Level 2 `sys.settrace()` ogni 50 statement
2. **Override `time.sleep`**: Rediretto a `Misc.Pause()` per intercettare blocking
3. **Roslyn**: Compilazione C# moderna vs CodeDom
4. **DI-based**: API iniettate via `ScriptGlobals` (45 righe)

#### API Scripting (18 file)

| API | File | Righe | Stato |
|---|---|---|---|
| MiscApi | `MiscApi.cs` | 1146 | ⚠️ 6 metodi menu stub, 3 query string stub |
| PlayerApi | `PlayerApi.cs` | 866 | ✅ |
| GumpsApi | `GumpsApi.cs` | 768 | ✅ |
| MobilesApi | `MobilesApi.cs` | 726 | ✅ |
| TargetApi | `TargetApi.cs` | 580 | ✅ |
| SpellsApi | `SpellsApi.cs` | 376 | ✅ |
| ItemsApi | `ItemsApi.cs` | 361 | ✅ |
| JournalApi | `JournalApi.cs` | 350 | ✅ |
| StaticsApi | `StaticsApi.cs` | 329 | ⚠️ `CheckDeedHouse()` → false |
| Wrappers | `Wrappers.cs` | 258 | ✅ |
| TimerApi | `TimerApi.cs` | 140 | ✅ |
| FriendApi | `FriendApi.cs` | 138 | ⚠️ `ChangeList()` stub |
| SkillsApi | `SkillsApi.cs` | 135 | ✅ |
| SoundApi | `SoundApi.cs` | 131 | ⚠️ `GetMin/MaxDuration()` → 0 |
| FiltersApi | `FiltersApi.cs` | 111 | ✅ |
| HotkeyApi | `HotkeyApi.cs` | 100 | ✅ |
| AgentApis | `AgentApis.cs` | 74 | ✅ |
| SpecialMovesApi | `SpecialMovesApi.cs` | 65 | ✅ |

**API mancanti** (servizio esiste ma non esposto via `ScriptGlobals`):
| API | Servizio | TASK |
|---|---|---|
| PathFindingApi | `PathFindingService.cs` (360 righe) | TASK-014 |
| DPSMeterApi | `DPSMeterService.cs` (132 righe) | TASK-015 |
| PacketLoggerApi | `PacketLoggerService.cs` (234 righe) | TASK-016 |
| CounterApi | `CounterService.cs` (104 righe) | TASK-023 |

#### Inventario Completo Metodi Stub nelle API

| File | Metodo | Ritorna | Impatto |
|---|---|---|---|
| `MiscApi.cs` | `HasMenu()` | `false` | **Alto** — script crafting |
| `MiscApi.cs` | `CloseMenu()` | No-op | **Alto** |
| `MiscApi.cs` | `MenuContain(text)` | `false` | **Alto** |
| `MiscApi.cs` | `GetMenuTitle()` | `""` | **Alto** |
| `MiscApi.cs` | `WaitForMenu(delay)` | `false` | **Alto** — script crafting |
| `MiscApi.cs` | `MenuResponse(text)` | No-op | **Critico** — script crafting |
| `MiscApi.cs` | `HasQueryString()` | `false` | Medio |
| `MiscApi.cs` | `WaitForQueryString(delay)` | `false` | Medio |
| `MiscApi.cs` | `QueryStringResponse(ok, text)` | No-op | Medio |
| `MiscApi.cs` | `GetMapInfo(serial)` | Solo serial, no dati | Medio |
| `MiscApi.cs` | `ExportPythonAPI()` | "not implemented" | Basso |
| `MiscApi.cs` | `GetContPosition()` | `(0,0)` | Basso |
| `MiscApi.cs` | `LastHotKey()` | `null` | Basso |
| `SoundApi.cs` | `GetMinDuration()` | `0` | Basso |
| `SoundApi.cs` | `GetMaxDuration()` | `0` | Basso |
| `FriendApi.cs` | `ChangeList(name)` | No-op | Medio |
| `StaticsApi.cs` | `CheckDeedHouse(x,y)` | `false` | Medio |

---

### 3.7 UOSteam Interpreter — Il Gap Piu Grande

**Cos'e UOSteam?** E il linguaggio di scripting piu popolare tra i giocatori di Ultima Online. E piu semplice di Python/C# e permette di automatizzare azioni di gioco con una sintassi tipo batch script.

**Il problema**: Il legacy implementa **208 comandi+espressioni** (8.331 righe). Il nuovo implementa **62 comandi** (716 righe) = **~30% del totale**.

#### Comandi Implementati nel Nuovo (62 totali)

**Controllo flusso** (8): `if`, `elseif`, `else`, `endif`, `while`, `endwhile`, `for`, `endfor`

**Variabili/Liste** (8): `setvar`, `clearvar`, `setalias`, `unsetalias`, `pushlist`, `poplist`, `removelist`, `clearlist`

**Timer** (3): `createtimer`, `settimer`, `removetimer`

**Journal/Output** (5): `clearjournal`/`clearsysmsg`, `waitforjournal`, `overhead`, `sysmsg`, `msg`/`say`

**Azioni personaggio** (8): `headmsg`, `setability`, `interrupt`, `resync`, `setoption`, `cast`, `attack`, `warmode`, `useskill`/`skill`

**Pausa** (1): `pause`/`wait`

**Item** (4): `useobject`/`dclick`/`doubleclick`, `click`/`singleclick`, `move`/`moveitem`, `lift`, `drop`

**Target** (3): `targetself`, `waitfortarget`, `target`

**Gump** (3): `waitforgump`, `replygump`/`gumpresponse`, `closegump`

**Container** (1): `waitforcontents`/`waitforcontainer`

**Movimento** (2): `walk`, `turn`

**Equipment/Agenti** (6): `dress`, `undress`, `organizer`, `restock`, `autoloot`, `scavenger`

**Misc** (5): `random`, `getlabel`, `playsound`, `playmusic`, `stopmusic`/`stopsound`

**Bandage/Controllo** (3): `bandageheal`, `replay`, `stop`

#### Comandi Mancanti — Priorita CRITICA (usati frequentemente)

| Comando | Categoria | Perche serve |
|---|---|---|
| `fly` / `land` | Movimento | Script di viaggio per gargoyle |
| `run` | Movimento | Corsa direzionale |
| `pathfindto` | Movimento | Navigazione automatica a coordinate |
| `miniheal` / `bigheal` | Healing | Cura tramite spell — **script combattimento** |
| `chivalryheal` | Healing | Cura Close Wounds per paladin |
| `bandageself` | Healing | Auto-bendaggio rapido — **script combattimento** |
| `targettype` | Targeting | Target per tipo grafico — **script combattimento/gathering** |
| `targetground` | Targeting | Target a terra per harvesting |
| `targettile*` | Targeting | Target per tile/offset per mining/lumberjacking |
| `targetresource` | Targeting | Target risorsa specifica |
| `cleartargetqueue` | Targeting | Pulisci coda target |
| `autotargetobject` | Targeting | Setup auto-targeting |
| `buy` / `sell` | Vendor | Interazione automatica con NPC vendor |
| `contextmenu` / `waitforcontext` | Interazione | Menu contestuali NPC — **script farming/craft** |
| `waitforproperties` | Item | Attesa caricamento proprieta OPL |
| `moveitemoffset` / `movetypeoffset` | Item | Spostamento avanzato item |
| `getfriend` / `getenemy` | Combat | Selezione friend/enemy — **PvP** |
| `ignoreobject` / `clearignorelist` | Filtro | Ignora oggetti/NPC durante farming |

#### Comandi Mancanti — Priorita ALTA

| Comando | Categoria | Perche serve |
|---|---|---|
| `partymsg` / `guildmsg` / `allymsg` | Chat | Messaggi su canali specifici |
| `whispermsg` / `yellmsg` / `emotemsg` | Chat | Messaggi con tipo specifico |
| `equipitem` / `equipwand` | Equip | Equipaggiamento diretto da script |
| `togglemounted` / `togglehands` / `clearhands` | Equip | Toggle mount/armi |
| `clickobject` | Interazione | Click singolo (diverso da double-click) |
| `useonce` / `clearusequeue` | Item | Coda uso item (uno alla volta) |
| `addfriend` / `removefriend` | Friends | Gestione lista amici da script |
| `toggleautoloot` / `togglescavenger` | Agent | Toggle agenti on/off da script |
| `dressconfig` | Dress | Configurazione dress list |
| `promptalias` | Alias | Input interattivo per alias |
| `playmacro` | Script | Esecuzione macro da script |
| `virtue` | Ability | Invocazione virtu |

#### Espressioni/Condizioni Mancanti (35+)

| Espressione | Categoria | Priorita |
|---|---|---|
| `true` / `false` | Booleano | **Critica** |
| `physical`/`fire`/`cold`/`poison`/`energy` | Resistenze | **Critica** |
| `skill`/`skillbase`/`skillvalue`/`skillstate` | Skill | **Critica** |
| `findtype`/`findobject`/`findlayer` | Ricerca | **Alta** |
| `amount`/`graphic`/`color`/`durability` | Item props | **Alta** |
| `property` | OPL generica | **Alta** |
| `criminal`/`enemy`/`friend`/`gray`/`innocent`/`murderer` | Notoriety | **Alta** |
| `flying`/`waitingfortarget` | Stato | Media |
| `direction`/`directionname` | Navigazione | Media |
| `diffmana`/`diffstam` | Stats | Media |
| `infriendlist`/`ingump`/`inregion` | Query | Media |
| `counttypeground` | Counter | Media |

---

### 3.8 Targeting e Hotkey — Le Regressioni Piu Gravi

#### 3.8.1 TargetingService — Copertura ~37% (TASK-039)

**Cos'e il targeting in UO?** Quando lanci una spell o usi un'abilita, il gioco ti chiede di selezionare un bersaglio ("target cursor"). Il sistema di targeting automatizza questa selezione.

**Il legacy** (`Targeting.cs`, 980 righe) ha un sistema sofisticato:

| Feature | Legacy | Nuovo (363 righe) |
|---|---|---|
| Tipi di target | 4: LastTarget, LastHarmTarg, LastBeneTarg, LastGroundTarg | **1 solo**: `_lastTarget` |
| Smart Last Target | ✅ Sceglie automaticamente harm/bene in base alla spell | ❌ **MANCA** |
| Target queue | ✅ Accoda azioni se nessun cursor attivo | ❌ Azioni falliscono silenziosamente |
| DoAutoTarget() | ✅ Selezione automatica con validazione | ❌ **Metodo assente** |
| OneTimeTarget() | ✅ Callback per scripting con intercept | ❌ **MANCA** |
| CheckHealPoisonTarget() | ✅ Blocca heal su avvelenato | ❌ **MANCA** |
| TextFlags | ✅ Messaggi overhead per tipo target | ❌ **MANCA** |
| Spell Target ID | ✅ Distingue target spell da normali | ❌ **MANCA** |
| Range check configurabile | ✅ Per-profilo `RangeCheckLT` + `LTRange` | ❌ Solo `config.Range` fisso |

**Metodi pubblici attuali** (13 totali):
`ClearTargetCursor`, `RequestTarget`, `RequestLocationTarget`, `TargetNext`, `TargetClosest`, `TargetSelf`, `Clear`, `SendTarget` (2 overload), `CancelTarget`, `AcquireTargetAsync`, `SetLastTarget`, `SetPrompt`, `SendPrompt`

**Impatto PvP**: Il "Smart Last Target" e **essenziale** in PvP. Senza di esso, un giocatore che alterna spell offensive e curative colpira/curera il bersaglio sbagliato.

#### 3.8.2 HotkeyService — Copertura ~11% (TASK-040)

**Cos'e l'HotkeyService?** Permette di associare tasti della tastiera (e mouse) ad azioni di gioco. In UO, un giocatore PvP configura **30-50 hotkey** per spell, pozioni, abilita, target, bendaggi.

**Il legacy** (`HotKey.cs`, 2.139 righe) ha **31 categorie di azioni**:
- 7 categorie Spell (Magery, Chivalry, Necro, Bushido, Ninjitsu, Spellweaving, Mysticism)
- ProcessAbilities (abilita primaria/secondaria arma)
- ProcessAttack (nearest/last/etc.)
- ProcessBandage (bendaggio rapido)
- ProcessPotions (uso pozioni)
- ProcessHands (toggle armi)
- ProcessDress (liste dress)
- ProcessSkills (uso skill)
- 7 categorie Agent toggle (AutoLoot, Scavenger, Organizer, Dress, Restock, Bandage, Buy/Sell)
- ProcessScript (esecuzione script)
- ProcessGeneral (resync, screenshot, ecc.)
- ProcessPet, ProcessEquipWands, ProcessOther, ProcessShowName

**Il nuovo** (`HotkeyService.cs`, 240 righe) e un **dispatcher generico** con solo 4 metodi pubblici:
- `Start()` — avvia il keyboard hook
- `StopAsync()` — ferma il hook
- `RegisterAction(name, action)` — registra un'azione
- `Dispose()`

Le uniche azioni effettivamente registrate provengono dal `TargetingService`:
- "Target Next", "Target Closest", "Target Self", "Last Target", "Clear Target"

E dall'`UOSteamInterpreter`/`ScriptingService` se l'azione inizia con `"Script:"`.

**Tutto il resto (spell, abilita, pozioni, agenti, dress, pet) NON e registrato.**

**Impatto**: L'HotkeyService e **praticamente inutilizzabile** per gameplay reale. Un giocatore non puo lanciare spell, usare pozioni, o toggleare agenti da tastiera.

---

### 3.9 UI e Finestre

**Legacy**: 16+ dialoghi modali separati (Windows Forms)
**Nuovo**: Pagine tabbed WPF con MVVM + finestre floating

#### Mappatura UI

| UI Legacy | Equivalente Nuovo | Stato |
|---|---|---|
| GumpInspector | `InspectorPage.xaml` (tab Gumps) | ✅ Ma manca response logging (TASK-024) |
| ItemInspector | `InspectorPage.xaml` (tab Entity) | ✅ Consolidato |
| MobileInspector | `InspectorPage.xaml` (tab Entity) | ✅ Consolidato |
| StaticInspector | `InspectorPage.xaml` (tab Map) | ✅ Consolidato |
| ScriptEditor (FastColoredTextBox) | `ScriptingPage.xaml` (AvalonEdit) | ✅ Modernizzato |
| ChangeLog | `ChangelogWindow.xaml` | ✅ |
| Launcher | `GeneralPage.xaml` (integrato) | ✅ Ma solo TmClient |
| Profile dialoghi (3) | `GeneralViewModel` methods | ✅ Integrato |
| AutolootEditProps | `EditLootItemWindow.xaml` | ✅ Migliorato |
| SpellGrid | `SpellGridWindow.xaml` | ✅ |
| Toolbar | `FloatingToolbarWindow.xaml` | ✅ |
| DPSMeter | `DPSMeterWindow.xaml` | ✅ |
| Screenshot/Video UI | — | ❌ ~~TASK-017~~ (COMPLETATO) (backend ok) |

#### Pagine NUOVE in TMRazorImproved

| Pagina | Descrizione |
|---|---|
| `DashboardPage.xaml` | Overview principale con stato connessione |
| `PacketLoggerPage.xaml` | Debug pacchetti in tempo reale |
| `GalleryPage.xaml` | Galleria screenshot |
| `GumpListPage.xaml` | Lista gump aperti |
| `CountersPage.xaml` | Contatori item |
| `DisplayPage.xaml` | Impostazioni display |
| `SoundPage.xaml` | Impostazioni audio |

#### Finestre Floating NUOVE

| Finestra | Descrizione |
|---|---|
| `HuePickerWindow.xaml` | Selettore colori UO |
| `OverheadMessageOverlay.xaml` | Chat bubbles floating |
| `TargetHPWindow.xaml` | Barra HP target floating |
| `MapWindow.xaml` | Mappa mondo con zoom |

---

### 3.10 Infrastruttura, Multi-Shard e Comandi Chat

#### 3.10.1 Sistema Gestione Shard — ASSENTE (TASK-027)

**Cos'e uno shard?** In Ultima Online, ogni server di gioco si chiama "shard". Esistono server ufficiali (OSI) e centinaia di server privati ("free shard"), ognuno con indirizzo/porta diversi e talvolta client diversi.

**Il legacy** (`Shards.cs`) ha un **gestore shard completo** con CRUD:

```
ShardEntry {
    Description     // "OSI Ultima Online", "UO Eventine"
    ClientPath      // Path al client OSI (client.exe)
    CUOClient       // Path a ClassicUO (ClassicUO.exe)
    ClientFolder    // Cartella dati UO (.mul/.uop)
    Host            // Server address
    Port            // Server port
    PatchEnc        // Patch encryption (bool)
    OSIEnc          // OSI encryption (bool)
    Selected        // Shard attivo (bool)
    StartTypeSelected // OSI o CUO (enum)
}
```

Metodi: `Load()`, `Insert()`, `Update()`, `Delete()`, `Read()`, `Save()`.

**Il nuovo**: Solo `ShardId`/`ShardName` passivi (letti dal pacchetto 0xA8). Nessuna UI di selezione, nessun CRUD, nessuna persistenza.

#### 3.10.2 Adapter Pattern — Stub (TASK-028, TASK-029)

L'interfaccia `IClientAdapter` esiste con 2 implementazioni:
- `ClassicUOAdapter.cs` (31 righe) — **tutti placeholder** (`return true`, `return Array.Empty<byte>()`)
- `OsiClientAdapter.cs` (39 righe) — **tutti placeholder**

L'interfaccia **non e registrata nel DI**, **non e iniettata** in nessun servizio, e `PacketService` **bypassa** completamente il pattern adapter accedendo direttamente alla shared memory.

#### 3.10.3 Sistema Comandi Chat — ASSENTE (TASK-001)

**Cos'e?** Nel legacy, l'utente digita comandi nella chat del gioco preceduti da `-` (es. `-where`, `-ping`, `-getserial`). Il sistema intercetta il messaggio prima che venga inviato al server.

**18 comandi legacy**: `-where`, `-ping`, `-getserial`, `-inspect`, `-inspectgumps`, `-inspectalias`, `-sync`/`-resync`, `-echo`, `-help`/`-listcommand`, `-playscript`, `-setalias`, `-unsetalias`, `-hideitem`/`-hide`, `-drop`, `-reducecpu`, `-pping`.

**Nel nuovo**: Nessuna intercettazione. Il `WorldPacketHandler` processa i pacchetti speech ma **non controlla se il testo inizia con `-`**.

#### 3.10.4 OSI Encryption — Non Supportata (TASK-036)

Il legacy gestisce 2 tipi di encryption:
- **PatchEnc**: Disabilita l'encryption del client (per free shard)
- **OSIEnc**: Mantiene l'encryption attiva (per server ufficiali EA)

Il nuovo ha solo `PatchEncryption` globale. Impossibile connettersi ai server ufficiali OSI.

#### 3.10.5 LegacyMacroMigrator — Conversione Lossy (TASK-026)

`LegacyMacroMigrator.cs` converte macro legacy nel formato testo del nuovo sistema, ma la conversione di condizioni IF complesse perde informazioni:
- Condizioni `Find` con container specifico → perde il filtro container
- Condizioni `Find` con `FindStoreSerial` → perde il salvataggio serial
- Nessun warning all'utente sulla semplificazione

---

## 4. Inventario Completo dei Gap

### 4.1 Task Critici (10)

| ID | Tipo | Descrizione | File da Modificare | Impatto |
|---|---|---|---|---|
| ~~TASK-004~~ | ~~Bug~~ | ~~**Organizer: Amount field non rispettato**~~ | ~~`OrganizerService.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-006~~ | ~~Bug~~ | ~~**BandageHeal: cerca bandage per serial fisso**~~ | ~~`BandageHealService.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-012~~ | ~~Mancante~~ | ~~**ATTACK macro: solo serial**~~ | ~~`MacrosService.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-021~~ | ~~Mancante~~ | ~~**UOSteam: ~140 comandi/espressioni mancanti** — COMPLETATO (GeminiCLI) + bugfix (Claude)~~ | ~~`UOSteamInterpreter.cs` + `ConditionEvaluator.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-022~~ | ~~Mancante~~ | ~~**Condizioni macro: 8 condizioni mancanti** — COMPLETATO (GeminiCLI)~~ | ~~`ConditionEvaluator.cs`~~ | Macro avanzate ok |
| ~~TASK-027~~ | ~~Mancante~~ | ~~**Sistema gestione shard** — CRUD con host/port/encryption per-shard~~ | ~~Nuovi file + `GeneralPage.xaml`~~ | ✅ **COMPLETATO** |
| ~~TASK-028~~ | ~~Mancante~~ | ~~**Riconoscimento intelligente client** — biforcazione OSI/CUO/TmClient~~ | ~~`GeneralViewModel.cs` + `ClientInteropService.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-029~~ | ~~Architettura~~ | ~~**Adapter pattern non collegato** — stub inutilizzati, PacketService bypassa~~ | ~~`ClassicUOAdapter.cs` + `OsiClientAdapter.cs` + `TmClientAdapter.cs` + `ClientAdapterFactory.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-039~~ | ~~Incompleto~~ | ~~**TargetingService: smart targeting mancante**~~ | ~~`TargetingService.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-040~~ | ~~Incompleto~~ | ~~**HotkeyService: 25+ categorie mancanti**~~ | ~~`HotkeyService.cs`~~ | ✅ **COMPLETATO** |

### 4.2 Task Alti (1)

| ID | Tipo | Descrizione | File da Modificare |
|---|---|---|---|
| ~~TASK-041~~ | ~~Incompleto~~ | ~~**DragDropCoordinator: code agente, weight check, Z-level**~~ | ~~`DragDropCoordinator.cs`~~ | ✅ **COMPLETATO** |

### 4.3 Task Medi (16)

| ID | Tipo | Descrizione | File |
|---|---|---|---|
| ~~TASK-001~~ | ~~Mancante~~ | ~~**Commands service** — 18 comandi chat (`-where`, `-ping`, ecc.)~~ | ~~Nuovo `CommandService.cs`~~ |
| ~~TASK-006b~~ | ~~Incompleto~~ | ~~BandageHeal: buff wait prima di ri-applicare~~ | ~~`BandageHealService.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-010~~ | ~~Mancante~~ | ~~MenuResponse packet (0x7D C→S) + coda menu~~ | ~~`PacketBuilder.cs` + `MacrosService.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-013~~ | ~~Mancante~~ | ~~**SETABILITY macro command**~~ | ~~`MacrosService.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-014~~ | ~~Mancante~~ | ~~**PathFinding API per scripting**~~ | ~~Nuovo `PathFindingApi.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-017~~ (COMPLETATO) | Mancante | UI pagina Media (screenshot/video config) | Nuovo `MediaPage.xaml` |
| ~~TASK-023~~ | ~~Mancante~~ | ~~**Counter API non esposta a scripting**~~ | ~~Nuovo `CounterApi.cs`~~ | ✅ **COMPLETATO** |
| TASK-024 | Mancante | ~~GumpInspector response logging~~ | ✅ **COMPLETATO** |
| ~~TASK-026~~ | ~~Incompleto~~ | ~~LegacyMacroMigrator: warning su conversione lossy~~ | ~~`LegacyMacroMigrator.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-030~~ | ~~Stub~~ | ~~**Menu system (6 metodi) non funzionale** — script crafting rotti~~ | ~~`MiscApi.cs` + `WorldPacketHandler.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-031~~ | ~~Stub~~ | ~~**Query String system (3 metodi) non funzionale** — script crafting rotti~~ | ~~`MiscApi.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-034~~ | ~~Stub~~ | ~~FriendApi.ChangeList() no-op~~ | ~~`FriendApi.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-036~~ | ~~Mancante~~ | ~~OSI Encryption non supportata~~ | ~~`ConfigModels.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-038~~ | ~~Architettura~~ | ~~Launch flow supporta solo TmClient~~ | ~~`GeneralViewModel.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-042~~ | ~~Incompleto~~ | ~~SecureTradeService: Offer API, StartTrade~~ | ~~`SecureTradeService.cs`~~ | ✅ **COMPLETATO** |
| ~~TASK-043~~ | ~~Incompleto~~ | ~~FilterHandler: graphic configurabile, filtro per tipo msg~~ | ~~`FilterHandler.cs`~~ | ✅ **COMPLETATO** |

### 4.4 Task Bassi (11)

| ID | Tipo | Descrizione | File |
|---|---|---|---|
| ~~TASK-011~~ | ~~Mancante~~ | ~~UnicodePromptSend packet (0xC2 C→S)~~ | ~~`PacketBuilder.cs`~~ | ✅ **COMPLETATO** |
| TASK-015 | Mancante | DPSMeter API scripting | Nuovo `DPSMeterApi.cs` |
| TASK-016 | Mancante | PacketLogger API scripting | Nuovo `PacketLoggerApi.cs` |
| TASK-018 | Mancante | AutoDoc generazione documentazione API | Nuovo script/servizio |
| TASK-019 | Mancante | Multi/House data service | Nuovo `MultiService.cs` |
| TASK-020 | Incompleto | PacketLogger template parsing | `PacketLoggerService.cs:179` |
| TASK-025 | Incompleto | SoundApi GetMin/MaxDuration() return 0 | `SoundApi.cs` |
| TASK-032 | Stub | MiscApi.GetMapInfo() dati vuoti | `MiscApi.cs` |
| TASK-033 | Stub | MiscApi metodi minori (ExportPythonAPI, GetContPosition, LastHotKey) | `MiscApi.cs` |
| TASK-035 | Stub | StaticsApi.CheckDeedHouse() return false | `StaticsApi.cs` |
| TASK-037 | Architettura | Login Relay non gestito attivamente (riscrittura IP per OSI) | `WorldPacketHandler.cs` |

---

## 5. Piano Sprint

Il piano e diviso in **5 Sprint**, ordinati per impatto sul gameplay. Ogni Sprint ha task e sottotask con indicazione precisa di **cosa fare**, **dove farlo**, e **come verificarlo**.

### Convenzioni

- **File**: Path relativo a `TMRazorImproved/` a meno che non specificato
- **Verifica**: Come testare che il task e completato
- **Dipendenza**: Task che devono essere completati prima
- **Stima**: T-shirt sizing (S = poche ore, M = 1-2 giorni, L = 3-5 giorni, XL = 1+ settimana)

---

### SPRINT 0 — Regressioni Critiche di Gameplay (Stima: 2-3 settimane)

**Obiettivo**: Rendere il gameplay reale (PvP/PvE) possibile con TMRazorImproved.

Senza targeting funzionale e hotkey, l'applicazione e inutilizzabile per qualsiasi attivita in-game che richieda reazioni rapide.

---

#### ✅ TASK-039: TargetingService — Smart Targeting (COMPLETATO)

**Perche**: Senza smart targeting, un giocatore che alterna spell offensive e curative colpira/curera il bersaglio sbagliato. E il bug piu impattante per il gameplay.

**File**: `TMRazorImproved.Core/Services/TargetingService.cs`

**Sottotask**:

**039-A) Aggiungere i 4 tipi di target tracking** (M)
- **Cosa fare**: Aggiungere 3 nuove variabili volatile oltre a `_lastTarget`:
  ```
  _lastHarmTarget   — ultimo target per spell offensive
  _lastBeneTarget   — ultimo target per spell curative
  _lastGroundTarget — ultimo target a terra (coordinate)
  ```
- **Dove**: Nelle variabili di istanza di `TargetingService`
- **Come verificare**: Lanciare una spell offensiva su un nemico, poi una curativa su un alleato. I due target devono essere memorizzati separatamente.

**039-B) Implementare Smart Last Target** (M)
- **Cosa fare**: Quando arriva un target cursor (pacchetto 0x6C), leggere i `TargetFlags`:
  - Flag `0x01` (harmful) → usa `_lastHarmTarget`
  - Flag `0x02` (beneficial) → usa `_lastBeneTarget`
  - Flag `0x00` (neutral) → usa `_lastTarget` generico
- **Dove**: Nel metodo che gestisce l'arrivo del cursor (callback da `WorldPacketHandler` per 0x6C)
- **Come verificare**: Configurare "Smart Last Target" e verificare che spell diverse usino target diversi automaticamente.

**039-C) Implementare Target Queue** (M)
- **Cosa fare**: Aggiungere una `Queue<Action>` privata. Quando `DoLastTarget()` o `DoTargetSelf()` vengono chiamati senza un target cursor attivo (`PendingCursorId == 0`), accodare l'azione. Quando arriva un cursor, dequeue ed eseguire.
- **Dove**: Nuovo campo `_targetQueue` in `TargetingService`, logica in `SendTarget()` e nel handler di 0x6C.
- **Come verificare**: Premere "Last Target" prima che arrivi il cursor → l'azione deve essere eseguita automaticamente appena il cursor arriva.

**039-D) Implementare CheckHealPoisonTarget** (S)
- **Cosa fare**: Prima di inviare un target per spell curative, verificare se il target e avvelenato. Se si, bloccare e mostrare warning. (Opzionale tramite config)
- **Dove**: In `SendTarget()`, check su `Mobile.IsPoisoned`
- **Come verificare**: Tentare di curare un target avvelenato → deve bloccare con messaggio.

**039-E) Implementare Range Check configurabile** (S)
- **Cosa fare**: Aggiungere `RangeCheckEnabled` (bool) e `MaxRange` (int) alla config targeting. Prima di inviare un last target, verificare la distanza.
- **Dove**: In `SendTarget()` → calcolo distanza vs `MaxRange`
- **Come verificare**: Configurare range 10, target a distanza 15 → deve mostrare "out of range".

---

#### ✅ TASK-040: HotkeyService — Azioni Complete (COMPLETATO)

**Perche**: Senza hotkey per spell, pozioni e abilita, il giocatore non puo giocare in modo efficace. E la seconda regressione piu grave.

**File**: `TMRazorImproved.Core/Services/HotkeyService.cs`

**Sottotask**:

**040-A) Registrazione categorie Spell (7 scuole di magia)** (L)
- **Cosa fare**: Per ogni scuola di magia (Magery, Chivalry, Necromancy, Bushido, Ninjitsu, Spellweaving, Mysticism), registrare un'azione hotkey per ogni spell. Usare `SpellDefinitions` per l'elenco completo e `SpellsApi.Cast(spellName)` per l'esecuzione.
- **Dove**: Creare un metodo `RegisterSpellHotkeys()` chiamato al `Start()`. Per ogni spell in `SpellDefinitions`, chiamare `RegisterAction($"Spell:{spellName}", () => _spellsApi.Cast(spellName))`.
- **Stima**: ~180 spell totali tra tutte le scuole. E un loop, non 180 case diversi.
- **Come verificare**: Configurare hotkey "F1 = Fireball", premere F1 in-game → la spell deve essere lanciata.

**040-B) Registrazione abilita arma (Primary/Secondary)** (S)
- **Cosa fare**: Registrare `"Ability:Primary"` e `"Ability:Secondary"` che chiamano `_weaponService.SetPrimaryAbility()` / `SetSecondaryAbility()`.
- **Dove**: In `RegisterSpellHotkeys()` o metodo dedicato.

**040-C) Registrazione Attack (nearest/last)** (S)
- **Cosa fare**: Registrare `"Attack:Nearest"`, `"Attack:Last"` che usano `_targetingService.TargetClosest()` + `_packetService.SendToServer(PacketBuilder.Attack(serial))`.

**040-D) Registrazione Bandage (self/target)** (S)
- **Cosa fare**: Registrare `"Bandage:Self"` e `"Bandage:Last"` che cercano bandage (ItemID 0x0E21) nel backpack e le usano sul target.
- **Dove**: Delegare a `BandageHealService` o implementare inline.

**040-E) Registrazione Pozioni** (S)
- **Cosa fare**: Registrare `"Potion:Heal"`, `"Potion:Cure"`, `"Potion:Refresh"`, `"Potion:Strength"`, `"Potion:Agility"`, ecc. Ognuna cerca la pozione per ItemID nel backpack e fa double-click.
- **Dove**: Lista di pozioni con i rispettivi ItemID in una tabella statica.

**040-F) Registrazione Toggle Agenti** (S)
- **Cosa fare**: Registrare `"Agent:AutoLoot"`, `"Agent:Scavenger"`, ecc. che toggleano start/stop degli agenti.
- **Dove**: Ogni agente ha `StartAsync()`/`StopAsync()` — toggle in base allo stato.

**040-G) Registrazione Dress/Hands** (S)
- **Cosa fare**: Registrare `"Dress:List1"`, `"Undress:List1"`, `"Hands:Clear"`, `"Hands:Toggle"`.

**040-H) Registrazione Script execution** (S)
- **Cosa fare**: Gia parzialmente implementato (prefix `"Script:"`). Verificare che funzioni per tutti i linguaggi.

**040-I) Master Key Toggle** (S)
- **Cosa fare**: Registrare un tasto master che disabilita/riabilita tutti gli hotkey. Utile quando si chatta.
- **Dove**: Flag `_hotkeyEnabled` in `HotkeyService`, check in `CheckHotkey()`.

**040-J) Supporto mouse buttons** (M)
- **Cosa fare**: Estendere il hook per catturare click del mouse (Wheel, X1, X2). Il legacy usa codici 500-504.
- **Dove**: Nel loop di `CheckHotkey()`, aggiungere gestione `WM_XBUTTONDOWN`.

---

#### ✅ TASK-004: Organizer Amount (COMPLETATO)

**File**: `TMRazorImproved.Core/Services/OrganizerService.cs`

**Cosa fare**: Nel loop di spostamento item, aggiungere il rispetto del campo `Amount`:
```
Per ogni item nella config list:
  remaining = configItem.Amount == -1 ? int.MaxValue : configItem.Amount
  Per ogni item corrispondente trovato:
    if remaining <= 0: break
    toMove = min(item.Amount, remaining)
    MoveAsync(item.Serial, destination, toMove)
    remaining -= toMove
```

**Come verificare**: Configurare "sposta 50 bandage". Con 200 bandage nel bag, devono essere spostate solo 50.

---

#### ✅ TASK-006: BandageHeal Ricerca Bandage (COMPLETATO)

**File**: `TMRazorImproved.Core/Services/BandageHealService.cs`

**Sottotask**:

**006-A) Ricerca bandage per ItemID** (S)
- **Cosa fare**: Invece di usare il serial dalla config, cercare nel backpack l'item con `GraphicId == 0x0E21`:
  ```
  var bandage = worldService.Items.FirstOrDefault(i =>
      i.Container == player.Backpack && i.GraphicId == 0x0E21);
  ```
- **Come verificare**: Usare tutte le bandage di uno stack → l'agente deve trovare automaticamente un altro stack.

**006-B) Buff wait (TASK-006b)** (S)
- **Cosa fare**: Prima di applicare una nuova bandage, verificare che il buff `HealingSkill` non sia attivo nel `Player.ActiveBuffs`.
- **Come verificare**: Applicare bandage → attendere che il buff sparisca → solo allora applicare la successiva.

---

#### ✅ TASK-012: ATTACK Macro Avanzato (COMPLETATO)

**File**: `TMRazorImproved.Core/Services/MacrosService.cs`

**Cosa fare**: Nel case `"ATTACK"` di `ExecuteActionAsync`, aggiungere parsing per modalita:
- `ATTACK nearest [notoriety]` → trova il mobile piu vicino con filtro notoriety (enemy, criminal, gray, ecc.)
- `ATTACK farthest [notoriety]` → idem ma il piu lontano
- `ATTACK 0x0190` o `ATTACK bytype graphic` → attacca per graphic ID
- Fallback: `ATTACK serial` (comportamento attuale)

**Come verificare**: Creare macro "ATTACK nearest enemy", avvicinarsi a un mob nemico → deve attaccarlo automaticamente.

---

### SPRINT 1 — Bug Agenti e Automazione (Stima: 1-2 settimane)

**Obiettivo**: Correggere le regressioni degli agenti e aggiungere il coordinamento drag/drop.

---

#### ✅ TASK-041: DragDropCoordinator — Code e Validazione (COMPLETATO)

**File**: `TMRazorImproved.Core/Services/DragDropCoordinator.cs`

**Sottotask**:

**041-A) Aggiungere code per agenti** (M)
- **Cosa fare**: Creare 3 `ConcurrentQueue` interne (loot, scavenger, carver). Gli agenti accodano richieste. Un loop async le processa una alla volta, rispettando i delay configurabili.
- **Dove**: Espandere `DragDropCoordinator` da 53 righe a ~200.

**041-B) Weight check** (S)
- Prima di ogni operazione, verificare `player.Weight + item.Weight <= player.MaxWeight - 5`.

**041-C) Z-Level validation** (S)
- Verificare `|player.Z - item.Z| <= 8`. Se fuori range Z, skippare l'item.

**041-D) Delay configurabili** (S)
- Leggere `ObjectDelay`, `AutoLootDelay`, `ScavengerDelay` dalla config invece di hardcodare 50ms/150ms.

---

#### ✅ TASK-030: Menu System (COMPLETATO)

**File**: `MiscApi.cs`, `WorldPacketHandler.cs`, `MenuStore.cs` (nuovo), `PacketBuilder.cs`

**030-A)** `WorldPacketHandler` parsа 0x7C e scrive in `MenuStore.Set()` ✅
**030-B)** Tutti e 6 i metodi in `MiscApi`: `HasMenu()`, `CloseMenu()`, `MenuContain()`, `GetMenuTitle()`, `WaitForMenu()`, `MenuResponse()` ✅
**030-C)** `PacketBuilder.MenuResponse()` per 0x7D ✅
**TASK-010)** `WAITFORMENU` + `MENURESPONSE` aggiunti a `MacrosService` ✅

---

#### ✅ TASK-001: Commands Service (COMPLETATO)

**File**: Nuovo `TMRazorImproved.Core/Services/CommandService.cs`

**Sottotask**:

**001-A) Creare CommandService con intercettazione speech** (S)
- Registrarsi come viewer per i pacchetti speech outgoing (0x03/0xAD C→S)
- Se il testo inizia con `-`, intercettare e non inviare al server
- Dispatch al command handler corrispondente

**001-B) Implementare comandi priorita alta** (M)
- `-where` → mostra coordinate (legge da `WorldService.Player.Position`)
- `-ping` → mostra latenza (tempo tra invio 0x73 e risposta)
- `-getserial` → attiva target cursor, mostra serial cliccato
- `-inspect` → attiva target, mostra tutte le proprieta OPL
- `-sync`/`-resync` → invia pacchetto resync

**001-C) Implementare comandi rimanenti** (S)
- `-help`, `-playscript`, `-setalias`, `-unsetalias`, `-echo`, ecc.

---

### SPRINT 2 — UOSteam Interpreter (Stima: 2-3 settimane)

**Obiettivo**: Portare la copertura UOSteam dal 30% al 80%+.

---

#### TASK-021: UOSteam Comandi Mancanti (Stima: XL)

**File**: `TMRazorImproved.Core/Services/Scripting/Engines/UOSteamInterpreter.cs`

**Sottotask raggruppati per area**:

**021-A) Comandi Movimento** (M) ✅ **COMPLETATO**
- `fly` → `_miscApi.Fly()`
- `land` → `_miscApi.Land()`
- `run` → come `walk` ma con flag run
- `pathfindto x y z` → `_pathFindingService.NavigateTo(x, y, z)`

**021-B) Comandi Healing** (M) ✅ **COMPLETATO + BUGFIX**
- `miniheal` → tenta Cast skill-based, WaitForTarget + TargetSelf/Target(serial) — **FIX: aggiunto WaitForTarget dopo cast, supporto arg serial**
- `bigheal` → `Cast("Greater Heal")` + WaitForTarget + TargetSelf/Target(serial) — **FIX: aggiunto WaitForTarget + target management**
- `chivalryheal` → `Cast("Close Wounds")`
- `bandageself` → cerca bandage 0x0E21, use + targetself

**021-C) Comandi Targeting Avanzato** (L) ✅ **COMPLETATO + BUGFIX**
- `targettype graphic [color] [containerOrRange]` → **FIX: terzo arg ora gestito come container serial (> 0x7FFF) o range**
- `targetground graphic [color] [range]` → come sopra ma solo items a terra
- `targettile x y z` → target coordinata specifica
- `targettileoffset dx dy dz` → target offset da player
- `targettilerelative serial dx dy` → target offset da serial
- `targetresource tool_serial resource_name` → **FIX: implementato via `_targetApi.TargetResource()`**
- `cleartargetqueue` → pulisci coda target
- `autotargetobject serial` / `cancelautotarget` → setup auto-target

**021-D) Comandi Vendor (S)** ✅ **COMPLETATO (GeminiCLI)**
- `buy` → delegare a `VendorService.SetBuyList()`
- `sell` → delegare a `VendorService.SetSellList()`
- `clearbuy` / `clearsell` → pulisci liste

**021-E) Comandi Context Menu (M)** ✅ **COMPLETATO + BUGFIX**
- `contextmenu serial optionString` → `UseContextMenu(serial, option, 1000)` — **FIX: usa string name matching come legacy**
- `waitforcontext serial intIndex [timeout]` → ContextMenu + WaitForContext + ContextReply(index) — **FIX: separato da contextmenu**

**021-F) Comandi Item Avanzati** (M) ✅ **COMPLETATO (GeminiCLI)**
- `waitforproperties serial timeout` → attendi caricamento OPL
- `moveitemoffset serial ground x y z [amount]` → move con offset
- `movetypeoffset graphic dest x y z [amount]` → move per tipo con offset
- `useonce graphic` → usa item una volta e rimuovi dalla lista
- `clearusequeue` → pulisci coda use-once

**021-G) Comandi Combat/Friends** (S) ✅ **COMPLETATO (GeminiCLI)**
- `getfriend` / `getenemy` → selezione da lista amici/nemici
- `ignoreobject serial` / `clearignorelist` → gestione ignore list

**021-H) Comandi Chat** (S) ✅ **COMPLETATO (GeminiCLI)**
- `partymsg text` / `guildmsg text` / `allymsg text`
- `whispermsg text` / `yellmsg text` / `emotemsg text`

**021-I) Comandi Equip/Pet/Misc** (M) ✅ **COMPLETATO (GeminiCLI)**
- `equipitem serial layer` / `equipwand type`
- `togglemounted` / `togglehands which` / `clearhands which`
- `addfriend` / `removefriend`
- `toggleautoloot` / `togglescavenger`
- `promptalias name` / `playmacro name`
- `virtue name` / `rename serial`
- `feed serial graphic` (pet feeding)
- `clickobject serial`
- `dressconfig name`

**021-J) Comandi UI/Debug (bassa priorita)** (S) ✅ **COMPLETATO (GeminiCLI)**
- `paperdoll` / `helpbutton` / `guildbutton`
- `ping` / `where` / `snapshot`
- `messagebox title text` / `shownames type`
- `hotkeys on/off` / `counter`

---

#### TASK-021-EXP: UOSteam Espressioni Mancanti (Stima: L)

**File**: `TMRazorImproved.Core/Services/ConditionEvaluator.cs` (o logica in `UOSteamInterpreter`)

**Sottotask**:

**EXP-A) Letterali e Resistenze** (S) ✅ **COMPLETATO**
- `true` / `false` → line 986-987 in `EvaluateExpression`
- `physical`/`fire`/`cold`/`poison`/`energy` → lines 1222-1226 in `GetValue`

**EXP-B) Skill dettagliate** (S) ✅ **COMPLETATO**
- `skill name` / `skillvalue name` → valore skill × 10 (in GetValue)
- `skillbase name` → valore base × 10 (in GetValue)
- `skillstate name` → **FIX: implementato** → up=1, down=0, locked=2 (confrontabile con letterali "up"/"down"/"locked")

**EXP-C) Ricerca avanzata** (M) ✅ **COMPLETATO**
- `findtype graphic [color] [container] [range]` → `HandleFindType()`, imposta `_aliases["found"]`
- `findobject serial` → **FIX: imposta `_aliases["found"]`** (fix applicato)
- `findlayer layer [serial]` → cerca item su layer, imposta `_aliases["found"]`

**EXP-D) Proprieta item** (M) ✅ **COMPLETATO**
- `amount serial` → `_items.FindBySerial().Amount` in `GetValue`
- `graphic serial` → `_items.FindBySerial().Graphic` in `GetValue`
- `color serial` → `_items.FindBySerial().Hue` in `GetValue`
- `durability serial` → via `GetPropValue("Durability")` in `GetValue`
- `property serial name` → OPL property check in `EvaluateExpression`

**EXP-E) Notoriety** (S) ✅ **COMPLETATO**
- `criminal serial` / `enemy serial` / `friend serial` / `gray serial` / `innocent serial` / `murderer serial`
- Implementati in `EvaluateExpression` via `_mobiles.FindBySerial().Notoriety`

**EXP-F) Altre** (S) ✅ **COMPLETATO**
- `flying` → `player.Flying`
- `waitingfortarget` → `_targeting.PendingCursorId != 0`
- `direction` → direzione numerica | `directionname` → **FIX: implementato** (confrontabile con "north"/"south"/ecc.)
- `diffmana` / `diffstam` → max - current
- `infriendlist serial` / `ingump gumpId` / `inregion regionName`
- `counttypeground graphic [color] [range]` → conta items a terra

---

#### ✅ TASK-022: Condizioni Macro Mancanti (COMPLETATO)

**File**: `TMRazorImproved.Core/Services/ConditionEvaluator.cs`

Aggiungere nel metodo `EvaluateCondition()`:

| Condizione | Implementazione | Stato |
|---|---|---|
| `MOUNTED` | `player.FindItemByLayer(Layer.Mount) != null` oppure check sul mount serial | ✅ |
| `ISALIVE` / `ALIVE` | `player.IsGhost == false` | ✅ |
| `RIGHTHANDEQUIPPED` | `player.FindItemByLayer(Layer.RightHand) != null` | ✅ |
| `LEFTHANDEQUIPPED` | `player.FindItemByLayer(Layer.LeftHand) != null` | ✅ |
| `FIND graphic container [range]` | Estendere FIND con parametro container opzionale | ✅ |
| `FINDSTORESERIAL` | Salvare il serial trovato in `_aliases["found"]` dopo un FIND | ✅ |
| `INRANGETYPE ITEM graphic range` | Cerca item per graphic entro range | ✅ |
| `INRANGETYPE MOBILE graphic range` | Cerca mobile per body entro range | ✅ |

---

### SPRINT 3 — Architettura Multi-Shard (Stima: 3-5 settimane)

**Obiettivo**: Permettere l'uso di TMRazorImproved con qualsiasi shard UO, non solo TmClient.


---

#### ✅ TASK-027: Sistema Gestione Shard (COMPLETATO)

**Sottotask**:

**027-A) Creare modello ShardEntry** (S) ✅
- **File**: `TMRazorImproved.Shared/Models/Config/ShardConfig.cs`
- Campi: Name, Host, Port, ClientPath, DataFolder, PatchEncryption, OSIEncryption, StartType (enum: OSI/ClassicUO/TmClient), IsSelected

**027-B) Creare IShardService + ShardService** (M) ✅
- **File**: `IShardService.cs` + `TMRazorImproved.Core/Services/ShardService.cs`
- Metodi: `GetAll()`, `GetSelected()`, `Add()`, `Update()`, `Delete()`, `Select()`, `Save()`, `Load()`
- Persistenza: JSON file `Config/shards.json`

**027-C) UI Launcher con tabella shard** (M) ✅
- **File**: `GeneralPage.xaml` + `GeneralViewModel.cs`
- DataGrid con lista shard, pulsanti Add/Edit/Delete/Set Active, dialog CRUD inline
- Selezione shard auto-popola ClientPath, DataPath, Host, Port

---

#### ✅ TASK-028 + TASK-029: Riconoscimento Client e Adapter Pattern (COMPLETATO)

**Sottotask**:

**028-A) Implementare auto-detection tipo client** (M)
- In `GeneralViewModel.LaunchClient()`, leggere il nome dell'eseguibile dal shard selezionato
- Se contiene "tmclient" → `ClientType.TmClient`
- Se e "client.exe" → `ClientType.OSI`
- Altrimenti → `ClientType.ClassicUO`

**028-B) Biforcazione flusso launch** (L)
- TmClient: DeployPlugin + Process.Start + SharedMemory (flusso attuale)
- ClassicUO: DeployPlugin + Process.Start + Crypt.dll injection
- OSI: Loader.dll + Crypt.dll injection (x86 only)

**029-A) Collegare adapter a PacketService** (L)
- Creare `IClientAdapter.ReceivePacket()` implementato diversamente per ogni tipo client
- `PacketService` deve leggere da `_adapter.ReceivePacket()` invece che direttamente dalla shared memory
- Registrare l'adapter corretto nel DI in base al tipo client selezionato

---

#### ✅ TASK-036: OSI Encryption (COMPLETATO)

- `ShardConfig.OSIEncryption` aggiunto ✅
- `GeneralViewModel.OsiEncryption` observable property aggiunta ✅
- `OnSelectedShardChanged` propaga `OSIEncryption` dallo shard ✅
- `LaunchClient()` flags: `PatchEncryption && !OsiEncryption` → bit 0x08 — mutua esclusione garantita ✅

#### ✅ TASK-038: Launch Flow Biforcato (COMPLETATO)

- `GeneralViewModel.LaunchClient()` biforca su `ClientStartType`:
  - OSI → `_clientInterop.LaunchClient()` via Loader.dll ✅
  - ClassicUO/TmClient → `Process.Start` + DeployPlugin + shared memory ✅
- `DetectClientTypeFromPath()` per auto-detect da nome exe ✅

---

### SPRINT 4 — Completamento e Polish (Stima: 2-3 settimane)

**Obiettivo**: Completare tutti i task medi e bassi rimanenti.

---

#### ✅ TASK-013: SETABILITY Macro (COMPLETATO)
- **File**: `MacrosService.cs`
- Aggiungere case `"SETABILITY"` → `_weaponService.SetPrimaryAbility()` / `SetSecondaryAbility()`

#### ✅ TASK-014: PathFinding API Scripting (COMPLETATO)
- Creato `PathFindingApi.cs` con `GetPath(destX,destY)`, `GetPath(startX,startY,startZ,destX,destY,mapId)`, `CanReach(x,y)`, `MoveTo(x,y,z)`
- Aggiunto `PathFind(x,y,z)` a `PacketBuilder.cs`
- Registrato in `ScriptGlobals` + Python scope

#### ✅ TASK-023: Counter API Scripting (COMPLETATO)
- Creato `CounterApi.cs` con `GetCount(graphic, hue)` e `Recalculate()`
- Registrato in `ScriptGlobals` + Python scope

#### ✅ TASK-015 + TASK-016: DPSMeter e PacketLogger API (COMPLETATO)
- Creati `DPSMeterApi.cs` e `PacketLoggerApi.cs`
- Registrati in `ScriptGlobals` + scope Python + `ScriptingService` constructor

#### ✅ ~~TASK-017~~ (COMPLETATO): UI Pagina Media (Stima: M) - RICONTROLLARE
- **File**: Nuovo `MediaPage.xaml` + `MediaViewModel.cs`
- Configurazione: path output, formato screenshot (PNG/JPG), impostazioni video (FPS, codec)
- Collegare a `ScreenCaptureService` e `VideoCaptureService`

#### ✅ TASK-024: GumpInspector Response Logging (COMPLETATO) - RICONTROLLARE
- Aggiungere viewer per pacchetto 0xB1 (GumpResponse C→S) in `WorldPacketHandler`
- Pubblicare `GumpResponseLogMessage` via `IMessenger`
- In `InspectorViewModel`, registrarsi come recipient e mostrare log

#### ✅ TASK-026: LegacyMacroMigrator Warning (COMPLETATO) - RICONTROLLARE
- **File**: `LegacyMacroMigrator.cs`
- Aggiunti commenti `// WARNING` per conversioni lossy (Backpack conversion, FindStoreSerial)
- Implementata migrazione completa per If, ElseIf e While basata su `ConditionType` legacy

#### ✅ TASK-031: Query String System (COMPLETATO)  - RICONTROLLARE MOLTO BENE
- Implementare coda prompt (come TASK-030 per menu)
- Collegare ai metodi stub in `MiscApi`

#### ✅ TASK-034: FriendApi.ChangeList (COMPLETATO) - RICONTROLLARE MOLTO BENE
- Implementare switch lista amici delegando a `FriendsService`

#### ✅ TASK-042: SecureTradeService API (COMPLETATO) - RICONTROLLARE MOLTO BENE
- Aggiungere `Offer(tradeId, gold, platinum)`, `StartTrade()`, container tracking

#### ✅ TASK-043: FilterHandler Granularita (COMPLETATO) - RICONTROLLARE MOLTO BENE
- Rendere il graphic di sostituzione MobileFilter configurabile (non hardcoded 0x0033)
- Aggiungere aggiornamento stato luce quando filtro viene toggleato

#### Task Bassi Rimanenti (Stima: S ciascuno)
- ~~TASK-011: Pacchetto 0xC2 in `PacketBuilder`~~ ✅ **COMPLETATO** - RICONTROLLARE MOLTO BENE
- TASK-018: AutoDoc 
- TASK-019: `MultiService.cs` per house data
- TASK-020: Template parsing in `PacketLoggerService`
- TASK-025: Documentare che `SoundApi.GetMin/MaxDuration()` ritorna 0
- TASK-032: `MiscApi.GetMapInfo()` con dati reali da `MapService`
- TASK-033: `ExportPythonAPI()`, `GetContPosition()`, `LastHotKey()`
- TASK-035: `StaticsApi.CheckDeedHouse()` con dati da `WorldService`
- TASK-037: Login Relay per OSI (dipende da TASK-027/028)

---

## Appendice A — Statistiche Finali

| Metrica | Legacy | TMRazorImproved | Gap |
|---|---|---|---|
| File C# totali | ~253 | ~283 | Piu file grazie a DI/MVVM |
| Servizi Core | Classi statiche | 43 servizi DI | ✅ Architettura migliore |
| API Scripting | 25+ | 18 file (+ 4 mancanti) | ⚠️ 4 API da creare |
| Packet handler S→C | 56 | 60+ | ✅ 4 nuovi |
| Packet handler C→S | 18 | 13 | 5 delegati a TmClient |
| Filtri | 13 classi | Tutti + 5 nuovi | ✅ |
| Azioni macro | 43 classi | 38 comandi | 2 rimossi intenzionalmente |
| Condizioni macro | ~23 tipi | 15 implementati | ⚠️ 8 mancanti |
| UOSteam comandi | 119 | ~38 | ❌ **32%** |
| UOSteam espressioni | 84+ | ~30 | ❌ **~36%** |
| Targeting modes | 4 (harm/bene/ground/general) | 1 (generico) | ❌ Regressione |
| Hotkey categories | 31 | ~5 | ❌ **11%** |
| DragDrop queues | 3 (loot/scav/carve) | 0 | ❌ Regressione |
| Shard supportati | Illimitati (CRUD) | Solo TmClient | ❌ |
| Tipi client | 3 (OSI/CUO/variant) | 1 (TmClient) | ❌ |
| Adapter funzionanti | 2 | 0 (stub) | ❌ |
| Comandi chat | 18 | 0 | ❌ |
| Stub/placeholder | — | 17+ metodi | Audit completo |
| Agenti | 8 | 11 | ✅ 3 nuovi |
| UI pagine | 16 dialoghi separati | 39 XAML (pagine+finestre) | ✅ Piu moderno |
| Nuove finestre | — | 5 (HuePicker, MapWindow, ecc.) | ✅ |
| **Task totali aperti** | — | **38** | |
| **Task critici** | — | **10** | |

### Metriche di Copertura per Asse

| Asse | Copertura | Commento |
|---|---|---|
| Servizi Core (backend dati) | ~90% | World, Skills, Journal sono solidi |
| Servizi di Automazione | ~55% | Targeting, HotKey, DragDrop degradati |
| Scripting API | ~75% | Base solida, stub presenti |
| UOSteam | ~32% | Gap piu grande dell'intera migrazione |
| UI/UX | ~85% | WPF molto piu moderno del WinForms |
| Multi-Shard | ~30% | Solo TmClient, no shard CRUD/adapter |
| **Complessiva ponderata** | **~65%** | Ponderata per importanza gameplay |

---

## Appendice B — Glossario

| Termine | Significato |
|---|---|
| **UO** | Ultima Online — MMORPG del 1997 di Origin/EA |
| **Shard** | Un server di Ultima Online. Ci sono server ufficiali (OSI) e privati (free shard) |
| **TmClient** | Fork personalizzato di ClassicUO per lo shard "The Miracle" |
| **ClassicUO** | Client UO open-source alternativo al client ufficiale |
| **OSI** | "Origin Systems Inc." — indica il client ufficiale EA |
| **Packet** | Un messaggio binario scambiato tra client e server UO |
| **S→C** | Pacchetto dal Server al Client (incoming) |
| **C→S** | Pacchetto dal Client al Server (outgoing) |
| **OPL** | Object Property List — le proprieta degli oggetti (nome, statistiche, ecc.) |
| **Gump** | Finestra di interfaccia del gioco (menu, dialogo, vendor, ecc.) |
| **Target cursor** | Il cursore speciale che appare quando il gioco chiede di selezionare un bersaglio |
| **Smart Last Target** | Sistema che memorizza target diversi per spell offensive vs curative |
| **DI** | Dependency Injection — pattern architetturale per disaccoppiare i componenti |
| **MVVM** | Model-View-ViewModel — pattern architetturale per WPF |
| **IMessenger** | Mediator pattern di CommunityToolkit.Mvvm per comunicazione tra servizi |
| **Shared Memory** | Area di memoria condivisa tra due processi (plugin e UI) |
| **Named Mutex** | Meccanismo di sincronizzazione tra processi con nome globale |
| **Length-prefix** | Protocollo dove ogni messaggio e preceduto dalla sua lunghezza in byte |
| **Crypt.dll** | DLL C++ che intercetta pacchetti di rete del client UO |
| **Loader.dll** | DLL C++ che carica Crypt.dll nel processo del client |
| **AgentServiceBase** | Classe base per tutti gli agenti automatici nel nuovo sistema |
| **Notoriety** | Stato di reputazione di un giocatore (Innocent=blu, Criminal=grigio, Murderer=rosso, ecc.) |
| **PvP** | Player vs Player — combattimento tra giocatori |
| **PvE** | Player vs Environment — combattimento contro mostri/NPC |
| **Macro** | Sequenza di azioni automatizzate (piu semplice degli script) |
| **UOSteam** | Linguaggio di scripting semplificato per UO, il piu popolare nella community |
| **IronPython** | Implementazione Python per .NET usata come motore scripting |
| **Roslyn** | Compilatore C# moderno di Microsoft (sostituisce CodeDom) |
| **Stub** | Metodo che esiste ma non fa nulla / ritorna un valore fittizio |
| **Context Menu** | Menu che appare cliccando con tasto destro su un NPC/oggetto nel gioco |
| **Backpack** | L'inventario del giocatore (container principale) |
| **Serial** | Identificatore univoco (uint32) di ogni oggetto/mobile nel mondo di gioco |
| **Graphic/GraphicId** | L'aspetto visivo di un oggetto (tipo di oggetto) |
| **Hue** | Il colore di un oggetto (0 = default) |
| **Layer** | La posizione di equipaggiamento (mano destra, mano sinistra, testa, ecc.) |
| **Corpse** | Il cadavere di un mobile ucciso, contiene il loot |

---
**Update 19 Marzo 2026**: COMPLETATI TASK-031 (Query String), TASK-034 (FriendApi), TASK-042 (SecureTrade Offer), TASK-043 (FilterHandler Granularità), TASK-011 (Unicode Prompt). (GeminiCLI)

