# Final Review — API Migration TMRazor → TMRazorImproved
**Data revisione:** 11 marzo 2026
**Revisore:** Architetto Software
**Esito:** ❌ MIGRAZIONE NON CONFORME — 33 criticità identificate

---

## Premessa

Il tool `Analyze-ApiMigration.ps1` controlla **la presenza** dei metodi, non la **correttezza** dell'implementazione. Questa revisione colma quel gap. Le criticità sono ordinate per gravità decrescente e riguardano il comportamento **identico** a TMRazor che ogni script esistente si aspetta.

---

## SEVERITY 1 — ERRORE DI COMPILAZIONE

### ✅ [S1-01] `TargetApi.cs` — Metodo duplicato `TargetResource(uint, int)` → CS0111
**File:** `TMRazorImproved.Core/Services/Scripting/Api/TargetApi.cs`
**Riga:** 231 e 312

Due metodi con firma identica `public virtual void TargetResource(uint item_serial, int resource_number)` coesistono nello stesso file. Uno invia un target normale (riga 231), l'altro è vuoto (riga 312). Il compilatore C# rifiuta il file con CS0111. Il codice non compila.

**Fix:** Eliminare il corpo duplicato alla riga 312. Implementare correttamente il metodo che rimane (vedi S2-02).

---

## SEVERITY 2 — COMPORTAMENTO FUNZIONALMENTE ERRATO

### ✅ [S2-01] `Target.TargetExecuteRelative` — Ignora completamente il parametro `offset`
**File:** `TargetApi.cs`, riga 304–310

**Originale:** Calcola la posizione relativa rispetto alla direzione del mobile (North/South/East/West/Up/Down/Left/Right), applica l'offset in quella direzione, controlla i `CaveTiles` per scegliere tile vs item, e invia il target sulla posizione risultante. Logica critica per mining e harvesting script.

**Migroto:** Ignora il parametro `offset`. Prende semplicemente X,Y,Z corrente dell'entità e la invia come target di terra. Un mining script con `Target.TargetExecuteRelative(miner, 2)` non funzionerà mai.

**Fix:** Reimplementare la logica di directional offset (8 direzioni + CaveTiles) come nell'originale `Target.cs` righe 224–288.

---

### ✅ [S2-02] `Target.TargetResource` — Invia il pacchetto sbagliato
**File:** `TargetApi.cs`, riga 231

**Originale:** Invia il pacchetto specializzato `TargetByResource` (pacchetto UO dedicato al resource targeting). Questo è il meccanismo con cui il client UO comunica al server che vuole fare mining/woodcutting su una specifica risorsa.

**Migrato:** Invia un normale `SendTarget(item_serial)` — un target generico sul serial dell'item. Il server ignora questo per il resource gathering. Nessuno script di harvesting funzionerà.

**Fix:** Implementare il pacchetto `TargetByResource` come nell'originale, che invia un messaggio di targeting con il resource number specifico.

---

### ✅ [S2-03] `Target.WaitForTargetOrFizzle` — Non rileva mai il fizzle
**File:** `TargetApi.cs`, riga 365

**Originale:** Registra un handler per il pacchetto `0x54` (PlaySound). Quando il server invia il suono di fizzle (sound ID `0x5c`), il metodo ritorna immediatamente. È l'unico modo affidabile per sapere se un incantesimo è stato interrotto senza attendere l'intero timeout.

**Migrato:** Chiama banalmente `WaitForTarget(delay)`. Se la spell fizzla, lo script aspetta l'intero timeout (default 5 secondi) prima di procedere. Tutti gli script di combattimento magico perderanno 5 secondi ad ogni fizzle.

**Fix:** Iscriversi al `PacketService.OnPacketReceived` per intercettare `0x54`, controllare sound ID `0x5c` e uscire anticipatamente.

---

### ✅ [S2-04] `Target.PromptGroundTarget` — Ritorna sempre `null` per click sul terreno
**File:** `TargetApi.cs`, riga 239–261

**Originale:** Usa `Targeting.OneTimeTarget(true, callback)` dove il callback riceve le coordinate X,Y,Z reali del click a terra dal client. Ritorna un `Point3D` valido.

**Migrato:** Usa `AcquireTargetAsync()` che non cattura coordinate X,Y,Z quando il serial è `0` (click su terra). Lo stesso commento nel codice lo ammette: *"Al momento TargetingService non cattura le coordinate X,Y,Z per serial 0"*. Il metodo ritorna sempre `null` per qualsiasi click a terra. Qualsiasi script che usi `PromptGroundTarget` per spell come Fire Field, Wall of Stone, ecc. è completamente rotto.

**Fix:** Estendere `ITargetingService` per catturare le coordinate X,Y,Z quando il serial del target è `0`. Aggiornare il `WorldPacketHandler` per estrarre queste coordinate dal pacchetto `0x6C` client-to-server.

---

### ✅ [S2-05] `Target.HasTarget(string targetFlag)` — Il parametro `targetFlag` non esiste
**File:** `TargetApi.cs`, riga 80

**Originale:** `bool HasTarget(string targetFlag = "Any")` — filtra per tipo di target cursor: `"Beneficial"`, `"Harmful"`, `"Neutral"`, `"Any"`. Usato nei combat script per distinguere heal target da attack target. Passare un flag non valido lancia `ArgumentOutOfRangeException`.

**Migrato:** `bool HasTarget()` — nessun parametro. Script che chiamano `Target.HasTarget("Harmful")` riceveranno `TypeError` in Python. Tutti i combat script che verificano il tipo di cursore sono rotti.

**Fix:** Aggiungere l'overload con parametro stringa. Richiedere che `ITargetingService` esponga il `TargetFlags` corrente (dal pacchetto `0x6C`).

---

### ✅ [S2-06] `Target.TargetType` — Ordine di ricerca invertito rispetto all'originale
**File:** `TargetApi.cs`, riga 314

**Originale:** Cerca prima **items nel backpack**, poi **items a terra** (con filter), infine **mobiles**. Questo garantisce che se uno stesso graphic ID appartiene sia a un item che a un mobile, l'item ha priorità.

**Migrato:** Cerca prima i **mobiles**, poi gli **items**. L'ordine è invertito. Script di targeting che sfruttano questa priorità (ad es. "targetta il minerale vicino, non il mostro) produrranno comportamenti errati.

**Fix:** Reimplementare nell'ordine corretto: backpack items → ground items → mobiles.

---

### ✅ [S2-07] `Player.Area()` e `Player.Zone()` — Hardcoded `"Britannia"`
**File:** `PlayerApi.cs`

**Originale:** Interroga i dati di mappa del client per restituire la regione/zona corrente del player (es. `"Britain"`, `"Minoc"`, `"Shame"`, ecc.).

**Migrato:** Entrambi ritornano sempre `"Britannia"` indipendentemente dalla posizione. Script che navigano in base alla zona o usano `Player.Zone()` per decidere comportamenti saranno completamente ciechi alla geografia.

**Fix:** Implementare una mappa delle regioni UO (Region table) o almeno leggere la RegionFlag dal `WorldService` se disponibile.

---

### ✅ [S2-08] `Target.GetLastAttack()` — Ritorna sempre `0`
**File:** `TargetApi.cs`, riga 189

**Originale:** Ritorna `(int)Assistant.Targeting.LastAttack` — il serial del last attack target (aggiornato ogni volta che il player attacca). Usato pervasivamente nei combat script.

**Migrato:** `uint GetLastAttack() => 0` — stub fisso. Qualsiasi script che usa `Target.GetLastAttack()` per retargetare dopo un ciclo di combattimento si troverà con serial `0`.

**Fix:** Tracciare il LastAttack serial in `ITargetingService` o `IWorldService`, aggiornato via pacchetto `0xAA` (Attack OK) dal server.

---

### ✅ [S2-10] `Spells.Cast` — Mancano gli overload con `target`, `wait`, `waitAfter`
**File:** `SpellsApi.cs`

**Originale:** `Cast(string SpellName, uint target, bool wait = true, int waitAfter = 0)` — un'unica chiamata che: invia il pacchetto di cast, attende la conferma del server, invia il target automaticamente, aspetta il numero di ms specificato in `waitAfter`. È la firma usata nella stragrande maggioranza degli script di combattimento.

**Migrato:** `Cast(string name)` — nessun parametro target, wait, o waitAfter. Per replicare il comportamento originale servono 3–4 chiamate separate: `Cast(name)`, `WaitForTarget()`, `Target.TargetExecute(serial)`, `WaitCast()`. Tutti gli script esistenti che usano la firma con parametri (`Spells.Cast("Greater Heal", target.Serial, True, 500)`) generano `TypeError` in Python per numero di argomenti errato.

**Fix:** Aggiungere gli overload: `Cast(string name, uint target, bool wait = true, int waitAfter = 0)` e `Cast(string name, Mobile mobile, bool wait = true, int waitAfter = 0)` che internamente gestiscono il targeting e il wait come nell'originale.

---

### ✅ [S2-11] `Spells.WaitCastComplete` — Polling a 50ms, nessuna rilevazione di fizzle né paralisi
**File:** `SpellsApi.cs`, riga 31–39

**Originale:** Usa `WaitHandle.WaitAny([ManualResetEvent, CountdownEvent], timeout)` event-driven al millisecondo. Registra un handler sul pacchetto `0x54` (PlaySound) per rilevare il fizzle. Ha il callback `ParalyzeChanged`: se il player viene paralizzato durante il cast, il CountdownEvent viene segnalato e il metodo esce immediatamente. Non consuma CPU in attesa (WaitHandle è kernel-level sleep).

**Migrato:** `while (TickCount64 < deadline) { Sleep(50); }` — polling puro. Conseguenze concrete:
1. **Nessuna rilevazione di fizzle**: lo script aspetta il timeout intero dopo ogni spell fallita (identico a S2-03 ma da lato `WaitCast`, non `WaitForTargetOrFizzle`)
2. **Nessuna rilevazione di paralisi**: se il player viene paralizzato durante l'attesa, il ciclo prosegue ignorando lo stato
3. **Jitter di 50ms** su ogni operazione di spell-cast: cumulativo su script ad alta frequenza di cast

**Fix:** Iscriversi al `PacketService.OnPacketReceived` per `0x54` (fizzle sound) e a un evento `IWorldService.OnPlayerStateChanged` per paralisi, usando `ManualResetEventSlim` per risvegliare il thread di attesa senza polling.

---

### ✅ [S2-09] `Target.LastUsedObject()` — Ritorna sempre `0`
**File:** `TargetApi.cs`, riga 221

**Originale:** Ritorna `World.Player.LastObject` (il serial dell'ultimo oggetto usato dal player, es. per auto-repeat di azioni). Ritorna `-1` se non disponibile.

**Migrato:** `uint LastUsedObject() => 0` — stub fisso. Ritornare `0` invece di `-1` modifica anche la semantica del valore "non disponibile" che gli script originali testano.

**Fix:** Tracciare `LastObject` nel `WorldService` (aggiornato da pacchetto `0x08` double-click inviato) e ritornarlo qui.

---

## SEVERITY 3 — TYPE MISMATCH (rompono script Python)

### ✅ [S3-01] `Target.GetLast()` — Ritorna `uint` invece di `int`
**Originale:** `static int GetLast()` — ritorna signed int. Script che testano `if Target.GetLast() == -1:` (serial non valido) sono idiomatici in UO scripting.

**Migrato:** `uint GetLast()` — unsigned. Il test `== -1` non corrisponde mai su un uint. Tutti gli script che controllano il valore di ritorno per validità sono silenziosamente rotti.

**Fix:** Cambiare la firma di ritorno in `int`, ritornare `-1` se `LastTarget == 0`.

---

### ✅ [S3-02] `Target.PromptTarget()` — Ritorna `uint` invece di `int`
**Originale:** `int PromptTarget(...)` — ritorna `-1` se il prompt è stato cancellato. Il pattern `if result == -1: return` è universale.

**Migrato:** `uint PromptTarget(...)` — non può rappresentare `-1`. Cancellazione non distinguibile da serial `0`.

**Fix:** Restituire `int`, con `-1` come sentinel per cancellazione.

---

### ✅ [S3-03] `Items.Filter.OnGround` — `bool` invece di tristate `int`
**Originale:** `int OnGround` con semantica tristate: `1` = solo a terra, `0` = qualsiasi, `-1` = solo in container. Script che scrivono `filter.OnGround = 1` su un bool ottengono errore di tipo in Python (o silenzioso cast sbagliato in C#).

**Migrato:** `bool OnGround` — perde la semantica tristate. Non si può più filtrare "solo in container".

**Fix:** Ripristinare come `int` con i tre valori (-1/0/1) documentati.

---

### ✅ [S3-04] `Mobiles.Filter.Notoriety` — Singolo `int` invece di `List<byte> Notorieties`
**Originale:** `List<byte> Notorieties` — permette di filtrare per più notorietà contemporaneamente (es. criminali `4` + murderers `5`). Script tipici: `filter.Notorieties.Add(4)`.

**Migrato:** `int Notoriety` — valore singolo. Script che fanno `.Notorieties.Add(...)` ottengono `AttributeError`. Script che cercano più notorietà devono essere riscritti.

**Fix:** Rinominare in `Notorieties` e tornare a `List<byte>` (o `List<int>`). Aggiornare `ApplyFilter` di conseguenza.

---

## SEVERITY 4 — PROPRIETÀ MANCANTI NEI FILTER

### ✅ [S4-01] `Items.Filter` — 7 proprietà dell'originale assenti
**File:** `ItemsApi.cs`, classe `Filter`

| Proprietà | Tipo originale | Stato in Improved |
|-----------|---------------|-------------------|
| `IsContainer` | `int` (tristate) | ❌ Assente |
| `IsCorpse` | `int` (tristate) | ❌ Assente |
| `IsDrop` | `int` (tristate) | ❌ Assente |
| `RangeMin` | `int` | ❌ Assente (solo `Range`) |
| `RangeMax` | `int` | ❌ Assente (solo `Range`) |
| `Parent` | `int` (container serial) | ❌ Assente (solo `Container`) |
| `ExcludeSerial` | `int` | ❌ Assente |

Script AutoLoot e organizer usano `IsCorpse = 1` per filtrare. Script avanzati usano `RangeMin`/`RangeMax` per range annuli. `ExcludeSerial` è usato per escludere il player o item speciali.

**Fix:** Aggiungere tutte le proprietà mancanti alla classe `Filter` e gestirle nel metodo `ApplyFilter`.

---

### ✅ [S4-02] `Mobiles.Filter` — 5 proprietà dell'originale assenti
**File:** `MobilesApi.cs`, classe `Filter`

| Proprietà | Tipo originale | Stato in Improved |
|-----------|---------------|-------------------|
| `IsHuman` | `int` (tristate) | ❌ Assente |
| `IsGhost` | `int` (tristate) | ❌ Assente |
| `IsAlly` | `int` (tristate) | ❌ Assente |
| `IsEnemy` | `int` (tristate) | ❌ Assente |
| `IsNeutral` | `int` (tristate) | ❌ Assente |

Script PvP e healing usano `IsHuman = 1` per filtrare solo player. `IsGhost = 1` è usato per evitare di targetare ghost. L'assenza di questi flag rende il filtering dei mobiles molto meno preciso.

**Fix:** Aggiungere le proprietà mancanti e implementare la logica corrispondente in `ApplyFilter`.

---

## SEVERITY 5 — OVERLOAD MANCANTI

### ✅ [S5-01] `Target.TargetResource` — Mancano 3 overload su 4
**File:** `TargetApi.cs`

| Firma originale | Stato |
|----------------|-------|
| `TargetResource(int serial, int resource_number)` | ❌ Duplicato + errato (vedi S1-01, S2-02) |
| `TargetResource(int serial, string resource_name)` | ❌ Assente |
| `TargetResource(Item item, string resource_name)` | ❌ Assente |
| `TargetResource(Item item, int resource_number)` | ❌ Assente |

Lo string overload è essenziale: `Target.TargetResource(pickaxe, "ore")` è la chiamata più comune nei mining script. I nomi validi (`"ore"`, `"sand"`, `"wood"`, `"graves"`, `"red_mushroom"`) devono essere mappati ai resource number 0–4.

**Fix:** Implementare tutti e 4 gli overload con il pacchetto corretto (vedi S2-02).

---

### ✅ [S5-02] `Items.FindByID` — 4° parametro incompatibile
**Originale:** `FindByID(int itemid, int color = -1, int container = -1, int range = -1)` — `range` è la distanza massima di ricerca.
**Migrato:** `FindByID(int, int hue, uint container, bool recurse)` — il 4° parametro è `bool recurse` non `int range`.

Script che passano un range come 4° parametro (es. `Items.FindByID(0x0EED, -1, -1, 5)`) silenziosamente falliscono il loro intento.

**Fix:** Aggiungere un overload che accetti `range` come 4° parametro `int`. Considerare di mantenere la firma originale come primary overload.

---

## SEVERITY 6 — DIFFERENZE COMPORTAMENTALI

### ✅ [S6-01] `Spells.Cast` — Risoluzione nome: substring matching vs Levenshtein
**File:** `SpellDefinitions.cs`, riga 174–192

**Originale:** `GuessSpellName` usa `UOAssist.LevenshteinDistance` su tutti i nomi di spell. Qualsiasi abbreviazione o typo viene risolto alla spell con edit-distance minima. `"GrHeal"`, `"GreaterHeal"`, `"grtheal"`, `"greather heal"` (con typo) trovano tutti `"Greater Heal"`. Il sistema non fallisce mai la risoluzione — ritorna sempre la match più vicina.

**Migrato:** `TryGetSpellId` fa prima match esatta (case-insensitive) poi `Contains` (substring). `"GrHeal"` non è substring di nulla → **fallisce silenziosamente** (solo un LogWarning, nessun errore allo script). Script storici che usano abbreviazioni comuni o nomi con typo smettono di castare senza errore esplicito, comportamento difficile da debuggare.

**Fix:** Aggiungere uno step di fallback con Levenshtein (o anche solo con matching su iniziali/abbreviazioni) quando la ricerca per substring fallisce. Accettare tutte le abbreviazioni documentate nel wiki di RazorEnhanced.

---

### ✅ [S6-02] Null-coalescing restituisce `0` silenziosamente: script continuano con dati falsi
**File:** `PlayerApi.cs` — pattern pervasivo `P?.Hits ?? 0`, `P?.Mana ?? 0`, ecc.

**Originale:** Se `World.Player` è null e lo script accede a `Player.Hits`, il codice originale accede a una proprietà statica che o lancia eccezione o usa un controllo esplicito. Il comportamento è **fail-fast e visibile**: lo script si ferma o registra un errore.

**Migrato:** Il pattern `P?.Hits ?? 0` restituisce `0` quando `Player` è null. Uno script che controlla `if Player.Hits < 10: HealSelf()` continuerà a girare e tenterà di curarsi anche quando non è connesso, potendo innescare cicli di azioni in stato incoerente. Non c'è modo per lo script di distinguere "HP = 0 perché disconnesso" da "HP = 0 perché morto".

**Fix:** Esporre un metodo `Player.IsConnected` o `Player.IsValid` come guard esplicito (già esiste come `IsConnected` in `PlayerApi`). Documentare chiaramente che tutte le proprietà ritornano `0`/`null` quando disconnessi. Valutare se alcune proprietà critiche (`Hits`, `Mana`, `Stam`) debbano lanciare eccezione o ritornare un sentinel negativo (`-1`) per distinguere il caso null.

---

### ✅ [S6-04] `Misc.SendToClient(string keys)` — Non supporta la sintassi SendKeys
**Originale:** Usa `System.Windows.Forms.SendKeys.SendWait(keys)` che supporta la notazione speciale: `{Enter}`, `{Tab}`, `^u` (Ctrl+U), `%f` (Alt+F), ecc.

**Migrato:** Invia `WM_CHAR` per ogni singolo carattere della stringa. Non interpreta `{Enter}`, `^u`, `%f`. `Misc.SendToClient("{Enter}")` invierà letteralmente `{`, `E`, `n`, `t`, `e`, `r`, `}` invece di INVIO.

**Fix:** O aggiungere riferimento a `System.Windows.Forms` e usare `SendKeys.SendWait`, o implementare un parser della sintassi SendKeys che mappa le sequenze speciali ai VK codes appropriati.

---

### ✅ [S6-05] `Player.SetLast(int serial, bool wait)` — Manca l'effetto highlight
**Originale:** Chiama `TargetMessage(serial, wait)` che invia un pacchetto di evidenziazione visiva del target (il cursore lampeggioso), poi chiama `Targeting.SetLastTarget(serial, 0, wait)`.

**Migrato:** Solo `_targeting.LastTarget = serial` — nessun pacchetto inviato, nessun effetto visivo. Il target non si illumina nel client UO quando uno script fa `Target.SetLast(serial)`.

**Fix:** Inviare il pacchetto appropriato (un target generico al serial) per triggerare il feedback visivo, poi aggiornare `LastTarget`.

---

### ✅ [S6-06] `Misc.AppendToFile` / `WriteFile` / `DeleteFile` — Nessuna validazione path/estensione
**Originale:** Valida che il filename abbia una delle estensioni permesse (`.data`, `.xml`, `.map`, `.csv`) e che il path sia dentro le `ValidFileLocations` di RazorEnhanced. Restituisce `false` se il file è fuori dalle path consentite.

**Migrato:** Nessuna validazione. Uno script Python può scrivere su qualsiasi path del filesystem (es. `Misc.WriteFile("C:\\Windows\\System32\\test.dll", ...)`) senza restrizioni. È una regressione di sicurezza.

**Fix:** Implementare whitelist di estensioni permesse e confinare le operazioni alle directory `Scripts/`, `Data/`, `Config/` dell'applicazione.

---

### ✅ [S6-07] `Misc.ScriptStop(string scriptfile)` — Può fermare solo lo script corrente
**Originale:** Può fermare qualsiasi script in esecuzione per nome, inclusi script avviati da altri script.

**Migrato:** Controlla `string.Equals(_scripting.CurrentScriptName, scriptfile)` — se il nome non corrisponde allo script in esecuzione, non fa nulla. La capacità di fermare script "fratelli" è persa.

**Fix:** Introdurre un registry di script in esecuzione in `IScriptingService` e supportare lo stop per nome anche di script diversi dal corrente.

---

### ✅ [S6-08] `ScriptSuspend` e `ScriptResume` — Stub vuoti
**Originale:** Supporta la sospensione/ripresa granulare di script in esecuzione.

**Migrato:** Entrambi sono stub vuoti `{ /* stub */ }`. `Misc.ScriptIsSuspended(name)` ritorna sempre `false`. Script che coordinano il proprio ciclo vita tramite suspend/resume non funzionano.

**Fix:** Implementare un flag di sospensione in `ScriptingService` e integrarlo nel loop di esecuzione Python.

---

### ✅ [S6-09] `Player.Pets` — Basato su Notoriety anziché tracking lato server
**Originale:** Usa `World.Player.Pets` — lista aggiornata dai pacchetti `0x11` / bonding status del server.

**Migrato:** Restituisce tutti i mobiles con `Notoriety == 2`. Un mobile neutrale (`Notoriety == 2`) visibile nel viewport potrebbe non essere un pet del player. I pet in stalla o fuori range non appaiono nella lista anche se sono effettivamente del player.

**Fix:** Tracciare i serial dei pet dal pacchetto di status del player o dai party/pet packets. Usare la lista aggiornata dal server anziché filtrare per notorietà.

---

### ✅ [S6-10] `Misc.Distance` — Algoritmo Chebyshev invece di Pythagorean
**Originale:** Delega a `Utility.Distance(X1, Y1, X2, Y2)` di Razor, che usa distanza Euclidea (Pythagorean).

**Migrato:** `Math.Max(Math.Abs(x2-x1), Math.Abs(y2-y1))` — distanza di Chebyshev. Per distanze diagonali i risultati divergono significativamente. Script che usano `Misc.Distance` per range checks avranno comportamenti diversi, soprattutto in diagonale.

> **Nota:** Verificare nel codice originale di Razor `Utility.Distance` prima di correggere — se anche Razor usa Chebyshev internamente, questo non è un bug. Se usa Euclidean, aggiornare a `Math.Sqrt(dx*dx + dy*dy)`.

---

## SEVERITY 7 — AGENTI: API SCRIPTING NON ESPOSTA

### ✅ [S7-01] AutoLoot, Dress, Scavenger, Restock — Servizi presenti, nessun wrapper script
I servizi `AutoLootService`, `DressService`, `ScavengerService`, `RestockService` esistono nel Core e hanno ViewModel e UI. **Non esistono però wrapper di scripting** equivalenti a `AutoLoot`, `Dress`, `Scavenger`, `Restock` nel namespace dell'API Python.

**Originale (TMRazor):**
```python
AutoLoot.Start()
AutoLoot.ChangeList("MyList")
Dress.Start()
Scavenger.Start()
Restock.Start()
```

**Migrato:** `AttributeError: 'NoneType' object has no attribute 'Start'` — le variabili non sono definite nel contesto Python.

**Fix:** Creare wrapper script `AutoLootApi`, `DressApi`, `ScavengerApi`, `RestockApi` (analoghi a `BandageHealApi`, `OrganizerApi` se già esistenti) e registrarli in `ScriptingService` come variabili Python `AutoLoot`, `Dress`, `Scavenger`, `Restock`.

---

## SEVERITY 8 — PROBLEMI MINORI / ATTENZIONE

### ✅ [S8-01] `Journal.JournalEntry.Serial` — Tipo `int` vs `int` (OK, ma timestamp diverso)
L'originale usa `double Timestamp` come secondi Unix (TotalSeconds). L'improved usa `long` Unix milliseconds. Script che confrontano timestamp potrebbero avere problemi se mescolano chiamate API. **Documentare il cambio di unità.**

### ✅ [S8-02] `Gumps.WaitForGump(PythonList, int)` — Verifica compatibilità
L'originale ha un overload che accetta `IronPython.Runtime.PythonList`. L'improved usa `List<uint>`. In IronPython, una lista Python è automaticamente convertibile a `List<T>` se i tipi matchano. **Verificare con test di integrazione.**

### ✅ [S8-03] `Target.SetLast` con `wait = true` — Comportamento wait non implementato
L'originale con `wait = true` attende conferma dal server prima di ritornare. Il migrato ignora il parametro `wait` completamente. Per script time-sensitive questo può causare race conditions.

### ✅ [S8-04] `NoRunStealthToggle` / `NoRunStealthStatus` — Stub senza implementazione
L'originale controlla l'opzione "No-Run Stealth" del profilo utente. L'improved ha stub vuoti. Non critico per la maggior parte degli script, ma script stealth possono dipenderci.

### ✅ [S8-05] `Items.ApplyFilter` vs `Items.Filter` scripting pattern
L'improved ha sia il metodo `ApplyFilter(Filter filter)` che la classe `Filter` come inner class. **Verificare** che da Python si possa istanziare correttamente `filter = Items.Filter()` (accesso alla nested class su un'istanza IronPython) e che `Items.ApplyFilter(filter)` accetti l'oggetto restituito.

---

## Riepilogo priorità

| Severity | Count | Azione richiesta |
|----------|-------|-----------------|
| S1 — Compile Error | 1 | Blocca il build — fix immediato |
| S2 — Funzionalmente errato | 11 | Rotto per gli script — fix release |
| S3 — Type mismatch | 4 | Silently broken — fix release |
| S4 — Filter properties mancanti | 2 | Script avanzati rotti — fix release |
| S5 — Overload mancanti | 2 | API incompleta — fix release |
| S6 — Differenze comportamentali | 10 | Degrado silenzioso — fix post-release |
| S7 — Agenti senza wrapper script | 1 | Feature mancante — fix release |
| S8 — Minori / attenzione | 5 | Monitorare / documentare |

**Totale criticità bloccanti o gravi (S1–S5):** 20
**Totale criticità significative (S6–S7):** 11
**Totale annotazioni (S8):** 5

---

## Conclusione

Il team ha migrato correttamente la **struttura** dell'API (tutti i nomi di classe e metodo esistono) ma ha trascurato la **fedeltà comportamentale**. Le criticità più dannose riguardano il sistema di targeting (`TargetExecuteRelative`, `TargetResource`, `WaitForTargetOrFizzle`, `PromptGroundTarget`) e i Filter objects (`Items.Filter`, `Mobiles.Filter`), che sono le fondamenta di qualsiasi script UO non banale. Uno script di mining, harvesting, combat magico o auto-loot avanzato proveniente da TMRazor **non funzionerà correttamente** su TMRazorImproved senza modifiche.

Il build attuale non compila nemmeno (S1-01).
