![GitHub License](https://img.shields.io/github/license/RazorEnhanced/RazorEnhanced)
![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-blueviolet)
![Fork of](https://img.shields.io/badge/fork%20of-Razor%20Enhanced-orange)
![Ultima Online](https://img.shields.io/badge/game-Ultima%20Online-darkblue)

<div align="center">

# ⚔️ TM Razor

### La versione di Razor Enhanced pensata per **The Miracle Shard**

*Un fork di [Razor Enhanced](https://github.com/RazorEnhanced/RazorEnhanced) completamente reimmaginato per l'esperienza di gioco su The Miracle.*

</div>

---

## 📖 Cos'è TM Razor?

**TM Razor** è un fork personalizzato di [Razor Enhanced](https://github.com/RazorEnhanced/RazorEnhanced), il client assistant open-source più avanzato per Ultima Online 2D (.NET).

Razor Enhanced nasce come evoluzione di Razor, il celebre tool di Bryan Pass ("Zippy"), e aggiunge un potente sistema di scripting in Python (IronPython) e C#, agenti automatici, un sistema di macro avanzato, la gestione degli hotkey, filtri di contenuto e molto altro. TM Razor prende tutto questo e lo specializza per il server **The Miracle**, lo shard italiano che ospita una community dedicata e appassionata di Ultima Online.

> **Progetto originale:** [github.com/RazorEnhanced/RazorEnhanced](https://github.com/RazorEnhanced/RazorEnhanced)
> **Sito Razor Enhanced:** [razorenhanced.net](http://razorenhanced.net/)
> **Wiki Razor Enhanced:** [razorenhanced.net/dokuwiki](http://razorenhanced.net/dokuwiki/doku.php)
> **API Scripting:** [razorenhanced.github.io/doc/api](https://razorenhanced.github.io/doc/api/)

---

## ✨ Modifiche specifiche per The Miracle

TM Razor introduce una serie di modifiche mirate, pensate per integrarsi perfettamente con il client **The Miracle Shard**.

---

### 🛡️ 1. Anti Macro Check 

---

### 🗣️ 2. Traduzione dell'Interfaccia

Tutta l'interfaccia utente di TM Razor è stata tradotta in **italiano** tramite il componente `LanguageHelper`, un sistema di traduzione dinamica che:

- **Traduce automaticamente** tutti i controlli di ogni form (label, bottoni, menu, tooltip, ToolStrip) all'avvio, senza richiedere codice manuale per ogni singola stringa.
- Utilizza **chiavi gerarchiche** (`FormName.ControlName.Text`) mappate su file di risorse `.resx`, facilmente estensibili per aggiungere nuove stringhe.
- Supporta la **traduzione ricorsiva** di controlli annidati e menu a tendina (`ToolStripDropDown`).
- Si integra con il sistema di localizzazione storico di Razor (`LocString`) per le stringhe di gioco (100+ voci enumerate).
- Il **Launcher** di TM Razor è localizzato con titolo "Welcome to TM Razor" e tutti i campi (shard, percorso client, lingua, host, porta) tradotti.

Il risultato è un'interfaccia completamente in italiano, familiare e immediata per i giocatori di The Miracle.

---

### 🎨 3. Refactoring Grafico Totale — Design Moderno e Accattivante

TM Razor introduce un **redesign completo dell'interfaccia grafica**, ispirato ai moderni design system con un'estetica dark, pulita e professionale. L'obiettivo era trasformare l'aspetto classico di Razor Enhanced (basato su form WinForms standard) in qualcosa di visivamente all'altezza di un tool del 2020s.

#### Palette colori (Dark Mode)

| Ruolo | Colore | Hex |
|---|---|---|
| Primario / Accento | Arancione | `#F97316` |
| Sfondo principale | Deep Blue Grey | `#152331` |
| Sfondo gradiente | Nero puro | `#000000` |
| Card / Pannelli | Blu scuro | `#1E2D3D` |
| Superficie profonda | Blu notte | `#050C26` |
| Glow / Bordi | Viola | `#8B5CF6` |
| Testo principale | Grigio chiaro | `#E5E7EB` |
| Testo secondario | Grigio medio | `#9CA3AF` |
| Successo | Verde | `rgb(34, 197, 94)` |
| Errore | Rosso | `rgb(239, 44, 44)` |
| Avviso | Giallo | `rgb(234, 179, 8)` |

#### Componenti UI custom creati da zero

- **`RazorCard`** — Pannello con angoli arrotondati (radius 12), bordo luminoso viola (`#8B5CF6`) e sfondo a tema. Sostituisce i classici `GroupBox` di WinForms con un look moderno e stilizzato.
- **`RazorButton`** — Pulsante con stile flat, colore arancione `#F97316`, effetti hover (`#EA580C`) e pressed (`#C2410C`) animati, cursore mano, font Nunito Bold. Supporta colori personalizzati per bottoni semantici (es. rosso per "Elimina").
- **`RazorToggle`** — Interruttore on/off in stile moderno (sliding toggle) con animazione, colori semantici e rendering personalizzato. Sostituisce le `CheckBox` standard di WinForms.
- **`RazorComboBox`** — Menu a tendina con stile dark, font tematizzato e bordo personalizzato.
- **`RazorTextBox`** — Campo di testo tematizzato con sfondo scuro e colori integrati nel tema.
- **`RazorSidebarTab`** — Tab di navigazione laterale con indicatore di selezione animato, rendering anti-aliased, font Nunito e icone testuali.
- **`RazorTabControl`** — Controllo tab personalizzato integrato con il tema globale.

#### Funzionalità UI avanzate

- **Dark Mode nativa su Windows**: integrazione con l'API `dwmapi.dll` per attivare il titolo della finestra in dark mode su Windows 10/11, rendendo l'intera finestra coerente con il tema.
- **Gradiente di sfondo**: lo sfondo dell'applicazione usa un gradiente verticale da `#152331` a `#000000` per dare profondità e dimensione all'interfaccia.
- **Font Nunito**: tipografia moderna e leggibile al posto dei font di sistema predefiniti, con fallback su Segoe UI.
- **Anti-aliasing su tutti i controlli custom**: rendering vettoriale (GDI+) con `SmoothingMode.AntiAlias` per bordi perfettamente levigati su qualsiasi risoluzione.
- **Layout a sidebar**: la navigazione tra le sezioni usa una sidebar verticale con tab attivi evidenziati, al posto del classico sistema di tab orizzontali.
- **Double buffering**: tutti i controlli usano `OptimizedDoubleBuffer` per eliminare lo sfarfallio durante il rendering.

---

## 🚀 Come installare TM Razor — Il Patcher

> **Non è necessaria alcuna installazione manuale.** Un semplice tool automatizzato si occupa di tutto.

### Cos'è il Patcher?

Nella sezione [**Releases**](../../releases) di questa repository è disponibile il **TM Razor Patcher**, un eseguibile Windows che si occupa di:

1. **Rilevare la cartella di installazione** del client The Miracle sul tuo PC.
2. **Creare una copia clonata** della cartella di The Miracle — il tuo originale rimane **intatto e funzionante** esattamente come prima.
3. **Integrare il plugin TM Razor** nella copia clonata, copiando `RazorEnhanced.exe`, tutte le DLL necessarie, patchando il client per adattarsi alla modifica.
4. **Configurare ClassicUO** modificando `settings.json` affinché carichi automaticamente TM Razor come plugin ad ogni avvio.

### Procedura di installazione

```
1. Vai nella sezione "Releases" di questa repository
2. Scarica il file "TMRazorPatcher.exe"
3. Esegui il Patcher (potrebbe richiedere i permessi di Amministratore)
4. Seleziona la cartella di installazione di The Miracle
   (se non viene rilevata automaticamente)
5. Attendi il completamento — il Patcher crea la cartella "TheMiracle_TMRazor"
6. Avvia il client dalla nuova cartella
7. TM Razor si aprirà automaticamente insieme al client ClassicUO
```

### Cosa fa il Patcher nel dettaglio?

| Operazione | Descrizione |
|---|---|
| **Copia sicura** | Clona l'intera cartella del client senza modificare l'originale |
| **Integrazione plugin** | Copia `RazorEnhanced.exe` e le DLL dipendenti nella cartella clonata |
| **Configurazione ClassicUO** | Aggiunge il plugin alla lista dei plugin in `settings.json` |
| **Profilo pre-configurato** | Crea un profilo con host e porta di The Miracle già impostati |
| **Lingua default** | Imposta l'italiano come lingua predefinita dell'interfaccia |
| **Aggiornamenti automatici** | Configura l'auto-updater per ricevere gli aggiornamenti futuri di TM Razor |

### Struttura dopo il Patcher

```
C:\TheMiracle\                      ← cartella originale (INVARIATA)
C:\TheMiracle_TMRazor\              ← nuova cartella creata dal Patcher
    ├── ClassicUO.exe               ← client UO
    ├── settings.json               ← configurato con plugin TM Razor
    ├── RazorEnhanced.exe           ← TM Razor (plugin)
    ├── RazorEnhanced.resources.dll ← risorse in italiano
    ├── it\                         ← satellite assembly italiana
    │   └── RazorEnhanced.resources.dll
    ├── Profiles\                   ← profili pre-configurati
    │   └── RazorEnhanced.shards    ← The Miracle pre-configurato
    └── ... (resto dei file del client)
```

### Requisiti

- **Windows 10 / 11** (64-bit consigliato)
- **.NET Framework 4.8** (già incluso in Windows 10 v1903 e successivi)
- **The Miracle Client** installato (versione ClassicUO)
- Connessione internet per eventuali aggiornamenti automatici

---

## 🔌 Architettura Plugin (come funziona sotto al cofano)

TM Razor si integra con **ClassicUO** come plugin nativo tramite l'interfaccia `PluginHeader`. Questo significa che:

- Il plugin viene caricato **in-process** da ClassicUO all'avvio, senza processi separati.
- Ha accesso diretto ai pacchetti di rete, intercettando e inviando pacchetti sia nel senso client→server che server→client.
- Può leggere lo stato completo del gioco in tempo reale (posizione del player, oggetti, mobile, abilità, buff, ecc.).
- Non richiede iniezione di codice o DLL esterne invasive — è il metodo ufficiale di estensione di ClassicUO.

```
ClassicUO.exe (client Ultima Online)
    └── Plugin: RazorEnhanced.exe (TM Razor)
            ├── Intercetta pacchetti di rete (bidirezionale)
            ├── Macro System (43 tipi di azione)
            ├── Script Engine
            │   ├── Python (IronPython 3.4)
            │   ├── C# (Roslyn compiler runtime)
            │   └── UOSteam (engine custom)
            ├── Agenti automatici
            │   ├── AutoLoot, Scavenger, Organizer
            │   ├── Restock, Dress, AutoBandage
            │   └── Vendor Buy/Sell
            ├── Hotkey Manager
            ├── Gump Inspector & Handler
            ├── Proto-Control (gRPC/WebSocket)
            └── UI TM Razor
                ├── Dark Mode (Deep Blue + Orange)
                ├── Interfaccia in Italiano
                └── Componenti custom (Card, Toggle, ecc.)
```

---

## 🧩 Funzionalità ereditate da Razor Enhanced

Oltre alle modifiche specifiche per The Miracle, TM Razor mantiene **tutte le funzionalità** di Razor Enhanced.

### Sistema di Scripting

| Linguaggio | Motore | Formato file |
|---|---|---|
| **Python** | IronPython 3.4 | `.py` |
| **C#** | Roslyn (Microsoft.CodeAnalysis) | `.cs` |
| **UOSteam** | Engine compatibile | `.uos` |

Tutti i linguaggi hanno accesso all'intera **API di gioco**: Player, Items, Mobiles, Journal, Target, Gumps, Spells, Skills, e molto altro.

### Agenti Automatici

| Agente | Funzione |
|---|---|
| **AutoLoot** | Raccolta automatica oggetti da cadaveri/terra con filtri |
| **Scavenger** | Raccolta oggetti con priorità e whitelist |
| **Organizer** | Organizzazione automatica dell'inventario |
| **Restock** | Rifornimento da contenitori configurati |
| **Dress** | Gestione e cambio outfit rapido |
| **AutoBandage** | Cura automatica con bende |
| **Vendor Buy** | Acquisto automatico da NPC venditori |
| **Vendor Sell** | Vendita automatica a NPC compratori |

### Sistema Macro (43 tipi di azione)

- **Controllo di flusso**: `If / ElseIf / Else / While / For` con supporto variabili e alias
- **Combattimento**: Cast spell, Attack, Set ability (Primary/Secondary), Arm/Disarm, Toggle War Mode
- **Inventario**: Pickup, Drop, Move item, Use potion, Use context menu
- **Targeting**: Target, Wait for target, Target by type, Relative location
- **UI**: Gump response, Wait for gump, Prompt response, Query string
- **Comunicazione**: Say, Emote, Whisper, Yell
- **Utility**: Pause, Mount, Movement, Fly, Disconnect, Resync, Clear journal
- **Avanzate**: Set/Remove alias, Rename mobile, Run Organizer once, Comment

### Altre funzionalità

- **Hotkey Manager** — Associa qualsiasi tasto a qualsiasi azione di gioco
- **Gump Inspector** — Analisi visuale e replay delle finestre di gioco
- **DPS Meter** — Misuratore di danni in tempo reale con statistiche
- **Journal** — Log completo e filtrabile di tutti i messaggi di gioco
- **Filtri** — Filtra suoni (morte, animali, magie), meteo, luci, messaggi, spam
- **Toolbar personalizzabile** — Barra degli strumenti rapida configurabile
- **Screenshot / Video** — Cattura schermo automatica o manuale con formati multipli
- **Proto-Control (gRPC/WebSocket)** — Controllo remoto degli script da applicazioni esterne
- **Pathfinding** — Navigazione automatica sulla mappa verso coordinate o oggetti
- **SpellGrid** — Griglia incantesimi personalizzabile per accesso rapido
- **Skill Lock** — Gestione dei lock delle abilità
- **Friend List** — Gestione lista amici per targeting selettivo
- **Packet Logger** — Debug avanzato del traffico di rete

---

## 🏗️ Struttura del progetto

```
TMRazor/
├── Razor/                          # Progetto principale C# (RazorEnhanced.exe)
│   ├── RazorEnhanced/             # Funzionalità enhanced
│   │   ├── Macros/                # Sistema macro (43 tipi di azione)
│   │   │   ├── Macro.cs           # Classe macro con loop e stato
│   │   │   ├── MacroManager.cs    # Gestione esecuzione macro
│   │   │   ├── MacroAction.cs     # Classe base azioni
│   │   │   └── Actions/           # 43 implementazioni di azione
│   │   └── UI/                    # UI Enhanced
│   │       ├── EnhancedLauncher   # Launcher TM Razor con selezione lingua
│   │       ├── LanguageHelper.cs  # Sistema traduzione italiano/inglese
│   │       └── EnhancedScriptEditor # Editor script con syntax highlight
│   ├── Core/                      # Core del client (Player, Item, Mobile, World)
│   ├── Network/                   # Gestione pacchetti di rete
│   ├── Client/                    # Supporto ClassicUO, OSI, UOAssist
│   ├── UI/                        # Interfaccia principale
│   │   ├── Razor.cs               # Form principale (MainForm)
│   │   ├── MacrosUI.cs            # Editor macro UI
│   │   ├── Languages.cs           # Enum LocString (100+ stringhe)
│   │   └── Controls/              # Componenti UI custom
│   │       ├── RazorTheme.cs      # Sistema colori e font
│   │       ├── RazorCard.cs       # Pannello con bordi arrotondati
│   │       ├── RazorButton.cs     # Pulsante tematizzato
│   │       ├── RazorToggle.cs     # Toggle switch moderno
│   │       ├── RazorSidebarTab.cs # Tab sidebar animata
│   │       ├── RazorComboBox.cs   # ComboBox tematizzata
│   │       ├── RazorTextBox.cs    # TextBox tematizzata
│   │       └── RazorTabControl.cs # TabControl tematizzato
│   ├── Filters/                   # Filtri contenuto
│   └── Enums/                     # Tipi enumerativi
├── Crypt/                         # DLL C++ per crittografia di rete UO
├── Loader/                        # DLL C++ per integrazione con il client
├── UltimaSDK/                     # SDK per dati Ultima Online
├── FastColoredTextBox/            # Editor di codice con syntax highlighting
└── BuildSupplementalFiles/        # File supplementari per la build
    ├── Language/                  # File di localizzazione (Razor_lang.ENU)
    ├── Scripts/                   # Script di esempio
    └── Config/                    # Template di configurazione
```

---

## 🔧 Build dal sorgente

Per compilare TM Razor dal sorgente sono necessari:

- **Visual Studio 2022** (o MSBuild 17+)
- **.NET Framework 4.8 Developer Pack**
- **C++ Build Tools** (per Crypt.dll e Loader.dll)

```bash
# Clona il repository
git clone https://github.com/FabiodAgostino/TMRazor.git
cd TMRazor

# Apri la soluzione in Visual Studio
start Razor.sln

# oppure compila da riga di comando
msbuild Razor.sln /p:Configuration=Release /p:Platform=Win32
```

L'output compilato si trova in `bin/Win32/Release/`.

---

## 📜 Licenza

Questo progetto è distribuito sotto la stessa licenza del progetto originale Razor Enhanced.
Vedi il file [LICENSE.txt](LICENSE.txt) per i dettagli completi.

Razor Enhanced è un progetto open-source della community Ultima Online.
TM Razor è una fork non ufficiale, sviluppata indipendentemente per la community di **The Miracle Shard**.

---

## 🙏 Crediti

- **[Razor Enhanced Team](https://github.com/RazorEnhanced/RazorEnhanced)** — per il fantastico progetto base su cui TM Razor è costruito
- **Bryan Pass (Zippy)** — creatore del Razor originale (RunUO community)
- **[Alexdan2](https://github.com/alexdan) & MagnetoStaff** — sviluppatori storici di Razor Enhanced (2015–2019)
- **[Jaedan](https://github.com/jaedan)** — aggiornamento del progetto per Visual Studio 2017
- **[SaschaKP](https://github.com/SaschaKP)** — ottimizzazioni performance delle collezioni
- **The Miracle Staff** — per lo shard, la community e l'ispirazione

---

<div align="center">

*TM Razor — Fatto con ❤️ per la community di The Miracle Shard*

</div>
