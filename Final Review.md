# Final Review - Analisi Migrazione Legacy TMRazor → TMRazorImproved

**Data:** 2026-03-18
**Scope:** Mappatura 1:1 architetturale tra il codice sorgente legacy (`/Razor`) e il nuovo progetto (`/TMRazorImproved`)
**Legacy:** WinForms, .NET Framework, classi statiche monolitiche (253 file .cs)
**Nuovo:** WPF + WPF-UI, .NET 10, MVVM + Dependency Injection (242+ file .cs)

---

## Indice

1. [Riepilogo Esecutivo](#1-riepilogo-esecutivo)
2. [Funzionalita Mancanti dal Nuovo Codice](#2-funzionalita-mancanti-dal-nuovo-codice)
3. [Funzionalita Migrate ma Incomplete](#3-funzionalita-migrate-ma-incomplete)
4. [Discrepanze Strutturali Critiche](#4-discrepanze-strutturali-critiche)
5. [Mappatura Dettagliata per Area](#5-mappatura-dettagliata-per-area)
6. [Task di Implementazione](#6-task-di-implementazione)

---

## 1. Riepilogo Esecutivo

| Area | File Legacy | Controparte Nuova | Stato |
|------|-------------|-------------------|-------|
| Filtri Pacchetti | 13 classi in `/Filters/` | 1 classe `FilterHandler.cs` | **Parziale** - 5 filtri mancanti |
| Rete/Pacchetti | 8 file in `/Network/` | `PacketService.cs` + `WorldPacketHandler.cs` | **Parziale** - 4 moduli mancanti |
| Agenti | 11 classi in `/RazorEnhanced/` | 10 servizi in `/Services/` | **Buono** - gap minori |
| Core/World | 20+ file in `/Core/` | `WorldService.cs` + modelli Shared | **Buono** - ristrutturato |
| Scripting API | 15+ classi in `/RazorEnhanced/` | 15 file in `/Services/Scripting/Api/` | **Buono** - 3 API mancanti |
| Macro System | 43 action types + MacroManager | `LegacyMacroMigrator` + `MacrosService` | **Critico** - 35/43 azioni non migrate |
| UI Forms/Dialogs | 20+ dialog WinForms | 30+ pagine WPF | **Parziale** - 18 dialog mancanti |
| Client Abstraction | 4 classi Client | 1 `ClientInteropService` | **Ristrutturato** - funzionale |

**Copertura complessiva stimata: ~75-80%** - Le funzionalita core sono migrate, ma rimangono gap significativi nei filtri avanzati, nel sistema macro e nei dialog di utility.

---

## 2. Funzionalita Mancanti dal Nuovo Codice

### 2.1 FILTRI - Assenti in TMRazorImproved

#### TASK-001: Mobile Graphics Filter (MobileFilter.cs)
**Legacy:** `Razor/Filters/MobileFilter.cs`
**Nuovo:** Nessun equivalente

Il legacy contiene tre metodi statici che sostituiscono la grafica di creature specifiche per ridurre il lag visivo:
- `ApplyDragonFilter(Packet p, Mobile m)` - Sostituisce grafica Draghi/Wyrm
- `ApplyDrakeFilter(Packet p, Mobile m)` - Sostituisce grafica Drake
- `ApplyDaemonFilter(Packet p, Mobile m)` - Sostituisce grafica Demoni

**Impatto utente:** Chi giocava con lag elevato e usava questi filtri per visualizzare creature grandi come modelli piu leggeri, perdera questa funzionalita.

**Come implementare:**
1. Aprire `TMRazorImproved.Core/Handlers/FilterHandler.cs`
2. Aggiungere un dizionario `_graphicReplacements` con mapping `{graphic_originale -> graphic_sostitutiva}`
3. Registrare un viewer (non filter) sui pacchetti `0x78` (MobileIncoming) e `0x20` (MobileUpdate)
4. Nel callback, leggere il campo `graphic` dal buffer del pacchetto (offset 3-4 per 0x78), confrontare con il dizionario, e se presente sovrascrivere i byte
5. Aggiungere le tre opzioni booleane in `UserProfile` (`FilterDragon`, `FilterDrake`, `FilterDaemon`)
6. Nel `OptionsPage.xaml` aggiungere tre checkbox nella sezione filtri, con binding a `CurrentProfile.FilterDragon` etc.

---

#### TASK-002: Target Filter Manager (TargetFilterManager.cs)
**Legacy:** `Razor/Filters/TargetFilterManager.cs`
**Nuovo:** Nessun equivalente

Sistema completo per filtrare bersagli specifici dal targeting automatico:
- `AddTargetFilter(string name, Serial serial)` - Aggiunge serial alla lista filtrata
- `AddAllMobileAsTargetFilters()` - Aggiunge tutti i mobile visibili
- `AddAllHumanoidsAsTargetFilters()` - Aggiunge solo umanoidi
- `IsFilteredTarget(Serial serial)` - Verifica se un serial e filtrato
- `RemoveTargetFilter(int index)` / `ClearTargetFilters()`
- Persistenza XML con `Save()` / `Load()`
- Inner class `TargetFilter` con proprietà `Name` e `Serial`

**Impatto utente:** Impossibilita di escludere NPC/giocatori specifici dal targeting automatico.

**Come implementare:**
1. Creare interfaccia `ITargetFilterService` in `TMRazorImproved.Shared/Interfaces/`
   ```csharp
   public interface ITargetFilterService
   {
       void AddFilter(uint serial, string name);
       void AddAllMobiles();
       void AddAllHumanoids();
       bool IsFiltered(uint serial);
       void RemoveFilter(uint serial);
       void ClearAll();
       IReadOnlyList<TargetFilterEntry> Filters { get; }
   }
   ```
2. Creare `TargetFilterService` in `TMRazorImproved.Core/Services/` con `ConcurrentDictionary<uint, string>` per i filtri
3. Nel `TargetingService`, prima di restituire un target, chiamare `_targetFilterService.IsFiltered(serial)` e saltarlo se filtrato
4. Aggiungere sezione nella `TargetingPage.xaml` con ListView dei filtri e pulsanti Add/Remove/Clear
5. Aggiungere persistenza in `UserProfile` come `List<TargetFilterEntry>`

---

#### TASK-003: Vet Reward Gump Filter (VetRewardGumpFilter.cs)
**Legacy:** `Razor/Filters/VetRewardGumpFilter.cs`
**Nuovo:** Nessun equivalente

Blocca il gump delle ricompense veterano (cliloc 1006046) su pacchetti `0xB0` e `0xDD`.

**Impatto utente:** Il popup delle ricompense veterano appare ogni volta al login, disturbando il gameplay.

**Come implementare:**
1. In `FilterHandler.cs`, aggiungere un filtro per `0xB0` (CompressedGump)
2. Nel callback, decomprimere i dati gump, cercare la stringa cliloc `1006046`
3. Se trovata, bloccare il pacchetto (`return true`)
4. Aggiungere checkbox `FilterVetRewardGump` nel profilo e nella UI Options

---

#### TASK-004: Wall Static Filter (WallStaticFilter.cs)
**Legacy:** `Razor/Filters/WallStaticFilter.cs`
**Nuovo:** Nessun equivalente

Converte muri animati in grafiche statiche per ridurre il rendering.
- `MakeWallStatic(Item wall)` - Converte graphic ID di muri animati in equivalenti statici, mantenendo il colore

**Impatto utente:** Performance ridotte in case con muri animati.

**Come implementare:**
1. Creare dizionario statico `AnimatedWallToStatic` in una classe utility
2. Nel `WorldPacketHandler`, nel handler di `0x1A` (WorldItem), verificare se il graphic e un muro animato
3. Se si, sostituire il graphic con quello statico
4. Aggiungere opzione `FilterAnimatedWalls` nel profilo

---

#### TASK-005: Staff Items & Staff NPCs Filter
**Legacy:** `Razor/Filters/StaffItems.cs` + `Razor/Filters/StaffNpcs.cs`
**Nuovo:** Nessun equivalente

- **StaffItems:** Blocca item staff-only (TypeID `0x36FF` LOS blocker, `0x1183` movement blocker) su pacchetto `0x1A`. OnEnable rimuove tutti gli staff items dal World, OnDisable li ripristina.
- **StaffNpcs:** Blocca mobile invisibili (flag visibilita) su pacchetti `0x20`, `0x78`, `0x77`. OnEnable rimuove mobile invisibili, OnDisable li ripristina.

**Impatto utente:** Free shard con item/NPC staff visibili causeranno confusione visiva.

**Come implementare:**
1. In `FilterHandler.cs`, aggiungere filtri per `0x1A` che controllano il TypeID
2. Aggiungere filtri per `0x78`/`0x20`/`0x77` che controllano il flag di visibilita (byte flags nel pacchetto mobile)
3. Aggiungere opzioni `FilterStaffItems` e `FilterStaffNpcs` nel profilo e UI

---

### 2.2 RETE - Moduli Assenti

#### TASK-006: Packet Logger (PacketLogger.cs)
**Legacy:** `Razor/Network/PacketLogger.cs`
**Nuovo:** UI presente (`PacketLoggerPage.xaml` + `PacketLoggerViewModel.cs`) ma il servizio backend completo manca

Il legacy ha un sistema completo:
- Whitelist/blacklist per packet ID
- Template JSON per parsing strutturato dei pacchetti
- Hex dump formattato su file
- Ascolto selettivo per direzione (S2C / C2S)
- Start/Stop recording

**Impatto utente:** Lo sviluppatore di script non puo analizzare pacchetti sconosciuti.

**Come implementare:**
1. Creare `IPacketLoggerService` in Shared con metodi `StartRecording()`, `StopRecording()`, `AddBlacklist(int)`, `AddWhitelist(int)`, `AddTemplate(string)`
2. Implementare `PacketLoggerService` in Core che usa `IPacketService.RegisterViewer()` per intercettare tutti i pacchetti
3. Scrivere output su file con formato hex dump + timestamp + direzione
4. Collegare al `PacketLoggerViewModel` esistente

---

#### TASK-007: UO Mod Injection (UoMod.cs)
**Legacy:** `Razor/Network/UoMod.cs`
**Nuovo:** Nessun equivalente

Sistema per iniettare patch client-side nel processo UO:
- `InjectUoMod()` - Inietta UOMod.dll nel client
- `EnableDisable(bool enable, int patch)` - Attiva/disattiva patch (FPS unlock, paperdoll, sound, view range, ecc.)
- `ProfileChange()` - Gestisce cambio profilo con stato patch

**Impatto utente:** Impossibilita di applicare mod client come FPS unlock o extended view range.

**Come implementare:**
1. Creare `IUOModService` con metodi `InjectMod()`, `EnablePatch(int)`, `DisablePatch(int)`, `GetPatchState()`
2. Implementare usando `ClientInteropService` esistente per l'injection DLL (gia supporta `CreateRemoteThread`)
3. Caricare la DLL `UOMod.dll` dalla cartella dell'applicazione
4. Esporre le opzioni nella `OptionsPage.xaml` come lista di checkbox per ogni mod

---

#### TASK-008: Ping/Latency Measurement (Ping.cs)
**Legacy:** `Razor/Network/Ping.cs`
**Nuovo:** Nessun equivalente

Misura la latenza server con min/max/media:
- `StartPing(int count)` - Avvia sequenza ping
- `Response(byte seq)` - Gestisce risposta e calcola statistiche
- Pacchetto `0x73` (Ping) gia gestito dal `WorldPacketHandler` ma senza logica di misurazione

**Impatto utente:** Nessuna visualizzazione del ping/latenza nella UI.

**Come implementare:**
1. Aggiungere logica di misurazione nel handler di `0x73` in `WorldPacketHandler`
2. Usare `Stopwatch` per misurare RTT tra invio e ricezione
3. Calcolare min/max/avg su finestra scorrevole
4. Esporre via proprieta in `IWorldService` o servizio dedicato
5. Visualizzare nella TitleBar o nella DashboardPage

---

#### TASK-009: UO Warper (UoWarper.cs)
**Legacy:** `Razor/Network/UoWarper.cs`
**Nuovo:** Nessun equivalente

Interfaccia per eseguire comandi macro via DLL:
- `Pathfind(int X, int Y, int Z)` - Pathfinding via client
- `OpenPaperdoll()` - Apre paperdoll
- `EUOWeaponPrimary()` / `EUOWeaponSecondary()` - Abilita primaria/secondaria
- `CloseBackpack()` - Chiude zaino
- `NextContPos(X, Y)` - Posizione prossimo contenitore
- `ToggleAlwaysRun()` - Toggle corsa automatica

**Impatto utente:** Alcune macro avanzate che usano questi comandi non funzioneranno.

**Come implementare:**
1. Integrare questi metodi in `IClientInteropService` come metodi aggiuntivi
2. Usare `PostMessage` per inviare i comandi al client UO
3. Esporre nell'API scripting tramite `PlayerApi` o `MiscApi`

---

### 2.3 SCRIPTING API - Assenti

#### TASK-010: Sound API (Sound.cs)
**Legacy:** `Razor/RazorEnhanced/Sound.cs`
**Nuovo:** Nessun equivalente nell'API di scripting (il `SoundService` e `SoundViewModel` esistono per la UI, ma non sono esposti agli script)

Metodi legacy:
- `PlaySoundEffect(int id)`, `PlayMusic(int id)`, `StopSound()`
- `PlayObject(int serial)`, `PlayMobile(int serial)`, `PlayTarget()`
- `PlayLocation(int x, int y, int z, int id)`
- `PlayDelay(int ms)`, `GetMinDuration()`, `GetMaxDuration()`

**Impatto utente:** Script che riproducono suoni non funzioneranno.

**Come implementare:**
1. Creare `SoundApi.cs` in `/Services/Scripting/Api/`
2. Iniettare `ISoundService` gia esistente
3. Mappare ogni metodo legacy: `PlaySoundEffect` -> costruire pacchetto `0x54` e inviare via `_packetService.SendToClient()`
4. Registrare `SoundApi` come variabile `Sound` nello scope degli script (in `ScriptingService.cs`)

---

#### TASK-011: HotKey API (HotKey.cs)
**Legacy:** `Razor/RazorEnhanced/HotKey.cs`
**Nuovo:** Nessun equivalente nell'API di scripting (l'`HotkeyService` esiste ma non e esposto)

Metodi legacy:
- `Get()` - Restituisce lista hotkey configurati
- `GetStatus(string)` - Stato abilitazione hotkey
- `SetStatus(string, bool)` - Abilita/disabilita hotkey
- `GetKey(string)` - Ottiene key binding
- `KeyString(string)` - Rappresentazione testuale

**Impatto utente:** Script non possono interrogare o modificare hotkey a runtime.

**Come implementare:**
1. Creare `HotkeyApi.cs` in `/Services/Scripting/Api/`
2. Iniettare `IHotkeyService`
3. Mappare i metodi legacy
4. Registrare come variabile `Hotkey` nello scope script

---

#### TASK-012: Script Recorder (ScriptRecorder.cs) ✅ COMPLETATO
**Legacy:** `Razor/RazorEnhanced/ScriptRecorder.cs`
**Nuovo:** `IScriptRecorderService` + `ScriptRecorderService` + integrazione UI

Sistema di registrazione che cattura azioni del giocatore e le converte in codice script:
- `PyScriptRecorder` - Genera codice Python
- `UosScriptRecorder` - Genera codice UOSteam
- Registra 15+ tipi di azione: attack, double-click, drop, equip, movement, targeting, gumps, context menus, virtues, potions
- 11 packet hooks per la cattura: `0x02`, `0x05`, `0x06`, `0x07`, `0x08`, `0x09`, `0x13`, `0x6C`, `0xBF`, `0xD7`, server `0x6C`

**Impatto utente:** **CRITICO** - Gli utenti non possono piu registrare macro giocando. Devono scrivere script manualmente.

**Come implementare:**
1. Creare `IScriptRecorderService` in Shared:
   ```csharp
   public interface IScriptRecorderService
   {
       void StartRecording(ScriptLanguage language);
       void StopRecording();
       string GetRecordedScript();
       bool IsRecording { get; }
   }
   ```
2. Implementare `ScriptRecorderService` in Core:
   - Iniettare `IPacketService` per registrare viewer sui pacchetti client-to-server
   - Per ogni pacchetto catturato, generare la riga di codice corrispondente nella lingua selezionata
   - Esempio: pacchetto `0x06` (DoubleClick) con serial `0x12345678` → genera `Items.UseItem(0x12345678)` (Python) o `DOUBLECLICK 0x12345678` (UOSteam)
3. Creare classe base `ScriptRecorderBase` con metodo `virtual string RecordAction(string actionType, params object[] args)`
4. Creare `PythonScriptRecorder : ScriptRecorderBase` e `UOSteamScriptRecorder : ScriptRecorderBase`
5. Collegare al pulsante "Record" gia presente nella `ScriptingPage.xaml`

---

### 2.4 CORE - Classi Mancanti

#### TASK-013: DragDropManager (DragDropManager.cs)
**Legacy:** `Razor/RazorEnhanced/DragDropManager.cs`
**Nuovo:** Logica distribuita nei singoli servizi, ma senza coordinamento centrale

Il legacy ha un manager centralizzato che coordina tutte le operazioni di drag-drop:
- `AutoLootSerialCorpseRefresh` - Coda apertura corpi
- `ScavengerSerialToGrab` - Coda raccolta oggetti da terra
- `CorpseToCutSerial` - Coda per auto-carver
- `AutoRun()` - Loop principale che processa tutte le code
- `ProcessLootList()` - Mutex-protected per evitare conflitti tra agenti

**Impatto utente:** Potenziali conflitti tra agenti che tentano di spostare oggetti contemporaneamente (es. AutoLoot + Scavenger nello stesso momento).

**Come implementare:**
1. Creare `IDragDropCoordinator` in Shared con `SemaphoreSlim` per serializzare le operazioni
2. Implementare `DragDropCoordinator` in Core che espone `async Task<bool> RequestDragDrop(uint serial, uint destination)`
3. Ogni servizio (AutoLoot, Scavenger, Organizer) deve acquisire il semaforo prima di inviare pacchetti `0x07`/`0x08`
4. Aggiungere timeout e retry logic

---

#### TASK-014: EncodedSpeech (EncodedSpeech.cs)
**Legacy:** `Razor/RazorEnhanced/EncodedSpeech.cs`
**Nuovo:** Nessun equivalente

Gestisce la codifica/decodifica del protocollo di speech encoded di UO (pacchetto `0xAD`).

**Come implementare:**
1. Creare `EncodedSpeechHelper.cs` in `TMRazorImproved.Core/Utilities/`
2. Portare la tabella di codifica e i metodi `Encode()`/`Decode()` dal legacy
3. Usare in `WorldPacketHandler` per gestire correttamente il pacchetto `0xAD`

---

#### TASK-015: Proto-Control/gRPC (ProtoControl*.cs)
**Legacy:** `Razor/RazorEnhanced/Proto-Control/ProtoControl.cs`, `ProtoControlGrpc.cs`, `ProtoControlServer.cs`
**Nuovo:** Nessun equivalente

Server gRPC per controllo remoto dell'applicazione. Permette ad applicazioni esterne di inviare comandi a TMRazor.

**Impatto utente:** Tool esterni (bot manager, dashboard web) non possono piu comunicare con TMRazor.

**Come implementare:**
1. Aggiungere pacchetto `Grpc.AspNetCore` al progetto Core
2. Portare i file `.proto` dal legacy
3. Creare `ProtoControlService` che espone i servizi Core via gRPC
4. Registrare nel DI container e avviare il server gRPC all'avvio dell'app

---

### 2.5 UI - Dialog Mancanti

#### TASK-016: Dialog WinForms Non Ricreate in WPF

Le seguenti finestre di dialogo legacy non hanno equivalente nella nuova UI:

| Dialog Legacy | File | Scopo | Priorita |
|---------------|------|-------|----------|
| `EnhancedAgentAddList` | UI/EnhancedAgentAddList.cs | Aggiunta item a liste agenti | Alta |
| `EnhancedAutolootEditItemProps` | UI/EnhancedAutolootEditItemProps.cs | Configurazione proprietà item per autoloot | Alta |
| `EnhancedScavengerEditItemProps` | UI/EnhancedScavengerEditItemProps.cs | Configurazione proprietà item per scavenger | Alta |
| `EnhancedDressAddUndressLayer` | UI/EnhancedDressAddUndressLayer.cs | Aggiunta layer dress/undress | Media |
| `EnhancedFriendAddGuildManual` | UI/EnhancedFriendAddGuildManual.cs | Aggiunta manuale gilda | Media |
| `EnhancedFriendAddPlayerManual` | UI/EnhancedFriendAddPlayerManual.cs | Aggiunta manuale giocatore | Media |
| `EnhancedGumpInspector` | UI/EnhancedGumpInspector.cs | Inspector gump dedicato | Media |
| `EnhancedItemInspector` | UI/EnhancedItemInspector.cs | Inspector item dedicato | Media |
| `EnhancedMobileInspector` | UI/EnhancedMobileInspector.cs | Inspector mobile dedicato | Media |
| `EnhancedObjectInspector` | UI/EnhancedObjectInspector.cs | Inspector generico | Media |
| `EnhancedStaticInspector` | UI/EnhancedStaticInspector.cs | Inspector static tiles | Bassa |
| `EnhancedLauncher` | UI/EnhancedLauncher.cs | Launcher client UO | Bassa |
| `EnhancedChangeLog` | UI/EnhancedChangeLog.cs | Visualizzazione changelog | Bassa |
| `EnhancedProfileAdd` | UI/EnhancedProfileAdd.cs | Creazione nuovo profilo | Media |
| `EnhancedProfileClone` | UI/EnhancedProfileClone.cs | Clonazione profilo | Media |
| `EnhancedProfileRename` | UI/EnhancedProfileRename.cs | Rinomina profilo | Media |
| `EnhancedScriptEditor` | UI/EnhancedScriptEditor.cs | Editor script standalone | Bassa (sostituito da ScriptingPage) |
| `RE_MessageBox` | UI/RE_MessageBox.cs | MessageBox personalizzata | Bassa |
| `SplashScreen` | UI/SplashScreen.cs | Splash screen avvio | Bassa |

**Nota:** L'`InspectorViewModel` nel nuovo codice combina parzialmente le funzionalita di Item/Mobile/Gump/Object Inspector in un'unica pagina. I dialog dedicati per l'editing delle proprietà degli item (AutoLoot/Scavenger) sono quelli con priorità piu alta.

**Come implementare (esempio per EnhancedAutolootEditItemProps):**
1. Creare `AutoLootEditItemWindow.xaml` in `Views/Windows/`
2. Layout: StackPanel con TextBox per ItemID, Hue, Name
3. Sezione proprietà: DataGrid con colonne "Property Name", "Min Value", "Max Value"
4. Pulsanti: "Add Property", "Remove Property", "Save", "Cancel"
5. Creare `AutoLootEditItemViewModel` con binding ai dati dell'item
6. Aprire il dialog dal `AutoLootViewModel` tramite `IDialogService` o direttamente

---

## 3. Funzionalita Migrate ma Incomplete

### 3.1 TODO nel Codice

#### TASK-017: FriendApi.AddFriendTarget() (TODO)
**File:** `TMRazorImproved.Core/Services/Scripting/Api/FriendApi.cs:35`
**Problema:** Il metodo `AddFriendTarget()` ha solo un commento `// TODO: Delega al TargetService per ottenere il target e aggiungerlo`
**Corpo attuale:** Vuoto (solo `_cancel.ThrowIfCancelled()`)

**Come fixare:**
```csharp
public virtual void AddFriendTarget()
{
    _cancel.ThrowIfCancelled();
    // 1. Richiedi un target al TargetingService
    var targetInfo = _targetingService.WaitForTarget(10000); // 10s timeout
    if (targetInfo != null && targetInfo.Serial != 0)
    {
        // 2. Cerca il mobile nel WorldService
        var mobile = _worldService.FindMobile(targetInfo.Serial);
        string name = mobile?.Name ?? $"Unknown_0x{targetInfo.Serial:X}";
        // 3. Aggiungi alla lista amici
        _friends.AddFriend(targetInfo.Serial, name);
    }
}
```
Richiede l'iniezione di `ITargetingService` e `IWorldService` nel costruttore di `FriendApi`.

---

#### TASK-018: LegacyMacroMigrator - UseSkill (TODO)
**File:** `TMRazorImproved.Core/Utilities/LegacyMacroMigrator.cs:59`
**Problema:** `return $"// TODO: USESKILL {parts[1]} (needs ID)";`
**Il migrator non converte le azioni UseSkill perche manca la tabella di mapping nome→ID.**

**Come fixare:**
1. Aggiungere dizionario statico nel migrator:
```csharp
private static readonly Dictionary<string, int> SkillNameToId = new()
{
    { "Anatomy", 1 }, { "AnimalLore", 2 }, { "AnimalTaming", 35 },
    { "ArmsLore", 4 }, { "Begging", 6 }, { "Cartography", 12 },
    { "DetectHidden", 14 }, { "Discordance", 15 }, { "EvalInt", 16 },
    { "Forensics", 19 }, { "Healing", 17 }, { "Herding", 18 },
    { "Hiding", 21 }, { "Inscription", 23 }, { "ItemID", 3 },
    { "Meditation", 46 }, { "Peacemaking", 9 }, { "Poisoning", 30 },
    { "Provocation", 22 }, { "RemoveTrap", 48 }, { "SpiritSpeak", 32 },
    { "Stealing", 33 }, { "Stealth", 47 }, { "Taste", 36 },
    { "Tracking", 38 }
};
```
2. Nel case `"UseSkill"`:
```csharp
case "UseSkill":
    if (parts.Length >= 2 && SkillNameToId.TryGetValue(parts[1], out int skillId))
        return $"USESKILL {skillId}";
    return $"USESKILL {parts[1]}"; // fallback al nome
```

---

#### TASK-019: InspectorViewModel - Targeting per Locazione (TODO)
**File:** `TMRazorImproved.UI/ViewModels/InspectorViewModel.cs:250`
**Problema:** `// TODO: Implementare targeting specifico per locazione/terreno nel targetingService`
**Il pulsante "Inspect Map" imposta `IsWaitingForTarget = true` ma non invia effettivamente una richiesta di targeting location-based.**

**Come fixare:**
1. In `ITargetingService`, aggiungere metodo `RequestLocationTarget()` che invia un pacchetto target cursor con flag `0x01` (location target)
2. Nel `InspectorViewModel.InspectMap()`, chiamare `_targetingService.RequestLocationTarget()` invece del commento TODO
3. Nel callback `OnTargetReceived`, gestire il caso in cui `info.Serial == 0` (location target) mostrando le coordinate X/Y/Z e il tile static

---

#### TASK-020: GumpListViewModel - Open Gump Inspector (TODO)
**File:** `TMRazorImproved.UI/ViewModels/GumpListViewModel.cs:59`
**Problema:** `// TODO: Apri Gump Inspector con questo gump`

**Come fixare:**
1. Il `GumpListViewModel` deve navigare alla sezione Inspector con il gump selezionato
2. Usare `IMessenger` per inviare un messaggio `NavigateToInspectorMessage(UOGump gump)`
3. L'`InspectorViewModel` riceve il messaggio e chiama `InspectSpecificGump(gump)` (metodo gia esistente)

---

### 3.2 NotImplementedException nel Codice

#### TASK-021: Value Converters - ConvertBack
**File:** `TMRazorImproved.UI/Views/Converters/CommonConverters.cs` (11 occorrenze) e `Converters/GainToHeightConverter.cs` (1 occorrenza)

Converters con `ConvertBack` che lancia `NotImplementedException`:
- `NullToVisibilityConverter` (riga 24)
- `NotNullToVisibilityConverter` (riga 37)
- `HotkeyDisplayConverter` (riga 60)
- `CaptureAppearanceConverter` (riga 75)
- `IntToVisibilityConverter` (riga 112)
- `EnumToBooleanConverter` (riga 147)
- `BoolToFontWeightConverter` (riga 160)
- `StringNotEmptyToVisibilityConverter` (riga 195)
- `CollectionCountToVisibilityConverter` (riga 209)
- `ByteToHexConverter` (riga 228)
- `GainToHeightConverter` (riga 21)

**Valutazione rischio:** **BASSO** - I `ConvertBack` nei converter WPF sono richiesti solo per binding TwoWay. Questi converter sono usati in binding OneWay (display-only), quindi il `NotImplementedException` non verra mai raggiunto a runtime in condizioni normali. Tuttavia, e buona pratica restituire `Binding.DoNothing` o `DependencyProperty.UnsetValue` anziche lanciare eccezioni.

**Come fixare:**
Per ogni converter, sostituire:
```csharp
throw new NotImplementedException();
```
con:
```csharp
return System.Windows.Data.Binding.DoNothing;
```

---

### 3.3 Agenti con Gap di Funzionalita

#### TASK-022: DressService - Conflitto Armi a Due Mani
**Legacy:** `Razor/RazorEnhanced/Dress.cs` - Logica complessa per gestire il conflitto tra armi a due mani e scudi
**Nuovo:** `TMRazorImproved.Core/Services/DressService.cs` - Logica semplificata senza conflict resolution

Il legacy gestisce:
- Rilevamento arma a due mani gia equipaggiata
- Rimozione automatica prima di equipaggiare un nuovo oggetto nello slot conflittuale
- Supporto per pacchetti `EquipItemMacro` / `UnEquipItemMacro` del client UO3D
- Elemento speciale "UNDRESS" per svuotare slot specifici

**Come fixare:**
1. In `DressService.AgentLoopAsync()`, prima di equipaggiare un item:
   ```csharp
   // Controlla se l'item da equipaggiare e un'arma a due mani
   bool isTwoHanded = IsTwoHandedWeapon(item.Graphic);
   if (isTwoHanded)
   {
       // Rimuovi sia mano destra che sinistra
       await UnequipLayer(Layer.RightHand, token);
       await UnequipLayer(Layer.LeftHand, token);
   }
   else if (item.Layer == Layer.LeftHand)
   {
       // Controlla se c'e un'arma a due mani equipaggiata
       var rightHand = GetEquippedItem(Layer.RightHand);
       if (rightHand != null && IsTwoHandedWeapon(rightHand.Graphic))
           await UnequipLayer(Layer.RightHand, token);
   }
   ```
2. Aggiungere metodo `IsTwoHandedWeapon(ushort graphic)` con lookup da file JSON `weapons.json`

---

#### TASK-023: BandageHealService - Funzionalita Avanzate
**Legacy:** `Razor/RazorEnhanced/BandageHeal.cs` - Sistema complesso con calcolo delay basato su DEX, blocco veleno/mortal strike, modalita target multipli
**Nuovo:** `TMRazorImproved.Core/Services/BandageHealService.cs` - Implementazione base

Funzionalita legacy mancanti nel nuovo codice:
- **Buff-based delay calculation:** Il delay tra bende dipende dalla DEX del giocatore (formula UO)
- **Poison blocking:** Non curare se avvelenato (opzionale)
- **Mortal Strike blocking:** Non curare se sotto effetto Mortal Strike
- **Hidden blocking:** Non curare se nascosto (opzionale)
- **Target modes:** Self, Specific Target, Nearest Friend, Friend-or-Self
- **Countdown display:** Mostra timer visivo della benda
- **Text command mode:** Invio comando testuale invece di target manuale

**Come fixare:**
1. In `BandageHealService`, aggiungere proprieta dalla config: `PoisonBlock`, `MortalBlock`, `HiddenBlock`, `TargetMode`
2. Nel loop principale, prima di applicare benda:
   ```csharp
   if (_config.PoisonBlock && _worldService.Player.Poisoned) continue;
   if (_config.MortalBlock && HasBuff("Mortal Strike")) continue;
   if (_config.HiddenBlock && _worldService.Player.IsHidden) continue;
   ```
3. Calcolo delay DEX-based:
   ```csharp
   int GetBandageDelay()
   {
       int dex = _worldService.Player.Dex;
       // Formula UO: base 8 secondi, -1 sec ogni 20 DEX (min 3 sec)
       return Math.Max(3000, 8000 - (dex / 20) * 1000);
   }
   ```
4. Aggiungere opzioni nella `BandageHealPage.xaml` per tutte le modalita

---

#### TASK-024: AutoLoot/Scavenger - Property Filtering
**Legacy:** `AutoLootItem` ha lista di `Property` con `Name`, `MinValue`, `MaxValue` per filtrare item basandosi sulle proprietà
**Nuovo:** Il filtro e solo per `Graphic` e `Hue`, senza controllo proprietà

**Come fixare:**
1. In `AutoLootConfig` (ConfigModels.cs), aggiungere alla classe item:
   ```csharp
   public List<PropertyFilter> Properties { get; set; } = new();
   ```
2. Con `PropertyFilter`: `string Name`, `double MinValue`, `double MaxValue`
3. In `AutoLootService.AgentLoopAsync()`, dopo il match per graphic/hue, verificare le proprietà:
   ```csharp
   var itemProps = _worldService.FindItem(serial)?.Properties;
   if (config.Properties.Any() && itemProps != null)
   {
       bool allMatch = config.Properties.All(pf =>
           itemProps.Any(p => p.Contains(pf.Name) && ExtractValue(p) >= pf.MinValue && ExtractValue(p) <= pf.MaxValue));
       if (!allMatch) continue;
   }
   ```
4. Creare il dialog di editing proprietà (vedi TASK-016)

---

## 4. Discrepanze Strutturali Critiche

### 4.1 Sistema Macro - Gap Architetturale Principale

#### TASK-025: LegacyMacroMigrator - Copertura Azioni ✅ COMPLETATO
**Criticita:** ALTA

Il `LegacyMacroMigrator.cs` gestisce solo **15 dei 43 tipi di azione** macro del legacy. Le azioni non gestite vengono convertite in commenti `// UNMIGRATED: ...`.

**Azioni migrate (15):**
| Azione | Formato Output |
|--------|---------------|
| `Pause` | `PAUSE {ms}` |
| `CastSpell` | `CAST {id}` |
| `DoubleClick` | `DOUBLECLICK {serial}` |
| `Target` | `TARGET {serial}` |
| `WaitForTarget` | `WAITFORTARGET {ms}` |
| `AttackEntity` | `ATTACK {serial}` |
| `Messaging` | `SAY {text}` |
| `If` | `IF {params}` |
| `ElseIf` | `ELSEIF {params}` |
| `Else` | `ELSE` |
| `EndIf` | `ENDIF` |
| `While` | `WHILE {params}` |
| `EndWhile` | `ENDWHILE` |
| `For` | `FOR {iterations}` |
| `EndFor` | `ENDFOR` |
| `Comment` | `// {text}` |

**Azioni NON migrate (28):**
| Azione | Complessita Legacy | Priorita |
|--------|-------------------|----------|
| `UseSkill` | Media (needs ID table) | **Alta** |
| `Bandage` | Alta (172 righe, self/serial/alias) | **Alta** |
| `MoveItem` | Alta (155 righe, entity/ground) | **Alta** |
| `PickUp` | Media | **Alta** |
| `Drop` | Media | **Alta** |
| `Mount` | Alta (125 righe, mount/dismount) | Media |
| `ArmDisarm` | Media (left/right/both) | Media |
| `UsePotion` | Media (7 tipi pozione) | Media |
| `UseContextMenu` | Alta (124 righe) | Media |
| `SetAbility` | Bassa (primary/secondary) | Media |
| `InvokeVirtue` | Bassa (8 virtu) | Bassa |
| `WaitForGump` | Media (timeout) | Media |
| `GumpResponse` | Alta (switch + text entries) | Media |
| `PromptResponse` | Bassa | Bassa |
| `QueryStringResponse` | Bassa (yes/no + text) | Bassa |
| `SetAlias` | Bassa | Media |
| `RemoveAlias` | Bassa | Bassa |
| `Movement` | Media (walk/run direction) | Media |
| `Fly` | Bassa | Bassa |
| `ToggleWarMode` | Bassa | Bassa |
| `Disconnect` | Bassa | Bassa |
| `ClearJournal` | Bassa | Bassa |
| `Resync` | Bassa | Bassa |
| `RunOrganizerOnce` | Bassa | Bassa |
| `TargetResource` | Media | Bassa |
| `RenameMobile` | Bassa | Bassa |

**Come implementare i piu critici:**
Per ogni azione mancante, aggiungere un `case` nel `switch` di `MigrateAction()`:

```csharp
case "UseSkill":
    return SkillNameToId.TryGetValue(parts[1], out int sid)
        ? $"USESKILL {sid}" : $"USESKILL {parts[1]}";

case "Bandage":
    return parts.Length >= 2 ? $"BANDAGESELF" : "BANDAGESELF";

case "MoveItem":
    return parts.Length >= 4 ? $"MOVEITEM {parts[1]} {parts[2]} {parts[3]}" : $"// MOVEITEM {line}";

case "Mount":
    return "MOUNT";

case "ToggleWarMode":
    return "WARMODE";

case "SetAbility":
    return parts.Length >= 2 ? $"SETABILITY {parts[1]}" : "SETABILITY primary";

case "WaitForGump":
    return parts.Length >= 2 ? $"WAITFORGUMP {parts[1]}" : "WAITFORGUMP 5000";
```

---

### 4.2 Condition Evaluation Engine

#### TASK-026: MacrosService - Valutazione Condizioni If/While ✅ COMPLETATO
**Legacy:** `IfAction.cs` - 568 righe con 9 categorie di condizioni (PlayerStats, PlayerStatus, Skill, Find, Count, InRange, TargetExists, InJournal, BuffExists)
**Nuovo:** `MacrosService` - Parsing basico di stringhe IF/WHILE senza documentazione della logica di valutazione

Il legacy supporta:
- Token substitution: `{maxhp}`, `{maxmana}`, `{maxstam}`
- Operatori di confronto: `>`, `<`, `==`, `>=`, `<=`, `!=`
- Condizioni composite con Find + serial storage
- Color/hue filtering su items e mobiles

**Come fixare:**
1. Creare `ConditionEvaluator.cs` in Core:
   ```csharp
   public class ConditionEvaluator
   {
       private readonly IWorldService _world;

       public bool Evaluate(string condition)
       {
           var parts = condition.Split(' ');
           return parts[0].ToLower() switch
           {
               "hp" => ComparePlayerStat(_world.Player.Hits, parts[1], int.Parse(parts[2])),
               "mana" => ComparePlayerStat(_world.Player.Mana, parts[1], int.Parse(parts[2])),
               "poisoned" => _world.Player.Poisoned,
               "hidden" => _world.Player.IsHidden,
               "find" => EvaluateFind(parts),
               "injournal" => EvaluateJournal(parts),
               "buffexists" => EvaluateBuff(parts),
               _ => false
           };
       }
   }
   ```
2. Integrare nel `MacrosService.ExecuteWithControlFlowAsync()` per le istruzioni IF/WHILE

---

### 4.3 Architettura Client

#### TASK-027: Supporto Multi-Client (ClassicUO + OSI)
**Legacy:** Gerarchia di ereditarietà `Client` → `ClassicUO` / `OSIClient` / `UOAssist`
**Nuovo:** Unica classe `ClientInteropService` con P/Invoke diretto

Il legacy distingue tra:
- **ClassicUO:** Plugin API (`CUO_API`), caricamento assembly custom, integrazione diretta
- **OSI Client:** Win32 messaging (`UONetMessage`), injection DLL, shared memory
- **UOAssist:** Protocollo UOAssist per tool di terze parti

Il nuovo `ClientInteropService` supporta solo il modello OSI-style con DLL injection. L'integrazione con ClassicUO come plugin e assente.

**Impatto utente:** Chi usa ClassicUO come client dovra usare il metodo di injection anziche il plugin nativo.

**Come fixare (se richiesto):**
1. Creare interfaccia `IClientAdapter` con metodi `Connect()`, `SendPacket()`, `ReceivePacket()`
2. Implementare `OsiClientAdapter` (basato su `ClientInteropService` attuale) e `ClassicUOAdapter`
3. Factory pattern per selezionare l'adapter giusto in base al client rilevato

---

### 4.4 Modello Item - Proprietà Mancanti

#### TASK-028: Item Model - Campi Mancanti vs Legacy
**Legacy `Assistant.Item`:**
- `Contains` (List<Item>) - Lista figli in contenitore
- `RootContainer` - Traversal ricorsivo al contenitore radice
- `IsCorpse` / `IsLootable` / `IsLootableTarget` - Check di tipo
- `CorpseNumberItems` - Conteggio item nel corpo
- `Price` / `BuyDesc` - Dati vendor
- `Updated` - Flag di aggiornamento contenitore
- `Owner` - Proprietario dell'item

**Nuovo `TMRazorImproved.Shared.Models.Item`:**
- `Container` (uint) - Solo serial del parent
- Manca `Contains`, `RootContainer`, `IsCorpse`, `Price`, `Owner`

**Come fixare:**
1. In `Item.cs`, aggiungere proprietà calcolate:
   ```csharp
   public bool IsCorpse => Graphic >= 0x2006 && Graphic <= 0x2006; // o lista di graphics
   public bool IsMovable { get; set; }
   ```
2. Il `RootContainer` puo essere calcolato via `WorldService`:
   ```csharp
   // In WorldService
   public uint GetRootContainer(uint serial)
   {
       var item = FindItem(serial);
       while (item != null && item.Container != 0)
       {
           var parent = FindItem(item.Container);
           if (parent == null) break;
           item = parent;
       }
       return item?.Serial ?? serial;
   }
   ```
3. Aggiungere `Price` e `BuyDesc` come proprietà opzionali per il vendor

---

## 5. Mappatura Dettagliata per Area

### 5.1 Filtri - Mapping Completo

| Legacy Class | Pacchetti | Nuovo Equivalente | Stato |
|-------------|-----------|-------------------|-------|
| `LightFilter` | 0x4E, 0x4F | `FilterHandler` (Light) | Migrato |
| `WeatherFilter` | 0x65 | `FilterHandler` (Weather) | Migrato |
| `DeathFilter` | 0x2C | `FilterHandler` (Death) | Migrato |
| `SeasonFilter` | 0xBC | `FilterHandler` (Season) | Migrato |
| `SoundFilter` | 0x54 | `FilterHandler` (Sound + Footsteps) | Migrato |
| `AsciiMessageFilter` | 0x1C | `FilterHandler` (Message Content) | Parziale (solo testo, non colore) |
| `LocMessageFilter` | 0xC1 | Assente | **MANCANTE** |
| `MobileFilter` | 0x78, 0x20 | Assente | **MANCANTE** (vedi TASK-001) |
| `TargetFilterManager` | N/A | Assente | **MANCANTE** (vedi TASK-002) |
| `VetRewardGumpFilter` | 0xB0, 0xDD | Assente | **MANCANTE** (vedi TASK-003) |
| `WallStaticFilter` | 0x1A | Assente | **MANCANTE** (vedi TASK-004) |
| `StaffItemFilter` | 0x1A | Assente | **MANCANTE** (vedi TASK-005) |
| `StaffNpcsFilter` | 0x20, 0x78, 0x77 | Assente | **MANCANTE** (vedi TASK-005) |
| Bard Music filter | 0x6D | `FilterHandler` (BardMusic) | Migrato |
| Trade Request filter | 0x6F | `FilterHandler` (Trade) | Migrato |
| Party Invite filter | 0xBF | `FilterHandler` (PartyInvite) | Migrato |
| Poison/Karma/Snoop filter | 0x1C, 0xAE | `FilterHandler` (Message Content) | Migrato |

### 5.2 Agenti - Mapping Completo

| Legacy Class | Nuovo Servizio | Stato | Gap |
|-------------|---------------|-------|-----|
| `AutoLoot` | `AutoLootService` | Migrato | Property filtering mancante (TASK-024) |
| `Scavenger` | `ScavengerService` | Migrato | OSI locked item check mancante |
| `Organizer` | `OrganizerService` | Migrato | Color matching semplificato |
| `Dress` | `DressService` | Parziale | Conflitto 2-mani mancante (TASK-022) |
| `BandageHeal` | `BandageHealService` | Parziale | Feature avanzate mancanti (TASK-023) |
| `Restock` | `RestockService` | Migrato | Verificare conteggio destinazione |
| `Vendor` | `VendorService` | Migrato | - |
| `Friend` | `FriendsService` | Migrato | Faction/guild matching parziale |
| `DPSMeter` | `DPSMeterViewModel` | Migrato | Verificare completezza |
| `Trade` | `SecureTradeService` | Migrato | - |
| `DragDropManager` | Distribuito | Ristrutturato | Coordinamento centralizzato assente (TASK-013) |

### 5.3 Scripting API - Mapping Completo

| Legacy API | Nuova API | Copertura | Gap |
|-----------|----------|-----------|-----|
| `Player` (40+ metodi) | `PlayerApi.cs` | ~87% | Alcuni metodi UO3D |
| `Items` (15+ metodi) | `ItemsApi.cs` | ~100% | Migliorato |
| `Mobiles` (8+ metodi) | `MobilesApi.cs` | ~100% | Migliorato |
| `Misc` (50+ metodi) | `MiscApi.cs` | ~80% | File I/O ridotto |
| `Target` (8+ metodi) | `TargetApi.cs` | ~100% | - |
| `Gumps` (5+ metodi) | `GumpsApi.cs` | ~100% | - |
| `Journal` (7+ metodi) | `JournalApi.cs` | ~85% | - |
| `Spells` (6+ metodi) | `SpellsApi.cs` | ~100% | - |
| `Skills` (5+ metodi) | `SkillsApi.cs` | ~100% | - |
| `Statics` (6+ metodi) | `StaticsApi.cs` | ~100% | - |
| `Timer` (6+ metodi) | `TimerApi.cs` | ~100% | - |
| `SpecialMoves` | `SpecialMovesApi.cs` | ~100% | - |
| `Friend` (8+ metodi) | `FriendApi.cs` | ~90% | `AddFriendTarget` TODO |
| `Filters` | `FiltersApi.cs` | ~90% | - |
| `AutoLoot/Scavenger/...` | `AgentApis.cs` | ~90% | - |
| `Sound` (10+ metodi) | **ASSENTE** | 0% | TASK-010 |
| `HotKey` (5+ metodi) | **ASSENTE** | 0% | TASK-011 |
| `CUO` (ClassicUO API) | **ASSENTE** | 0% | Specifico ClassicUO |
| `ScriptRecorder` | **ASSENTE** | 0% | TASK-012 |

### 5.4 Core Models - Mapping

| Legacy Class | Nuovo Equivalente | Stato |
|-------------|------------------|-------|
| `UOEntity` | `UOEntity` (Shared) | Migrato + migliorato (DistanceTo) |
| `Mobile` | `Mobile` (Shared) | Migrato + esteso (AOS stats, buffs, pets) |
| `Item` | `Item` (Shared) | Parziale - campi mancanti (TASK-028) |
| `PlayerData` | Unificato in `Mobile` | Ristrutturato |
| `World` (static) | `WorldService` (DI) | Migrato - architettura migliorata |
| `Targeting` | `TargetingService` | Migrato |
| `ActionQueue` | Distribuito nei servizi | Ristrutturato |
| `Buffs` + `BuffInfo` | `Mobile.ActiveBuffs` + Messages | Migrato |
| `Commands` | Distribuito nelle API | Ristrutturato |
| `Spells` | `SpellDefinitions.cs` (Shared) | Migrato |
| `Serial` | Tipo nativo `uint` | Semplificato |
| `Timer` | `System.Threading.Timer` / `Task.Delay` | Modernizzato |
| `TitleBar` | `TitleBarService` | Migrato |
| `Utility` | Distribuito | Ristrutturato |
| `VideoCapture` | `VideoCaptureService` | Migrato (SharpAvi) |
| `ScreenCapture` | `ScreenCaptureService` | Migrato |
| `ZLib` | Assente (usato per gump compression) | **Verificare** |
| `Facet` | Dati JSON in Config | Migrato |
| `Geometry` + `Point3D` | Coordinate X/Y/Z separate | Semplificato |
| `MsgQueue` | `ConcurrentQueue<T>` | Modernizzato |
| `PasswordMemory` | Assente | **Verificare necessita** |
| `StealthSteps` | Assente | **Verificare** |
| `SkillIcon` | `SpellIcon.cs` (Shared) | Migrato |
| `TypeID` | Costanti/Enums | Ristrutturato |
| `CircularBuffer` | Assente | **Non necessario** (era per ScriptRecorder) |
| `SyncPrimitives` | `SemaphoreSlim`/`ConcurrentDictionary` | Modernizzato |
| `EncodedSpeech` | Assente | **MANCANTE** (TASK-014) |

---

## 6. Task di Implementazione - Riepilogo Priorità

### Priorita CRITICA (Funzionalita core perse per l'utente)

| # | Task | File da Modificare | Effort | Stato |
|---|------|-------------------|--------|-------|
| TASK-012 | Script Recorder | `IScriptRecorderService.cs` + `ScriptRecorderService.cs` + `ScriptingViewModel.cs` + `ScriptingPage.xaml` | Alto | ✅ COMPLETATO (2026-03-18) |
| TASK-025 | Macro Migrator - 28 azioni mancanti | `LegacyMacroMigrator.cs` | Alto | ✅ COMPLETATO (2026-03-18) |
| TASK-026 | Condition Evaluation Engine | `ConditionEvaluator.cs` + `MacrosService.cs` | Alto | ✅ COMPLETATO (2026-03-18) |
| TASK-013 | DragDrop Coordinator | Nuovo servizio | Medio | Pendente |

### Priorità ALTA (Feature significative mancanti)

| # | Task | File da Modificare | Effort |
|---|------|-------------------|--------|
| TASK-001 | Mobile Graphics Filter | `FilterHandler.cs` + config | Basso |
| TASK-002 | Target Filter Manager | Nuovo servizio + UI | Medio |
| TASK-010 | Sound API per scripting | Nuovo `SoundApi.cs` | Basso |
| TASK-011 | HotKey API per scripting | Nuovo `HotkeyApi.cs` | Basso |
| TASK-016 | Dialog editing proprietà item (AutoLoot/Scavenger) | Nuovi XAML + VM | Medio |
| TASK-022 | Dress - Conflitto armi 2 mani | `DressService.cs` | Medio |
| TASK-023 | BandageHeal - Feature avanzate | `BandageHealService.cs` | Medio |
| TASK-024 | AutoLoot/Scavenger - Property Filter | Servizi + config + UI | Medio |
| TASK-028 | Item Model - Campi mancanti | `Item.cs` + `WorldService.cs` | Basso |

### Priorità MEDIA (Funzionalità secondarie)

| # | Task | File da Modificare | Effort |
|---|------|-------------------|--------|
| TASK-003 | Vet Reward Gump Filter | `FilterHandler.cs` | Basso |
| TASK-004 | Wall Static Filter | `WorldPacketHandler.cs` | Basso |
| TASK-005 | Staff Items/NPCs Filter | `FilterHandler.cs` | Medio |
| TASK-006 | Packet Logger Backend | Nuovo servizio | Medio |
| TASK-007 | UO Mod Injection | Nuovo servizio | Alto |
| TASK-008 | Ping/Latency | `WorldPacketHandler.cs` | Basso |
| TASK-009 | UO Warper | `ClientInteropService.cs` | Medio |
| TASK-014 | EncodedSpeech | Nuova utility | Basso |
| TASK-017 | FriendApi.AddFriendTarget() | `FriendApi.cs` | Basso |
| TASK-018 | Migrator UseSkill fix | `LegacyMacroMigrator.cs` | Basso |
| TASK-019 | Inspector location targeting | `InspectorViewModel.cs` | Basso |
| TASK-020 | Gump Inspector navigation | `GumpListViewModel.cs` | Basso |
| TASK-021 | ConvertBack fix | `CommonConverters.cs` | Basso |

### Priorità BASSA (Nice-to-have)

| # | Task | File da Modificare | Effort |
|---|------|-------------------|--------|
| TASK-015 | Proto-Control/gRPC | Nuovo progetto | Alto |
| TASK-027 | Multi-client support | `ClientInteropService.cs` | Alto |
| TASK-016 | Dialog UI minori (changelog, splash, profile mgmt) | Nuovi XAML | Medio |

---

## Appendice A: Statistiche di Copertura

```
Legacy:  253 file .cs
Nuovo:   242+ file .cs (escl. publish/)

Filtri:     9/13  migrati  (69%)
Rete:       4/8   migrati  (50%)
Agenti:     10/11 migrati  (91%)
API Script: 12/15 migrati  (80%)
Macro:      15/43 azioni   (35%)
UI Dialog:  12/30 migrati  (40%)
Core:       20/26 migrati  (77%)
Test:       20+ file nuovi (miglioramento)

Media ponderata copertura: ~75%
```

## Appendice B: Miglioramenti Architetturali nel Nuovo Codice

Il nuovo codice, pur non essendo completo al 100%, introduce miglioramenti significativi rispetto al legacy:

1. **Thread Safety:** `ConcurrentDictionary`, `ConcurrentQueue`, `volatile`, `SemaphoreSlim` invece di `lock` manuali e `Thread.Abort()`
2. **Dependency Injection:** Tutti i servizi iniettati via costruttore, testabili in isolamento
3. **Async/Await:** `Task`-based anziche `Thread`-based, cancellazione cooperativa
4. **MVVM:** Separazione netta View/ViewModel/Model con `CommunityToolkit.Mvvm`
5. **Messaging:** `IMessenger` per comunicazione disaccoppiata tra componenti
6. **Logging:** `ILogger<T>` strutturato anziche output console
7. **Configuration:** JSON-based con profili, anziche XML monolitico
8. **UI Moderna:** WPF-UI (Fluent Design) anziche WinForms custom
9. **Script Safety:** Cancellazione a due livelli (Thread.Interrupt + sys.settrace) anziche Thread.Abort
10. **Test Coverage:** 20+ file di test (unit, integration, stress, fuzz) - assenti nel legacy
