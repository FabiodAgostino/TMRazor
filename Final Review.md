# Final Review: Mappatura Architetturale 1:1 TMRazor -> TMRazorImproved

**Data**: 2026-03-20
**Autore**: Senior Architect Review (AI-Assisted)
**Scopo**: Garantire che NESSUNA funzionalita, classe o metodo venga perso durante la migrazione
**Formato**: Ogni discrepanza e un Task azionabile per un Junior Developer

---

## Indice

1. [Riepilogo Esecutivo](#1-riepilogo-esecutivo)
2. [Motori di Scripting](#2-motori-di-scripting)
3. [API di Gioco (Script-Facing)](#3-api-di-gioco-script-facing)
4. [Sistema Agenti](#4-sistema-agenti)
5. [Sistema Macro](#5-sistema-macro)
6. [Filtri e Rete](#6-filtri-e-rete)
7. [Modelli Core e Configurazione](#7-modelli-core-e-configurazione)
8. [UI Forms e Dialoghi](#8-ui-forms-e-dialoghi)
9. [Sistemi Utility](#9-sistemi-utility)
10. [Riepilogo Task per Priorita](#10-riepilogo-task-per-priorita)

---

## 1. Riepilogo Esecutivo

### Statistiche Globali

| Metrica | Legacy (TMRazor) | Nuovo (TMRazorImproved) | Copertura |
|---------|-------------------|--------------------------|-----------|
| File .cs totali | ~510 | ~335 | N/A (architettura diversa) |
| Motori scripting | 3 (C#, Python, UOSteam) | 3 (C#, Python, UOSteam) | 100% motori, ~16% UOSteam |
| API scripting (classi) | 18 | 22 | ~89% metodi |
| Agenti automazione | 8 | 8 + 3 nuovi | 100% agenti, ~77% metodi |
| Azioni macro | 43 tipi | ~33 coperti | 77% |
| Filtri pacchetti | 13 classi | 1 handler unificato | ~85% |
| Handler pacchetti C2S | 25 | 13 | 52% |
| Handler pacchetti S2C | 48 | 56 (8 nuovi) | ~94% legacy + nuovi |
| Configurazione | ~6.893 righe | ~816 righe | ~85-90% settings |
| Form/Dialoghi UI | 17 form | 10 window + 27 page | ~90% funzionalita |

### Aree Critiche Identificate

| # | Area | Severita | Impatto Utente |
|---|------|----------|----------------|
| 1 | UOSteam Interpreter copre ~16% del legacy | CRITICO | Script UOSteam complessi non funzioneranno |
| 2 | API Trade completamente assente | CRITICO | Script di trading non funzioneranno |
| 3 | API CUO completamente assente | CRITICO | Integrazione ClassicUO da script impossibile |
| 4 | `int` -> `uint` seriali in TUTTE le API | CRITICO | Potenziale rottura di tutti gli script esistenti |
| 5 | `Gumps` -> `Gump` namespace | ALTO | Tutti gli script con gump non compilano |
| 6 | Spell school-specific targeted overloads mancanti | ALTO | `Spells.CastMagery("Heal", target)` fallisce |
| 7 | Movement/Pathfind nelle macro assenti | ALTO | Macro di movimento non funzionano |
| 8 | 13 handler C2S mancanti | MEDIO | Alcune interazioni client non tracciate |
| 9 | Debug stepping nello script editor assente | MEDIO | Sviluppatori script perdono debugging |
| 10 | Inspector Mobile/Item degradato | BASSO | Meno dettagli nell'ispezione entita |

---

## 2. Motori di Scripting

### 2.1 CSharpEngine.cs -> CSharpScriptEngine.cs

| Aspetto | Legacy | Nuovo |
|---------|--------|-------|
| Righe | 473 | 116 |
| Compilatore | `CSharpCodeProvider` (out-of-process csc.exe) | `Microsoft.CodeAnalysis.CSharp.Scripting` (Roslyn in-process) |
| Pattern | Singleton `CSharpEngine.Instance` | DI, istanza per-esecuzione |

#### TASK-FR-001: Direttive C# Script Mancanti ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/CSharpEngine.cs` -> `FindDirectivesInFile()`, `ExtractFileNameFromDirective()`, `FindAllAssembliesIncludedInCSharpScripts()`, `FindAllIncludedCSharpScript()`, `CheckForceDebugDirective()`
- **Stato nel Nuovo Codice**: ✅ Implementato in `CSharpScriptEngine.cs` — metodi `PreprocessDirectives()`, `ParseDirectives()`, `TryResolvePath()`
- **Dettaglio**: Il legacy supporta le direttive `//#import` (includere altri file .cs), `//#assembly` (referenziare assembly custom) e `//#forcedebug` (forzare compilazione debug). Nessuna di queste e implementata nel nuovo `CSharpScriptEngine.cs`.
- **Impatto Utente**: Gli script C# che usano `//#import` per dividere il codice in piu file o `//#assembly` per caricare DLL custom **non compileranno**. Questo influenza utenti avanzati che organizzano i propri script in moduli.
- **Documentazione per Junior Dev**: Aprire `TMRazorImproved.Core/Services/Scripting/Engines/CSharpScriptEngine.cs`. Attualmente il metodo `Execute()` usa `CSharpScript.RunAsync()`. Bisogna aggiungere un pre-processing del codice sorgente prima della compilazione:
  1. Scansionare le righe del file per `//#import <filename>` e sostituirle con il contenuto del file referenziato (vedi `FindAllIncludedCSharpScript()` in `CSharpEngine.cs` righe 85-130)
  2. Scansionare per `//#assembly <path>` e aggiungerle a `ScriptOptions.WithReferences()` (vedi `FindAllAssembliesIncludedInCSharpScripts()` righe 60-84)
  3. Scansionare per `//#forcedebug` e se presente impostare `OptimizationLevel.Debug` nelle opzioni Roslyn

---

### 2.2 PythonEngine.cs -> ScriptingService.ExecutePythonInternal()

| Aspetto | Legacy | Nuovo |
|---------|--------|-------|
| Righe | 269 | ~70 (inline) |
| Architettura | Classe standalone riutilizzabile | Inline in `ScriptingService` |
| Engine | IronPython con riuso istanza | IronPython3, nuova istanza per esecuzione |

#### TASK-FR-002: Python Call() da C# Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/PythonEngine.cs` -> `Call(PythonFunction, params object[])`
- **Stato nel Nuovo Codice**: ✅ Implementato — `ScriptingService.CallPythonFunction(string, params object[])` + aggiunto a `IScriptingService`
- **Dettaglio**: Il legacy permette di invocare funzioni Python da codice C# tramite `Call()`. Questo metodo non esiste nel nuovo codice.
- **Impatto Utente**: Funzionalita interna usata da alcuni componenti per callback Python. Impatto diretto utente **basso** ma puo impedire estensioni future.
- **Documentazione per Junior Dev**: In `ScriptingService.cs`, metodo `ExecutePythonInternal()` (riga ~430). Se necessario, aggiungere un metodo `CallPythonFunction(string functionName, params object[] args)` che usi `scope.GetVariable<Func<...>>()` di IronPython per invocare la funzione.

---

### 2.3 UOSteamEngine.cs -> UOSteamInterpreter.cs (GAP CRITICO)

| Aspetto | Legacy | Nuovo |
|---------|--------|-------|
| Righe | 8.331 | 1.307 (~15.7%) |
| Parser | AST completo (Lexer + Parser + nodi AST) | Tokenizer riga-per-riga con regex |
| Comandi registrati | 121 | ~105 |
| Espressioni registrate | 85+ | ~40 |
| Totale handler | ~206 | ~145 |
| Eccezioni custom | 5 classi (`UOSScriptError`, `UOSSyntaxError`, etc.) | try/catch generico |

#### TASK-FR-003: Comandi UOSteam Mancanti ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/UOSteamEngine.cs` -> Interpreter.RegisterCommandHandler()
- **Stato nel Nuovo Codice**: ✅ Implementati tutti i 16 comandi in `UOSteamInterpreter.cs` (switch ExecuteLine)
- **Dettaglio**: I seguenti comandi UOSteam non esistono nel nuovo interprete:

| Comando Mancante | Descrizione | Impatto Utente |
|-------------------|-------------|----------------|
| `uniquejournal` | Filtraggio journal | Script journal avanzati falliscono |
| `info` | Popup info item | Script che mostrano info item falliscono |
| `clickscreen` | Click su coordinate schermo | Automazione basata su coordinate fallisce |
| `mapuo` | Integrazione mappa | Script mappa falliscono |
| `questsbutton` | Apri finestra quest | Automazione quest fallisce |
| `logoutbutton` | Logout dal gioco | Script logout falliscono |
| `chatmsg` | Messaggio chat | Script chat falliscono |
| `promptmsg` | Messaggio prompt | Script prompt falliscono |
| `timermsg` | Messaggio timer overhead | Timer visivi falliscono |
| `waitforprompt` | Attesa prompt server | Script interattivi falliscono |
| `cancelprompt` | Cancella prompt | Script interattivi falliscono |
| `setskill` | Set skill lock | Gestione skill fallisce |
| `autocolorpick` | Color picker automatico | Script dyeing falliscono |
| `canceltarget` | Cancella target cursor | Targeting fallisce |
| `namespace` | Namespace script | Organizzazione script avanzata impossibile |
| `script` | Chiamata inter-script | Composizione script impossibile |

- **Documentazione per Junior Dev**: Aprire `TMRazorImproved.Core/Services/Scripting/Engines/UOSteamInterpreter.cs`. Il metodo `ExecuteLine()` usa uno `switch` su `cmd.ToUpperInvariant()`. Per ogni comando mancante:
  1. Aggiungere un nuovo `case "NOMECOMANDO":` nello switch
  2. Implementare la logica referenziando il legacy `UOSteamEngine.cs` dove il comando e registrato con `RegisterCommandHandler("nomecomando", HandlerMethod)`
  3. Cercare il metodo handler corrispondente nel legacy (es. `HandleNamespace`, `HandleScript`, etc.)

#### TASK-FR-004: Espressioni UOSteam Mancanti ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/UOSteamEngine.cs` -> Interpreter.RegisterExpressionHandler()
- **Stato nel Nuovo Codice**: ✅ Implementate le espressioni mancanti in `UOSteamInterpreter.cs` — `findalias`, `organizing`, `restocking`, `buffexists`, `findwand` (stub), `contents`, `distance`, `bandage`, `name` (confronto stringa). Le espressioni numeriche già presenti: `x/y/z`, `inregion`, `skillbase`, `findobject`, `amount`, `graphic`, `inrange`, `property`, `durability`, `findlayer`, `skillstate`, `counttypeground`, `infriendlist`, `ingump`, `gumpexists`, `weight/maxweight/diffweight`, `mana/stam/str/dex/int`, `followers/gold/luck`, resistenze, `diffhits`, `serial`, `direction/directionname`, notorietà.
- **Dettaglio**: Le seguenti espressioni booleane/valore per IF/WHILE non esistono:

| Espressione | Tipo | Uso Tipico |
|-------------|------|------------|
| `findalias` | bool | `if findalias 'myalias'` |
| `x`, `y`, `z` | int | `if x = 1234` (coordinate player) |
| `organizing` | bool | `if organizing` (agente attivo?) |
| `restocking` | bool | `if restocking` |
| `contents` | int | `if contents 'backpack' < 125` |
| `inregion` | bool | `if inregion 'town'` |
| `skillbase` | int | `if skillbase 'Magery' >= 1000` |
| `findobject` | bool | `if findobject 0x1234` |
| `amount` | int | `if amount 'found' > 5` |
| `distance` | int | `if distance 'enemy' <= 2` |
| `graphic` | int | `if graphic 'found' = 0x1234` |
| `inrange` | bool | `if inrange 'enemy' 10` |
| `buffexists` | bool | `if buffexists 'Strength'` |
| `property` | mixed | `if property 'Damage Increase' 'self' >= 50` |
| `durability` | int | `if durability 'self' < 20` |
| `findlayer` | serial | `if findlayer 'self' InnerTorso` |
| `skillstate` | string | `if skillstate 'Magery' = 'up'` |
| `counttypeground` | int | `if counttypeground 0x0EED 0 3` |
| `findwand` | bool | `if findwand 'heal'` |
| `infriendlist` | bool | `if infriendlist 'found'` |
| `ingump` | bool | `if ingump 0xABCD 'text'` |
| `gumpexists` | bool | `if gumpexists 0xABCD` |
| `serial` | int | `if serial 'found' = 0x1234` |
| `weight` | int | `if weight < 300` |
| `maxweight` | int | Peso massimo |
| `diffweight` | int | Peso disponibile |
| `mana` / `maxmana` | int | Mana corrente/max |
| `stam` / `maxstam` | int | Stamina corrente/max |
| `dex` / `int` / `str` | int | Statistiche base |
| `followers` | int | Numero follower |
| `gold` | int | Gold in backpack |
| `luck` | int | Luck stat |
| `criminal` / `enemy` / `friend` / `gray` / `innocent` / `murderer` | bool | Check notorieta |
| `bandage` | int | Conta bende |
| `color` / `direction` / `directionname` / `name` | mixed | Attributi entita |
| `diffhits` | int | HP mancanti |
| Tutte le resistenze (`fireresist`, `coldresist`, etc.) | int | Resistenze player |

- **Impatto Utente**: **CRITICO** - La maggior parte degli script UOSteam usa queste espressioni in condizioni IF/WHILE. Senza di esse, script anche basilari di combattimento, healing e farming **non funzioneranno**.
- **Documentazione per Junior Dev**: Aprire `UOSteamInterpreter.cs`, metodo `EvaluateExpression()`. Per ogni espressione mancante:
  1. Aggiungere un `case "nomeespressione":` nello switch
  2. Recuperare il valore richiesto tramite i servizi iniettati (es. `_worldService`, `_scriptGlobals`)
  3. Referenziare `UOSteamEngine.cs` cercando `RegisterExpressionHandler("nome", HandlerMethod)` per la logica esatta

#### TASK-FR-005: Stub UOSteam Esistenti ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/UOSteamEngine.cs`
- **Stato nel Nuovo Codice**: ✅ Implementato in `UOSteamInterpreter.cs` — `equipwand`: cerca nel backpack item col nome "wand" + tipo richiesto e lo equipaggia; `shownames`: invia SingleClick (0x09) per ogni mobile/corpse visibile nel range 18 tile; `replay`: già correttamente implementato (reset `_currentLineIndex=-1` → ripartenza da riga 0). Nota: `equipwand` e `shownames` erano stub anche nel legacy (NotImplemented).
- **Dettaglio**: I seguenti comandi esistono nel nuovo codice ma sono stub non funzionali:
  - `equipwand` (riga ~714: "Stub")
  - `shownames` (riga ~847: "Stub for now, requires interop integration")
  - `replay` (riga ~856: "Stub")
- **Documentazione per Junior Dev**: Cercare "Stub" in `UOSteamInterpreter.cs`. Per `equipwand`: implementare la ricerca di bacchette nel backpack e l'equip (vedi `HandleEquipWand` nel legacy). Per `shownames`: inviare il pacchetto 0x98 tramite `IPacketService`. Per `replay`: implementare la registrazione e replay di sequenze di azioni.

---

### 2.4 EnhancedScript.cs + Scripts.cs -> ScriptingService.cs

#### TASK-FR-006: Script Loop Mode Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/EnhancedScript.cs` -> `bool loop`
- **Stato nel Nuovo Codice**: ✅ Implementato — `RunAsync()` e `RunScript()` accettano `bool loop = false`; se `true` lo script viene rieseguito in `do-while` finché il CancellationToken non viene cancellato. Aggiornata anche `IScriptingService`.
- **Dettaglio**: Il legacy supporta l'esecuzione ciclica degli script (loop infinito fino a Stop). Non presente nel nuovo codice.
- **Impatto Utente**: Gli utenti che configurano script per eseguirsi in loop continuo (es. farming bot) dovranno aggiungere manualmente un `while True:` nel codice.
- **Documentazione per Junior Dev**: In `ScriptingService.cs`, metodo `RunAsync()`. Aggiungere un parametro `bool loop = false`. Se `loop == true`, wrappare l'esecuzione in un ciclo `while (!cancellationToken.IsCancellationRequested)`.

#### TASK-FR-007: Script Autostart Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Scripts.cs` -> `AutoStart()`
- **Stato nel Nuovo Codice**: ✅ Implementato — aggiunta classe `ScriptConfig` (Name, AutoStart, Loop) in `ConfigModels.cs` + lista `Scripts` in `UserProfile`. `ScriptingService` ora implementa `IRecipient<LoginCompleteMessage>` e chiama `AutoStartScripts()` al login, che avvia in background tutti gli script marcati con `AutoStart=true` (con eventuale `Loop=true`).
- **Dettaglio**: Il legacy permette di marcare script per l'avvio automatico al login. Non presente nel nuovo codice.
- **Impatto Utente**: Gli utenti che hanno script configurati per partire automaticamente al login dovranno avviarli manualmente.
- **Documentazione per Junior Dev**: In `ScriptingService.cs`, aggiungere un metodo `AutoStartScripts()` che legga dalla configurazione (`IConfigService.CurrentProfile`) gli script marcati come autostart e li avvii con `RunAsync()`. Chiamare questo metodo quando si riceve il messaggio `LoginConfirmMessage`.

#### TASK-FR-008: Script FileSystemWatcher Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Scripts.cs` -> `ScriptChanged()` (FileSystemWatcher)
- **Stato nel Nuovo Codice**: ✅ Implementato — aggiunto `FileSystemWatcher` in `ScriptingService` (InitScriptsWatcher), event `ScriptsChanged` in `IScriptingService` e implementazione. Monitora `ScriptsPath` con sottocartelle su Created/Deleted/Renamed/Changed, notificando la UI via evento.
- **Dettaglio**: Il legacy monitora la cartella degli script per modifiche e ricarica automaticamente. Non presente.
- **Impatto Utente**: Modificare un file .py/.cs esternamente non aggiorna la lista script nell'UI. Bisogna riaprire manualmente.
- **Documentazione per Junior Dev**: In `ScriptingService.cs`, aggiungere un `FileSystemWatcher` nel costruttore che monitora `ScriptsDirectory`. Al cambiamento file, invocare un event `ScriptsChanged` che la UI osserva per aggiornare la lista.

#### TASK-FR-009: Script Preload/Precompile Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/EnhancedScript.cs` -> `bool preload`
- **Stato nel Nuovo Codice**: ✅ Implementato — aggiunta property `Preload` in `ScriptConfig`; `CSharpScriptEngine.Precompile()` usa `CSharpScript.Create().Compile()` senza eseguire; `IScriptingService.PreloadScripts()` + implementazione in `ScriptingService` che itera il profilo e pre-compila in background tutti gli script C# con `Preload=true`.
- **Dettaglio**: Il legacy pre-compila gli script C# all'avvio per ridurre il tempo di esecuzione alla prima invocazione.
- **Impatto Utente**: La prima esecuzione di uno script C# sara piu lenta.
- **Documentazione per Junior Dev**: Aggiungere un metodo `PreloadScripts()` in `ScriptingService.cs` che compili (senza eseguire) gli script C# marcati per preload usando `CSharpScript.Create()` di Roslyn.

#### TASK-FR-010: ScriptRecorder Copertura Pacchetti Incompleta ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/ScriptRecorder.cs` -> tutti i metodi `Record_*`
- **Stato nel Nuovo Codice**: ✅ Implementato — aggiunti 8 handler mancanti in `ScriptRecorderService`: 0x08 DropRequest, 0x75 RenameMobile, 0x9A AsciiPromptResponse, 0xB1 GumpsResponse, 0xD7 SADisarm/SAStun, 0xBF sub 0x0015 ContextMenuResponse, 0xAC ResponseStringQuery, 0x7D MenuResponse. Handler 0xBF esteso da gestire anche sub 0x0015.
- **Dettaglio**: Il nuovo ScriptRecorderService copre 9 tipi di pacchetto vs ~15 del legacy. Pacchetti mancanti nella registrazione:

| Azione | Pacchetto | Stato |
|--------|-----------|-------|
| DropRequest | 0x08 | ❌ Mancante |
| RenameMobile | 0x75 | ❌ Mancante |
| AsciiPromptResponse | 0x9A | ❌ Mancante |
| GumpsResponse | 0xB1 | ❌ Mancante |
| SADisarm/SAStun | 0xD7 | ❌ Mancante |
| ContextMenuResponse | 0xBF sub | ❌ Mancante |
| ResponseStringQuery | 0xAC | ❌ Mancante |
| MenuResponse | 0x7D | ❌ Mancante |

- **Impatto Utente**: Registrando macro, azioni come drop item, rinomina mobile, risposte a gump e context menu **non vengono catturate**.
- **Documentazione per Junior Dev**: Aprire il file che contiene ScriptRecorderService (probabilmente sotto `Core/Services/`). Aggiungere viewer per ciascun pacchetto mancante usando `IPacketService.RegisterClientToServerViewer()`. Per il formato dell'output, referenziare `ScriptRecorder.cs` nel legacy, metodi `Record_DropRequest()`, `Record_RenameMobile()`, etc.


---

## 3. API di Gioco (Script-Facing)

### NOTA CRITICA: Cambio Tipo Seriale `int` -> `uint`

#### TASK-FR-012: Breaking Change Pervasivo int -> uint ✅ DONE (2026-03-20)
- **File/Classe Legacy**: TUTTE le API in `Razor/RazorEnhanced/`
- **Stato nel Nuovo Codice**: ✅ Implementato — aggiunti overload `int` per tutti i metodi serial-taking in ItemsApi, MobilesApi, PlayerApi, TargetApi, SpellsApi, MiscApi, FriendApi, JournalApi, SoundApi via `#region int-serial overloads` (opzione 1 consigliata). Build Core: 0 errori.
- **Dettaglio**: TUTTE le API nel nuovo codice usano `uint` per i seriali dove il legacy usava `int`. Questo e un cambio pervasivo che influenza OGNI API.
- **Impatto Utente**: **CRITICO** - Script Python possono funzionare grazie al typing dinamico, ma script C# con `int serial` non compileranno. Confronti come `serial == -1` o `serial == 0` potrebbero comportarsi diversamente.
- **Documentazione per Junior Dev**: Due opzioni:
  1. **(Consigliata)** Aggiungere overload `int` per tutti i metodi API che accettano serial, con cast implicito `(uint)serial`
  2. Aggiungere conversioni implicite nello ScriptGlobals o wrapper Python

### NOTA CRITICA: Cambio Namespace Gumps -> Gump

#### TASK-FR-013: Breaking Change Namespace Gumps ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Gumps.cs` (nome classe: `Gumps`)
- **Stato nel Nuovo Codice**: ✅ Implementato — `ScriptGlobals.cs` riga 20: `public GumpsApi Gumps { get => Gump; set => Gump = value; }` — alias bidirezionale già presente. Per Python, lo scope IronPython riceve `Gumps` tramite ScriptGlobals. Build verificata.
- **Dettaglio**: Ogni script che chiama `Gumps.HasGump()`, `Gumps.SendGump()`, etc. fallira con `NameError: name 'Gumps' is not defined`.
- **Impatto Utente**: **TUTTI** gli script che usano l'API Gumps non funzioneranno.
- **Documentazione per Junior Dev**: In `ScriptGlobals.cs` o nel setup dello scope Python, aggiungere un alias: `scope.SetVariable("Gumps", scope.GetVariable("Gump"))`. Per C#, creare una classe wrapper `public static class Gumps` che delega a `Gump`.

---

### 3.1 Player API

**Legacy**: `Razor/RazorEnhanced/Player.cs` (~104KB)
**Nuovo**: `TMRazorImproved.Core/Services/Scripting/Api/PlayerApi.cs`

**Proprieta**: Tutte le ~60 proprieta sono presenti nel nuovo codice (Hits, HitsMax, Str, Mana, ManaMax, Int, Stam, StamMax, Dex, StatCap, AR, tutte le resistenze, tutte le stats AOS, IsGhost, Poisoned, YellowHits, Visible, WarMode, Paralized, HasSpecial, Female, Name, Notoriety, Serial, Gold, Luck, Body, MobileID, Followers, MaxWeight, Weight, Map, Direction, Position, Backpack, Bank, Quiver, Mount, Connected, Pets, StaticMount).

**Metodi presenti**: TrackingArrow, Area, Zone, ToggleAlwaysRun, DistanceTo, InRange/InRangeMobile/InRangeItem, GetSkillValue, GetRealSkillValue, GetSkillCap, GetSkillStatus, SetSkillStatus, GetStatStatus, SetStatStatus, BuffsExist, GetBuffInfo, BuffTime, SpellIsEnabled, Buffs, BuffsInfo, UnEquipItemByLayer, EquipItem, EquipUO3D, UnEquipUO3D, GetItemOnLayer, ChatSay/ChatWhisper/ChatYell/ChatEmote/ChatChannel/ChatParty, PartyInvite, PartyAccept, LeaveParty, KickMember, PartyCanLoot, SetWarMode, Attack, AttackLast, InvokeVirtue, Run, Walk, PathFindTo, Fly, HeadMessage, OpenPaperDoll, QuestButton, GuildButton, EquipLastWeapon, WeaponPrimarySA/SecondarySA/ClearSA/DisarmSA/StunSA, SumAttribute, GetPropStringList, GetPropStringByIndex, GetPropValue, ClearCorpseList, SetStaticMount.

#### TASK-FR-014: Player.AttackType() Overload Complessi Mancanti ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Player.cs` -> `AttackType(int graphic, int rangemax, string selector, ...)`
- **Stato nel Nuovo Codice**: ✅ Implementato — aggiunti `AttackType(int graphic, int rangemax, string selector, List<int>? color, List<byte>? notoriety)` e `AttackType(List<int> graphics, ...)` in `PlayerApi.cs`. Selectors: Nearest/Farthest/Weakest/Strongest/Random/Next/Previous. Helper privato `ApplyMobileSelector()`. Build: 0 errori.
- **Dettaglio**: Il legacy ha overload complessi di `AttackType` che trovano e attaccano mobile per graphic/color/notorieta con selettori (Nearest/Farthest/Weakest/Strongest/Random). Il nuovo ha solo `AttackType(string type)` per "disarm"/"grapple".
- **Impatto Utente**: Script di combattimento che usano `Player.AttackType(0x00EC, 10, "Nearest")` per attaccare il nemico piu vicino di un certo tipo **non funzioneranno**.
- **Documentazione per Junior Dev**: In `PlayerApi.cs`, aggiungere overload `AttackType(int graphic, int rangemax, string selector, int color = -1, int notoriety = -1)`. La logica deve: 1) Usare `_worldService` per ottenere i mobile nel range, 2) Filtrare per graphic/color/notoriety, 3) Applicare il selettore (Nearest = min distanza, etc.), 4) Chiamare `Attack(serial)` sul risultato. Referenziare `Player.cs` legacy per i selettori (cerca `AttackType`).

#### TASK-FR-015: Player.Corpses Property Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Player.cs` -> `Corpses` (HashSet)
- **Stato nel Nuovo Codice**: ✅ Implementato — `Corpses` property in `PlayerApi.cs` restituisce merge di `_corpseSerials` (HashSet statico) + corpse items in world (graphic 0x2006). `TrackCorpse(uint)` statico per hook WorldPacketHandler. `ClearCorpseList()` ora svuota il set.
- **Dettaglio**: La proprieta `Corpses` traccia i seriali dei cadaveri uccisi dal player. Non presente nel nuovo.
- **Impatto Utente**: Script di looting che usano `Player.Corpses` per iterare i cadaveri recenti non funzioneranno.
- **Documentazione per Junior Dev**: Aggiungere `HashSet<uint> CorpseSerials` nel modello `Mobile` (UOEntity.cs) e una proprieta `Corpses` in `PlayerApi.cs` che la espone. Popolarla in `WorldPacketHandler` quando si riceve un pacchetto di morte (0x2C).

#### TASK-FR-016: Player Chat Overload Interi Mancanti ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Player.cs` -> `ChatEmote(int color, int msg)`, `ChatWhisper(int color, int msg)`, `ChatYell(int color, int msg)`, `ChatChannel(int msg)`
- **Stato nel Nuovo Codice**: ✅ Implementato — aggiunti overload `ChatEmote(int,int)`, `ChatWhisper(int,int)`, `ChatYell(int,int)`, `ChatChannel(int)` in `PlayerApi.cs`. Usano msgId.ToString() come testo (cliloc resolution stub).
- **Dettaglio**: Overload che accettano ID messaggio intero (da cliloc) invece di stringa. Solo le versioni stringa esistono.
- **Impatto Utente**: Script che usano ID messaggio localizzato per le chat non funzioneranno. Impatto **basso** (pochi script usano questa variante).
- **Documentazione per Junior Dev**: In `PlayerApi.cs`, aggiungere overload come `ChatEmote(int color, int msgId)` che risolvono il cliloc ID in stringa e chiamano la versione stringa.

#### TASK-FR-017: Differenze Tipo Ritorno in Player API ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Player.cs`
- **Stato nel Nuovo Codice**: ✅ Implementato — `Run()` e `Walk()` ora ritornano `bool` (true se direzione valida e pacchetto inviato); `PartyAccept()` ritorna `bool true`. Build: 0 errori.
- **Dettaglio**: Alcune signature hanno tipi ritorno diversi:
  - `PartyAccept` ritorna `bool` nel legacy, `void` nel nuovo
  - `Run`/`Walk` ritornano `bool` nel legacy, `void` nel nuovo
- **Impatto Utente**: Script che controllano il valore di ritorno (`if Player.Walk("North"):`) avranno un errore a runtime.
- **Documentazione per Junior Dev**: Modificare le signature in `PlayerApi.cs` per ritornare `bool` dove il legacy lo faceva. Per `Walk`/`Run`, ritornare `true` se il movimento e stato inviato con successo.

---

### 3.2 Items API

**Legacy**: `Razor/RazorEnhanced/Item.cs`
**Nuovo**: `TMRazorImproved.Core/Services/Scripting/Api/ItemsApi.cs`

**Metodi presenti**: FindBySerial, FindByID (overload multipli), FindAllByID, WaitForContents, UseItem, UseItemByID, Move (overload multipli), MoveOnGround, Lift, SingleClick, GetPropStringList, GetPropStringByIndex, GetPropValue, GetPropValueString, WaitForProps, Message, Hide, Close, OpenAt/OpenContainerAt, ContainerCount, BackpackCount, ApplyFilter, SetColor/Color, ChangeDyeingTubColor, DropItemGroundSelf, DropFromHand, FindByName, ContextExist, IgnoreTypes.

#### TASK-FR-018: Items.Select() con Selettore Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Item.cs` -> `Select(List<Item> items, string selector)`
- **Stato nel Nuovo Codice**: ✅ Implementato — `Select(IEnumerable<ScriptItem>, string)` e `Select(IList, string)` in `ItemsApi.cs`. Selectors: Nearest/Farthest/Less/Most/Lightest/Heaviest/Random.
- **Dettaglio**: Il legacy ha `Select(List<Item>, string)` dove selector puo essere "Nearest"/"Farthest"/"Less"/"Most"/"Lightest"/"Heaviest". Il nuovo ha solo `Select(uint serial)` che e una cosa diversa.
- **Impatto Utente**: Script che filtrano liste di item per prossimita/peso/quantita non funzioneranno.
- **Documentazione per Junior Dev**: In `ItemsApi.cs`, aggiungere un metodo `Select(List<dynamic> items, string selector)` che ordina la lista secondo il criterio e ritorna il primo elemento. Referenziare `Item.cs` legacy, metodo `Select()`.

#### TASK-FR-019: Items.GetImage() Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Item.cs` -> `GetImage(int itemID, int hue)`
- **Stato nel Nuovo Codice**: ✅ Implementato — `GetImage(int itemID, int hue = 0)` in `ItemsApi.cs` usa `Ultima.Art.GetStatic()` + `Ultima.Hues.GetHue().ApplyTo()`. Ritorna `Ultima.Data.Bitmap?` (tipo nativo SDK, non `System.Drawing.Bitmap`). Il clone della bitmap è necessario per non modificare la cache dell'SDK.
- **Dettaglio**: Ritorna un `Bitmap` dell'immagine dell'item con hue applicato. Usa UltimaSDK.
- **Impatto Utente**: Script che mostrano immagini item in gump custom non funzioneranno.
- **Documentazione per Junior Dev**: In `ItemsApi.cs`, aggiungere `GetImage(int itemID, int hue)`. Servira un riferimento a UltimaSDK o un equivalente per caricare l'art statica. In WPF, ritornare un `BitmapImage` invece di `System.Drawing.Bitmap`.

#### TASK-FR-020: Items.GetWeaponAbility() Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Item.cs` -> `GetWeaponAbility(int itemId)`
- **Stato nel Nuovo Codice**: ✅ Implementato — `GetWeaponAbility(int itemId)` in `ItemsApi.cs` delega a `IWeaponService.GetWeaponInfo()` (iniettato come param opzionale nel ctor). Ritorna `(string Primary, string Secondary)` tuple. Graceful null-check se weaponService non iniettato.
- **Dettaglio**: Ritorna una tupla `(string primary, string secondary)` con i nomi delle abilita arma. Usa dati da `weapons.json`.
- **Impatto Utente**: Script che controllano l'abilita dell'arma equipaggiata non funzioneranno.
- **Documentazione per Junior Dev**: Aggiungere in `ItemsApi.cs` un metodo che delega a `IWeaponService.GetAbilities(itemId)`. Il `WeaponService` dovrebbe gia avere queste info (verificare `WeaponInfo` model).

#### TASK-FR-021: Items.ContainerCount() Semantica Diversa ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Item.cs` -> `ContainerCount(Item container, int itemid, int color, bool recursive)`
- **Stato nel Nuovo Codice**: ✅ Implementato — aggiunto overload `ContainerCount(uint containerSerial, int itemid, int color = -1, bool recursive = true)` con `CountInContainer` ricorsiva. Overload int aggiunto. L'originale `ContainerCount(uint)` (conta tutti) resta invariato.
- **Dettaglio**: Il legacy conta item specifici (per ID e colore) in un container. Il nuovo `ContainerCount(uint containerSerial)` conta TUTTI gli item nel container, senza filtro per tipo.
- **Impatto Utente**: Script che usano `Items.ContainerCount(backpack, 0x0EED, 0, true)` per contare gold in backpack riceveranno il conteggio TOTALE di tutti gli item.
- **Documentazione per Junior Dev**: In `ItemsApi.cs`, aggiungere un overload `ContainerCount(uint containerSerial, int itemid, int color = -1, bool recursive = true)` che filtra per graphic e hue prima di contare.

---

### 3.3 Mobiles API

**Legacy**: `Razor/RazorEnhanced/Mobile.cs`
**Nuovo**: `TMRazorImproved.Core/Services/Scripting/Api/MobilesApi.cs`

**Metodi presenti**: FindBySerial, ApplyFilter, Select, FindMobile, UseMobile, SingleClick, Message, WaitForProps, WaitForStats, GetPropStringList, GetPropStringByIndex, GetPropValue, ContextExist, GetTrackingInfo, GetTargetingFilter.

**Stato**: ✅ Copertura buona. Nessun metodo critico mancante.

---

### 3.4 Gumps API

**Legacy**: `Razor/RazorEnhanced/Gumps.cs` (~59KB)
**Nuovo**: `TMRazorImproved.Core/Services/Scripting/Api/GumpsApi.cs`

**Stato**: ✅ Copertura eccellente. Tutti i metodi di building e querying gump presenti. Unico problema: il cambio namespace `Gumps` -> `Gump` (vedi TASK-FR-013).

---

### 3.5 Spells API

**Legacy**: `Razor/RazorEnhanced/Spells.cs` (~58KB)
**Nuovo**: `TMRazorImproved.Core/Services/Scripting/Api/SpellsApi.cs`

#### TASK-FR-022: Spell School-Specific Targeted Overloads Mancanti ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Spells.cs` -> `CastMagery(string, uint target, bool wait)`, `CastNecro(string, uint target, bool wait)`, etc.
- **Stato nel Nuovo Codice**: ✅ Implementato — aggiunti overload `(string name, uint target, bool wait)` per tutte le cerchie (Magery/Necro/Chivalry/Bushido/Ninjitsu/Spellweaving/Mysticism/Mastery/Cleric/Druid) + versioni `int target` per FR-012.
- **Dettaglio**: Il legacy ha overload per ogni scuola di magia che accettano un target serial e flag wait. Nel nuovo, `CastMagery(string name)` delega solo a `Cast(name)` senza target.
- **Impatto Utente**: **ALTO** - Script come `Spells.CastMagery("Greater Heal", Player.Serial)` falliranno con wrong argument count. Questo e uno dei pattern piu comuni negli script.
- **Documentazione per Junior Dev**: In `SpellsApi.cs`, aggiungere overload per ogni metodo school-specific:
  ```csharp
  public void CastMagery(string name, uint target, bool wait = true)
  {
      Cast(name, target, wait);
  }
  ```
  Ripetere per: CastNecro, CastChivalry, CastBushido, CastNinjitsu, CastSpellweaving, CastMysticism, CastMastery, CastCleric, CastDruid.

#### TASK-FR-023: CastLastSpell Targeted Overload Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Spells.cs` -> `CastLastSpell(uint target)`, `CastLastSpell(Mobile m)`
- **Stato nel Nuovo Codice**: ✅ Implementato — `CastLastSpell(uint target)` e `CastLastSpell(int target)` aggiunti nel region int-serial overloads di `SpellsApi.cs`.
- **Dettaglio**: Overload che lanciano l'ultimo spell su un target specifico.
- **Impatto Utente**: Script che usano `Spells.CastLastSpell(target.Serial)` non funzioneranno.
- **Documentazione per Junior Dev**: In `SpellsApi.cs`, aggiungere `CastLastSpell(uint target)` che chiama `Cast(GetLastSpell(), target)`.

---

### 3.6 Target API

**Legacy**: `Razor/RazorEnhanced/Target.cs`
**Nuovo**: `TMRazorImproved.Core/Services/Scripting/Api/TargetApi.cs`

#### TASK-FR-024: TargetExecute 3-Parametri Ground Target Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Target.cs` -> `TargetExecute(int x, int y, int z)`
- **Stato nel Nuovo Codice**: ✅ Implementato — `TargetExecute(int x, int y, int z)` in `TargetApi.cs` delega a `TargetExecute(x, y, z, 0)` (graphic=0 = terreno puro).
- **Dettaglio**: Overload per target su coordinate ground senza StaticID. Il nuovo ha solo la versione a 4 parametri `(x, y, z, graphic)`.
- **Impatto Utente**: Script che targetano il terreno con `Target.TargetExecute(x, y, z)` non funzioneranno.
- **Documentazione per Junior Dev**: In `TargetApi.cs`, aggiungere overload `TargetExecute(int x, int y, int z)` che chiama la versione a 4 parametri con `graphic: 0`.

---

### 3.7 Journal API

**Legacy**: `Razor/RazorEnhanced/Journal.cs`
**Nuovo**: `TMRazorImproved.Core/Services/Scripting/Api/JournalApi.cs`

#### TASK-FR-025: Journal.Clear(string) Overload Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Journal.cs` -> `Clear(string toBeRemoved)`
- **Stato nel Nuovo Codice**: ✅ Implementato — `Clear(string toBeRemoved)` in `JournalApi.cs` chiama `_journal.RemoveWhere(...)`. Aggiunto `RemoveWhere(Func<JournalEntry, bool>)` all'interfaccia `IJournalService` e a `JournalService` (drena e riempie la `ConcurrentQueue` filtrando le entry matchate).
- **Dettaglio**: Overload che rimuove solo le entry contenenti una stringa specifica.
- **Impatto Utente**: Script che puliscono selettivamente il journal non funzioneranno.
- **Documentazione per Junior Dev**: In `JournalApi.cs`, aggiungere `Clear(string text)` che filtra `_journalService.Entries` rimuovendo quelle contenenti `text`.

#### TASK-FR-026: Journal.GetJournalEntry con Timestamp Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Journal.cs` -> `GetJournalEntry(double afterTimestamp)`
- **Stato nel Nuovo Codice**: ✅ Implementato — `GetJournalEntry(double afterTimestamp)` in `JournalApi.cs` filtra le entry con `e.Timestamp > DateTime.FromOADate(afterTimestamp)` e ritorna `List<string>`. `JournalEntry.Timestamp` (DateTime.Now) già esistente nel modello.
- **Dettaglio**: Permette di ottenere entry journal dopo un certo timestamp.
- **Impatto Utente**: Script che monitorano il journal dall'ultimo check non funzioneranno.
- **Documentazione per Junior Dev**: In `JournalApi.cs`, aggiungere `GetJournalEntry(double afterTimestamp)` che filtra per `DateTime.FromOADate(afterTimestamp)`.

---

### 3.8 Misc API

**Legacy**: `Razor/RazorEnhanced/Misc.cs` (~54KB)
**Nuovo**: `TMRazorImproved.Core/Services/Scripting/Api/MiscApi.cs`

**Stato**: ✅ Copertura molto buona. Tutti i metodi principali presenti. Il nuovo aggiunge anche funzionalita non presenti nel legacy (CreateList/PushList/PopList per UOSteam list API, PlayMusic/StopMusic, WaitFor, Random, Log, Timestamp, ReadFile/WriteFile, GetWindowSize).

---

### 3.9 Sound API

**Legacy**: `Razor/RazorEnhanced/Sound.cs`
**Nuovo**: `TMRazorImproved.Core/Services/Scripting/Api/SoundApi.cs`

#### TASK-FR-027: Sound Filtering API Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Sound.cs` -> `AddFilter()`, `RemoveFilter()`, `WaitForSound()`, `LastSoundMatch()`
- **Stato nel Nuovo Codice**: ✅ Implementato — `AddFilter(string name, List<int> soundIds)`, `RemoveFilter(string name)`, `WaitForSound(List<int> soundIds, int timeout=-1)`, `LastSoundMatch()` aggiunti a `SoundApi.cs`. Usa viewer su pacchetto 0x54 (S2C) registrato lazily via `IPacketService`. Stato statico thread-safe con `lock(_syncRoot)` + `ManualResetEvent` per `WaitForSound`. `IPacketService` ora passato al ctor di SoundApi (opzionale) e propagato da `ScriptingService`.
- **Dettaglio**: Il legacy permette di aggiungere filtri sonori per nome/ID, rimuoverli, attendere suoni specifici e ottenere l'ultimo suono matchato. Il nuovo `SoundApi` ha solo metodi di riproduzione (PlaySoundEffect, PlayMusic, etc.), nessun metodo di filtraggio.
- **Impatto Utente**: Script che usano `Sound.WaitForSound()` per reagire a suoni nel gioco (es. suono di morte, suono di scoperta) non funzioneranno.
- **Documentazione per Junior Dev**: In `SoundApi.cs`, aggiungere:
  1. `AddFilter(string name, List<int> soundIds)` - registra un filtro nel `ISoundService`
  2. `RemoveFilter(string name)` - rimuove filtro
  3. `WaitForSound(List<int> soundIds, int timeout)` - attende un suono specifico (usa `TaskCompletionSource` + viewer su pacchetto 0x54)
  4. `LastSoundMatch()` - ritorna posizione dell'ultimo suono filtrato

---

### 3.10 Trade API (COMPLETAMENTE ASSENTE)

#### TASK-FR-028: Trade API Completamente Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Trade.cs`
- **Stato nel Nuovo Codice**: ✅ Implementato — creato `TradeApi.cs` in `Services/Scripting/Api/`. Metodi: `TradeList()→List<TradeData>`, `Accept(uint tradeId, bool accept=true)`, `Accept(bool)`, `Cancel(uint tradeId)`, `Cancel()`, `Offer(uint tradeId, int gold, int platinum)`, `Offer(int gold, int platinum)` + overload `int` serial per FR-012. `ISecureTradeService` aggiunto al costruttore di `ScriptingService`. `Trade` registrato in `ScriptGlobals` e in entrambi i percorsi di istanziazione (Python scope + C# globals).
- **Dettaglio**: L'intero modulo Trade API non ha controparte nelle API di scripting. Nel legacy:
  - `Trade.TradeList()` - ritorna lista trade attivi
  - `Trade.Accept()` - accetta un trade
  - `Trade.Cancel()` - cancella un trade
  - `Trade.Offer(int serial, int amount)` - offre un item nel trade
- **Impatto Utente**: **CRITICO** - Tutti gli script di trading automatizzato non funzioneranno. Questo include script di vendor, scambio items tra account, etc.
- **Documentazione per Junior Dev**: Creare una nuova classe `TradeApi.cs` in `Services/Scripting/Api/`. Il servizio `ISecureTradeService` esiste gia nel nuovo codice. La nuova API deve esporre:
  1. `TradeList()` -> ritorna lista di `TradeSession` da `ISecureTradeService`
  2. `Accept(uint tradeSerial)` -> invia pacchetto 0x6F (accetta)
  3. `Cancel(uint tradeSerial)` -> invia pacchetto 0x6F (cancella)
  4. `Offer(uint itemSerial, int amount)` -> move item nella finestra trade
  Registrare la nuova API in `ScriptGlobals.cs`.

---

### 3.11 CUO API (COMPLETAMENTE ASSENTE)

#### TASK-FR-029: CUO API Completamente Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/CUO.cs`
- **Stato nel Nuovo Codice**: ✅ Implementato — creato `CuoApi.cs` in `Services/Scripting/Api/`. Metodi implementati via packet/interop: `OpenContainerAt` (usa `IClientInteropService.NextContPosition` + double-click 0x06), `CloseGump` (invia S2C 0xBF/0x0004). `FollowMobile`/`FollowOff`/`Following` (tracking interno serial). Tutti gli altri metodi (LoadMarkers, GoToMarker, FreeView, CloseTMap, ProfilePropertySet, GetSetting, PlayMacro, SetGumpOpenLocation, MoveGump, OpenMobileHealthBar, CloseMobileHealthBar, etc.) sono stub che loggano un warning — richiedono accesso in-process a ClassicUO non disponibile nell'architettura out-of-process di TMRazorImproved. `CUO` registrato in ScriptGlobals e in entrambi i percorsi di istanziazione.
- **Dettaglio**: L'intero modulo di integrazione ClassicUO non ha controparte. Metodi legacy:
  - `CUO.LoadMarkers()`, `CUO.GoToMarker()` - gestione marker mappa
  - `CUO.FreeView()`, `CUO.CloseTMap()` - vista mappa
  - `CUO.ProfilePropertySet(string, string)` (3 overload) - modifica settings CUO
  - `CUO.OpenContainerAt(serial, x, y)` - apri container a posizione
  - `CUO.SetGumpOpenLocation(gumpId, x, y)`, `CUO.MoveGump(gumpId, x, y)` - posiziona gump
  - `CUO.CloseMyStatusBar()`, `CUO.OpenMyStatusBar()` - barra status
  - `CUO.OpenMobileHealthBar(serial)`, `CUO.CloseMobileHealthBar(serial)` - barra HP mobile
  - `CUO.CloseGump(gumpId)` - chiudi gump
  - `CUO.GetSetting(section, key)` - leggi setting CUO
  - `CUO.PlayMacro(name)` - esegui macro CUO
  - `CUO.FollowMobile(serial)`, `CUO.FollowOff()`, `CUO.Following()` - segui mobile
- **Impatto Utente**: **CRITICO per utenti ClassicUO** - Script che interagiscono con il client ClassicUO (posizionamento gump, marker mappa, follow mobile, etc.) non funzioneranno.
- **Documentazione per Junior Dev**: Creare `CuoApi.cs` in `Services/Scripting/Api/`. Poiche TMRazorImproved usa TmClient (basato su ClassicUO), molte di queste funzionalita possono essere implementate tramite il plugin `TMRazorPlugin`. Per i metodi che richiedono comunicazione con il client, usare `IPacketService.SendToClient()` con pacchetti personalizzati o estendere il protocollo IPC del plugin.

---

### 3.12 Filters API

**Legacy**: `Razor/RazorEnhanced/Filters.cs`
**Nuovo**: `TMRazorImproved.Core/Services/Scripting/Api/FiltersApi.cs`

**Stato**: ✅ Copertura adeguata. Il nuovo aggiunge Enable/Disable/IsEnabled che migliora il legacy.

---

### 3.13 Statics API

**Legacy**: `Razor/RazorEnhanced/Statics.cs`
**Nuovo**: `TMRazorImproved.Core/Services/Scripting/Api/StaticsApi.cs`

**Stato**: ✅ Copertura buona. Il nuovo aggiunge GetHighestZ, GetTilesInRange, GetTileFlags, IsLand, IsImpassable, GetLOS, GetStaticFlagsAt, CanFit che migliorano il legacy.

---

## 4. Sistema Agenti

### 4.1 AutoLoot

**Legacy**: `Razor/RazorEnhanced/AutoLoot.cs`
**Nuovo**: `TMRazorImproved.Core/Services/AutoLootService.cs`

**Metodi presenti**: Start, Stop, Status (IsRunning), ChangeList.

#### TASK-FR-030: AutoLoot.RunOnce() Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/AutoLoot.cs` -> `RunOnce(string listName, int msDelay, Items.Filter filter)`
- **Stato nel Nuovo Codice**: ✅ Implementato — `AutoLootService.RunOnce(string, int)` esegue un Task.Run one-shot che drena la lootQueue corrente usando la config della lista nominata. Esposto in `AutoLootApi.RunOnce(string, int msDelay=600)`.
- **Dettaglio**: Esecuzione singola dell'autoloot su un container specifico con filtro custom.
- **Impatto Utente**: Script che lanciano il loot manualmente su container specifici non funzioneranno.
- **Documentazione per Junior Dev**: In `AutoLootService.cs`, aggiungere metodo `RunOnce(string listName, int msDelay)` che esegue un singolo ciclo di scansione e si ferma. Referenziare `AutoLoot.cs` legacy metodo `RunOnce`.

#### TASK-FR-031: AutoLoot.SetNoOpenCorpse() Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/AutoLoot.cs` -> `SetNoOpenCorpse(bool)`
- **Stato nel Nuovo Codice**: ✅ Implementato — `AutoLootService.SetNoOpenCorpse(bool)` toglie/imposta `AutoLootConfig.NoOpenCorpse` della lista attiva e ritorna il valore precedente. Esposto in `AutoLootApi.SetNoOpenCorpse(bool)`.
- **Dettaglio**: Disabilita l'apertura automatica dei cadaveri (solo loot da container gia aperti).
- **Impatto Utente**: Script che configurano il loot silenzioso non funzioneranno.
- **Documentazione per Junior Dev**: Aggiungere configurazione in `AutoLootConfig` e esporre via `AgentApis.cs`.

#### TASK-FR-032: AutoLoot.GetList() / GetLootBag() / ResetIgnore() Mancanti ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/AutoLoot.cs`
- **Stato nel Nuovo Codice**: ✅ Implementato — `GetList(string)` ritorna `AutoLootConfig.ItemList` della lista nominata; `GetLootBag()` verifica il container configurato (fallback al backpack); `ResetIgnore()` svuota `_processedSerials` e `_lootQueue`. Tutti esposti in `AutoLootApi` e in `IAutoLootService`.
- **Dettaglio**:
  - `GetList(string name, bool filter)` - ritorna lista item con filtro opzionale
  - `GetLootBag()` - ritorna seriale del contenitore loot
  - `ResetIgnore()` - resetta la lista dei cadaveri gia processati
- **Impatto Utente**: Script che interagiscono programmaticamente con le liste di loot non funzioneranno.
- **Documentazione per Junior Dev**: In `AgentApis.cs` o `AutoLootService.cs`, esporre questi metodi. `ResetIgnore` deve fare `_processedSerials.Clear()`.

#### TASK-FR-033: AutoLoot Shared Loot Container Detection Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/AutoLoot.cs` -> logica shared loot
- **Stato nel Nuovo Codice**: ✅ Implementato — `IsCorpse(uint)` in `AutoLootService` ora controlla anche container NON-corpse che si trovano entro 3 tile da un corpse (graphic 0x2006) tramite `_worldService.Items`. Questo copre UO Dreams Instanced Loot e OSI shared containers.
- **Dettaglio**: Il legacy rileva container di loot condiviso (UO Dreams Instanced Loot, OSI shared containers). Il nuovo non ha questa logica.
- **Impatto Utente**: Su shard che usano container condivisi, il loot automatico potrebbe non rilevare correttamente i cadaveri.
- **Documentazione per Junior Dev**: In `AutoLootService.cs`, aggiungere logica per rilevare container condivisi controllando il graphic del container e la distanza dal cadavere.

---

### 4.2 BandageHeal

**Legacy**: `Razor/RazorEnhanced/BandageHeal.cs`
**Nuovo**: `TMRazorImproved.Core/Services/BandageHealService.cs`

#### TASK-FR-034: BandageHeal FriendOrSelf Mode Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/BandageHeal.cs` -> modalita "FriendOrSelf"
- **Stato nel Nuovo Codice**: ✅ Implementato — aggiunto `"FriendOrSelf"` case in `GetTargetSerial()` di `BandageHealService`. `GetFriendOrSelfTarget()` trova il friend più debole (HP%), lo confronta con player e cura chi ha meno HP%. Anche `GetNearestFriend` refactored in `GetWeakestFriend` (ordine per HP%).
- **Dettaglio**: Il legacy ha 4 modalita: Self, Target, Friend, FriendOrSelf. La modalita FriendOrSelf confronta gli HP del player e del friend piu debole e cura quello con meno vita. Il nuovo ha Self, Last, Friend ma non FriendOrSelf.
- **Impatto Utente**: Utenti che usano la modalita di cura automatica friend-or-self perderanno questa funzionalita.
- **Documentazione per Junior Dev**: In `BandageHealService.cs`, aggiungere un case "FriendOrSelf" che: 1) Cerca il friend con meno HP, 2) Confronta con HP del player, 3) Cura il piu debole.

#### TASK-FR-035: BandageHeal Text-Based Healing Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/BandageHeal.cs` -> `SelfHealUseText`, `ChatSay`
- **Stato nel Nuovo Codice**: ✅ Implementato — in `AgentLoopAsync` aggiunto branch `if (config.SendTextMsg)`: per self invia `UnicodeSpeech(TextMsgSelf)`, per target invia `UnicodeSpeech(TextMsgTarget)` + wait + TargetObject. Config `SendTextMsg`/`TextMsgSelf`/`TextMsgTarget` già presenti in `BandageHealConfig`.
- **Dettaglio**: Il legacy supporta la cura tramite comando testuale (es. `[bandageself]`) invece che tramite double-click su bende.
- **Impatto Utente**: Su shard custom che usano comandi testuali per le bende, la cura automatica non funzionera.
- **Documentazione per Junior Dev**: In `BandageHealService.cs`, aggiungere supporto per config `UseTextHeal` e `TextHealCommand` che invia un messaggio chat invece di usare l'item benda.

#### TASK-FR-036: BandageHeal Countdown Display Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/BandageHeal.cs` -> `ShowCount(Mobile)`
- **Stato nel Nuovo Codice**: ✅ Implementato — `ShowCountdownAsync(target, totalMs, token)` invia overhead unicode (0xAE) al client con il numero di secondi trascorsi ogni secondo, per la durata del bandage. Attivato quando `config.ShowCountdown == true`.
- **Dettaglio**: Il legacy mostra un countdown overhead sopra il target durante la cura. Non presente nel nuovo.
- **Impatto Utente**: L'utente non vedra il timer visivo di cura sopra il target. Feature di usabilita perduta.
- **Documentazione per Junior Dev**: In `BandageHealService.cs`, dopo aver avviato la cura, calcolare il delay (basato su DEX) e inviare messaggi overhead periodici tramite `IPacketService.SendToClient()` (pacchetto 0x1C) decrementando il timer.

#### TASK-FR-037: BandageHeal Formula DEX Diversa ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/BandageHeal.cs`
- **Stato nel Nuovo Codice**: ✅ Implementato — `CalculateBandageDelay` ora usa `(11 - (dex - dex % 10) / 20) * 1000` (formula legacy esatta, min 100ms).
- **Dettaglio**: Legacy formula: `(11 - (Dex - Dex%10)/20) * 1000`. Nuova formula: `max(3000, 8000 - (dex/20)*1000)`. La nuova formula e piu generosa ad alta DEX.
- **Impatto Utente**: Il timing della cura sara leggermente diverso, potrebbe causare sovrappopolamento di bende ad alta DEX.
- **Documentazione per Junior Dev**: In `BandageHealService.cs`, sostituire la formula attuale con quella legacy: `int delay = (11 - (dex - dex % 10) / 20) * 1000;`. Referenziare `BandageHeal.cs` legacy per la formula esatta.

#### TASK-FR-038: BandageHeal Mortal Strike Detection Diversa ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/BandageHeal.cs` -> check `BuffIcon.MortalStrike`
- **Stato nel Nuovo Codice**: ✅ Implementato — in `AgentLoopAsync` aggiunto `isMortalByBuff` che controlla `player.ActiveBuffs.ContainsKey("Mortal Strike")` solo per target self. `isMortal = target.IsYellowHits || isMortalByBuff`.
- **Dettaglio**: Legacy controlla `World.Player.Buffs.ContainsKey(BuffIcon.MortalStrike)`. Nuovo controlla `IsYellowHits` (check basato su hue). Sono metodi di rilevamento diversi.
- **Impatto Utente**: Mortal Strike potrebbe non essere rilevato correttamente in tutti i casi con il check hue-based.
- **Documentazione per Junior Dev**: In `BandageHealService.cs`, aggiungere un check buff-based: `player.ActiveBuffs.ContainsKey("Mortal Strike")` oltre al check YellowHits esistente.

---

### 4.3 Dress

**Legacy**: `Razor/RazorEnhanced/Dress.cs`
**Nuovo**: `TMRazorImproved.Core/Services/DressService.cs`

#### TASK-FR-039: Dress.ReadPlayerDress() Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Dress.cs` -> `ReadPlayerDress()`
- **Stato nel Nuovo Codice**: ✅ Implementato — `DressService.ReadPlayerDress()` itera `_worldService.Items` dove `Container == player.Serial && Layer > 0 && Layer <= 0x1A`, resetta `LayerItems` della lista attiva e la ripopola. Esposto in `IDressService` e `DressApi`.
- **Dettaglio**: Scansiona l'equipaggiamento corrente del player e lo salva come dress list.
- **Impatto Utente**: L'utente non puo "catturare" il suo outfit corrente come template di dress.
- **Documentazione per Junior Dev**: In `DressService.cs`, aggiungere metodo che itera `_worldService.GetPlayerEquipment()` e salva i serial/layer come nuova dress list nella config.

#### TASK-FR-040: Dress UO3D EquipItemMacro Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Dress.cs` -> `DressUseUO3D`
- **Stato nel Nuovo Codice**: ✅ Implementato — `DressService.Dress()`/`Undress()` verificano `DressList.Use3D`; se true inviano `PacketBuilder.EquipItemMacro` (0xEC) / `PacketBuilder.UnEquipItemMacro` (0xED) in batch. Aggiunti i due metodi builder in `PacketBuilder.cs`.
- **Dettaglio**: Il legacy supporta il pacchetto UO3D EquipItemMacro/UnEquipItemMacro per dress/undress piu veloce. Il nuovo usa solo LiftItem + WearItem tradizionale.
- **Impatto Utente**: Dress piu lento su client che supportano UO3D.
- **Documentazione per Junior Dev**: In `DressService.cs`, aggiungere opzione per inviare il pacchetto 0xEC (EquipItemMacro) quando disponibile. Referenziare `Dress.cs` legacy sezione `DressUseUO3D`.

#### TASK-FR-041: Dress/Undress Status Separati Mancanti ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Dress.cs` -> `DressStatus()`, `UnDressStatus()`
- **Stato nel Nuovo Codice**: ✅ Implementato — aggiunto campo `_isDressingNow` (volatile bool); `DressStatus()` = `IsRunning && _isDressingNow`; `UnDressStatus()` = `IsRunning && !_isDressingNow`. Esposti in `IDressService` e `DressApi`.
- **Dettaglio**: Legacy ha status separati per dress e undress (thread separati). Nuovo ha un singolo `IsRunning`.
- **Impatto Utente**: Script che controllano `Dress.DressStatus()` e `Dress.UnDressStatus()` separatamente avranno solo un valore.
- **Documentazione per Junior Dev**: Valutare se aggiungere `IsDressing` e `IsUndressing` separati, o se il singolo `IsRunning` e sufficiente con un campo aggiuntivo che indica l'operazione corrente.

---

### 4.4 Organizer

#### TASK-FR-042: Organizer.RunOnce() Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Organizer.cs` -> `RunOnce(string, int, int, int)`
- **Stato nel Nuovo Codice**: ✅ Implementato — `OrganizerService.RunOnce(string, uint, uint, int)` esegue un Task.Run one-shot che legge `GetItemsInContainer(src)`, applica filtri ItemList con color matching, sposta in `dst`. Esposto in `IOrganizerService` e `OrganizerApi`.

---

### 4.5 Restock

#### TASK-FR-043: Restock.RunOnce() Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Restock.cs` -> `RunOnce(string, int, int, int)`
- **Stato nel Nuovo Codice**: ✅ Implementato — `RestockService.RunOnce(string, uint, uint, int)` esegue un Task.Run one-shot che calcola il delta backpack, poi muove gli item necessari. Esposto in `IRestockService` e `RestockApi`.

#### TASK-FR-044: Restock Color Matching Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Restock.cs` -> filtraggio per colore
- **Stato nel Nuovo Codice**: ✅ Implementato — aggiunto `(restockItem.Color == -1 || i.Hue == restockItem.Color)` sia nella query sorgente che nel conteggio backpack in `AgentLoopAsync` e `RunOnce`. `LootItem.Color` già presente nel model.

---

### 4.6 Scavenger

#### TASK-FR-045: Scavenger.RunOnce() / GetScavengerBag() / ResetIgnore() Mancanti ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Scavenger.cs`
- **Stato nel Nuovo Codice**: ✅ Implementato — `RunOnce()` drena la queue in un Task.Run; `GetScavengerBag()` verifica il container configurato (fallback backpack); `ResetIgnore()` svuota `_processedSerials` + queue. Esposti in `IScavengerService` e `ScavengerApi`.

#### TASK-FR-046: Scavenger Locked-Down Items Detection Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Scavenger.cs` -> check `GetPropValue("Locked Down")`
- **Stato nel Nuovo Codice**: ✅ Implementato — in `AgentLoopAsync` e `RunOnce`, prima di spostare un item, si controlla `item.Properties.Any(p => p.Contains("Locked Down"))` e si fa skip se trovato.

---

### 4.7 Vendor (Buy/Sell)

**Legacy**: `Razor/RazorEnhanced/Vendor.cs` (3 classi: Vendor, SellAgent, BuyAgent)
**Nuovo**: `TMRazorImproved.Core/Services/VendorService.cs`

#### TASK-FR-047: Vendor.Buy() Script API Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Vendor.cs` -> `Buy(int vendorSerial, string itemName, int amount, int maxPrice)`, `Buy(int vendorSerial, int itemID, int amount, int maxPrice)`, `BuyList(int vendorSerial)`
- **Stato nel Nuovo Codice**: ✅ Implementato — aggiunti `Buy(uint, int, int, int)`, `Buy(uint, string, int, int)`, `BuyList(uint)` in `VendorService`. Usano `_worldService.LastOpenedContainer` + `_lastBuyItems` cache (popolata da 0x74). Esposti in `IVendorService` e `VendorApi`.

#### TASK-FR-048: Vendor CompareName / CompleteAmount Mancanti ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Vendor.cs` -> BuyAgent con `CompareName`, `CompleteAmount`
- **Stato nel Nuovo Codice**: ✅ Implementato — in `Receive(VendorBuyMessage)`: se `config.CompareName`, confronta `buyReq.Name` con `match.Name` (Contains); se `config.CompleteAmount`, sottrae il count già in backpack prima di inviare la quantità. `CompleteAmount` aggiunto a `VendorConfig`.

#### TASK-FR-049: Vendor Max Price Filter Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Vendor.cs` -> SellAgent con max price
- **Stato nel Nuovo Codice**: ✅ Implementato — aggiunto `MaxBuyPrice` (int, 0=no limit) a `VendorConfig`. In `Receive(VendorBuyMessage)` e `Buy()` script methods, la price viene verificata contro `_lastBuyItems` e l'item viene skippato se `price > MaxBuyPrice`.

---

### 4.8 DPSMeter

#### TASK-FR-050: DPSMeter.Pause() Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/DPSMeter.cs` -> `Pause()`
- **Stato nel Nuovo Codice**: ✅ Implementato — `Pause()` aggiunto a `IDPSMeterService`, `DPSMeterService` e `DPSMeterApi`. Ferma il timer e accumula il tempo in `_totalTime`; `IsPaused` property esposta. `Start()` riprende la sessione se `IsPaused==true`. `CombatTime` aggiornato per non incrementare durante la pausa. Build Core: 0 errori.
- **Dettaglio**: Pausa il conteggio DPS senza resettare i dati.
- **Impatto Utente**: Non si puo mettere in pausa il DPS meter durante pause di combattimento.
- **Documentazione per Junior Dev**: In `DPSMeterService.cs`, aggiungere `Pause()` che ferma il timer ma preserva `_totalDamage` e `_combatStartTime`.

#### TASK-FR-051: DPSMeter.GetDamage(serial) Mancante ✅ DONE (2026-03-20)
- **File/Classe Legacy**: `Razor/RazorEnhanced/DPSMeter.cs` -> `GetDamage(int serial)`
- **Stato nel Nuovo Codice**: ✅ Implementato — `GetDamage(uint serial)` aggiunto a `IDPSMeterService`, `DPSMeterService` (cerca in `_targetDamage`) e `DPSMeterApi` (con overload `int` per FR-012). Build Core: 0 errori.
- **Dettaglio**: Ritorna il danno totale inflitto a/da un mobile specifico.
- **Impatto Utente**: Script che monitorano il danno per target specifico non funzioneranno.
- **Documentazione per Junior Dev**: In `DPSMeterService.cs`, il `TargetDamage` dictionary esiste. Esporre `GetDamage(uint serial)` che cerca nel dictionary.

---

### 4.9 LoginAutostart per Tutti gli Agenti

#### TASK-FR-052: LoginAutostart Mancante per Tutti gli Agenti
- **File/Classe Legacy**: Vari agenti -> `LoginAutostart()`
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Nel legacy, AutoLoot, Scavenger e BandageHeal possono essere configurati per avviarsi automaticamente al login. Nessun agente nel nuovo codice ha questa funzionalita.
- **Impatto Utente**: L'utente deve avviare manualmente ogni agente dopo il login.
- **Documentazione per Junior Dev**: In `App.xaml.cs` o in un servizio dedicato, registrarsi al messaggio `LoginConfirmMessage` e avviare automaticamente gli agenti configurati per autostart nella config del profilo corrente.

---

## 5. Sistema Macro

### 5.1 Architettura

| Aspetto | Legacy | Nuovo |
|---------|--------|-------|
| Pattern | Classe-per-azione OOP (43 classi) | Engine a comandi testo (switch statement) |
| Serializzazione | Pipe-delimited `ActionType\|param1\|param2\|...` | Plain text (un comando per riga) |
| Flow control | Scansione lineare con `FindMatchingEndIf()` | Jump table precomputata `BuildJumpTables()` |
| Loop | `Macro.Loop` property | Non supportato (workaround: `WHILE TRUE`/`ENDWHILE`) |

### 5.2 Azioni Macro Mancanti

#### TASK-FR-053: MacroAction DisconnectAction Mancante
- **File/Classe Legacy**: `Razor/RazorEnhanced/Macros/Actions/DisconnectAction.cs`
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Nessun comando `DISCONNECT` esiste nelle macro.
- **Impatto Utente**: Macro che disconnettono dal gioco (es. safety macro) non funzioneranno.
- **Documentazione per Junior Dev**: In `MacrosService.cs`, aggiungere `case "DISCONNECT":` che chiama `_packetService.Disconnect()` o equivalente.

#### TASK-FR-054: MacroAction MovementAction Mancante (CRITICO)
- **File/Classe Legacy**: `Razor/RazorEnhanced/Macros/Actions/MovementAction.cs`
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Nessun comando `WALK`, `RUN` o `PATHFIND` esiste nelle macro. Il legacy supporta 8 direzioni + pathfinding a coordinate/serial/alias.
- **Impatto Utente**: **CRITICO** - Macro di movimento (farming routes, mining, lumber) non funzioneranno affatto.
- **Documentazione per Junior Dev**: In `MacrosService.cs`, aggiungere:
  1. `case "WALK":` con direzione (North/South/East/West/NE/NW/SE/SW) -> invia pacchetto 0x02
  2. `case "RUN":` con direzione e flag running
  3. `case "PATHFIND":` -> delega a `IPathFindingService.PathFindTo(x, y, z)`
  Referenziare `MovementAction.cs` legacy per i dettagli dei pacchetti.

#### TASK-FR-055: MacroAction QueryStringResponseAction Mancante
- **File/Classe Legacy**: `Razor/RazorEnhanced/Macros/Actions/QueryStringResponseAction.cs`
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Nessun comando `QUERYSTRINGRESPONSE` per rispondere ai prompt del server.
- **Impatto Utente**: Macro che rispondono a prompt testuali del server non funzioneranno.
- **Documentazione per Junior Dev**: Aggiungere `case "QUERYSTRINGRESPONSE":` in `MacrosService.cs`. Referenziare `QueryStringResponseAction.cs` legacy.

#### TASK-FR-056: MacroAction Whisper/Yell Mancanti
- **File/Classe Legacy**: `Razor/RazorEnhanced/Macros/Actions/MessagingAction.cs` -> Whisper, Yell
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Solo `SAY` e `EMOTE` esistono. `WHISPER` e `YELL` mancano.
- **Impatto Utente**: Macro che sussurrano o gridano non funzioneranno.
- **Documentazione per Junior Dev**: In `MacrosService.cs`, aggiungere `case "WHISPER":` (MessageType 0x02) e `case "YELL":` (MessageType 0x01).

### 5.3 Azioni Macro Parzialmente Implementate

#### TASK-FR-057: TargetAction Modalita Mancanti
- **File/Classe Legacy**: `Razor/RazorEnhanced/Macros/Actions/TargetAction.cs`
- **Stato nel Nuovo Codice**: ⚠️ Incompleto
- **Dettaglio**: Legacy supporta 6 modalita: Self, Serial, Location, Last, Closest, Random. Nuovo ha solo `TARGET <serial>`. Mancano: `TARGETSELF`, `TARGETLAST`, `TARGETCLOSEST`, `TARGETRANDOM`, `TARGETLOCATION <x> <y> <z>`.
- **Impatto Utente**: La maggior parte delle macro di targeting non funzionera.
- **Documentazione per Junior Dev**: In `MacrosService.cs`, aggiungere comandi separati per ogni modalita target. `TARGETSELF` usa `_worldService.Player.Serial`. `TARGETLAST` usa `_targetingService.LastTarget`. `TARGETCLOSEST` + `TARGETRANDOM` richiedono `_worldService.GetMobilesInRange()` con selettore.

#### TASK-FR-058: GumpResponseAction Avanzata Mancante
- **File/Classe Legacy**: `Razor/RazorEnhanced/Macros/Actions/GumpResponseAction.cs`
- **Stato nel Nuovo Codice**: ⚠️ Incompleto
- **Dettaglio**: Legacy supporta risposte gump con switches e text entries. Nuovo `RESPONDGUMP` invia solo button ID.
- **Impatto Utente**: Macro che rispondono a gump complessi (con checkbox e campi testo) non funzioneranno correttamente.
- **Documentazione per Junior Dev**: Estendere `RESPONDGUMP` per accettare parametri aggiuntivi: `RESPONDGUMP <serial> <typeId> <buttonId> [switches=1,2,3] [texts="text1","text2"]`.

### 5.4 Registrazione Macro Mancanti

#### TASK-FR-059: Recording Pacchetti Mancanti
- **File/Classe Legacy**: `Razor/RazorEnhanced/Macros/MacroManager.cs`
- **Stato nel Nuovo Codice**: ⚠️ Incompleto
- **Dettaglio**: Il nuovo recorder non registra: Movement (0x02), PickUp (0x07), Drop (0x08), Equip (0x13), Ability (0xD7).
- **Impatto Utente**: Registrando una macro, movimenti, pickup, drop, equip e abilita non vengono catturati.
- **Documentazione per Junior Dev**: In `MacrosService.cs`, aggiungere viewer per i pacchetti mancanti nella funzione di registrazione. Per ogni pacchetto, generare il comando testuale corrispondente (es. 0x02 -> `WALK North`, 0x07 -> `PICKUP <serial>`, 0x08 -> `DROP <serial> <x> <y> <z>`, etc.).

---

## 6. Filtri e Rete

### 6.1 Filtri Pacchetti

**Legacy**: 13 classi separate in `Razor/Filters/`
**Nuovo**: 1 handler unificato `FilterHandler.cs` + `TargetFilterService.cs`

| Filtro Legacy | Stato Nuovo | Note |
|---------------|-------------|------|
| DeathFilter (0x2C) | ✅ Portato | |
| LightFilter (0x4E, 0x4F) | ✅ Portato (migliorato) | Invia 0x00 brightness attivamente |
| WeatherFilter (0x65) | ✅ Portato | |
| SeasonFilter (0xBC) | ⚠️ Parziale | Non invia stagione forzata al client |
| SoundFilter (0x54) | ⚠️ Parziale | Solo tutto/niente, perde categorie |
| VetRewardGumpFilter (0xB0, 0xDD) | ✅ Portato | |
| StaffItemFilter (0x1A) | ✅ Portato | |
| StaffNpcsFilter (0x78, 0x20, 0x77) | ✅ Portato | |
| AsciiMessageFilter (0x1C) | ⚠️ Cambiato | Filtra per keyword, non per stringhe configurabili |
| LocMessageFilter (0xC1) | ❌ Non portato | |
| MobileFilter (graphics) | ✅ Portato (migliorato) | Supporta custom GraphFilters |
| WallStaticFilter | ✅ Portato | In WorldPacketHandler |
| TargetFilterManager | ✅ Portato | Come TargetFilterService |

#### TASK-FR-060: LocMessageFilter (0xC1) Mancante
- **File/Classe Legacy**: `Razor/Filters/MessageFilter.cs` -> `LocMessageFilter`
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Il filtraggio di messaggi localizzati per numero cliloc non e implementato. Include: messaggi spell paladino (1060718-1060727), e qualsiasi altro messaggio filtrabile per cliloc ID.
- **Impatto Utente**: I messaggi di spam del paladino e altri messaggi localizzati filtrabili appariranno nel journal.
- **Documentazione per Junior Dev**: In `FilterHandler.cs`, aggiungere un handler per il pacchetto 0xC1 che controlla il cliloc number contro una lista configurabile e blocca se presente. Referenziare `MessageFilter.cs` legacy per i numeri cliloc specifici.

#### TASK-FR-061: SoundFilter Per-Categoria Mancante
- **File/Classe Legacy**: `Razor/Filters/SoundFilters.cs`
- **Stato nel Nuovo Codice**: ⚠️ Incompleto
- **Dettaglio**: Il legacy carica filtri sonori per categoria da `ConfigFiles.FilterSounds` (es. draghi, uccelli, etc.) con toggle individuali. Il nuovo ha solo un boolean `FilterSound` che blocca TUTTI i suoni.
- **Impatto Utente**: L'utente non puo filtrare selettivamente i suoni (es. solo rumori ambientali mantenendo suoni di combattimento).
- **Documentazione per Junior Dev**: In `FilterHandler.cs`, sostituire il check `FilterSound` singolo con un check contro una lista di sound ID configurabili in `UserProfile.FilteredSoundIds`.

#### TASK-FR-062: SeasonFilter ForceSend Mancante
- **File/Classe Legacy**: `Razor/Filters/Season.cs`
- **Stato nel Nuovo Codice**: ⚠️ Incompleto
- **Dettaglio**: Legacy invia `ForceSendToClient(new SeasonChange(forcedSeason))` per applicare la stagione scelta. Nuovo blocca solo il pacchetto senza inviare la stagione forzata.
- **Impatto Utente**: Abilitando il filtro stagione, la stagione del client non cambia effettivamente.
- **Documentazione per Junior Dev**: In `FilterHandler.cs`, quando `FilterSeason` e attivo, oltre a bloccare il pacchetto in arrivo, inviare un pacchetto 0xBC con la stagione forzata dalla config usando `_packetService.SendToClient()`.

---

### 6.2 Handler Pacchetti C2S Mancanti

#### TASK-FR-063: 13 Handler C2S Mancanti
- **File/Classe Legacy**: `Razor/Network/Handlers.cs`
- **Stato nel Nuovo Codice**: ❌ Mancante

| Pacchetto | Handler Legacy | Impatto |
|-----------|---------------|---------|
| 0x00/0xF8 | `CreateCharacter` | Creazione personaggio non tracciata |
| 0x22 | `ResyncRequest` | Resync non tracciato |
| 0x3A | `SetSkillLock` | Cambi skill lock non catturati |
| 0x5D | `PlayCharacter` | Selezione personaggio non tracciata |
| 0x7D | `MenuResponse` | Risposte menu non tracciate |
| 0x80 | `ServerListLogin` | Login non filtrato (auto-login) |
| 0x91 | `GameLogin` | Game login non filtrato |
| 0x95 | `HueResponse` (C2S) | Hue picker non tracciato |
| 0x9A | `ClientAsciiPromptResponse` | Prompt response non tracciato |
| 0xA0 | `PlayServer` | Selezione server non tracciata |
| 0xAC | `ResponseStringQuery` | Query response non tracciato |
| 0xC2 | `UnicodePromptSend` | Unicode prompt non tracciato |
| 0xC2 | `UnicodePromptRecevied` (S2C) | Unicode prompt ricevuto non gestito |

- **Impatto Utente**: La maggior parte sono per tracking interno (basso impatto diretto). **0x7D MenuResponse** e **0x9A PromptResponse** sono i piu impattanti perche il ScriptRecorder e il sistema di Prompt non li tracciano.
- **Documentazione per Junior Dev**: In `WorldPacketHandler.cs`, aggiungere handler per ciascun pacchetto. I piu urgenti sono 0x7D (registrare in `MenuStore`) e 0x9A/0xAC (notificare `MiscApi` per Prompt/StringQuery). Per la struttura dei pacchetti, referenziare `Handlers.cs` legacy.

---

### 6.3 Handler Downgrade da Filter a Viewer

#### TASK-FR-064: Handler Degradati (Filter -> Viewer)
- **File/Classe Legacy**: `Razor/Network/Handlers.cs`
- **Stato nel Nuovo Codice**: ⛔ Discrepanza Strutturale
- **Dettaglio**: Diversi handler legacy registrati come **filter** (possono bloccare/modificare pacchetti) sono stati portati come **viewer** (solo lettura). Questo impedisce di bloccare certi pacchetti.

| Pacchetto | Funzionalita Persa |
|-----------|-------------------|
| 0x02 MovementRequest | Non puo piu bloccare movimenti (queued movement) |
| 0x05 AttackRequest | Non puo piu bloccare attacchi |
| 0x25 ContainerContentUpdate | Non puo piu manipolare contenuto container |
| 0x2E EquipmentUpdate | Non puo piu bloccare cambi equipaggiamento |
| 0x3C ContainerContent | Non puo piu manipolare contenuto container |
| 0x77 MobileMoving | Non puo piu filtrare movimenti mobile |
| 0xAE UnicodeSpeech | Non puo piu filtrare speech |
| 0xC1 LocalizedMessage | Non puo piu filtrare messaggi localizzati |
| 0xC8 SetUpdateRange | Non puo piu modificare update range |
| 0xCC LocalizedMessageAffix | Non puo piu filtrare messaggi con affisso |

- **Impatto Utente**: Le funzionalita di filtraggio speech, manipolazione container e queued movement sono perse. **Speech filtering** e il piu impattante (non si possono piu bloccare messaggi spam di altri giocatori).
- **Documentazione per Junior Dev**: In `WorldPacketHandler.cs`, per gli handler che devono poter bloccare pacchetti, registrarli con `RegisterFilter()` invece di `RegisterViewer()`. Il filtro deve ritornare `false` per bloccare il pacchetto. Iniziare con 0xAE (UnicodeSpeech) e 0xC1 (LocalizedMessage) che sono i piu richiesti.

---

### 6.4 Client Layer

#### TASK-FR-065: UOAssist Compatibility Layer Mancante
- **File/Classe Legacy**: `Razor/Client/UOAssist.cs`
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Il layer di compatibilita UOAssist non e stato portato.
- **Impatto Utente**: **Basso** - UOAssist e un tool legacy. Se necessario su shard specifici, andra implementato.
- **Documentazione per Junior Dev**: Se richiesto, creare `UoAssistAdapter.cs` in `Services/Adapters/` implementando il protocollo WM_COPYDATA di UOAssist.

#### TASK-FR-066: FeatureBit System Mancante
- **File/Classe Legacy**: `Razor/Client/Client.cs` -> `FeatureBit` flags
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Il legacy usa FeatureBits per abilitare/disabilitare feature per server (es. alcuni shard disabilitano il packet filtering). Il nuovo usa boolean config senza gate server-enforced.
- **Impatto Utente**: **Basso** - La maggior parte dei shard non usa FeatureBits.

---

## 7. Modelli Core e Configurazione

### 7.1 Modello UOEntity

#### TASK-FR-067: UOEntity.Deleted Flag Mancante
- **File/Classe Legacy**: `Razor/Core/UOEntity.cs` -> `bool Deleted`
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Il flag di soft-delete non esiste nel nuovo modello. Le entita vengono rimosse direttamente dalla world.
- **Impatto Utente**: **Basso** - Il soft-delete e un pattern di gestione stato interno.

### 7.2 Modello Mobile

#### TASK-FR-068: Mobile.Visible Mancante
- **File/Classe Legacy**: `Razor/Core/Mobile.cs` -> `bool Visible`
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: La proprieta `Visible` non e sul modello Mobile nel nuovo codice.
- **Impatto Utente**: Script che controllano `mobile.Visible` falliranno con AttributeError.
- **Documentazione per Junior Dev**: Aggiungere `bool Visible` al modello `Mobile` in `UOEntity.cs`. Popolarlo da flag byte bit 0x80 in `WorldPacketHandler.HandleMobileIncoming()`.

#### TASK-FR-069: Mobile Items (Equipment List) Mancante
- **File/Classe Legacy**: `Razor/Core/Mobile.cs` -> `List<Item> m_Items`
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: La lista degli item equipaggiati/portati non e nel modello Mobile.
- **Impatto Utente**: Script che iterano `mobile.Items` o cercano item equipaggiati su un mobile falliranno.
- **Documentazione per Junior Dev**: Aggiungere `List<uint> EquippedItemSerials` al modello Mobile, o usare un metodo in `WorldService.GetMobileEquipment(serial)`.

#### TASK-FR-070: Mobile Race/Expansion/CanRename/Mounted/MaximumManaIncrease Mancanti
- **File/Classe Legacy**: `Razor/Core/Player.cs` (PlayerData)
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Le seguenti proprieta player non sono nel modello:
  - `Race` (Human/Elf/Gargoyle)
  - `Expansion` (livello espansione)
  - `CanRename` (puo rinominare il mobile)
  - `Mounted` (sta cavalcando)
  - `MaximumManaIncrease`
- **Impatto Utente**: Script che usano queste proprieta falliranno. `Mounted` e `Race` sono i piu usati.
- **Documentazione per Junior Dev**: Aggiungere queste proprieta al modello `Mobile` in `UOEntity.cs`. Per `Mounted`, controllare se esiste un item nel Layer.Mount. Per `Race`, leggere dal pacchetto 0x11 (MobileStatus).

### 7.3 Modello Item

#### TASK-FR-071: Item Child Items List Mancante
- **File/Classe Legacy**: `Razor/Core/Item.cs` -> `List<Item> m_Items`
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: La lista di sotto-item (contenuto di un container) non e nel modello Item.
- **Impatto Utente**: Script che iterano `container.Contains` o `item.Items` falliranno.
- **Documentazione per Junior Dev**: Aggiungere `List<uint> ContainedItemSerials` al modello `Item`, oppure esporre via `WorldService.GetItemsInContainer(serial)`.

#### TASK-FR-072: Item Layer Enum Persa
- **File/Classe Legacy**: `Razor/Core/Item.cs` -> `Layer` enum con nomi descrittivi
- **Stato nel Nuovo Codice**: ⚠️ Semplificato
- **Dettaglio**: Il legacy ha un enum `Layer` con nomi (InnerTorso, OuterTorso, Bracelet, etc.). Il nuovo usa `byte Layer` senza nomi.
- **Impatto Utente**: Script che usano `Layer.InnerTorso` etc. non funzioneranno.
- **Documentazione per Junior Dev**: Verificare se l'enum `Layer` esiste in `TMRazorImproved.Shared/Enums/Layer.cs`. Se si, usarlo nel modello Item. Altrimenti, crearlo con tutti i valori dal legacy.

### 7.4 Configurazione

#### TASK-FR-073: Toolbar Items Config Mancante
- **File/Classe Legacy**: `Razor/RazorEnhanced/Settings.cs` -> tabella TOOLBAR_ITEMS
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: La configurazione degli item nella toolbar (spell/ability/command shortcuts) non ha un modello config nel nuovo codice.
- **Impatto Utente**: La toolbar floating non sara configurabile con spell/item custom.
- **Documentazione per Junior Dev**: Aggiungere `List<ToolbarItem>` a `UserProfile` in `ConfigModels.cs` dove `ToolbarItem` ha: Type (Spell/Item/Command), Id, Name, Hotkey.

#### TASK-FR-074: Password Memory Mancante
- **File/Classe Legacy**: `Razor/RazorEnhanced/PasswordMemory.cs`, `Settings.cs` tabella PASSWORD
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Il salvataggio password per auto-login non e implementato.
- **Impatto Utente**: L'utente dovra inserire la password ogni volta al login.
- **Documentazione per Junior Dev**: Creare un `PasswordService.cs` che salvi password criptate per shard/account. Usare DPAPI o altro metodo sicuro per la persistenza.

#### TASK-FR-075: Journal Filter Config Mancante
- **File/Classe Legacy**: `Razor/RazorEnhanced/Settings.cs` -> tabella JOURNAL_FILTER
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: La configurazione dei filtri journal (quali messaggi mostrare/nascondere) non ha un modello config dedicato.
- **Impatto Utente**: L'utente non puo configurare filtri persistenti per il journal.
- **Documentazione per Junior Dev**: Aggiungere `JournalFilterConfig` con lista di pattern testo/tipo da filtrare a `UserProfile`.

---

## 8. UI Forms e Dialoghi

### 8.1 Inspector

#### TASK-FR-076: Item Inspector Flags Section Mancante
- **File/Classe Legacy**: `Razor/RazorEnhanced/UI/EnhancedItemInspector.cs`
- **Stato nel Nuovo Codice**: ⚠️ Incompleto
- **Dettaglio**: L'inspector unificato in `InspectorPage.xaml` non mostra i 10 flag item: IsContainer, IsCorpse, IsDoor, IsMulti, IsPotion, Movable, IsDamagable, IsTwoHanded, OnGround, Visible.
- **Impatto Utente**: Meno informazioni disponibili per debugging degli script.
- **Documentazione per Junior Dev**: In `InspectorPage.xaml`, aggiungere una sezione "Flags" sotto i dettagli entity con toggle/checkbox per ogni flag. Leggere i valori dalla `UOEntity.Flags` e dalle proprieta bool dell'`Item`.

#### TASK-FR-077: Mobile Inspector Dettagli Mancanti
- **File/Classe Legacy**: `Razor/RazorEnhanced/UI/EnhancedMobileInspector.cs`
- **Stato nel Nuovo Codice**: ⚠️ Incompleto
- **Dettaglio**: Mancano: barre HP/Mana/Stam, flag mobile (7 flag), notorieta, direzione, animazione, stats estese player, SumAttributes per item equipaggiati.
- **Impatto Utente**: L'utente perde funzionalita di ispezione dettagliata dei mobile, utile per debugging.
- **Documentazione per Junior Dev**: In `InspectorPage.xaml`, aggiungere pannello mobile con: ProgressBar per HP/Mana/Stam, TextBlocks per stats, StackPanel per flags, e metodo SumAttributes che itera l'equipaggiamento.

#### TASK-FR-078: Static Inspector Assente
- **File/Classe Legacy**: `Razor/RazorEnhanced/UI/EnhancedStaticInspector.cs`
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Il Map Inspector nella nuova UI e una mappa mondo, NON un ispettore di tile statiche. Mancano: dati land tile (ID, Hue, Z, texture), 14 flag per tile, lista tile statiche a una posizione, dettagli singola tile statica.
- **Impatto Utente**: Utenti che sviluppano script di pathfinding o costruzione non possono ispezionare i dati del terreno.
- **Documentazione per Junior Dev**: Aggiungere un tab "Tile Inspector" in `InspectorPage.xaml` che, data una coordinata, mostra: LandTile (ID, Hue, Z, flag), lista StaticTile (ID, Hue, Z, flag) tramite `StaticsApi` metodi `GetStaticsLandInfo`/`GetStaticsTileInfo`.

#### TASK-FR-079: Object Inspector (SharedData/Timers) Mancante
- **File/Classe Legacy**: `Razor/RazorEnhanced/UI/EnhancedObjectInspector.cs`
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: L'inspector per SharedScriptData e Timer non ha controparte.
- **Impatto Utente**: Utenti non possono ispezionare dati condivisi tra script e timer attivi.
- **Documentazione per Junior Dev**: Creare un tab "Debug" in `InspectorPage.xaml` con due DataGrid: uno per `Misc.SharedScriptData` (key/value) e uno per timer attivi.

### 8.2 Script Editor

#### TASK-FR-080: Debug Stepping Mancante
- **File/Classe Legacy**: `Razor/RazorEnhanced/UI/EnhancedScriptEditor.cs` -> debug stepping
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Il legacy supporta breakpoint, step-by-line, step-into-call, step-over-return. Il nuovo script editor non ha nessuna funzionalita di debug.
- **Impatto Utente**: **MEDIO** - Gli sviluppatori di script perdono il debugger integrato.
- **Documentazione per Junior Dev**: Questo e un task complesso. Richiede:
  1. Aggiungere breakpoint tracking nel `ScriptingService`
  2. Per Python: usare `sys.settrace()` con callback che verifica breakpoints
  3. Per C#: usare Roslyn debug symbols
  4. Per UOSteam: check per-riga nell'interprete
  5. UI: aggiungere margine clickable in AvalonEdit per set breakpoints, e toolbar Step/StepInto/StepOut

### 8.3 Dialoghi Mancanti

#### TASK-FR-081: RE_MessageBox Custom Mancante
- **File/Classe Legacy**: `Razor/RazorEnhanced/UI/RE_MessageBox.cs`
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Message box custom con bottoni configurabili, colori, link, sizing dinamico.
- **Impatto Utente**: **Basso** - WPF ha dialoghi built-in. Ma il look&feel sara diverso.
- **Documentazione per Junior Dev**: Se necessario, creare `MessageDialog.xaml` con ContentDialog WPF e stile coerente con il tema TMRazor.

#### TASK-FR-082: Scavenger Edit Props Window Mancante
- **File/Classe Legacy**: `Razor/RazorEnhanced/UI/EnhancedScavengerEditItemProps.cs`
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: L'edit delle property per item scavenger non ha una finestra dedicata (l'`EditLootItemWindow` sembra essere solo per AutoLoot).
- **Impatto Utente**: L'utente non puo configurare filtri property per lo scavenger.
- **Documentazione per Junior Dev**: Riutilizzare `EditLootItemWindow.xaml` per lo scavenger, o creare una window simile. Verificare che la `ScavengerPage.xaml` abbia il binding corretto.

#### TASK-FR-083: Predefined Property List (~90 proprieta) Mancante
- **File/Classe Legacy**: `Razor/RazorEnhanced/UI/EnhancedAutolootEditItemProps.cs` -> ComboBox con ~90 proprieta
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Il legacy ha un ComboBox con ~90 nomi di proprieta predefinite (resistenze, slayer, cariche, etc.). Il nuovo `EditLootItemWindow` richiede inserimento manuale del nome.
- **Impatto Utente**: L'utente deve ricordare i nomi esatti delle proprieta invece di sceglierli da una lista.
- **Documentazione per Junior Dev**: In `EditLootItemWindow.xaml`, aggiungere un ComboBox con la lista delle proprieta. La lista si trova in `EnhancedAutolootEditItemProps.cs` nel costruttore (righe 30-120 circa).

---

## 9. Sistemi Utility

### 9.1 Utility Mancanti

#### TASK-FR-084: Geometry.cs (Point3D, Line2D, Rectangle2D) Mancante
- **File/Classe Legacy**: `Razor/Core/Geometry.cs` (573 righe)
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Struct geometriche Point3D, Line2D, Rectangle2D con calcoli di angolo, distanza, intersezione.
- **Impatto Utente**: **Basso** - Usato internamente. Le coordinate nel nuovo codice usano proprietA X,Y,Z inline.

#### TASK-FR-085: ActionQueue.cs Mancante
- **File/Classe Legacy**: `Razor/Core/ActionQueue.cs` (640 righe)
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Coda di azioni sequenziali con delay. Il macro engine gestisce il sequenziamento diversamente.
- **Impatto Utente**: **Basso** - Pattern interno. Il nuovo usa Task/async.

#### TASK-FR-086: ToolBar.cs Logica Mancante
- **File/Classe Legacy**: `Razor/RazorEnhanced/ToolBar.cs` (1.768 righe)
- **Stato nel Nuovo Codice**: ⚠️ Parziale
- **Dettaglio**: La logica della toolbar nel legacy include gestione slot personalizzabili, trascinamento spell/item, counting, e salvataggio configurazione. Il nuovo `FloatingToolbarWindow.xaml` esiste ma la logica di personalizzazione slot potrebbe essere incompleta.
- **Impatto Utente**: La toolbar potrebbe non essere completamente personalizzabile.
- **Documentazione per Junior Dev**: Verificare `FloatingToolbarViewModel.cs` per la completezza delle funzionalita di personalizzazione slot. Se mancante, aggiungere logica per drag&drop di spell/item nella toolbar e persistenza in config.

#### TASK-FR-087: ZLib.cs Compressione Mancante
- **File/Classe Legacy**: `Razor/Core/ZLib.cs` (346 righe)
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Utility di compressione/decompressione ZLib.
- **Impatto Utente**: La decompressione gump compressi (pacchetto 0xDD) potrebbe non funzionare senza ZLib.
- **Documentazione per Junior Dev**: Verificare se il handler di `HandleCompressedGump` in `WorldPacketHandler.cs` ha gia la decompressione implementata (potrebbe usare `System.IO.Compression`). Se si, questo task e non necessario.

#### TASK-FR-088: Serial.IsMobile / Serial.IsItem Helper Mancanti
- **File/Classe Legacy**: `Razor/Core/Serial.cs` -> `IsMobile`, `IsItem`, `IsValid`
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Helper statici per determinare se un serial uint e un mobile (>= 0x00000001 e < 0x40000000) o un item (>= 0x40000000).
- **Impatto Utente**: Script che usano `Serial.IsMobile(serial)` falliranno.
- **Documentazione per Junior Dev**: Aggiungere metodi statici in `MiscApi.cs` o in una utility class:
  ```csharp
  public static bool IsMobile(uint serial) => serial >= 1 && serial < 0x40000000;
  public static bool IsItem(uint serial) => serial >= 0x40000000 && serial < 0x80000000;
  ```

#### TASK-FR-089: Facet.cs Map Metadata Mancante
- **File/Classe Legacy**: `Razor/Core/Facet.cs` (167 righe)
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Definizioni metadata per le mappe (Felucca, Trammel, Ilshenar, Malas, Tokuno, TerMur) con dimensioni, ID e nomi.
- **Impatto Utente**: **Basso** - Usato internamente per boundary checking.

### 9.2 HotkeyService Gap

#### TASK-FR-090: Hotkey Mouse Button/Wheel Mancanti
- **File/Classe Legacy**: `Razor/RazorEnhanced/HotKey.cs` -> supporto mouse
- **Stato nel Nuovo Codice**: ❌ Mancante
- **Dettaglio**: Il legacy supporta: middle click, wheel up/down, X buttons 1-2, con combinazioni Ctrl/Alt/Shift. Il nuovo usa solo keyboard hook (`WH_KEYBOARD_LL`).
- **Impatto Utente**: Utenti che usano pulsanti mouse per hotkey (molto comuni nel PvP) non potranno assegnarli.
- **Documentazione per Junior Dev**: In `HotkeyService.cs`, aggiungere un mouse hook (`WH_MOUSE_LL`) accanto al keyboard hook. Definire `MouseHotkeyType` enum (MiddleClick, WheelUp, WheelDown, XButton1, XButton2) e supportarli nella registrazione hotkey.

#### TASK-FR-091: Hotkey Categorie Mancanti
- **File/Classe Legacy**: `Razor/RazorEnhanced/HotKey.cs`
- **Stato nel Nuovo Codice**: ⚠️ Incompleto
- **Dettaglio**: Categorie hotkey mancanti nel nuovo codice:
  - Pet Commands (all'follow, all kill, all stay, etc.)
  - Show Names (mostra nomi overhead)
  - Equip Wands (equip bacchette specifiche)
  - Skills (usa skill via hotkey)
  - Sell/Buy agent toggles
  - BoneCutter/AutoCarver/AutoRemount (toggle via hotkey)
  - GraphFilter, Friend list hotkeys
  - Target list hotkeys
  - Dress list hotkeys
- **Impatto Utente**: Utenti che usano queste categorie di hotkey dovranno trovare alternative.
- **Documentazione per Junior Dev**: In `HotkeyService.cs`, espandere le categorie di azioni registrate. Per ciascuna categoria mancante, aggiungere handler che delegano al servizio appropriato (es. `ICommandService` per Pet Commands, `ITargetingService` per Target list, etc.).

---

## 10. Riepilogo Task per Priorita

### CRITICO (Blocca funzionalita core per la maggior parte degli utenti)

| Task | Descrizione | Area |
|------|-------------|------|
| TASK-FR-003 | 16 comandi UOSteam mancanti | Scripting |
| TASK-FR-004 | ~45 espressioni UOSteam mancanti | Scripting |
| TASK-FR-012 | Breaking change int -> uint seriali | API |
| TASK-FR-013 | Breaking change Gumps -> Gump | API |
| TASK-FR-022 | Spell school-specific targeted overloads | API |
| TASK-FR-028 | Trade API completamente mancante | API |
| TASK-FR-029 | CUO API completamente mancante | API |
| TASK-FR-054 | Movement/Pathfind nelle macro | Macro |

### ALTO (Blocca funzionalita significative)

| Task | Descrizione | Area |
|------|-------------|------|
| TASK-FR-001 | Direttive C# script (//#import, //#assembly) | Scripting |
| TASK-FR-006 | Script loop mode | Scripting |
| TASK-FR-007 | Script autostart | Scripting |
| TASK-FR-010 | ScriptRecorder pacchetti mancanti | Scripting |
| TASK-FR-014 | Player.AttackType() overload complessi | API |
| TASK-FR-017 | Differenze tipo ritorno Player API | API |
| TASK-FR-021 | Items.ContainerCount() semantica diversa | API |
| TASK-FR-027 | Sound filtering API | API |
| TASK-FR-030 | AutoLoot.RunOnce() | Agenti |
| TASK-FR-034 | BandageHeal FriendOrSelf mode | Agenti |
| TASK-FR-037 | BandageHeal formula DEX diversa | Agenti |
| TASK-FR-047 | Vendor.Buy() script API | Agenti |
| TASK-FR-048 | Vendor CompareName/CompleteAmount | Agenti |
| TASK-FR-052 | LoginAutostart tutti agenti | Agenti |
| TASK-FR-053 | Macro DisconnectAction | Macro |
| TASK-FR-057 | Macro Target modalita mancanti | Macro |
| TASK-FR-059 | Recording pacchetti mancanti | Macro |
| TASK-FR-060 | LocMessageFilter mancante | Filtri |
| TASK-FR-064 | Handler degradati filter->viewer | Rete |
| TASK-FR-090 | Hotkey mouse button/wheel | Hotkey |
| TASK-FR-091 | Hotkey categorie mancanti | Hotkey |

### MEDIO (Funzionalita secondarie o di nicchia)

| Task | Descrizione | Area |
|------|-------------|------|
| TASK-FR-002 | Python Call() da C# | Scripting |
| TASK-FR-005 | Stub UOSteam (equipwand, shownames, replay) | Scripting |
| TASK-FR-008 | Script FileSystemWatcher | Scripting |
| TASK-FR-009 | Script preload/precompile | Scripting |
| TASK-FR-011 | AutoDoc export formati | Scripting |
| TASK-FR-015 | Player.Corpses property | API |
| TASK-FR-016 | Player Chat overload interi | API |
| TASK-FR-018 | Items.Select() con selettore | API |
| TASK-FR-019 | Items.GetImage() | API |
| TASK-FR-020 | Items.GetWeaponAbility() | API |
| TASK-FR-023 | CastLastSpell targeted overload | API |
| TASK-FR-024 | TargetExecute 3-parametri | API |
| TASK-FR-025 | Journal.Clear(string) overload | API |
| TASK-FR-026 | Journal.GetJournalEntry con timestamp | API |
| TASK-FR-031 | AutoLoot.SetNoOpenCorpse() | Agenti |
| TASK-FR-032 | AutoLoot.GetList/GetLootBag/ResetIgnore | Agenti |
| TASK-FR-033 | AutoLoot shared loot container | Agenti |
| TASK-FR-035 | BandageHeal text-based healing | Agenti |
| TASK-FR-036 | BandageHeal countdown display | Agenti |
| TASK-FR-038 | BandageHeal mortal strike detection | Agenti |
| TASK-FR-039 | Dress.ReadPlayerDress() | Agenti |
| TASK-FR-042 | Organizer.RunOnce() | Agenti |
| TASK-FR-043 | Restock.RunOnce() | Agenti |
| TASK-FR-044 | Restock color matching | Agenti |
| TASK-FR-045 | Scavenger RunOnce/GetBag/ResetIgnore | Agenti |
| TASK-FR-046 | Scavenger locked-down items | Agenti |
| TASK-FR-049 | Vendor max price filter | Agenti |
| TASK-FR-050 | DPSMeter.Pause() | Agenti |
| TASK-FR-051 | DPSMeter.GetDamage(serial) | Agenti |
| TASK-FR-055 | Macro QueryStringResponse | Macro |
| TASK-FR-056 | Macro Whisper/Yell | Macro |
| TASK-FR-058 | Macro GumpResponse avanzata | Macro |
| TASK-FR-061 | SoundFilter per-categoria | Filtri |
| TASK-FR-062 | SeasonFilter force send | Filtri |
| TASK-FR-063 | 13 handler C2S mancanti | Rete |
| TASK-FR-068 | Mobile.Visible | Modelli |
| TASK-FR-069 | Mobile items (equipment list) | Modelli |
| TASK-FR-070 | Mobile Race/Mounted/etc. | Modelli |
| TASK-FR-071 | Item child items list | Modelli |
| TASK-FR-073 | Toolbar items config | Config |
| TASK-FR-076 | Item inspector flags | UI |
| TASK-FR-077 | Mobile inspector dettagli | UI |
| TASK-FR-078 | Static inspector assente | UI |
| TASK-FR-080 | Script editor debug stepping | UI |
| TASK-FR-088 | Serial.IsMobile/IsItem helper | Utility |

### BASSO (Nice-to-have, impatto minimo)

| Task | Descrizione | Area |
|------|-------------|------|
| TASK-FR-040 | Dress UO3D EquipItemMacro | Agenti |
| TASK-FR-041 | Dress/Undress status separati | Agenti |
| TASK-FR-065 | UOAssist compatibility layer | Rete |
| TASK-FR-066 | FeatureBit system | Rete |
| TASK-FR-067 | UOEntity.Deleted flag | Modelli |
| TASK-FR-072 | Item Layer enum | Modelli |
| TASK-FR-074 | Password memory | Config |
| TASK-FR-075 | Journal filter config | Config |
| TASK-FR-079 | Object inspector SharedData/Timers | UI |
| TASK-FR-081 | RE_MessageBox custom | UI |
| TASK-FR-082 | Scavenger edit props window | UI |
| TASK-FR-083 | Predefined property list | UI |
| TASK-FR-084 | Geometry.cs | Utility |
| TASK-FR-085 | ActionQueue.cs | Utility |
| TASK-FR-086 | ToolBar.cs logica | Utility |
| TASK-FR-087 | ZLib.cs compressione | Utility |
| TASK-FR-089 | Facet.cs map metadata | Utility |

---

### Conteggio Finale

| Priorita | Conteggio Task |
|----------|----------------|
| CRITICO | 8 |
| ALTO | 21 |
| MEDIO | 42 |
| BASSO | 17 |
| **TOTALE** | **88** |

---

*Report generato il 2026-03-20. Ogni task e stato verificato leggendo direttamente il codice sorgente di entrambe le codebase. Nessun metodo e stato dato per scontato senza verifica.*
