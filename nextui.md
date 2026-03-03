# Roadmap UI/Logica: Verso la Parità Funzionale (TMRazor Improved)

Questo documento definisce i passi necessari per completare la migrazione da TMRazor (WinForms) a TMRazorImproved (WPF/MVVM .NET 10), colmando le lacune identificate nell'analisi architettonica.

## 1. Infrastruttura Shell & Navigazione (MainWindow)
- [x] **Integrazione dinamica TitleBarService**: Collegare il servizio alla `ui:TitleBar` per visualizzare nome personaggio, shard, coordinate e contatori personalizzati.
- [x] **Completamento Menu di Navigazione**: 
    - [x] Collegare le voci di sottomenu per tutti gli Agenti (Dress, Restock, Vendor, ecc.) che hanno già una Pagina.
    - [x] Aggiungere icone coerenti (WPFUI Icons) a tutte le voci di navigazione.
- [x] **Sistema di Notifiche**: Implementare un `NotificationService` (WPFUI `Snackbar`) per feedback visivi su avvio/stop script, cattura immagini e avvisi degli agenti.

## 2. Pagine UI Mancanti (Parità con i Servizi esistenti)
- [x] **Modulo Journal (Log di gioco)**
    - [x] Creazione `JournalPage.xaml` e `JournalViewModel.cs`.
    - [x] Supporto per il filtraggio dei messaggi (Sistema, World, Gilda).
    - [x] Supporto per il salvataggio dei log su file.
- [x] **Modulo Network (Packet Logger)**
    - [x] Creazione `PacketLoggerPage.xaml`.
    - [x] Visualizzazione in tempo reale dei pacchetti In/Out (collegamento a `PacketService`).
    - [x] Implementazione funzioni "Ignora pacchetto" e "Copia Hex".
- [x] **Modulo Secure Trade**
    - [x] UI per il monitoraggio degli scambi sicuri e logica di accettazione automatica basata su filtri.
- [x] **Modulo Media & Gallery**
    - [x] Pagina per la gestione e visualizzazione di Screenshot e Video catturati tramite i servizi dedicati.
- [ ] **Modulo Mappa & Navigazione**
    - [ ] Visualizzazione di base della mappa utilizzando `UltimaSDK` e `MapService`.

## 3. Logica Core Mancante (Gap di Servizi)
- [ ] **Special Moves Service**: Migrare la logica di gestione delle mosse speciali e la relativa UI per la configurazione.
- [ ] **Season Manager**: Implementare il servizio per la gestione delle stagioni e dei suoni ambientali mancante in Improved.
- [ ] **Gump Display Engine**: Mentre l'Inspector esiste, manca un sistema per visualizzare i Gump di gioco all'interno della UI WPF per debug o scripting assistito.

## 4. Rafforzamento Scripting & Editor
- [ ] **Integrazione AvalonEdit**:
    - [ ] Implementare la sintassi Highlighting per UOSteam (XSHD).
    - [ ] Implementare la sintassi Highlighting per Python.
    - [ ] Aggiungere l'autocompletamento basato sulle classi `Api` (Player, Items, Mobiles, ecc.).
- [ ] **Script Recorder UI**: Creazione dell'interfaccia per registrare azioni in-game e trasformarle in script Python/UOSteam.

## 5. Ottimizzazioni per .NET 10 & WPF
- [ ] **Conversione GDI+ -> WPF**: Refactoring definitivo di `UltimaSDK` per eliminare `System.Drawing` e usare `WriteableBitmap` per Tile e Gump, evitando colli di bottiglia nel rendering.
- [ ] **Binding Optimization**: Verificare che tutte le `ObservableCollection` usate nei servizi (es. Journal, Mobile list) utilizzino `BindingOperations.EnableCollectionSynchronization` per evitare crash da thread diversi.
- [ ] **Native Interop Check**: Validazione finale dei P/Invoke x86 per il Client UO su runtime .NET 10.

## 6. Polishing & UX
- [ ] **Gestione Profili UI**: UI dedicata per il caricamento, salvataggio e duplicazione dei profili (collegamento a `ConfigService`).
- [ ] **Dark/Light Mode Sync**: Assicurarsi che il cambio tema si propaghi correttamente a tutti i controlli custom (incluso AvalonEdit).
