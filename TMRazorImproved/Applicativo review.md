# TMRazor Improved — Documento di Review e Roadmap Completa

> **Autore review:** Analisi architetturale e gap analysis completa
> **Data:** 15 Marzo 2026
> **Branch attivo:** `claude/tmrazor-migration-review-IZ6Ft`
> **Scope:** Confronto TMRazor legacy (.NET 4.7.2 WinForms) vs TMRazorImproved (.NET 10 WPF)

---

## 1. Sommario Esecutivo

### Stato della Migrazione per Area

| Area | Completamento | Stato |
|------|:---:|:---:|
| Infrastruttura DI / MVVM | 95% | ✅ Buono |
| Packet Handling | 85% | ✅ Buono |
| Thread Safety | 88% | ⚠️ Quasi completo |
| Sistema Macro | 82% | ⚠️ Quasi completo |
| Agenti (AutoLoot, Dress, ecc.) | 80% | ⚠️ Mancano alcune funzioni |
| Vendor Buy/Sell | 70% | ⚠️ Backend parziale |
| Scripting API (Python/C#/UOS) | 78% | ⚠️ Gap importanti |
| UI / WPF / MVVM | 75% | ⚠️ Finestre overlay incomplete |
| SpecialMoves API | 10% | ❌ Quasi assente |
| Screen Capture | 30% | ❌ System.Drawing da migrare |
| Profili per-shard | 0% | ❌ Non iniziato |
| Macro Recorder | 60% | ⚠️ Backend parziale, UI da finire |

### Semaforo Generale

| Dimensione | Stato |
|---|:---:|
| Compilazione | ✅ Compila |
| Crash runtime noti | ⚠️ 3 race condition residue |
| Feature parity vs originale | ⚠️ ~78% |
| Compatibilità script Python/UOS | ⚠️ ~80% (gap su SpecialMoves) |
| Pronto per produzione | ❌ No (task critici aperti) |

### Cosa è già stato fixato (non da rifare)

I seguenti bug identificati nelle review precedenti (7 e 11 Marzo 2026) sono **già stati corretti** nel codice attuale:

- ✅ CS0111 duplicate method in `TargetApi.cs` — rimosso
- ✅ `WorldService.IsCasting` — ora `volatile`
- ✅ `TargetingService` `_lastTarget`, `_hasPrompt` — ora `volatile`; `_targetQueue` protetto da `_queueLock`
- ✅ `SecureTradeService._trades` — protetto da `_tradesLock`
- ✅ `DPSMeterService._damageHistory` / `_targetDamage` — protetti con `lock()`
- ✅ `MacrosService._isPlaying` / `_isRecording` — ora `volatile`
- ✅ `MacrosService.MacroList.Add()` — `SynchronizationContext` catturato al costruzione
- ✅ `SkillsService` handler doppio 0x3A — rimosso, ora usa messaggi tipizzati
- ✅ `BandageHealService` cursorId=0 — usa `PendingCursorId` del server
- ✅ `Target.GetLastAttack()` — legge da `Player.AttackTarget`
- ✅ `Target.LastUsedObject()` — legge da `Player.LastObject`
- ✅ `TargetResource` — 4 overload presenti
- ✅ `Spells.Cast` — overload con `targetSerial`, `wait`, `waitAfter` presenti
- ✅ `ScriptSuspend` / `ScriptResume` — implementati via `ScriptingService`
- ✅ `Items.Filter` — `IsContainer`, `IsCorpse`, `RangeMin/Max`, `ExcludeSerial` presenti
- ✅ `Mobiles.Filter` — `IsHuman`, `IsGhost`, `IsAlly`, `IsEnemy`, `IsNeutral` presenti
- ✅ `AgentApis.cs` — wrapper Python per AutoLoot, Dress, Scavenger, Restock, Organizer, BandageHeal presenti
- ✅ `CSharpScriptEngine` (Roslyn) — implementato

---

## 2. Architettura Attuale

### Stack Tecnologico

```
TMRazorImproved.Shared (.NET 10)
  └── Contratti (Interfaces), DTOs, Models, Enumerations, Messages

TMRazorImproved.Core (.NET 10-windows)
  ├── Services/ (30+ singleton)
  ├── Services/Scripting/ (IronPython 3.4 + Roslyn + UOSteam)
  ├── Services/Scripting/Api/ (15 file API)
  ├── PacketHandlers/ (70+ handler)
  └── Utilities/ (PacketBuilder, UiThrottler, ecc.)

TMRazorImproved.UI (.NET 10-windows)
  ├── Views/Pages/ (23 pagine XAML)
  ├── Views/Windows/ (7 finestre overlay)
  ├── ViewModels/ (MVVM CommunityToolkit)
  └── Resources/ (localizzazione IT)

TMRazorImproved.Tests (xUnit, Moq)
  ├── Unit Tests
  ├── Stress Tests
  └── Fuzz Tests
```

### Pattern Architetturali Adottati

- **Dependency Injection** — Microsoft.Extensions.DependencyInjection, configurato in `App.xaml.cs`
- **MVVM** — CommunityToolkit.Mvvm con `[RelayCommand]` e `[ObservableProperty]`
- **Messenger** — `IMessenger` per disaccoppiamento UI/Core
- **Async/Await (TAP)** — nessun `Thread.Abort()`; si usa `CancellationTokenSource`
- **Thread Safety** — `ConcurrentDictionary`, `volatile`, `lock()`, `SynchronizationContext`

---

## 3. Criticità Residue per Priorità

### P0 — Critiche (bloccano funzionalità core)

| ID | Servizio | Problema | File |
|----|----------|----------|------|
| P0-01 | `ScreenCaptureService` | Usa `System.Drawing` / `System.Drawing.Imaging` — non supportato nativamente in .NET 10 | `Services/ScreenCaptureService.cs` righe 2-3 |
| P0-01b | `MiscApi` | Usa `System.Drawing.Point` nelle firme P/Invoke per `GetCursorPos`/`ClientToScreen` (righe 118-119, 134, 138, 157, 161) | `Scripting/Api/MiscApi.cs` |
| P0-02 | `PlayerApi` | `SpecialMoves` non migrati — `HasSpecial`, `PrimarySpecialId`, `SecondarySpecialId` ritornano sempre `false`/`0` | `Scripting/Api/PlayerApi.cs` righe 142-146 |
| P0-03 | Config | `Config/regions.json` mancante — `Player.Area()` e `Player.Zone()` ritornano sempre `"Unknown"` | `Scripting/Api/PlayerApi.cs` riga 239 |

### P1 — Importanti (comportamento errato o degradato)

| ID | Servizio | Problema | File |
|----|----------|----------|------|
| P1-01 | `MacrosService` | `_recordingBuffer.Clear()` a riga 605 non è dentro `lock(_recordingBuffer)` — race condition al reset | `Services/MacrosService.cs` riga 605 |
| P1-02 | `WorldService` | `PartyMembers` è un `HashSet<uint>` senza lock — scritto da packet thread, letto da UI thread | `Services/WorldService.cs` riga 69 |
| P1-03 | `DPSMeterService` | Proprietà `TargetDamage` (riga 33) crea `ReadOnlyDictionary` wrappando `_targetDamage` senza lock — enumerazione non thread-safe | `Services/DPSMeterService.cs` riga 33 |
| P1-04 | `Misc.ScriptStop` | Può fermare solo lo script attualmente in esecuzione per nome. Non può fermare script diversi da quello corrente | `Scripting/Api/MiscApi.cs` riga 753 |
| P1-05 | `FriendsService` | Rilevamento dei membri di party incompleto — TODO nel codice | `Services/FriendsService.cs` riga 39 |
| P1-06 | Config | `Config/doors.json` mancante — le porte potrebbero non essere riconosciute dal pathfinding | PlayerApi / PathFinding |
| P1-07 | `PlayerApi` | `PathFindTo(x,y,z)` è uno stub vuoto — non invia nessun pacchetto al client | `Scripting/Api/PlayerApi.cs` riga 494 |
| P1-08 | `UOSteamInterpreter` | Implementazione parziale — i comandi IF/FOR/WHILE sono presenti ma non completamente testati; alcuni comandi UOS potrebbero non funzionare | `Scripting/Engines/UOSteamInterpreter.cs` |
| P1-09 | `VideoCaptureService` | Stub completo — `StartAsync()` ritorna sempre `false` con log "Not yet implemented with SharpAvi" | `Services/VideoCaptureService.cs` riga 17 |

### P2 — Qualità del codice (da fare nella prossima release)

| ID | Servizio | Problema | File |
|----|----------|----------|------|
| P2-01 | Vari service | Pattern `GetActiveConfig()` ripetuto 7 volte con codice identico — violazione DRY | `VendorService.cs`, `OrganizerService.cs`, ecc. |
| P2-02 | `Misc.Distance` | Usa distanza di Chebyshev (tile UO). L'originale usava Euclidea — possibile rottura di script | `MiscApi.cs` riga 439 |
| P2-03 | `PathFindingService` | Range massimo hardcodato a 100 tile — non configurabile | `Services/PathFindingService.cs` |
| P2-04 | `SearchService` | Granularità lock su `_dynamicProviders` potenzialmente troppo grossa — rischio deadlock | `Services/SearchService.cs` |
| P2-05 | `SkillsService.Skills` | Lista non thread-safe per accessi concorrenti UI/packet | `Services/SkillsService.cs` |
| P2-06 | `Player.SetLast` | Usa `OverheadMsg` invece del pacchetto 0x73 (highlight ufficiale UO) | `TargetApi.cs` riga 357-368 |

---

## 4. Task List Completa per la Migrazione al 100%

> **Legenda priorità:** 🔴 P0 (critico) · 🟠 P1 (importante) · 🟡 P2 (qualità) · 🔵 Feature

---

## BLOCCO A — Bug Critici (P0)

---

### TASK-A01 🔴 — Migrare ScreenCaptureService da System.Drawing a WPF

**Problema:**
`ScreenCaptureService.cs` usa `System.Drawing.Bitmap` e `System.Drawing.Imaging.ImageFormat`, che sono deprecati in .NET 10. Il package `System.Drawing.Common` è necessario come NuGet aggiuntivo e causa avvisi di deprecazione. La feature screenshot non funzionerà su macchine senza il package installato.

**File coinvolti:**
- `TMRazorImproved.Core/Services/ScreenCaptureService.cs` (righe 2-3, 52-95)
- `TMRazorImproved.Core/TMRazorImproved.Core.csproj` (aggiungere o rimuovere NuGet)

**Passi dettagliati:**

1. **Apri** `ScreenCaptureService.cs`

2. **Rimuovi** le seguenti righe all'inizio del file:
   ```csharp
   using System.Drawing;
   using System.Drawing.Imaging;
   ```

3. **Aggiungi** questi using al loro posto:
   ```csharp
   using System.Windows.Interop;
   using System.Windows.Media.Imaging;
   ```

4. **Sostituisci** il metodo `CaptureAsync()` e il metodo privato `CaptureWindow()` con questa implementazione WPF-nativa:
   ```csharp
   public Task<string> CaptureAsync()
   {
       return Task.Run(() =>
       {
           IntPtr hWnd = _clientInterop.GetWindowHandle();
           if (hWnd == IntPtr.Zero)
           {
               _logger.LogWarning("Cannot capture: UO Window handle is Zero.");
               return string.Empty;
           }

           try
           {
               string playerName = _worldService.Player?.Name ?? "Unknown";
               string fileName = $"{playerName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.jpg";
               string fullPath = Path.Combine(_capturePath, fileName);

               // Usa PrintWindow via P/Invoke per catturare la finestra UO
               // poi converte in BitmapSource WPF tramite interop
               RECT rect = default;
               GetWindowRect(hWnd, ref rect);
               int width  = Math.Max(rect.right - rect.left, 800);
               int height = Math.Max(rect.bottom - rect.top, 600);

               // Crea un DIB compatibile via GDI (nessun System.Drawing)
               IntPtr hdc    = GetDC(IntPtr.Zero);
               IntPtr hdcMem = CreateCompatibleDC(hdc);
               IntPtr hBmp   = CreateCompatibleBitmap(hdc, width, height);
               IntPtr hOld   = SelectObject(hdcMem, hBmp);

               PrintWindow(hWnd, hdcMem, 0x00000002);

               SelectObject(hdcMem, hOld);
               DeleteDC(hdcMem);
               ReleaseDC(IntPtr.Zero, hdc);

               // Converti HBITMAP in BitmapSource WPF
               BitmapSource bmpSrc = Imaging.CreateBitmapSourceFromHBitmap(
                   hBmp, IntPtr.Zero, System.Windows.Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());
               DeleteObject(hBmp);

               // Salva come JPEG usando WPF encoder
               var encoder = new JpegBitmapEncoder { QualityLevel = 90 };
               encoder.Frames.Add(BitmapFrame.Create(bmpSrc));
               using var stream = new FileStream(fullPath, FileMode.Create);
               encoder.Save(stream);

               _logger.LogInformation("Screenshot saved to {Path}", fullPath);
               return fullPath;
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Failed to capture screenshot.");
               return string.Empty;
           }
       });
   }
   ```

5. **Aggiorna** la sezione `#region P/Invoke` aggiungendo le funzioni GDI mancanti:
   ```csharp
   [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
   [DllImport("gdi32.dll")] private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
   [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);
   [DllImport("gdi32.dll")] private static extern bool DeleteDC(IntPtr hdc);
   [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr hObject);
   [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);
   [DllImport("user32.dll")] private static extern int ReleaseDC(IntPtr hWnd, IntPtr hdc);
   ```

6. **Rimuovi System.Drawing anche da MiscApi.cs** — righe 118-119, 134, 138, 157, 161 usano `System.Drawing.Point` nelle firme P/Invoke. Aggiungi una struct locale in fondo alla classe e sostituisci tutte le occorrenze:
   ```csharp
   // Prima (SBAGLIATO):
   [DllImport("user32.dll")] private static extern bool GetCursorPos(ref System.Drawing.Point lp);
   [DllImport("user32.dll")] private static extern bool ClientToScreen(IntPtr hWnd, ref System.Drawing.Point lp);
   ...
   var old = new System.Drawing.Point();

   // Dopo (CORRETTO — struct locale, nessuna dipendenza da System.Drawing):
   [StructLayout(LayoutKind.Sequential)]
   private struct POINT { public int X; public int Y; }

   [DllImport("user32.dll")] private static extern bool GetCursorPos(ref POINT lp);
   [DllImport("user32.dll")] private static extern bool ClientToScreen(IntPtr hWnd, ref POINT lp);
   ...
   var old = new POINT();
   ```

7. **Verifica:** Compila il progetto (`dotnet build`). Non devono esserci warning su `System.Drawing`.

**Criterio di accettazione:**
Premere il tasto Screenshot in-game produce un file `.jpg` nella cartella `Screenshots/` senza errori a runtime. `Misc.LeftMouseClick()` e `Misc.RightMouseClick()` funzionano correttamente.

---

### TASK-A02 🔴 — Implementare SpecialMoves API completa

**Problema:**
In `PlayerApi.cs` le proprietà `HasSpecial`, `HasPrimarySpecial`, `HasSecondarySpecial`, `PrimarySpecialId`, `SecondarySpecialId` ritornano valori hardcoded. Gli script Python che usano le ability di combattimento speciali (Armor Ignore, Bleed, ecc.) non funzionano.

**File coinvolti:**
- `TMRazorImproved.Core/Services/Scripting/Api/PlayerApi.cs` (righe 142-146)
- `TMRazorImproved.Shared/Models/Mobile.cs` (aggiungere campi)
- `TMRazorImproved.Core/PacketHandlers/WorldPacketHandler.cs` (aggiungere handler 0xBF sub-0x19)
- Da creare: `TMRazorImproved.Core/Services/Scripting/Api/SpecialMovesApi.cs`

**Sottotask:**

#### TASK-A02.1 — Aggiungere tracking abilità speciali al modello Player

1. Apri `TMRazorImproved.Shared/Models/Mobile.cs`
2. Cerca la classe `Mobile` e aggiungi questi campi:
   ```csharp
   /// <summary>ID abilità speciale primaria corrente (0 = nessuna)</summary>
   public int PrimaryAbilityId { get; set; }
   /// <summary>ID abilità speciale secondaria corrente (0 = nessuna)</summary>
   public int SecondaryAbilityId { get; set; }
   /// <summary>True se la primaria è attiva</summary>
   public bool PrimaryAbilityActive { get; set; }
   /// <summary>True se la secondaria è attiva</summary>
   public bool SecondaryAbilityActive { get; set; }
   ```

#### TASK-A02.2 — Aggiungere handler per il pacchetto 0xBF (sub 0x19)

Il server invia `0xBF` con sub-command `0x19` per notificare le abilità speciali disponibili.

1. Apri `TMRazorImproved.Core/PacketHandlers/WorldPacketHandler.cs`
2. Nel costruttore, registra il nuovo handler:
   ```csharp
   _packetService.RegisterViewer(PacketPath.ServerToClient, 0xBF, HandleGeneral0xBF);
   ```
3. Aggiungi il metodo (nella stessa classe):
   ```csharp
   private void HandleGeneral0xBF(byte[] data)
   {
       if (data.Length < 5) return;
       ushort subCmd = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(3));

       if (subCmd == 0x0019 && data.Length >= 9) // Special Ability Info
       {
           var player = _worldService.Player;
           if (player == null) return;
           player.PrimaryAbilityId   = data[5];
           player.SecondaryAbilityId = data[6];
           player.PrimaryAbilityActive   = (data[7] & 0x01) != 0;
           player.SecondaryAbilityActive = (data[7] & 0x02) != 0;
       }
   }
   ```

#### TASK-A02.3 — Creare SpecialMovesApi.cs

Crea il file `TMRazorImproved.Core/Services/Scripting/Api/SpecialMovesApi.cs`:

```csharp
namespace TMRazorImproved.Core.Services.Scripting.Api
{
    /// <summary>
    /// API esposta agli script Python come variabile <c>SpecialMoves</c>.
    /// Gestisce le abilità speciali di combattimento (Weapon Abilities).
    /// </summary>
    public class SpecialMovesApi
    {
        private readonly IWorldService _world;
        private readonly IPacketService _packet;
        private readonly ScriptCancellationController _cancel;

        public SpecialMovesApi(IWorldService world, IPacketService packet, ScriptCancellationController cancel)
        {
            _world  = world;
            _packet = packet;
            _cancel = cancel;
        }

        /// <summary>Attiva o disattiva l'abilità primaria dell'arma corrente.</summary>
        public virtual void SetPrimaryAbility()
        {
            _cancel.ThrowIfCancelled();
            SendToggleAbility(0x01);
        }

        /// <summary>Attiva o disattiva l'abilità secondaria dell'arma corrente.</summary>
        public virtual void SetSecondaryAbility()
        {
            _cancel.ThrowIfCancelled();
            SendToggleAbility(0x02);
        }

        /// <summary>Disattiva entrambe le abilità speciali.</summary>
        public virtual void ClearCurrentAbility()
        {
            _cancel.ThrowIfCancelled();
            SendToggleAbility(0x00);
        }

        public virtual bool HasPrimary   => _world.Player?.PrimaryAbilityActive   ?? false;
        public virtual bool HasSecondary => _world.Player?.SecondaryAbilityActive ?? false;
        public virtual int  PrimaryId    => _world.Player?.PrimaryAbilityId       ?? 0;
        public virtual int  SecondaryId  => _world.Player?.SecondaryAbilityId     ?? 0;

        private void SendToggleAbility(byte abilityIndex)
        {
            // Pacchetto 0xD7 (SA+) sub 0x0019 oppure 0xBF sub 0x0019 (pre-SA)
            byte[] pkt = new byte[9];
            pkt[0] = 0xD7;
            pkt[1] = 0x00; pkt[2] = 0x09;
            BinaryPrimitives.WriteUInt32BigEndian(pkt.AsSpan(3), _world.Player?.Serial ?? 0);
            pkt[7] = 0x00; pkt[8] = abilityIndex;
            _packet.SendToServer(pkt);
        }
    }
}
```

#### TASK-A02.4 — Aggiornare PlayerApi con le stub corrette

In `PlayerApi.cs` righe 142-146, sostituisci le stub hardcoded:
```csharp
// Prima (SBAGLIATO):
public virtual bool HasSpecial => false;
public virtual bool HasPrimarySpecial => false;
...

// Dopo (CORRETTO):
public virtual bool HasSpecial          => (P?.PrimaryAbilityActive ?? false) || (P?.SecondaryAbilityActive ?? false);
public virtual bool HasPrimarySpecial   => P?.PrimaryAbilityActive   ?? false;
public virtual bool HasSecondarySpecial => P?.SecondaryAbilityActive ?? false;
public virtual int  PrimarySpecialId    => P?.PrimaryAbilityId       ?? 0;
public virtual int  SecondarySpecialId  => P?.SecondaryAbilityId     ?? 0;
```

#### TASK-A02.5 — Registrare SpecialMovesApi negli ScriptGlobals

1. Apri `TMRazorImproved.Core/Services/Scripting/ScriptGlobals.cs`
2. Aggiungi la proprietà:
   ```csharp
   public SpecialMovesApi SpecialMoves { get; init; } = null!;
   ```
3. Apri `TMRazorImproved.Core/Services/Scripting/ScriptingService.cs` dove si costruisce `ScriptGlobals`
4. Aggiungi al costruttore dell'oggetto globals:
   ```csharp
   SpecialMoves = new SpecialMovesApi(_world, _packet, cancel),
   ```

**Criterio di accettazione:**
Uno script Python con `SpecialMoves.SetPrimaryAbility()` non genera `AttributeError`. `SpecialMoves.HasPrimary` ritorna `True` dopo che il server ha inviato il pacchetto 0xBF.

---

### TASK-A03 🔴 — Creare Config/regions.json

**Problema:**
`PlayerApi.Area()` e `PlayerApi.Zone()` leggono da `Config/regions.json`. Il file non esiste. Tutti gli script che usano `Player.Area()` ricevono `"Unknown"`.

**File coinvolti:**
- Da creare: `TMRazorImproved.UI/Config/regions.json` (o `TMRazorImproved.Core/` a seconda del percorso di output)

**Passi dettagliati:**

1. Cerca nel build output il percorso atteso dal codice. In `PlayerApi.cs` riga 239:
   ```csharp
   var path = System.IO.Path.Combine(AppContext.BaseDirectory, "Config", "regions.json");
   ```
   Quindi il file deve trovarsi in `<output_dir>/Config/regions.json`.

2. Crea la cartella `Config/` nella directory di progetto UI se non esiste.

3. Crea il file `regions.json` con questa struttura base (adatta alle zone del server The Miracle):
   ```json
   {
     "Felucca": {
       "Towns": {
         "Britain": [
           { "X": "1400", "Y": "1600", "Width": "400", "Height": "400" }
         ],
         "Trinsic": [
           { "X": "1800", "Y": "2700", "Width": "300", "Height": "300" }
         ]
       },
       "Dungeons": {
         "Destard": [
           { "X": "5100", "Y": "3200", "Width": "200", "Height": "200" }
         ]
       },
       "Guarded": {},
       "Forest": {}
     },
     "Trammel": {
       "Towns": {
         "Britain": [
           { "X": "1400", "Y": "1600", "Width": "400", "Height": "400" }
         ]
       },
       "Dungeons": {},
       "Guarded": {},
       "Forest": {}
     },
     "Ilshenar": { "Towns": {}, "Dungeons": {}, "Guarded": {}, "Forest": {} },
     "Malas":    { "Towns": {}, "Dungeons": {}, "Guarded": {}, "Forest": {} },
     "Tokuno":   { "Towns": {}, "Dungeons": {}, "Guarded": {}, "Forest": {} },
     "TerMur":   { "Towns": {}, "Dungeons": {}, "Guarded": {}, "Forest": {} }
   }
   ```

4. Configura il file per essere copiato nell'output. Nel progetto (`.csproj`) che contiene il file:
   ```xml
   <ItemGroup>
     <Content Include="Config\regions.json">
       <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
     </Content>
   </ItemGroup>
   ```

5. Popola le zone del shard "The Miracle" con le coordinate reali. Consulta il documento di configurazione del shard o i file di configurazione del TMRazor legacy in `Razor/Config/`.

**Criterio di accettazione:**
Uno script con `Misc.SendMessage(Player.Area())` mostra il nome della città/zona corretta invece di `"Unknown"`.

---

## BLOCCO B — Bug Thread Safety (P1)

---

### TASK-B01 🟠 — Fix MacrosService._recordingBuffer.Clear() senza lock

**Problema:**
In `MacrosService.cs` riga 605, `_recordingBuffer.Clear()` viene chiamato senza `lock(_recordingBuffer)`. Tutte le altre operazioni su `_recordingBuffer` usano il lock correttamente (righe 614, 622, 633, 643, ecc.), ma il Clear no. Se il packet thread sta aggiungendo un elemento esattamente mentre avviene il Clear, il comportamento è undefined.

**File:** `TMRazorImproved.Core/Services/MacrosService.cs`

**Passi:**

1. Apri `MacrosService.cs`
2. Cerca la riga 605 (circa):
   ```csharp
   _recordingBuffer.Clear();  // MANCA IL LOCK
   ```
3. Sostituiscila con:
   ```csharp
   lock (_recordingBuffer) { _recordingBuffer.Clear(); }
   ```

**Criterio di accettazione:**
Avviare la registrazione macro subito dopo averla fermata (stress test rapido start/stop) non causa `InvalidOperationException`.

---

### TASK-B02 🟠 — Fix WorldService.PartyMembers senza lock

**Problema:**
`WorldService.PartyMembers` è un `HashSet<uint>` pubblico (riga 69). Il packet thread (handler `0x6D` o simili) vi aggiunge/rimuove elementi, mentre il thread UI o script Python vi iterano. Accesso concorrente non sincronizzato su `HashSet` causa `InvalidOperationException: Collection was modified`.

**File:** `TMRazorImproved.Core/Services/WorldService.cs`

**Passi:**

1. Apri `WorldService.cs`
2. Cambia la dichiarazione di `PartyMembers` da:
   ```csharp
   public HashSet<uint> PartyMembers { get; } = new();
   ```
   a:
   ```csharp
   // Thread-safe: usa ConcurrentDictionary come set (value = byte.MinValue ignorato)
   private readonly System.Collections.Concurrent.ConcurrentDictionary<uint, byte> _partyMembers = new();
   public IReadOnlyCollection<uint> PartyMembers => _partyMembers.Keys.ToList().AsReadOnly();
   public void AddPartyMember(uint serial)    => _partyMembers.TryAdd(serial, 0);
   public void RemovePartyMember(uint serial) => _partyMembers.TryRemove(serial, out _);
   public bool IsPartyMember(uint serial)     => _partyMembers.ContainsKey(serial);
   ```

3. **Trova tutti i posti** del codice che usano `PartyMembers.Add(...)` o `PartyMembers.Remove(...)` e sostituiscili con `AddPartyMember(...)` / `RemovePartyMember(...)`.
   ```bash
   # Cerca con grep:
   grep -rn "PartyMembers" TMRazorImproved.Core/ TMRazorImproved.UI/
   ```

4. Aggiorna l'interfaccia `IWorldService` se include `PartyMembers`.

**Nota per il junior:** `ConcurrentDictionary` non ha un `ConcurrentHashSet` nativo in .NET. Il trucco di usare il dizionario con un valore inutile (`byte`) è il pattern standard per creare un set concorrente.

**Criterio di accettazione:**
`PlayerApi.InParty` (`return _world.PartyMembers.Count > 0`) funziona senza eccezioni anche con molti aggiornamenti di party in rapida successione.

---

### TASK-B03 🟠 — Fix DPSMeterService.TargetDamage senza lock

**Problema:**
In `DPSMeterService.cs` riga 33:
```csharp
public IReadOnlyDictionary<uint, long> TargetDamage => new ReadOnlyDictionary<uint, long>(_targetDamage);
```
Il `ReadOnlyDictionary` viene creato wrappando direttamente `_targetDamage`. Se il packet thread modifica `_targetDamage` mentre il consumer (UI thread) itera il `ReadOnlyDictionary`, si ottiene `InvalidOperationException`.

**File:** `TMRazorImproved.Core/Services/DPSMeterService.cs`

**Passi:**

1. Apri `DPSMeterService.cs`
2. Sostituisci riga 33:
   ```csharp
   // Prima (SBAGLIATO — no lock):
   public IReadOnlyDictionary<uint, long> TargetDamage => new ReadOnlyDictionary<uint, long>(_targetDamage);

   // Dopo (CORRETTO — snapshot thread-safe):
   public IReadOnlyDictionary<uint, long> TargetDamage
   {
       get { lock (_targetDamage) return new Dictionary<uint, long>(_targetDamage); }
   }
   ```

**Nota:** Ritorniamo un `Dictionary` separato (snapshot) invece di un wrapper. Così il chiamante ha una copia isolata che non può essere modificata dal packet thread.

**Criterio di accettazione:**
Il DPS Meter mostra i danni per target senza crash anche durante combattimenti intensi con molti target diversi.

---

## BLOCCO C — Fix Runtime / Behavioral (P1)

---

### TASK-C01 🟠 — Fix Misc.ScriptStop per script non-corrente

**Problema:**
`MiscApi.ScriptStop(scriptfile)` funziona solo se `scriptfile` è il nome dello script attualmente in esecuzione. Non può fermare altri script. In UO con scripting multi-script questo è un problema: uno script che gestisce il loop principale non riesce a fermare uno script satellite.

**File:** `TMRazorImproved.Core/Services/Scripting/Api/MiscApi.cs` (riga 753)
**File correlato:** `TMRazorImproved.Core/Services/Scripting/ScriptingService.cs`

**Passi:**

1. Apri `ScriptingService.cs` e controlla se esiste un registry degli script in esecuzione. Se non esiste:
   - Aggiungi un `ConcurrentDictionary<string, CancellationTokenSource>` per tracciare tutti gli script attivi per nome.
   - Quando uno script parte, registralo; quando finisce, rimuovilo.

2. Apri `MiscApi.cs` riga 753 e modifica `ScriptStop`:
   ```csharp
   public virtual void ScriptStop(string scriptfile)
   {
       _cancel.ThrowIfCancelled();
       if (_scripting == null) return;
       // Ferma qualsiasi script con quel nome, non solo il corrente
       _scripting.StopScript(scriptfile);
   }
   ```

3. Aggiungi `StopScript(string name)` all'interfaccia `IScriptingService` e implementalo in `ScriptingService`.

**Criterio di accettazione:**
`Misc.ScriptStop("mio_script.py")` ferma `mio_script.py` anche se lo script chiamante è un altro.

---

### TASK-C02 🟠 — Fix FriendsService Party Member Detection

**Problema:**
In `FriendsService.cs` riga 39 c'è un commento `TODO: Need IPartyService or check world for party members`. Il metodo `IsFriend()` probabilmente non controlla correttamente i membri del party come "amici".

**File:** `TMRazorImproved.Core/Services/FriendsService.cs`

**Passi:**

1. Apri `FriendsService.cs`
2. Trova il metodo `IsFriend(uint serial)` o il punto con il TODO
3. Aggiungi il controllo al party via `IWorldService.IsPartyMember(serial)` (dopo TASK-B02):
   ```csharp
   public bool IsFriend(uint serial)
   {
       var config = GetActiveConfig();
       if (config == null) return false;

       // Controlla la lista friends esplicita
       if (config.FriendList.Any(f => f.Serial == serial)) return true;

       // Controlla se è un membro del party (se "TreatPartyAsFriends" è abilitato)
       if (config.TreatPartyAsFriends && _worldService.IsPartyMember(serial)) return true;

       return false;
   }
   ```

**Criterio di accettazione:**
Con `TreatPartyAsFriends = true` nel config, i membri del party non vengono attaccati dall'auto-targeting.

---

### TASK-C03 🟡 — Fix Misc.Distance (documentare comportamento Chebyshev)

**Problema:**
`Misc.Distance(x1,y1,x2,y2)` usa la distanza di Chebyshev (tile UO), mentre alcuni script legacy si aspettano distanza Euclidea. Attualmente `DistanceSqrt()` fornisce la Euclidea. È un potenziale breaking change silenzioso.

**File:** `TMRazorImproved.Core/Services/Scripting/Api/MiscApi.cs` (riga 438)

**Passi:**

1. Aggiungi un commento XML esplicativo al metodo `Distance`:
   ```csharp
   /// <summary>
   /// Distanza di Chebyshev (UO tile-based) tra due punti.
   /// NOTA: In UO, la distanza "ufficiale" è Chebyshev perché il movimento in diagonale
   /// conta 1 tile esattamente come il movimento cardinale.
   /// Se hai bisogno della distanza Euclidea (in linea d'aria), usa DistanceSqrt().
   /// Comportamento identico all'originale TMRazor.
   /// </summary>
   public virtual int Distance(int x1, int y1, int x2, int y2)
   ```

2. Controlla nel codebase degli script originali (cartella `Razor/Scripts/` se esiste) se qualcuno usa `Misc.Distance` aspettandosi Euclidea. Se trovato, adatta quelli specifici a usare `Misc.DistanceSqrt`.

**Criterio di accettazione:**
Nessun script si rompe dopo la modifica. Il comportamento è documentato.

---

## BLOCCO D — File di Configurazione Mancanti

---

### TASK-D01 🟠 — Creare Config/doors.json

**Problema:**
Il pathfinding e alcuni sistemi potrebbero leggere `doors.json` per sapere quali tile sono porte apribili. Senza questo file, il pathfinding potrebbe bloccarsi davanti a porte.

**File:** Da creare: `Config/doors.json`

**Passi:**

1. Cerca nel codebase se `doors.json` viene letto da qualche servizio:
   ```bash
   grep -rn "doors.json" TMRazorImproved.Core/ TMRazorImproved.UI/
   ```

2. Se trovato, segui la stessa procedura di TASK-A03 per creare il file con la struttura richiesta.

3. Il file deve elencare i graphic ID delle porte UO. Esempio:
   ```json
   {
     "DoorGraphics": [
       1653, 1655, 1657, 1659, 1661, 1663, 1665, 1667,
       1669, 1671, 1673, 1675, 1677, 1679, 1681, 1683
     ]
   }
   ```
   *(I valori esatti sono negli asset di Ultima Online — vedi `Razor/Config/` nell'originale)*

4. Configura la copia in output nel `.csproj` (uguale a TASK-A03 step 4).

**Criterio di accettazione:**
Il pathfinding attraversa le porte senza bloccarsi.

---

### TASK-D02 🟡 — Audit completo dei file di configurazione necessari

**Problema:**
Potrebbero esserci altri file JSON di configurazione nell'originale TMRazor che non sono stati portati nel nuovo progetto.

**Passi:**

1. Elenca tutti i file `.json` presenti nel legacy:
   ```bash
   find /home/user/TMRazor/Razor/ -name "*.json" 2>/dev/null
   find /home/user/TMRazor/Config/ -name "*" 2>/dev/null
   ```

2. Per ciascun file trovato, verifica se esiste un equivalente in `TMRazorImproved/`.

3. Per ogni file mancante, crea un task di migrazione separato (o estendi TASK-D01).

---

## BLOCCO E — Features UI Incomplete

---

### TASK-E01 🔵 — Completare SpellGrid Overlay

**Problema:**
`SpellGridWindow.xaml` esiste ma usa un'icona generica (`Symbol.Flash24`) per tutti gli incantesimi invece delle icone UO reali. Mancano: salvataggio posizione finestra, configurazione righe/colonne, assegnazione incantesimi per slot.

**File coinvolti:**
- `TMRazorImproved.UI/Views/Windows/SpellGridWindow.xaml`
- `TMRazorImproved.UI/ViewModels/SpellGridViewModel.cs` (verificare esistenza)
- `TMRazorImproved.Core/Services/Scripting/Api/SpellsApi.cs`

**Sottotask:**

#### TASK-E01.1 — Icone spell reali

1. Verifica se nella cartella `Content/` o `Resources/` esistono immagini per gli spell (es. `spell_001.png`, ecc.)
2. Nel `DataTemplate` dello `SpellGridWindow.xaml`, sostituisci:
   ```xml
   <ui:SymbolIcon Symbol="Flash24" />
   ```
   con:
   ```xml
   <Image Source="{Binding IconPath}" Stretch="Uniform" />
   ```
3. Aggiungi la proprietà `IconPath` al ViewModel degli spell.

#### TASK-E01.2 — Salvataggio posizione finestra

1. Nel `SpellGridViewModel` (o code-behind), cattura l'evento `LocationChanged` della finestra
2. Salva `Left` e `Top` nel `ConfigService` (es. in `CurrentProfile.SpellGridX`, `SpellGridY`)
3. Al riapertura della finestra, ripristina la posizione

#### TASK-E01.3 — Configurazione slot

1. Aggiungi una finestra di configurazione (o dialog) per assegnare incantesimi agli slot
2. Salva la configurazione tramite `ConfigService`

**Criterio di accettazione:**
La finestra SpellGrid mostra le icone corrette, ricorda la posizione e permette di configurare gli slot.

---

### TASK-E02 🔵 — Completare Map/Radar Real-time Rendering

**Problema:**
`MapWindow.xaml` e `MapControl.xaml` esistono, ma il rendering real-time del radar non è collegato. Il `WriteableBitmap` è pronto nell'architettura ma il loop di aggiornamento non viene avviato.

**File coinvolti:**
- `TMRazorImproved.UI/Views/Windows/MapWindow.xaml`
- `TMRazorImproved.UI/Views/Controls/MapControl.xaml`
- `TMRazorImproved.Core/Services/MapService.cs`

**Passi:**

1. Apri `MapService.cs` e verifica quali metodi pubblici espone (es. `GetRadarBitmap()`, `UpdatePlayerPosition()`, ecc.)

2. Nel ViewModel della Map (es. `MapViewModel.cs`):
   - Crea un `WriteableBitmap` delle dimensioni del radar
   - Avvia un `DispatcherTimer` con intervallo 200ms (5fps per il radar è sufficiente)
   - Ogni tick, chiama `MapService` per aggiornare la bitmap e notifica la UI via `OnPropertyChanged`

3. Nel XAML, lega l'`Image` alla bitmap:
   ```xml
   <Image Source="{Binding RadarBitmap}" RenderOptions.BitmapScalingMode="NearestNeighbor" />
   ```

4. Ferma il timer quando la finestra viene chiusa.

**Criterio di accettazione:**
La finestra Map mostra la posizione del player e dei mob visibili con aggiornamento continuo.

---

### TASK-E03 🔵 — Completare HP Bar Overlay (TargetHPWindow)

**Problema:**
`TargetHPWindow.xaml` esiste ma mancano: modalità `TopMost` configurabile, auto-hide quando non c'è target, posizionamento salvato.

**File:** `TMRazorImproved.UI/Views/Windows/TargetHPWindow.xaml` e relativo ViewModel

**Passi:**

1. Nel ViewModel della HP Bar:
   - Aggiungi una proprietà `IsVisible` che diventa `false` quando `_worldService.Player?.AttackTarget == 0`
   - Lega `Window.Visibility` a `IsVisible` tramite converter

2. Aggiungi un'opzione nel config per `TopMost` (booleano) e lega `Window.Topmost` ad essa

3. Salva e ripristina la posizione come in TASK-E01.2

**Criterio di accettazione:**
La HP Bar appare solo quando c'è un target selezionato e rimane sopra la finestra UO.

---

### TASK-E04 🔵 — Implementare Macro Recorder UI

**Problema:**
Il backend del Macro Recorder in `MacrosService.cs` è quasi completo (intercetta pacchetti 0x06, 0x09, 0xAD, 0x6C, 0x12 e li converte in comandi macro). Manca la UI per avviare/fermare la registrazione e salvare la macro.

**File coinvolti:**
- `TMRazorImproved.UI/Views/Pages/MacrosPage.xaml`
- `TMRazorImproved.UI/ViewModels/MacrosViewModel.cs`
- `TMRazorImproved.Core/Services/MacrosService.cs`

**Passi:**

1. Apri `MacrosPage.xaml`
2. Aggiungi un pulsante "Registra" (icona `Record24`) vicino ai controlli macro esistenti
3. Nel ViewModel, aggiungi `[RelayCommand] void StartRecording()` e `StopRecording()`
4. `StartRecording` chiama `_macrosService.StartRecording()`
5. `StopRecording` chiama `_macrosService.StopRecording()` e poi mostra un dialog per il nome
6. Dopo aver inserito il nome, salva la macro con `_macrosService.SaveMacro(name, steps)`
7. Aggiungi feedback visivo: il pulsante diventa rosso durante la registrazione

**Criterio di accettazione:**
È possibile avviare la registrazione, eseguire azioni in-game, fermare la registrazione e salvare la macro generata.

---

### TASK-E05 🔵 — Completare Vendor Buy/Sell Backend

**Problema:**
`VendorPage.xaml` è ben fatto UI-wise e `VendorService.cs` ha la logica base, ma il flow completo buy/sell (invio pacchetto 0x3B per compra, 0x9F per vendita) potrebbe non essere completamente implementato o testato.

**Passi:**

1. Leggi completamente `VendorService.cs`
2. Verifica che il metodo `ExecuteBuy()` invii correttamente il pacchetto `0x3B` con:
   - Lista item da comprare (serial + quantità)
   - Formato compatibile con server SA (Stygian Abyss)
3. Verifica che il metodo `ExecuteSell()` invii correttamente il pacchetto `0x9F`
4. Testa con un vendor in-game (richiede accesso al server The Miracle)
5. Controlla i log per eventuali errori di pacchetti

**Criterio di accettazione:**
Il Vendor Agent compra gli item dalla lista configurata automaticamente quando si apre un vendor.

---

### TASK-E06 🔵 — Completare Object Inspector

**Problema:**
`InspectorPage.xaml` esiste. L'Object Inspector permette di esaminare item/mobile e vedere tutti i loro attributi. Verificare che sia funzionante.

**Passi:**

1. Apri `InspectorPage.xaml` e il relativo ViewModel
2. Verifica che il pulsante "Ispeziona" invii il target cursor al client
3. Quando l'utente clicca su un oggetto, i suoi attributi devono apparire nella pagina
4. Gli attributi da mostrare: Serial, Graphic, Hue, Name, Layer, Container, OPL (proprietà)
5. Se mancante, aggiungi un pulsante per copiare il Serial negli appunti

---

### TASK-E07 🔵 — Implementare Profile per-Shard

**Problema:**
L'originale TMRazor supporta profili diversi per shard diversi (non solo per personaggio). TMRazorImproved ha solo profili per personaggio.

**File:** `TMRazorImproved.Core/Services/ConfigService.cs`, `TMRazorImproved.UI/Views/Pages/GeneralPage.xaml`

**Passi:**

1. In `ConfigService`, aggiungi il concetto di `ShardId` (stringa) associato ad ogni profilo
2. Al login, recupera l'IP/porta del server e usali come `ShardId`
3. Nella selezione profilo della UI, mostra anche i profili del shard corrente
4. Documenta il mapping shard → profilo nella UI

---

## BLOCCO F — Qualità del Codice (P2)

---

### TASK-F01 🟡 — Refactor GetActiveConfig() — Eliminare duplicazione ×7

**Problema:**
Il pattern seguente appare identico in 7 service diversi (VendorService, OrganizerService, AutoLootService, ScavengerService, DressService, BandageHealService, RestockService):
```csharp
private VendorConfig? GetActiveConfig()
{
    var profile = _configService.CurrentProfile;
    if (profile == null) return null;
    return profile.VendorLists.FirstOrDefault(l => l.Name == profile.ActiveVendorList)
           ?? profile.VendorLists.FirstOrDefault();
}
```
Solo il tipo di config e il nome della lista cambiano.

**Passi:**

1. In `AgentServiceBase.cs` (già esistente), aggiungi un metodo generico:
   ```csharp
   protected TConfig? GetActiveConfig<TConfig>(
       Func<ProfileConfig, IList<TConfig>> getList,
       Func<ProfileConfig, string?> getActiveName)
       where TConfig : class, INamedConfig
   {
       var profile = _configService.CurrentProfile;
       if (profile == null) return null;
       var list = getList(profile);
       var active = getActiveName(profile);
       return list.FirstOrDefault(c => c.Name == active) ?? list.FirstOrDefault();
   }
   ```
   *(Richiede che `IConfigService` sia iniettato in `AgentServiceBase`)*

2. In ogni service figlio, sostituisci il metodo privato con la chiamata al metodo base.

3. Verifica che tutti i test passino.

---

### TASK-F02 🟡 — PathFindingService: rendere configurabile il range massimo

**Problema:**
Il range massimo del pathfinding è hardcoded a 100 tile. Se si deve raggiungere una destinazione più lontana, il pathfinding fallisce silenziosamente.

**File:** `TMRazorImproved.Core/Services/PathFindingService.cs`

**Passi:**

1. Apri `PathFindingService.cs`
2. Trova la costante `100` (o `MaxRange = 100`)
3. Sostituiscila con un parametro configurabile letto da `ConfigService`:
   ```csharp
   private int MaxPathRange => _configService.CurrentProfile?.PathFindingMaxRange ?? 200;
   ```
4. Aggiungi `PathFindingMaxRange` al modello `ProfileConfig` con default `200`
5. Aggiungi un controllo nella UI (Options page) per modificarlo

---

### TASK-F03 🟡 — Player.SetLast: aggiungere highlight packet 0x73

**Problema:**
`TargetApi.SetLast(uint serial)` usa `OverheadMsg("* Target *")` per indicare visivamente il target, invece del pacchetto highlight ufficiale UO `0x73`. Il comportamento visivo è diverso dall'originale.

**File:** `TMRazorImproved.Core/Services/Scripting/Api/TargetApi.cs` (riga 357-368)

**Passi:**

1. Apri `TargetApi.cs` riga 357
2. Aggiungi l'invio del pacchetto `0x73` (Target Highlight) dopo aver impostato `LastTarget`:
   ```csharp
   // Pacchetto 0x73: Animate (usato per evidenziare il target)
   // [0] 0x73, [1] 0x00 (placeholder per animazione)
   // In alternativa, usa 0xAA (Attack OK) se il server lo supporta
   var highlightPkt = new byte[] { 0x73, serial >> 24, serial >> 16, serial >> 8, (byte)serial };
   _packet.SendToClient(highlightPkt);
   ```
   *(Verifica il formato esatto del pacchetto 0x73 nella documentazione UO)*

---

## BLOCCO G — Test e Verifica

---

### TASK-G01 🟠 — Test integrazione non-mock per Scripting API

**Problema:**
I test attuali usano mock per tutti i service. Serve almeno un test che esegua uno script Python reale contro le API, verificando che le chiamate vadano a buon fine end-to-end.

**File:** `TMRazorImproved.Tests/`

**Passi:**

1. Crea `TMRazorImproved.Tests/Integration/ScriptingIntegrationTests.cs`
2. Il test deve:
   - Creare una istanza reale di `ScriptingService` (non mock) con dipendenze mock leggere
   - Eseguire uno script Python base: `Misc.SendMessage("test")`
   - Verificare che lo script giri senza eccezioni
   - Eseguire: `Player.Hits` → ritorna un valore (anche 0 se player è null)
3. Aggiungi test per ogni blocco fisso delle API (Target, Items, Mobiles, Spells)

---

### TASK-G02 🟠 — Test per i fix di thread safety (B01-B03)

**Problema:**
I fix di thread safety nei TASK-B01/B02/B03 sono difficili da testare manualmente. Serve un test di stress automatico.

**Passi:**

1. Per `WorldService.PartyMembers` (TASK-B02):
   ```csharp
   [Fact]
   public async Task PartyMembers_ConcurrentAccess_NoException()
   {
       var service = new WorldService(messenger);
       var tasks = Enumerable.Range(0, 100).Select(i => Task.Run(() =>
       {
           service.AddPartyMember((uint)i);
           _ = service.PartyMembers.Count;
           service.RemovePartyMember((uint)i);
       }));
       await Task.WhenAll(tasks); // Non deve lanciare eccezioni
   }
   ```

2. Per `DPSMeterService.TargetDamage` (TASK-B03): crea un test simile che simula accessi concorrenti in lettura e scrittura.

---

### TASK-G03 🟡 — Test di regressione API per SpecialMoves

Dopo TASK-A02:
1. Crea test che verificano che `SpecialMoves.SetPrimaryAbility()` invii il pacchetto corretto
2. Verifica che `HasPrimary` torni `true` dopo un messaggio server simulato

---

## BLOCCO H — Completamento Migrazione Scripting (Gap Residui)

---

### TASK-H01 🟠 — Documentare e comunicare differenza Misc.Distance

**Contesto:** Vedi TASK-C03. Azione: aggiornare il documento di migrazione per gli script.

**Passi:**
1. Crea (o aggiorna) `TMRazorImproved/ScriptMigrationGuide.md`
2. Documenta che `Misc.Distance` usa Chebyshev (identico all'originale)
3. Documenta che `Misc.DistanceSqrt` usa Euclidea (aggiunta nuova)
4. Lista qualsiasi altra differenza comportamentale rispetto all'originale

---

### TASK-H02 🟡 — Revisione completa ScriptGlobals — Verificare che tutte le API siano esposte

**Problema:**
Potrebbero esserci API create ma non esposte in `ScriptGlobals`. I script Python che le chiamano riceverebbero `AttributeError`.

**Passi:**

1. Apri `ScriptGlobals.cs`
2. Elenca tutte le proprietà esposte
3. Confronta con la lista degli Api file in `Scripting/Api/`:
   - `TargetApi` → `Target`
   - `ItemsApi` → `Items`
   - `MobilesApi` → `Mobiles`
   - `PlayerApi` → `Player`
   - `SpellsApi` → `Spells`
   - `GumpsApi` → `Gump`
   - `JournalApi` → `Journal`
   - `MiscApi` → `Misc`
   - `SkillsApi` → `Skills`
   - `StaticsApi` → `Statics`
   - `FriendApi` → `Friend`
   - `FiltersApi` → `Filters`
   - `TimerApi` → `Timer`
   - **`SpecialMovesApi` → `SpecialMoves`** ← da aggiungere (TASK-A02)
   - `AutoLootApi` → `AutoLoot`
   - `DressApi` → `Dress`
   - `ScavengerApi` → `Scavenger`
   - `RestockApi` → `Restock`
   - `OrganizerApi` → `Organizer`
   - `BandageHealApi` → `BandageHeal`

4. Per ogni API mancante, aggiungila a `ScriptGlobals` e al costruttore in `ScriptingService`.

---

### TASK-H03 🟠 — Implementare PlayerApi.PathFindTo(x, y, z)

**Problema:**
`PlayerApi.PathFindTo(int x, int y, int z)` è uno stub vuoto (riga 494). Gli script Python che usano `Player.PathFindTo(x,y,z)` non fanno muovere il personaggio.

**File:** `TMRazorImproved.Core/Services/Scripting/Api/PlayerApi.cs` riga 494

**Passi:**

1. Verifica se `IPathFindingService` espone un metodo `MoveTo(int x, int y, int z)` o simile
2. Se sì, delega la chiamata:
   ```csharp
   public virtual void PathFindTo(int x, int y, int z)
   {
       _cancel.ThrowIfCancelled();
       _pathfinding?.MoveTo(x, y, z);
   }
   ```
3. Se `IPathFindingService` non ha ancora questo metodo, aggiungilo e implementa la logica di pathfinding che già esiste nel servizio.
4. Alternativa: invia il pacchetto `0x06` (DoubleClick su tile di destinazione) o `0x26` (Move Request) a seconda del protocollo ClassicUO.

**Criterio di accettazione:**
Uno script con `Player.PathFindTo(1000, 1000, 0)` fa muovere il personaggio verso quella coordinata.

---

### TASK-E08 🔵 — Implementare VideoCaptureService con SharpAvi

**Problema:**
`VideoCaptureService.cs` è uno stub completo. `StartAsync()` ritorna sempre `false` con il log `"Video recording requested (Not yet implemented with SharpAvi)"`. La funzione di registrazione video non funziona.

**File coinvolti:**
- `TMRazorImproved.Core/Services/VideoCaptureService.cs`
- `TMRazorImproved.Core/TMRazorImproved.Core.csproj` (aggiungere NuGet SharpAvi)

**Passi:**

1. Aggiungi il pacchetto NuGet `SharpAvi` al progetto Core:
   ```bash
   dotnet add TMRazorImproved.Core package SharpAvi
   ```

2. Implementa `VideoCaptureService` usando `SharpAvi`:
   ```csharp
   // Schema di base — adatta al progetto
   using SharpAvi;
   using SharpAvi.Output;

   public class VideoCaptureService : IVideoCaptureService
   {
       private AviWriter? _writer;
       private IAviVideoStream? _stream;
       private CancellationTokenSource? _cts;
       public bool IsRecording { get; private set; }

       public async Task<bool> StartAsync(int fps = 15)
       {
           if (IsRecording) return false;
           var path = Path.Combine(AppContext.BaseDirectory, "Videos",
               $"capture_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.avi");
           Directory.CreateDirectory(Path.GetDirectoryName(path)!);
           _writer = new AviWriter(path) { FramesPerSecond = fps, EmitIndex1 = true };
           _stream = _writer.AddVideoStream();
           // dimensioni finestra UO — adatta come in ScreenCaptureService
           _cts = new CancellationTokenSource();
           IsRecording = true;
           _ = CaptureLoopAsync(fps, _cts.Token);
           return true;
       }

       private async Task CaptureLoopAsync(int fps, CancellationToken token)
       {
           int delayMs = 1000 / fps;
           while (!token.IsCancellationRequested)
           {
               // Cattura frame — riusa la logica di ScreenCaptureService
               await Task.Delay(delayMs, token).ContinueWith(_ => { });
           }
       }

       public Task StopAsync()
       {
           _cts?.Cancel();
           _writer?.Close();
           IsRecording = false;
           return Task.CompletedTask;
       }
   }
   ```

3. Collega il pulsante "Registra Video" nella UI (se presente) al servizio

**Criterio di accettazione:**
Il pulsante Record Video crea un file `.avi` nella cartella `Videos/` con la registrazione della finestra UO.

---

### TASK-E09 🔵 — Tool di migrazione macro legacy (.macro → nuovo formato)

**Problema:**
Gli utenti che migrano da TMRazor legacy hanno macro salvate nel vecchio formato. Non esiste un tool di conversione automatica. Le macro devono essere riscritte manualmente.

**File originale di riferimento:**
- `Razor/RazorEnhanced/Macros/` — formato legacy macro
- `TMRazorImproved.Core/Services/MacrosService.cs` — nuovo formato

**Passi:**

1. Studia il formato delle macro nel legacy (vedi `Razor/RazorEnhanced/Macros/Actions/`)
2. Studia il formato delle macro in `TMRazorImproved` (file `.macro` in output `Macros/`)
3. Crea un tool di migrazione (può essere una utility console o una funzione nella UI):
   ```
   File → Importa Macro Legacy...
   ```
4. Il tool deve:
   - Leggere il file `.macro` nel vecchio formato
   - Convertire ogni azione nel nuovo formato testuale
   - Salvare nella cartella `Macros/` del nuovo sistema
5. Aggiungi un pulsante "Importa Macro Legacy" nella `MacrosPage.xaml`

**Criterio di accettazione:**
Una macro creata con il vecchio TMRazor viene importata e funziona nel nuovo sistema senza modifiche manuali.

---

### TASK-H04 🟠 — Audit e completamento UOSteamInterpreter

**Problema:**
`UOSteamInterpreter.cs` è parzialmente implementato. I comandi IF/FOR/WHILE sono visibili ma potrebbero non essere completamente testati con tutti i casi edge del formato UOS.

**File:** `TMRazorImproved.Core/Services/Scripting/Engines/UOSteamInterpreter.cs`

**Passi:**

1. Leggi completamente `UOSteamInterpreter.cs`
2. Elenca tutti i comandi UOS presenti nel file (es. `if`, `while`, `for`, `say`, `cast`, ecc.)
3. Confronta con la lista completa dei comandi UOS nell'originale: `Razor/RazorEnhanced/UOSteamEngine.cs` (280KB)
4. Per ogni comando mancante, apri un sub-task e implementalo nel nuovo interprete
5. Crea test in `TMRazorImproved.Tests/MockTests/Scripting/` per i comandi più usati
6. Testa con script UOS reali dalla cartella `Scripts/` se presenti

**Criterio di accettazione:**
Gli script `.uos` esistenti del server The Miracle vengono eseguiti correttamente dal nuovo interprete.

---

## Riepilogo Priorità di Esecuzione

### Sprint 1 — Critico (da fare subito)
| Task | Descrizione | Stima |
|------|-------------|-------|
| TASK-A01 | ScreenCapture → WPF (no System.Drawing) | 4h |
| TASK-A02 | SpecialMoves API completa | 6h |
| TASK-A03 | Creare regions.json | 2h |
| TASK-B01 | MacrosService._recordingBuffer Clear senza lock | 30min |
| TASK-B02 | WorldService.PartyMembers thread-safe | 2h |
| TASK-B03 | DPSMeterService.TargetDamage snapshot | 30min |

### Sprint 2 — Importante
| Task | Descrizione | Stima |
|------|-------------|-------|
| TASK-C01 | ScriptStop per script non-corrente | 3h |
| TASK-C02 | FriendsService Party detection | 2h |
| TASK-D01 | Creare doors.json | 1h |
| TASK-D02 | Audit config files | 2h |
| TASK-G01 | Test integrazione scripting | 4h |
| TASK-G02 | Test stress thread safety | 3h |
| TASK-H03 | Implementare PlayerApi.PathFindTo | 2h |
| TASK-H04 | Audit e completamento UOSteamInterpreter | 8h |

### Sprint 3 — Features
| Task | Descrizione | Stima |
|------|-------------|-------|
| TASK-E01 | SpellGrid overlay completo | 8h |
| TASK-E02 | Map/Radar real-time | 6h |
| TASK-E03 | HP Bar overlay completo | 3h |
| TASK-E04 | Macro Recorder UI | 4h |
| TASK-E05 | Vendor Buy/Sell backend | 4h |
| TASK-E06 | Object Inspector | 3h |
| TASK-E08 | VideoCaptureService con SharpAvi | 6h |
| TASK-E09 | Tool migrazione macro legacy | 5h |

### Sprint 4 — Qualità e Rifinitura
| Task | Descrizione | Stima |
|------|-------------|-------|
| TASK-E07 | Profile per-shard | 8h |
| TASK-F01 | Refactor GetActiveConfig | 2h |
| TASK-F02 | PathFinding range configurabile | 1h |
| TASK-F03 | Player.SetLast highlight packet | 1h |
| TASK-H01 | Doc differenze API scripting | 2h |
| TASK-H02 | Audit ScriptGlobals completo | 2h |
| TASK-G03 | Test regressione SpecialMoves | 2h |

---

## Appendice — File Chiave di Riferimento

| File | Scopo |
|------|-------|
| `TMRazorImproved.Core/Services/ScreenCaptureService.cs` | Da migrare (System.Drawing) |
| `TMRazorImproved.Core/Services/MacrosService.cs` | Macro engine + recorder |
| `TMRazorImproved.Core/Services/WorldService.cs` | Stato mondo UO (mobili, item, player) |
| `TMRazorImproved.Core/Services/DPSMeterService.cs` | DPS tracking |
| `TMRazorImproved.Core/Services/Scripting/Api/PlayerApi.cs` | API Player per script |
| `TMRazorImproved.Core/Services/Scripting/Api/TargetApi.cs` | API Target per script |
| `TMRazorImproved.Core/Services/Scripting/Api/SpellsApi.cs` | API Spells per script |
| `TMRazorImproved.Core/Services/Scripting/Api/MiscApi.cs` | API Misc per script |
| `TMRazorImproved.Core/Services/Scripting/ScriptGlobals.cs` | Punto di esposizione di tutte le API agli script |
| `TMRazorImproved.Core/Services/Scripting/ScriptingService.cs` | Orchestrazione script Python/C#/UOS |
| `TMRazorImproved.UI/Views/Windows/SpellGridWindow.xaml` | Overlay spell grid |
| `TMRazorImproved.UI/Views/Windows/MapWindow.xaml` | Overlay mappa |
| `TMRazorImproved.UI/Views/Windows/TargetHPWindow.xaml` | Overlay HP target |
| `TMRazorImproved.UI/Views/Pages/MacrosPage.xaml` | UI macro + recorder |
| `TMRazorImproved.UI/Views/Pages/Agents/VendorPage.xaml` | UI vendor buy/sell |
| `Razor/RazorEnhanced/` | Codebase legacy di riferimento per gap analysis |
| `TMRazorImproved/ReviewArchitetturale_Marzo2026.md` | Review architetturale precedente |
