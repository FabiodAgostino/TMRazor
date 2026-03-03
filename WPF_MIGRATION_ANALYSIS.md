# Documento di Analisi: Migrazione TMRazor a WPF (.NET 10)

## 1. Visione del Progetto
**TMRazorImproved** rappresenta l'evoluzione moderna di TMRazor. L'obiettivo è trasformare un'applicazione monolitica basata su WinForms in una moderna applicazione **WPF** basata su **.NET 10**, adottando un'architettura a **Micro-Servizi Interni** e il pattern **MVVM**.

## 2. Stack Tecnologico Selezionato
*   **Runtime**: .NET 10 (Sfruttando le ultime ottimizzazioni JIT e il supporto DPI migliorato).
*   **UI Framework**: WPF (Windows Presentation Foundation).
*   **Design System**: **WPFUI** (Libreria ispirata a Windows 11 Sun Valley, altamente manutenuta e moderna).
*   **Pattern Architetturale**: MVVM (Model-View-ViewModel) con **CommunityToolkit.Mvvm**.
*   **Dependency Injection**: **Microsoft.Extensions.DependencyInjection**.
*   **Code Editor**: **AvalonEdit** (Sostituzione di FastColoredTextBox per il supporto scripting).
*   **Log**: NLog (già presente, da integrare nel sistema DI).

## 3. Strategia di Migrazione: "Side-by-Side Reconstruction"
Invece di tentare una conversione automatica, il progetto verrà ricostruito da zero per garantire la massima pulizia del codice.
1.  Creazione della nuova Solution in .NET 10.
2.  Estrazione della logica "Core" dal vecchio progetto.
3.  Integrazione dei servizi uno alla volta.
4.  Sviluppo dell'interfaccia WPF modulare.

## 4. Architettura dei Micro-Servizi Interni
La logica verrà separata in servizi indipendenti (Singleton) iniettati nei ViewModel:

| Servizio | Responsabilità | Stato Originale |
| :--- | :--- | :--- |
| **IPacketService** | Intercettazione e invio pacchetti UO | `Razor/Network/` |
| **IWorldService** | Stato di Mobiles, Items, Gumps e Mappa | `Razor/Core/World.cs` |
| **IScriptingService** | Esecuzione script Python/UOSteam | `Razor/RazorEnhanced/Scripts.cs` |
| **IConfigService** | Gestione profili, JSON e impostazioni | `Razor/RazorEnhanced/Settings.cs` |
| **IMacroService** | Registrazione ed esecuzione macro | `Razor/UI/MacrosUI.cs` |
| **IAgentService** | AutoLoot, Scavenger, Organizer, Dress | `Razor/RazorEnhanced/Agent/` |

## 5. Analisi dei Componenti Critici

### 5.1 UltimaSDK (Referenza Esterna)
*   **Sfida**: Utilizza `System.Drawing` (GDI+).
*   **Soluzione**: Mantenere la logica di parsing dei file `.mul/.uop`, ma creare un layer di conversione `Bitmap` -> `BitmapSource` (WPF) per la visualizzazione dei tile e dei gump.

### 5.2 FastColoredTextBox -> AvalonEdit
*   **Sfida**: FCTB è WinForms e gestisce la sintassi personalizzata di UOSteam.
*   **Soluzione**: Implementare un `HighlightingDefinition` (XSHD) per AvalonEdit che replichi la sintassi UOSteam e Python, garantendo performance superiori con file di grandi dimensioni.

### 5.3 DLL Native (Crypt/Loader)
*   **Sfida**: Interazione a basso livello con il client UO.
*   **Soluzione**: Mantenere i P/Invoke esistenti in un servizio dedicato `IClientInteropService`. Verificare la compatibilità degli indirizzi di memoria con il runtime .NET 10.

## 6. Piano d'Azione (Roadmap)

### Fase 0: Setup Iniziale
*   Creazione Solution `TMRazorImproved`.
*   Setup progetti: `TMRazor.Core` (Logica), `TMRazor.UI` (WPF), `TMRazor.Shared` (Interfacce e Modelli).
*   Configurazione del container DI (Inversion of Control).

### Fase 1: Core Logic Extraction
*   Estrarre `Assistant.Engine` e trasformarlo in un `Bootstrapper`.
*   Isolare il Networking layer dal vecchio `MainForm`.
*   Migrare `UltimaSDK` come progetto referenziato.

### Fase 2: UI Shell Development (WPFUI)
*   Creazione della finestra principale con navigazione laterale (Sidebar).
*   Implementazione del Dark/Light Mode dinamico.
*   Creazione dei ViewModel di base per la sincronizzazione dei dati di gioco.

### Fase 3: Moduli Agenti e Macro
*   Migrazione logica AutoLoot/Scavenger in servizi dedicati.
*   Creazione delle View WPF per ogni agente.
*   Implementazione del sistema di Data Binding per eliminare gli aggiornamenti manuali della UI.

### Fase 4: Scripting & Editor
*   Integrazione AvalonEdit.
*   Porting dei motori IronPython e UOSteam.
*   Console di output integrata con binding asincroni.

## 7. Domande Strategiche e Rischi (Aggiornamento Critico .NET 10)
1.  **Prestazioni**: Il rendering WPF è più pesante di WinForms? *No, su .NET 10 con accelerazione hardware sarà più fluido, specialmente per le barre della salute e i contatori.*
2.  **Threading e Cross-Thread Exceptions (Bloccante)**: La natura asincrona di WPF richiede un uso massiccio di `ObservableCollection` protette. Poiché la rete elabora pacchetti su background thread, **qualsiasi modifica alla UI senza `Dispatcher.Invoke` o `BindingOperations.EnableCollectionSynchronization` causerà il crash dell'applicazione**.
3.  **Il Cimitero dei Thread (Bloccante)**: In .NET Framework, TMRazor usava ampiamente `Thread.Abort()` per fermare agenti e script. In .NET 10 questo lancia una `PlatformNotSupportedException`. **È obbligatorio un refactoring totale basato su `Task` e `CancellationTokenSource`** (vedi documento allegato `WPF_THREADING_AND_DISPATCHING_PLAN.md`).
4.  **Il GDI+ "Zombie"**: L'uso di `System.Drawing` in `UltimaSDK` è deprecato e solleva warning in .NET 10. Passare continuamente HBitmap a WPF causerà colli di bottiglia e memory leak. Andrà convertito a `WriteableBitmap`.
5.  **Sicurezza x86 e P/Invoke**: L'iniezione di codice (es. `CreateRemoteThread`) tramite `UOMod.dll` o chiamate native deve avvenire rigorosamente in un contesto a 32-bit. In .NET 10, le protezioni di memoria e gli antivirus sono molto più restrittivi, richiedendo massima attenzione nel marshalling degli `IntPtr`.
6.  **IronPython Compatibility**: L'attuale IronPython 3.4.2 per `net472` rischia problemi di compatibilità con la DLR e i tipi .NET 10, specialmente durante il binding. Richiederà testing intensivo.

---
**Documento creato il**: 2 Marzo 2026
**Autore**: Gemini CLI (Engineering Sub-agent)
**Aggiornamenti Correlati**: Vedi `WPF_THREADING_AND_DISPATCHING_PLAN.md` per il piano di risoluzione delle criticità di Threading e UI.
