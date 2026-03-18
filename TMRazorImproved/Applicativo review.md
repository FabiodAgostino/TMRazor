# Applicativo Review — TMRazorImproved
**Data revisione:** 17 marzo 2026
**Revisore:** Architetto Software Senior
**Metodologia:** Analisi diretta del codice sorgente (nessun documento preesistente usato come fonte)
**Branch:** `claude/tmrazor-migration-review-DMs0o`

---

## 1. Stato Generale

La migrazione è in ottimo stato. L'architettura a layer (Shared / Core / UI / Plugin) è pulita,
il threading è corretto, il sistema di DI è ben configurato e la quasi totalità delle API di scripting
è funzionante. Le criticità rimanenti si concentrano in tre aree distinte:

| Area | Stato |
|------|-------|
| Scripting API (PlayerApi, SpellsApi, TargetApi, ecc.) | ✅ Quasi completo |
| Macro Engine (MacrosService) | ⚠️ Parzialmente incompleto |
| Feature non migrate (gRPC, ScriptRecorder, GumpInspector, AutoDoc) | ❌ Assenti |
| Stub minori sparsi | ⚠️ Da completare |

---

## 2. Criticità Emerse dall'Analisi del Codice

### 2.1 Stub confermati in `MiscApi.cs`

I seguenti metodi sono stub confermati (corpo vuoto o return hardcoded),
verificati direttamente leggendo il file:

| Metodo | Comportamento attuale | Impatto |
|--------|-----------------------|---------|
| `ClearDragQueue()` | Corpo vuoto (stub esplicito) | Basso |
| `ShardName()` | Ritorna `string.Empty` | Medio — script che usano il nome dello shard falliranno silenziosamente |
| `FilterSeason(bool, uint)` | Corpo vuoto | Basso |
| `HasMenu()` | Ritorna `false` | Medio — pre-context-menu UO (server datati) |
| `CloseMenu()` | Corpo vuoto | Medio |
| `MenuContain(string)` | Ritorna `false` | Medio |
| `GetMenuTitle()` | Ritorna `string.Empty` | Medio |
| `WaitForMenu(int)` | Ritorna `false` | Medio |
| `MenuResponse(string)` | Corpo vuoto | Medio |
| `HasQueryString()` | Ritorna `false` | Medio |
| `WaitForQueryString(int)` | Ritorna `false` | Medio |
| `QueryStringResponse(bool, string)` | Corpo vuoto | Medio |
| `GetMapInfo(uint)` | Ritorna oggetto con solo Serial popolato | Basso |
| `NoRunStealthToggle(bool)` | Parziale — scrive config ma non invia pacchetto | Basso |

### 2.2 Stub in `PlayerApi.cs`

| Metodo | Comportamento attuale | Impatto |
|--------|-----------------------|---------|
| `HashSet(string key, object value)` | Corpo completamente vuoto | Basso — feature raramente usata |
| `SpellIsEnabled(string spellName)` | Ritorna sempre `true` hardcoded | Medio — script che controllano se una spell è abilitata non funzionano |

### 2.3 Stub in `StaticsApi.cs`

| Metodo | Comportamento attuale | Impatto |
|--------|-----------------------|---------|
| `CheckDeedHouse()` | Ritorna sempre `false` hardcoded | Basso |

### 2.4 `Mobile.LastObject` mai aggiornato

`TargetApi.LastUsedObject()` legge correttamente `Player.LastObject`, ma questa proprietà
non viene mai scritta: il `WorldPacketHandler` non osserva il pacchetto `0x06`
(DoubleClick inviato dal client) per aggiornare `LastObject`. Il valore sarà sempre `0`.

> **Nota:** `0x08` in `WorldPacketHandler` è registrato come `HandleDropRequest` (pacchetto drop C2S),
> non come observer del double-click. Sono due pacchetti distinti.

### 2.5 `CommonConverters.cs` — 10 `ConvertBack` con `NotImplementedException`

I seguenti converter WPF hanno il metodo `ConvertBack` che lancia eccezione.
Per i binding one-way (`Mode=OneWay`) questo non è un problema. Diventa un crash
se un binding bidirezionale usa questi converter per errore.

Converter coinvolti: `NullToVisibilityConverter`, `NotNullToVisibilityConverter`,
`HotkeyDisplayConverter`, `CaptureAppearanceConverter`, `SpellIconConverter`,
`IntToVisibilityConverter`, `CountToVisibilityConverter`, `BooleanToTextConverter`,
`BooleanToIconConverter`, `BooleanToAppearanceConverter`.

### 2.6 TODO aperti nella UI

| File | Riga | Descrizione |
|------|------|-------------|
| `GumpListViewModel.cs` | 59 | "Apri Gump Inspector con questo gump" — la finestra non esiste |
| `InspectorViewModel.cs` | 250 | "Implementare targeting specifico per locazione/terreno nel targetingService" |

### 2.7 Macro Engine — Comandi di esecuzione mancanti

`MacrosService.ExecuteActionAsync()` gestisce attualmente questi comandi:
`IF/ELSEIF/ELSE/ENDIF`, `WHILE/ENDWHILE`, `FOR/ENDFOR`, `PAUSE/WAIT`, `SAY/MSG`,
`DOUBLECLICK/DCLICK`, `SINGLECLICK`, `TARGET`, `CAST`, `USESKILL`, `ATTACK`,
`WAITFORTARGET`, `USETYPE`, `EQUIPITEM`, `MOUNT`, `DISMOUNT`, `RESPONDGUMP`.

I seguenti comandi presenti nel legacy **non sono implementati**:

| Comando | Descrizione |
|---------|-------------|
| `WARMODE on/off/toggle` | Attiva/disattiva modalità guerra |
| `FLY` / `LAND` | Toggle volo |
| `MOVEITEM <serial> <dest> [amount]` | Sposta item tra container |
| `PICKUP <serial> [amount]` | Raccoglie item nel backpack |
| `DROP <serial> <x> <y> <z>` | Lascia cadere item a terra |
| `USECONTEXTMENU <serial> <entry>` | Apre e usa voce del menu contestuale |
| `WAITFORGUMP <serial> <timeout>` | Attende arrivo di un gump specifico |
| `ARMDISARM` | Equip/unequip arma a due mani |
| `BANDAGE [target_serial]` | Usa bende sul target (o su se stessi) |
| `TARGETRESOURCE <serial> <resource>` | Target per raccolta risorse (mining, ecc.) |
| `DISCONNECT` | Disconnette dal server |
| `RESYNC` | Invia pacchetto di risincronizzazione |
| `CLEARJOURNAL` | Svuota il journal |
| `INVOKEVIRTUE <name>` | Attiva virtù (Honor, Sacrifice, Valor, ecc.) |
| `EMOTE <text>` | Invia azione emote |
| `RENAMEMOBILE <serial> <name>` | Rinomina un mobile |
| `RUNORGANIZER` | Esegue l'Organizer agent una volta |
| `USEPOTIONTYPE <name>` | Usa una pozione dal backpack per tipo |
| `SETALIAS <name> <serial>` | Imposta un alias locale |
| `REMOVEALIAS <name>` | Rimuove un alias locale |
| `PROMPTRESPONSE <text>` | Risponde a un prompt di testo del server |

### 2.8 Macro Recording — Azioni non catturate

Il recorder di `MacrosService` cattura: `DOUBLECLICK`, `SINGLECLICK`, `SAY`, `TARGET`,
`CAST`, `USESKILL`, `ATTACK`. Non cattura:

| Azione | Pacchetto |
|--------|-----------|
| Cambio WarMode | `0x72` |
| Toggle volo | `0xBF.0x32` |
| Set alias | N/A — locale |
| Remove alias | N/A — locale |

---

## 3. Feature Non Migrate

Le seguenti funzionalità del legacy TMRazor **non hanno alcun file corrispondente**
in TMRazorImproved (verificato con ricerca sul filesystem):

| Feature | Legacy | TMRazorImproved |
|---------|--------|-----------------|
| **gRPC Proto-Control server** | `Proto-Control/` (4 file + `.proto`) | ❌ Zero file |
| **Script Recorder** | `ScriptRecorderService` + 3 impl. (Python/C#/UOSteam) | ❌ Zero file |
| **AutoDoc** | `AutoDoc.cs` (genera `api.json` da XML comments) | ❌ Zero file |
| **Gump Inspector Window** | `GumpInspector.cs` | ❌ Zero file |
| **Build / Publish pipeline** | N/A (era .NET Framework) | ❌ Non configurata |

---

## 4. Task List — Tutto Quello da Fare per la Migrazione al 100%

I task sono ordinati per priorità. Inizia sempre dall'alto verso il basso.

---

### TASK-01 — Aggiornare `Mobile.LastObject` dal pacchetto DoubleClick
**Priorità:** 🔴 Alta — `LastUsedObject()` ritorna sempre 0
**File:** `TMRazorImproved.Core/Handlers/WorldPacketHandler.cs`
**Tempo stimato:** 30 minuti

**Contesto:**
`TargetApi.LastUsedObject()` legge `Player.LastObject`. Questa proprietà però non viene mai
scritta perché non esiste un observer sul pacchetto `0x06` (DoubleClick C2S).

**Passi:**

1. Aprire `WorldPacketHandler.cs`
2. Nel metodo che registra i viewer dei pacchetti (cerca `RegisterViewer`), aggiungere:
   ```csharp
   _packetService.RegisterViewer(PacketPath.ClientToServer, 0x06, HandleDoubleClick);
   ```
3. Aggiungere il metodo handler:
   ```csharp
   private void HandleDoubleClick(byte[] data)
   {
       // 0x06: cmd(1) serial(4)
       if (data.Length < 5) return;
       var reader = new UOBufferReader(data);
       reader.ReadByte(); // skip cmd
       uint serial = reader.ReadUInt32();
       var player = _worldService.Player;
       if (player != null)
           lock (player.SyncRoot) { player.LastObject = serial; }
   }
   ```
4. Compilare (`dotnet build`) e verificare che non ci siano errori
5. Aprire `TMRazorImproved.Tests/MockTests/Networking/WorldPacketHandlerTests.cs`
   e aggiungere un test che verifica che dopo il pacchetto `0x06`, `Player.LastObject` sia aggiornato

---

### TASK-02 — Completare `SpellIsEnabled` in `PlayerApi.cs`
**Priorità:** 🟠 Media
**File:** `TMRazorImproved.Core/Services/Scripting/Api/PlayerApi.cs`
**Tempo stimato:** 45 minuti

**Contesto:**
`SpellIsEnabled(string spellName)` ritorna sempre `true`. Nel legacy leggeva dalla configurazione
del profilo se la spell è nella lista delle spell abilitate/disabilitate.

**Passi:**

1. Aprire `PlayerApi.cs`, trovare il metodo `SpellIsEnabled`
2. Aprire `TMRazorImproved.Shared/Interfaces/IConfigService.cs` e verificare se esiste
   una lista di spell abilitate nel profilo (cerca `EnabledSpells`, `DisabledSpells`, o simili)
3. **Se la lista esiste nel profilo:**
   - Implementare: `return _config.GetCurrentProfile()?.EnabledSpells?.Contains(spellName) ?? true;`
4. **Se la lista non esiste nel profilo:**
   - Aggiungere `List<string> DisabledSpells` al modello di profilo in `ConfigModels.cs`
   - Implementare il controllo
5. Testare con uno script Python: `if Player.SpellIsEnabled("Greater Heal"): ...`

---

### TASK-03 — Implementare `ShardName()` in `MiscApi.cs`
**Priorità:** 🟠 Media
**File:** `TMRazorImproved.Core/Services/Scripting/Api/MiscApi.cs`
**Tempo stimato:** 30 minuti

**Contesto:**
`ShardName()` ritorna `string.Empty`. Il nome dello shard è disponibile nel profilo di configurazione
(campo Server/Shard).

**Passi:**

1. Aprire `MiscApi.cs`, trovare il metodo `ShardName()`
2. Verificare in `IConfigService` come si legge il nome del shard (cerca `ShardName`, `ServerName`, `Server`)
3. Implementare: `return _config.GetCurrentProfile()?.ShardName ?? string.Empty;`
4. Il nome dello shard viene tipicamente impostato nella pagina General durante la configurazione

---

### TASK-04 — Completare le azioni mancanti nel Macro Engine
**Priorità:** 🟠 Alta — molti script legacy usano queste azioni
**File:** `TMRazorImproved.Core/Services/MacrosService.cs`
**Tempo stimato:** 5–7 ore totali (suddiviso in sottotask)

**Contesto:**
Il `MacrosService` interpreta un testo riga per riga e per ogni riga chiama `ExecuteActionAsync()`.
Per aggiungere un nuovo comando basta aggiungere un `case` allo switch esistente.
I servizi disponibili (iniettati nel costruttore) sono: `_packetService`, `_worldService`, `_targetingService`.

---

#### TASK-04.1 — Aggiungere `WARMODE`
**Tempo:** 20 minuti

**Passi:**
1. Aprire `MacrosService.cs`, trovare lo switch in `ExecuteActionAsync`
2. Aggiungere:
   ```csharp
   case "WARMODE":
   {
       bool enable = args.Equals("on", StringComparison.OrdinalIgnoreCase) ||
                     (args.Equals("toggle", StringComparison.OrdinalIgnoreCase)
                         ? !(_worldService.Player?.WarMode ?? false)
                         : false);
       // Pacchetto 0x72: cmd(1) flag(1) unk(1) unk(1) unk(1)
       var pkt = new byte[] { 0x72, (byte)(enable ? 1 : 0), 0x00, 0x32, 0x00 };
       _packetService.SendToServer(pkt);
       break;
   }
   ```
3. Aggiungere anche nel recorder (sezione `StartRecording`): osservare pacchetto `0x72` C2S e
   registrare `WARMODE on` o `WARMODE off` in base al flag

---

#### TASK-04.2 — Aggiungere `FLY` e `LAND`
**Tempo:** 20 minuti

**Passi:**
1. Aggiungere i case:
   ```csharp
   case "FLY":
   case "LAND":
   {
       // Pacchetto 0xBF sub-command 0x32: toggle volo
       var pkt = new byte[] { 0xBF, 0x00, 0x05, 0x00, 0x32, 0x00 };
       _packetService.SendToServer(pkt);
       break;
   }
   ```
2. Aggiungere nel recorder: osservare `0xBF` + sub `0x32` e registrare `FLY` o `LAND`
   in base allo stato corrente del player (`Player.IsFlying`)

---

#### TASK-04.3 — Aggiungere `DISCONNECT` e `RESYNC`
**Tempo:** 20 minuti

**Passi:**
1. Aggiungere:
   ```csharp
   case "DISCONNECT":
       _clientInterop.Disconnect();
       break;

   case "RESYNC":
   {
       // Pacchetto 0x22 0xFF: resync
       _packetService.SendToServer(new byte[] { 0x22, 0xFF });
       break;
   }
   ```
2. Per `DISCONNECT`, iniettare `IClientInteropService` nel costruttore di `MacrosService`
   (probabilmente è già presente come dipendenza — verificare)

---

#### TASK-04.4 — Aggiungere `CLEARJOURNAL`
**Tempo:** 15 minuti

**Passi:**
1. Iniettare `IJournalService` nel costruttore di `MacrosService` (se non già presente)
2. Aggiungere:
   ```csharp
   case "CLEARJOURNAL":
       _journalService.Clear();
       break;
   ```

---

#### TASK-04.5 — Aggiungere `MOVEITEM`, `PICKUP`, `DROP`
**Tempo:** 45 minuti

**Contesto:**
Questi tre comandi usano i pacchetti di lift/drop del protocollo UO:
- Lift (`0x07`): prende un item in mano
- Drop (`0x08`): rilascia l'item in mano su container o a terra

**Passi per `MOVEITEM <serial> <destContainer> [amount]`:**
1. Aggiungere:
   ```csharp
   case "MOVEITEM":
   {
       var parts = args.Split(' ');
       if (parts.Length < 2) break;
       uint serial = Convert.ToUInt32(parts[0], parts[0].StartsWith("0x") ? 16 : 10);
       uint dest   = Convert.ToUInt32(parts[1], parts[1].StartsWith("0x") ? 16 : 10);
       ushort amount = parts.Length > 2 ? ushort.Parse(parts[2]) : (ushort)0;

       // Pacchetto 0x07: cmd(1) serial(4) amount(2)
       var lift = new byte[7];
       lift[0] = 0x07;
       // serial big-endian
       lift[1] = (byte)(serial >> 24); lift[2] = (byte)(serial >> 16);
       lift[3] = (byte)(serial >>  8); lift[4] = (byte)(serial);
       lift[5] = (byte)(amount >>  8); lift[6] = (byte)(amount);
       _packetService.SendToServer(lift);

       await Task.Delay(100, ct); // Breve pausa tra lift e drop

       // Pacchetto 0x08: cmd(1) serial(4) x(2) y(2) z(1) containerSerial(4)
       var drop = new byte[14];
       drop[0] = 0x08;
       drop[1] = (byte)(serial >> 24); drop[2] = (byte)(serial >> 16);
       drop[3] = (byte)(serial >>  8); drop[4] = (byte)(serial);
       drop[5] = 0xFF; drop[6] = 0xFF; // x = 0xFFFF
       drop[7] = 0xFF; drop[8] = 0xFF; // y = 0xFFFF
       drop[9] = 0x00;                 // z = 0
       drop[10] = (byte)(dest >> 24); drop[11] = (byte)(dest >> 16);
       drop[12] = (byte)(dest >>  8); drop[13] = (byte)(dest);
       _packetService.SendToServer(drop);
       break;
   }
   ```

2. **`PICKUP <serial> [amount]`**: stesso approccio ma il container destinazione è il serial del backpack
   (`_worldService.Player?.Backpack?.Serial ?? 0`)

3. **`DROP <serial> <x> <y> <z>`**: lift + drop con coordinate invece del container serial
   (container serial = `0xFFFFFFFF` per drop a terra)

---

#### TASK-04.6 — Aggiungere `EMOTE`, `INVOKEVIRTUE`, `RENAMEMOBILE`
**Tempo:** 30 minuti

**`EMOTE <text>`:**
```csharp
case "EMOTE":
{
    // Pacchetto 0xAD: speech con type 0x03 (emote)
    // usa già _packetService.SendSpeech(...) se disponibile
    // altrimenti costruisci pacchetto 0xAD manualmente con type=0x03
    break;
}
```

**`INVOKEVIRTUE <name>`:**
```csharp
case "INVOKEVIRTUE":
{
    var virtueMap = new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase)
    {
        ["honor"]     = 0x01, ["sacrifice"] = 0x02, ["valor"]    = 0x04,
        ["compassion"]= 0x08, ["honesty"]   = 0x10, ["humility"] = 0x20,
        ["justice"]   = 0x40, ["spirituality"] = 0x80
    };
    if (virtueMap.TryGetValue(args.Trim(), out byte id))
    {
        // Pacchetto 0x12: cmd(1) type(1)=0x69 len(1) id(1) 0x00
        var pkt = new byte[] { 0x12, 0x69, 0x02, id, 0x00 };
        _packetService.SendToServer(pkt);
    }
    break;
}
```

**`RENAMEMOBILE <serial> <newName>`:**
```csharp
case "RENAMEMOBILE":
{
    var idx = args.IndexOf(' ');
    if (idx < 0) break;
    uint serial = Convert.ToUInt32(args[..idx].Trim(), 16);
    string name = args[(idx + 1)..].Trim();
    // Pacchetto 0x75: cmd(1) serial(4) name(30, null-terminated)
    var pkt = new byte[35];
    pkt[0] = 0x75;
    pkt[1] = (byte)(serial >> 24); pkt[2] = (byte)(serial >> 16);
    pkt[3] = (byte)(serial >> 8);  pkt[4] = (byte)(serial);
    var nameBytes = System.Text.Encoding.ASCII.GetBytes(name);
    Array.Copy(nameBytes, 0, pkt, 5, Math.Min(nameBytes.Length, 29));
    _packetService.SendToServer(pkt);
    break;
}
```

---

#### TASK-04.7 — Aggiungere `USECONTEXTMENU` e `WAITFORGUMP`
**Tempo:** 45 minuti

**`USECONTEXTMENU <serial> <entryIndex>`:**
```csharp
case "USECONTEXTMENU":
{
    var parts = args.Split(' ');
    if (parts.Length < 2) break;
    uint serial = Convert.ToUInt32(parts[0], 16);
    ushort entry = ushort.Parse(parts[1]);
    // Request menu: 0xBF sub 0x13
    var req = new byte[] { 0xBF, 0x00, 0x07, 0x00, 0x13,
        (byte)(serial >> 8), (byte)serial };
    _packetService.SendToServer(req);
    await Task.Delay(150, ct);
    // Response: 0xBF sub 0x15
    var resp = new byte[] { 0xBF, 0x00, 0x09, 0x00, 0x15,
        (byte)(serial >> 8), (byte)serial,
        (byte)(entry >> 8), (byte)entry };
    _packetService.SendToServer(resp);
    break;
}
```

**`WAITFORGUMP <typeId> <timeout>`:**
```csharp
case "WAITFORGUMP":
{
    var parts = args.Split(' ');
    uint typeId = Convert.ToUInt32(parts[0], 16);
    int timeout = parts.Length > 1 ? int.Parse(parts[1]) : 5000;

    var tcs = new TaskCompletionSource<bool>();
    void Handler(UOPacketMessage msg)
    {
        // msg.Data[0] == 0xB0 (SendGump)
        // parsare il typeId dal pacchetto e confrontare
        if (msg.Data != null && msg.Data.Length > 8)
        {
            // offset 5: typeId uint32 big-endian
            uint t = (uint)(msg.Data[5] << 24 | msg.Data[6] << 16 |
                            msg.Data[7] << 8  | msg.Data[8]);
            if (t == typeId) tcs.TrySetResult(true);
        }
    }
    _packetService.OnPacketReceived += Handler;
    try
    {
        await Task.WhenAny(tcs.Task, Task.Delay(timeout, ct));
    }
    finally
    {
        _packetService.OnPacketReceived -= Handler;
    }
    break;
}
```

---

#### TASK-04.8 — Aggiungere `ARMDISARM`, `BANDAGE`, `TARGETRESOURCE`, `USEPOTIONTYPE`
**Tempo:** 45 minuti

**`ARMDISARM`:**
```csharp
case "ARMDISARM":
{
    // Cerca un item nel layer "TwoHanded" o "MainHand"
    var player = _worldService.Player;
    if (player == null) break;
    var weapon = player.GetItemOnLayer(Layer.MainHand)
                 ?? player.GetItemOnLayer(Layer.TwoHanded);
    if (weapon != null)
    {
        // Unequip: lift → drop backpack
        // [usa logica simile a MOVEITEM con backpack come destinazione]
    }
    break;
}
```

**`BANDAGE [target_serial]`:**
```csharp
case "BANDAGE":
{
    // Trova bende nel backpack (graphic 0x0E21)
    var bandage = /* Items.FindByID(0x0E21, -1, backpackSerial) */;
    if (bandage == null) break;
    uint target = string.IsNullOrEmpty(args)
        ? _worldService.Player?.Serial ?? 0
        : Convert.ToUInt32(args, 16);
    // DoubleClick bandage + target
    _packetService.SendToServer(new byte[] { 0x06,
        (byte)(bandage.Serial >> 24), (byte)(bandage.Serial >> 16),
        (byte)(bandage.Serial >> 8),  (byte)(bandage.Serial) });
    await Task.Delay(100, ct);
    _targetingService.SendTarget(target);
    break;
}
```

**`TARGETRESOURCE <serial> <resourceName>`:**
```csharp
case "TARGETRESOURCE":
{
    var parts = args.Split(' ');
    if (parts.Length < 2) break;
    uint serial = Convert.ToUInt32(parts[0], 16);
    // usa TargetApi.TargetResource(serial, resourceName) se accessibile
    // altrimenti costruisci direttamente il pacchetto TargetByResource
    break;
}
```

**`USEPOTIONTYPE <potionName>`:**
```csharp
case "USEPOTIONTYPE":
{
    var potionGraphics = new Dictionary<string, ushort>(StringComparer.OrdinalIgnoreCase)
    {
        ["heal"] = 0x0F0C,  ["cure"] = 0x0F07,  ["refresh"] = 0x0F0B,
        ["agility"] = 0x0F08, ["strength"] = 0x0F09, ["explosion"] = 0x0F0D
    };
    if (!potionGraphics.TryGetValue(args.Trim(), out ushort graphic)) break;
    // FindByID nel backpack e DoubleClick
    break;
}
```

---

#### TASK-04.9 — Aggiungere `SETALIAS`, `REMOVEALIAS`, `PROMPTRESPONSE`, `RUNORGANIZER`
**Tempo:** 30 minuti

Aggiungere un dizionario `_aliases` privato in `MacrosService` (già parzialmente presente per le variabili):

**`SETALIAS <name> <serial>`:**
```csharp
case "SETALIAS":
{
    var idx = args.IndexOf(' ');
    if (idx < 0) break;
    string aliasName = args[..idx].Trim();
    uint serial = Convert.ToUInt32(args[(idx + 1)..].Trim(), 16);
    _aliases[aliasName.ToLower()] = serial;
    break;
}
```

**`REMOVEALIAS <name>`:**
```csharp
case "REMOVEALIAS":
    _aliases.Remove(args.Trim().ToLower());
    break;
```

**Aggiornare la risoluzione serial** in tutti i case esistenti: prima di `Convert.ToUInt32`,
controllare se il valore è un alias:
```csharp
uint ResolveSerial(string s) =>
    _aliases.TryGetValue(s.ToLower(), out uint v) ? v
    : Convert.ToUInt32(s, s.StartsWith("0x") ? 16 : 10);
```

**`PROMPTRESPONSE <text>`:**
```csharp
case "PROMPTRESPONSE":
    _targetingService.SendPrompt(args);
    break;
```

**`RUNORGANIZER`:**
```csharp
case "RUNORGANIZER":
    await _organizerService.RunOnceAsync(ct);
    break;
```
Iniettare `IOrganizerService` nel costruttore se non già presente.

---

### TASK-05 — Creare la `GumpInspectorWindow`
**Priorità:** 🟠 Media
**File:** Nuovi file in `TMRazorImproved.UI/`
**Tempo stimato:** 3–4 ore

**Contesto:**
`GumpListViewModel.cs:59` ha un TODO per aprire il Gump Inspector. La finestra deve mostrare
la struttura di un gump ricevuto dal server (titolo, layout XML, lista pulsanti e testi).

#### TASK-05.1 — Creare il ViewModel
1. Creare `TMRazorImproved.UI/ViewModels/GumpInspectorViewModel.cs`:
   ```csharp
   public partial class GumpInspectorViewModel : ObservableObject
   {
       [ObservableProperty] private uint _serial;
       [ObservableProperty] private uint _typeId;
       [ObservableProperty] private string _rawLayout = string.Empty;
       [ObservableProperty] private ObservableCollection<GumpControl> _controls = new();
   }
   ```
2. Popolare i dati da `IWorldService.GetGump(serial)` o `IWorldService.CurrentGump`

#### TASK-05.2 — Creare la Window XAML
1. Creare `TMRazorImproved.UI/Views/Windows/GumpInspectorWindow.xaml`
2. Layout minimo:
   - Header: Serial, TypeId, dimensioni W/H
   - Tab 1 "Layout": TextBox read-only con il layout grezzo
   - Tab 2 "Controlli": DataGrid con colonne Tipo, Testo, X, Y, W, H, ButtonID
   - Pulsante "Invia Risposta": apre un dialog per inserire il ButtonID e inviarla

#### TASK-05.3 — Collegare il TODO nel GumpListViewModel
1. Aprire `GumpListViewModel.cs` riga 59
2. Sostituire il commento TODO con:
   ```csharp
   var win = new GumpInspectorWindow(SelectedGump.Serial, _world);
   win.Show();
   ```
3. Registrare `GumpInspectorWindow` nel DI container se necessario

---

### TASK-06 — Implementare il Sistema di Script Recorder
**Priorità:** 🟡 Media — feature importante per l'usabilità
**Tempo stimato:** 6–8 ore

**Contesto:**
Il legacy ha un `ScriptRecorderService` che ascolta le azioni dell'utente (click, target, cast, skill)
e genera automaticamente il codice corrispondente in Python, C# o UOSteam.
Non esiste nessun file corrispondente in TMRazorImproved.

#### TASK-06.1 — Creare l'interfaccia
1. Creare `TMRazorImproved.Shared/Interfaces/IScriptRecorderService.cs`:
   ```csharp
   public interface IScriptRecorderService
   {
       bool IsRecording { get; }
       void StartRecording(ScriptLanguage language);
       string StopRecording();  // restituisce il codice generato
   }
   ```

#### TASK-06.2 — Creare il servizio
1. Creare `TMRazorImproved.Core/Services/ScriptRecorderService.cs`
2. Nel costruttore, iniettare `IPacketService`
3. In `StartRecording`, sottoscriversi agli eventi dei pacchetti C2S via `IPacketService.RegisterViewer`:
   - `0x06` → genera `Items.UseItem(0x{serial})`
   - `0x09` → genera `Items.SingleClick(0x{serial})`
   - `0xAD` → genera `Player.ChatMessage("{text}")`
   - `0x6C` → genera `Target.TargetExecute(0x{serial})`
   - `0x12` type `0x56`/`0x27` → genera `Spells.Cast("{spellName}")`
   - `0x12` type `0x24` → genera `Player.UseSkill("{skillName}")`
   - `0x05` → genera `Player.Attack(0x{serial})`
   - `0x72` → genera `Player.SetWarMode(true/false)`
4. In `StopRecording`, de-sottoscriversi e restituire il codice accumulato nel buffer
5. Implementare generatori separati per Python, C# e UOSteam (3 classi interne `PyGenerator`, `CsGenerator`, `UosGenerator`)

#### TASK-06.3 — Collegare il recorder alla pagina Scripting
1. In `ScriptingViewModel.cs`, aggiungere:
   ```csharp
   [RelayCommand] void StartRecord() => _recorder.StartRecording(SelectedLanguage);
   [RelayCommand] void StopRecord()  { Code = _recorder.StopRecording(); }
   ```
2. In `ScriptingPage.xaml`, aggiungere un pulsante "⏺ Registra" / "⏹ Stop" nella toolbar
3. Il codice generato viene inserito nell'editor AvalonEdit

---

### TASK-07 — Implementare il gRPC Proto-Control Server
**Priorità:** 🟡 Media — permette controllo remoto da script esterni
**Tempo stimato:** 5–6 ore

**Contesto:**
Il legacy espone un server gRPC su porta configurabile che permette a script Python/altri
di controllare TMRazor remotamente senza aprire la UI.
Non esistono file corrispondenti in TMRazorImproved.

#### TASK-07.1 — Aggiungere le dipendenze
1. Aprire `TMRazorImproved.Core/TMRazorImproved.Core.csproj`
2. Aggiungere:
   ```xml
   <PackageReference Include="Google.Protobuf" Version="3.29.3" />
   <PackageReference Include="Grpc.Core" Version="2.46.6" />
   <PackageReference Include="Grpc.Tools" Version="2.70.0" />
   ```

#### TASK-07.2 — Creare il file `.proto`
1. Creare `TMRazorImproved.Core/Proto/ProtoControl.proto`
2. Copiare (e adattare) il contenuto da `/home/user/TMRazor/Razor/RazorEnhanced/Proto-Control/ProtoControl.proto`
3. Aggiungere nel `.csproj`:
   ```xml
   <Protobuf Include="Proto/ProtoControl.proto" GrpcServices="Server" />
   ```
4. Compilare → il codice C# stub viene generato automaticamente da `Grpc.Tools`

#### TASK-07.3 — Implementare il server
1. Creare `TMRazorImproved.Core/Services/ProtoControlService.cs`
2. Implementare la classe derivata dal server stub generato
3. I metodi principali da implementare:
   - `ExecuteScript(request)` → chiama `IScriptingService.RunAsync(...)`
   - `GetPlayerStatus(request)` → legge da `IWorldService.Player`
   - `GetItems(request)` → query su `IWorldService.Items`
   - `SendPacket(request)` → delega a `IPacketService.SendToServer(...)`
4. Registrare il servizio in `App.xaml.cs` come `IHostedService`:
   ```csharp
   services.AddHostedService<ProtoControlService>();
   ```
5. Porta di default: 50051, configurabile nel profilo

---

### TASK-08 — Implementare AutoDoc (generazione `api.json`)
**Priorità:** 🟢 Bassa
**Tempo stimato:** 2–3 ore

**Contesto:**
Il legacy ha un `AutoDoc.cs` che genera un file `api.json` leggendo i commenti XML
dai file delle API. Questo file è usato per il completamento automatico nell'editor.
`CompletionService.cs` in `TMRazorImproved.UI` è già pronto a caricare dati di completamento.

#### TASK-08.1 — Creare il generatore
1. Creare `TMRazorImproved.Core/Utilities/ApiDocGenerator.cs`
2. Usare reflection per enumerare tutte le classi API (`PlayerApi`, `ItemsApi`, ecc.)
3. Per ogni metodo pubblico, estrarre il commento `<summary>` dal file XML di documentazione
   (il file `.xml` viene generato automaticamente da `dotnet build` se aggiungi `<GenerateDocumentationFile>true</GenerateDocumentationFile>` al csproj)
4. Serializzare il risultato in `api.json` nella cartella di output

#### TASK-08.2 — Integrare con il completamento AvalonEdit
1. Aprire `TMRazorImproved.UI/Utilities/CompletionService.cs`
2. Caricare `api.json` generato dal passo precedente
3. Usare i dati per arricchire i suggerimenti di completamento già presenti

---

### TASK-09 — Configurare Build, Publish e CI/CD
**Priorità:** 🟡 Media — necessario per la distribuzione
**Tempo stimato:** 2–3 ore

#### TASK-09.1 — Aggiungere la configurazione di Publish
1. Aprire `TMRazorImproved.UI/TMRazorImproved.UI.csproj`
2. Aggiungere nella `<PropertyGroup>`:
   ```xml
   <PublishSingleFile>true</PublishSingleFile>
   <SelfContained>false</SelfContained>
   <RuntimeIdentifier>win-x86</RuntimeIdentifier>
   ```
3. Creare `publish.ps1` nella root:
   ```powershell
   dotnet publish TMRazorImproved/TMRazorImproved.UI/TMRazorImproved.UI.csproj `
     -c Release -r win-x86 --self-contained false `
     -o ./dist/TMRazorImproved
   ```

#### TASK-09.2 — Configurare GitHub Actions
1. Creare `.github/workflows/build.yml` con:
   ```yaml
   on: [push, pull_request]
   jobs:
     build:
       runs-on: windows-latest
       steps:
         - uses: actions/checkout@v4
         - uses: actions/setup-dotnet@v4
           with: { dotnet-version: '10.x' }
         - run: dotnet build TMRazorImproved/ --configuration Release
         - run: dotnet test TMRazorImproved/ --configuration Release
   ```

#### TASK-09.3 — Test di stabilità memoria
1. Creare `TMRazorImproved.Tests/MockTests/Stress/MemoryLeakTest.cs`
2. Il test deve:
   - Avviare e fermare `ScriptingService` 50 volte con script brevi
   - Misurare `GC.GetTotalMemory(true)` prima e dopo
   - Fallire se la memoria cresce di più del 10% tra inizio e fine

---

### TASK-10 — Stub minori da completare (bassa priorità)
**Priorità:** 🟢 Bassa
**Tempo stimato:** 2 ore totali

Questi stub hanno impatto molto limitato ma andrebbero completati prima del rilascio finale:

#### TASK-10.1 — `ClearDragQueue()` in `MiscApi.cs`
Se esiste un sistema di drag-drop (vedi `DragDropManager` nel legacy), collegare questo metodo
al reset della coda. Se il sistema non è stato migrato, lasciarlo come stub documentato.

#### TASK-10.2 — `FilterSeason()` in `MiscApi.cs`
Inviare il pacchetto `0xBC` (Season) con il flag appropriato:
```csharp
public override void FilterSeason(bool enable, uint seasonFlag)
{
    _cancel.ThrowIfCancelled();
    var pkt = new byte[] { 0xBC, (byte)seasonFlag, (byte)(enable ? 1 : 0) };
    _packetService.SendToServer(pkt);
}
```

#### TASK-10.3 — `HashSet()` in `PlayerApi.cs`
Valutare se questa feature (salvataggio di coppie chiave/valore locali per script) è necessaria.
Se sì, aggiungere un `Dictionary<string, object>` statico in `MiscApi` (o `ScriptGlobals`)
e implementare `HashSet`, `HashGet`, `HashDelete`.

#### TASK-10.4 — `InspectorViewModel` — ground targeting (TODO riga 250)
Quando l'utente clicca "Target Terreno" nell'Inspector, usare:
```csharp
var target = await _targetingService.AcquireTargetAsync(ct);
// Il pacchetto 0x6C C2S con serial=0 porta X/Y/Z: leggere LastGroundTarget
```

#### TASK-10.5 — Documentare `ConvertBack` in `CommonConverters.cs`
I `ConvertBack` che lanciano `NotImplementedException` sono tecnicamente corretti per converter
one-way, ma è bene aggiungere un commento XML che lo espliciti, per prevenire confusione:
```csharp
/// <inheritdoc />
/// <remarks>Questo converter è one-way. ConvertBack non è supportato.</remarks>
public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    => throw new NotImplementedException("One-way converter.");
```

---

## 5. Riepilogo Priorità

### 🔴 Alta (da fare subito)
| Task | Descrizione | Ore |
|------|-------------|-----|
| TASK-01 | Fix LastObject mai aggiornato (observer 0x06) | 0.5 |
| TASK-04 | Completare macro engine (21 comandi mancanti) | 7 |

### 🟠 Media (da fare nel prossimo sprint)
| Task | Descrizione | Ore |
|------|-------------|-----|
| TASK-02 | SpellIsEnabled reale | 0.75 |
| TASK-03 | ShardName reale | 0.5 |
| TASK-05 | GumpInspectorWindow | 4 |
| TASK-06 | Script Recorder | 7 |
| TASK-09 | Build pipeline e CI/CD | 3 |

### 🟡 Bassa (backlog)
| Task | Descrizione | Ore |
|------|-------------|-----|
| TASK-07 | gRPC Proto-Control server | 6 |
| TASK-08 | AutoDoc + completamento | 3 |
| TASK-10 | Stub minori (drag queue, season, hash set, ecc.) | 2 |

**Totale ore stimate: ~34 ore**

---

## 6. Riferimento Rapido — Struttura del Progetto

Per chi non conosce la codebase:

```
TMRazorImproved/
├── TMRazorImproved.Shared/        ← Interfacce, modelli, enums
│   ├── Interfaces/                ← IWorldService, ITargetingService, IPacketService…
│   ├── Models/                    ← UOEntity, Mobile, Item, UOBufferReader…
│   └── Messages/                  ← Messaggi per IMessenger (es. PlayerStatusMessage)
├── TMRazorImproved.Core/          ← Logica pura, nessuna dipendenza WPF
│   ├── Services/                  ← WorldService, PacketService, MacrosService…
│   │   └── Scripting/Api/         ← PlayerApi, TargetApi, SpellsApi, ItemsApi…
│   └── Handlers/                  ← WorldPacketHandler (decodifica pacchetti UO)
├── TMRazorImproved.UI/            ← Tutto WPF
│   ├── ViewModels/                ← Logica UI (si interfaccia con i servizi Core)
│   └── Views/                    ← File XAML + code-behind minimi
├── TMRazorImproved.Tests/         ← Test automatici
│   ├── MockTests/                 ← Test veloci con mock
│   └── IntegrationTests/          ← Test di integrazione (più lenti)
└── TMRazorPlugin/                 ← Plugin per ClassicUO (net48, separato)
```

**Workflow raccomandato:**
1. Modifica → `dotnet build` per verificare la compilazione
2. `dotnet test` per verificare che i test passino
3. Se aggiungi una nuova funzionalità, aggiungi almeno un test in `MockTests/`

---

*Documento generato dall'analisi diretta del codice sorgente — 17 marzo 2026.*
*Nessun documento preesistente (`.md`) è stato usato come fonte per questa revisione.*
