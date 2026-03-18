# Analisi Sviluppo: Sistema di Auto-Update (Approccio Self-Updating con Helper)

## 1. Visione Generale
L'obiettivo è fornire a TMRazorImproved un sistema di aggiornamento automatico integrato che comunichi direttamente con le Release di GitHub. Questo garantirà che gli utenti abbiano sempre l'ultima versione con le nuove funzionalità e i bug fix, migliorando l'esperienza utente e riducendo i problemi di supporto legati a versioni obsolete.

Abbiamo scelto l'approccio **Self-Updating (Soluzione 2)**, ideale per applicazioni "portable" (distribuite come file ZIP/eseguibile stand-alone senza un vero e proprio installer di sistema). Questo approccio mantiene l'applicazione contenuta nella sua cartella ma gestisce elegantemente il problema del blocco dei file (File Lock) imposto dal sistema operativo Windows quando un eseguibile è in uso.

## 2. Flusso dell'Esperienza Utente (UX)

1. **Avvio Silenzioso:** L'utente avvia normalmente `TmRazorImproved.exe`.
2. **Controllo Background:** Durante il caricamento iniziale (Splash Screen o primissimi istanti nella Dashboard), il programma effettua una richiesta asincrona e silenziosa alle API di GitHub.
3. **Notifica Discreta:** Se viene trovata una versione con un tag superiore a quella attuale, compare un dialog (o una barra in cima alla Dashboard) con il titolo della nuova release (es. `v1.2.0`) e il changelog formattato.
4. **Scelta dell'Utente:**
   - **"Ignora per ora":** Il dialog si chiude e l'utente continua a usare l'applicazione normalmente.
   - **"Aggiorna Ora":** Inizia il processo di download. Un indicatore di progresso mostra lo stato dello scaricamento dello `.zip` della nuova release.
5. **Transizione:** Al termine del download, il programma avvisa l'utente: *"L'applicazione si riavvierà per applicare l'aggiornamento"*. `TmRazorImproved.exe` si chiude.
6. **Magia dell'Helper:** Appare per qualche secondo una piccola finestra (o console) dell'`UpdateHelper.exe` che estrae i nuovi file e sovrascrive i vecchi.
7. **Riavvio:** `TmRazorImproved.exe` viene riavviato automaticamente, aggiornato all'ultima versione.

## 3. Architettura Tecnica e Risoluzione del File Lock

Il problema principale nell'aggiornare un software in esecuzione è che Windows impedisce la modifica (o l'eliminazione) del file `.exe` principale e delle sue librerie (`.dll`) mentre sono caricati in memoria.

Ecco come l'architettura risolve questo scoglio:

### 3.1. Fase 1: Controllo Versione (TMRazorImproved)
- L'app principale utilizza `HttpClient` per chiamare `https://api.github.com/repos/{owner}/{repo}/releases/latest`.
- Analizza la risposta JSON per estrarre `tag_name` (es. "v1.5.0") e confrontarlo con la versione attuale (`Assembly.GetExecutingAssembly().GetName().Version`).
- Estrae il campo `body` (Changelog) e l'URL di download dell'asset (`browser_download_url` del file `.zip`).

### 3.2. Fase 2: Download e Preparazione (TMRazorImproved)
- L'app scarica il file `.zip` in una cartella temporanea sicura, ad esempio `%LocalAppData%\Temp\TMRazorUpdate` o in una sottocartella `_update` all'interno della directory del programma.
- Insieme al programma principale, verrà distribuito un piccolo eseguibile di servizio chiamato `TMRazorUpdater.exe` (l'Helper).
- L'app principale avvia `TMRazorUpdater.exe` passando come argomenti:
  1. Il PID (Process ID) di TMRazorImproved.
  2. Il percorso del file `.zip` appena scaricato.
  3. Il percorso della cartella di destinazione (dove si trova attualmente TMRazor).
- Subito dopo aver lanciato l'Helper, **TMRazorImproved chiama `Application.Current.Shutdown()` per chiudersi e liberare i file**.

### 3.3. Fase 3: Estrazione e Sostituzione (TMRazorUpdater.exe)
Questo Helper deve essere un'applicazione console .NET leggerissima (senza dipendenze complesse che potrebbero essere bloccate) che esegue i seguenti passaggi:
1. **Attesa Chiusura:** Attende (tramite `Process.GetProcessById(pid).WaitForExit()`) che TMRazorImproved sia completamente chiuso e che i file non siano più bloccati.
2. **Estrazione e Sovrascrittura:** Usa `System.IO.Compression.ZipFile` per estrarre il contenuto dello `.zip` scaricato direttamente nella cartella di destinazione, sovrascrivendo i file esistenti (es. `TmRazorImproved.exe`, `TMRazorPlugin.dll`, ecc.).
3. **Pulizia:** Elimina il file `.zip` temporaneo scaricato per non lasciare "spazzatura" nel sistema.
4. **Riavvio:** Avvia la nuova versione di `TmRazorImproved.exe`.
5. **Terminazione:** Si auto-chiude.

## 4. Salvaguardia dei Dati Utente

Un aspetto critico di questo processo è **NON sovrascrivere i dati dell'utente** (configurazioni, profili, macro, log).
Poiché lo `.zip` scaricato da GitHub conterrà solo i file eseguibili e le librerie pulite, l'estrazione sovrascriverà solo i binari.
Tuttavia, è essenziale che:
- File come `global.json`, la cartella `Profiles`, e i log risiedano in una struttura di directory che non viene toccata dallo `.zip` della release.
- Se il file zip della release contiene file di configurazione di default (es. `default_config.json`), questi non devono chiamarsi come i file di configurazione in uso dall'utente, altrimenti l'estrazione li azzererebbe.

## 5. Fasi di Implementazione Consigliate

1. **Creazione dell'Helper (`TMRazorUpdater`):** Creare un nuovo progetto Console molto semplice all'interno della Solution di TMRazor. Dovrà solo prendere argomenti da riga di comando, aspettare un processo, estrarre uno zip, e avviare un nuovo processo.
2. **Integrazione API GitHub:** Scrivere un servizio in `TMRazorImproved.Core` (es. `GitHubUpdateService`) che faccia la chiamata REST, faccia il parsing del JSON (gestendo i limiti di rate (Rate Limiting) delle API di GitHub in modo appropriato) e scarichi l'asset.
3. **Sviluppo UI di Aggiornamento:** Creare la finestra di dialogo o il pannello in `DashboardPage` che mostri il Changelog e la progress bar del download.
4. **Test di Sostituzione (Locale):** Creare una finta "nuova versione" in uno zip locale e testare il passaggio di consegne tra `TMRazorImproved` e `TMRazorUpdater` per assicurarsi che i file vengano effettivamente liberati e sovrascritti senza eccezioni `IOException` da parte di Windows.