# Roadmap Refactoring: TMRazor Improved (.NET 10)

Questo documento dettaglia le ultime tre fasi necessarie per portare la UI/UX di TMRazor Improved a un livello professionale, garantendo stabilità, velocità e modernità.

---

## 1. Global Search / Command Palette 🔍
**Obiettivo**: Implementare un sistema di ricerca universale (stile VS Code `Ctrl+Shift+P`) per navigare istantaneamente tra script, macro e agenti senza usare il mouse.

### Perché serve?
Con l'aumentare della complessità del profilo, trovare uno specifico script o attivare un agente può richiedere molti click. Una Command Palette centralizza il controllo dell'applicazione.

### Step di implementazione:
- [ ] **Creazione del SearchService (Core)**: Un servizio singleton che indicizza dinamicamente:
    - Nomi degli Script caricati.
    - Nomi delle Macro registrate.
    - Pagine della UI (es. "Go to AutoLoot", "Open Filters").
    - Comandi rapidi (es. "Stop All Scripts", "Take Screenshot").
- [ ] **Sviluppo UI Overlay**: Un controllo popup leggero che appare sopra ogni altra finestra, con una barra di ricerca e una lista di risultati filtrabili.
- [ ] **Algoritmo di Fuzzy Search**: Implementare una ricerca non esatta per permettere di trovare "ALoot" scrivendo solo "al".
- [ ] **Global Shortcut**: Registrare la combinazione di tasti `Ctrl+Shift+P` (o `Alt+Space`) per invocare la palette da qualsiasi punto dell'app o del gioco.

---

## 2. Validazione Input & Security 🛡️
**Obiettivo**: Rendere l'applicazione "robusta" impedendo l'inserimento di dati malformati e proteggendo l'esecuzione di codice esterno.

### Perché serve?
Attualmente, inserire un valore non numerico in un campo "Serial" o "Hue" può causare eccezioni nel core o crash della UI. Inoltre, l'integrazione di script Python richiede un layer di sicurezza per evitare operazioni IO non autorizzate.

### Step di implementazione:
- [ ] **Base ViewModel Validation**: Estendere `ViewModelBase` con il supporto a `IDataErrorInfo` o `INotifyDataErrorInfo`.
- [ ] **Surgical UI Masks**: Applicare maschere di input nelle View per:
    - **Seriali**: Accettare solo esadecimali (es. 0x4001ABCD) o decimali lunghi.
    - **Hue**: Limitare il range tra 0 e 3000.
    - **Timer/Delay**: Impedire valori negativi o eccessivamente bassi che saturano il network.
- [ ] **Feedback Visivo**: Usare gli stili WPFUI per evidenziare i bordi delle TextBox in rosso e mostrare un ToolTip di errore quando il dato è invalido.
- [ ] **Script Sandbox Check**: Aggiungere una scansione preliminare degli script caricati per avvisare l'utente se contengono chiamate potenzialmente pericolose (es. `os.remove`).

---

## 3. Theme & UI Polish 🎨
**Obiettivo**: Perfezionare l'estetica "Sun Valley" (Windows 11) e garantire che le preferenze dell'utente (Dark/Light mode) siano persistenti.

### Perché serve?
Un'applicazione professionale deve rispettare le impostazioni di sistema e avere transizioni fluide. Al momento, alcune finestre secondarie potrebbero non seguire il tema principale o resettarsi al riavvio.

### Step di implementazione:
- [ ] **Theme Persistence Service**: Integrare la scelta del tema (Light, Dark, High Contrast) nel `GlobalSettings.json`.
- [ ] **Auto-Detection Sistema**: Implementare un listener che cambia il tema dell'app in tempo reale quando l'utente cambia il tema di Windows.
- [ ] **Transizioni tra Pagine**: Aggiungere animazioni di "Fade" o "Slide" quando si naviga nel `NavigationView` per eliminare il salto netto tra le schermate.
- [ ] **Window Backdrop (Mica/Acrylic)**: Applicare l'effetto Mica (vetro satinato tipico di Win11) a tutte le finestre, incluse le toolbar flottanti, per un feeling premium.
- [ ] **Custom ScrollBars**: Uniformare lo stile delle barre di scorrimento in tutte le pagine (Scripting, Log, Macros) seguendo il design system WPFUI.
