# TMRazor Improved — Review Architetturale Approfondita

**Data**: 7 Marzo 2026
**Autore**: Senior Architect — Code Review Sprint
**Branch**: `claude/review-tmrazor-migration-KnjR7`
**Scope**: Full codebase inspection — 227 file C#, 4 progetti, test suite completa
**Base di confronto**: TMRazor (RazorEnhanced) .NET Framework 4.8 originale

---

## 1. Executive Summary

TMRazor Improved ha compiuto progressi significativi dall'ultima analisi (3 Marzo 2026, "35-40% feature parity"). Il team ha implementato aree fondamentali precedentemente assenti: sistema macro completo con motore IF/WHILE/FOR, PathFinding A*, DPSMeter, SecureTrade, e ha centralizzato il parsing dei pacchetti nel WorldPacketHandler (70+ handler). La stima attuale di feature parity sale al **65-75%**.

L'architettura di base rimane **solida e ben strutturata**, con eccellente applicazione di DI, MVVM, async/await e thread-safety sulle strutture dati principali. Tuttavia, la presente review ha identificato **nuovi bug critici di thread-safety** in servizi recentemente aggiunti (DPSMeterService, SecureTradeService), una **violazione della migrazione System.Drawing** ancora presente in ScreenCaptureService, e un elenco di feature ancora mancanti rispetto all'originale.

### Stato Complessivo Aggiornato

| Area | Stato Precedente | Stato Attuale | Delta |
|------|-----------------|---------------|-------|
| Infrastruttura DI/Threading | 90% | 95% | +5% |
| Packet Handling | 33% | **85%** | +52% |
| Sistema Macro | 0% | **80%** | +80% |
| Scripting API | 30% | **55%** | +25% |
| PathFinding | 0% | **70%** | +70% |
| Agents | 50% | **75%** | +25% |
| DPS Meter | 0% | **60%** | +60% |
| SecureTrade | 0% | **65%** | +65% |
| UI/MVVM | 40% | **75%** | +35% |
| Thread Safety (globale) | 70% | **78%** | +8% |

---

## 2. Analisi Architetturale — Punti di Forza Confermati

### 2.1 Separazione dei Layer (Eccellente)

Il grafo delle dipendenze rimane un **DAG pulito** senza dipendenze circolari:

```
TMRazorImproved.Shared  →  Contratti, modelli, enums (net10.0)
TMRazorImproved.Core    →  Business logic, servizi (net10.0-windows)
TMRazorImproved.UI      →  WPF + MVVM (net10.0-windows)
TMRazorImproved.Tests   →  xUnit + Moq + Stress + Fuzz
```

Nessuna dipendenza UI→Core di tipo inverso. I servizi ricevono dati strutturati (tipizzati) dal WorldPacketHandler tramite Messenger, non più byte[] raw come nell'architettura iniziale. **Questa è la correzione più importante rispetto alla prima review** (anti-pattern 5.1 ora risolto).

### 2.2 DI Container — Registrazioni Corrette (App.xaml.cs)

La registrazione dei servizi è ora completa e corretta:
- 30+ servizi Core registrati come **Singleton** (corretto: stato condiviso, singola connessione)
- ViewModels stateful registrati come **Singleton** (`PlayerStatusViewModel`, `ScriptingViewModel`, ecc.)
- ViewModels stateless o con stato transitorio registrati come **Transient** (`GeneralViewModel`, `OptionsViewModel`, `FriendsViewModel`)
- **Bug precedente risolto**: nessuna doppia registrazione di `ITargetingService`
- **Bug precedente risolto**: `IScriptingService` correttamente registrato

### 2.3 Centralizzazione Packet Handling (Miglioramento Fondamentale)

WorldPacketHandler ora registra **70+ handler** (vs 20 nella prima analisi):
- Session/Login: 0x1B, 0x55, 0x8C, 0x73, 0xBD, 0xBF (con sub-dispatch completo)
- Mobiles: 0x78(filter), 0x20(filter), 0x77, 0x98, 0x88, 0x16, 0x17
- Items: 0x1A(filter), 0xF3(filter), 0x1D, 0x3C, 0x25, 0x2E, 0x89, 0x24, 0x27
- Stats/Skills: 0x11, 0xA1, 0xA2, 0xA3, 0x2D, 0x3A
- Combat: 0x0B, 0x72, 0xAA, 0x21, 0x22, 0x97
- Messages: 0x1C, 0xAE, 0xAF, 0xC1, 0xC2, 0xCC
- Gumps: 0xB0, 0xDD (compressed), 0x7C, 0xB1(C2S)
- Trade: 0x6F (S2C + C2S)
- Vendor: 0x74, 0x9E
- Effects/Audio: 0xC0, 0x6D, 0x54, 0x65, 0xDF, 0xE2, 0x6E
- Filters: 0x4E, 0x4F
- Map: 0x90, 0xF5, 0x76, 0xF6, 0x56
- OPL: 0xD6
- GameState: 0x2C, 0x95, 0x9A, 0xA8, 0xAB, 0xB8, 0xB9, 0xBA, 0x83, 0xBC, 0xC8, 0xD8, 0xF0
- C2S Viewers: 0x02, 0x05, 0x06, 0x07, 0x08, 0x09, 0x12, 0x13, 0x75, 0xBF, 0xD7

I servizi Agente ricevono ora **messaggi tipizzati** via Messenger (`WorldItemMessage`, `ContainerContentMessage`, `VendorBuyMessage`, ecc.) invece di parsare byte[] raw. Anti-pattern eliminato.

### 2.4 Pattern di Thread-Safety Consolidati

I pattern corretti rimangono applicati su strutture critiche:
- `WorldService._mobiles` e `_items`: `ConcurrentDictionary<uint, T>` — nessun lock necessario per reads/iterations
- `WorldService._player`: `volatile Mobile?` — visibilità cross-thread garantita
- `WorldService.IsCasting`: `volatile bool` — JIT non può cachare in registro
- `WorldService.PartyMembers`: `lock(PartyMembers)` per operazioni add/remove su `HashSet<uint>`
- `Mobile.SyncRoot` / `Item.SyncRoot`: lock applicato nei handler che aggiornano più proprietà atomicamente
- `SkillsService`: `lock(_skillsLock)` su tutte le operazioni della `List<Skill>`
- `ScriptingService`: `SemaphoreSlim(1,1)` per esecuzione script serializzata
- `PacketService`: timer AutoReset=false per prevenire re-entranza sul buffer handler

---

## 3. Nuovi Bug Critici Identificati

### 3.1 CRITICI (P0) — Correggere Immediatamente

---

#### BUG-NEW-01: Race Condition su DPSMeterService — Strutture Dati Non Thread-Safe

**File**: `TMRazorImproved.Core/Services/DPSMeterService.cs`
**Linee**: 17-19, 37-38

```csharp
// ❌ SBAGLIATO — accesso non protetto da thread multipli
private readonly List<(DateTime Time, ushort Amount)> _damageHistory = new();
private readonly Dictionary<uint, long> _targetDamage = new();
public double CurrentDPS { get; private set; }   // ← set da CalculateDPS (Timer thread)
                                                  // ← get da UI thread (PropertyChanged)
```

**Scenario di crash**:
1. `Receive(DamageMessage)` viene chiamato dal **packet thread** → scrive in `_damageHistory` e `_targetDamage`
2. `CalculateDPS()` viene chiamato dal **Timer thread** (ogni 1 secondo) → itera `_damageHistory`, legge `_targetDamage`
3. Concorrenza non gestita: `InvalidOperationException` "Collection was modified during enumeration" o corruzione del Dictionary

**Fix richiesto**:
```csharp
// ✅ CORRETTO
private readonly object _statsLock = new();
// In Receive: lock(_statsLock) { _damageHistory.Add(...); _targetDamage[...] += ...; }
// In CalculateDPS: lock(_statsLock) { /* lettura sicura */ }
// Oppure usare ConcurrentQueue per _damageHistory e ConcurrentDictionary per _targetDamage
```

---

#### BUG-NEW-02: Race Condition su SecureTradeService._trades — Dictionary Non Thread-Safe

**File**: `TMRazorImproved.Core/Services/SecureTradeService.cs`
**Linee**: 16, 79-135, 39-77

```csharp
// ❌ SBAGLIATO — Dictionary acceduto da thread multipli senza lock
private readonly Dictionary<uint, TradeData> _trades = new();

// Thread 1 (PacketThread): Receive(TradeMessage) → _trades[serial] = data;
// Thread 2 (UIThread):     CancelTrade(serial)   → _trades.ContainsKey(...)
// Thread 3 (PacketThread): Receive(TradeMessage) → _trades.TryGetValue(...)
```

**Impatto**: Corruzione del Dictionary con `NullReferenceException` o `KeyNotFoundException` a runtime durante scambi simultanei o interazione UI/rete concorrente.

**Fix richiesto**:
```csharp
// ✅ CORRETTO — usare ConcurrentDictionary o lock esplicito
private readonly ConcurrentDictionary<uint, TradeData> _trades = new();
// Aggiornare tutte le operazioni per usare API ConcurrentDictionary
// (AddOrUpdate, TryGetValue, TryRemove)
```

---

#### BUG-NEW-03: ScreenCaptureService usa System.Drawing — Violazione Migrazione .NET 10

**File**: `TMRazorImproved.Core/Services/ScreenCaptureService.cs`
**Linee**: 3-5, 52-58

```csharp
using System.Drawing;           // ❌ RICHIEDE System.Drawing.Common NuGet package
using System.Drawing.Imaging;   // ❌ Non incluso nativamente in net10.0-windows
// ...
using (Bitmap bmp = CaptureWindow(hWnd))   // ❌ System.Drawing.Bitmap
{
    bmp.Save(fullPath, ImageFormat.Jpeg);  // ❌ ImageFormat.Jpeg
}
```

**Problema**: La roadmap esplicita (TMRazorImprovedProgress.md, Sprint 7) prevede la **rimozione di System.Drawing**. La migrazione è stata completata in UltimaSDK ma NON in ScreenCaptureService. Su .NET 10 puro, `System.Drawing.Common` non è incluso nei framework assembly Windows — richiede `<PackageReference Include="System.Drawing.Common" Version="8.x" />` e genera warning di supporto deprecato.

**Fix richiesto**: Sostituire con `BitmapSource`/`WriteableBitmap` (WPF) o `ImagingFactory` (WIC interop) per il salvataggio dell'immagine. L'accesso al buffer raw della finestra già usa P/Invoke con `PrintWindow` + HBITMAP — completare la conversione al pipeline WPF.

---

### 3.2 IMPORTANTI (P1) — Correggere Prima del Rilascio

---

#### BUG-NEW-04: TargetingService — Thread Safety Incompleta su Campi Critici

**File**: `TMRazorImproved.Core/Services/TargetingService.cs`
**Linee**: 22-24, 37

```csharp
private uint _lastTarget;                    // ❌ non volatile — letto da scripting thread, scritto da UI/packet thread
private List<uint> _targetQueue = new();     // ❌ List non thread-safe — TargetNext() e Clear() da thread diversi
private int _queueIndex = -1;               // ❌ non volatile — letto/scritto insieme a _targetQueue senza lock
private bool _hasPrompt;                     // ❌ non volatile — _hasPrompt vs _hasTargetCursor (volatile): inconsistente
```

Mentre `_hasTargetCursor`, `_pendingCursorId`, `_pendingPromptSerial`, `_pendingPromptId` sono correttamente `volatile`, i campi sopra non lo sono.

**Scenario di race condition**:
- `TargetNext()` (da HotkeyService, thread separato) legge/modifica `_targetQueue` e `_queueIndex`
- `Clear()` (da API script Python, thread separato) svuota `_targetQueue` e resetta `_queueIndex`
- Concurrent access → `ArgumentOutOfRangeException` o indice corrotto

**Fix richiesto**:
```csharp
private volatile uint _lastTarget;           // ✅
private readonly object _queueLock = new();  // ✅ lock per _targetQueue e _queueIndex
private volatile bool _hasPrompt;           // ✅
```

---

#### BUG-NEW-05: MacrosService._recordingBuffer Non Thread-Safe

**File**: `TMRazorImproved.Core/Services/MacrosService.cs`
**Linea**: 43-44

```csharp
// FIX BUG-P1-05: buffer di registrazione macro
private readonly List<string> _recordingBuffer = new();    // ❌ List non thread-safe
private readonly List<Action> _recordingUnsubscribers = new(); // ❌ idem
```

I subscriber di eventi (registrati in `StartRecording`) possono invocare callback dal **packet thread** mentre la UI chiama `StopRecording()` che itera `_recordingUnsubscribers`. Nessun lock applicato.

**Fix richiesto**: Aggiungere `lock(_recordingBuffer)` attorno alle operazioni di add/read sul buffer, e `lock(_recordingUnsubscribers)` sulle operazioni di unsubscribe.

---

#### BUG-NEW-06: OrganizerService — Race Condition su Proprietà Item Durante Spostamento

**File**: `TMRazorImproved.Core/Services/OrganizerService.cs`
**Linee**: 62-65, 76-95

```csharp
// ⚠️ PARZIALMENTE SICURO — snapshot corretto, ma proprietà item mutabili
var itemsToMove = _worldService.Items                         // ← snapshot List
    .Where(i => i.Container == config.Source)                // ← legge Container
    .Where(i => config.ItemList.Any(li => li.Graphic == (int)i.Graphic))
    .ToList();                                               // ← materializzazione

foreach (var item in itemsToMove)  // ← itera snapshot
{
    // item.Container può essere già cambiato (WorldPacketHandler ha aggiornato via 0x25)
    // tra lo snapshot e questa iterazione → item.Serial potrebbe non essere più valido
    MoveItem(item.Serial, config.Destination);
}
```

Il snapshot `.ToList()` protegge dall'`InvalidOperationException` durante l'iterazione, ma le **proprietà individuali dell'Item** (Container, Serial, ecc.) rimangono mutabili dal network thread. Un item già spostato da un altro container event (0x1D RemoveObject, poi re-aggiunto con 0x25) potrebbe risultare in un double-lift.

**Fix**: Snapshot completo con `lock(item.SyncRoot)` prima di leggere le proprietà chiave, o filtro `Container == config.Source` rieseguito appena prima del `MoveItem`.

---

#### BUG-NEW-07: Doppia Registrazione Pacchetto 0x6C (Funzionale ma Non Documentata)

**File**: `WorldPacketHandler.cs:145` + `TargetingService.cs:62`

```csharp
// WorldPacketHandler.cs
_packetService.RegisterViewer(PacketPath.ServerToClient, 0x6C, HandleTargetCursorFromServer);

// TargetingService.cs (costruttore)
_packetService.RegisterViewer(PacketPath.ServerToClient, 0x6C, HandleServerTargetCursor);
```

Entrambi i viewer ricevono lo stesso pacchetto 0x6C. Il comportamento è **intenzionale** (WorldPacketHandler emette il Messenger message, TargetingService aggiorna il proprio stato interno), ma non è documentato. Se in futuro viene aggiunto un terzo handler 0x6C, lo sviluppatore potrebbe non sapere degli altri due. **Non è un bug funzionale ma è un debito tecnico da documentare.**

**Azione**: Aggiungere un commento XML in entrambi i punti di registrazione che descriva l'intenzionalità della co-registrazione.

---

### 3.3 QUALITÀ (P2) — Correggere nel Prossimo Sprint

---

#### BUG-NEW-08: Nullable Reference Type Warnings Sistematici nei GetActiveConfig()

**File**: `OrganizerService.cs:44`, `ScavengerService.cs:55`, `VendorService.cs:43`

```csharp
// ❌ SBAGLIATO — return null per tipo non-nullable (CS8603 warning)
private OrganizerConfig GetActiveConfig()   // ← return type non nullable
{
    var profile = _configService.CurrentProfile;
    if (profile == null) return null;        // ← CS8603: Possible null reference return
    ...
}
```

Con `<Nullable>enable</Nullable>` nel .csproj, questo genera warning CS8603 che in build strict diventano errori. Le chiamate a `GetActiveConfig()` poi non hanno null-check perché il tipo sembra non-nullable.

**Fix**:
```csharp
private OrganizerConfig? GetActiveConfig()  // ✅ annotare nullable
{
    ...
}
// Nei caller: if (config == null) return;
```

---

#### BUG-NEW-09: ItemsApi.WaitForContents — Fallback su WeakReferenceMessenger.Default

**File**: `TMRazorImproved.Core/Services/Scripting/Api/ItemsApi.cs:39`

```csharp
// ⚠️ PARZIALE — messenger iniettato è preferito, ma il fallback default vanifica i test
_messenger = messenger ?? WeakReferenceMessenger.Default;
```

Nei test unitari con un mock `IMessenger`, se il parametro `messenger` è null (costruttore senza DI), il fallback usa il singleton globale. Questo può causare interferenza tra test eseguiti in parallelo che usano `WeakReferenceMessenger.Default`.

**Fix**: Rendere `messenger` obbligatorio nel costruttore (rimuovere `= null` e il fallback) e aggiornare i test.

---

#### BUG-NEW-10: PathFindingService — Range Massimo Hardcoded a 100 Tile

**File**: `TMRazorImproved.Core/Services/PathFindingService.cs:47`

```csharp
int scanMaxRange = Math.Max(distanceX, distanceY) + 2;
if (scanMaxRange > 100) scanMaxRange = 100;   // ⚠️ HARDCODED — A* si ferma a 100 tile
```

UO ha aree aperte ben superiori a 100 tile. Pathfinding su quest che richiedono percorsi lunghi (es. Moon gates → destinazione remota) fallirà silenziosamente restituendo `null`. Il limite è configurabile nell'originale TMRazor tramite `regions.json`.

**Fix**: Esporre il parametro come `maxRange` nel metodo `GetPath()` con default configurabile da `IConfigService`.

---

## 4. Analisi Feature Parity — Cosa Manca Rispetto al TMRazor Originale

### 4.1 Packet Handler — Stato Attuale

**Implementati (70+)**: Copertura quasi completa del protocollo classico UO (Classic, AOS, Stygian Abyss)

**Ancora Mancanti o Parziali**:

| Pacchetto | Nome | Impatto | Note |
|-----------|------|---------|------|
| 0x38 | PathfindingResult | Basso | Risposta del client al server |
| 0x3F | UpdateRange | Basso | Range update display |
| 0x46 | OpenBook | Medio | Libri e runedict non tracciati |
| 0x82 | LoginDenied | Basso | Solo per UX (messaggio errore) |
| 0x86 | ResendCharList | Basso | Re-lista personaggi |
| 0xA9 | CharList | Basso | Lista personaggi |
| 0xAC | PopupMessage | Medio | Messaggi popup server |
| 0xB2 | ChatMessage | Basso | Sistema chat UO (raro) |
| 0xBB | AccountID | Basso | Info account |
| 0xE3/0xEF | KRPackets | Basso | Pacchetti Kingdom Reborn (legacy) |

**Nota positiva**: 0xDD (CompressedGump) è ora registrato e parsato (`HandleCompressedGump` con `DeflateStream`).

### 4.2 Scripting API — Gap Residuo (~55% completamento)

| Modulo | Metodi Originale | Metodi Nuovi | Gap Principale |
|--------|-----------------|-------------|----------------|
| Player | ~100 | ~60 | Mosse speciali avanzate, Title tracking |
| Items | ~80 | ~35 | WaitForContents completo, layer check |
| Mobiles | ~60 | ~30 | AI state, polymorph detection |
| Spells | ~40 | ~20 | All circles, wait-for-mana logic |
| Skills | 58 skills | 58 ✅ | Completo |
| Target | ~15 | ~12 | Filter by notoriety esteso |
| Journal | ~25 | ~18 | Regex search mancante |
| Gumps | ~20 | ~15 | Radio button handling |
| Statics | ~15 | ~8 | LOS check, terrain height query |
| Misc | ~30 | ~20 | File I/O, advanced delay |
| **PathFinding** | Completo | **Implementato** ✅ | — |
| **DPSMeter** | Completo | ~60% | Thread-safety da fixare |
| **Trade** | Completo | ~65% | UI binding incompleta |
| Timer | ~8 | ~6 | Global timer list mancante |
| **C# Roslyn** | Completo | **Assente** ❌ | Engine non implementato |

**Totale stimato**: ~200 metodi implementati su ~570 originali (**~35% del totale**)
Nota: i 58 skills sono un'implementazione completa che pesa molto sulla percentuale.

### 4.3 Sistema Macro — Stato Attuale (80%)

**Implementati (IF/WHILE/FOR engine completo)**:
- Control flow: IF/ELSEIF/ELSE/ENDIF, WHILE/ENDWHILE, FOR/ENDFOR, BREAK/CONTINUE
- Hotkeys: USESKILL, CAST, ATTACK, USEITEM, EQUIPITEM, MOUNT, DISMOUNT
- Journal/messaging: SAY, WHISPER, EMOTE, YELL
- Utility: PAUSE, RESYNC, SETTIMER, INTERRUPT, OVERHEAD

**Mancanti (20%)**:
- `PICKUP` / `DROP` / `MOVEITEM` — gestione manuale item (azioni inventario)
- `RESPONDGUMP` / `WAITFORGUMP` — interazione con gump da macro
- `WAITFORTARGET` / `TARGETSERIAL` — targeting avanzato da macro
- `SETVAR` / `GETVAR` — variabili macro persistenti (cross-session)
- Macro recorder automatico (registra azioni del giocatore in tempo reale)

### 4.4 Agents — Stato Implementazione Aggiornato

| Agent | Stato | Cosa Manca |
|-------|-------|-----------|
| AutoLoot | **80%** | Paperdoll layer validation, delay per-item configurabile |
| Scavenger | **80%** | Terrain/obstacle check (richiede map data) |
| Organizer | **70%** | Destination-full handling, stacking items |
| BandageHeal | **75%** | Poison detection (0x11 flag), self-targettimng fallback |
| Dress | **65%** | Paperdoll visuale (slot UI), layer conflict detection |
| Restock | **50%** | Loop automatico e trigger da container open mancanti |
| Vendor | **70%** | Hue/name matching nella buy list, buy-per-click mode |
| AutoCarver | **40%** | Solo struttura base, logica corpse-search limitata |
| BoneCutter | **40%** | Idem AutoCarver |
| AutoRemount | **60%** | Mount-by-graphic fallback mancante |

### 4.5 Feature Completamente Mancanti vs TMRazor Originale

| Feature | Note |
|---------|------|
| **C# Roslyn Scripting Engine** | Solo Python (IronPython 3.4) e UOSteam. Il terzo motore dell'originale assente. |
| **SpellGrid Overlay** | `SpellGridWindow.xaml` e `SpellGridViewModel.cs` esistono come stub — nessuna logica di positioning/rendering |
| **HP Bar Overlay (TopMost)** | `TargetHPWindow.xaml` esiste ma solo come widget — manca il positioning topmost e l'autohide |
| **Map/Radar Real-Time** | `MapWindow.xaml` e `MapControl.xaml` esistono. Il rendering WriteableBitmap di UltimaSDK è pronto ma il loop di aggiornamento real-time con player position non è cablato |
| **DisplayPage/Counters** | `CountersPage.xaml` e `CountersViewModel.cs` esistono. `CounterService.cs` è implementato ma il binding alla TitleBar non è completo (TitleBarService separato) |
| **Theme Toggle** | `ThemeService.cs` nell'UI esiste ma la persistenza della preferenza e il toggle dal menu non sono implementati |
| **Profile Per-Shard** | Config è per-character. Profili per-shard (diversi set per Produzione, Atlantic, ecc.) non supportati |
| **SoundPage Volume Slider** | `SoundPage.xaml` e `SoundViewModel.cs` esistono. `SoundService.SetVolume()` usa Core Audio API via P/Invoke ma l'UI non ha il control volume bindato |
| **Paperdoll Slot Visualization** | `DressPage` non ha la griglia visiva slot-per-slot dell'originale |
| **Static Map Inspector** | Terrain height, LOS check, collision (richiede UltimaSDK Tiles + Statics API) |
| **Pathfinding Debug Overlay** | Nessun visualizzatore del percorso A* per debug |
| **Configurazione File** | `doors.json`, `foods.json`, `regions.json`, `maps.json`, `gump_ignore.json`, `soundfilters.json` — assenti |
| **Screen Capture** | Implementazione con System.Drawing.Bitmap (bug-new-03) — non funzionante su .NET 10 senza compat package |

---

## 5. Anti-Pattern Residui e Osservazioni Architetturali

### 5.1 MorphGraphic in WorldPacketHandler — Accoppiamento Config/Presentation

```csharp
// WorldPacketHandler.cs linee 51-81
private void MorphGraphic(ref ushort graphic, ref ushort hue)
{
    if (_configService.CurrentProfile == null) return;
    if (_configService.CurrentProfile.StaticFields) { /* graphic swap */ }
    var custom = _configService.CurrentProfile.GraphFilters.FirstOrDefault(...);
}
```

`MorphGraphic` legge direttamente dal profilo di config e modifica i byte del pacchetto **prima** che venga processato. È una responsabilità che appartiene a `FilterHandler` (già esistente per luce/meteo/suono). Averla in WorldPacketHandler viola il principio di singola responsabilità e rende più difficile l'aggiunta di nuovi filtri grafici.

**Raccomandazione**: Spostare `MorphGraphic` in `FilterHandler` e registrarlo come Filter (non Viewer) per 0x78 e 0xF3.

### 5.2 AgentServiceBase.GetActiveConfig() — Pattern Ripetuto Senza Astrazione

```csharp
// Identico in: OrganizerService, ScavengerService, VendorService, AutoLootService, ...
private XxxConfig GetActiveConfig()
{
    var profile = _configService.CurrentProfile;
    if (profile == null) return null;
    return profile.XxxLists.FirstOrDefault(l => l.Name == profile.ActiveXxxList)
           ?? profile.XxxLists.FirstOrDefault();
}
```

Lo stesso pattern è ripetuto 7 volte con leggere variazioni. Una `AgentServiceBase<TConfig>` generica con `abstract string GetActiveListName()` e `abstract IReadOnlyList<TConfig> GetConfigList()` eliminerebbe la duplicazione.

### 5.3 Mancanza di IDiagnosticsService / Health Check

Non esiste un modo programmatico per interrogare lo stato del sistema:
- Pacchetti/s processati
- Mobile/Item totali in World
- Script in esecuzione (booleano)
- Mutex lock state per il shared memory

Il `PacketService` ha già `_diagTick` con `Trace.WriteLine` ma non espone metriche via interfaccia. Con 70+ handler attivi e 10+ agent service, il debugging di performance diventa difficile senza strumenti diagnostici.

**Raccomandazione**: Aggiungere `IDiagnosticsService` con contatori `Interlocked` (packet counts, entity counts) e una `DiagnosticsPage` leggera.

### 5.4 SearchService._dynamicProviders — Lock Granularity Issue

```csharp
// SearchService.cs - GetFullList()
lock (_dynamicProviders)
{
    foreach (var provider in _dynamicProviders.Values)
    {
        list.AddRange(provider());   // ❌ provider() chiamata DENTRO il lock
    }
}
```

Le lambda `provider()` possono accedere a `_worldService.Items` o altri servizi che acquisiscono i propri lock. Se uno di questi servizi chiama `SearchService.RegisterCategory()` (che acquisisce `lock(_dynamicProviders)`) dall'interno del callback, si ha un potenziale **deadlock**.

**Fix**: Copiare i valori del Dictionary fuori dal lock, poi invocare i provider senza lock.

### 5.5 Gestione Lifecycle Transient ViewModel con Messenger

```csharp
// App.xaml.cs
services.AddTransient<FriendsViewModel>();
services.AddTransient<FiltersViewModel>();
services.AddTransient<OptionsViewModel>();
```

I ViewModel Transient che si iscrivono a `IMessenger` devono implementare `IDisposable` e de-registrarsi. Con `WeakReferenceMessenger`, il GC alla lunga de-registrerà i subscriber automaticamente, ma nel breve termine (navigazione avanti/indietro ripetuta) più istanze accumulate possono ricevere lo stesso messaggio.

Verificare che tutti i ViewModel Transient che usano `_messenger.Register<T>()` implementino `IDisposable` con `_messenger.UnregisterAll(this)`.

---

## 6. Qualità del Codice — Metriche

| Metrica | Score | Note |
|---------|-------|------|
| **Architettura Layer** | 9.5/10 | DAG pulito, DI corretto, nessuna dipendenza circolare |
| **Thread Safety (Core)** | 7/10 | Buona su WorldService, PacketService. Gap in DPSMeter, SecureTrade, Targeting |
| **Naming Convention** | 8.5/10 | Consistente. Qualche inconsistenza in `_hasPrompt` (non volatile) vs `_hasTargetCursor` (volatile) |
| **Documentazione XML** | 7/10 | Buona su PlayerApi/ItemsApi, assente in molti Handler |
| **Error Handling** | 7.5/10 | Try-finally applicati correttamente. Packet handler mancano di length validation uniforme |
| **Test Coverage** | 7.5/10 | 15+ suite con stress/fuzz. Mancano integration test per Targeting, DPS, Trade |
| **Performance** | 8.5/10 | Async/await corretto, UiThrottler 100ms, mutex non bloccante |
| **Manutenibilità** | 8.5/10 | Interfacce chiare, DI facilita l'estensione. Codice duplicato in GetActiveConfig() |
| **Sicurezza** | 8/10 | Sandboxing Python (sys.settrace). Nessuna path traversal in config loader. Marshalling con charset espliciti |

**Media Ponderata**: **8.0/10**

---

## 7. Roadmap Correzioni Raccomandata

### Sprint Immediato — Bug Fix P0 (Entro 1 Settimana)

| ID | Priorità | File | Azione |
|----|----------|------|--------|
| BUG-NEW-01 | P0 | DPSMeterService.cs | Aggiungere lock su `_damageHistory` e `_targetDamage`, o sostituire con `ConcurrentQueue`/`ConcurrentDictionary` |
| BUG-NEW-02 | P0 | SecureTradeService.cs | Sostituire `Dictionary<uint, TradeData>` con `ConcurrentDictionary<uint, TradeData>` |
| BUG-NEW-03 | P0 | ScreenCaptureService.cs | Rimuovere dipendenza `System.Drawing`. Usare `BitmapSource.Create()` WPF per encoding JPEG |

### Sprint 11 — Bug Fix P1 e Qualità (2 Settimane)

| ID | Priorità | File | Azione |
|----|----------|------|--------|
| BUG-NEW-04 | P1 | TargetingService.cs | Aggiungere `volatile` a `_lastTarget`, `_hasPrompt`. Proteggere `_targetQueue` con lock |
| BUG-NEW-05 | P1 | MacrosService.cs | Aggiungere lock a `_recordingBuffer` e `_recordingUnsubscribers` |
| BUG-NEW-06 | P1 | OrganizerService.cs | Snapshot item con `lock(item.SyncRoot)` prima di MoveItem |
| BUG-NEW-07 | P1 (doc) | WorldPacketHandler.cs + TargetingService.cs | Documentare co-registrazione 0x6C con commento XML |
| BUG-NEW-08 | P2 | OrganizerService/ScavengerService/VendorService | Annotare `GetActiveConfig()` con `?` nullable |
| BUG-NEW-09 | P2 | ItemsApi.cs | Rendere `messenger` obbligatorio nel costruttore |
| BUG-NEW-10 | P2 | PathFindingService.cs | Esporre `maxRange` come parametro configurabile |

### Sprint 12 — Feature Completion (3-4 Settimane)

**Ordine di priorità per massimo impatto utente**:

1. **SpellGrid overlay** — Finalizzare `SpellGridWindow`: positioning topmost, click-through, layout 7 cerchi × 8 spell
2. **HP Bar TopMost** — Finalizzare `TargetHPWindow`: autohide quando non in target, draggable
3. **Map real-time rendering** — Collegare il loop di aggiornamento di `MapViewModel` al player position via `PlayerStatusMessage`
4. **Macro recorder** — Implementare registrazione automatica delle azioni (intercettare C2S 0x12, 0x06, 0x07, 0x08, 0x13)
5. **C# Roslyn engine** — Implementare terzo motore di scripting (Microsoft.CodeAnalysis.CSharp.Scripting NuGet)
6. **CountersPage → TitleBarService** binding completo

### Sprint 13 — Polish e Release Prep

1. Migrazione completa `System.Drawing` (ScreenCaptureService → WIC/BitmapSource)
2. Theme Toggle con persistenza in `ConfigService`
3. Profili per-shard
4. File di configurazione mancanti (`doors.json`, `regions.json`, ecc.)
5. `IDiagnosticsService` con `DiagnosticsPage`
6. Integration test per Targeting (0x6C/0x9A roundtrip), DPSMeter, SecureTrade

---

## 8. Confronto Qualitativo con TMRazor Originale

| Aspetto | TMRazor (originale) | TMRazor Improved | Giudizio |
|---------|--------------------|--------------------|----------|
| Framework | .NET 4.8 WinForms | .NET 10 WPF | ✅ Molto migliore |
| Architettura | Monolitico, static class | DI + MVVM + Messenger | ✅ Molto migliore |
| Threading | Thread.Abort, lock globali | CancellationToken, async | ✅ Molto migliore |
| Testabilità | Nessun test | xUnit + Moq + Stress + Fuzz | ✅ Molto migliore |
| Packet handling | 60+ pacchetti | **70+ pacchetti** | ✅ Superato |
| Scripting API | 570+ metodi | ~200 metodi | ❌ Gap residuo |
| Macro system | 41 action types + recorder | 25 action types, no recorder | ⚠️ In avanzamento |
| PathFinding | A* con regions.json | A* senza config regions | ⚠️ Parziale |
| UI/UX | WinForms dark theme | WPF Fluent UI | ✅ Molto migliore |
| Performance | Baseline (sync blocking) | Async throttling 10fps | ✅ Migliore |
| Manutenibilità | Difficile (strong coupling) | Facile (DI + interfaces) | ✅ Molto migliore |
| Thread Safety | Lock globali grossolani | Lock fine-grained, volatile | ✅ Migliore (con gap residui) |
| Screen Capture | GDI+ Win32 | System.Drawing (da rimuovere) | ⚠️ Regressione tecnica |
| C# Scripting | Roslyn completo | Assente | ❌ Regressione funzionale |

---

## 9. Conclusioni

### Punti di Forza della Versione Attuale

L'architettura di TMRazor Improved è **significativamente superiore** all'originale su tutti i fronti tecnici: DI container, MVVM, async/await, thread-safety sulle strutture world state, e testabilità con stress/fuzz test. Il progresso rispetto alla prima analisi è notevole: da 35% a 65-75% di feature parity in circa 4 settimane di sviluppo.

La centralizzazione del parsing pacchetti in WorldPacketHandler (70+ handler) è la correzione architetturale più importante rispetto alla review precedente, eliminando l'anti-pattern di parsing distribuito tra i servizi agente.

### Punti di Attenzione Critici

I **3 bug P0** identificati (DPSMeterService, SecureTradeService, ScreenCaptureService) devono essere corretti **prima di qualsiasi test con utenti esterni**. Il bug di DPSMeter può causare crash a runtime durante sessioni di combattimento intense. Il bug di SecureTrade può corrompere lo stato degli scambi. Il ScreenCaptureService non funzionerà su .NET 10 senza modifiche.

### Rischio Principale Residuo

Il gap nella **Scripting API** (~200 vs 570 metodi) è il rischio principale per l'adozione da parte di utenti che migrano script esistenti da RazorEnhanced. Ogni script Python che usa `Player.AllFollowers`, `Items.ContainerExists()`, o qualsiasi metodo non implementato causerà un `AttributeError` silenzioso o un'eccezione bloccante. Si raccomanda di creare una **mappa di compatibilità API** che elenca esplicitamente i metodi implementati vs. quelli che sollevano `NotImplementedException`.

### Raccomandazione Finale

Il software è **quasi pronto per una Beta pubblica limitata** con la seguente condizione: i 3 bug P0 devono essere corretti e un annuncio di "API compatibility matrix" deve accompagnare il rilascio per gestire le aspettative degli utenti che migrano script esistenti.

**Stima per rilascio v1.0-beta**: 2-3 sprint aggiuntivi (6-9 settimane).

---

*Documento generato come parte della review architetturale Sprint — TMRazor Improved, 7 Marzo 2026.*
*Branch di riferimento: `claude/review-tmrazor-migration-KnjR7`*
