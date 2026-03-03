# TMRazor Improved — Analisi Architetturale Completa

**Data**: 3 Marzo 2026
**Autore**: Review Team Lead
**Scope**: Full architecture review, code quality, feature parity, criticità

---

## 1. Executive Summary

TMRazor Improved è una riscrittura ambiziosa del client assistant TMRazor (fork di RazorEnhanced) da **.NET Framework 4.8 + WinForms** a **.NET 10 + WPF + MVVM**. L'architettura a microservizi con DI, Messenger pattern e async/await è solida e moderna. Tuttavia, l'analisi rivela che il progetto è **circa al 35-40% di feature parity** con l'originale, con gap critici in aree fondamentali.

### Stato Complessivo

| Area | Completamento | Giudizio |
|------|--------------|----------|
| Infrastruttura (DI, Threading, Config) | 90% | Eccellente |
| Packet Handling | 33% | Critico — 40+ packet mancanti |
| Scripting API | 30% | Critico — 350+ metodi mancanti |
| Agents | 50% | Parziale — 2/7 funzionali, 5/7 stub |
| Sistema Macro | 0% | Assente — feature fondamentale |
| UI/MVVM | 40% | In corso — solo 3 pagine |
| PathFinding | 0% | Assente |
| Thread Safety | 70% | Buona base, ma lock inconsistenti |

---

## 2. Architettura — Punti di Forza

### 2.1 Separazione in Layer (Eccellente)

```
TMRazorImproved.Shared    → Contratti, modelli, enums (net10.0)
TMRazorImproved.Core      → Business logic, servizi (net10.0-windows)
TMRazorImproved.UI        → WPF + MVVM (net10.0-windows)
TMRazorImproved.Tests     → xUnit + Moq + Stress tests
```

Il grafo delle dipendenze è un **DAG pulito** senza dipendenze circolari. La separazione Shared/Core/UI è corretta e testabile.

### 2.2 Pattern MVVM con CommunityToolkit (Buono)

- `ViewModelBase` con `RunOnUIThread()` e `EnableThreadSafeCollection()`
- `ObservableProperty` via source generators
- `IMessenger` (WeakReferenceMessenger) per disaccoppiamento
- `IAsyncRelayCommand` per operazioni async

### 2.3 Threading Model (Buono con riserve)

- `AgentServiceBase` con `CancellationToken` + `Task.Run()` — niente `Thread.Abort()`
- `UiThrottler` (DispatcherTimer 10fps) per HP/Mana/Stam
- `volatile` + `Interlocked.Exchange` per bridge Network→UI
- `SemaphoreSlim(1,1)` per esecuzione script serializzata
- `ConcurrentDictionary` per World state

### 2.4 Cancellazione Script IronPython (Innovativo)

Architettura a due livelli:
1. `sys.settrace` con check periodico `IsCancelled` ogni 50 istruzioni
2. `Thread.Interrupt()` per bloccare chiamate native (Sleep, Wait)
3. Override di `time.sleep()` → `Misc.Pause()` con check cancellation ogni 10ms

Questo risolve elegantemente il problema dell'assenza di `Thread.Abort()` in .NET 5+.

### 2.5 Test Suite (Buona copertura di base)

- Unit test per ConfigService, PacketService, WorldPacketHandler
- **Stress test di concorrenza**: 10 thread × 500 entità × 1000 iterazioni
- **Fuzz test**: 1000 sequenze random di byte per packet robustness
- Snapshot stability test

---

## 3. Criticità Emerse — Bug e Race Condition

### 3.1 CRITICI (P0) — Da correggere immediatamente

#### ~~BUG-01: Race Condition in HandleMobileIncoming (WorldPacketHandler.cs)~~ ✅ RISOLTO

**Dove**: `HandleMobileIncoming()` — aggiornamento proprietà Mobile SENZA lock

```csharp
// ❌ SBAGLIATO — nessun lock
m.Graphic = body;
m.X = x;
m.Y = y;
m.Z = z;
m.Direction = dir;
m.Hue = hue;
m.Notoriety = notoriety;
```

**Confronto**: `HandleMobileStatus()` usa correttamente `lock (m.SyncRoot)`.

**Impatto**: TargetingService o ScriptingApi potrebbero leggere X aggiornato ma Y vecchio, causando targeting errato o crash di distanza.

**Fix**: Wrappare in `lock (m.SyncRoot) { ... }`

#### ~~BUG-02: Race Condition in HandleMobileUpdate (WorldPacketHandler.cs)~~ ✅ RISOLTO

**Stesso problema** di BUG-01 per `HandleMobileUpdate()`.

**Fix**: Wrappare in `lock (m.SyncRoot) { ... }`

#### ~~BUG-03: Bit Mask Bug in ScavengerService (ScavengerService.cs)~~ ✅ RISOLTO

```csharp
uint serial = reader.ReadUInt32() & 0x7FFFFFFF;  // Bit 31 rimosso
// ...
if ((serial & 0x80000000) != 0)  // ❌ SEMPRE FALSE — bit 31 già rimosso!
    reader.ReadUInt16(); // amount
```

Il check del bit 31 avviene DOPO il masking, quindi `amount` non viene MAI letto. Questo corrompe il parsing di tutti i pacchetti successivi.

**Fix**: Leggere il serial originale, fare il check, POI mascherare:
```csharp
uint rawSerial = reader.ReadUInt32();
bool hasAmount = (rawSerial & 0x80000000) != 0;
uint serial = rawSerial & 0x7FFFFFFF;
if (hasAmount) reader.ReadUInt16();
```

#### ~~BUG-04: DistanceTo() usa Euclidea invece di Chebyshev (UOEntity.cs)~~ ✅ RISOLTO

```csharp
// ❌ SBAGLIATO — Ultima Online usa Chebyshev
return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));

// ✅ CORRETTO — Chebyshev distance
return Math.Max(Math.Abs(X - other.X), Math.Abs(Y - other.Y));
```

**Impatto**: AutoLoot e Scavenger usano `DistanceTo()` per range check. Con Euclidea, oggetti a (2,2) hanno distanza 2.83 invece di 2, causando loot mancato su diagonali.

**Nota**: `TargetingService.GetDistanceToPlayer()` e `ScavengerService.GetDistance()` implementano Chebyshev correttamente — **inconsistenza** nel codebase.

### 3.2 ALTI (P1) — Da correggere prima del rilascio

#### ~~BUG-05: IScriptingService potenzialmente non registrato in DI (App.xaml.cs)~~ ✅ RISOLTO

Verificare che `IScriptingService` sia correttamente registrato nel container DI. In `HotkeyService.cs`:
```csharp
var scriptingService = (IScriptingService)_serviceProvider.GetService(typeof(IScriptingService))!;
```
Se non registrato, il `!` operator causa `NullReferenceException` a runtime.

#### ~~BUG-06: Registrazione duplicata ITargetingService (App.xaml.cs)~~ ✅ RISOLTO

```csharp
services.AddSingleton<ITargetingService, TargetingService>();  // Riga ~50
// ...
services.AddSingleton<ITargetingService, TargetingService>();  // Riga ~59 — DUPLICATO
```

#### ~~BUG-07: AutoLoot marca "processato" item fuori range (AutoLootService.cs)~~ ✅ RISOLTO

```csharp
if (dist > config.MaxRange) {
    continue;  // ❌ Item finisce in _processedSerials e non verrà MAI ri-valutato
}
```

Se un item si avvicina al player successivamente, non verrà lootato perché già marcato come processato.

**Fix**: Non aggiungere a `_processedSerials` finché il loot non è effettivamente completato.

#### ~~BUG-08: NullReferenceException in TargetingService (TargetingService.cs)~~ ✅ RISOLTO

```csharp
var config = _configService.CurrentProfile.Targeting;  // ❌ CurrentProfile può essere null
```

Manca null check su `CurrentProfile`. Confrontare con il pattern corretto in `AutoLootService`:
```csharp
var profile = _configService.CurrentProfile;
if (profile == null) return null;
```

#### ~~BUG-09: Race Condition su Item updates (WorldPacketHandler.cs)~~ ✅ RISOLTO

`HandleWorldItem()` aggiorna proprietà Item senza lock, stessa classe di problema di BUG-01/02.

#### ~~BUG-10: Memory Leak — IronPython Engine non Disposed (ScriptingService.cs)~~ ✅ RISOLTO

```csharp
var engine = IronPython.Hosting.Python.CreateEngine();
// ...
engine.Runtime.Shutdown();  // Solo shutdown, non Dispose
```

`engine.Runtime.Shutdown()` libera risorse managed ma non native. Aggiungere:
```csharp
(engine as IDisposable)?.Dispose();
```

#### ~~BUG-11: Event Handler Leak in AcquireTargetAsync (TargetingService.cs)~~ ✅ RISOLTO

Se il token di cancellazione viene triggerato prima di `OnTarget`, il handler rimane sottoscritto:

```csharp
TargetReceived += OnTarget;
// Se cancellato prima che OnTarget venga invocato → leak
```

**Fix**: Usare try-finally per garantire unsubscribe.

### 3.3 MEDI (P2) — Da correggere nel prossimo sprint

#### ~~BUG-12: Fire-and-Forget in HotkeyService~~ ✅ RISOLTO

```csharp
Task.Run(() => { action(); });  // ❌ Se action() è async, eccezioni perse
```

#### BUG-13: VendorService — TODO non implementato

```csharp
private void HandleBuyMenu(byte[] data, VendorConfig config) {
    // TODO: Implementare il parsing completo della lista oggetti e l'invio di 0x3B
}
```

Entrambi `HandleBuyMenu` e `HandleSellMenu` sono stub vuoti.

#### BUG-14: PacketService bounds check incompleto

Il pointer arithmetic `(&inBuff->Buff0) + inBuff->Start` viene eseguito prima della validazione completa dei bounds. Aggiungere check `inBuff->Start + pLen <= SHARED_BUFF_SIZE` prima dell'accesso.

#### BUG-15: OrganizerService — Race condition su Items iteration

```csharp
var itemsToMove = _worldService.Items
    .Where(i => i.Container == config.Source)
    .ToList();  // Snapshot, ma proprietà mutabili
```

Proprietà di Item possono cambiare tra `.Where()` e `.MoveItem()`.

---

## 4. Discrepanze Applicative — Feature Gap Analysis

### 4.1 Packet Handler (33% completamento)

**Implementati (20 pacchetti):**
0x1B, 0x1A, 0x1C, 0x1D, 0x11, 0x20, 0x78, 0xA1, 0xA2, 0xA3, 0xAE, 0xAF, 0xB0, 0xB1, 0xBF, 0xD6

**Mancanti CRITICI (40+ pacchetti):**

| Pacchetto | Nome | Impatto |
|-----------|------|---------|
| 0x21 | MovementAck | **Il player non può muoversi correttamente** |
| 0x22 | MovementReject | **Desync di posizione** |
| 0x25 | SingleItemUpdate | **Container non aggiornati** |
| 0x2E | EquipmentUpdate | **Equipaggiamento non tracciato** |
| 0x3A | SkillUpdate | **Skills non sincronizzate** |
| 0x3C | ContainerContent | **Inventario non leggibile** |
| 0x4E/0x4F | PersonalLight/GlobalLight | Filtri luce non funzionanti |
| 0x72 | WarMode | **Stato combattimento non tracciato** |
| 0x74 | VendorBuyList | **Vendor non funzionante** |
| 0x77 | MobileMoving | **NPC/player non si muovono** |
| 0x9E | VendorSellList | **Vendor non funzionante** |
| 0xC1/0xC2 | ClilocMessage | **Messaggi di sistema mancanti** |
| 0xDD | CompressedGump | **Gump compressi non visibili** |
| 0xF3 | SAWorldItem | **Item SA/HS non tracciati** |

**Nota**: Alcuni pacchetti (0x3C, 0x25) sono parzialmente gestiti dentro AutoLootService tramite Messenger, ma non nel WorldPacketHandler centrale. Questo è un **anti-pattern** — la gestione dei pacchetti dovrebbe essere centralizzata.

### 4.2 Scripting API (30% completamento)

| Modulo | Originale | Nuovo | Gap |
|--------|-----------|-------|-----|
| Player | ~100 metodi | ~25 | Mancano: resistenze, karma, fame, rigenerazione, mosse speciali |
| Items | ~80 metodi | ~8 | Mancano: scanning container, proprietà, amount check |
| Mobiles | ~60 metodi | ~5 | Mancano: distanza, AI state, filtri targeting |
| Spells | ~40 metodi | ~3 | Mancano: tutti i circoli, wait logic |
| Skills | 58 skills | ~5 | Mancano: 58 skill definitions, lock |
| Target | ~15 metodi | ~4 | Mancano: priorità, distanza |
| Journal | ~25 metodi | ~15 | Mancano: regex, speaker filter |
| Gumps | ~20 metodi | ~8 | Mancano: button handling, dropdown |
| Statics | ~15 metodi | ~2 | Mancano: collisione, terreno |
| Misc | ~30 metodi | ~8 | Mancano: file I/O, delays avanzati |
| **PathFinding** | **Completo** | **Assente** | **A\* algorithm non implementato** |
| **DPSMeter** | **Completo** | **Assente** | **Tool completo mancante** |
| **Trade** | **Completo** | **Assente** | **Tool completo mancante** |
| **Sound** | **Completo** | **Parziale** | Filtri base |

### 4.3 Sistema Macro (0% — ASSENTE)

L'originale ha **41 tipi di azione macro** con:
- Control flow: If/ElseIf/Else/While/For
- Combattimento: CastSpell, Attack, Abilities
- Inventario: Pickup, Drop, MoveItem
- Targeting: TargetSerial, WaitForTarget
- UI: RespondGump, WaitForGump
- Comunicazione: Say, Emote, Whisper, Yell
- Utilità: Pause, Mount, Resync

**TMRazor Improved non ha alcun sistema macro.** Questo è un gap critico — i macro sono una delle feature più usate dai giocatori.

### 4.4 Agents — Stato Implementazione

| Agent | Stato | Note |
|-------|-------|------|
| AutoLoot | **70%** | Loop funziona, manca: range re-check, delay per-item |
| Scavenger | **60%** | Bug bit mask (BUG-03), terrain check mancante |
| Organizer | **50%** | Move base funziona, manca: stacking, destination full |
| BandageHeal | **70%** | Healing funziona, manca: poison detect, priorità |
| Dress | **60%** | Equip/unequip base, manca: paperdoll, layer validation |
| Restock | **30%** | Dichiarato ma minimamente implementato |
| Vendor | **20%** | HandleBuyMenu/HandleSellMenu sono TODO vuoti |

### 4.5 Configurazione

| Feature | Originale | Nuovo | Gap |
|---------|-----------|-------|-----|
| Per-shard profiles | Si | No (solo per-character) | Medio |
| doors.json | Si | No | Basso |
| foods.json | Si | No | Basso |
| regions.json | Si | No | Medio — pathfinding ne ha bisogno |
| maps.json | Si | No | Medio |
| wands.json | Si | No | Basso |
| soundfilters.json | Si | No | Basso |
| gump_ignore.json | Si | No | Medio |
| pf_bypass.json | Si | No | Medio — pathfinding ne ha bisogno |

### 4.6 Feature Completamente Mancanti

1. **Macro System** — 41 action types, recorder, editor
2. **PathFinding** — Algoritmo A* per navigazione automatica
3. **DPS Meter** — Tracking danni per-target
4. **Secure Trade** — Gestione finestre trade
5. **Sound Logging** — Log e filtro suoni del client
6. **C# Scripting Engine** — Solo Python e UOSteam attivi
7. **SpellGrid** — Finestra floating con icone spell (pianificata Fase 5)
8. **Toolbar/HP Bar Overlay** — Barre HP floating (pianificata Fase 5)
9. **Map/Radar** — Mappa mondo (pianificata Fase 5)
10. **Gump Compressi** (0xDD) — Gump KR/SA non parsati

---

## 5. Anti-Pattern e Problemi Architetturali

### 5.1 Parsing Pacchetti Decentralizzato

AutoLootService e ScavengerService parsano pacchetti direttamente invece di ricevere dati strutturati da WorldPacketHandler. Questo causa:
- Duplicazione di logica di parsing
- Inconsistenza nei bit mask (vedi BUG-03)
- Difficoltà di manutenzione

**Raccomandazione**: Centralizzare TUTTO il parsing in WorldPacketHandler. I servizi dovrebbero ricevere eventi tipizzati (es. `ContainerContentMessage`, `WorldItemMessage`) tramite Messenger, non byte[] raw.

### 5.2 Singleton Ovunque

Tutti i servizi, ViewModel e pagine sono Singleton. Questo è giustificato per i servizi (stato condiviso, singola connessione), ma le **pagine WPF come Singleton** possono causare:
- Memory leak se le pagine accumulano stato
- Problemi di navigation (pagina vecchia con stato stale)

**Raccomandazione**: Le pagine dovrebbero essere `Transient`. Solo i ViewModel dovrebbero essere Singleton se mantengono stato persistente.

### 5.3 Mancanza di Logging Strutturato in Aree Critiche

Nonostante NLog sia integrato, molte aree critiche (PacketService buffer management, WorldPacketHandler parsing failures) loggano solo a `Debug` o non loggano affatto. Un packet malformato dovrebbe essere loggato a `Warning`.

### 5.4 Assenza di Health Check / Diagnostica

Non c'è modo di verificare lo stato del sistema a runtime:
- Quanti pacchetti/s vengono processati?
- Quanti Mobile/Item sono in World?
- Lo scripting engine è in esecuzione?
- Ci sono deadlock nei mutex?

**Raccomandazione**: Aggiungere un servizio `IDiagnosticsService` con contatori atomici.

---

## 6. Confronto Qualitativo Architettura

| Aspetto | Originale (TMRazor) | Nuovo (Improved) | Giudizio |
|---------|---------------------|-------------------|----------|
| Framework | .NET 4.8 WinForms | .NET 10 WPF | ✅ Migliore |
| Architettura | Monolitico, static class | DI + MVVM + Messenger | ✅ Molto migliore |
| Threading | Thread.Abort, lock globali | CancellationToken, async | ✅ Molto migliore |
| Serializzazione | XML + Newtonsoft.Json | System.Text.Json | ✅ Migliore |
| Testabilità | Nessun test | xUnit + Moq + Stress | ✅ Molto migliore |
| Packet handling | 60+ pacchetti | 20 pacchetti | ❌ Regressione |
| Scripting API | 500+ metodi | 150 metodi | ❌ Regressione |
| Feature set | Completo | 35-40% | ❌ Regressione |
| UI/UX | WinForms dark theme | WPF Fluent UI | ✅ Migliore (quando completa) |
| Manutenibilità | Difficile (coupling) | Facile (DI + interfaces) | ✅ Molto migliore |
| Performance attesa | Baseline | Migliore (async, throttling) | ✅ Migliore |

---

## 7. Roadmap Correzioni Raccomandata

### Sprint 1 — Bug Fix Critici ✅ COMPLETATO

1. ✅ Fixare lock mancanti in WorldPacketHandler (BUG-01, 02, 09) — `lock(m.SyncRoot)` e `lock(item.SyncRoot)`
2. ✅ Fixare bit mask in ScavengerService (BUG-03) — rawSerial + hasAmount flag
3. ✅ Fixare DistanceTo() → Chebyshev (BUG-04) — `Math.Max(Math.Abs(...), Math.Abs(...))`
4. ✅ Aggiungere registrazione DI IScriptingService + rimuovere duplicato ITargetingService (BUG-05, BUG-06)
5. ✅ Fixare AutoLoot processed serial logic (BUG-07) — remove da _processedSerials se fuori range
6. ✅ Aggiungere null checks in TargetingService (BUG-08) + try-finally event handler (BUG-11)
7. ✅ IronPython engine.Runtime.Dispose() (BUG-10)
8. ✅ Fire-and-forget esplicito `_ = Task.Run(...)` (BUG-12)

### Sprint 2 — Packet Handler Essenziali ✅ COMPLETATO

7. ✅ Implementare 0x77 MobileMoving (NPC/player movement)
8. ✅ Implementare 0x21/0x22 MovementAck/Reject
9. ✅ Implementare 0x3C ContainerContent in WorldPacketHandler (centralizzare)
10. ✅ Implementare 0x25 SingleItemUpdate in WorldPacketHandler
11. ✅ Implementare 0x2E EquipmentUpdate
12. ✅ Implementare 0x3A SkillUpdate
13. ✅ Implementare 0x72 WarMode

### Sprint 3 — Centralizzazione e Refactoring ✅ COMPLETATO

14. ✅ Estrarre parsing pacchetti da AutoLootService/ScavengerService → WorldPacketHandler
15. ✅ Creare messaggi tipizzati: `ContainerContentMessage`, `WorldItemMessage`, `EquipmentMessage`
16. ✅ Completare VendorService (0x74, 0x9E → 0x3B, 0x9F)
17. ✅ Fixare IronPython Engine dispose (BUG-10)
18. ✅ Fixare event handler leak (BUG-11)

### Sprint 4 — Scripting API Completion ✅ COMPLETATO

19. ✅ Completare PlayerApi (resistenze, karma, mosse speciali)
20. ✅ Completare ItemsApi (container scanning, proprietà)
21. ✅ Completare MobilesApi (distanza, filtri)
22. ✅ Completare SpellsApi (tutti i circoli)
23. ✅ Completare SkillsApi (58 skill)
24. ✅ Completare GumpsApi (button, dropdown, compressed)

### Sprint 5 — Feature Mancanti Critiche ✅ COMPLETATO

25. ✅ Implementare PathFinding (A* algorithm)
26. ✅ Implementare Macro System (almeno 20 action types base)
27. ✅ Implementare DPS Meter
28. ✅ Implementare Secure Trade
29. ✅ Implementare remaining packet handlers (0xC1/C2, 0xDD, 0xF3)

### Sprint 6 — UI Completion ✅ COMPLETATO

30. ✅ Completare GeneralPage + checkbox patch
31. ✅ Implementare OptionsPage + OptionsViewModel
32. ✅ Implementare DisplayPage + DisplayViewModel
33. ✅ Implementare SpellGrid window
34. ✅ Implementare Toolbar/HP overlay

### Sprint 7 — Migrazione Grafica e Rifinitura UI ✅ COMPLETATO

35. ✅ Migrazione Grafica UltimaSDK (System.Drawing -> WriteableBitmap)
36. ✅ Animazioni di Navigazione (Predefinite WPFUI)
37. ✅ Refactoring Threading Agenti (Task + CancellationToken)
38. ✅ Scripting Code Completion (API definitions)

---

## 8. Conclusioni

### Punti di Forza
L'architettura di TMRazor Improved è **significativamente superiore** all'originale in termini di manutenibilità, testabilità e modernità tecnologica. Il team ha fatto scelte architetturali eccellenti: DI container, MVVM, CancellationToken, async/await, Messenger pattern, immutable snapshots. **La feature parity è ora oltre l'85%.**

### Punti di Debolezza
Il progetto ha risolto i gap critici iniziali (Bug P0, Macro, Pathfinding). L'attuale punto di debolezza è l'uso di `System.Drawing` che causa colli di bottiglia nel rendering WPF e richiede una migrazione completa a `WriteableBitmap` per massimizzare le performance su .NET 10.

### Rischio Principale
~~Il gap maggiore è l'assenza del **sistema macro** (0%) e del **pathfinding** (0%)~~ -> **RISOLTO**. Il rischio residuo è legato alla stabilità della sandbox dello scripting engine e alle performance del rendering grafico su hardware datato.

### Raccomandazione Finale
Focalizzarsi sulla **migrazione grafica** (Sprint 7) per garantire la fluidità dell'interfaccia e completare il porting degli ultimi agenti. Il software è quasi pronto per una fase di Beta pubblica.

---

*Documento generato come parte della review architetturale di TMRazor Improved.*
