# Analisi Interfacce GUI Mancanti — TMRazor Improved

**Data Analisi**: 4 Marzo 2026
**Data Aggiornamento**: 4 Marzo 2026 (Post-Sprint UI Fix)
**Architetto**: Senior Software Architect — Migrazione .NET 10 / WPF
**Stato**: 🟢 Core UI Implementato | 🟡 Feature Avanzate in corso

## 1. Introduzione
In seguito allo sprint di sviluppo interfacce, i gap critici identificati per la "Feature Parity" sono stati colmati. Il sistema ora dispone dei componenti fondamentali per la selezione dei colori, l'interazione con i Gump e la gestione avanzata di Macro e Mappa.

---

## 2. Stato Interfacce GUI

### 2.1 Utility & Controlli Condivisi
| Componente | Stato | Descrizione |
|------------|-------|-------------|
| **HuePicker Dialog** | ✅ **Completato** | Implementato `HuePickerWindow` con ricerca e anteprima real-time dei colori UO. |
| **Search/Filter Global Tool** | ❌ Mancante | Manca una UI di ricerca globale per Items e Mobiles nel mondo. |
| **Context Menus Globali** | ⚠️ Parziale | In corso di integrazione nei vari `ListView`. |

### 2.2 Modulo Map & Pathfinding
| Componente | Stato | Descrizione |
|------------|-------|-------------|
| **Advanced Map Window** | ✅ **Completato** | Implementata `MapWindow` flottante con zoom, tracking del player e markers per mobile. |
| **Pathfinding Visualizer** | ❌ Mancante | Interfaccia di debug per visualizzare in-game il percorso calcolato. |
| **Static Map Inspector** | ❌ Mancante | UI per l'ispezione dei tiles (Z, Flags, LOS). |

### 2.3 Scripting & Automazione
| Componente | Stato | Descrizione |
|------------|-------|-------------|
| **Persistent Console Window** | ⚠️ Parziale | La console è presente in `ScriptingPage`. |
| **SpellGrid Designer** | ✅ **Completato** | Implementata `SpellGridWindow` con configurazione salvata nel profilo utente. |
| **Macro Fine-Tuning** | ✅ **Completato** | `MacrosPage` ora permette l'editing dei parametri e il riordinamento dei passi. |

### 2.4 Ispezione & Debugging
| Componente | Stato | Descrizione |
|------------|-------|-------------|
| **Gump Response Simulator** | ✅ **Completato** | Aggiunto pulsante "Execute" nell'Inspector per simulare risposte reali (0xB1). |
| **Gump List / Manager** | ❌ Mancante | Manca una lista centralizzata dei Gump aperti. |
| **OPL Deep Inspector** | ⚠️ Parziale | Implementato in `InspectorPage`. |

### 2.5 Combat & Health
| Componente | Stato | Descrizione |
|------------|-------|-------------|
| **Combat Strategy Editor** | ❌ Mancante | UI per auto-potions e priorità di cura. |
| **Overhead Messages Config** | ❌ Mancante | Configurazione dei messaggi overhead (Danni, System). |
| **Skill Gain History** | ❌ Mancante | Grafico storico del progresso skill. |

---

## 3. Avanzamento Priorità

1.  **P0 (Bloccanti)**: ✅ **100%** (HuePicker, Macro Tuning, Gump Simulator completati).
2.  **P1 (Feature Parity)**: ⚠️ **60%** (SpellGrid e Map Window completati; Overhead Config mancante).
3.  **P2 (Polish)**: ❌ **0%** (Ancora da iniziare).

---
*Documento aggiornato da Gemini CLI — Senior Software Architect*
