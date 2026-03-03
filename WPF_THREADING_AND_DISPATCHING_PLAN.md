# Piano di Migrazione Architetturale: Threading, Cancellazione e Sicurezza UI in .NET 10

Questo documento analizza le due criticità più gravi emerse durante la migrazione di TMRazor da .NET Framework 4.7.2 (WinForms) a .NET 10 (WPF) e fornisce una roadmap passo-passo per risolverle definitivamente.

---

## 🛑 1. Il Problema della Cancellazione: Addio a `Thread.Abort()`

In .NET Framework, TMRazor faceva largo uso di `Thread.Abort()` per interrompere forzatamente l'esecuzione di script (Python/UOSteam), macro e agenti in background (AutoLoot, Scavenger, ecc.).
In .NET 10, **`Thread.Abort()` lancia `PlatformNotSupportedException`**. Non è più possibile "uccidere" un thread dall'esterno in modo arbitrario, poiché questo lasciava la memoria corrotta e causava deadlock nei lock di sistema.

### Obiettivo: Passaggio alla Cancellazione Cooperativa (TAP)
Tutto il codice basato su `Thread` deve essere migrato al **Task-based Asynchronous Pattern (TAP)** utilizzando `Task` e `CancellationTokenSource`.

### Step di Implementazione:

- [ ] **1.1. Refactoring dei Servizi di Background (Agenti/Macro):**
  - Sostituire le istanze di `System.Threading.Thread` con `System.Threading.Tasks.Task`.
  - Introdurre un `CancellationTokenSource` a livello di servizio per ogni agente in esecuzione.
  - Modificare la firma dei metodi di loop (es. `AutoLootLoop()`) in `async Task AutoLootLoopAsync(CancellationToken token)`.

- [ ] **1.2. Implementazione dei Check di Cancellazione:**
  - All'interno dei loop `while(true)` o `for` degli agenti, inserire chiamate regolari a `token.ThrowIfCancellationRequested()`.
  - Passare il `CancellationToken` a tutti i metodi bloccanti o di lunga durata (es. `Task.Delay(100, token)` al posto di `Thread.Sleep(100)`).
  - Assicurarsi che i blocchi `try-catch` intercettino la `OperationCanceledException` o `TaskCanceledException` per eseguire una pulizia "pulita" dello stato dell'agente allo stop.

- [ ] **1.3. Gestione del Motore Scripting (Python & UOSteam):**
  - *Il problema più complesso:* gli script scritti dagli utenti possono contenere loop infiniti o istruzioni bloccanti.
  - **UOSteam Engine**: Nel parser C# che esegue riga per riga, passare un `CancellationToken` al motore di esecuzione. Prima di ogni riga interpretata, chiamare `token.ThrowIfCancellationRequested()`.
  - **IronPython Engine**: Iniettare un meccanismo di trace/hook in IronPython (`sys.settrace` o tramite l'hosting API) che verifichi lo stato del `CancellationToken` ogni $N$ istruzioni eseguite. In alternativa, tutte le funzioni esposte all'utente (`Item.FindByID`, `Player.Walk`, ecc.) devono internamente controllare il token prima di procedere. Se il token è annullato, devono sollevare un'eccezione Python che interrompe lo script.

---

## 🚧 2. Il Problema della UI: Cross-Thread Exceptions in WPF

WPF impone una rigida **Thread Affinity**: solo il thread che ha creato un elemento UI (il Dispatcher Thread) può modificarlo.
In TMRazor, il thread di rete (`IPacketService`) riceve centinaia di pacchetti al secondo (es. update delle barre della vita, movimento del pg, nuovi oggetti a schermo) e aggiorna lo stato (`IWorldService`).

Se un ViewModel (`ObservableObject`) è in binding con la UI e viene aggiornato da un thread di rete, WPF andrà in crash istantaneamente con l'errore:
`The calling thread cannot access this object because a different thread owns it.`

### Obiettivo: Sincronizzazione sicura ed efficiente tra Core e UI
Dobbiamo garantire che la UI rifletta lo stato del mondo senza bloccare il thread di rete e senza intasare il Dispatcher WPF con troppi eventi.

### Step di Implementazione:

- [ ] **2.1. Separazione dei Modelli (Thread-Safe State):**
  - I modelli nel layer `Core` (es. `UOEntity`, `Mobile`, `Item`) **NON** devono implementare `INotifyPropertyChanged` in modo diretto, oppure, se lo fanno, devono essere progettati in modo da non triggerare la UI WPF direttamente dal thread di rete.
  - Lo stato in `IWorldService` deve essere aggiornabile da qualsiasi thread usando lock appropriati (`lock`, `ConcurrentDictionary`, o `ReaderWriterLockSlim`).

- [ ] **2.2. Dispatching Esplicito per Aggiornamenti Singoli:**
  - Quando il layer Core usa l'`IMessenger` per notificare un cambiamento importante (es. "PlayerHpChanged"), il ViewModel WPF che riceve il messaggio deve esplicitamente spostare l'esecuzione sul thread della UI:
    ```csharp
    Application.Current.Dispatcher.Invoke(() => {
        HpValue = message.NewHp;
    });
    ```

- [ ] **2.3. Gestione Sicura delle Collezioni (ObservableCollection):**
  - Le `ObservableCollection<T>` non sono thread-safe. Se un pacchetto di rete aggiunge 10 item alla borsa, la UI crasherà se l'inserimento non è sul thread UI.
  - **Soluzione A (Semplice)**: Usare `BindingOperations.EnableCollectionSynchronization(myCollection, _lockObject);` all'inizializzazione del ViewModel. Questo dice a WPF di gestire automaticamente i lock quando la collezione viene modificata da thread in background.
  - **Soluzione B (Performante, Consigliata per UI complesse)**: Limitarsi a usare collezioni non-osservabili nel `Core` e, nel `ViewModel`, fare un polling a bassa frequenza (es. ogni 100ms) o ricevere eventi "Batch" che sincronizzano la `ObservableCollection` della UI solo con i cambiamenti effettivi, eseguiti in un unico blocco sul Dispatcher.

- [ ] **2.4. Throttling/Debouncing degli Eventi ad Alta Frequenza:**
  - Pacchetti come lo "Status Update" o "Object Property List" arrivano a raffica. Inviare un evento UI per ciascuno intaserà il Dispatcher rendendo l'app non responsiva ("lag visivo").
  - Implementare un meccanismo (tramite `System.Reactive` Rx.NET o un semplice timer `DispatcherTimer`) nel ViewModel: i cambiamenti vengono accumulati e la UI viene aggiornata con i nuovi valori aggregati a un refresh rate fisso (es. 60 FPS o ogni 50ms).

---

## 📋 Checklist Riepilogativa (Ordine di Esecuzione)

1. [ ] **Pulizia Generale:** Cercare globalmente `Thread.Abort()`, `Thread.Sleep()`, `new Thread(` in tutto il vecchio codice convertito.
2. [ ] **Infrastruttura Async:** Creare `CancellationTokenSource` nei servizi che gestiscono cicli di vita prolungati (Scripting, Macro, Agenti).
3. [ ] **Refactoring Loop:** Sostituire `Thread.Sleep(x)` con `Task.Delay(x, token)` all'interno degli Agenti (AutoLoot, Dress, ecc.).
4. [ ] **Interop Scripts:** Implementare i controlli del `CancellationToken` all'interno dell'interprete UOSteam e nelle API esposte a IronPython.
5. [ ] **Setup WPF Sync:** In tutti i costruttori dei ViewModel che espongono liste (es. lista amici, lista filter, hotkeys), configurare `BindingOperations.EnableCollectionSynchronization`.
6. [ ] **Messenger Sicuro:** Configurare i recipienti `IMessenger` della UI affinché utilizzino sistematicamente `Dispatcher.Invoke` (o `Dispatcher.InvokeAsync`) per aggiornare le singole proprietà esposte (HP, Mana, Weight, ecc.).
7. [ ] **Test sotto stress:** Avviare il client UO loggando in un'area densamente popolata e verificare che l'uso della CPU da parte di WPF rimanga stabile, assicurandosi che non ci siano *Cross-Thread Exceptions*.