# Final Review — Mappatura Architetturale TMRazor → TMRazorImproved

**Data**: 18 Marzo 2026
**Autore**: Analisi automatica
**Scopo**: Confronto 1:1 dell'architettura legacy (TMRazor/Razor) con il nuovo codice (TMRazorImproved) per identificare gap, incompletezze e discrepanze strutturali.

---

## Indice

1. [Panoramica Architetturale](#1-panoramica-architetturale)
2. [Mappatura Servizi Core](#2-mappatura-servizi-core)
3. [Sistema Agenti](#3-sistema-agenti)
4. [Packet Handler e Rete](#4-packet-handler-e-rete)
5. [Filtri](#5-filtri)
6. [Sistema Macro](#6-sistema-macro)
7. [Motori di Scripting](#7-motori-di-scripting)
8. [UI: Dialoghi, Finestre e Inspector](#8-ui-dialoghi-finestre-e-inspector)
9. [Utilità e Servizi Secondari](#9-utilità-e-servizi-secondari)
10. [Funzionalità Mancanti](#10-funzionalità-mancanti)
11. [Funzionalità Incomplete](#11-funzionalità-incomplete)
12. [Nuove Funzionalità in TMRazorImproved](#12-nuove-funzionalità-in-tmrazorimproved)
13. [Riepilogo Task](#13-riepilogo-task)

---

## 1. Panoramica Architetturale

### Legacy (TMRazor/Razor)
- **Framework**: Windows Forms, .NET Framework 4.8
- **Pattern**: Monolitico, classi statiche, thread manuali (`Thread.Abort()`)
- **File C# totali**: ~253
- **Comunicazione interna**: Callback diretti, eventi UI
- **Configurazione**: XML + binary .razor files
- **Client**: Iniezione DLL (SetWindowsHookEx) — solo x86

### Nuovo (TMRazorImproved)
- **Framework**: WPF, .NET 10
- **Pattern**: MVVM + Dependency Injection + Messenger (CommunityToolkit.Mvvm)
- **File C# totali**: ~150+ (riduzione grazie a DI/MVVM)
- **Comunicazione interna**: `IMessenger` (mediator pattern)
- **Configurazione**: JSON (`GlobalSettings` + `UserProfile`)
- **Client**: Plugin ClassicUO + shared memory — x86/x64

### Cambio architetturale chiave
Il legacy usa **DLL injection** via `SetWindowsHookEx` (funziona solo x86). TMRazorImproved usa un **plugin ClassicUO** (`TMRazorPlugin.dll`) che comunica via **shared memory + named mutex**. Questo è un cambio fondamentale che elimina la dipendenza da Crypt.dll per l'intercettazione pacchetti (usa protocollo length-prefix).

---

## 2. Mappatura Servizi Core

| Componente Legacy | File Legacy | Equivalente TMRazorImproved | Stato |
|---|---|---|---|
| World (entity management) | `Core/World.cs` | `Core/Services/WorldService.cs` | ✅ Completo |
| Mobile | `Core/Mobile.cs` | `Shared/Models/UOEntity.cs` (classe Mobile) | ✅ Completo |
| Item | `Core/Item.cs` | `Shared/Models/UOEntity.cs` (classe Item) | ✅ Completo |
| Player | `Core/Player.cs` | `Shared/Models/UOEntity.cs` + `WorldService.Player` | ✅ Completo |
| Skills (data) | `Core/Player.cs` (Skill class) | `Shared/Models/SkillInfo.cs` + `Core/Services/SkillsService.cs` | ✅ Completo |
| Serial | `Core/Serial.cs` | Usato come `uint` (Serial struct rimosso) | ✅ Semplificato |
| Targeting | `Core/Targeting.cs` | `Core/Services/TargetingService.cs` | ✅ Completo |
| Spells | `Core/Spells.cs` | `Shared/Models/SpellDefinitions.cs` | ✅ Completo |
| Buffs | `Core/Buffs.cs`, `BuffInfo.cs` | Gestito in `WorldPacketHandler` (0xDF) | ✅ Completo |
| ObjectPropertyList | `Core/ObjectPropertyList.cs` | Proprietà in-line su `Mobile`/`Item` | ✅ Refactored |
| StealthSteps | `Core/StealthSteps.cs` | In `WorldService` o `PlayerApi` | ✅ Integrato |
| TitleBar | `Core/TitleBar.cs` | `Core/Services/TitleBarService.cs` | ✅ Completo |
| ActionQueue | `Core/ActionQueue.cs` | `Core/Services/DragDropCoordinator.cs` | ✅ Completo |
| Timer | `Core/Timer.cs` | Sostituito da `Task.Delay()` / `PeriodicTimer` | ✅ Modernizzato |
| PasswordMemory | `Core/PasswordMemory.cs` | `ConfigService` (credentials in profile) | ✅ Integrato |
| PathFinding | `RazorEnhanced/PathFinding.cs` | `Core/Services/PathFindingService.cs` | ✅ Completo |
| DragDropManager | `RazorEnhanced/DragDropManager.cs` | `Core/Services/DragDropCoordinator.cs` | ✅ Completo |
| Commands | `Core/Commands.cs` | Non presente come servizio separato | ⚠️ Vedi TASK-001 |
| MsgQueue | `Core/MsgQueue.cs` | Sostituito da `IMessenger` | ✅ Modernizzato |
| Geometry | `Core/Geometry.cs` | Funzioni in-line nei servizi | ✅ Semplificato |
| ScreenCapture | `Core/ScreenCapture.cs` | `Core/Services/ScreenCaptureService.cs` | ✅ Completo |
| VideoCapture | `Core/VideoCapture.cs` | `Core/Services/VideoCaptureService.cs` | ✅ Completo (backend) |
| Config | `RazorEnhanced/Config.cs` | `Core/Services/ConfigService.cs` | ✅ Completo |
| Profiles | `RazorEnhanced/Profiles.cs` | `ConfigService.SwitchProfile/CloneProfile/RenameProfile` | ✅ Completo |
| Settings | `RazorEnhanced/Settings.cs` | `Shared/Models/ConfigModels.cs` (GlobalSettings) | ✅ Completo |
| HotKey | `RazorEnhanced/HotKey.cs` | `Core/Services/HotkeyService.cs` | ✅ Completo |
| Journal | `RazorEnhanced/Journal.cs` | `Core/Services/JournalService.cs` | ✅ Completo |
| DPSMeter | `RazorEnhanced/DPSMeter.cs` | `Core/Services/DPSMeterService.cs` | ✅ Completo |
| Trade | `RazorEnhanced/Trade.cs` | `Core/Services/SecureTradeService.cs` | ✅ Completo |
| Vendor | `RazorEnhanced/Vendor.cs` | `Core/Services/VendorService.cs` | ✅ Completo |
| SpellGrid | `RazorEnhanced/SpellGrid.cs` | `UI/Views/Windows/SpellGridWindow.xaml` | ✅ Completo |
| ToolBar | `RazorEnhanced/ToolBar.cs` | `UI/Views/Windows/FloatingToolbarWindow.xaml` | ✅ Completo |
| Shards | `RazorEnhanced/Shards.cs` | In `ConfigService` / `GlobalSettings` | ✅ Integrato |
| Constants | `RazorEnhanced/Constants.cs` | Sparsi nei modelli / hardcoded dove serve | ✅ Semplificato |
| EncodedSpeech | `RazorEnhanced/EncodedSpeech.cs` | `Core/Utilities/EncodedSpeechHelper.cs` | ✅ Completo |
| Sound | `RazorEnhanced/Sound.cs` | `Core/Services/SoundService.cs` | ✅ Completo |
| SpecialMoves | `RazorEnhanced/SpecialMoves.cs` | `Core/Services/WeaponService.cs` | ✅ Completo |
| ItemID | `RazorEnhanced/ItemID.cs` | `Core/Utilities/ItemDataHelper.cs` | ✅ Completo |

---

## 3. Sistema Agenti

### Architettura

**Legacy**: Ogni agente è una classe statica con metodo `Engine()` eseguito su thread separato. Arresto tramite `Thread.Abort()`.

**Nuovo**: Ogni agente eredita da `AgentServiceBase` con pattern `AgentLoopAsync(CancellationToken)`. Arresto cooperativo via `CancellationToken`. Riduzione codice media: **75-80%**.

### Mappatura Dettagliata

#### TASK-002: AutoLoot — ✅ Completo (con semplificazioni)

| Aspetto | Legacy (`AutoLoot.cs`, 837 righe) | Nuovo (`AutoLootService.cs`, ~200 righe) |
|---|---|---|
| Loop principale | `Engine()` scansiona corpses in range ogni tick | `AgentLoopAsync()` riceve items via `ContainerContentMessage` |
| Coda | `ConcurrentQueue<SerialToGrab>` (serial+corpse) | `ConcurrentQueue<uint>` (solo serial) |
| Filtro proprietà | `Items.WaitForProps()` manuale | `MatchProperties()` generico |
| Thread safety | `Monitor.TryEnter()` | `ConcurrentDictionary` per tracking |
| Auto-ignore | HashSet (non thread-safe) | `ConcurrentDictionary<uint, byte>` (fix BUG-C03) |

**Come funziona nel nuovo codice**: Il servizio si registra come `IRecipient<ContainerContentMessage>` e `IRecipient<ContainerItemAddedMessage>`. Quando il server invia il contenuto di un container (corpse), il messaggio arriva direttamente al servizio che lo filtra e accoda gli item da lootare. Non serve più scansionare ciclicamente i corpse nel mondo.

**Semplificazioni**: Nessun `LootBagOverride` per item → un singolo container target dalla config.

---

#### TASK-003: Scavenger — ✅ Completo

| Aspetto | Legacy (`Scavenger.cs`, 567 righe) | Nuovo (`ScavengerService.cs`, ~150 righe) |
|---|---|---|
| Pattern | Scansione ciclica items a terra | Event-driven via `WorldItemMessage` |
| Thread | `Thread` separato | `AgentLoopAsync` async |
| Filtro | Graphics + color match | Config-driven con `ScavengerConfig` |

**Come funziona**: Il servizio si registra come `IRecipient<WorldItemMessage>`. Quando un item appare nel mondo, il messaggio viene ricevuto, confrontato con la lista scavenger attiva, e se corrisponde viene accodato per il pickup. Il loop async processa la coda con delay configurabile.

---

#### TASK-004: Organizer — ⚠️ Incompleto

| Aspetto | Legacy (`Organizer.cs`, 522 righe) | Nuovo (`OrganizerService.cs`, ~106 righe) |
|---|---|---|
| Supporto Amount | ✅ Parziale (amount per item, -1 = tutto) | ❌ **Manca** |
| Stack parziali | ✅ Split stack se necessario | ❌ **Manca** |
| Esecuzione | Loop continuo | Single-pass + `OnComplete` |
| Refresh container | `Items.WaitForContents()` manuale | Cache WorldService |

**Come funziona nel nuovo codice**: LINQ query su `_worldService.Items` filtrato per container sorgente e graphics nella lista. Sposta tutti gli item corrispondenti con `_dragDropCoordinator`. Si ferma dopo un singolo passaggio.

**Cosa manca**:
- Il campo `Amount` dell'item nella lista organizer non è rispettato → sposta TUTTI gli item corrispondenti, non un numero specifico.
- Non supporta split di stack (es. "sposta solo 50 di questi 100 reagenti").
- Single-pass: se arrivano nuovi item dopo il primo passaggio, non vengono processati.

**Impatto utente**: Un utente che configura "sposta 100 bandage dal bank" sposterebbe TUTTO il suo stack invece che solo 100. **Regressione funzionale**.

**Implementazione suggerita**: Aggiungere parametro `amount` nel loop di spostamento:
```csharp
// In AgentLoopAsync, per ogni item nella config list:
int remaining = configItem.Amount == -1 ? int.MaxValue : configItem.Amount;
foreach (var item in matchingItems)
{
    if (remaining <= 0) break;
    int toMove = Math.Min(item.Amount, remaining);
    await _dragDropCoordinator.MoveAsync(item.Serial, config.Destination, toMove, ct);
    remaining -= toMove;
}
```

---

#### TASK-005: Dress — ✅ Completo (con semplificazioni)

| Aspetto | Legacy (`Dress.cs`, 807 righe) | Nuovo (`DressService.cs`, ~200 righe) |
|---|---|---|
| Two-handed weapons | ✅ ~60 righe di logica | ✅ ~25 righe via `_weaponService.IsTwoHanded()` |
| UO3D macro support | ✅ `EquipItemMacro` speciale | ❌ **Rimosso** |
| Conflict flag | ✅ Opzionale (checkbox) | ❌ Sempre attivo |

**Come funziona**: Queue-based design con `ActionTask(Serial, Layer, IsDress)`. Per vestire: scorre la lista, verifica conflitti (specialmente armi two-handed), undress conflicting, poi equip. Per svestire: unequip ogni pezzo nella lista verso il bag configurato.

**Semplificazioni**:
- **UO3D support rimosso**: Non rilevante per TmClient (ClassicUO-based).
- **Conflict flag rimosso**: Sempre effettua undress dei pezzi conflicting (comportamento più sicuro).

---

#### TASK-006: BandageHeal — ⚠️ Incompleto

| Aspetto | Legacy (`BandageHeal.cs`, 635 righe) | Nuovo (`BandageHealService.cs`, ~158 righe) |
|---|---|---|
| Ricerca bandage | ✅ Cerca per ItemID (0x0E21) | ❌ Assume serial da config |
| Conteggio bandage | ✅ Warning se poche | ❌ **Manca** |
| Buff tracking | ✅ HealingSkill/Veterinary buff | ❌ **Manca** |
| Formula delay | ✅ Dex + custom | ✅ Solo dex |
| Target modes | ✅ Self/Last/Friend/FriendOrSelf | ✅ Identici |

**Come funziona**: Loop async che: 1) sceglie target tramite `GetTargetSerial()`, 2) double-click bandage, 3) aspetta cursor (150ms), 4) invia target, 5) delay calcolato su dex.

**Cosa manca**:
- **Ricerca automatica bandage**: Il legacy cerca bandage nel backpack per ItemID. Il nuovo assume che il serial sia nella config. Se il serial diventa invalido (bandage usate tutte, stack cambiato), l'agente non funziona.
- **Conteggio/Warning**: Nessun avviso "bandage in esaurimento".
- **Buff wait**: Il legacy aspetta che il buff `HealingSkill` sparisca prima di applicare un'altra bandage. Il nuovo usa solo un delay fisso — potrebbe applicare bandage troppo presto.

**Implementazione suggerita per ricerca bandage**:
```csharp
private uint FindBandageSerial()
{
    var backpack = _worldService.Player?.Backpack;
    if (backpack == 0) return 0;

    var bandage = _worldService.Items
        .FirstOrDefault(i => i.Container == backpack && i.GraphicId == 0x0E21);
    return bandage?.Serial ?? 0;
}
```

---

#### TASK-007: Restock — ✅ Completo (con semplificazione)

| Aspetto | Legacy (`Restock.cs`, 501 righe) | Nuovo (`RestockService.cs`, ~128 righe) |
|---|---|---|
| Amount limit | ✅ Per-item | ✅ Per-item |
| Container scan | ✅ Ricorsivo (nested bags) | ⚠️ Flat (solo primo livello) |
| Esecuzione | Loop continuo | Single-pass |

**Come funziona**: Per ogni item nella config, conta quanti ne ha nella destinazione, calcola la differenza rispetto al limite, e sposta dal sorgente.

**Semplificazione**: Scansione flat dei container — se un utente ha items in sub-bags dentro il bag principale, il conteggio potrebbe essere errato. Impatto basso per la maggior parte degli use case.

---

#### TASK-008: Friends — ✅ Completo

| Aspetto | Legacy (`Friend.cs`, 626 righe) | Nuovo (`FriendsService.cs`, ~142 righe) |
|---|---|---|
| IsFriend() | ✅ Serial + party + guild + faction | ✅ Identico |
| Guild detection | ✅ Via OPL property[0] | ✅ Identico |
| Faction detection | ✅ [SL], [TB], [CoM], [MiN] | ✅ Identico |
| Multi-list | ✅ Una lista attiva | ✅ Config-driven |

**Parità funzionale completa**. L'implementazione è un port diretto con architettura DI.

---

#### TASK-009: VendorBuy/Sell — ✅ Completo

| Aspetto | Legacy (`Vendor.cs`, ~1000 righe) | Nuovo (`VendorService.cs`) |
|---|---|---|
| Buy automation | ✅ Lista compra | ✅ Via `IRecipient<VendorBuyMessage>` |
| Sell automation | ✅ Lista vendita | ✅ Via `IRecipient<VendorSellMessage>` |
| Packet parsing | ✅ 0x74/0x9E | ✅ In WorldPacketHandler |

**Come funziona**: Il `WorldPacketHandler` parsa i pacchetti 0x74 (Buy) e 0x9E (Sell) e invia messaggi. Il `VendorService` li riceve e risponde automaticamente secondo la configurazione.

---

## 4. Packet Handler e Rete

### Architettura

**Legacy** (`Razor/Network/`):
- `PacketHandler.cs`: Routing con callback (viewer/filter)
- `Handlers.cs`: ~50+ handler statici
- `PacketTable.cs`: Tabella dimensioni statiche
- `Packet.cs`: Serializzazione/deserializzazione

**Nuovo** (`TMRazorImproved.Core/`):
- `PacketService.cs`: Legge da shared memory, dispatcha
- `WorldPacketHandler.cs`: Handler centralizzato (~70+ pacchetti)
- `PacketBuilder.cs`: Costruzione pacchetti in uscita
- `UOBufferReader.cs`: Parser binario con position tracking

### Copertura Pacchetti

#### Server → Client (Legacy: 56 pacchetti, Nuovo: 60+ pacchetti)

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
| 0x20 | MobileUpdate | ✅ | ✅ | Con filtri |
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
| 0x78 | MobileIncoming | ✅ | ✅ | Con filtri |
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
| 0xA1 | HitsUpdate | ✅ | ✅ | |
| 0xA2 | ManaUpdate | ✅ | ✅ | |
| 0xA3 | StamUpdate | ✅ | ✅ | |
| 0xA8 | ServerList | ✅ | ✅ | |
| 0xAA | AttackOK | ❌ | ✅ | **Nuovo** |
| 0xAB | DisplayStringQuery | ✅ | ✅ | |
| 0xAD | EncodedUnicodeSpeech | ❌ | ✅ | **Nuovo** |
| 0xAE | UnicodeSpeech | ✅ | ✅ | Con filtri |
| 0xAF | DeathAnimation | ✅ | ✅ | |
| 0xB0 | Gump | ✅ | ✅ | |
| 0xB8 | Profile | ✅ | ✅ | |
| 0xB9 | Features | ✅ | ✅ | |
| 0xBA | TrackingArrow | ✅ | ✅ | |
| 0xBC | ChangeSeason | ✅ | ✅ | |
| 0xBD | ClientVersion | ❌ | ✅ | **Nuovo** |
| 0xBF | ExtendedPacket | ✅ | ✅ | |
| 0xC0 | GraphicalEffect | ❌ | ✅ | **Nuovo** |
| 0xC1 | LocalizedMessage | ✅ | ✅ | Con filtri |
| 0xC2 | UnicodePrompt | ✅ | ✅ | |
| 0xC8 | SetUpdateRange | ✅ | ✅ | |
| 0xCC | LocalizedMsgAffix | ✅ | ✅ | Con filtri |
| 0xD6 | OPL (Properties) | ✅ | ✅ | |
| 0xD8 | CustomHouseInfo | ✅ | ✅ | |
| 0xDD | CompressedGump | ✅ | ✅ | |
| 0xDF | BuffDebuff | ✅ | ✅ | |
| 0xE2 | TestAnimation | ✅ | ✅ | |
| 0xF0 | RunUO Protocol | ✅ | ✅ | |
| 0xF3 | SA WorldItem | ✅ | ✅ | |
| 0xF5 | MapDetails (alt) | ✅ | ✅ | |
| 0xF6 | MoveBoat | ✅ | ✅ | |

#### Client → Server (Legacy: 18, Nuovo: 13)

| Packet ID | Nome | Legacy | Nuovo | Note |
|---|---|---|---|---|
| 0x00 | CreateCharacter | ✅ | ❌ | Non necessario (TmClient gestisce) |
| 0x02 | MovementRequest | ✅ | ✅ | |
| 0x05 | AttackRequest | ✅ | ✅ | |
| 0x06 | DoubleClick | ✅ | ✅ | |
| 0x07 | LiftRequest | ✅ | ✅ | |
| 0x08 | DropRequest | ✅ | ✅ | |
| 0x09 | SingleClick | ✅ | ✅ | |
| 0x12 | TextCommand | ✅ | ✅ | |
| 0x13 | EquipRequest | ✅ | ✅ | |
| 0x3A | SetSkillLock | ✅ | ❌ | Gestito via API SkillsService |
| 0x5D | PlayCharacter | ✅ | ❌ | TmClient gestisce |
| 0x6F | TradeRequest | ✅ | ✅ | |
| 0x75 | RenameMobile | ✅ | ✅ | |
| 0x7D | MenuResponse | ✅ | ❌ | ⚠️ Vedi TASK-010 |
| 0x80 | ServerListLogin | ✅ | ❌ | TmClient gestisce |
| 0x91 | GameLogin | ✅ | ❌ | TmClient gestisce |
| 0xA0 | PlayServer | ✅ | ❌ | TmClient gestisce |
| 0xB1 | GumpResponse | ✅ | ✅ | |
| 0xBF | ExtendedCommand | ✅ | ✅ | |
| 0xC2 | UnicodePromptSend | ✅ | ❌ | ⚠️ Vedi TASK-011 |
| 0xD7 | EncodedPacket | ✅ | ✅ | |
| 0xF8 | CreateChar (alt) | ✅ | ❌ | TmClient gestisce |

**Nota**: I pacchetti login/character creation (0x00, 0x5D, 0x80, 0x91, 0xA0, 0xF8) non sono necessari perché TmClient gestisce direttamente l'autenticazione.

---

## 5. Filtri

### Architettura

**Legacy**: 13 file separati in `Razor/Filters/`, ogni filtro è una classe con pattern eredità da `Filter`.

**Nuovo**: Consolidato in `FilterHandler.cs` + `TargetFilterService.cs`. Registrazione diretta come viewer/filter su `IPacketService`.

### Mappatura

| Filtro Legacy | File Legacy | Equivalente Nuovo | Stato |
|---|---|---|---|
| Death (0x2C) | `Death.cs` | `FilterHandler` | ✅ |
| Light (0x4E, 0x4F) | `Light.cs` | `FilterHandler` | ✅ |
| MessageFilter (0x1C, 0xC1) | `MessageFilter.cs` | `FilterHandler` (Poison, Karma, Snoop) | ✅ |
| MobileFilter (dragon/drake/daemon) | `MobileFilter.cs` | `FilterHandler.MorphGraphic()` | ✅ |
| Season (0xBC) | `Season.cs` | `FilterHandler` | ✅ |
| SoundFilters (0x54) | `SoundFilters.cs` | `FilterHandler` | ✅ |
| StaffItems | `StaffItems.cs` | `FilterHandler` | ✅ |
| StaffNpcs | `StaffNpcs.cs` | `FilterHandler` | ✅ |
| TargetFilterManager | `TargetFilterManager.cs` | `TargetFilterService.cs` | ✅ |
| VetRewardGump | `VetRewardGump.cs` | `FilterHandler` (0xB0, 0xDD) | ✅ |
| WallStaticFilter | `WallStaticFilter.cs` | `FilterHandler.MorphGraphic()` | ✅ |
| Weather (0x65) | `Weather.cs` | `FilterHandler` | ✅ |

**Filtri nuovi in TMRazorImproved**:
- Bard Music Filter (0x6D)
- Party Invite Block (0xBF sub 0x06)
- Trade Request Block (0x6F)
- Footsteps Filter (0x54, sound IDs 0x12-0x1A)
- Custom Graph Filters (utente definisce graphic ID → replacement)

**Copertura filtri: 100%** — tutti i filtri legacy sono presenti, più funzionalità aggiuntive.

---

## 6. Sistema Macro

### Architettura

**Legacy** (`Razor/RazorEnhanced/Macros/`):
- 43 classi dedicate (una per tipo di azione), tutte eredità da `MacroAction`
- Serializzazione binaria/XML per persistenza
- Esecuzione sequenziale con `MacroAction.Execute()`
- Controllo flusso: IF/ELSEIF/ELSE/ENDIF, WHILE/ENDWHILE, FOR/ENDFOR come classi

**Nuovo** (`TMRazorImproved.Core/Services/MacrosService.cs`, 1018 righe):
- Comandi testuali unificati (switch/case in `ExecuteActionAsync`)
- Two-pass: `BuildJumpTables()` → `ExecuteWithControlFlowAsync()`
- Persistenza come `MacroStep` (Command string + IsEnabled bool)
- `LegacyMacroMigrator.cs` per convertire macro legacy

### Mappatura Azioni Macro

| Azione Legacy | Classe Legacy | Comando Nuovo | Stato |
|---|---|---|---|
| AttackAction | `AttackAction.cs` | `ATTACK` | ⚠️ Solo serial-based (vedi sotto) |
| ArmDisarm | `ArmDisarmAction.cs` | `ARMDISARM` | ✅ |
| Bandage | `BandageAction.cs` | `BANDAGE` | ✅ |
| CastSpell | `CastSpellAction.cs` | `CAST` | ✅ |
| ClearJournal | `ClearJournalAction.cs` | `CLEARJOURNAL` | ✅ |
| Comment | `CommentAction.cs` | `// commento` | ✅ |
| Disconnect | `DisconnectAction.cs` | — | ❌ Rimosso intenzionalmente |
| DoubleClick | `DoubleClickAction.cs` | `DOUBLECLICK` / `DCLICK` | ✅ |
| Drop | `DropAction.cs` | `DROP` | ✅ |
| Fly | `FlyAction.cs` | `FLY` / `LAND` | ✅ |
| GumpResponse | `GumpResponseAction.cs` | `RESPONDGUMP` | ✅ |
| IfAction | `IfAction.cs` | `IF` | ✅ |
| ElseIfAction | `ElseIfAction.cs` | `ELSEIF` | ✅ |
| ElseAction | `ElseAction.cs` | `ELSE` | ✅ |
| EndIfAction | `EndIfAction.cs` | `ENDIF` | ✅ |
| WhileAction | `WhileAction.cs` | `WHILE` | ✅ |
| EndWhileAction | `EndWhileAction.cs` | `ENDWHILE` | ✅ |
| ForAction | `ForAction.cs` | `FOR` | ✅ |
| EndForAction | `EndForAction.cs` | `ENDFOR` | ✅ |
| InvokeVirtue | `InvokeVirtueAction.cs` | `INVOKEVIRTUE` | ✅ |
| Messaging | `MessagingAction.cs` | `SAY` / `MSG` | ✅ |
| Mount | `MountAction.cs` | `MOUNT` / `DISMOUNT` | ✅ |
| MoveItem | `MoveItemAction.cs` | `MOVEITEM` | ✅ |
| MovementAction | `MovementAction.cs` | — | ❌ Rimosso |
| Pause | `PauseAction.cs` | `PAUSE` / `WAIT` | ✅ |
| PickUp | `PickupAction.cs` | `PICKUP` | ✅ |
| PromptResponse | `PromptResponseAction.cs` | `PROMPTRESPONSE` | ✅ |
| QueryStringResponse | `QueryStringResponseAction.cs` | — | ❌ Rimosso (raro) |
| RemoveAlias | `RemoveAliasAction.cs` | `REMOVEALIAS` | ✅ |
| RenameMobile | `RenameMobileAction.cs` | `RENAMEMOBILE` | ✅ |
| Resync | `ResyncAction.cs` | `RESYNC` | ✅ |
| RunOrganizerOnce | `RunOrganizerOnceAction.cs` | `RUNORGANIZER` | ✅ |
| SetAbility | `SetAbilityAction.cs` | — | ⚠️ Vedi TASK-012 |
| SetAlias | `SetAliasAction.cs` | `SETALIAS` | ✅ |
| TargetAction | `TargetAction.cs` | `TARGET` | ✅ |
| TargetResource | `TargetResource.cs` | `TARGETRESOURCE` | ✅ |
| ToggleWarMode | `ToggleWarModeAction.cs` | `WARMODE` | ✅ |
| UseContextMenu | `UseContextMenuAction.cs` | `USECONTEXTMENU` | ✅ |
| UseEmote | `UseEmoteAction.cs` | `EMOTE` | ✅ |
| UsePotion | `UsePotionAction.cs` | `USEPOTIONTYPE` | ✅ |
| UseSkill | `UseSkillAction.cs` | `USESKILL` | ✅ |
| WaitForGump | `WaitForGumpAction.cs` | `WAITFORGUMP` | ✅ |
| WaitForTarget | `WaitForTargetAction.cs` | `WAITFORTARGET` | ✅ |

### TASK-012: AttackAction — Modalità Avanzate Mancanti

Il legacy `AttackAction` supporta 6 modalità:
1. **LastTarget** — attacca l'ultimo target
2. **Serial** — attacca serial specifico
3. **Alias** — attacca alias salvato
4. **Nearest** — attacca il più vicino (con filtro notoriety)
5. **Farthest** — attacca il più lontano (con filtro notoriety)
6. **ByType** — attacca per graphic ID

Il nuovo `ATTACK` supporta **solo Serial** (via `uint.TryParse(args)`).

**Impatto**: Macro che usano `ATTACK Nearest Enemy` o `ATTACK ByType 0x0190` non funzionerebbero.

**Implementazione suggerita**:
```csharp
case "ATTACK":
    if (args.StartsWith("nearest", StringComparison.OrdinalIgnoreCase))
        AttackNearest(args); // parse notoriety filter
    else if (args.StartsWith("farthest", StringComparison.OrdinalIgnoreCase))
        AttackFarthest(args);
    else if (uint.TryParse(args, out uint atkSerial))
        _packetService.SendToServer(PacketBuilder.Attack(atkSerial));
    break;
```

### TASK-013: SetAbility Macro Command Mancante

Il legacy `SetAbilityAction` permette di attivare abilità primaria/secondaria nelle macro. Il nuovo non ha un comando `SETABILITY`.

**Implementazione suggerita**: Aggiungere case in `ExecuteActionAsync`:
```csharp
case "SETABILITY":
    if (args.Equals("primary", StringComparison.OrdinalIgnoreCase))
        _weaponService.SetPrimaryAbility();
    else if (args.Equals("secondary", StringComparison.OrdinalIgnoreCase))
        _weaponService.SetSecondaryAbility();
    break;
```

---

## 7. Motori di Scripting

### Mappatura

| Engine Legacy | File Legacy | Equivalente Nuovo | Stato |
|---|---|---|---|
| PythonEngine | `PythonEngine.cs` | `ScriptingService.cs` (IronPython 3.4) | ✅ Migliorato |
| CSharpEngine | `CSharpEngine.cs` (CodeDom) | `CSharpScriptEngine.cs` (Roslyn) | ✅ Modernizzato |
| UOSteamEngine | `UOSteamEngine.cs` (8331 righe) | `UOSteamInterpreter.cs` | ✅ Presente |
| ScriptRecorder | `ScriptRecorder.cs` | `ScriptRecorderService.cs` | ✅ Completo |

### API Scripting

| API Legacy | Equivalente Nuovo | Stato |
|---|---|---|
| `Misc` | `MiscApi.cs` | ✅ |
| `Items` | `ItemsApi.cs` | ✅ |
| `Mobiles` | `MobilesApi.cs` | ✅ |
| `Player` | `PlayerApi.cs` | ✅ |
| `Spells` | `SpellsApi.cs` | ✅ |
| `Target` | `TargetApi.cs` | ✅ |
| `Gumps` | `GumpsApi.cs` | ✅ |
| `Journal` | `JournalApi.cs` | ✅ |
| `Skills` | `SkillsApi.cs` | ✅ |
| `Statics` | `StaticsApi.cs` | ✅ |
| `Sound` | `SoundApi.cs` | ✅ |
| `Timer` | `TimerApi.cs` | ✅ |
| `Friend` | `FriendApi.cs` | ✅ |
| `SpecialMoves` | `SpecialMovesApi.cs` | ✅ |
| `Hotkey` | `HotkeyApi.cs` | ✅ |
| `Filters` | `FiltersApi.cs` | ✅ |
| `Agent` (AutoLoot, etc.) | `AgentApis.cs` | ✅ |
| `PathFinding` | Non in Api/ | ⚠️ Vedi TASK-014 |
| `DPSMeter` (script) | Non in Api/ | ⚠️ Vedi TASK-015 |
| `PacketLogger` (script) | Non in Api/ | ⚠️ Vedi TASK-016 |
| `CUO` (ClassicUO) | Non applicabile | N/A |

### TASK-014: PathFinding API Mancante nello Scripting

`PathFindingService.cs` esiste come servizio ma non è esposto agli script tramite un `PathFindingApi.cs`.

**Implementazione suggerita**: Creare `TMRazorImproved.Core/Services/Scripting/Api/PathFindingApi.cs`:
```csharp
public class PathFindingApi
{
    private readonly IPathFindingService _pathFinding;

    public PathFindingApi(IPathFindingService pathFinding)
        => _pathFinding = pathFinding;

    public bool MoveTo(int x, int y, int z)
        => _pathFinding.NavigateTo(x, y, z);

    public List<(int X, int Y)> GetPath(int startX, int startY, int endX, int endY)
        => _pathFinding.CalculatePath(startX, startY, endX, endY);
}
```
Poi registrarlo in `ScriptGlobals`.

### TASK-015: DPSMeter API Mancante nello Scripting

`DPSMeterService.cs` esiste ma non è esposto via API scripting.

### TASK-016: PacketLogger API Mancante nello Scripting

`PacketLoggerService.cs` esiste ma l'accesso diretto dai script non è esposto. Il legacy permetteva agli script di catturare e analizzare pacchetti raw.

### Miglioramenti del Nuovo Sistema di Scripting

1. **Cancellation a 2 livelli**:
   - Level 1: `Thread.Interrupt()` per chiamate blocking (zero overhead normalmente)
   - Level 2: `sys.settrace()` ogni 50 statement per loop CPU-bound
2. **Override time.sleep**: Rediretto a `Misc.Pause()` per intercettare blocking involontari
3. **Roslyn**: Compilatore C# moderno vs CodeDom (più veloce, migliore supporto .NET 10)
4. **DI-based**: API iniettate via `ScriptGlobals` invece di registrazione runtime dei moduli

---

## 8. UI: Dialoghi, Finestre e Inspector

### Architettura

**Legacy**: Windows Forms, dialoghi modali separati (16+ classi `Enhanced*`)
**Nuovo**: WPF + Fluent Design, pagine tabbed con MVVM, finestre floating

### Mappatura Completa

| UI Legacy | File Legacy | Equivalente Nuovo | Stato |
|---|---|---|---|
| GumpInspector | `EnhancedGumpInspector.cs` | `InspectorPage.xaml` (tab Gumps) | ✅ Consolidato |
| ItemInspector | `EnhancedItemInspector.cs` | `InspectorPage.xaml` (tab Entity) | ✅ Consolidato |
| MobileInspector | `EnhancedMobileInspector.cs` | `InspectorPage.xaml` (tab Entity) | ✅ Consolidato |
| StaticInspector | `EnhancedStaticInspector.cs` | `InspectorPage.xaml` (tab Map) | ✅ Consolidato |
| ObjectInspector | `EnhancedObjectInspector.cs` | `InspectorPage.xaml` (generico) | ✅ Consolidato |
| ScriptEditor | `EnhancedScriptEditor.cs` (FastColoredTextBox) | `ScriptingPage.xaml` (AvalonEdit) | ✅ Modernizzato |
| ChangeLog | `EnhancedChangeLog.cs` | `ChangelogWindow.xaml` | ✅ |
| Launcher | `EnhancedLauncher.cs` | `GeneralPage.xaml` (integrato) | ✅ Integrato |
| ProfileAdd | `EnhancedProfileAdd.cs` | `GeneralPage.xaml` (InputBox inline) | ✅ Integrato |
| ProfileClone | `EnhancedProfileClone.cs` | `GeneralViewModel.CloneProfile()` | ✅ Integrato |
| ProfileRename | `EnhancedProfileRename.cs` | `GeneralViewModel.RenameProfile()` | ✅ Integrato |
| DressAddUndressLayer | `EnhancedDressAddUndressLayer.cs` | `DressPage.xaml` (inline) | ✅ Integrato |
| AutolootEditItemProps | `EnhancedAutolootEditItemProps.cs` | `EditLootItemWindow.xaml` | ✅ Migliorato |
| ScavengerEditItemProps | `EnhancedScavengerEditItemProps.cs` | Inline in `ScavengerPage.xaml` | ✅ Integrato |
| FriendAddPlayerManual | `EnhancedFriendAddPlayerManual.cs` | `FriendsPage.xaml` (inline) | ✅ Integrato |
| FriendAddGuildManual | `EnhancedFriendAddGuildManual.cs` | `FriendsPage.xaml` (inline) | ✅ Integrato |
| RE_MessageBox | `RE_MessageBox.cs` | WPF MessageBox / Fluent dialogs | ✅ |
| SpellGrid | `Razor/UI/Grids-Bars/SpellGrid.cs` | `SpellGridWindow.xaml` | ✅ |
| Toolbar | `Razor/UI/Grids-Bars/Toolbar.cs` | `FloatingToolbarWindow.xaml` | ✅ |
| DPSMeter | `Razor/UI/Agent/DPSMeter.cs` | `DPSMeterWindow.xaml` | ✅ |
| Screenshot UI | `Razor/UI/Video-Screen/ScreenShot.cs` | Backend solo (`ScreenCaptureService`) | ⚠️ Vedi TASK-017 |
| VideoRecorder UI | `Razor/UI/Video-Screen/VideoRecorder.cs` | Backend solo (`VideoCaptureService`) | ⚠️ Vedi TASK-017 |

### TASK-017: UI Video/Screenshot Mancante

I servizi `ScreenCaptureService.cs` e `VideoCaptureService.cs` esistono nel backend ma **non c'è una pagina UI** per configurare le impostazioni (path output, formato, FPS, risoluzione, hotkey cattura).

**Impatto utente**: L'utente non può configurare screenshot/video recording dall'interfaccia. Può solo usarli via hotkey/script.

**Implementazione suggerita**: Creare `Views/Pages/MediaPage.xaml` con:
- Path di output per screenshot
- Formato (PNG/JPG/BMP)
- Impostazioni video (FPS, codec)
- Hotkey per cattura rapida
- Anteprima ultima cattura

### Finestre Nuove in TMRazorImproved

| Finestra | File | Descrizione |
|---|---|---|
| HuePickerWindow | `HuePickerWindow.xaml` | Selettore colori UO dedicato |
| OverheadMessageOverlay | `OverheadMessageOverlay.xaml` | Chat bubbles floating |
| TargetHPWindow | `TargetHPWindow.xaml` | Barra HP target floating |
| MapWindow | `MapWindow.xaml` | Mappa mondo dedicata con zoom |
| SearchOverlay | `SearchOverlay.xaml` | Ricerca globale nell'UI |

### Pagine Nuove in TMRazorImproved

| Pagina | File | Descrizione |
|---|---|---|
| DashboardPage | `DashboardPage.xaml` | Overview principale con stato connessione |
| PacketLoggerPage | `PacketLoggerPage.xaml` | Debug pacchetti in tempo reale |
| GalleryPage | `GalleryPage.xaml` | Galleria screenshot |
| GumpListPage | `GumpListPage.xaml` | Lista gump aperti |
| CountersPage | `CountersPage.xaml` | Contatori item |
| DisplayPage | `DisplayPage.xaml` | Impostazioni display |
| SoundPage | `SoundPage.xaml` | Impostazioni audio |

---

## 9. Utilità e Servizi Secondari

### Mappatura

| Componente Legacy | File Legacy | Equivalente Nuovo | Stato |
|---|---|---|---|
| UoMod (DLL injection) | `Network/UoMod.cs` | `Core/Services/UOModService.cs` | ✅ Migliorato (async) |
| Ping | `Network/Ping.cs` | In `WorldPacketHandler` (0x73) | ✅ Integrato |
| UoWarper | `Network/UoWarper.cs` | — | ❌ Non applicabile (TmClient ≠ OSI) |
| PacketLogger | `Network/PacketLogger.cs` | `Core/Services/PacketLoggerService.cs` | ✅ Completo |
| PacketTable | `Network/PacketTable.cs` | Non necessario (length-prefix protocol) | ✅ Rimosso by design |
| Client (abstract) | `Client/Client.cs` | `Shared/Interfaces/IClientAdapter.cs` | ✅ |
| ClassicUO adapter | `Client/ClassicUO.cs` | `Core/Services/Adapters/ClassicUOAdapter.cs` | ⚠️ Stub |
| OSI adapter | `Client/OSIClient.cs` | `Core/Services/Adapters/OsiClientAdapter.cs` | ⚠️ Stub |
| UOAssist | `Client/UOAssist.cs` | — | ❌ Non necessario |
| AutoDoc | `RazorEnhanced/AutoDoc.cs` | — | ❌ Vedi TASK-018 |
| SeasonManager | `RazorEnhanced/SeasonManager.cs` | In `FilterHandler` | ✅ Integrato |
| CircularBuffer | `RazorEnhanced/CircularBuffer.cs` | Non necessario (buffer managed) | ✅ |
| JsonData | `RazorEnhanced/JsonData.cs` | System.Text.Json nativo | ✅ |
| Multi | `RazorEnhanced/Multi.cs` | Non trovato | ⚠️ Vedi TASK-019 |
| ProtoControl (gRPC) | `RazorEnhanced/Proto-Control/` | `Core/Services/ProtoControlService.cs` | ✅ Completo |
| LanguageHelper | `RazorEnhanced/UI/LanguageHelper.cs` | `UI/Utilities/LanguageHelper.cs` | ✅ Completo |
| Localization | `Strings.resx` + `Strings.it.resx` | `Shared/Resources/Strings.resx` + `.it.resx` | ✅ Completo |

### TASK-018: AutoDoc Mancante

Il legacy `AutoDoc.cs` genera documentazione automatica delle API scripting. Non è stato portato.

**Impatto**: Gli sviluppatori di script non hanno documentazione auto-generata. Impatto basso — può essere generato da commenti XML.

### TASK-019: Multi (House/Building Data) Mancante

Il legacy `Multi.cs` gestisce dati strutturali di case/edifici multi-tile. Non trovato nel nuovo codice.

**Impatto**: Funzionalità di piazzamento/ispezione case potrebbe non funzionare.

**Implementazione suggerita**: Verificare se il pacchetto 0xD8 (CustomHouseInfo) copre questa funzionalità. Se serve, creare `MultiService.cs` che carica i dati multi da file Ultima.

### Nota sugli Adapter (ClassicUO/OSI)

`ClassicUOAdapter.cs` e `OsiClientAdapter.cs` sono **stub intenzionali**. Tutti i metodi (`Connect`, `Disconnect`, `ReceivePacket`, `SendPacket`) sono placeholder con commenti che spiegano che la funzionalità reale è gestita da `PacketService` + shared memory. Questo è **by design** — l'interfaccia `IClientAdapter` esiste per futura estensibilità, non come bug.

---

## 10. Funzionalità Mancanti

Elenco completo delle funzionalità presenti nel legacy ma **assenti** nel nuovo codice.

| ID | Funzionalità | Impatto | Priorità |
|---|---|---|---|
| TASK-010 | MenuResponse (pacchetto 0x7D C→S) | Script/macro che rispondono a menu legacy non funzionano | Media |
| TASK-011 | UnicodePromptSend (pacchetto 0xC2 C→S) | Script che inviano risposte a prompt Unicode | Bassa |
| TASK-012 | ATTACK modalità avanzate (Nearest/Farthest/ByType) nelle macro | Macro di combattimento avanzate non funzionano | Alta |
| TASK-013 | SETABILITY comando macro | Macro che attivano abilità speciali | Media |
| TASK-014 | PathFinding API per scripting | Script che usano navigazione automatica | Media |
| TASK-015 | DPSMeter API per scripting | Script che leggono dati DPS | Bassa |
| TASK-016 | PacketLogger API per scripting | Script che catturano pacchetti raw | Bassa |
| TASK-017 | UI pagina Video/Screenshot | Utente non può configurare cattura media | Media |
| TASK-018 | AutoDoc (generazione documentazione API) | Sviluppatori script senza docs auto | Bassa |
| TASK-019 | Multi/House data service | Ispezione/piazzamento case | Bassa |

---

## 11. Funzionalità Incomplete

Funzionalità migrate ma con **implementazione parziale**.

| ID | Funzionalità | Dettaglio Incompletezza | Impatto | Priorità |
|---|---|---|---|---|
| TASK-004 | Organizer Amount field | Non rispetta il campo quantità — sposta TUTTO invece del numero configurato | Regressione funzionale | **Alta** |
| TASK-006 | BandageHeal ricerca bandage | Assume serial fisso da config invece di cercare per ItemID nel backpack | Agente smette di funzionare quando stack finisce | **Alta** |
| TASK-006b | BandageHeal buff tracking | Non aspetta fine buff HealingSkill prima di ri-applicare | Spreca bandage | Media |
| TASK-020 | PacketLoggerService template parsing | Commento `// TODO: Implement Template parsing if needed` a linea 179 | Solo hex dump, no parsing strutturato | Bassa |

---

## 12. Nuove Funzionalità in TMRazorImproved

Funzionalità **non presenti** nel legacy, aggiunte nel nuovo codice.

### Nuovi Agenti
| Agente | File | Descrizione |
|---|---|---|
| AutoRemount | `AutoRemountService.cs` (71 righe) | Rimonta automaticamente dopo dismount. Supporta mount ethereal e pet. |
| AutoCarver | `AutoCarverService.cs` (83 righe) | Taglia automaticamente corpse vicini con blade. Range 3 tile. |
| BoneCutter | `BoneCutterService.cs` (86 righe) | Taglia automaticamente ossa (0x0ECA-0x0ED2). Range 1 tile. |

### Nuovi Pacchetti Gestiti
- 0x55 LoginComplete, 0x6C TargetCursor, 0x6D PlayMusic, 0x6E CharAnimation
- 0x83 DeleteCharacter, 0x89 CorpseEquipment, 0x8C RelayServer
- 0x95 HueResponse, 0x98 MobileName, 0xAA AttackOK
- 0xAD EncodedUnicodeSpeech, 0xBD ClientVersion, 0xC0 GraphicalEffect

### Nuovi Filtri
- Bard Music Filter, Party Invite Block, Trade Request Block, Footsteps Filter, Custom Graph Filters

### Miglioramenti Architetturali
- **Async/await** ovunque (zero `Thread.Abort()`)
- **Cancellation a 2 livelli** per scripting Python
- **Roslyn** per compilazione C# (vs CodeDom legacy)
- **Thread-safe snapshots** (MobileSnapshot, ItemSnapshot) per letture consistenti
- **Plugin-based IPC** (x86/x64 compatible) vs DLL injection (solo x86)
- **Protocol Buffers** support (ProtoControlService)
- **Fluent Design** WPF (temi Light/Dark/HighContrast)

### Nuove Pagine UI
- DashboardPage, PacketLoggerPage, GalleryPage, GumpListPage
- CountersPage, DisplayPage, SoundPage
- HuePickerWindow, OverheadMessageOverlay, TargetHPWindow, MapWindow

---

## 13. Riepilogo Task

### Task Critici (Alta Priorità)

| ID | Tipo | Descrizione | File da Modificare |
|---|---|---|---|
| TASK-004 | Bug/Incompleto | **Organizer: aggiungere supporto Amount field** — attualmente sposta tutti gli item invece del numero configurato | `OrganizerService.cs` |
| TASK-006 | Bug/Incompleto | **BandageHeal: implementare ricerca bandage per ItemID** — attualmente assume serial fisso che diventa invalido | `BandageHealService.cs` |
| TASK-012 | Mancante | **ATTACK macro: aggiungere modalità Nearest/Farthest/ByType** — combattimento avanzato non funziona | `MacrosService.cs` |

### Task Medi (Media Priorità)

| ID | Tipo | Descrizione | File da Modificare |
|---|---|---|---|
| TASK-001 | Mancante | **Commands service** — sistema comandi chat (es. `-info`, `-where`) | Nuovo `CommandService.cs` |
| TASK-006b | Incompleto | **BandageHeal: buff wait** — aspettare fine buff prima di ri-applicare | `BandageHealService.cs` |
| TASK-010 | Mancante | **MenuResponse packet** (0x7D C→S) — risposta a menu server | `PacketBuilder.cs` + `MacrosService.cs` |
| TASK-013 | Mancante | **SETABILITY macro command** — attivazione abilità speciali | `MacrosService.cs` |
| TASK-014 | Mancante | **PathFinding API scripting** — esporre PathFindingService agli script | Nuovo `PathFindingApi.cs` + `ScriptGlobals.cs` |
| TASK-017 | Mancante | **UI pagina Media** — configurazione screenshot/video | Nuovo `MediaPage.xaml` + ViewModel |

### Task Bassi (Bassa Priorità)

| ID | Tipo | Descrizione | File da Modificare |
|---|---|---|---|
| TASK-011 | Mancante | UnicodePromptSend packet (0xC2 C→S) | `PacketBuilder.cs` |
| TASK-015 | Mancante | DPSMeter API scripting | Nuovo `DPSMeterApi.cs` |
| TASK-016 | Mancante | PacketLogger API scripting | Nuovo `PacketLoggerApi.cs` |
| TASK-018 | Mancante | AutoDoc generazione documentazione | Nuovo script/servizio |
| TASK-019 | Mancante | Multi/House data service | Nuovo `MultiService.cs` |
| TASK-020 | Incompleto | PacketLogger template parsing | `PacketLoggerService.cs:179` |

---

## Statistiche Finali

| Metrica | Legacy | TMRazorImproved |
|---|---|---|
| File C# totali | ~253 | ~150+ |
| Riduzione codice agenti | — | 74-80% media |
| Packet handler S→C | 56 | 60+ (4 nuovi) |
| Packet handler C→S | 18 | 13 (5 gestiti da TmClient) |
| Filtri | 13 classi | Tutti + 5 nuovi |
| Azioni macro | 43 classi | 38 comandi (2 rimossi intenzionalmente) |
| API scripting | 25+ | 23 (3 mancanti: PathFinding, DPS, PacketLogger) |
| Motori scripting | 3 (Python, C#, UOSteam) | 3 (Python migliorato, C# Roslyn, UOSteam) |
| Agenti | 8 | 11 (3 nuovi) |
| UI dialoghi | 16 separati | Consolidati in pagine/tab |
| Task critici aperti | — | 3 |
| Task medi aperti | — | 6 |
| Task bassi aperti | — | 6 |
| **Copertura funzionale totale** | — | **~92%** |

---

# PARTE 2 — Analisi Complementare

**Data**: 18 Marzo 2026 (sessione 2)
**Autore**: Analisi automatica (revisione complementare)
**Scopo**: Completare l'analisi della Parte 1 verificando aree non coperte: UOSteam interpreter, condizioni macro, sistema contatori, GumpInspector, file legacy infrastrutturali.

---

## 14. UOSteam Interpreter — Gap Critico

### Architettura

**Legacy** (`Razor/RazorEnhanced/UOSteamEngine.cs`, 8.331 righe):
- 119 comandi registrati via `RegisterCommandHandler`
- 84 espressioni registrate via `RegisterExpressionHandler`
- Parser completo con AST, namespace, gestione scope
- Supporto completo per tutte le operazioni di gioco

**Nuovo** (`TMRazorImproved.Core/Services/Scripting/Engines/UOSteamInterpreter.cs`, 716 righe):
- ~38 comandi implementati (switch/case)
- Condizioni delegate a `ConditionEvaluator.cs`
- Delegazione a API specializzate
- Riduzione codice: **91%** (da 8.331 a 716 righe)

### TASK-021: UOSteam Interpreter — Comandi Mancanti (83+ comandi)

**Stato**: ⚠️ **Incompleto critico** — Solo il ~32% dei comandi legacy è implementato.

Questo è il **gap più significativo** dell'intera migrazione. Script UOSteam esistenti degli utenti che usano comandi non implementati **non funzioneranno affatto** nel nuovo sistema.

#### Comandi Mancanti — Priorità CRITICA (usati frequentemente)

| Comando Legacy | Categoria | Descrizione | Impatto |
|---|---|---|---|
| `fly` / `land` | Movimento | Attiva/disattiva volo gargoyle | Script di viaggio |
| `run` | Movimento | Corsa direzionale | Script di navigazione |
| `pathfindto` | Movimento | Pathfinding automatico a coordinate | Script di navigazione automatica |
| `miniheal` / `bigheal` | Healing | Cura minore/maggiore (bandage o spell) | **Script di combattimento** |
| `chivalryheal` | Healing | Cura via Close Wounds (Chivalry) | Script paladin |
| `bandageself` | Healing | Auto-bendaggio rapido | **Script di combattimento** |
| `targettype` | Targeting | Target per tipo grafico | **Script di combattimento/gathering** |
| `targetground` | Targeting | Target a terra | Script harvesting |
| `targettile` / `targettileoffset` / `targettilerelative` | Targeting | Target per tile/offset | Script harvesting/mining |
| `targetresource` | Targeting | Target risorsa specifica | Script harvesting |
| `cleartargetqueue` | Targeting | Pulisci coda target | Script combattimento |
| `autotargetobject` / `cancelautotarget` | Targeting | Auto-targeting setup | Script combattimento |
| `buy` / `sell` / `clearbuy` / `clearsell` | Vendor | Interazione NPC vendor | **Script commercio** |
| `contextmenu` / `waitforcontext` | Interazione | Menu contestuali NPC | **Script farming/craft** |
| `waitforproperties` | Item | Attesa caricamento proprietà | Script valutazione loot |
| `moveitem` / `moveitemoffset` / `movetypeoffset` | Item | Spostamento item avanzato | Script organizzazione |
| `getfriend` / `getenemy` | Combat | Selezione friend/enemy dalla lista | Script PvP |
| `ignoreobject` / `clearignorelist` | Filtro | Ignora oggetti/NPC | Script farming |

#### Comandi Mancanti — Priorità ALTA

| Comando Legacy | Categoria | Descrizione |
|---|---|---|
| `partymsg` / `guildmsg` / `allymsg` | Chat | Messaggi canale specifico |
| `whispermsg` / `yellmsg` / `emotemsg` / `chatmsg` | Chat | Messaggi con tipo |
| `promptmsg` / `timermsg` | Chat | Messaggi speciali |
| `equipitem` / `equipwand` | Equip | Equipaggiamento diretto |
| `togglemounted` / `togglehands` / `clearhands` | Equip | Toggle mount/armi |
| `clickobject` | Interazione | Click singolo (diverso da dclick) |
| `useonce` / `clearusequeue` | Item | Coda uso item |
| `addfriend` / `removefriend` | Friends | Gestione lista amici |
| `toggleautoloot` / `togglescavenger` | Agent | Toggle agenti da script |
| `dressconfig` | Dress | Configurazione lista dress |
| `promptalias` | Alias | Input interattivo alias |
| `playmacro` | Script | Esecuzione macro da script |
| `uniquejournal` | Journal | Deduplicazione journal |
| `location` | Info | Coordinate correnti per vendor |
| `feed` / `rename` | Pet | Comandi per animali domestici |
| `virtue` | Ability | Invocazione virtù |

#### Comandi Mancanti — Priorità BASSA

| Comando Legacy | Categoria | Descrizione |
|---|---|---|
| `paperdoll` / `helpbutton` / `guildbutton` / `questsbutton` / `logoutbutton` | UI | Apertura finestre UI |
| `ping` / `where` / `snapshot` | Debug | Utilità di debug |
| `messagebox` / `mapuo` / `clickscreen` | UI | Interazione UI avanzata |
| `namespace` / `script` | Meta | Gestione scope script (avanzato) |
| `hotkeys` | System | Gestione hotkey (stub nel legacy) |
| `counter` | System | Gestione contatori (stub nel legacy) |
| `autocolorpick` | UI | Color picker automatico |
| `shownames` | Display | Mostra nomi mobiles |

#### Espressioni/Condizioni Mancanti (35+)

| Espressione Legacy | Categoria | Descrizione | Priorità |
|---|---|---|---|
| `true` / `false` | Booleano | Letterali booleani | **Critica** |
| `physical` / `fire` / `cold` / `poison` / `energy` | Resistenze | Resistenze elementali | **Critica** |
| `skill` / `skillbase` / `skillvalue` / `skillstate` | Skill | Query dettagliate skill | **Critica** |
| `findtype` / `findobject` / `findlayer` | Ricerca | Ricerca oggetti avanzata | **Alta** |
| `amount` / `graphic` / `color` / `durability` | Item | Proprietà oggetti | **Alta** |
| `property` | Item | Proprietà OPL generica | **Alta** |
| `criminal` / `enemy` / `friend` / `gray` / `innocent` / `murderer` | Notoriety | Stato notorietà mobile | **Alta** |
| `flying` / `waitingfortarget` | Stato | Flag stato giocatore | Media |
| `direction` / `directionname` | Navigazione | Direzione corrente | Media |
| `diffmana` / `diffstam` | Stats | Deficit stat (max - current) | Media |
| `infriendlist` / `ingump` / `inregion` | Query | Membership checks | Media |
| `counttypeground` | Counter | Conteggio item a terra | Media |
| `usetype` / `movetype` | Item | Tipo uso/movimento | Bassa |
| `timerexists` / `listexists` | Data | Esistenza strutture dati | Bassa |
| `findwand` / `bandage` / `famous` / `karma` / `fame` | Misc | Vari check | Bassa |

**Impatto utente**: **Qualsiasi script UOSteam che usa comandi non implementati fallirà silenziosamente o con errore**. UOSteam è il linguaggio scripting più popolare tra i giocatori UO — questa è la regressione con l'impatto più ampio sull'utenza.

**Implementazione suggerita — Fase 1 (comandi critici)**:

```csharp
// In UOSteamInterpreter.cs, aggiungere nel switch dei comandi:

case "fly":
    await _miscApi.Fly();
    break;
case "land":
    await _miscApi.Land();
    break;
case "pathfindto":
    var pfArgs = ParseArgs(args);
    await _pathFindingService.NavigateTo(
        int.Parse(pfArgs[0]), int.Parse(pfArgs[1]), int.Parse(pfArgs[2]));
    break;

case "miniheal":
    await _spellsApi.Cast("Close Wounds"); // Chivalry
    // Fallback: cercara bandage se no Chivalry
    break;
case "bigheal":
    await _spellsApi.Cast("Greater Heal");
    break;
case "bandageself":
    var bandage = _itemsApi.FindByGraphic(0x0E21, _worldService.Player.Backpack);
    if (bandage != 0) { _itemsApi.UseItem(bandage); _targetApi.TargetSelf(); }
    break;

case "targettype":
    var typeArgs = ParseArgs(args);
    ushort graphic = ushort.Parse(typeArgs[0]);
    var found = _worldService.Items.FirstOrDefault(i => i.GraphicId == graphic);
    if (found != null) _targetApi.Target(found.Serial);
    break;

case "buy":
    // Delegare a VendorService.SetBuyList()
    break;
case "sell":
    // Delegare a VendorService.SetSellList()
    break;

case "contextmenu":
    var cmArgs = ParseArgs(args);
    uint cmSerial = uint.Parse(cmArgs[0]);
    int cmIndex = int.Parse(cmArgs[1]);
    _packetService.SendToServer(PacketBuilder.ContextMenuRequest(cmSerial));
    // + logica per selezionare voce dopo risposta
    break;
case "waitforcontext":
    await WaitForConditionAsync(() => _lastContextMenuSerial != 0, timeout, ct);
    break;

case "moveitem":
    var miArgs = ParseArgs(args);
    await _dragDropCoordinator.MoveAsync(
        uint.Parse(miArgs[0]),  // serial
        uint.Parse(miArgs[1]),  // destination
        miArgs.Length > 2 ? int.Parse(miArgs[2]) : -1, ct); // amount
    break;

case "ignoreobject":
    _ignoreList.Add(uint.Parse(args));
    break;
case "clearignorelist":
    _ignoreList.Clear();
    break;
```

**Implementazione suggerita — Espressioni critiche**:

```csharp
// In ConditionEvaluator.cs o UOSteamInterpreter, aggiungere:

// Letterali booleani
case "true": return true;
case "false": return false;

// Resistenze
case "physical": return GetResistance("Physical");
case "fire": return GetResistance("Fire");
case "cold": return GetResistance("Cold");
case "poison": return GetResistance("Poison");
case "energy": return GetResistance("Energy");

private int GetResistance(string type)
{
    var player = _worldService.Player;
    return type switch
    {
        "Physical" => player?.PhysicalResist ?? 0,
        "Fire" => player?.FireResist ?? 0,
        "Cold" => player?.ColdResist ?? 0,
        "Poison" => player?.PoisonResist ?? 0,
        "Energy" => player?.EnergyResist ?? 0,
        _ => 0
    };
}

// Notoriety
case "criminal": return IsMobileNotoriety(serial, Notoriety.Criminal);
case "enemy": return IsMobileNotoriety(serial, Notoriety.Enemy);
case "murderer": return IsMobileNotoriety(serial, Notoriety.Murderer);
case "innocent": return IsMobileNotoriety(serial, Notoriety.Innocent);
case "friend": return _friendsService.IsFriend(serial);
```

**Stima lavoro**: ~3-5 giorni per Fase 1 (comandi critici), ~2-3 giorni per espressioni, ~5+ giorni per comandi rimanenti.

---

## 15. Condizioni Macro — Gap Dettagliato

### TASK-022: Condizioni IF/WHILE Mancanti nel Sistema Macro

**Legacy** (`Razor/RazorEnhanced/Macros/Actions/IfAction.cs`):
- 9 categorie strutturate con enumerazioni tipizzate
- GUI-friendly: ogni condizione ha proprietà serializzabili
- `FindStoreSerial`: salva il serial trovato per uso successivo
- `FindEntityLocation`: ricerca in Backpack, Container specifico, o Ground

**Nuovo** (`TMRazorImproved.Core/Services/ConditionEvaluator.cs`):
- 9 categorie text-based (parsing da stringa)
- Supporto `NOT` prefix per negazione
- Nuove condizioni: WARMODE, FLYING, YELLOWHITS, resistenze, GOLD, LUCK, FOLLOWERS

#### Condizioni mancanti nel nuovo sistema

| Condizione Legacy | Tipo | Descrizione | Impatto |
|---|---|---|---|
| `Mounted` | PlayerStatus | Verifica se il player è montato | **Alto** — macro di combattimento usano mount/dismount |
| `IsAlive` (Ghost check) | PlayerStatus | Verifica `!World.Player.IsGhost` | **Alto** — logica diversa da `DEAD` (HP-based) |
| `RightHandEquipped` | PlayerStatus | Verifica item in mano destra | **Alto** — macro weapon swap |
| `LeftHandEquipped` | PlayerStatus | Verifica item in mano sinistra | **Alto** — macro weapon swap |
| `Find in Container` | Find | Cerca item in container specifico (non solo backpack) | **Alto** — organizer/craft macro |
| `FindStoreSerial` | Find | Salva serial item trovato per uso successivo | **Alto** — macro multi-step |
| `InRange ItemType` | InRange | Cerca item per graphic/color entro range | Media — macro loot/harvest |
| `InRange MobileType` | InRange | Cerca mobile per graphic/color entro range | Media — macro combattimento |

**File da modificare**: `TMRazorImproved.Core/Services/ConditionEvaluator.cs`

**Implementazione suggerita**:

```csharp
// Aggiungere in EvaluateCondition():

// --- PlayerStatus estensioni ---
case "MOUNTED":
    var mount = _worldService.Player?.Mount;
    return mount != null && mount != 0;

case "ALIVE":
case "ISALIVE":
    return _worldService.Player?.IsGhost == false;

case "RIGHTHANDEQUIPPED":
    return _worldService.Player?.FindItemByLayer(Layer.RightHand) != null;

case "LEFTHANDEQUIPPED":
    return _worldService.Player?.FindItemByLayer(Layer.LeftHand) != null;

// --- Find esteso con container ---
// Sintassi: FIND graphic [container_serial] [range]
case "FIND":
    var findArgs = ParseArgs(rest);
    ushort findGraphic = ushort.Parse(findArgs[0]);
    uint? containerId = findArgs.Length > 1 ? uint.Parse(findArgs[1]) : null;
    int findRange = findArgs.Length > 2 ? int.Parse(findArgs[2]) : -1;

    IEnumerable<Item> searchSpace = containerId.HasValue
        ? _worldService.Items.Where(i => i.Container == containerId.Value)
        : _worldService.Items;

    var foundItem = searchSpace.FirstOrDefault(i =>
        i.GraphicId == findGraphic &&
        (findRange < 0 || _worldService.Distance(i) <= findRange));

    if (foundItem != null && storeSerial)
        _aliases["found"] = foundItem.Serial;

    return foundItem != null;

// --- InRange per tipo ---
// Sintassi: INRANGETYPE ITEM|MOBILE graphic range [hue]
case "INRANGETYPE":
    var irtArgs = ParseArgs(rest);
    bool isMobile = irtArgs[0].Equals("MOBILE", StringComparison.OrdinalIgnoreCase);
    ushort irtGraphic = ushort.Parse(irtArgs[1]);
    int irtRange = int.Parse(irtArgs[2]);

    if (isMobile)
        return _worldService.Mobiles.Any(m =>
            m.Body == irtGraphic && _worldService.Distance(m) <= irtRange);
    else
        return _worldService.Items.Any(i =>
            i.GraphicId == irtGraphic && _worldService.Distance(i) <= irtRange);
```

---

## 16. Sistemi Infrastrutturali Non Mappati

### TASK-023: Counter API Non Esposta allo Scripting

**Stato**: `CounterService.cs` esiste come servizio con UI dedicata (`CountersPage.xaml` + `CountersViewModel.cs`), ma **non è esposto agli script** tramite `ScriptGlobals`.

**Legacy**: I contatori erano sparsi nel codice senza un servizio dedicato. L'espressione UOSteam `counttype` permetteva di contare item per graphic.

**Nuovo**:
- `CounterService.cs` implementa: `GetCount(graphic, hue)`, `RecalculateAll()`, evento `CounterChanged`
- Debounce 500ms per ricalcolo
- Scansione ricorsiva dei container
- **Ma**: nessuna classe `CounterApi.cs` in `Scripting/Api/`

**Impatto**: Script Python/C# che necessitano conteggio item non possono usare il servizio. Devono reimplementare la logica con `Items.FindByGraphic()`.

**File da creare**: `TMRazorImproved.Core/Services/Scripting/Api/CounterApi.cs`
**File da modificare**: `TMRazorImproved.Core/Services/Scripting/ScriptGlobals.cs`

**Implementazione suggerita**:

```csharp
// CounterApi.cs
public class CounterApi
{
    private readonly ICounterService _counterService;

    public CounterApi(ICounterService counterService)
        => _counterService = counterService;

    /// <summary>Conta item per graphic nel backpack (ricorsivo).</summary>
    public int GetCount(ushort graphic, ushort hue = 0)
        => _counterService.GetCount(graphic, hue);

    /// <summary>Forza ricalcolo di tutti i contatori.</summary>
    public void Recalculate()
        => _counterService.RecalculateAll();
}

// In ScriptGlobals.cs, aggiungere:
public CounterApi Counter { get; }
// Nel costruttore:
Counter = new CounterApi(serviceProvider.GetRequiredService<ICounterService>());
```

---

### TASK-024: GumpInspector — Response Logging Mancante

**Legacy** (`Razor/RazorEnhanced/GumpInspector.cs`, 96 righe):
- `GumpResponseAddLogMain(serial, id, buttonId)` — logga ogni risposta gump
- `GumpResponseAddLogSwitchID(switchId)` — logga switch ID selezionati
- `GumpResponseAddLogTextID(textId, text)` — logga testo inserito nei gump
- `GumpResponseAddLogEnd()` — chiude il log
- Log visibile in real-time nel `EnhancedGumpInspectorListBox`

**Nuovo** (`TMRazorImproved.UI/ViewModels/InspectorViewModel.cs`):
- Mostra struttura gump (controlli, layout, proprietà)
- `ExecuteGumpAction()` invia risposta gump
- **Genera snippet** Python/C# per replicare l'azione (miglioramento!)
- ❌ **Nessun log di risposta**: quando l'utente interagisce con un gump in-game, non viene registrata la sequenza di azioni

**Impatto**: L'utente non può "registrare" le proprie interazioni con i gump per poi automatizzarle. È una funzionalità chiave per lo sviluppo di script.

**File da modificare**: `TMRazorImproved.UI/ViewModels/InspectorViewModel.cs`
**Alternativa**: Aggiungere un viewer in `WorldPacketHandler` per il pacchetto 0xB1 (GumpResponse C→S) che pubblica un messaggio `GumpResponseLogMessage` verso l'InspectorViewModel.

**Implementazione suggerita**:

```csharp
// In WorldPacketHandler.cs, aggiungere viewer per GumpResponse (0xB1):
private void HandleGumpResponseLog(byte[] data, int length)
{
    var reader = new UOBufferReader(data);
    uint serial = reader.ReadUInt32();
    uint gumpId = reader.ReadUInt32();
    int buttonId = reader.ReadInt32();
    // ... parse switchIDs, textEntries ...

    _messenger.Send(new GumpResponseLogMessage(serial, gumpId, buttonId, switchIds, texts));
}

// In InspectorViewModel.cs, ricevere:
public void Receive(GumpResponseLogMessage msg)
{
    ResponseLogs.Add($"Gump 0x{msg.GumpId:X8} → Button {msg.ButtonId}");
    foreach (var sw in msg.SwitchIds)
        ResponseLogs.Add($"  Switch: {sw}");
    foreach (var (id, text) in msg.TextEntries)
        ResponseLogs.Add($"  Text[{id}]: {text}");
}
```

---

### TASK-025: SoundApi — Metodi Placeholder

**File**: `TMRazorImproved.Core/Services/Scripting/Api/SoundApi.cs`, righe 125-129

```csharp
/// <summary>Placeholder per compatibilità legacy.</summary>
public virtual int GetMinDuration() => 0;

/// <summary>Placeholder per compatibilità legacy.</summary>
public virtual int GetMaxDuration() => 0;
```

**Legacy**: `Sound.cs` forniva durata min/max dei suoni UO per sincronizzare script con effetti sonori.

**Nuovo**: Ritorna sempre `0` — script che attendono la fine di un suono non funzioneranno correttamente.

**Impatto**: Basso — pochissimi script usano questa feature. Ma viola il principio di compatibilità API.

**Implementazione suggerita**: Se i dati durata suono non sono disponibili dal client, documentare che i metodi ritornano 0 e suggerire `Misc.Pause()` come alternativa.

---

### TASK-026: LegacyMacroMigrator — Conversione Lossy

**File**: `TMRazorImproved.Core/Utilities/LegacyMacroMigrator.cs`

Il migratore converte macro legacy nel formato testo del nuovo sistema. Tuttavia, la conversione di condizioni IF complesse è **potenzialmente lossy**:

- Condizioni `Find` con `Container` specifico → perde il filtro container
- Condizioni `Find` con `FindStoreSerial` → perde il salvataggio serial
- Condizioni `InRange` con `ItemType`/`MobileType` → convertite in semplice distance check

**Impatto**: Macro legacy importate potrebbero comportarsi diversamente senza warning all'utente.

**Implementazione suggerita**: Aggiungere commenti di warning nella macro migrata:

```csharp
// Nel LegacyMacroMigrator, quando si incontra una condizione non completamente convertibile:
if (legacyCondition.HasContainerSearch || legacyCondition.HasStoreSerial)
{
    migratedSteps.Add(new MacroStep
    {
        Command = $"// WARNING: Condizione originale '{legacyCondition}' semplificata durante migrazione",
        IsEnabled = true
    });
}
```

---

## 17. File Legacy Infrastrutturali — Mappatura

I seguenti file legacy non erano coperti nella Parte 1 ma hanno equivalenti nel nuovo codice:

### File Core Infrastrutturali

| File Legacy | Descrizione | Equivalente Nuovo | Stato |
|---|---|---|---|
| `Core/DLLImport.cs` | P/Invoke Win32 (PostMessage, CreateRemoteThread, etc.) | `ClientInteropService.cs` + P/Invoke inline | ✅ Integrato |
| `Core/NativeMethods.cs` | P/Invoke aggiuntivi (Zlib, SetWindowTheme) | Zlib via NuGet, tema WPF nativo | ✅ Sostituito |
| `Core/Facet.cs` | Mappatura facet/map con cache | Implicito in `StaticsApi.GetUltimaMap()` + `MiscApi.MapInfo` | ⚠️ Basico |
| `Core/TypeID.cs` | Wrapper ushort per ItemID | `ushort` diretto (senza wrapper) | ✅ Semplificato |
| `Core/UOEntity.cs` | Classe base entity (Serial, Position, Hue) | `Shared/Models/UOEntity.cs` | ✅ Completo |
| `Core/Utility.cs` | Random, InRange, Distance, FormatBuffer | Sparsi in `MiscApi`, `MobilesApi`, `PlayerApi` | ⚠️ Frammentato |
| `Core/ZLib.cs` | Compressione Zlib custom | NuGet package `System.IO.Compression` | ✅ Sostituito |
| `Core/SkillIcon.cs` | Enum icone skill/spell | In `SpellDefinitions.cs` / risorse XAML | ✅ Integrato |
| `Core/Main.cs` | Engine startup, crash report, client version | `App.xaml.cs` + `ClientInteropService` | ✅ Distribuito |

### File RazorEnhanced Infrastrutturali

| File Legacy | Descrizione | Equivalente Nuovo | Stato |
|---|---|---|---|
| `RazorEnhanced/EnhancedEntity.cs` | Wrapper entity per scripting | `Wrappers.cs` in Api/ | ✅ Completo |
| `RazorEnhanced/EnhancedScript.cs` | Container esecuzione script | `ScriptingService.cs` | ✅ Completo |
| `RazorEnhanced/Mobile.cs` | Wrapper scripting per Mobile | `MobilesApi.cs` + `Wrappers.cs` | ✅ Completo |
| `RazorEnhanced/Property.cs` | Wrapper OPL property | Inline nelle API | ✅ Semplificato |
| `RazorEnhanced/Gumps.cs` | Storage e interazione gump | `GumpsApi.cs` | ✅ Completo |
| `RazorEnhanced/GumpInspector.cs` | Log risposte gump | ❌ Mancante (vedi TASK-024) | ⚠️ Mancante |
| `RazorEnhanced/Target.cs` | API targeting per script | `TargetApi.cs` | ✅ Completo |
| `RazorEnhanced/Filters.cs` | Filtri avanzati gioco | `FilterHandler.cs` | ✅ Completo |
| `RazorEnhanced/CSharpEngine.cs` | Compilatore C# (CodeDom → Roslyn) | `CSharpScriptEngine.cs` | ✅ Modernizzato |
| `RazorEnhanced/PythonEngine.cs` | Runtime IronPython | `ScriptingService.cs` (IronPython 3.4) | ✅ Completo |
| `RazorEnhanced/CUO.cs` | Reflection-based ClassicUO API | Non necessario (plugin nativo) | ✅ Rimosso by design |
| `RazorEnhanced/Statics.cs` | API dati mappa/tile | `StaticsApi.cs` + `UltimaMapDataProvider.cs` | ✅ Completo |
| `RazorEnhanced/ScriptRecorder.cs` | Registrazione azioni → script | `ScriptRecorderService.cs` | ✅ Completo |

### File Network

| File Legacy | Descrizione | Equivalente Nuovo | Stato |
|---|---|---|---|
| `Network/Packets.cs` | 90+ definizioni packet class | `PacketBuilder.cs` (costruzione) + `UOBufferReader.cs` (parsing) | ✅ Refactored |

### File UI Helper

| File Legacy | Descrizione | Equivalente Nuovo | Stato |
|---|---|---|---|
| `UI/Ext.cs` | `SafeAction()` thread-safe invoke | `Application.Current.Dispatcher` WPF nativo | ✅ Sostituito |
| `UI/HueEntry.cs` | Dialog selettore colore | `HuePickerWindow.xaml` | ✅ Migliorato |
| `UI/Languages.cs` | Enum `LocString` (200+ chiavi) | `Strings.resx` + `Strings.it.resx` | ✅ Modernizzato |
| `UI/SplashScreen.cs` | Splash screen startup | Non presente | ❌ Vedi nota |
| `UI/Platform.cs` | P/Invoke per cattura schermo | `ScreenCaptureService.cs` | ✅ Integrato |
| `UI/Controls/RazorButton.cs` | Bottone custom themed | `Wpf.Ui` controls nativi | ✅ Sostituito |
| `UI/Controls/RazorTheme.cs` | Theme manager globale | `Wpf.Ui` FluentTheme | ✅ Sostituito |
| `RazorEnhanced/UI/RazorFastColoredTextBox.cs` | Syntax highlighting editor | AvalonEdit in `ScriptingPage.xaml` | ✅ Modernizzato |
| `RazorEnhanced/UI/ScriptListView.cs` | ListView ottimizzato + HotkeyTextBox | WPF `ListView` + custom KeyBinding | ✅ Sostituito |

**Nota su SplashScreen**: Non presente nel nuovo codice. Non è una regressione funzionale — è una scelta estetica. Il tempo di avvio con .NET 10 è sufficientemente rapido da non necessitare uno splash screen.

---

## 18. Riepilogo Task Aggiornato (Parte 1 + Parte 2)

### Task Critici (Alta Priorità)

| ID | Tipo | Descrizione | File da Modificare | Fonte |
|---|---|---|---|---|
| TASK-004 | Bug/Incompleto | **Organizer: supporto Amount field** | `OrganizerService.cs` | Parte 1 |
| TASK-006 | Bug/Incompleto | **BandageHeal: ricerca bandage per ItemID** | `BandageHealService.cs` | Parte 1 |
| TASK-012 | Mancante | **ATTACK macro: modalità Nearest/Farthest/ByType** | `MacrosService.cs` | Parte 1 |
| **TASK-021** | **Mancante** | **UOSteam Interpreter: 83+ comandi mancanti** — Solo 32% implementato. Fase 1: healing, targeting, vendor, movement, pathfinding | `UOSteamInterpreter.cs` + `ConditionEvaluator.cs` | **Parte 2** |
| **TASK-022** | **Mancante** | **Condizioni macro: Mounted, HandEquipped, Find in Container, FindStoreSerial, InRange by Type** — 8 condizioni mancanti | `ConditionEvaluator.cs` | **Parte 2** |

### Task Medi (Media Priorità)

| ID | Tipo | Descrizione | File da Modificare | Fonte |
|---|---|---|---|---|
| TASK-001 | Mancante | Commands service | Nuovo `CommandService.cs` | Parte 1 |
| TASK-006b | Incompleto | BandageHeal: buff wait | `BandageHealService.cs` | Parte 1 |
| TASK-010 | Mancante | MenuResponse packet (0x7D C→S) | `PacketBuilder.cs` + `MacrosService.cs` | Parte 1 |
| TASK-013 | Mancante | SETABILITY macro command | `MacrosService.cs` | Parte 1 |
| TASK-014 | Mancante | PathFinding API scripting | Nuovo `PathFindingApi.cs` | Parte 1 |
| TASK-017 | Mancante | UI pagina Media | Nuovo `MediaPage.xaml` | Parte 1 |
| **TASK-023** | **Mancante** | **Counter API non esposta a scripting** | Nuovo `CounterApi.cs` + `ScriptGlobals.cs` | **Parte 2** |
| **TASK-024** | **Mancante** | **GumpInspector response logging** — registrazione interazioni gump per automazione | `InspectorViewModel.cs` + `WorldPacketHandler.cs` | **Parte 2** |
| **TASK-026** | **Incompleto** | **LegacyMacroMigrator conversione lossy** — condizioni complesse semplificate senza warning | `LegacyMacroMigrator.cs` | **Parte 2** |

### Task Bassi (Bassa Priorità)

| ID | Tipo | Descrizione | File da Modificare | Fonte |
|---|---|---|---|---|
| TASK-011 | Mancante | UnicodePromptSend packet (0xC2 C→S) | `PacketBuilder.cs` | Parte 1 |
| TASK-015 | Mancante | DPSMeter API scripting | Nuovo `DPSMeterApi.cs` | Parte 1 |
| TASK-016 | Mancante | PacketLogger API scripting | Nuovo `PacketLoggerApi.cs` | Parte 1 |
| TASK-018 | Mancante | AutoDoc generazione documentazione | Nuovo script/servizio | Parte 1 |
| TASK-019 | Mancante | Multi/House data service | Nuovo `MultiService.cs` | Parte 1 |
| TASK-020 | Incompleto | PacketLogger template parsing | `PacketLoggerService.cs:179` | Parte 1 |
| **TASK-025** | **Incompleto** | **SoundApi placeholder** — `GetMinDuration()`/`GetMaxDuration()` return 0 | `SoundApi.cs:125-129` | **Parte 2** |

---

## 19. Statistiche Finali Aggiornate

| Metrica | Legacy | TMRazorImproved | Note Parte 2 |
|---|---|---|---|
| File C# totali | ~253 | ~150+ | |
| Riduzione codice agenti | — | 74-80% media | |
| Packet handler S→C | 56 | 60+ (4 nuovi) | |
| Packet handler C→S | 18 | 13 (5 gestiti da TmClient) | |
| Filtri | 13 classi | Tutti + 5 nuovi | |
| Azioni macro | 43 classi | 38 comandi (2 rimossi) | |
| Condizioni macro IF | 9 categorie, ~15 tipi | 9 categorie, **8 tipi mancanti** | **Aggiornato** |
| API scripting | 25+ | 23 (3 mancanti + Counter) | **+1 mancante** |
| **UOSteam comandi** | **119** | **~38 (~32%)** | **⚠️ Nuovo: 83 mancanti** |
| **UOSteam espressioni** | **84** | **~30 (~36%)** | **⚠️ Nuovo: 54 mancanti** |
| Motori scripting | 3 | 3 | |
| Agenti | 8 | 11 (3 nuovi) | |
| UI dialoghi | 16 separati | Consolidati in pagine/tab | |
| **Task critici aperti** | — | **5** (+2) | **Aggiornato** |
| **Task medi aperti** | — | **9** (+3) | **Aggiornato** |
| **Task bassi aperti** | — | **7** (+1) | **Aggiornato** |
| File legacy non mappati (Parte 1) | — | — | 39 file ora mappati |
| **Copertura funzionale totale** | — | **~85%** | **Rivista al ribasso per gap UOSteam** |

### Nota sulla Copertura Rivista

La copertura funzionale è stata rivista dal ~92% della Parte 1 al **~85%** perché la Parte 1 non aveva analizzato in dettaglio l'interprete UOSteam. Con solo il 32% dei comandi UOSteam implementati, e considerando che UOSteam è il linguaggio scripting più usato dalla community, il gap è significativo. La copertura dei servizi core, agenti, filtri, e packet handler rimane eccellente (95%+). Il deficit è concentrato in:

1. **UOSteam interpreter** (32% comandi, 36% espressioni)
2. **Condizioni macro** (8 tipi mancanti su ~23)
3. **API scripting minori** (Counter, GumpInspector logging)

---

# PARTE 3 — Analisi Differenziale: Compatibilità Multi-Shard e Gap Architetturali

**Data**: 18 Marzo 2026 (sessione 3)
**Autore**: Analisi automatica (revisione architetturale senior)
**Scopo**: Completare l'analisi affrontando il **difetto fondamentale** delle Parti 1 e 2: l'assunzione che TMRazorImproved debba supportare **solo TmClient**. Il requisito reale è la **piena compatibilità con l'architettura originale**, gestendo sia il flusso Multi-Shard (legacy) sia il flusso TM Client tramite **Riconoscimento Intelligente** (`if TMClient then X, else Y`).

---

## 20. Gap Fondamentale: Mancanza di Riconoscimento Intelligente del Client

### Il Problema

Le Parti 1 e 2 hanno analizzato la migrazione assumendo un singolo target: **TmClient.exe** (ClassicUO-based). Questa assunzione è **errata**. Il sistema legacy TMRazor supporta:

1. **Client OSI** (`client.exe`) — Il client ufficiale Ultima Online, x86, con encryption OSI
2. **ClassicUO** (`ClassicUO.exe`) — Client open-source, x86/x64, senza encryption
3. **TmClient** (`TmClient.exe`) — Fork di ClassicUO personalizzato per "The Miracle" shard
4. **Qualsiasi free shard** — Con host/porta/encryption custom configurabili

### Architettura Legacy Multi-Shard (Shards.cs)

Il legacy implementa un **gestore shard completo** con:

```
Shard {
    Description     // "OSI Ultima Online", "UO Eventine", etc.
    ClientPath      // Path al client OSI (client.exe)
    CUOClient       // Path al client ClassicUO (ClassicUO.exe)
    ClientFolder    // Cartella dati UO (per file .mul/.uop)
    Host            // Server address (es. "login.ultimaonline.com")
    Port            // Server port (es. 7776)
    PatchEnc        // Patch encryption (bool)
    OSIEnc          // Use OSI encryption (bool)
    StartType       // OSI o CUO (enum)
}
```

**Default shards**: OSI Ultima Online (login.ultimaonline.com:7776) e UO Eventine (shard.uoeventine.com:2593).

### Cosa Manca in TMRazorImproved

**TASK-027: Sistema Gestione Shard Completo**

| Aspetto | Legacy (Shards.cs) | Nuovo (ConfigModels.cs) | Gap |
|---|---|---|---|
| Lista shard multipli | ✅ `Dictionary<string, Shard>` con CRUD completo | ❌ Solo `ShardId`/`ShardName` passivo (letto dal pacchetto 0xA8) | **Critico** |
| Selezione shard attivo | ✅ `Selected` flag, UI launcher dedicata | ❌ Nessun UI di selezione shard | **Critico** |
| Tipo di client (OSI/CUO) | ✅ `StartType` enum con logica biforcata | ❌ Singolo `ClientPath` per TmClient | **Critico** |
| Host/Port configurabili | ✅ Per-shard, usati nel login packet routing | ❌ Non configurabili dall'utente | **Critico** |
| Encryption flags | ✅ `PatchEnc` + `OSIEnc` per-shard | ⚠️ Solo `PatchEncryption` globale, no `OSIEncryption` | **Alto** |
| ClientFolder (dati UO) | ✅ Per-shard (shards diversi usano dati diversi) | ⚠️ Solo `DataPath` globale | **Medio** |
| Launcher UI dedicata | ✅ `EnhancedLauncher.cs` con tabella shard | ❌ Solo "Launch Client" button | **Alto** |
| Persistenza | ✅ `RazorEnhanced.shards` (JSON) | ❌ Non esiste file shard | **Critico** |
| CRUD shard | ✅ Insert/Update/Delete/Read/Save | ❌ Nessuna operazione shard | **Critico** |

**Impatto**: Un utente che gioca su **più shard** (es. OSI + The Miracle + un free shard) **non può** configurare TMRazorImproved per passare dall'uno all'altro. Deve modificare manualmente il `ClientPath` e rilanciare.

---

### TASK-028: Riconoscimento Intelligente del Client (If-Then-Else Architecture)

Il nuovo codice deve implementare un pattern di **riconoscimento automatico del tipo di client** e biforcazione del flusso:

```
                     ┌─────────────────────┐
                     │  User selects Shard  │
                     └─────────┬───────────┘
                               │
                     ┌─────────▼───────────┐
                     │  Detect Client Type  │
                     │  (OSI / CUO / TM)   │
                     └────┬──────┬────┬────┘
                          │      │    │
           ┌──────────────▼──┐   │  ┌─▼──────────────┐
           │  OSI Client     │   │  │  TmClient       │
           │  (DLL Inject    │   │  │  (Plugin-based   │
           │   via Crypt.dll)│   │  │   shared memory) │
           └────────┬────────┘   │  └────────┬────────┘
                    │      ┌─────▼────┐      │
                    │      │  CUO     │      │
                    │      │  (Both   │      │
                    │      │  paths)  │      │
                    │      └────┬─────┘      │
                    │           │             │
           ┌────────▼───────────▼─────────────▼────┐
           │  IPacketService (unified interface)    │
           │  Stessa API per tutti i tipi client    │
           └───────────────────────────────────────┘
```

**File da creare/modificare**:

| File | Azione | Descrizione |
|---|---|---|
| `Shared/Models/Config/ShardConfig.cs` | **Nuovo** | Modello `ShardEntry` con tutti i campi del legacy |
| `Shared/Interfaces/IShardService.cs` | **Nuovo** | Interfaccia CRUD shard |
| `Core/Services/ShardService.cs` | **Nuovo** | Implementazione con persistenza JSON |
| `Core/Services/ClientInteropService.cs` | **Modifica** | Aggiungere `ClientType` detection e biforcazione |
| `UI/ViewModels/GeneralViewModel.cs` | **Modifica** | Aggiungere selezione shard, UI launcher |
| `UI/Views/Pages/GeneralPage.xaml` | **Modifica** | Aggiungere DataGrid shard + pulsanti CRUD |

**Implementazione suggerita — ShardConfig**:

```csharp
public class ShardEntry
{
    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public uint Port { get; set; } = 2593;
    public string ClientPath { get; set; } = string.Empty;      // OSI client
    public string CUOClientPath { get; set; } = string.Empty;   // ClassicUO client
    public string DataFolder { get; set; } = string.Empty;      // UO data files
    public bool PatchEncryption { get; set; } = true;
    public bool OSIEncryption { get; set; }
    public ClientType StartType { get; set; } = ClientType.ClassicUO;
    public bool IsSelected { get; set; }
}

public enum ClientType
{
    OSI,        // client.exe — DLL injection via Crypt.dll
    ClassicUO,  // ClassicUO.exe — Plugin + DLL injection hybrid
    TmClient    // TmClient.exe — Plugin-only (shared memory)
}
```

**Implementazione suggerita — Riconoscimento Intelligente in LaunchClient()**:

```csharp
private async Task LaunchClient()
{
    var shard = _shardService.GetSelectedShard();
    if (shard == null) { StatusMessage = "Seleziona uno shard."; return; }

    // Riconoscimento intelligente del tipo client
    ClientType clientType = shard.StartType;

    // Auto-detection se non specificato
    if (clientType == ClientType.ClassicUO)
    {
        string exeName = Path.GetFileName(shard.CUOClientPath ?? shard.ClientPath).ToLower();
        if (exeName.Contains("tmclient"))
            clientType = ClientType.TmClient;
        else if (exeName == "client.exe")
            clientType = ClientType.OSI;
    }

    switch (clientType)
    {
        case ClientType.TmClient:
            // Flusso attuale: Deploy Plugin + Process.Start + Shared Memory
            DeployPlugin(clientDir);
            LaunchViaTmClient(shard);
            break;

        case ClientType.ClassicUO:
            // CUO standard: Plugin + Crypt.dll injection
            DeployPlugin(clientDir);
            LaunchViaClassicUO(shard);
            InjectCryptDll(gamePid, shard);
            break;

        case ClientType.OSI:
            // OSI legacy: Loader.dll + Crypt.dll injection (x86 only)
            LaunchViaLoader(shard);
            break;
    }
}
```

**Stima lavoro**: ~5-8 giorni per implementazione completa (ShardService + UI + biforcazione launch).

---

## 21. Adapter Stub — Architettura Incompleta

### TASK-029: ClassicUOAdapter e OsiClientAdapter sono Stub Inutilizzati

L'interfaccia `IClientAdapter` è stata predisposta con buone intenzioni ma **non è collegata a niente**:

**ClassicUOAdapter.cs** (31 righe — tutti placeholder):
```csharp
public bool Connect(int processId) { return true; }       // Placeholder
public void Disconnect() { }                                // Placeholder
public byte[] ReceivePacket(PacketPath direction) { return Array.Empty<byte>(); } // Placeholder
public void SendPacket(byte[] data, PacketPath direction) { } // Placeholder
```

**OsiClientAdapter.cs** (39 righe — tutti placeholder):
```csharp
public bool Connect(int processId) { return true; }       // Placeholder
public void Disconnect() { }                                // Placeholder
public byte[] ReceivePacket(PacketPath direction) { return Array.Empty<byte>(); } // Placeholder
public void SendPacket(byte[] data, PacketPath direction) { } // Placeholder
```

**Problema**: L'interfaccia `IClientAdapter` non è registrata nel DI container, non è iniettata in nessun servizio, e non partecipa al flusso dati. `PacketService` usa direttamente `ClientInteropService` e la shared memory, bypassando completamente il pattern adapter.

**Impatto**: Senza adapter funzionanti, **non c'è modo** di supportare client diversi da TmClient. L'`OsiClientAdapter` dovrebbe implementare il flusso via `Crypt.dll` (DLL injection + shared memory legacy) e `ClassicUOAdapter` dovrebbe implementare il flusso plugin standard.

**Implementazione suggerita**: Gli adapter devono diventare il **punto di entry per PacketService**, non essere bypassed:

```csharp
// In PacketService, sostituire l'accesso diretto alla shared memory con:
private readonly IClientAdapter _adapter;

public PacketService(IClientAdapter adapter, ...)
{
    _adapter = adapter;
}

// HandleComm legge da _adapter.ReceivePacket() invece che dalla shared memory diretta
// Ogni adapter implementa la propria logica di ricezione:
// - ClassicUOAdapter: plugin shared memory (flusso attuale)
// - OsiClientAdapter: Crypt.dll shared memory (flusso legacy)
```

---

## 22. API Scripting — Stubs Non Funzionali Dettagliati

### TASK-030: MiscApi — Menu System Completamente Stub

La sezione "Old Menu stubs" in `MiscApi.cs` (righe 754-762) contiene **6 metodi non funzionali**:

| Metodo | Legacy Funzionante? | Nuovo | Impatto |
|---|---|---|---|
| `HasMenu()` | ✅ Verifica se c'è un menu server aperto | ❌ `return false` sempre | Script che interagiscono con menu NPC |
| `CloseMenu()` | ✅ Chiude il menu | ❌ No-op | Idem |
| `MenuContain(text)` | ✅ Cerca testo nel menu | ❌ `return false` | Idem |
| `GetMenuTitle()` | ✅ Ritorna titolo | ❌ `return ""` | Idem |
| `WaitForMenu(delay)` | ✅ Attende arrivo menu | ❌ `return false` | **Critico per script di crafting** |
| `MenuResponse(text)` | ✅ Invia risposta | ❌ No-op | **Critico per script di crafting** |

**Nota**: Questo è collegato a TASK-010 (MenuResponse packet 0x7D). Il pacchetto 0x7C (SendMenu S→C) è gestito nel `WorldPacketHandler`, ma la **risposta e la coda dei menu** non sono implementate. Script che interagiscono con NPC via menu legacy (non context-menu) non funzionano.

### TASK-031: MiscApi — Query String System Stub

La sezione "Query String stubs" (righe 764-770):

| Metodo | Funzionante? | Impatto |
|---|---|---|
| `HasQueryString()` | ❌ `return false` | Script che rispondono a prompt server |
| `WaitForQueryString(delay)` | ❌ `return false` | Idem |
| `QueryStringResponse(ok, text)` | ❌ No-op | Idem |

**Collegato a**: TASK-011 (UnicodePromptSend). Il pacchetto 0x9A/0xC2 (Prompt S→C) è gestito, ma la coda prompt e la risposta scripting no.

### TASK-032: MiscApi — Map Info Stub

`GetMapInfo(serial)` (riga 861) ritorna solo il serial senza dati mappa effettivi:
```csharp
return new MapInfo { Serial = serial }; // Nessun dato mappa reale
```

**Legacy**: Ritornava coordinate, facet, bounds dal data file `.mul`.

### TASK-033: MiscApi — Metodi Non Implementati

| Metodo | Riga | Comportamento | Legacy |
|---|---|---|---|
| `ExportPythonAPI()` | 1109 | Stampa "not implemented" | ✅ Generava documentazione API |
| `GetContPosition()` | 1115 | Ritorna `(0,0)` con messaggio "not implemented" | ✅ Posizione container aperti |
| `LastHotKey()` | 1128 | `return null` | ✅ Ultimo hotkey premuto |

### TASK-034: FriendApi — ChangeList() Stub

`FriendApi.ChangeList(friendlist)` (riga 66) è un **no-op stub**:
```csharp
public virtual void ChangeList(string friendlist)
{
    _cancel.ThrowIfCancelled();
    // Stub per compatibilità
}
```

**Legacy**: Permetteva di switchare tra liste amici diverse. Il servizio `FriendsService` supporta già le multi-liste (config-driven), ma l'API scripting **non espone** la funzionalità di switch.

### TASK-035: StaticsApi — CheckDeedHouse() Non Implementato

`StaticsApi.CheckDeedHouse(x, y)` (riga 194) ritorna sempre `false`:
```csharp
// Senza accesso al World di TMRazor, per ora ritorno false
return false;
```

**Legacy**: Verificava se una posizione è occupata da una casa piazzata. Script di piazzamento/vendita case dipendono da questo metodo.

---

## 23. Sistema Comandi Chat — TASK-001 Dettagliato

### TASK-001 (espanso): Command System Completamente Assente

Il legacy registra **18 comandi chat** che l'utente invoca digitando `-comando` nella chat del gioco:

| Comando | Funzione | Priorità |
|---|---|---|
| `-where` | Mostra coordinate correnti (X, Y, Z, Map, Region) | **Alta** |
| `-ping` | Mostra latenza verso il server | **Alta** |
| `-getserial` | Attiva target → mostra serial dell'oggetto cliccato | **Alta** |
| `-inspect` | Attiva target → mostra tutte le proprietà dell'oggetto | **Alta** |
| `-inspectgumps` | Lista tutti i gump aperti con ID e struttura | Media |
| `-inspectalias` | Lista alias definiti (per debug script) | Media |
| `-sync` / `-resync` | Forza resync col server | **Alta** |
| `-echo` | Stampa testo (debug) | Bassa |
| `-help` / `-listcommand` | Lista comandi disponibili | Media |
| `-playscript [nome]` | Avvia uno script per nome | Media |
| `-setalias [nome] [serial]` | Imposta alias per serial | Media |
| `-unsetalias [nome]` | Rimuove alias | Media |
| `-hideitem` / `-hide` | Target → nasconde item dal rendering | Bassa |
| `-drop` | Target → drop dell'item nella posizione del player | Bassa |
| `-reducecpu` / `-renice` | Riduce carico CPU (sleep nel main loop) | Bassa |
| `-pping` | Ping con statistiche pacchetto | Bassa |

**Nel nuovo codice**: Nessun sistema di intercettazione comandi chat. Il `WorldPacketHandler` processa i pacchetti speech (0x03/0xAD C→S) ma **non controlla se il testo inizia con `-`** per intercettare comandi.

**Implementazione suggerita**:

```csharp
// Nuovo file: Core/Services/CommandService.cs
public class CommandService : IRecipient<OutgoingSpeechMessage>
{
    private readonly Dictionary<string, Func<string[], Task>> _commands = new();

    public CommandService(IMessenger messenger, ...)
    {
        messenger.Register(this);
        RegisterCommand("where", WhereCommand);
        RegisterCommand("ping", PingCommand);
        RegisterCommand("getserial", GetSerialCommand);
        RegisterCommand("inspect", InspectCommand);
        RegisterCommand("sync", SyncCommand);
        // ... etc.
    }

    public void Receive(OutgoingSpeechMessage msg)
    {
        if (msg.Text.StartsWith("-"))
        {
            string[] parts = msg.Text[1..].Split(' ');
            if (_commands.TryGetValue(parts[0].ToLower(), out var handler))
            {
                msg.Cancel = true; // Non inviare al server
                _ = handler(parts[1..]);
            }
        }
    }
}
```

**Nota**: Richiede che il `WorldPacketHandler` pubblichi un `OutgoingSpeechMessage` **prima** di inviare il pacchetto al server, con possibilità di cancellazione.

---

## 24. Encryption e Login — Gap per Supporto Multi-Shard

### TASK-036: OSI Encryption Non Supportata

Il legacy gestisce **due tipi di encryption** via i flag `PatchEnc` e `OSIEnc`:

- **PatchEnc**: Patcha l'encryption del client (Crypt.dll modifica il client in memoria per disabilitare la cifratura). Funziona con tutti i free shard.
- **OSIEnc**: Mantiene l'encryption OSI attiva. Necessario **solo** per i server ufficiali EA/Broadsword.

Il nuovo codice ha **solo** `PatchEncryption` (globale, singolo flag). Non c'è:
- `OSIEncryption` flag
- Logica per-shard dei flag di encryption
- Gestione dell'encryption handshake per server OSI

**Impatto**: Impossibile connettersi ai **server ufficiali OSI** che richiedono encryption attiva.

### TASK-037: Login Relay Non Gestito Attivamente

Il legacy gestisce il flusso di login multi-step:
1. Client → LoginServer (0x80)
2. LoginServer → ServerList (0xA8) — lista dei server/game-server
3. Client sceglie server (0xA0)
4. LoginServer → RelayServer (0x8C) — IP/porta del game-server
5. Client → GameServer (0x91)
6. GameServer → CharacterList (0xA9)

Il nuovo codice **osserva** 0xA8 (legge il nome del server) e 0x8C (legge IP:porta per `SetCurrentShard`), ma **non partecipa attivamente** al flusso. Questo funziona con TmClient perché il client gestisce tutto internamente. Ma per un client OSI dove l'assistant deve **riscrivere** l'IP di relay (redirect a localhost per l'intercettazione), questo flusso non funziona.

---

## 25. Flusso di Lancio — Analisi Completa

### TASK-038: Launch Flow Supporta Solo TmClient

Il `GeneralViewModel.LaunchClient()` implementa **un solo flusso**:

1. `DeployPlugin()` — Copia TMRazorPlugin.dll nella cartella plugin di TmClient
2. `Process.Start(ClientPath)` — Avvia il client
3. `WaitForWindow()` → `InstallLibrary()` — Inietta Crypt.dll (per SetWindowsHookEx)
4. `NotifyCryptReady()` — Abilita PacketService
5. `InjectUoMod()` — Patches opzionali

**Problemi per client non-TmClient**:

| Step | TmClient | ClassicUO Standard | Client OSI |
|---|---|---|---|
| DeployPlugin | ✅ Plugin caricato nativamente | ⚠️ Funziona se la cartella plugin è corretta | ❌ OSI non ha plugin API |
| Process.Start | ✅ | ✅ | ❌ OSI richiede Loader.dll per iniettare Crypt.dll al boot |
| InstallLibrary | ⚠️ SetWindowsHookEx fallisce su x64, ma shared memory funziona | ✅ Funziona su x86 CUO | ✅ Funziona su x86 client.exe |
| Plugin shared memory | ✅ Plugin crea shared memory | ❌ Plugin potrebbe non avere `Engine.cs` compatibile | ❌ No plugin, shared memory creata da Crypt.dll |

**Nota critica**: `LaunchClient()` chiama `_clientInterop.LaunchClient(exePath, dllPath)` che usa `Loader.dll` **solo** nel codice di `ClientInteropService`, ma il ViewModel usa `Process.Start` direttamente. Il metodo `LaunchClient` di `ClientInteropService` (che usa `Loader.dll`) **non è mai chiamato dal ViewModel**.

---

## 26. File Legacy Non Coperti nelle Parti 1-2

### Macro Actions — File Individuali Non Mappati

Il legacy ha **47 file** in `Razor/RazorEnhanced/Macros/Actions/`. La Parte 1 ha mappato i comandi ma non ha verificato file per file. Ecco i file legacy **senza controparte esplicita**:

| File Legacy | Descrizione | Stato nel Nuovo |
|---|---|---|
| `MovementAction.cs` | Movimento direzionale (N/S/E/W/NE/NW/SE/SW) | ❌ Rimosso (WALK command) |
| `QueryStringResponseAction.cs` | Risposta a query string | ❌ Rimosso (raro) |
| `DisconnectAction.cs` | Disconnetti dal server | ❌ Rimosso intenzionalmente |
| `AbsoluteTargetAction.cs` | Target per coordinate assolute | ⚠️ Non verificato |
| `AbsoluteTargetVariableAction.cs` | Target variabile per coordinate | ⚠️ Non verificato |
| `WaitForPropertyAction.cs` | Attendi caricamento proprietà OPL | ⚠️ Non verificato se implementato come comando macro |

### Client/ Folder — Astrazione Client

| File Legacy | Descrizione | Nuovo |
|---|---|---|
| `Client/Client.cs` | Classe base astratta per tutti i client | `IClientAdapter.cs` (interfaccia, mai usata) |
| `Client/ClassicUO.cs` | Adapter per ClassicUO via reflection (API in-process) | `ClassicUOAdapter.cs` (**stub**) |
| `Client/OSIClient.cs` | Adapter per client OSI (DLL injection) | `OsiClientAdapter.cs` (**stub**) |
| `Client/UOAssist.cs` | Compatibilità UOAssist (IPC tra assistant) | ❌ Rimosso |

### Grids-Bars/ Legacy

| File Legacy | Descrizione | Nuovo |
|---|---|---|
| `UI/Grids-Bars/SpellGrid.cs` | SpellGrid WinForms | `SpellGridWindow.xaml` ✅ |
| `UI/Grids-Bars/Toolbar.cs` | Toolbar floating WinForms | `FloatingToolbarWindow.xaml` ✅ |
| `UI/Grids-Bars/UOABar.cs` | Barra compatibilità UOAssist | ❌ Rimosso (UOAssist non supportato) |

---

## 27. Audit Metodi Vuoti/Stub — Inventario Completo

Risultato della scansione completa del codebase TMRazorImproved per metodi non funzionali:

### Categoria: `return false/0/null/empty` con commento "stub" o "placeholder"

| File | Metodo | Riga | Ritorna | Impatto |
|---|---|---|---|---|
| `MiscApi.cs` | `HasMenu()` | 757 | `false` | **Alto** — Script crafting |
| `MiscApi.cs` | `MenuContain(text)` | 759 | `false` | **Alto** |
| `MiscApi.cs` | `GetMenuTitle()` | 760 | `""` | **Alto** |
| `MiscApi.cs` | `WaitForMenu(delay)` | 761 | `false` | **Alto** |
| `MiscApi.cs` | `HasQueryString()` | 768 | `false` | Medio |
| `MiscApi.cs` | `WaitForQueryString(delay)` | 769 | `false` | Medio |
| `MiscApi.cs` | `GetMapInfo(serial)` | 861 | `new MapInfo { Serial }` | Medio |
| `MiscApi.cs` | `LastHotKey()` | 1128 | `null` | Basso |
| `MiscApi.cs` | `GetContPosition()` | 1115 | `(0,0)` | Basso |
| `MiscApi.cs` | `ExportPythonAPI()` | 1109 | Messaggio "not implemented" | Basso |
| `SoundApi.cs` | `GetMinDuration()` | 125 | `0` | Basso |
| `SoundApi.cs` | `GetMaxDuration()` | 128 | `0` | Basso |
| `FriendApi.cs` | `ChangeList(name)` | 66 | No-op | Medio |
| `StaticsApi.cs` | `CheckDeedHouse(x,y)` | 194 | `false` | Medio |
| `ClassicUOAdapter.cs` | Tutti i metodi | 9-30 | Placeholder | **Critico** (architettura) |
| `OsiClientAdapter.cs` | Tutti i metodi | 17-37 | Placeholder | **Critico** (architettura) |
| `DisplayViewModel.cs` | Commento placeholder | 110 | N/A | Basso |

### Categoria: TODO espliciti

| File | Riga | Contenuto |
|---|---|---|
| `PacketLoggerService.cs` | 179 | `// TODO: Implement Template parsing if needed` |

### Categoria: No-op (metodi vuoti senza commento)

| File | Metodo | Impatto |
|---|---|---|
| `MiscApi.cs` | `CloseMenu()` | Alto — Script di crafting |
| `MiscApi.cs` | `MenuResponse(text)` | **Critico** — Script di crafting |
| `MiscApi.cs` | `QueryStringResponse(ok, text)` | Medio |

**Nota positiva**: Il codebase è molto pulito — solo 1 TODO esplicito e ~17 stub/placeholder totali. La maggior parte del codice è implementata e funzionale.

---

## 28. Riepilogo Task Aggiornato (Parti 1 + 2 + 3)

### Task Critici (Alta Priorità)

| ID | Tipo | Descrizione | File da Modificare | Fonte |
|---|---|---|---|---|
| TASK-004 | Bug/Incompleto | **Organizer: supporto Amount field** | `OrganizerService.cs` | Parte 1 |
| TASK-006 | Bug/Incompleto | **BandageHeal: ricerca bandage per ItemID** | `BandageHealService.cs` | Parte 1 |
| TASK-012 | Mancante | **ATTACK macro: modalità Nearest/Farthest/ByType** | `MacrosService.cs` | Parte 1 |
| TASK-021 | Mancante | **UOSteam Interpreter: 83+ comandi mancanti** | `UOSteamInterpreter.cs` + `ConditionEvaluator.cs` | Parte 2 |
| TASK-022 | Mancante | **Condizioni macro: 8 condizioni mancanti** | `ConditionEvaluator.cs` | Parte 2 |
| **TASK-027** | **Mancante** | **Sistema gestione shard completo** — CRUD shard con host/port/encryption/client type per-shard, persistenza JSON, UI launcher | Nuovo `ShardService.cs` + `ShardConfig.cs` + `GeneralPage.xaml` | **Parte 3** |
| **TASK-028** | **Mancante** | **Riconoscimento Intelligente Client** — Biforcazione flusso OSI/CUO/TmClient nel launch e nell'IPC | `GeneralViewModel.cs` + `ClientInteropService.cs` + `PacketService.cs` | **Parte 3** |
| **TASK-029** | **Architettura** | **Adapter pattern non collegato** — `ClassicUOAdapter` e `OsiClientAdapter` sono stub, `PacketService` bypassa l'interfaccia | `ClassicUOAdapter.cs` + `OsiClientAdapter.cs` + `PacketService.cs` | **Parte 3** |

### Task Medi (Media Priorità)

| ID | Tipo | Descrizione | File da Modificare | Fonte |
|---|---|---|---|---|
| TASK-001 | Mancante | **Commands service** — 18 comandi chat (`-where`, `-ping`, `-getserial`, etc.) | Nuovo `CommandService.cs` + `WorldPacketHandler.cs` | Parte 1/3 |
| TASK-006b | Incompleto | BandageHeal: buff wait | `BandageHealService.cs` | Parte 1 |
| TASK-010 | Mancante | MenuResponse packet (0x7D C→S) + coda menu | `PacketBuilder.cs` + `MacrosService.cs` | Parte 1 |
| TASK-013 | Mancante | SETABILITY macro command | `MacrosService.cs` | Parte 1 |
| TASK-014 | Mancante | PathFinding API scripting | Nuovo `PathFindingApi.cs` | Parte 1 |
| TASK-017 | Mancante | UI pagina Media | Nuovo `MediaPage.xaml` | Parte 1 |
| TASK-023 | Mancante | Counter API non esposta a scripting | Nuovo `CounterApi.cs` + `ScriptGlobals.cs` | Parte 2 |
| TASK-024 | Mancante | GumpInspector response logging | `InspectorViewModel.cs` + `WorldPacketHandler.cs` | Parte 2 |
| TASK-026 | Incompleto | LegacyMacroMigrator conversione lossy | `LegacyMacroMigrator.cs` | Parte 2 |
| **TASK-030** | **Stub** | **Menu system (6 metodi) completamente non funzionale** — Script di crafting rotti | `MiscApi.cs` + `WorldPacketHandler.cs` (coda menu) | **Parte 3** |
| **TASK-031** | **Stub** | **Query String system (3 metodi) non funzionale** | `MiscApi.cs` + `WorldPacketHandler.cs` (coda prompt) | **Parte 3** |
| **TASK-034** | **Stub** | **FriendApi.ChangeList() no-op** — Impossibile switchare lista amici da script | `FriendApi.cs` | **Parte 3** |
| **TASK-036** | **Mancante** | **OSI Encryption non supportata** — No `OSIEncryption` flag per-shard | `ConfigModels.cs` + `ClientInteropService.cs` | **Parte 3** |
| **TASK-038** | **Architettura** | **Launch flow supporta solo TmClient** — `Process.Start` al posto di `Loader.dll` per OSI | `GeneralViewModel.cs` | **Parte 3** |

### Task Bassi (Bassa Priorità)

| ID | Tipo | Descrizione | File da Modificare | Fonte |
|---|---|---|---|---|
| TASK-011 | Mancante | UnicodePromptSend packet (0xC2 C→S) | `PacketBuilder.cs` | Parte 1 |
| TASK-015 | Mancante | DPSMeter API scripting | Nuovo `DPSMeterApi.cs` | Parte 1 |
| TASK-016 | Mancante | PacketLogger API scripting | Nuovo `PacketLoggerApi.cs` | Parte 1 |
| TASK-018 | Mancante | AutoDoc generazione documentazione | Nuovo script/servizio | Parte 1 |
| TASK-019 | Mancante | Multi/House data service | Nuovo `MultiService.cs` | Parte 1 |
| TASK-020 | Incompleto | PacketLogger template parsing | `PacketLoggerService.cs:179` | Parte 1 |
| TASK-025 | Incompleto | SoundApi placeholder | `SoundApi.cs:125-129` | Parte 2 |
| **TASK-032** | **Stub** | **MiscApi.GetMapInfo() ritorna dati vuoti** | `MiscApi.cs` | **Parte 3** |
| **TASK-033** | **Stub** | **MiscApi metodi minori** (ExportPythonAPI, GetContPosition, LastHotKey) | `MiscApi.cs` | **Parte 3** |
| **TASK-035** | **Stub** | **StaticsApi.CheckDeedHouse() ritorna sempre false** | `StaticsApi.cs` | **Parte 3** |
| **TASK-037** | **Architettura** | **Login Relay non gestito attivamente** (riscrittura IP per intercettazione OSI) | `WorldPacketHandler.cs` | **Parte 3** |

---

## 29. Statistiche Finali Aggiornate (Parti 1 + 2 + 3)

| Metrica | Legacy | TMRazorImproved | Note Parte 3 |
|---|---|---|---|
| File C# totali | ~253 | ~150+ | |
| Riduzione codice agenti | — | 74-80% media | |
| Packet handler S→C | 56 | 60+ (4 nuovi) | |
| Packet handler C→S | 18 | 13 (5 gestiti da TmClient) | ⚠️ Solo per TmClient |
| Filtri | 13 classi | Tutti + 5 nuovi | |
| Azioni macro | 43 classi | 38 comandi | |
| Condizioni macro IF | ~23 tipi | 15 implementati, **8 mancanti** | |
| API scripting | 25+ | 23 + **6 API stub** (Menu, Query, Map) | **Aggiornato** |
| UOSteam comandi | 119 | ~38 (~32%) | |
| UOSteam espressioni | 84 | ~30 (~36%) | |
| Motori scripting | 3 | 3 | |
| Agenti | 8 | 11 (3 nuovi) | |
| **Shard supportati** | **Illimitati** (CRUD completo) | **Solo TmClient** (no shard CRUD) | **⚠️ Gap critico** |
| **Tipi client** | **3** (OSI, ClassicUO, varianti) | **1** (solo TmClient) | **⚠️ Gap critico** |
| **Adapter funzionanti** | **2** (OSI + CUO) | **0** (entrambi stub) | **⚠️ Gap architetturale** |
| **Comandi chat** | **18** | **0** | **⚠️ Gap** |
| **Stub/placeholder totali** | — | **17** metodi | Audit completo |
| Task critici aperti | — | **8** (+3) | **Aggiornato** |
| Task medi aperti | — | **14** (+5) | **Aggiornato** |
| Task bassi aperti | — | **11** (+4) | **Aggiornato** |
| **Copertura funzionale TmClient** | — | **~85%** | Invariata |
| **Copertura funzionale Multi-Shard** | — | **~60%** | **⚠️ Nuova metrica** |

### Nota sulla Doppia Copertura

La copertura funzionale deve essere misurata su **due assi**:

1. **Copertura TmClient (~85%)**: Per l'uso con TmClient.exe, la migrazione è quasi completa. I gap sono concentrati in UOSteam interpreter, condizioni macro, e API stub minori.

2. **Copertura Multi-Shard (~60%)**: Per l'uso come assistant generico multi-shard (come il legacy), mancano componenti **strutturali**: gestione shard, adapter funzionanti, encryption OSI, flusso di lancio biforcato, comandi chat.

### Priorità di Implementazione Consigliata

**Fase 1 — Consolidamento TmClient** (1-2 settimane):
- TASK-004, TASK-006, TASK-012 (bug/regressioni critiche)
- TASK-030 (Menu system per crafting scripts)
- TASK-001 (Command system — qualità della vita)

**Fase 2 — UOSteam Interpreter** (2-3 settimane):
- TASK-021, TASK-022 (massimo impatto sull'utenza scripting)

**Fase 3 — Architettura Multi-Shard** (3-5 settimane):
- TASK-027 (ShardService + UI)
- TASK-028, TASK-029 (Riconoscimento Intelligente + Adapter pattern)
- TASK-036, TASK-037, TASK-038 (Encryption + Login Relay + Launch biforcato)

**Fase 4 — Completamento** (2 settimane):
- Tutti i task rimanenti di media/bassa priorità

---

# PARTE 4 — Audit Profondo: Servizi Erroneamente Segnati come "Completi"

**Data**: 18 Marzo 2026 (sessione 4)
**Autore**: Analisi automatica (revisione deep-diff)
**Scopo**: Confronto metodo-per-metodo dei servizi segnati come "✅ Completo" nella Parte 1 che si rivelano **significativamente incompleti**. Questa sezione corregge valutazioni ottimistiche della review iniziale.

---

## 30. TargetingService — Copertura Reale: ~37%

**Stato precedente**: ✅ Completo → **⚠️ Significativamente Incompleto**

**Legacy** (`Razor/Core/Targeting.cs`, 980 righe) vs **Nuovo** (`TargetingService.cs`, 363 righe)

### TASK-039: TargetingService — Funzionalità Mancanti Critiche

| Funzionalità Legacy | Descrizione | Presente nel Nuovo? |
|---|---|---|
| Smart Last Target | Differenzia target per tipo: harm, beneficial, ground | ❌ **Mancante** |
| 4 tipi di target tracking | `m_LastTarget`, `m_LastHarmTarg`, `m_LastBeneTarg`, `m_LastGroundTarg` | ❌ Solo `_lastTarget` singolo |
| Range check configurabile | `RangeCheckLT` + `LTRange` per-profilo | ❌ Solo `config.Range` fisso |
| Target queue | Queue di azioni target (`DoTargetSelf()`, `DoLastTarget()`, `DoAutoTarget()`) se nessun cursor attivo | ❌ **Mancante** — azioni falliscono silenziosamente |
| DoAutoTarget() | Selezione automatica con validazione e range check | ❌ **Metodo assente** |
| OneTimeTarget() | Target callback per scripting con intercept mode | ❌ **Mancante** |
| CheckHealPoisonTarget() | Blocca heal su target avvelenato, blocca veleno su target curato | ❌ **Mancante** |
| TextFlags su mobili | Messaggi overhead per tipo di target (Last, Harm, Bene) | ❌ **Mancante** |
| Spell Target ID tracking | `m_SpellTargID` per distinguere target di spell da target normali | ❌ **Mancante** |

**Impatto**: Qualsiasi script o macro che usa targeting avanzato (heal/harm differentiation, auto-target, target queue) **non funzionerà** come previsto. In PvP, il smart last target è essenziale.

**Priorità**: **CRITICA** — Il targeting è il cuore del gameplay UO.

---

## 31. HotkeyService — Copertura Reale: ~11%

**Stato precedente**: ✅ Completo → **❌ Gravemente Incompleto**

**Legacy** (`RazorEnhanced/HotKey.cs`, 2.139 righe) vs **Nuovo** (`HotkeyService.cs`, 240 righe)

### TASK-040: HotkeyService — 25+ Categorie di Azioni Mancanti

Il legacy supporta **30+ categorie di azioni hotkey**. Il nuovo ne implementa **~5** (solo targeting di base):

#### Categorie Implementate (✅)
- Target Next
- Target Closest
- Target Self
- Last Target
- Clear Target

#### Categorie Mancanti (❌) — Ordinate per Impatto

| Categoria | Descrizione | Impatto |
|---|---|---|
| `ProcessSpellsMagery` | Lancio spell Magery (64 spell) | **Critico** — PvP/PvE |
| `ProcessSpellsChivalry` | Lancio spell Chivalry | **Critico** — Paladin |
| `ProcessSpellsNecro` | Lancio spell Necromancy | **Alto** |
| `ProcessSpellsBushido` | Lancio spell Bushido | **Alto** |
| `ProcessSpellsNinjitsu` | Lancio spell Ninjitsu | **Alto** |
| `ProcessSpellsSpellweaving` | Lancio spell Spellweaving | **Alto** |
| `ProcessSpellsMysticism` | Lancio spell Mysticism | **Alto** |
| `ProcessAbilities` | Abilità primaria/secondaria arma | **Critico** — PvP |
| `ProcessAttack` | Attacco target (nearest/last/etc.) | **Critico** — PvP |
| `ProcessBandage` | Bendaggio rapido (self/target) | **Critico** — PvP |
| `ProcessPotions` | Uso pozioni rapido | **Critico** — PvP |
| `ProcessHands` | Toggle armi nelle mani (clear/toggle) | **Alto** |
| `ProcessDress` | Attivazione liste dress | **Alto** |
| `ProcessSkills` | Uso skill da hotkey | **Alto** |
| `ProcessAgentsAutoloot` | Toggle AutoLoot | **Medio** |
| `ProcessAgentsScavenger` | Toggle Scavenger | **Medio** |
| `ProcessAgentsOrganizer` | Toggle Organizer | **Medio** |
| `ProcessAgentDress` | Dress/Undress da hotkey | **Medio** |
| `ProcessAgentRestock` | Toggle Restock | **Medio** |
| `ProcessAgentBandage` | Toggle BandageHeal | **Medio** |
| `ProcessAgentFriend` | Gestione amici | **Basso** |
| `ProcessScript` | Esecuzione script da hotkey | **Alto** |
| `ProcessGeneral` | Azioni generali (resync, screenshot, etc.) | **Medio** |
| `ProcessEquipWands` | Equip bacchette magiche | **Basso** |
| `ProcessPet` | Comandi pet | **Basso** |

#### Funzionalità Aggiuntive Mancanti

| Funzionalità | Descrizione | Impatto |
|---|---|---|
| Master Key Toggle | Tasto master per abilitare/disabilitare TUTTI gli hotkey | **Alto** |
| Supporto mouse | Wheel Click/Up/Down, X Button 1/2 (codici 500-504) | **Alto** — giocatori usano mouse per PvP |
| Key binding UI | Interfaccia per assegnare tasti (capture mode) | **Alto** |
| Pass-through | Flag per-hotkey: se premuto, il tasto passa anche al gioco | **Medio** |
| Conflict detection | Rilevamento conflitti tra hotkey duplicati | **Medio** |

**Impatto complessivo**: Un giocatore PvP configura tipicamente **30-50 hotkey** per spell, pozioni, abilità, target, bendaggi. Con solo 5 azioni disponibili, l'HotkeyService è **praticamente inutilizzabile** per il gameplay reale.

**Priorità**: **CRITICA** — Seconda solo al targeting per importanza nel gameplay.

---

## 32. DragDropCoordinator — Copertura Reale: ~22%

**Stato precedente**: ✅ Completo → **⚠️ Significativamente Incompleto**

**Legacy** (`RazorEnhanced/DragDropManager.cs`, 237 righe) vs **Nuovo** (`DragDropCoordinator.cs`, 53 righe)

### TASK-041: DragDropCoordinator — Automazione Mancante

| Funzionalità Legacy | Descrizione | Presente nel Nuovo? |
|---|---|---|
| Auto-Loot Queue | Coda di item da lootare dai corpse con refresh | ❌ **Mancante** |
| Scavenger Queue | Coda di item da raccogliere a terra | ❌ **Mancante** |
| Auto-Carver Queue | Coda di corpse da tagliare | ❌ **Mancante** |
| Weight Check | Verifica peso prima di lootare (salta se overweight) | ❌ **Mancante** |
| Z-Level Validation | Controllo altezza (|diff| > 8 = skip) | ❌ **Mancante** |
| Delay Management | Delay configurabili per azione | ⚠️ Solo hardcoded 50ms/150ms |
| Corpse Refresh | Gestione corpse nulli/stale con reordering | ❌ **Mancante** |
| Item Visibility | Check `IsLootableTarget` e item nascosti | ❌ **Mancante** |
| User Feedback | Log visibile e messaggi al player | ❌ Solo logger interno |

**Nota**: I servizi agente (`AutoLootService`, `ScavengerService`, `AutoCarverService`) esistono come classi separate, ma il `DragDropCoordinator` stesso non coordina le loro code. Ogni agente dovrebbe delegare al coordinator per rispettare i delay tra operazioni drag/drop, ma il coordinator attuale è un semplice wrapper `LiftAsync`/`DropAsync` senza coda.

**Impatto**: Le operazioni drag/drop concorrenti di più agenti possono **collidere** (due agenti che cercano di muovere item nello stesso tick causano desync).

**Priorità**: **ALTA** — Necessario per il corretto funzionamento degli agenti.

---

## 33. SecureTradeService — Copertura Reale: ~70%

**Stato precedente**: ✅ Completo → **⚠️ Parzialmente Incompleto**

### TASK-042: SecureTradeService — API Mancanti

| Funzionalità Legacy | Presente? |
|---|---|
| Trade accept/cancel | ✅ |
| Trade tracking (serial, status) | ✅ |
| Currency tracking (gold, plat) | ✅ |
| `Offer(tradeId, gold, platinum)` — aggiornamento offerta mid-trade | ❌ **Mancante** |
| `StartTrade()` — iniziazione trade | ❌ **Stub** |
| `Copy()` — deep clone trade | ❌ **Mancante** |
| Container serial tracking | ❌ **Mancante** |
| `LastUpdate` timestamp | ❌ **Mancante** |

**Impatto**: Script che automatizzano il commercio (es. aggiornamento offerta in base al valore degli item inseriti) **non funzionano**.

**Priorità**: Media — Il commercio manuale funziona, ma l'automazione è limitata.

---

## 34. FilterHandler — Copertura Reale: ~85%

**Stato precedente**: ✅ Completo → **⚠️ Quasi Completo (con lacune)**

### TASK-043: FilterHandler — Granularità Persa

| Funzionalità Legacy | Presente? | Dettaglio |
|---|---|---|
| Filtro per tipo messaggio (system vs speech) | ❌ | Il nuovo filtra solo per keywords hardcoded (poison, karma, snoop), non distingue system messages da player speech |
| Replacement grafico per-creatura configurabile | ❌ | Il nuovo usa `0x0033` (Slime) hardcoded per tutti i mostri filtrati, il legacy permetteva graphic diversi per dragon/drake/daemon |
| Light state update on toggle | ❌ | Il nuovo blocca solo pacchetti incoming, non aggiorna lo stato luce del client quando il filtro viene attivato/disattivato |

**Impatto**: Basso — I filtri principali funzionano, le lacune sono di configurabilità avanzata.

---

## 35. Riepilogo Correzioni ai Rating della Parte 1

La tabella della Sezione 2 (Mappatura Servizi Core) conteneva rating **ottimistici**. Ecco le correzioni:

| Servizio | Rating Parte 1 | Rating Corretto | Copertura Reale |
|---|---|---|---|
| Targeting | ✅ Completo | **⚠️ Incompleto** | ~37% |
| HotKey | ✅ Completo | **❌ Gravemente Incompleto** | ~11% |
| DragDropCoordinator | ✅ Completo | **⚠️ Incompleto** | ~22% |
| SecureTradeService | ✅ Completo | **⚠️ Parziale** | ~70% |
| FilterHandler | ✅ Completo (tutte le categorie) | **⚠️ Quasi Completo** | ~85% |
| ActionQueue → DragDropCoordinator | ✅ Completo | **⚠️ Incompleto** | ~22% |

### Servizi che rimangono ✅ Completo (verificato):
- WorldService, SkillsService, JournalService, ConfigService
- PathFindingService, FriendsService, VendorService
- SpellDefinitions, EncodedSpeechHelper
- PacketBuilder, UOBufferReader
- Tutti i motori di scripting (Python, C#, UOSteam base)

---

## 36. Task Aggiuntivi (Parte 4)

### Task Critici Aggiunti

| ID | Tipo | Descrizione | File da Modificare |
|---|---|---|---|
| **TASK-039** | Incompleto | **TargetingService: Smart targeting, harm/bene, queue, auto-target** | `TargetingService.cs` |
| **TASK-040** | Incompleto | **HotkeyService: 25+ categorie azioni mancanti** (spell, abilità, pozioni, agent toggle, dress, script) | `HotkeyService.cs` |

### Task Alti Aggiunti

| ID | Tipo | Descrizione | File da Modificare |
|---|---|---|---|
| **TASK-041** | Incompleto | **DragDropCoordinator: code agente, weight check, Z-level, delay** | `DragDropCoordinator.cs` |

### Task Medi Aggiunti

| ID | Tipo | Descrizione | File da Modificare |
|---|---|---|---|
| **TASK-042** | Incompleto | **SecureTradeService: Offer API, StartTrade, container tracking** | `SecureTradeService.cs` |
| **TASK-043** | Incompleto | **FilterHandler: filtro per tipo messaggio, graphic configurabile** | `FilterHandler.cs` |

---

## 37. Statistiche Finali Complete (Parti 1 + 2 + 3 + 4)

| Metrica | Legacy | TMRazorImproved | Note |
|---|---|---|---|
| File C# totali | ~253 | ~283 (inclusi test) | Il nuovo ha più file grazie a DI/MVVM |
| Riduzione codice agenti | — | 74-80% media | Ma coordinator è 78% meno funzionale |
| Packet handler S→C | 56 | 60+ | ✅ |
| Packet handler C→S | 18 | 13 | 5 delegati a TmClient |
| Filtri | 13 classi | Tutti + 5 nuovi | ⚠️ Granularità ridotta |
| Azioni macro | 43 classi | 38 comandi | |
| UOSteam comandi | 119 | ~38 (~32%) | ❌ Gap critico |
| UOSteam espressioni | 84 | ~30 (~36%) | ❌ Gap critico |
| **Targeting modes** | **4** (harm/bene/ground/general) | **1** (singolo) | **❌ Regressione** |
| **Hotkey categories** | **30+** | **~5** | **❌ Regressione critica** |
| **DragDrop queues** | **3** (loot/scav/carve) | **0** | **❌ Regressione** |
| Shard supportati | Illimitati | Solo TmClient | ❌ |
| Tipi client | 3 (OSI/CUO/variant) | 1 (TmClient) | ❌ |
| Adapter funzionanti | 2 | 0 (stub) | ❌ |
| Comandi chat | 18 | 0 | ❌ |
| Stub/placeholder API | — | 17+ metodi | Audit completo |
| **Task critici aperti** | — | **10** | +2 da Parte 4 |
| **Task alti aperti** | — | **1** | Nuovo |
| **Task medi aperti** | — | **16** | +2 da Parte 4 |
| **Task bassi aperti** | — | **11** | Invariati |
| **Task totali aperti** | — | **38** | |

### Metriche di Copertura Aggiornate

| Asse | Copertura | Trend |
|---|---|---|
| **Servizi Core (backend)** | ~90% | Invariata — i servizi dati (World, Skills, Journal) sono solidi |
| **Servizi di Automazione** | ~55% | **⬇ Rivista** — Targeting -37%, HotKey -11%, DragDrop -22% |
| **Scripting (API)** | ~75% | Invariata — stub presenti ma base solida |
| **UOSteam** | ~32% | Invariata — gap più grande |
| **UI/UX** | ~85% | Invariata — WPF è molto più moderno del WinForms |
| **Multi-Shard** | ~30% | **⬇ Rivista** — senza shard CRUD, adapter e launch biforcato |
| **Copertura complessiva ponderata** | **~65%** | **⬇ Da 85% a 65%** |

### Nota sulla Copertura Ponderata

La copertura è calcolata **pesando per importanza gameplay**:
- Targeting, Hotkey, DragDrop sono usati **ogni secondo** di gameplay attivo
- UOSteam è usato da **>50%** della community scripting
- Multi-Shard è necessario per **qualsiasi shard non-TmClient**
- I servizi backend (World, Skills, Journal) sono "invisibili" all'utente

La precedente stima dell'85% era basata su conteggio servizi senza ponderazione per importanza. Con la ponderazione, la copertura reale è **~65%**.

### Piano di Priorità Aggiornato

**Fase 0 — Regressioni Critiche** (1-2 settimane):
- TASK-039 (TargetingService) — Necessario per PvP/PvE funzionale
- TASK-040 (HotkeyService) — Necessario per gameplay reale

**Fase 1 — Bug e Automazione** (1-2 settimane):
- TASK-004, TASK-006, TASK-012 (bug agenti)
- TASK-041 (DragDropCoordinator coda)
- TASK-030 (Menu system per crafting)

**Fase 2 — UOSteam** (2-3 settimane):
- TASK-021, TASK-022

**Fase 3 — Multi-Shard** (3-5 settimane):
- TASK-027, TASK-028, TASK-029, TASK-036, TASK-037, TASK-038

**Fase 4 — Completamento** (2+ settimane):
- Tutti i task rimanenti
