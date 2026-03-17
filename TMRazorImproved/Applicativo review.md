# Applicativo Review — TMRazorImproved
**Data revisione:** 17 marzo 2026
**Revisore:** Architetto Software Senior
**Branch:** `claude/tmrazor-migration-review-DMs0o`
**Esito generale:** ⚠️ MIGRAZIONE PARZIALE — struttura solida, ma funzionalità critiche incomplete o non corrette

---

## 1. Panoramica dello Stato Attuale

### Cosa è stato fatto bene
- Architettura a 4 layer (Shared / Core / UI / Plugin) pulita e ben separata
- Stack tecnologico moderno: .NET 10, WPF + WPF-UI, MVVM + CommunityToolkit, DI con `IHost`
- Threading: eliminati tutti i `Thread.Abort()`, adottato pattern `CancellationToken` cooperativo
- Packet engine funzionante con shared-memory e `TMRazorPlugin` (classicuo plugin in net48)
- Suite di test presente (unit, mock, stress, fuzz)
- Localizzazione dichiarativa via `{loc:Loc}` e `.resx`
- Tutti i servizi agente (AutoLoot, Scavenger, Dress, Organizer, BandageHeal, Restock) hanno Service + ViewModel + View

### Cosa è ancora rotto, incompleto o mancante
Vedi sezioni 2–5. Le criticità si dividono in:
- **Blocchi di compilazione** (1): il codice non compila
- **Bug funzionali gravi** (19): script esistenti sono silenziosi o errati
- **Feature non migrate** (6 intere aree): Macro engine incompleto, gRPC, ScriptRecorder, AutoDoc, Gump Inspector, Build/Publish pipeline

---

## 2. Criticità Emerse dall'Analisi

> Le criticità contrassegnate con 🔴 bloccano il funzionamento degli script legacy.
> Quelle con 🟠 degradano silenziosamente il comportamento.
> Quelle con 🟡 sono incomplete ma non critiche per il core.

### 2.1 Errore di Compilazione (build rotta)
| ID | File | Problema |
|----|------|---------|
| **S1-01** 🔴 | `TargetApi.cs` righe 231 e 312 | Due metodi `TargetResource(uint, int)` con firma identica → CS0111. Il progetto non compila. |

### 2.2 Bug Funzionali Gravi (script legacy rotti)
| ID | File | Problema |
|----|------|---------|
| **S2-01** 🔴 | `TargetApi.cs` | `TargetExecuteRelative` ignora il parametro `offset`; mining script non funzionano |
| **S2-02** 🔴 | `TargetApi.cs` | `TargetResource` invia un target generico invece del pacchetto `TargetByResource` specializzato |
| **S2-03** 🔴 | `TargetApi.cs` | `WaitForTargetOrFizzle` non rileva mai il fizzle (`0x54`/`0x5c`); aspetta l'intero timeout |
| **S2-04** 🔴 | `TargetApi.cs` | `PromptGroundTarget` ritorna sempre `null` (le coordinate X/Y/Z non vengono catturate) |
| **S2-05** 🔴 | `TargetApi.cs` | `HasTarget()` manca il parametro stringa `targetFlag`; script `HasTarget("Harmful")` crashano |
| **S2-06** 🟠 | `TargetApi.cs` | `TargetType` cerca prima mobiles poi items: ordine invertito rispetto all'originale |
| **S2-07** 🟠 | `PlayerApi.cs` | `Area()` e `Zone()` hardcoded a `"Britannia"` |
| **S2-08** 🔴 | `TargetApi.cs` | `GetLastAttack()` ritorna sempre `0` |
| **S2-09** 🔴 | `TargetApi.cs` | `LastUsedObject()` ritorna sempre `0` invece di `LastObject` dal WorldService |
| **S2-10** 🔴 | `SpellsApi.cs` | `Cast` manca gli overload `(name, target, wait, waitAfter)` usati nella maggior parte degli script |
| **S2-11** 🟠 | `SpellsApi.cs` | `WaitCastComplete` fa polling a 50ms; nessuna rilevazione di fizzle né di paralisi |

### 2.3 Type Mismatch (script Python silenziosamente rotti)
| ID | File | Problema |
|----|------|---------|
| **S3-01** 🔴 | `TargetApi.cs` | `GetLast()` ritorna `uint` invece di `int`; il pattern `== -1` non funziona mai |
| **S3-02** 🔴 | `TargetApi.cs` | `PromptTarget()` ritorna `uint`; cancellazione non distinguibile da serial `0` |
| **S3-03** 🔴 | `ItemsApi.cs` | `Filter.OnGround` è `bool` invece di tristate `int` (-1/0/1) |
| **S3-04** 🔴 | `MobilesApi.cs` | `Filter.Notoriety` è singolo `int` invece di `List<byte> Notorieties` |

### 2.4 Proprietà Filter Mancanti
| ID | Filter | Proprietà mancanti |
|----|--------|-------------------|
| **S4-01** 🟠 | `Items.Filter` | `IsContainer`, `IsCorpse`, `IsDrop` (tristate), `RangeMin`, `RangeMax`, `Parent`, `ExcludeSerial` |
| **S4-02** 🟠 | `Mobiles.Filter` | `IsHuman`, `IsGhost`, `IsAlly`, `IsEnemy`, `IsNeutral` (tristate) |

### 2.5 Overload Mancanti
| ID | File | Problema |
|----|------|---------|
| **S5-01** 🔴 | `TargetApi.cs` | `TargetResource` ha solo 1 overload su 4; mancano quelli con `string resource_name` e `Item item` |
| **S5-02** 🟠 | `ItemsApi.cs` | `FindByID` 4° parametro è `bool recurse` invece di `int range` |

### 2.6 Differenze Comportamentali
| ID | File | Problema |
|----|------|---------|
| **S6-01** 🟠 | `SpellDefinitions.cs` | Risoluzione nome spell usa substring matching; l'originale usa Levenshtein. Abbreviazioni falliscono silenziosamente |
| **S6-02** 🟠 | `PlayerApi.cs` | Pattern `P?.Hits ?? 0` ritorna `0` quando disconnessi; script non distinguono "HP=0 perché morto" da "disconnesso" |
| **S6-04** 🟠 | `MiscApi.cs` | `SendToClient(string keys)` non interpreta la sintassi SendKeys (`{Enter}`, `^u`, ecc.) |
| **S6-05** 🟠 | `TargetApi.cs` | `SetLast` non invia il pacchetto di highlight visivo al client |
| **S6-06** 🔴 | `MiscApi.cs` | `AppendToFile`/`WriteFile`/`DeleteFile` privi di validazione path/estensione: regressione di sicurezza |
| **S6-07** 🟠 | `MiscApi.cs` | `ScriptStop` può fermare solo lo script corrente, non script "fratelli" |
| **S6-08** 🟠 | `MiscApi.cs` | `ScriptSuspend` / `ScriptResume` sono stub vuoti |
| **S6-09** 🟠 | `PlayerApi.cs` | `Pets` basato su notorietà invece di tracking lato server |
| **S6-10** 🟠 | `MiscApi.cs` | `Distance` usa algoritmo Chebyshev invece di Euclidea |

### 2.7 Agenti senza Wrapper Script
| ID | Problema |
|----|---------|
| **S7-01** 🔴 | `AutoLoot`, `Dress`, `Scavenger`, `Restock` esistono come servizi ma non hanno un wrapper Python/UOSteam. Chiamate `AutoLoot.Start()` in script Python danno `AttributeError`. `AgentApis.cs` già contiene `AutoLootApi`, `DressApi`, `ScavengerApi`, `RestockApi` ma **non sono registrati** in `ScriptingService` come variabili Python. |

### 2.8 Annotazioni Minori
| ID | Note |
|----|------|
| **S8-01** | `JournalEntry.Timestamp`: `long` ms vs `double` secondi Unix nell'originale — documentare |
| **S8-02** | `Gumps.WaitForGump(PythonList, int)`: verificare compatibilità IronPython auto-cast |
| **S8-03** | `Target.SetLast(wait=true)` ignora il parametro `wait` |
| **S8-04** | `NoRunStealthToggle` / `NoRunStealthStatus` stub vuoti |
| **S8-05** | Verificare che da Python si possa istanziare `Items.Filter()` come nested class |

---

## 3. Feature Non Migrate (Aree Mancanti)

| Area | Legacy | Improved | Stato |
|------|--------|----------|-------|
| **Macro engine completo** | 43 action types | ~18 commands testuale | ⚠️ Parziale |
| **gRPC Proto-Control server** | 4 file + .proto | Assente | ❌ Mancante |
| **Script Recorder** | `ScriptRecorderService` + 3 impl. | Assente | ❌ Mancante |
| **AutoDoc** | `AutoDoc.cs` (genera JSON da XML) | Assente | ❌ Mancante |
| **Gump Inspector completo** | `GumpInspector.cs` | TODO nel ViewModel | ⚠️ Stub |
| **Build & Publish pipeline** | N/A (framework) | Non configurata | ❌ Mancante |

### 3.1 Azioni Macro Mancanti
Le seguenti azioni del motore macro legacy **non sono implementate** nell'interprete testuale di `MacrosService`:

| Azione legacy | Comando equivalente | Priorità |
|--------------|-------------------|----------|
| `FlyAction` / `LandAction` | `FLY` / `LAND` | Media |
| `InvokeVirtueAction` | `INVOKEVIRTUE` | Media |
| `MoveItemAction` (drag A→B) | `MOVEITEM` | Alta |
| `PickupAction` (→backpack) | `PICKUP` | Alta |
| `DropAction` (→ground) | `DROP` | Alta |
| `PromptResponseAction` | `PROMPTRESPONSE` | Media |
| `QueryStringResponseAction` | `QUERYSTRING` | Bassa |
| `RemoveAliasAction` | `REMOVEALIAS` | Media |
| `SetAliasAction` | `SETALIAS` | Media |
| `RenameMobileAction` | `RENAMEMOBILE` | Bassa |
| `RunOrganizerOnceAction` | `RUNORGANIZER` | Media |
| `UseContextMenuAction` | `USECONTEXTMENU` | Alta |
| `UseEmoteAction` | `EMOTE` | Bassa |
| `UsePotionAction` | `USEPOTION` | Media |
| `WaitForGumpAction` | `WAITFORGUMP` | Alta |
| `ArmDisarmAction` | `ARM` / `DISARM` | Media |
| `BandageAction` (in macro ctx) | `BANDAGE` | Alta |
| `TargetResource` (in macro ctx) | `TARGETRESOURCE` | Alta |
| `ToggleWarModeAction` | `WARMODE` | Alta |
| `DisconnectAction` | `DISCONNECT` | Media |
| `ResyncAction` | `RESYNC` | Media |
| `ClearJournalAction` | `CLEARJOURNAL` | Media |

---

## 4. Task List Completa — Cosa Fare per la Migrazione al 100%

---

### TASK-01 — Correggere l'Errore di Compilazione (S1-01)
**Priorità:** 🔴 CRITICA — senza questo fix niente compila
**File:** `TMRazorImproved.Core/Services/Scripting/Api/TargetApi.cs`
**Tempo stimato:** 15 minuti

**Sottotask:**

#### TASK-01.1 — Rimuovere il metodo duplicato
1. Aprire `TMRazorImproved.Core/Services/Scripting/Api/TargetApi.cs`
2. Cercare "TargetResource" nel file (ci sono due metodi con la stessa firma `public virtual void TargetResource(uint item_serial, int resource_number)`)
3. Cancellare **interamente** il secondo blocco (quello vuoto, riga ~312), mantenendo solo il primo
4. Compilare (`dotnet build`) e verificare che non ci siano errori CS0111

---

### TASK-02 — Fix del Sistema di Targeting (S2-01 / S2-02 / S2-03 / S2-04 / S2-05 / S2-08 / S2-09 / S3-01 / S3-02 / S5-01 / S6-05 / S8-03)
**Priorità:** 🔴 CRITICA
**File:** `TMRazorImproved.Core/Services/Scripting/Api/TargetApi.cs` e `TMRazorImproved.Shared/Interfaces/ITargetingService.cs`
**Tempo stimato:** 3–4 ore

**Sottotask:**

#### TASK-02.1 — Aggiungere `LastAttack` e `LastObject` al WorldService
1. Aprire `TMRazorImproved.Shared/Interfaces/IWorldService.cs`
2. Aggiungere le proprietà:
   ```csharp
   uint LastAttackSerial { get; }
   uint LastUsedObjectSerial { get; }
   ```
3. Aprire `TMRazorImproved.Core/Services/WorldService.cs`
4. Aggiungere le implementazioni con i relativi backing field (`private uint _lastAttack`, `private uint _lastUsed`)
5. Aprire `TMRazorImproved.Core/Handlers/WorldPacketHandler.cs`
6. Nel handler del pacchetto `0xAA` (AttackOK) → assegnare `_worldService.LastAttackSerial`
7. Nel handler del pacchetto `0x08` (DoubleClick inviato dal client) → assegnare `_worldService.LastUsedObjectSerial`

#### TASK-02.2 — Correggere `GetLastAttack` e `LastUsedObject` in TargetApi
1. Aprire `TargetApi.cs`
2. Trovare il metodo `GetLastAttack()` (attualmente `return 0`)
3. Cambiarlo in: `return (int)_worldService.LastAttackSerial;`
4. Trovare il metodo `LastUsedObject()` (attualmente `return 0`)
5. Cambiarlo in: `return (int)_worldService.LastUsedObjectSerial;`
   - Se il valore è `0`, restituire `-1` (semantica "non disponibile" dell'originale)

#### TASK-02.3 — Correggere il tipo di ritorno di `GetLast()` e `PromptTarget()`
1. Trovare `GetLast()` in `TargetApi.cs`
2. Cambiare il tipo di ritorno da `uint` a `int`
3. Aggiungere: se `LastTarget == 0`, restituire `-1`
4. Trovare `PromptTarget(...)` in `TargetApi.cs`
5. Cambiare il tipo di ritorno da `uint` a `int`
6. Se l'utente cancella il prompt (es. preme ESC), restituire `-1` invece di `0`

#### TASK-02.4 — Aggiungere il parametro `targetFlag` a `HasTarget`
1. Trovare `HasTarget()` in `TargetApi.cs`
2. Aggiungere il parametro opzionale: `public virtual bool HasTarget(string targetFlag = "Any")`
3. Logica: se `targetFlag == "Any"`, restituire come prima; altrimenti controllare il tipo di cursore target (`Beneficial`, `Harmful`, `Neutral`) che deve essere salvato nel `ITargetingService` quando arriva il pacchetto `0x6C` dal server

#### TASK-02.5 — Aggiungere cattura coordinate X/Y/Z per ground target
1. Aprire `TMRazorImproved.Shared/Interfaces/ITargetingService.cs`
2. Aggiungere: `TargetInfo? LastGroundTarget { get; }` dove `TargetInfo` include `X`, `Y`, `Z`, `Serial`
3. Aprire `TMRazorImproved.Core/Handlers/WorldPacketHandler.cs`
4. Nel handler del pacchetto `0x6C` (client → server, TargetCursor), quando `Serial == 0` (click a terra), leggere X/Y/Z dal pacchetto e salvarli in `ITargetingService.LastGroundTarget`
5. Aprire `TargetApi.cs` → metodo `PromptGroundTarget`
6. Usare `ITargetingService.LastGroundTarget` per restituire le coordinate corrette invece di `null`

#### TASK-02.6 — Implementare `TargetExecuteRelative` con offset direzionale
1. Aprire `TargetApi.cs` → metodo `TargetExecuteRelative(uint serial, int offset)`
2. Leggere la posizione dell'entità target dal `IWorldService`
3. Leggere la direzione dell'entità (property `Direction` nel modello `Mobile`)
4. Calcolare la posizione risultante applicando l'offset nelle 8 direzioni (N/S/E/W + diagonali):
   ```
   North:     (X,   Y-1, Z)
   South:     (X,   Y+1, Z)
   East:      (X+1, Y,   Z)
   West:      (X-1, Y,   Z)
   NorthEast: (X+1, Y-1, Z)
   NorthWest: (X-1, Y-1, Z)
   SouthEast: (X+1, Y+1, Z)
   SouthWest: (X-1, Y+1, Z)
   Up:        (X,   Y,   Z+1)
   Down:      (X,   Y,   Z-1)
   ```
5. Inviare il target di terra sulla posizione calcolata usando `PacketBuilder.BuildTarget(x, y, z)`

#### TASK-02.7 — Implementare `TargetResource` con il pacchetto corretto + tutti gli overload
1. Cercare nel legacy `Target.cs` come viene costruito il pacchetto `TargetByResource` (è un pacchetto `0x6C` con campo resource_number)
2. Aggiungere in `PacketBuilder.cs` il metodo `BuildTargetByResource(uint serial, int resourceNumber)`
3. In `TargetApi.cs` implementare i 4 overload:
   - `TargetResource(uint serial, int resourceNumber)` → usa `BuildTargetByResource`
   - `TargetResource(uint serial, string resourceName)` → mappa il nome al numero (es. `"ore"→0`, `"sand"→1`, `"wood"→2`, `"graves"→3`, `"red_mushroom"→4`) poi chiama il primo overload
   - `TargetResource(Item item, int resourceNumber)` → estrae `item.Serial` e chiama il primo overload
   - `TargetResource(Item item, string resourceName)` → mappa e chiama il secondo overload

#### TASK-02.8 — Implementare l'highlight visivo in `SetLast`
1. In `TargetApi.cs` → metodo `SetLast(int serial, bool wait)`
2. Prima di aggiornare `_targeting.LastTarget`, inviare via `_packetService` il pacchetto di highlight (packet `0x6C` formato targeting che evidenzia l'entità)
3. Se `wait == true`, attendere la conferma dal server prima di ritornare (usare `WaitForTarget` internamente)

---

### TASK-03 — Fix del Sistema Spell (S2-10 / S2-11 / S6-01)
**Priorità:** 🔴 CRITICA
**File:** `TMRazorImproved.Core/Services/Scripting/Api/SpellsApi.cs`, `SpellDefinitions.cs`
**Tempo stimato:** 2 ore

#### TASK-03.1 — Aggiungere gli overload di `Cast` con target e wait
1. Aprire `SpellsApi.cs`
2. Aggiungere gli overload:
   ```csharp
   public virtual bool Cast(string spellName, uint target, bool wait = true, int waitAfter = 0)
   public virtual bool Cast(string spellName, Mobile mobile, bool wait = true, int waitAfter = 0)
   ```
3. Implementare l'overload completo:
   - Inviare il cast come fa il metodo `Cast(string)` esistente
   - Se `wait == true`, chiamare `WaitForTarget(5000)`
   - Inviare il target con `_targetApi.TargetExecute(target)`
   - Se `waitAfter > 0`, chiamare `Thread.Sleep(waitAfter)` (o `Task.Delay` con CancellationToken)

#### TASK-03.2 — Sostituire il polling di `WaitCastComplete` con event-driven
1. In `SpellsApi.cs` trovare `WaitCastComplete` (o `WaitCast`)
2. Creare un `ManualResetEventSlim _fizzleEvent` a livello di classe
3. In `PacketService` (o `WorldPacketHandler`), registrare un handler sul pacchetto `0x54` (PlaySound)
4. Quando il sound ID è `0x5c` (fizzle sound), segnalare `_fizzleEvent.Set()`
5. In `WaitCastComplete`, usare `_fizzleEvent.Wait(timeout)` invece del ciclo `while + Sleep(50)`
6. Resettare `_fizzleEvent` all'inizio di ogni attesa

#### TASK-03.3 — Aggiungere fallback Levenshtein in `TryGetSpellId`
1. Aprire `TMRazorImproved.Shared/Models/SpellDefinitions.cs`
2. Trovare il metodo `TryGetSpellId`
3. Dopo il fallimento della ricerca per substring, aggiungere un terzo step:
   - Calcolare la distanza di Levenshtein tra il nome cercato e ogni spell nella lista
   - Restituire la spell con distanza minima (solo se la distanza è ≤ 3 caratteri, per evitare false match)
4. Aggiungere il metodo helper `LevenshteinDistance(string a, string b)` (algoritmo standard DP)

---

### TASK-04 — Fix dei Filter (S3-03 / S3-04 / S4-01 / S4-02 / S5-02)
**Priorità:** 🔴 CRITICA
**File:** `TMRazorImproved.Core/Services/Scripting/Api/ItemsApi.cs`, `MobilesApi.cs`
**Tempo stimato:** 2–3 ore

#### TASK-04.1 — Correggere `Items.Filter.OnGround` da `bool` a tristate `int`
1. Aprire `ItemsApi.cs` → classe interna `Filter`
2. Trovare la proprietà `OnGround` (attualmente `bool`)
3. Cambiarla in `public int OnGround { get; set; } = 0;` (0=qualsiasi, 1=solo a terra, -1=solo in container)
4. Nel metodo `ApplyFilter`, aggiornare la logica:
   ```csharp
   if (filter.OnGround == 1)  → includi solo item con Z accessibile e non in container
   if (filter.OnGround == -1) → includi solo item in container
   ```

#### TASK-04.2 — Correggere `Mobiles.Filter.Notoriety` da singolo a lista
1. Aprire `MobilesApi.cs` → classe interna `Filter`
2. Trovare la proprietà `Notoriety` (attualmente `int`)
3. Rinominarla in `Notorieties` e cambiarla in `public List<byte> Notorieties { get; } = new();`
4. Nel metodo `ApplyFilter`, aggiornare la logica:
   ```csharp
   if (filter.Notorieties.Any() && !filter.Notorieties.Contains(mobile.Notoriety)) → escludi
   ```

#### TASK-04.3 — Aggiungere le 7 proprietà mancanti in `Items.Filter`
1. Nella classe `Filter` di `ItemsApi.cs`, aggiungere:
   ```csharp
   public int IsContainer { get; set; } = 0;   // tristate: 1=solo container, -1=no container
   public int IsCorpse    { get; set; } = 0;   // tristate: 1=solo corpse
   public int IsDrop      { get; set; } = 0;   // tristate: 1=solo droppable
   public int RangeMin    { get; set; } = -1;  // distanza minima (-1=nessun limite)
   public int RangeMax    { get; set; } = -1;  // distanza massima (-1=nessun limite)
   public int Parent      { get; set; } = -1;  // serial del container padre (-1=qualsiasi)
   public int ExcludeSerial { get; set; } = -1; // serial da escludere (-1=nessuno)
   ```
2. Nel metodo `ApplyFilter`, implementare i controlli corrispondenti per ogni nuova proprietà

#### TASK-04.4 — Aggiungere le 5 proprietà mancanti in `Mobiles.Filter`
1. Nella classe `Filter` di `MobilesApi.cs`, aggiungere:
   ```csharp
   public int IsHuman   { get; set; } = 0;  // tristate
   public int IsGhost   { get; set; } = 0;  // tristate
   public int IsAlly    { get; set; } = 0;  // tristate (in party/guild)
   public int IsEnemy   { get; set; } = 0;  // tristate
   public int IsNeutral { get; set; } = 0;  // tristate
   ```
2. Nel metodo `ApplyFilter`, implementare i controlli:
   - `IsHuman`: controllare se il graphic ID del mobile corrisponde a una body human (0x190, 0x191, ecc.)
   - `IsGhost`: controllare se il graphic è 0x192/0x193 o se la proprietà `IsGhost` del modello `Mobile` è vera
   - `IsAlly`/`IsEnemy`/`IsNeutral`: confrontare con la `FriendsList` e la `Notoriety`

#### TASK-04.5 — Correggere la firma di `FindByID` (4° parametro)
1. In `ItemsApi.cs` trovare il metodo `FindByID`
2. Aggiungere un overload che rispetti la firma originale:
   ```csharp
   public virtual Item? FindByID(int itemid, int color = -1, int container = -1, int range = -1)
   ```
3. Nell'implementazione, se `range >= 0`, filtrare gli item per distanza dal player

---

### TASK-05 — Fix Differenze Comportamentali (S6-02 / S6-04 / S6-06 / S6-07 / S6-08 / S6-10)
**Priorità:** 🟠 ALTA
**Tempo stimato:** 2 ore

#### TASK-05.1 — Aggiungere `Player.IsValid` come guard esplicito (S6-02)
1. Aprire `PlayerApi.cs`
2. Verificare che esista `public virtual bool IsConnected` (dovrebbe esserci già)
3. Aggiungere anche `public virtual bool IsValid => _world.Player != null;`
4. Aggiungere un commento XML che documenta chiaramente: "tutte le proprietà numeriche restituiscono 0 quando il player non è connesso"

#### TASK-05.2 — Implementare parser SendKeys in `Misc.SendToClient` (S6-04)
1. Aprire `MiscApi.cs` → metodo `SendToClient(string keys)`
2. Aggiungere un parser che traduca le sequenze speciali:
   - `{Enter}` → tasto VK_RETURN (WM_KEYDOWN + WM_KEYUP)
   - `{Tab}` → VK_TAB
   - `{Esc}` → VK_ESCAPE
   - `{Back}` → VK_BACK
   - `{Delete}` → VK_DELETE
   - `^` + char → CTRL + char (es. `^u` → Ctrl+U)
   - `%` + char → ALT + char
   - `+` + char → SHIFT + char
3. Inviare `PostMessage(hWnd, WM_KEYDOWN, vk, 0)` invece di WM_CHAR per queste sequenze

#### TASK-05.3 — Aggiungere validazione path in operazioni file (S6-06)
1. Aprire `MiscApi.cs`
2. Il metodo privato `IsValidPath` dovrebbe esistere già; verificarne il contenuto
3. Assicurarsi che sia applicato a `AppendToFile`, `WriteFile`, `ReadFile`, `DeleteFile`
4. La whitelist deve includere solo: `Scripts/`, `Data/`, `Profiles/`, `Config/`, `Logs/` (relative a `AppContext.BaseDirectory`)
5. Aggiungere una whitelist di estensioni permesse: `.txt`, `.data`, `.xml`, `.map`, `.csv`, `.json`, `.log`

#### TASK-05.4 — Implementare registry multi-script e ScriptStop per nome (S6-07)
1. Aprire `TMRazorImproved.Shared/Interfaces/IScriptingService.cs`
2. Aggiungere:
   ```csharp
   IReadOnlyDictionary<string, CancellationTokenSource> RunningScripts { get; }
   void StopScript(string scriptName);
   ```
3. Aprire `TMRazorImproved.Core/Services/Scripting/ScriptingService.cs`
4. Aggiungere un `Dictionary<string, CancellationTokenSource>` che mappa nome script → CTS
5. Registrare ogni script all'avvio e rimuoverlo al termine
6. Implementare `StopScript(name)` che chiama `Cancel()` sulla CTS corrispondente
7. In `MiscApi.cs` → `ScriptStop(string name)`, usare `_scripting.StopScript(name)` invece del controllo sul nome corrente

#### TASK-05.5 — Implementare ScriptSuspend / ScriptResume (S6-08)
1. In `IScriptingService.cs`, aggiungere:
   ```csharp
   void SuspendScript(string scriptName);
   void ResumeScript(string scriptName);
   bool IsScriptSuspended(string scriptName);
   ```
2. In `ScriptingService.cs`, aggiungere un `Dictionary<string, ManualResetEventSlim>` per gli script sospesi
3. In `SuspendScript`: creare un evento non-segnalato per lo script e mettere il thread in attesa
4. In `ResumeScript`: segnalare l'evento → il thread si sblocca
5. Nel loop di esecuzione Python, aggiungere un check cooperativo ogni N istruzioni: `_suspendEvents.TryGetValue(name, out var e); e?.Wait(token);`
6. In `MiscApi.cs` → collegare `ScriptSuspend`/`ScriptResume`/`ScriptIsSuspended` al servizio

#### TASK-05.6 — Correggere `Misc.Distance` da Chebyshev a Euclidea (S6-10)
1. Aprire `MiscApi.cs` → metodo `Distance(int x1, int y1, int x2, int y2)`
2. Cambiare da:
   ```csharp
   return Math.Max(Math.Abs(x2-x1), Math.Abs(y2-y1));
   ```
   a:
   ```csharp
   int dx = x2 - x1, dy = y2 - y1;
   return (int)Math.Round(Math.Sqrt(dx * dx + dy * dy));
   ```
   > **Nota:** Verificare prima nel codice legacy `Utility.cs` che `Utility.Distance` usi effettivamente Euclidea e non Chebyshev, per confermare la correzione.

---

### TASK-06 — Registrare i Wrapper Agenti in ScriptingService (S7-01)
**Priorità:** 🔴 CRITICA
**File:** `TMRazorImproved.Core/Services/Scripting/ScriptingService.cs`, `AgentApis.cs`
**Tempo stimato:** 1 ora

#### TASK-06.1 — Verificare che tutti i wrapper esistano in AgentApis.cs
1. Aprire `TMRazorImproved.Core/Services/Scripting/Api/AgentApis.cs`
2. Verificare che esistano le classi: `AutoLootApi`, `DressApi`, `ScavengerApi`, `RestockApi`
3. Verificare che ogni classe abbia i metodi: `Start()`, `Stop()`, `ChangeList(string name)`, `Status()`

#### TASK-06.2 — Registrare i wrapper come variabili Python
1. Aprire `TMRazorImproved.Core/Services/Scripting/ScriptingService.cs`
2. Trovare dove vengono registrate le variabili nel scope Python (es. `scope.SetVariable("Player", playerApi)`)
3. Aggiungere le registrazioni mancanti:
   ```csharp
   scope.SetVariable("AutoLoot",  _autoLootApi);
   scope.SetVariable("Dress",     _dressApi);
   scope.SetVariable("Scavenger", _scavengerApi);
   scope.SetVariable("Restock",   _restockApi);
   ```
4. Fare lo stesso per l'UOSteam interpreter (verificare che riceva i wrapper nel costruttore — già presenti in `UOSteamInterpreter.cs`)

---

### TASK-07 — Completare il Macro Engine (Azioni Mancanti)
**Priorità:** 🟠 ALTA
**File:** `TMRazorImproved.Core/Services/MacrosService.cs`
**Tempo stimato:** 4–6 ore

Ogni sottotask aggiunge uno o più comandi all'interprete testuale in `ExecuteAction()` dentro `MacrosService.cs`. Il pattern generale è:
- Aggiungere un `case "NOMECMD":` nel grande switch
- Estrarre i parametri da `args`
- Eseguire l'azione via i servizi iniettati (IPacketService, IWorldService, ecc.)
- Registrare anche il comando nel `RecordAction()` per la funzione di registrazione

#### TASK-07.1 — Comandi drag-n-drop (MOVEITEM, PICKUP, DROP)
**Cosa fanno:**
- `MOVEITEM <serial> <destContainer> [amount]` — sposta un item da un container a un altro
- `PICKUP <serial> [amount]` — raccoglie un item nel backpack del player
- `DROP <serial> <x> <y> <z>` — lascia cadere un item a terra

**Come implementarli:**
1. `MOVEITEM`: inviare pacchetto `0x07` (PickupItem) poi `0x08` (DropItem) con serial destinazione
2. `PICKUP`: inviare `0x07` (PickupItem) con serial del backpack come destinazione
3. `DROP`: inviare `0x07` + `0x08` con coordinate X/Y/Z e container serial `0xFFFFFFFF`

#### TASK-07.2 — Comandi warmode e combat (WARMODE, ATTACK)
> `ATTACK` è già presente. Aggiungere `WARMODE`.

1. Aggiungere `case "WARMODE":` con argomento `"on"`/`"off"`/`"toggle"`
2. Inviare pacchetto `0x72` con flag appropriato

#### TASK-07.3 — Comandi sessione (DISCONNECT, RESYNC, CLEARJOURNAL)
1. `DISCONNECT`: chiudere la connessione via `IClientInteropService.Disconnect()`
2. `RESYNC`: inviare il pacchetto di resync `0x22`
3. `CLEARJOURNAL`: chiamare `IJournalService.Clear()`

#### TASK-07.4 — Comandi context menu e gump (USECONTEXTMENU, WAITFORGUMP)
1. `USECONTEXTMENU <serial> <entryIndex>` — inviare pacchetto `0xBF.0x13` (context menu request) poi `0xBF.0x15` (context menu response)
2. `WAITFORGUMP <serial> <timeout>` — attendere che arrivi il pacchetto `0xB0` (SendGump) con il serial specificato; usare `ManualResetEventSlim` event-driven (non polling)

#### TASK-07.5 — Comandi skill avanzati (USEPOTIONTYPE, BANDAGE, ARMDISARM)
1. `USEPOTIONTYPE <potionName>` — trovare nel backpack un item che corrisponda al nome pozione, usarlo con double-click
2. `BANDAGE [target_serial]` — trovare una benda nel backpack, usarla sul target; se target omesso, usare sul player
3. `ARM <layer>` / `DISARM <layer>` — equipaggiare/rimuovere l'item nel layer specificato (usa `IPacketService` per pacchetti `0x13`/`0x07`)

#### TASK-07.6 — Comandi alias (SETALIAS, REMOVEALIAS)
> Probabilmente già presenti nell'UOSteam interpreter — verificare che funzionino anche nel motore macro.

1. `SETALIAS <name> <serial>` — salvare nella dictionary degli alias (`_macroAliases`)
2. `REMOVEALIAS <name>` — rimuovere l'alias
3. Aggiornare la risoluzione dei serial in tutti gli altri comandi per leggere dagli alias (es. `TARGET lasttarget` deve risolvere il serial corretto)

#### TASK-07.7 — Comandi volo, virtù, emote e rename (minore priorità)
1. `FLY` / `LAND` — toggle volo via pacchetto `0xBF.0x32`
2. `INVOKEVIRTUE <virtueName>` — mappare nome→ID (Honor=0x01, Sacrifice=0x02, Valor=0x04) e inviare `0x12` con type `0x69`
3. `EMOTE <text>` — inviare chat con tipo emote (`0xAD` con type `0x03`)
4. `RENAMEMOBILE <serial> <newName>` — inviare pacchetto `0x75`
5. `RUNORGANIZER` — chiamare `IOrganizerService.RunOnce()`

---

### TASK-08 — Implementare il Script Recorder
**Priorità:** 🟡 MEDIA
**Cosa manca:** Il legacy ha un `ScriptRecorderService` che intercetta le azioni dell'utente (click, target, cast, skill) e genera automaticamente il codice corrispondente in Python, C# o UOSteam.
**Tempo stimato:** 6–8 ore

#### TASK-08.1 — Creare l'interfaccia e il servizio base
1. In `TMRazorImproved.Shared/Interfaces/`, creare `IScriptRecorderService.cs`:
   ```csharp
   public interface IScriptRecorderService
   {
       bool IsRecording { get; }
       void StartRecording(ScriptLanguage language);
       string StopRecording();  // restituisce il codice generato
   }
   ```
2. In `TMRazorImproved.Core/Services/`, creare `ScriptRecorderService.cs`
3. Registrarlo nel DI container in `App.xaml.cs`

#### TASK-08.2 — Intercettare le azioni e generare codice
1. In `ScriptRecorderService`, sottoscrivere agli eventi di `IPacketService` per i pacchetti client-side:
   - `0x06` (SingleClick) → genera `Items.SingleClick(0x{serial})`
   - `0x08` (DoubleClick) → genera `Items.UseItem(0x{serial})` o `Misc.UseSkill(...)` se è un'abilità
   - `0x12` (Action: say) → genera `Player.ChatMessage("{text}")`
   - `0x6C` (TargetResponse) → genera `Target.TargetExecute(0x{serial})`
   - `0xBF` (cast spell via `0x12` + type `0x56`) → genera `Spells.Cast("{spellName}")`
2. Implementare generatori separati per ogni linguaggio (Python, C#, UOSteam)

#### TASK-08.3 — Collegare il recorder alla UI di scripting
1. In `ScriptingViewModel.cs`, aggiungere un comando `RecordCommand` (pulsante "Registra")
2. In `ScriptingPage.xaml`, aggiungere un pulsante Record/Stop accanto ai pulsanti Play/Stop esistenti
3. Quando si clicca "Stop Recording", il codice generato viene inserito nell'editor AvalonEdit

---

### TASK-09 — Implementare il gRPC Proto-Control Server
**Priorità:** 🟡 MEDIA
**Cosa manca:** Il legacy espone un server gRPC su porta configurabile che permette a script esterni (Python, ecc.) di controllare TMRazor remotamente.
**Tempo stimato:** 4–6 ore

#### TASK-09.1 — Aggiungere le dipendenze gRPC al progetto Core
1. Aprire `TMRazorImproved.Core/TMRazorImproved.Core.csproj`
2. Aggiungere i pacchetti:
   ```xml
   <PackageReference Include="Google.Protobuf" Version="3.29.3" />
   <PackageReference Include="Grpc.Core" Version="2.46.6" />
   <PackageReference Include="Grpc.Tools" Version="2.70.0" />
   ```
3. Copiare il file `ProtoControl.proto` dal legacy in `TMRazorImproved.Core/Proto/`

#### TASK-09.2 — Generare il codice C# dal .proto
1. Aggiungere nel `.csproj`:
   ```xml
   <Protobuf Include="Proto/ProtoControl.proto" GrpcServices="Server" />
   ```
2. Compilare → il codice stub viene generato automaticamente

#### TASK-09.3 — Implementare il server gRPC
1. Creare `TMRazorImproved.Core/Services/ProtoControlService.cs`
2. Implementare la classe derivata dal server stub generato
3. Esporre i metodi RPC principali: `ExecuteScript`, `GetPlayerStatus`, `GetItems`, `SendPacket`
4. Avviare il server in `App.xaml.cs` come `IHostedService` su porta configurabile (default 50051)

---

### TASK-10 — Completare il Gump Inspector
**Priorità:** 🟡 MEDIA
**File:** `TMRazorImproved.UI/ViewModels/GumpListViewModel.cs`
**Tempo stimato:** 2–3 ore

#### TASK-10.1 — Aprire Gump Inspector dal ViewModel
1. Aprire `GumpListViewModel.cs`
2. Trovare il commento `// TODO: Apri Gump Inspector con questo gump`
3. Creare un comando `InspectGumpCommand` che apre una nuova finestra `GumpInspectorWindow`

#### TASK-10.2 — Creare la finestra Gump Inspector
1. Creare `TMRazorImproved.UI/Views/Windows/GumpInspectorWindow.xaml` e `.cs`
2. Creare `TMRazorImproved.UI/ViewModels/GumpInspectorViewModel.cs`
3. La finestra deve mostrare:
   - Il Gump ID e Serial
   - Lista dei controlli del Gump (Label, Button, Checkbox, RadioButton, TextEntry)
   - Per ogni controllo: tipo, testo, posizione X/Y, dimensioni W/H
   - Una sezione "Interazione" che permette di cliccare button o inviare testo tramite UI
4. Collegare i dati a `IWorldService.GetGump(serial)`

---

### TASK-11 — Completare la Pipeline di Build e Publish
**Priorità:** 🟡 MEDIA
**Tempo stimato:** 2–3 ore

#### TASK-11.1 — Configurare la publish Single-File per .NET 10
1. Aprire `TMRazorImproved.UI/TMRazorImproved.UI.csproj`
2. Aggiungere nella `<PropertyGroup>`:
   ```xml
   <PublishSingleFile>true</PublishSingleFile>
   <SelfContained>false</SelfContained>
   <RuntimeIdentifier>win-x86</RuntimeIdentifier>
   <PublishReadyToRun>true</PublishReadyToRun>
   ```
3. Creare uno script `publish.ps1` nella root della solution:
   ```powershell
   dotnet publish TMRazorImproved/TMRazorImproved.UI/TMRazorImproved.UI.csproj `
     -c Release -r win-x86 --self-contained false `
     -o ./publish/TMRazorImproved
   ```

#### TASK-11.2 — Configurare GitHub Actions per CI/CD
1. Creare `.github/workflows/build.yml`
2. Il workflow deve:
   - Triggerare su `push` a `main` e su ogni PR
   - Eseguire `dotnet build` per verificare che compili
   - Eseguire `dotnet test` per far girare tutta la suite di test
   - (Opzionale) Creare un Release con l'artefatto `.zip` contenente il publish

#### TASK-11.3 — Test di stabilità memoria
1. Aggiungere in `TMRazorImproved.Tests/MockTests/Stress/` un test `MemoryLeakTest.cs`
2. Il test deve:
   - Avviare e fermare il `PacketService` 100 volte
   - Misurare `GC.GetTotalMemory(true)` prima e dopo
   - Fallire se la memoria cresce di più del 10%
3. Fare lo stesso per `ScriptingService` (avvio/stop di 50 script brevi)

---

### TASK-12 — Implementare AutoDoc (generazione documentazione API)
**Priorità:** 🟢 BASSA
**Cosa manca:** Il legacy ha `AutoDoc.cs` che legge i commenti XML dai file delle API e genera un file `api.json` usabile per la documentazione e il completamento automatico.
**Tempo stimato:** 3 ore

#### TASK-12.1 — Creare il generatore di documentazione
1. Creare `TMRazorImproved.Core/Utilities/ApiDocGenerator.cs`
2. Il generatore deve:
   - Usare reflection per enumerare tutte le classi di API (`PlayerApi`, `ItemsApi`, ecc.)
   - Per ogni metodo pubblico, estrarre il commento XML con `System.Xml.XPath`
   - Generare un file `api.json` nella cartella di output
3. Il formato JSON deve essere compatibile con il `CompletionService.cs` già esistente in `TMRazorImproved.UI/Utilities/`

#### TASK-12.2 — Integrare il completamento automatico con i dati generati
1. Aprire `TMRazorImproved.UI/Utilities/CompletionService.cs`
2. Caricare il file `api.json` generato
3. Usare i dati per alimentare i suggerimenti di completamento nell'editor AvalonEdit

---

### TASK-13 — Miglioramenti Vari Post-Release (S6-09 / Player.Pets / S8-01)
**Priorità:** 🟢 BASSA
**Tempo stimato:** 2 ore

#### TASK-13.1 — Implementare `Player.Pets` via tracking server
1. In `IWorldService`, aggiungere `List<uint> PetSerials { get; }` (lista serial dei pet conosciuti)
2. Nel `WorldPacketHandler`, nel handler del pacchetto `0x11` (MobileStatus), se il mobile è un pet (flag bonding), aggiungerne il serial a `PetSerials`
3. In `PlayerApi.cs` → `Pets`, filtrare `IWorldService.GetMobileSnapshots()` usando `PetSerials` invece della notorietà

#### TASK-13.2 — Documentare la differenza di timestamp in JournalEntry (S8-01)
1. Aprire `TMRazorImproved.Shared/Models/JournalEntry.cs`
2. Aggiungere un commento XML su `Timestamp`:
   ```csharp
   /// <remarks>
   /// NOTA: in TMRazorImproved Timestamp è espresso in millisecondi Unix (long).
   /// Nell'originale TMRazor era in secondi Unix (double).
   /// Convertire con: timestampMs / 1000.0
   /// </remarks>
   ```

#### TASK-13.3 — Implementare `Player.Area()` e `Player.Zone()` (S2-07)
1. Creare un file `TMRazorImproved.Shared/Data/UORegions.cs` con una lista statica delle regioni UO principali:
   - Ogni regione ha: nome, mappa ID, rectangle bounds (X1,Y1,X2,Y2)
2. In `PlayerApi.cs` → `Area()`:
   - Leggere le coordinate del player da `IWorldService.Player`
   - Cercare nella lista quale regione contiene il player
   - Restituire il nome della regione trovata, o `"Unknown"` se fuori da tutte le regioni note
3. `Zone()` può restituire la regione di livello più specifico (sub-regione, es. "Britain Graveyard" vs "Britannia")

---

## 5. Riepilogo Finale per Priorità

### 🔴 Da fare subito (blocca build o script legacy)
| Task | Contenuto | Ore |
|------|-----------|-----|
| TASK-01 | Fix compilazione (duplicato TargetResource) | 0.25 |
| TASK-02 | Fix sistema targeting | 4 |
| TASK-03 | Fix sistema spell (Cast overload + WaitCast + Levenshtein) | 2 |
| TASK-04 | Fix Filter (tristate, proprietà mancanti, FindByID) | 3 |
| TASK-06 | Registrare wrapper agenti in ScriptingService | 1 |

**Totale lavoro critico: ~10.25 ore**

### 🟠 Alta priorità (degrado silenzioso o feature importanti)
| Task | Contenuto | Ore |
|------|-----------|-----|
| TASK-05 | Differenze comportamentali (SendKeys, path validation, ScriptStop, Suspend, Distance) | 2 |
| TASK-07 | Completare macro engine (21 azioni mancanti) | 6 |

**Totale lavoro alta priorità: ~8 ore**

### 🟡 Media priorità (feature non migrate)
| Task | Contenuto | Ore |
|------|-----------|-----|
| TASK-08 | Script Recorder | 7 |
| TASK-09 | gRPC Proto-Control server | 5 |
| TASK-10 | Gump Inspector completo | 3 |
| TASK-11 | Build/Publish pipeline | 3 |

**Totale lavoro media priorità: ~18 ore**

### 🟢 Bassa priorità (nice-to-have)
| Task | Contenuto | Ore |
|------|-----------|-----|
| TASK-12 | AutoDoc + completamento AvalonEdit | 3 |
| TASK-13 | Pet tracking, Region table, doc timestamp | 2 |

**Totale lavoro bassa priorità: ~5 ore**

---

## 6. Come Leggere e Usare Questo Documento

**Per un profilo junior:**
1. Inizia sempre da `TASK-01` — è il più piccolo (15 min) e sblocca tutto il resto
2. Per ogni sottotask, il file da modificare è indicato nella sezione del task padre
3. Dopo ogni modifica, esegui `dotnet build` dalla root della solution per verificare che il codice compili
4. Dopo ogni gruppo di fix, esegui `dotnet test` per verificare che i test passino
5. Se un test fallisce dopo la tua modifica, leggi il messaggio di errore prima di chiedere aiuto

**Struttura dei progetti (dove si trovano i file):**
```
TMRazorImproved/
├── TMRazorImproved.Shared/       ← Interfacce, modelli, enums (cambia qui prima)
│   ├── Interfaces/               ← IWorldService, ITargetingService, ecc.
│   ├── Models/                   ← UOEntity, Mobile, Item, Filter, ecc.
│   └── Messages/                 ← Messaggi IMessenger
├── TMRazorImproved.Core/         ← Logica di business (servizi, handler)
│   ├── Services/                 ← WorldService, PacketService, ScriptingService, ecc.
│   │   └── Scripting/Api/        ← PlayerApi, TargetApi, SpellsApi, ItemsApi, ecc.
│   └── Handlers/                 ← WorldPacketHandler (decodifica i pacchetti UO)
├── TMRazorImproved.UI/           ← Interfaccia WPF
│   ├── ViewModels/               ← I ViewModel (logica della UI)
│   └── Views/Pages/              ← Le pagine XAML
└── TMRazorImproved.Tests/        ← Test automatici
    ├── MockTests/                ← Test con mock (veloci)
    └── IntegrationTests/         ← Test di integrazione (più lenti)
```

---

*Documento generato dall'analisi del codice sorgente in data 17 marzo 2026.*
*Basato su: confronto diretto tra TMRazor legacy (Razor/) e TMRazorImproved/, `finalreview.md`, `TMRazorImprovedProgress.md`.*
