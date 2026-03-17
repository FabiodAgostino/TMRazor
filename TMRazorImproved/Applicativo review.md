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
| Packet Handling | 95% | ✅ Buono |
| Thread Safety | 95% | ✅ Validato con stress test |
| Sistema Macro | 90% | ✅ Migrazione legacy completata |
| Agenti (AutoLoot, Dress, ecc.) | 95% | ✅ Fix auto-start completato |
| Vendor Buy/Sell | 100% | ✅ Completato |
| Scripting API (Python/C#/UOS) | 85% | ✅ Test integrazione passati |
| UI / WPF / MVVM | 95% | ✅ Display & Counters completati |
| SpecialMoves API | 100% | ✅ Completato |
| Screen Capture | 100% | ✅ Completato |
| Profili per-shard | 100% | ✅ Completato |
| Macro Recorder | 100% | ✅ Completato |

### Semaforo Generale

| Dimensione | Stato |
|---|:---:|
| Compilazione | ✅ Compila |
| Crash runtime noti | ✅ Race condition risolte |
| Feature parity vs originale | ⚠️ ~92% |
| Compatibilità script Python/UOS | ✅ ~85% |
| Pronto per produzione | ⚠️ Beta testing richiesto |

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
  ├── Views/Pages/ (24 pagine XAML)
  ├── Views/Windows/ (7 finestre overlay)
  ├── ViewModels/ (MVVM CommunityToolkit)
  └── Resources/ (localizzazione IT)

TMRazorImproved.Tests (xUnit, Moq)
  ├── Unit Tests
  ├── Stress Tests
  └── Fuzz Tests
```

---

## 3. Criticità Residue per Priorità

Tutte le criticità P0 e P1 precedentemente identificate sono state risolte negli Sprint 1-5.

---

## 4. Task List Completa per la Migrazione al 100%

### Sprint 1-4 (COMPLETATI)
*Vedi storico per dettagli su ScreenCapture, SpecialMoves, Thread Safety, Scripting e Features UI.*

### Sprint 5 — Display & Counters (COMPLETATO il 17 Marzo 2026)
| Task | Descrizione | Stato |
|------|-------------|-------|
| TASK-S5-01 | Integrazione TitleBar in UserProfile | ✅ |
| TASK-S5-02 | Creazione DisplayViewModel & DisplayPage | ✅ |
| TASK-S5-03 | Integrazione Counters in DisplayPage | ✅ |
| TASK-S5-04 | Fix Auto-Start Agenti in App.xaml.cs | ✅ |

---

## Riepilogo Priorità di Esecuzione

### Sprint 3 — Features
| Task | Descrizione | Stato |
|------|-------------|-------|
| TASK-E01 | SpellGrid overlay completo | ✅ Completato |
| TASK-E02 | Map/Radar real-time | ✅ Completato |
| TASK-E03 | HP Bar overlay completo | ✅ Completato |
| TASK-E04 | Macro Recorder UI | ✅ Completato |
| TASK-E05 | Vendor Buy/Sell backend | ✅ Completato |
| TASK-E06 | Object Inspector | ✅ Completato |
| TASK-E08 | VideoCaptureService con SharpAvi | ✅ Completato |
| TASK-E09 | Tool migrazione macro legacy | ✅ Completato |

### Sprint 4 — Qualità e Rifinitura
| Task | Descrizione | Stato |
|------|-------------|-------|
| TASK-E07 | Profile per-shard | ✅ Completato |
| TASK-F01 | Refactor GetActiveConfig | ✅ Completato |
| TASK-F02 | PathFinding range configurabile | ✅ Completato |
| TASK-F03 | Player.SetLast highlight packet | ✅ Completato |
| TASK-H01 | Doc differenze API scripting | ✅ Completato |
| TASK-H02 | Audit ScriptGlobals completo | ✅ Completato |
| TASK-G03 | Test regressione SpecialMoves | ✅ Completato |
| P2-04 | SearchService lock granularity | ✅ Completato |
| P2-05 | SkillsService thread-safety | ✅ Completato |

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
| `TMRazorImproved.UI/Views/Pages/DisplayPage.xaml` | UI Display & Counters |
| `Razor/RazorEnhanced/` | Codebase legacy di riferimento per gap analysis |
| `TMRazorImprovedProgress.md` | Checklist granulare di migrazione |

---
**IDENTIFICATIVO:** Claude-TMRazor-Sprint5-DisplayCounters-20260317
---