# Review 2 — Bug Fix Report
**Data**: 2026-03-04
**Sprint**: Fix-10 (basato su review2.md)
**Revisore**: Claude Sonnet 4.6

---

## Riepilogo

Tutti i bug P0, P1 e P2-01 identificati in `review2.md` sono stati risolti.
Di seguito la descrizione dettagliata di ogni fix applicato.

---

## Bug P0 (Critici) — TUTTI CORRETTI

### P0-01 — BandageHealService: cursorId=0 nella risposta di targeting
**File**: `TMRazorImproved.Core/Services/BandageHealService.cs`
**Problema**: `PacketBuilder.TargetObject(targetSerial)` usava il default `cursorId=0`. Il server UO rifiuta silenziosamente le risposte di target con cursorId errato, rendendo il bandage heal completamente non funzionante.
**Fix**: Prima di inviare `TargetObject`, si legge `_targetingService.PendingCursorId` (il cursorId ricevuto nel 0x6C S2C) e si chiama `ClearTargetCursor()` prima di costruire il pacchetto.

```csharp
// PRIMA (bug):
_packetService.SendToServer(PacketBuilder.TargetObject(targetSerial));

// DOPO (fix):
uint cursorId = _targetingService.PendingCursorId;
_targetingService.ClearTargetCursor();
_packetService.SendToServer(PacketBuilder.TargetObject(targetSerial, cursorId));
```

---

### P0-02 — SkillsService: ascolto del canale messaggio errato
**File**: `TMRazorImproved.Core/Services/SkillsService.cs`
**Problema**: `SkillsService` implementava `IRecipient<UOPacketMessage>` (messaggio raw), ma `WorldPacketHandler.HandleSkillsUpdate` invia `SkillsUpdatedMessage` (messaggio tipizzato). Le skill non venivano mai aggiornate.
**Fix**: Cambiato `IRecipient<UOPacketMessage>` in `IRecipient<SkillsUpdatedMessage>` e aggiornato il metodo `Receive()`.

```csharp
// PRIMA (bug):
public class SkillsService : ISkillsService, IRecipient<UOPacketMessage>
public void Receive(UOPacketMessage message) { ... }

// DOPO (fix):
public class SkillsService : ISkillsService, IRecipient<SkillsUpdatedMessage>
public void Receive(SkillsUpdatedMessage message)
{
    byte[] data = message.Value;
    if (data.Length > 0 && data[0] == 0x3A)
        HandleSkillUpdate(data);
}
```

---

### P0-03 — WorldService.IsCasting: campo non volatile
**File**: `TMRazorImproved.Core/Services/WorldService.cs`
**Problema**: `IsCasting` era una auto-property `{ get; set; }` — il JIT poteva cachearne il valore in registro, rendendo invisibili le scritture cross-thread (il handler del pacchetto 0x12 C2S e il 0x6C S2C scrivono su un thread, gli script IronPython leggono su un altro).
**Fix**: Aggiunto backing field `volatile bool _isCasting` con proprietà esplicita.

```csharp
// PRIMA (bug):
public bool IsCasting { get; set; }

// DOPO (fix):
private volatile bool _isCasting;
public bool IsCasting { get => _isCasting; set => _isCasting = value; }
```

---

### P0-04 — MacrosService: thread-safety ObservableCollection + volatile
**File**: `TMRazorImproved.Core/Services/MacrosService.cs`
**Problema 1**: `MacroList` è `ObservableCollection<string>` — WPF lancia `InvalidOperationException` se modificata da thread non-UI. `Save()`, `Delete()`, `Rename()` venivano chiamati dall'engine di scripting su thread pool.
**Problema 2**: `IsPlaying` e `IsRecording` erano auto-properties senza `volatile`, visibili male cross-thread.

**Fix 1**: Catturato `SynchronizationContext.Current` nel costruttore (eseguito su UI thread), usato `_uiContext.Post()` per tutte le mutazioni di `MacroList`.
**Fix 2**: Convertite `IsPlaying`/`IsRecording` in `private volatile bool _isPlaying/_isRecording` con proprietà read-only.

```csharp
// Nuovi campi:
private readonly SynchronizationContext? _uiContext;
private volatile bool _isPlaying;
private volatile bool _isRecording;
public bool IsPlaying => _isPlaying;
public bool IsRecording => _isRecording;

// In costruttore:
_uiContext = SynchronizationContext.Current;

// In Save():
if (_uiContext != null)
    _uiContext.Post(_ => MacroList.Add(name), null);
else
    MacroList.Add(name);
```

---

## Bug P1 (Importanti) — TUTTI CORRETTI

### P1-01 — ItemsApi.WaitForContents: messenger hardcoded
**File**: `TMRazorImproved.Core/Services/Scripting/Api/ItemsApi.cs`
**File**: `TMRazorImproved.Core/Services/Scripting/ScriptingService.cs`
**Problema**: `WaitForContents` usava `WeakReferenceMessenger.Default` hardcoded. Nei test unitari, il messenger iniettato (mock/isolato) era diverso dal Default → i test non potevano simulare l'arrivo del `ContainerContentMessage`.
**Fix**: Aggiunto parametro `IMessenger? messenger = null` al costruttore di `ItemsApi` (fallback su `WeakReferenceMessenger.Default` per backward-compat). Aggiornati tutti e 3 i call-site in `ScriptingService` (Python, UOSteam, CSharp) a passare `_messenger`.

```csharp
// PRIMA (bug):
_messenger.Register<ItemsApi, ContainerContentMessage>(this, ...);
// dove _messenger era WeakReferenceMessenger.Default hardcoded

// DOPO (fix):
public ItemsApi(..., IMessenger? messenger = null)
{
    _messenger = messenger ?? WeakReferenceMessenger.Default;
}
```

---

### P1-02 — TargetingService.SendPrompt: serial/promptId non tracciati
**File**: `TMRazorImproved.Core/Services/TargetingService.cs`
**File**: `TMRazorImproved.Shared/Interfaces/ITargetingService.cs`
**Problema**: `SendPrompt()` costruiva il pacchetto 0x9A C2S con `serial=0` e `promptId=0`. Il server UO ignora le risposte di prompt con serial errato — gli script che usavano `PromptMsg()` non funzionavano.
**Fix**:
1. Aggiunto viewer `PacketPath.ServerToClient, 0x9A` → `HandlePromptFromServer` che legge serial (offset 3) e promptId (offset 7) e li salva in campi `volatile`.
2. `SendPrompt()` usa i valori tracciati, poi li azzera.
3. Aggiunte `PendingPromptSerial` e `PendingPromptId` all'interfaccia.

```csharp
// Nuovi campi:
private volatile uint _pendingPromptSerial;
private volatile uint _pendingPromptId;

// Nuovo handler 0x9A S2C:
private void HandlePromptFromServer(byte[] data)
{
    if (data.Length < 15) return;
    _pendingPromptSerial = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(3));
    _pendingPromptId = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(7));
    SetPrompt(true);
}

// In SendPrompt():
BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(3), _pendingPromptSerial);
BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(7), _pendingPromptId);
_pendingPromptSerial = 0;
_pendingPromptId = 0;
```

---

### P1-03 — MacrosService: IsPlaying/IsRecording non volatile
Risolto insieme a P0-04 (vedere sopra).

---

### P1-04 — PacketBuilder.DropToContainer: byte grid implicito (SA)
**File**: `TMRazorImproved.Core/Utilities/PacketBuilder.cs`
**Problema**: Il byte `grid` al offset [10] era presente (il buffer aveva 15 byte) ma implicito — non documentato, non configurabile. L'analisi ha confermato che il layout 15-byte è già compatibile SA (`grid=0` è il default corretto), ma il codice era fuorviante.
**Fix**: Reso esplicito con parametro `byte grid = 0`, commento sul layout SA, e assegnazione diretta `pkt[10] = grid`.

```csharp
// DOPO (fix):
public static byte[] DropToContainer(uint serial, uint containerSerial, byte grid = 0)
{
    byte[] pkt = new byte[15];
    pkt[0]  = 0x08;
    // ...
    pkt[10] = grid;   // Grid slot (SA: 0 = random)
    BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(11), containerSerial);
    return pkt;
}
```

---

## Bug P2 — CORRETTI

### P2-01 — SkillsService: thread-safety su List<SkillInfo>
Risolto insieme a P0-02 (vedere sopra). Aggiunti `_skillsLock`, lock su `HandleSkillUpdate`, `ResetDelta`, `SetLock` e le proprietà `Skills`, `TotalReal`, `TotalBase`.

---

## File Modificati

| File | Bug risolti |
|------|-------------|
| `Core/Services/BandageHealService.cs` | P0-01 |
| `Core/Services/SkillsService.cs` | P0-02, P2-01 |
| `Core/Services/WorldService.cs` | P0-03 |
| `Core/Services/MacrosService.cs` | P0-04, P1-03 |
| `Core/Services/Scripting/Api/ItemsApi.cs` | P1-01 |
| `Core/Services/Scripting/ScriptingService.cs` | P1-01 |
| `Core/Services/TargetingService.cs` | P1-02 |
| `Core/Utilities/PacketBuilder.cs` | P1-04 |
| `Shared/Interfaces/ITargetingService.cs` | P1-02 |

---

## Note architetturali

- **P0-02**: il canale `SkillsUpdatedMessage` è il modo corretto per ricevere aggiornamenti skill — `WorldPacketHandler` parsifica 0x3A e pubblica il messaggio tipizzato. `SkillsService` non deve più intercettare il raw `UOPacketMessage`.
- **P0-03**: `volatile` è sufficiente per `bool` cross-thread (lettura/scrittura atomica su architetture x86/x64). Il lock sarebbe overkill per un singolo flag.
- **P0-04**: `SynchronizationContext` è preferito a `Dispatcher` nel Core layer (nessuna dipendenza WPF). Il contesto viene catturato in costruzione, che avviene sempre su UI thread grazie a `App.xaml.cs`.
- **P1-02**: Il parser 0x9A S2C è robusto: guard `data.Length < 15`, offset documentati, azzeramento campi dopo l'invio.

---

## Prossimi step (post-fix)

1. **Fase 3.3**: `DisplayPage` + `DisplayViewModel` (chat/journal UI)
2. **ToolBar overlay**: floating window sopra il client UO
3. **Test coverage**: `SkillsService` (0x3A parsing), `TargetingService` (0x9A prompt tracking), nuovi filtri `FilterHandler`

---

# Sprint 11 — UI Completamento
**Data**: 2026-03-04
**Sviluppatore**: Claude Sonnet 4.6

## Riepilogo

Tutti e 5 gli obiettivi di Sprint 11 sono stati implementati e la build è verde (69/69 test passati).

---

## 1. SoundPage — Volume control reale (Windows Core Audio API)

**Problema**: `ISoundService` aveva solo `PlaySound/PlayMusic/StopMusic` senza alcun controllo del volume del processo UO.

**Implementazione**:

- **`ISoundService.cs`**: aggiunti `void SetVolume(float volume)` e `float GetVolume()`.
- **`SoundService.cs`**: implementato controllo volume tramite Windows Core Audio API (COM P/Invoke):
  - `IMMDeviceEnumerator` → `GetDefaultAudioEndpoint(eRender, eMultimedia)`
  - `IAudioSessionManager2.GetSessionEnumerator()` → iterazione sessioni
  - `IAudioSessionControl2.GetProcessId()` → match con `_clientInterop.GetUOProcessId()`
  - `ISimpleAudioVolume.SetMasterVolume/GetMasterVolume` sulla sessione UO trovata
  - Tutte le interfacce COM dichiarate come tipi privati annidati in `SoundService`
- **`SoundViewModel.cs`**: aggiunta property `Volume` (double 0-100), `OnVolumeChanged` chiama `SetVolume((float)(value/100))`, costruttore legge il volume iniziale.
- **`SoundPage.xaml`**: aggiunta card "Client Volume" con slider 0-100, `SymbolIcon Speaker224`, label percentuale e `InfoBar` informativa.

**File modificati**: `ISoundService.cs`, `SoundService.cs`, `SoundViewModel.cs`, `SoundPage.xaml`

---

## 2. FloatingToolbar — Auto-posizionamento sopra client UO

**Problema**: La `FloatingToolbarWindow` era `Topmost=True` ma non si posizionava automaticamente sopra la finestra del client UO.

**Implementazione**:

- **`FloatingToolbarWindow.xaml.cs`**: refactoring completo del code-behind:
  - Iniettato `IClientInteropService` nel costruttore (per `FindUOWindow()`)
  - `IsVisibleChanged`: quando visibile → chiama `SnapToUOWindow()` + avvia `DispatcherTimer(500ms)`; quando nascosta → ferma il timer
  - `SnapToUOWindow()`: `GetWindowRect(hwnd, out RECT)` → `Left = rect.left`, `Top = rect.top - ActualHeight - 2`, `Width = Clamp(uoWidth, 400, 800)`
  - `_userMoved = true` su `MouseLeftButtonDown` → dopo il drag manuale non re-snappa automaticamente
  - `OnClosed`: stop timer + dispose ViewModel
- **`DisplayViewModel.cs`**:
  - Iniettato `IClientInteropService`
  - Aggiunto `ToggleFloatingToolbarCommand` (show/hide `FloatingToolbarWindow`)
  - Aggiunto `ToggleOverheadMessagesCommand` (show/hide `OverheadMessageOverlay`)
  - Aggiunto `ApplyWindowSize()`: chiama Win32 `SetWindowPos(hwnd, 0, 0, ForceWidth, ForceHeight, SWP_NOMOVE|SWP_NOZORDER|SWP_SHOWWINDOW)` quando `ForceWidth`/`ForceHeight` cambiano
  - `OnForceWidthChanged`/`OnForceHeightChanged` ora chiamano anche `ApplyWindowSize()`
- **`DisplayPage.xaml`**: sostituito singolo bottone con `WrapPanel` che include tre bottoni: Target HP, Floating Toolbar, Overhead Messages.

**File modificati**: `FloatingToolbarWindow.xaml.cs`, `DisplayViewModel.cs`, `DisplayPage.xaml`

---

## 3. DisplayPage — Collegare ForceWidth/Height a SetWindowPos

**Problema**: `DisplayViewModel` salvava `ForceWidth`/`ForceHeight` in config ma non li applicava alla finestra UO.

**Implementazione**: Vedere punto 2 → `ApplyWindowSize()` in `DisplayViewModel` con P/Invoke `SetWindowPos`.

---

## 4. CountersPage — Auto-recalculate on item change

**Problema**: `CounterService.RecalculateAll()` era solo manuale (pulsante). I counter non si aggiornano automaticamente quando il backpack cambia.

**Implementazione**:

- **`CounterService.cs`**: aggiunto `IMessenger` nel costruttore + `messenger.RegisterAll(this)`.
  - Implementa `IRecipient<ContainerItemAddedMessage>` → `ScheduleRecalculate()`
  - Implementa `IRecipient<ContainerContentMessage>` → `ScheduleRecalculate()`
  - Implementa `IRecipient<LoginCompleteMessage>` → `ScheduleRecalculate()` (conta al login)
  - `ScheduleRecalculate()`: usa `System.Threading.Timer` con debounce 500ms → non ricalcola più di una volta ogni 500ms durante batch di item

```csharp
private void ScheduleRecalculate()
{
    _debounceTimer?.Dispose();
    _debounceTimer = new System.Threading.Timer(
        _ => RecalculateAll(), null, DebounceMs, System.Threading.Timeout.Infinite);
}
```

**File modificati**: `CounterService.cs`

---

## 5. Overhead Messages Renderer

**Problema**: I messaggi speech/emote dai pacchetti 0x1C (ASCII) e 0xAE (Unicode) venivano inviati solo al `JournalService`. Nessun overlay visivo.

**Implementazione**:

### Shared Layer
- **`WorldStateMessages.cs`**: aggiunti `OverheadMessageType` (enum con Regular/Emote/Yell/Whisper/Guild/Alliance/Spell/...) e `OverheadMessageMessage` (ValueChangedMessage con `Serial, Name, Text, Hue, MessageType`).

### Core Layer
- **`WorldPacketHandler.cs`** — `HandleAsciiMessage` e `HandleUnicodeMessage`: aggiunto `byte msgType = reader.ReadByte()` (era ignorato). Per messaggi non-System (`msgType != 0x01`), pubblica `_messenger.Send(new OverheadMessageMessage(...))`.

### UI Layer
- **`OverheadMessageOverlayViewModel.cs`** (nuovo):
  - `OverheadEntry`: `ObservableObject` con `Text`, `SenderName`, `Opacity`, `TextBrush` (mappatura hue/tipo → `SolidColorBrush`), `ExpiresAt`
  - `OverheadMessageOverlayViewModel`: `IRecipient<OverheadMessageMessage>`, `IDisposable`
    - `ObservableCollection<OverheadEntry> Messages` (max 10)
    - `DispatcherTimer(200ms)` per cleanup: rimuove messaggi scaduti, fade-out nell'ultimo 1.5 secondi (`entry.Opacity = remaining / 1.5`)
    - `Receive()`: crea `OverheadEntry` e aggiunge via `RunOnUIThread()`
- **`OverheadMessageOverlay.xaml`** (nuovo): Window trasparente (`WindowStyle=None`, `AllowsTransparency=True`, `IsHitTestVisible=False`, `Topmost=True`). Panel messaggi ancorato in basso-sinistra con `Background="#99000000"`, `ItemsControl` con template `SenderName: Text` in `TextBlock` separati con `TextBrush` binding.
- **`OverheadMessageOverlay.xaml.cs`** (nuovo): `IClientInteropService` iniettato, `DispatcherTimer(500ms)` per `SnapToUOWindow()` (stessa logica di FloatingToolbar: copre esattamente la finestra UO), dispose corretto.

### App.xaml.cs
- Registrato `OverheadMessageOverlay` (Singleton) e `OverheadMessageOverlayViewModel` (Singleton).
- Pre-istanziato `OverheadMessageOverlayViewModel` in `OnStartup` per avviare le subscription.
- Dispose in `OnExit`.

### App.xaml
- Aggiunto `<conv:CountToVisibilityConverter x:Key="CountToVisibilityConverter"/>`.

### CommonConverters.cs
- Aggiunto `CountToVisibilityConverter` (int → Visibility, `> 0` = Visible).

**File creati**: `OverheadMessageOverlayViewModel.cs`, `OverheadMessageOverlay.xaml`, `OverheadMessageOverlay.xaml.cs`
**File modificati**: `WorldStateMessages.cs`, `WorldPacketHandler.cs`, `App.xaml.cs`, `App.xaml`, `CommonConverters.cs`

---

## Fix aggiuntivi durante Sprint 11

### Fix — PlayerApi.SetAbility (UOSteamInterpreter)
**Problema**: `UOSteamInterpreter` chiamava `_player.SetAbility(args[0])` ma il metodo mancava in `PlayerApi`.
**Fix**: Aggiunto `SetAbility(string abilityName)` a `PlayerApi`. Invia pacchetto `0xBF sub 0x14` (Toggle Special Move AOS+) con `ability=1` (primary), `2` (secondary) o `0` (clear).

### Fix — SecureTradePage.xaml StringFormat
**Problema**: `StringFormat='{0}x Item'` e `StringFormat='{0} Offers'` causavano `MC3074: tag '0' non esiste` nel compiler XAML.
**Fix**: Convertiti in `StringFormat={}{0}x Item` e `StringFormat={}{0} Offers` (sintassi XAML corretta per StringFormat con testo letterale).

### Fix — UOSteamInterpreterTests
**Problema**: Il costruttore di `UOSteamInterpreter` aveva aggiunto `JournalApi` ma il test chiamava l'overload senza.
**Fix**: Aggiunto `JournalApi` (mock) nel metodo factory `CreateInterpreter()` del test.

---

## Risultati Build & Test

| Metrica | Valore |
|---------|--------|
| Errori build | 0 |
| Warning build | 0 (non bloccanti) |
| Test totali | 69 |
| Test passati | 69 |
| Test falliti | 0 |

---

## File modificati — Sprint 11

| File | Tipo | Motivo |
|------|------|--------|
| `Shared/Interfaces/ISoundService.cs` | Modifica | Aggiunti SetVolume/GetVolume |
| `Core/Services/SoundService.cs` | Modifica | Implementazione Windows Core Audio |
| `UI/ViewModels/SoundViewModel.cs` | Modifica | Property Volume + OnVolumeChanged |
| `UI/Views/Pages/SoundPage.xaml` | Modifica | Card volume con slider |
| `UI/Views/Windows/FloatingToolbarWindow.xaml.cs` | Modifica | Auto-positioning UO window |
| `UI/ViewModels/DisplayViewModel.cs` | Modifica | ToggleToolbar/Overlay cmd + ApplyWindowSize |
| `UI/Views/Pages/DisplayPage.xaml` | Modifica | WrapPanel con 3 bottoni overlay |
| `Core/Services/CounterService.cs` | Modifica | Auto-recalculate via messenger + debounce |
| `Shared/Messages/WorldStateMessages.cs` | Modifica | OverheadMessageType + OverheadMessageMessage |
| `Core/Handlers/WorldPacketHandler.cs` | Modifica | Publish OverheadMessageMessage da 0x1C/0xAE |
| `UI/ViewModels/OverheadMessageOverlayViewModel.cs` | Nuovo | ViewModel overlay messaggi |
| `UI/Views/Windows/OverheadMessageOverlay.xaml` | Nuovo | Window trasparente overlay |
| `UI/Views/Windows/OverheadMessageOverlay.xaml.cs` | Nuovo | Code-behind con snap UO window |
| `UI/App.xaml.cs` | Modifica | Registrazione overlay + ViewModel |
| `UI/App.xaml` | Modifica | CountToVisibilityConverter |
| `UI/Views/Converters/CommonConverters.cs` | Modifica | CountToVisibilityConverter |
| `Core/Services/Scripting/Api/PlayerApi.cs` | Modifica | Aggiunto SetAbility() |
| `UI/Views/Pages/SecureTradePage.xaml` | Fix | StringFormat sintassi XAML corretta |
| `Tests/MockTests/Scripting/UOSteamInterpreterTests.cs` | Fix | JournalApi aggiunto alla factory |
