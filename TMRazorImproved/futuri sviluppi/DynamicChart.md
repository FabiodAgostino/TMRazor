# Analisi Sviluppo: Motore "DynamicChart" (Widget Dinamici)

## 1. Visione Generale
L'obiettivo di questa funzionalità è sostituire i grafici statici e hardcoded (come l'attuale Storico Gain basato su rettangoli XAML) con un **Motore di Dashboard Dinamico**. Questo permetterà all'utente finale di TMRazorImproved di agire come un "Data Analyst" del proprio gameplay in Ultima Online.
Tramite un tool integrato (Widget Editor), l'utente potrà definire *cosa* tracciare, *come* visualizzarlo (linee, barre, torte) e *dove* posizionarlo nell'interfaccia (Dashboard principale, pannello laterale, finestre overlay).

## 2. Esempi di Grafici (Casi d'Uso in Ultima Online)

Essendo UO un gioco profondamente basato su statistiche, macro ed economia, le possibilità sono vastissime. Ecco alcuni esempi di grafici che l'utente potrebbe creare in autonomia:

### 2.1. Progressione Personaggio
*   **Tracciamento Skill Mirato (Line Chart):** L'utente seleziona 2 o 3 skill specifiche (es. *Magery*, *Evaluating Intelligence*, *Resisting Spells*) e visualizza un grafico a linee che mostra la salita (Gain) o la discesa (Loss) nel tempo. Il tracciamento inizia dal momento in cui il grafico viene generato o la sessione viene avviata.
*   **Statistiche di Combattimento (Bar/Pie Chart):** Conteggio di *Nemici Uccisi* (PvE/PvP diviso per tipologia di mostro), *Morti* subite dal giocatore.
*   **Analisi Danni / DPS (Area Chart):** *Danni Inflitti (DPS)* confrontati con i *Danni Ricevuti*. Estremamente utile per valutare l'efficacia di un template d'armatura, una specifica arma o una macro di attacco durante un combattimento prolungato.

### 2.2. Economia e Risorse (Farming)
*   **Gold & Loot Tracker (Line/Area Chart):** Denaro raccolto nel tempo (Gold per Hour). Può essere esteso per includere il valore stimato di oggetti rari trovati.
*   **Gathering Rate (Bar Chart):** Risorse base raccolte all'ora. Quanti *Log* (legna), *Ore* (minerali) o *Leather* (pelli) la macro sta producendo. Fondamentale per i crafter.
*   **Consumo Reagenti / Bende (Line Chart Invertito/Discesa):** Un grafico che mostra il declino delle risorse primarie. Aiuta a capire visivamente quanto velocemente si stanno consumando risorse vitali durante uno scontro lungo (es. contro un Boss o in un Champion Spawn), fornendo un segnale chiaro su quando è il momento di ritirarsi o fare restock.
*   **Weight Capacity (Gauge Chart):** Un indicatore a "tachimetro" (o circolare progressivo) che mostra quanto il personaggio è vicino al limite di peso, vitale per chi farma in modo semi-AFK.

### 2.3. Sistema, Etica e Rete
*   **Fame & Karma (Line Chart):** Tracciamento delle variazioni di Fama e Karma durante una sessione di caccia, utile per chi cerca di raggiungere specifici titoli o livellare abilità basate sul Karma (es. Chivalry).
*   **Ping & Latenza (Line Chart):** Grafico tecnico ma vitale per il PvP. Mostra la stabilità della connessione verso il server (Min, Max, Avg) nel tempo, evidenziando chiaramente i picchi di lag (packet loss).

## 3. Architettura Tecnica

Per realizzare questo motore mantenendo l'applicazione leggera, reattiva e coerente con lo stack attuale (WPF + MVVM con CommunityToolkit), l'architettura si baserà su tre pilastri:

### 3.1. Il Motore Grafico: LiveCharts2
Attualmente TMRazorImproved disegna rettangoli WPF a mano (es. `GainToHeightConverter` in `SkillsPage.xaml`). Per il nuovo sistema è mandatoria l'integrazione di una libreria dedicata come **LiveCharts2**.
*   *Perché:* Offre rendering ad alte prestazioni, animazioni fluide, tooltips nativi, interattività e soprattutto un data-binding perfetto e nativo con MVVM.

### 3.2. Astrazione dei Dati (Data Pipeline)
I dati non devono più risiedere nei singoli ViewModel (come avviene ora in `SkillsViewModel`), ma devono essere generati dal `Core` tramite un pattern Publisher/Subscriber (Event Aggregation, già presente tramite `IMessenger`).
*   **Data Providers:** Servizi in background (es. `SkillTrackerService`, `CombatTrackerService`, `InventoryTrackerService`) analizzano costantemente i pacchetti di rete nativi (forniti da `PacketService`) ed emettono eventi strutturati.
*   **Data Stream:** Il motore grafico si "abbona" esclusivamente ai flussi dati richiesti dai widget attualmente attivi a schermo, risparmiando risorse CPU/RAM.

### 3.3. Configurazione Dinamica (Il Modello JSON)
Ogni grafico configurato dall'utente diventerà un file JSON salvato nella cartella `Profiles\Widgets` o direttamente in `global.json`.
*   **Struttura JSON (Esempio concettuale):**
    ```json
    {
      "WidgetId": "wgt_skill_magery",
      "Title": "Progressione Magery",
      "Type": "LineSeries",
      "DataSource": "SkillGainStream",
      "DataFilters": { "SkillName": "Magery" },
      "Color": "#FF00FF00",
      "RefreshRateMs": 1000,
      "TimeWindowMinutes": 60
    }
    ```

### 3.4. Rendering Dinamico in WPF
L'interfaccia non avrà più i grafici "scolpiti" direttamente nel codice XAML.
*   Utilizzeremo i `ContentControl` associati a un `DataTemplateSelector` di WPF.
*   Nelle viste (es. `DashboardPage.xaml` o in finestre custom flottanti), ci saranno degli "Slot" (zone contenitore vuote).
*   L'utente sceglierà dal menù a tendina: "In questo Slot, carica il Widget *Progressione Magery*".
*   Il ViewModel sottostante caricherà il JSON, inizializzerà il ViewModel generico del grafico collegandolo al `DataStream` corretto, e WPF renderizzerà il `LiveCharts2` corrispondente in tempo reale.

## 4. Fasi di Implementazione Consigliate

1.  **PoC (Proof of Concept) Tecnologico:** Installare LiveCharts2 (tramite NuGet) e sostituire l'attuale "Storico Gain" in `SkillsPage.xaml` con un grafico a linee base, mantenendo l'attuale sorgente dati. Questo valida la libreria all'interno dell'ecosistema dell'app.
2.  **Sviluppo dei Provider Dati (Core):** Creare i servizi in `TMRazorImproved.Core` necessari per tracciare Loot, Danni e Kills intercettando e parsando i pacchetti corretti di Ultima Online (es. pacchetti di update statistiche, messaggi di sistema del combat log, variazioni di inventory).
3.  **Sviluppo Widget Editor (UI):** Creare una nuova finestra WPF o un pannello dove l'utente può configurare graficamente il proprio "Widget" senza dover scrivere JSON manualmente (selezionando tramite Dropdown la sorgente dati, il tipo di grafico e i colori).
4.  **Integrazione Aree Dinamiche:** Aggiungere i `ContentControl` nella Dashboard e predisporre il sistema di caricamento dei profili utente per memorizzare la disposizione dei widget scelti.