# Final Review — TMRazor Legacy → TMRazorImproved Migration Audit

> **Autore**: AI Architecture Review
> **Data**: 2026-03-22
> **Scopo**: Mappatura esaustiva 1:1 di ogni file, classe e metodo tra il codice legacy (`Razor/`) e il nuovo codice (`TMRazorImproved/`) per garantire che nessuna funzionalita venga persa.
> **Destinatari**: Senior Architect (validazione) + Junior Developer (esecuzione dei Task)

---

## Legenda Stato

| Simbolo | Significato |
|---------|-------------|
| :white_check_mark: | Migrato correttamente, parita funzionale |
| :warning: | Parzialmente migrato / incompleto |
| :x: | Mancante nel nuovo codice |
| :no_entry: | Discrepanza strutturale (approccio diverso, potenziale perdita di feature) |
| :bulb: | Nuovo nel codice TMRazorImproved (non presente nel legacy) |

---

## Indice

1. [Core Entity Models](#1-core-entity-models)
2. [Packet Handling & Network](#2-packet-handling--network)
3. [Filters](#3-filters)
4. [Enhanced Services & Agents](#4-enhanced-services--agents)
5. [Scripting Engine](#5-scripting-engine)
6. [Scripting API](#6-scripting-api)
7. [Macro System](#7-macro-system)
8. [UI Components](#8-ui-components)
9. [Utility & Support Files](#9-utility--support-files)
10. [Riepilogo Priorita](#10-riepilogo-priorita)

---

## 1. Core Entity Models

### 1.1 UOEntity (Base Class)

**Legacy**: `Razor/Core/UOEntity.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Shared/Models/UOEntity.cs`

| ID | Membro Legacy | Stato | Dettaglio | Impatto Utente |
|----|---------------|-------|-----------|----------------|
| TASK-ENT-001 | `UOEntity.ContextMenu` (`Dictionary<ushort, int>`) | :x: Mancante | Il dictionary dei context menu non esiste nel nuovo UOEntity | Script/agent che leggono le voci del context menu di un NPC non funzioneranno |
| TASK-ENT-002 | `UOEntity.Body` (alias di TypeID) | :x: Mancante | Nel nuovo codice esiste solo `Graphic`. Il legacy usa `Body` come alias per il grafico del mobile | Script che usano `entity.Body` falliranno |
| TASK-ENT-003 | `UOEntity.TypeID` (struct con accesso a `ItemData`) | :no_entry: Discrepanza | Legacy ha una struct `TypeID` che accede a Ultima.dll per tile flags, peso, qualita, CalcHeight. Nuovo ha solo `ushort Graphic` | Perdita accesso dati tiles (flags, peso). Accettabile se ClassicUO fornisce equivalenti |
| TASK-ENT-004 | `UOEntity.OnPositionChanging()` (virtual hook) | :x: Mancante | Hook chiamato prima del cambio posizione. Usato per rimuovere entita fuori range | Entita fuori range non vengono ripulite al cambio posizione |
| TASK-ENT-005 | `UOEntity.ReadPropertyList(PacketReader)` | :no_entry: Discrepanza | Legacy parsa OPL dal pacchetto direttamente sull'entita. Nuovo gestisce OPL in `WorldPacketHandler` | Nessun impatto diretto — architettura diversa ma funzionalita preservata |

**Azione Junior per TASK-ENT-001**: Aprire `TMRazorImproved.Shared/Models/UOEntity.cs`. Nella classe base `UOEntity`, aggiungere `public Dictionary<ushort, int> ContextMenu { get; set; } = new();`. Poi cercare in `WorldPacketHandler.cs` dove arriva il pacchetto 0xBF sub-command context menu e popolare questo dictionary.

**Azione Junior per TASK-ENT-002**: Aprire `TMRazorImproved.Shared/Models/UOEntity.cs`, classe `Mobile`. Aggiungere `public ushort Body { get => Graphic; set => Graphic = value; }` come alias.

**Azione Junior per TASK-ENT-004**: Considerare l'aggiunta di un setter personalizzato per `X`, `Y`, `Z` nelle proprietà di `UOEntity` che invochi un metodo virtuale `OnPositionChanging()`.

---

### 1.2 Mobile

**Legacy**: `Razor/Core/Mobile.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Shared/Models/UOEntity.cs` (classe `Mobile`)

| ID | Membro Legacy | Stato | Dettaglio | Impatto Utente |
|----|---------------|-------|-----------|----------------|
| TASK-MOB-001 | `Mobile.Contains` (`List<Item>`) con `AddItem`/`RemoveItem` | :no_entry: Discrepanza | Nuovo ha solo `EquippedItemSerials` (`List<uint>`). Non si possono ottenere riferimenti `Item` diretti | `GetItemOnLayer`, `FindItemByID`, `Backpack`, `Quiver` dipendono tutti da riferimenti oggetto |
| TASK-MOB-002 | `Mobile.GetItemOnLayer(Layer)` | :x: Mancante | Cerca negli items equipaggiati per layer | Script/agent che controllano equipaggiamento (mani, zaino, cavalcatura) non funzionano |
| TASK-MOB-003 | `Mobile.Quiver` | :x: Mancante | Ritorna il container nel layer mantello | Feature auto-search/auto-loot nella faretra rotta |
| TASK-MOB-004 | `Mobile.FindItemByID(TypeID)` | :x: Mancante | Cerca items equipaggiati per graphic | Non si puo trovare un item specifico su un mobile |
| TASK-MOB-005 | `Mobile.GetNotorietyColor()` | :x: Mancante | Ritorna colore RGB per notorieta | Colorazione nomi overhead dei mobili mancante |
| TASK-MOB-006 | `Mobile.GetStatusCode()` | :x: Mancante | Ritorna byte di stato (1=avvelenato) | Display stato mancante |
| TASK-MOB-007 | `Mobile.GetPacketFlags()` / `ProcessPacketFlags()` | :x: Mancante | Costruisce/parsa il byte flag 0x78 (paralizzato, femmina, avvelenato, benedetto, warmode, visibile) | Flag non decodificati correttamente dai pacchetti di movimento |
| TASK-MOB-008 | `Mobile.OverheadMessage()` (6 overload) | :x: Mancante | Invia messaggi overhead al client | Script che usano `Mobile.OverheadMessage()` falliranno |
| TASK-MOB-009 | `Mobile.Remove()` (cascading) | :x: Mancante | Rimozione in cascata degli items equipaggiati + check party | Items rimangono orfani quando un mobile viene rimosso |
| TASK-MOB-010 | `Mobile.OnNotoChange()` | :x: Mancante | Hook virtuale per cambio notorieta (timer criminale nel Player) | Timer criminale non scatta al cambio notorieta |
| TASK-MOB-011 | `Mobile.OnMapChange()` | :x: Mancante | Hook virtuale per cambio mappa | Pulizia range al cambio mappa non attivata |
| TASK-MOB-012 | `Mobile.NameToFameKarma` (dictionary statico) | :x: Mancante | Mappa titoli karma -> valori fama/karma | Risoluzione titolo-karma -> valore rotta |
| TASK-MOB-013 | `Mobile.StatsUpdated` / `PropsUpdated` (flag di tracking) | :x: Mancante | Distingue "zero hits" da "stats mai ricevute" | Non si puo distinguere se le stats sono state ricevute o meno |
| TASK-MOB-014 | `Mobile.InParty` (property) | :no_entry: Discrepanza | Legacy controlla `PacketHandlers.Party`. Nuovo traccia party in `WorldService` | Usare `WorldService.IsPartyMember()` — nessun impatto se implementato |
| TASK-MOB-015 | `Mobile.ButtonPoint` | :x: Mancante | Posizione 2D per bottone toolbar | Bassa priorita — tracking posizione toolbar per mobili specifici |

**Azione Junior per TASK-MOB-001**: Questo e il gap piu critico del modello entita. Bisogna decidere se memorizzare `List<Item>` nell'entita oppure implementare lookup via `WorldService`. Approccio consigliato: aggiungere un metodo `GetItemOnLayer(byte layer)` al `WorldService` che cerca in `_items` un item il cui `ContainerSerial == mobile.Serial && Layer == layer`. Poi wrappare in property computate su `Mobile`.

**Azione Junior per TASK-MOB-002**: Implementare `GetItemOnLayer(byte layer)` come descritto sopra. Questo sblocca TASK-MOB-003 (Quiver = layer Cloak/Waist container) e TASK-MOB-004.

**Azione Junior per TASK-MOB-007**: In `WorldPacketHandler.cs`, dentro `HandleMobileIncoming` (0x78) e `HandleMobileUpdate` (0x20), leggere il byte dei flag e impostare le proprieta: `mobile.Paralyzed = (flags & 0x01) != 0; mobile.Female = (flags & 0x02) != 0;` ecc. Riferimento: `Razor/Core/Mobile.cs` righe 200-230.

**Azione Junior per TASK-MOB-008**: Implementare come metodo helper in `PacketBuilder` o nella scripting API. Il metodo deve costruire un pacchetto 0xAE (UnicodeMessage) o 0x1C (AsciiMessage) e inviarlo al client via `_packetService.SendToClient()`.

**Azione Junior per TASK-MOB-009**: In `WorldService.RemoveMobile(serial)`, prima di rimuovere il mobile, iterare su `EquippedItemSerials` e rimuovere ogni item da `_items`.

---

### 1.3 Item

**Legacy**: `Razor/Core/Item.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Shared/Models/UOEntity.cs` (classe `Item`)

| ID | Membro Legacy | Stato | Dettaglio | Impatto Utente |
|----|---------------|-------|-----------|----------------|
| TASK-ITEM-001 | `Item.Contains` (`List<Item>`) con albero parent-child | :no_entry: Discrepanza | Nuovo ha solo `ContainedItemSerials` (`List<uint>`). Non si puo attraversare la gerarchia container | `IsChildOf`, `GetWorldPosition`, `RootContainer` calcolato — tutto dipende da riferimenti oggetto |
| TASK-ITEM-002 | `Item.Container` (riferimento oggetto con lazy resolution via `UpdateContainer()`) | :no_entry: Discrepanza | Nuovo ha solo `uint ContainerSerial` | Perdita risoluzione parent-child e aggiornamenti cascata container |
| TASK-ITEM-003 | `Item.RootContainer` (calcolato, attraversa gerarchia) | :no_entry: Discrepanza | Nuovo ha `uint RootContainer` come auto-property semplice (non calcolata) | Il valore deve essere mantenuto manualmente; script che si affidano al root-finding automatico riceveranno dati stantii |
| TASK-ITEM-004 | `Item.IsChildOf(parent)` (attraversa albero fino a 100 livelli) | :x: Mancante | Check contenimento zaino/faretra rotto | Script/agent non possono verificare se un item e dentro un container specifico |
| TASK-ITEM-005 | `Item.GetWorldPosition()` (risale al root container) | :x: Mancante | Items dentro container riportano la grid-position, non la world-position | Coordinate errate per items in container |
| TASK-ITEM-006 | `Item.Amount` (getter complesso con check stackability via TileFlag) | :no_entry: Discrepanza | Nuovo ha auto-property semplice | Items non-stackable potrebbero riportare quantita errate |
| TASK-ITEM-007 | `Item.Layer` (auto-risoluzione via `ItemData.Quality`) | :no_entry: Discrepanza | Nuovo ha `byte Layer` semplice | Layer non auto-risolto per items con layer invalido dal server |
| TASK-ITEM-008 | `Item.IsContainer` (check via `ContainersData.json` + conteggio items) | :no_entry: Discrepanza | Nuovo ha `bool` auto-property semplice | Deve essere impostato esternamente nel packet handler |
| TASK-ITEM-009 | `Item.IsCorpse` (check TypeID == 0x2006 o range 0x0ECA-0x0ED2) | :no_entry: Discrepanza | Nuovo ha `bool` auto-property semplice | Deve essere impostato nel packet handler alla creazione item |
| TASK-ITEM-010 | `Item.IsDoor` (check via `DoorData.json`) | :no_entry: Discrepanza | Nuovo ha `bool` auto-property | Deve essere impostato esternamente |
| TASK-ITEM-011 | `Item.IsLootable` (check via `NotLootableData.json`, esclude capelli/barbe) | :no_entry: Discrepanza | Nuovo ha `bool` (default true) | Capelli/barbe erroneamente marcati come lootable |
| TASK-ITEM-012 | `Item.IsResource` (range grafici hardcoded per minerali/pesci/legno/granito/sabbia) | :no_entry: Discrepanza | Nuovo ha `bool` auto-property | Deve essere impostato esternamente |
| TASK-ITEM-013 | `Item.IsPotion` (range grafici 0x0F06-0x0F0D + cintura ninja) | :no_entry: Discrepanza | Nuovo ha `bool` auto-property | Deve essere impostato esternamente |
| TASK-ITEM-014 | `Item.IsTwoHanded` (logica complessa con `weapons.json` + OPL) | :no_entry: Discrepanza | Nuovo ha `bool` auto-property | Deve essere impostato esternamente |
| TASK-ITEM-015 | `Item.Factory()` (crea `MapItem` o `CorpseItem` in base al graphic) | :x: Mancante | Nessun sottotipo `MapItem` (pin mappa/mappe del tesoro) o `CorpseItem` (auto-open corpse) | Feature mappe del tesoro e auto-open corpse mancanti |
| TASK-ITEM-016 | `Item.Weapon` / `Weapons` static (carica `weapons.json`) | :x: Mancante | Nessun dato abilita armi (primaria/secondaria) | Abilita speciali armi non disponibili |
| TASK-ITEM-017 | `Item.AutoStackResource()` | :x: Mancante | Auto-impila risorse a terra | Feature auto-stack rotta |
| TASK-ITEM-018 | `Item.Remove()` (cascading child removal) | :x: Mancante | Items figli orfani quando container rimossi | Items fantasma nel world state |
| TASK-ITEM-019 | `Item.HouseRevision` / `HousePacket` | :x: Mancante | Tracking revisione multi/case | Bassa priorita |

**Azione Junior per TASK-ITEM-001/002/003/004/005**: Questi sono tutti interconnessi. Approccio consigliato:
1. In `WorldService.cs`, aggiungere metodo `GetRootContainer(uint serial)` che risale la catena `ContainerSerial` fino al primo item senza container o al mobile.
2. Aggiungere metodo `IsChildOf(uint itemSerial, uint parentSerial, int maxDepth = 100)` che risale la catena verificando ogni container.
3. Aggiungere metodo `GetWorldPosition(uint itemSerial)` che risale al root container e usa la sua posizione.
4. Questi metodi risolvono TUTTI i task ITEM-001 attraverso ITEM-005.

**Azione Junior per TASK-ITEM-008/009/010/011/012/013**: In `WorldPacketHandler.cs`, nei metodi `HandleWorldItem` e `HandleSAWorldItem`, dopo aver creato l'`Item`, impostare le proprietà booleane in base al graphic:
```csharp
item.IsCorpse = item.Graphic == 0x2006 || (item.Graphic >= 0x0ECA && item.Graphic <= 0x0ED2);
item.IsPotion = (item.Graphic >= 0x0F06 && item.Graphic <= 0x0F0D);
// ecc.
```
Per `IsContainer` e `IsDoor`, caricare le tabelle da file JSON come nel legacy.

**Azione Junior per TASK-ITEM-018**: In `WorldService.RemoveItem(serial)`, prima di rimuovere l'item, se ha `ContainedItemSerials.Count > 0`, rimuovere ricorsivamente tutti i figli.

---

### 1.4 Player (PlayerData)

**Legacy**: `Razor/Core/Player.cs`
**Nuovo**: Appiattito nella classe `Mobile` + servizi vari

| ID | Membro Legacy | Stato | Dettaglio | Impatto Utente |
|----|---------------|-------|-----------|----------------|
| TASK-PLR-001 | `PlayerData.MoveReq/MoveAck/MoveRej` (coda movimento completa) | :x: Mancante | Coda movimento con tracking sequenza walk, auto-open-doors, conteggio passi stealth | Predizione/validazione movimento rotta; auto-open-doors non funzionante; contatore stealth mancante |
| TASK-PLR-002 | `PlayerData.Menu state` (`CurrentMenuS/I`, `HasMenu`, `MenuQuestionText`, `MenuEntry`) | :x: Mancante | Menu popup vecchio stile (pacchetto 0x7C) non gestiti | Menu popup dei server custom non funzionano |
| TASK-PLR-003 | `PlayerData.DoubleClick()` (logica complessa) | :x: Mancante | Check equip pozione, mano libera, `ActionQueue.DoubleClick` | Auto-equip pozioni (mano libera prima di bere) non implementato |
| TASK-PLR-004 | `PlayerData.LastWeaponRight/Left` | :x: Mancante | Tracking per hotkey ri-equip arma | Macro ri-equip arma rotta |
| TASK-PLR-005 | `PlayerData.CriminalTime/CriminalTimer` | :x: Mancante | Timer countdown stato criminale (5 min) | Display timer criminale rotto |
| TASK-PLR-006 | `PlayerData.VisRange` default | :no_entry: Discrepanza | Legacy: 31 per player, 18 per NPC. Nuovo: `VisRange = 18` su Mobile | Default errato; 31 e specifico per il player |
| TASK-PLR-007 | `PlayerData.MaxWeight` (auto-calc da STR: `(STR * 3.5) + 40`) | :no_entry: Discrepanza | Nuovo ha auto-property semplice | Peso non auto-calcolato |
| TASK-PLR-008 | `PlayerData.ForcedSeason` | :x: Mancante | Override stagione server | Feature override stagione rotta |
| TASK-PLR-009 | `PlayerData.LocalLightLevel/GlobalLightLevel` | :no_entry: Discrepanza | Nuovo ha solo `CurrentLight` in WorldService (no local vs global) | Feature override luce parzialmente mancanti |
| TASK-PLR-010 | `PlayerData.LastSkill/LastSpell` | :x: Mancante | Tracking per replay macro | Solo `LastObject` e `AttackTarget` presenti su Mobile |
| TASK-PLR-011 | `PlayerData.Max Resistance caps` (MaxPhysic/Fire/Cold/Poison/Energy) | :x: Mancante | Cap resistenze | Display cap resistenze rotto |
| TASK-PLR-012 | `PlayerData.MaxDefenseChanceIncrease` | :x: Mancante | Cap DCI | Display cap DCI rotto |
| TASK-PLR-013 | `PlayerData.QueryString*` state | :x: Mancante | Stato prompt query string | Prompt query string dei shard custom non gestiti |

**Azione Junior per TASK-PLR-001**: Questo e un gap MAGGIORE. Creare un nuovo servizio `MovementService.cs` in `TMRazorImproved.Core/Services/` che gestisca la coda di movimento. Riferimento completo: `Razor/Core/Player.cs` sezione `MoveReq`/`MoveAck`/`MoveRej`. Deve tracciare la sequenza walk, gestire auto-open-doors e contare i passi stealth.

**Azione Junior per TASK-PLR-006**: In `WorldPacketHandler.cs`, nel metodo `HandleLoginConfirm`, impostare `player.VisRange = 31`.

**Azione Junior per TASK-PLR-007**: In `Mobile`, cambiare `MaxWeight` da auto-property a property calcolata: `public ushort MaxWeight => _maxWeightOverride > 0 ? _maxWeightOverride : (ushort)((Str * 3.5) + 40);`

---

### 1.5 World

**Legacy**: `Razor/Core/World.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/WorldService.cs`

| ID | Membro Legacy | Stato | Dettaglio | Impatto Utente |
|----|---------------|-------|-----------|----------------|
| TASK-WLD-001 | `World.Multis` (`ConcurrentDictionary<int, MultiData>`) | :x: Mancante | Tracking multi/case non supportato | Feature case non disponibili |
| TASK-WLD-002 | `World.FindItems(x,y,z)` | :x: Mancante | Ricerca items per posizione specifica | Ricerca items per posizione non funziona |
| TASK-WLD-003 | `World.FindAllEntityByID(type, color)` | :x: Mancante | Ricerca entita per graphic + hue | `Items.FindByID` / `Mobiles.FindByID` dello scripting API rotti |
| TASK-WLD-004 | `World.CorpsesInRange(range)` | :x: Mancante | Trova cadaveri nel range | Feature auto-open-corpse rotta |
| TASK-WLD-005 | `World.ShardName/OrigPlayerName/AccountName` | :x: Mancante | Metadati server | Display nome shard, switch profili |

**Azione Junior per TASK-WLD-002/003/004**: Aggiungere i seguenti metodi a `WorldService.cs`:
```csharp
public IEnumerable<Item> FindItems(int x, int y, int z) => _items.Values.Where(i => i.X == x && i.Y == y && i.Z == z);
public IEnumerable<UOEntity> FindAllByGraphicAndHue(ushort graphic, ushort hue) => ...;
public IEnumerable<Item> CorpsesInRange(int range) => _items.Values.Where(i => i.IsCorpse && Distance(i) <= range);
```

---

### 1.6 Serial

**Legacy**: `Razor/Core/Serial.cs`
**Nuovo**: `uint` semplice ovunque

| ID | Membro Legacy | Stato | Dettaglio | Impatto Utente |
|----|---------------|-------|-----------|----------------|
| TASK-SER-001 | `Serial.IsMobile` / `Serial.IsItem` / `Serial.IsValid` | :x: Mancante | Non si puo distinguere serial mobile vs item | Confusione tipo entita |
| TASK-SER-002 | `Serial.Parse(string)` | :x: Mancante | Parsa "0x..." hex o decimale | Parsing serial negli script rotto |

**Azione Junior per TASK-SER-001/002**: Creare una classe statica `SerialHelper` in `TMRazorImproved.Shared/Utilities/`:
```csharp
public static class SerialHelper {
    public static bool IsMobile(uint serial) => serial > 0 && serial < 0x40000000;
    public static bool IsItem(uint serial) => serial >= 0x40000000 && serial <= 0x7FFFFF00;
    public static bool IsValid(uint serial) => serial > 0 && serial <= 0x7FFFFF00;
    public static uint Parse(string s) => s.StartsWith("0x") ? uint.Parse(s[2..], System.Globalization.NumberStyles.HexNumber) : uint.Parse(s);
}
```

---

### 1.7 Geometry

**Legacy**: `Razor/Core/Geometry.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Shared/Models/Geometry.cs`

| ID | Membro Legacy | Stato | Dettaglio | Impatto Utente |
|----|---------------|-------|-----------|----------------|
| TASK-GEO-001 | `Point3D.Parse(string)` | :x: Mancante | Parsa "(x, y, z)" | Script API `Point3D.Parse` rotto |
| TASK-GEO-002 | `Point3D` operatori +/- | :x: Mancante | Operatori aritmetici per math posizioni | Calcoli posizione negli script rotti |
| TASK-GEO-003 | `Line2D` struct completa | :x: Mancante | Nessun supporto geometria linee | Controlli geometrici avanzati mancanti |
| TASK-GEO-004 | `Rectangle2D.Contains(Rectangle2D)` / `MakeHold()` | :x: Mancante | Check contenimento regioni | Controlli regione rotti |

---

### 1.8 Facet

**Legacy**: `Razor/Core/Facet.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Shared/Models/Facet.cs`

| ID | Membro Legacy | Stato | Dettaglio | Impatto Utente |
|----|---------------|-------|-----------|----------------|
| TASK-FAC-001 | `Facet.GetMap(int)` (ritorna `Ultima.Map`) | :x: Mancante | Nessuna dipendenza Ultima.dll | Impatto su pathfinding/ZTop; probabilmente gestito internamente da ClassicUO |
| TASK-FAC-002 | `Facet.ZTop()` / `GetAverageZ()` | :x: Mancante | Calcolo Z camminabile alla posizione | Non necessario se CUO gestisce il movimento |

---

### 1.9 Buffs

**Legacy**: `Razor/Core/Buffs.cs` + `Razor/Core/BuffInfo.cs`
**Nuovo**: `Mobile.ActiveBuffs` dictionary + `WorldService._buffNames`

| ID | Membro Legacy | Stato | Dettaglio | Impatto Utente |
|----|---------------|-------|-----------|----------------|
| TASK-BUF-001 | `BuffIcon` enum (170+ entries) | :warning: Incompleto | Nuovo ha solo ~48 entries in `_buffNames` | Molte icone buff non riconosciute (Sleep, StoneForm, SpellPlague, tutte le Skill Masteries 1169+) |
| TASK-BUF-002 | `BuffInfo` class (Duration, StartTime, Cliloc*) | :no_entry: Discrepanza | Nuovo ha solo `Dictionary<string, int>` (nome -> secondi) | Persi: tempo inizio buff, descrizioni cliloc, info tooltip dettagliate |

**Azione Junior per TASK-BUF-001**: Aprire `WorldPacketHandler.cs`, metodo che gestisce 0xDF (HandleBuffDebuff). Espandere la mappa `_buffNames` copiando tutti i 170+ valori dall'enum `BuffIcon` in `Razor/Core/Buffs.cs`.

---

### 1.10 Spells

**Legacy**: `Razor/Core/Spells.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Shared/Models/SpellDefinitions.cs`

| ID | Membro Legacy | Stato | Dettaglio | Impatto Utente |
|----|---------------|-------|-----------|----------------|
| TASK-SPL-001 | `Spell.Flag` (Beneficial/Harmful/Neutral) | :x: Mancante | Nessun flag B/H/N per spell nel nuovo `SpellInfo` | Smart targeting (harm/bene) non puo cercare tipo spell |
| TASK-SPL-002 | `Spell.WordsOfPower` | :x: Mancante | Parole di potere per spell | Display/detection parole potere rotto |
| TASK-SPL-003 | `Spell.Reagents` | :x: Mancante | Reagenti richiesti per spell | Conteggio/display reagenti rotto |
| TASK-SPL-004 | `Spell.GetHue()` | :x: Mancante | Hue configurato per tipo spell | Feature override hue spell rotta |
| TASK-SPL-005 | `Spell.Cast()` con auto-unequip mani | :x: Mancante | SpellUnequip feature | Auto-disarm prima del cast rotto |
| TASK-SPL-006 | `spells.def` file loading | :no_entry: Discrepanza | Legacy carica da `spells.def` (estensibile). Nuovo ha lista hardcoded | Estensibilita persa (spell custom server via .def file) |

**Azione Junior per TASK-SPL-001/002/003**: Aprire `TMRazorImproved.Shared/Models/SpellDefinitions.cs`. Per ogni `SpellInfo`, aggiungere i campi:
```csharp
public SpellFlag Flag { get; init; } // enum { Beneficial, Harmful, Neutral }
public string WordsOfPower { get; init; }
public string[] Reagents { get; init; }
```
Popolare i dati copiando da `Razor/Core/Spells.cs` (o dal file `spells.def`).

---

## 2. Packet Handling & Network

### 2.1 Architettura

| Aspetto | Legacy | Nuovo | Stato |
|---------|--------|-------|-------|
| Classe Packet | `Packet` con `MemoryStream`, `Read*/Write*`, `Compile()` | Raw `byte[]` + `UOBufferReader` + `BinaryPrimitives` | :no_entry: Approccio diverso, funzionalmente equivalente |
| Tabella lunghezze pacchetti | `PacketTable.cs` hardcoded `short[255]` | Protocollo length-prefix (4 byte LE) | :no_entry: Cambio architetturale intenzionale |
| Registrazione handler | `ConcurrentDictionary<int, List<callback>>` statiche | DI-based `ConcurrentDictionary<(PacketPath, int), List<Action<byte[]>>>` | :white_check_mark: |
| Packet builder | Classi packet nominate (`EmoteAction`, `QueryPartyLocs`, etc.) | Array byte inline | :no_entry: Funzionalmente equivalente |

### 2.2 Confronto Handler S2C (Server-to-Client)

| Packet ID | Handler Legacy | Handler Nuovo | Stato |
|-----------|---------------|---------------|-------|
| 0x0B | Damage | HandleDamage | :white_check_mark: |
| 0x11 | MobileStatus | HandleMobileStatus | :white_check_mark: |
| 0x16 | SAMobileStatus | HandleSAMobileStatus | :white_check_mark: |
| 0x17 | NewMobileStatus | HandleNewMobileStatus | :white_check_mark: |
| 0x1A | WorldItem (Viewer) | HandleWorldItem (**Filter**) | :no_entry: Promosso a filter (puo bloccare staff items) |
| 0x1B | LoginConfirm | HandleLoginConfirm | :white_check_mark: |
| 0x1C | AsciiSpeech (**Filter**) | HandleAsciiMessage (**Viewer**) | :no_entry: Downgrade a viewer; filtering spostato in FilterHandler |
| 0x1D | RemoveObject | HandleRemoveObject | :white_check_mark: |
| 0x20 | MobileUpdate (Filter) | HandleMobileUpdate (Filter) | :white_check_mark: |
| 0x21 | MovementRej | HandleWalkReject | :white_check_mark: |
| 0x22 | MovementAck | HandleMovementAck | :white_check_mark: |
| 0x24 | BeginContainerContent | HandleBeginContainerContent | :white_check_mark: |
| 0x25 | ContainerContentUpdate (Filter) | HandleAddItemToContainer (Filter) | :white_check_mark: |
| 0x27 | LiftReject | HandleLiftReject | :white_check_mark: |
| 0x2C | MyDeath | HandlePlayerDeath | :white_check_mark: |
| 0x2D | MobileStatInfo | HandleMobileStatInfo | :white_check_mark: |
| 0x2E | EquipmentUpdate (Filter) | HandleEquipUpdate (Filter) | :white_check_mark: |
| 0x3A | Skills | HandleSkillsUpdate | :white_check_mark: |
| 0x3C | ContainerContent (Filter) | HandleContainerContent (Filter) | :white_check_mark: |
| 0x4E | PersonalLight | HandlePersonalLightLevel | :white_check_mark: |
| 0x4F | GlobalLight | HandleGlobalLightLevel | :white_check_mark: |
| 0x56 | PinLocation | HandlePinLocation | :white_check_mark: |
| 0x6F | TradeRequest | HandleTradeRequest | :white_check_mark: |
| 0x72 | ServerSetWarMode | HandleWarMode | :white_check_mark: |
| 0x73 | PingResponse | HandlePing | :white_check_mark: |
| 0x74 | StoreBuyList | HandleBuyWindow | :white_check_mark: |
| 0x76 | ServerChange | HandleServerChange | :white_check_mark: |
| 0x77 | MobileMoving (**Filter**) | HandleMobileMoving (**Viewer**) | :no_entry: Downgrade da filter a viewer |
| 0x78 | MobileIncoming (Filter) | HandleMobileIncoming (Filter) | :white_check_mark: |
| 0x7C | SendMenu | HandleOpenMenu | :white_check_mark: |
| 0x88 | OpenPaperdoll | HandleOpenPaperdoll | :white_check_mark: |
| 0x90 | MapDetails | HandleMapDisplay | :white_check_mark: |
| 0x95 | HueResponse | HandleHueResponse | :white_check_mark: |
| 0x97 | MovementDemand | HandleMovementDemand | :white_check_mark: |
| 0x9A | AsciiPromptResponse | HandleAsciiPrompt | :white_check_mark: |
| 0x9E | StoreSellList | HandleSellWindow | :white_check_mark: |
| 0xA1 | HitsUpdate | HandleHitsUpdate | :white_check_mark: |
| 0xA2 | ManaUpdate | HandleManaUpdate | :white_check_mark: |
| 0xA3 | StamUpdate | HandleStaminaUpdate | :white_check_mark: |
| 0xA8 | ServerList | HandleServerList | :white_check_mark: |
| 0xAB | DisplayStringQuery | HandleDisplayStringQuery | :white_check_mark: |
| 0xAE | UnicodeSpeech (**Filter**) | HandleUnicodeMessage (**Viewer**) | :no_entry: Downgrade a viewer; filtering in FilterHandler |
| 0xAF | DeathAnimation | HandleDeathAnimation | :white_check_mark: |
| 0xB0 | SendGump + **GumpIgnore** (2 viewer) | HandleGump (1 viewer) | :warning: Callback GumpIgnore mancante |
| 0xB8 | Profile | HandleProfile | :white_check_mark: |
| 0xB9 | Features | HandleFeatures | :white_check_mark: |
| 0xBA | TrackingArrow | HandleTrackingArrow | :white_check_mark: |
| 0xBC | ChangeSeason | HandleChangeSeason | :white_check_mark: |
| 0xBF | ExtendedPacket | HandleExtendedPacket | :white_check_mark: |
| 0xC1 | LocalizedMessage (**Filter**) | HandleLocalizedMessage (**Viewer**) | :no_entry: Downgrade a viewer; filtering in FilterHandler |
| 0xC2 | UnicodePromptReceived (S2C) | **MANCANTE** | :x: Nessun handler S2C per 0xC2 |
| 0xC8 | SetUpdateRange (Filter) | HandleSetUpdateRange (Filter) | :white_check_mark: |
| 0xCC | LocalizedMessageAffix (**Filter**) | HandleLocalizedMessageAffix (**Viewer**) | :no_entry: Downgrade a viewer |
| 0xD6 | EncodedPacket (OPL) | HandleOPL | :white_check_mark: |
| 0xD8 | CustomHouseInfo | HandleCustomHouseInfo | :white_check_mark: |
| 0xDD | CompressedGump + **GumpIgnore** | HandleCompressedGump | :warning: Callback GumpIgnore mancante |
| 0xDF | BuffDebuff + **BandageHeal.BuffDebuff** | HandleBuffDebuff | :warning: Callback BandageHeal.BuffDebuff mancante |
| 0xE2 | TestAnimation | HandleTestAnimation | :white_check_mark: |
| 0xF0 | RunUOProtocolExtention | HandleRunUOProtocol | :white_check_mark: |
| 0xF3 | SAWorldItem (Viewer) | HandleSAWorldItem (**Filter**) | :no_entry: Promosso a filter |
| 0xF5 | MapDetails | HandleMapDisplay | :white_check_mark: |
| 0xF6 | MoveBoatHS | HandleMoveBoat | :white_check_mark: |

### 2.3 Confronto Handler C2S (Client-to-Server)

| Packet ID | Handler Legacy | Handler Nuovo | Stato |
|-----------|---------------|---------------|-------|
| 0x00 | CreateCharacter | **MANCANTE** | :x: |
| 0x02 | MovementRequest (Filter) | HandleMovementRequest (Filter) | :white_check_mark: |
| 0x05 | AttackRequest (Filter) | HandleAttackRequest (Filter) | :white_check_mark: |
| 0x06 | ClientDoubleClick | HandleClientDoubleClick | :white_check_mark: |
| 0x07 | LiftRequest | HandleLiftRequest | :white_check_mark: |
| 0x08 | DropRequest | HandleDropRequest | :white_check_mark: |
| 0x09 | ClientSingleClick | HandleClientSingleClick | :white_check_mark: |
| 0x12 | ClientTextCommand | HandleClientTextCommand | :white_check_mark: |
| 0x13 | EquipRequest | HandleEquipRequest | :white_check_mark: |
| 0x22 | ResyncRequest | HandleResyncRequest | :white_check_mark: |
| 0x3A | SetSkillLock | HandleSetSkillLockC2S | :white_check_mark: |
| 0x5D | PlayCharacter | HandlePlayCharacter | :white_check_mark: |
| 0x6F | TradeRequestFromClient | HandleTradeRequestC2S | :white_check_mark: |
| 0x75 | RenameMobile | HandleRenameMobile | :white_check_mark: |
| 0x7D | MenuResponse | HandleMenuResponseC2S | :white_check_mark: |
| 0x80 | ServerListLogin (**Filter**) | HandleServerListLogin (**Viewer**) | :no_entry: Downgrade intenzionale — password gestite da PasswordService |
| 0x91 | GameLogin (**Filter**) | HandleGameLogin (**Viewer**) | :no_entry: Downgrade intenzionale |
| 0x95 | HueResponse | HandleHueResponseC2S | :white_check_mark: |
| 0x9A | AsciiPromptResponse | HandleAsciiPromptResponseC2S | :white_check_mark: |
| 0xA0 | PlayServer | HandlePlayServer | :white_check_mark: |
| 0xAC | ResponseStringQuery | HandleResponseStringQueryC2S | :white_check_mark: |
| 0xB1 | ClientGumpResponse | HandleGumpResponse | :white_check_mark: |
| 0xBF | ExtendedClientCommand (**Filter**) | HandleExtendedClientCommand (**Viewer**) | :no_entry: Downgrade |
| 0xC2 | UnicodePromptSend | HandleUnicodePromptResponseC2S | :white_check_mark: |
| 0xD7 | ClientEncodedPacket | HandleClientEncodedPacket | :white_check_mark: |
| 0xF8 | CreateCharacter v2 | **MANCANTE** | :x: |

### 2.4 Handler mancanti — Task

| ID | Packet | Direzione | Dettaglio | Impatto | Priorita |
|----|--------|-----------|-----------|---------|----------|
| TASK-PKT-001 | 0xC2 | S2C | UnicodePromptReceived — nessun handler S2C | Prompt unicode dal server non gestiti | Media |
| TASK-PKT-002 | 0x00 | C2S | CreateCharacter — tracking creazione personaggio | Perdita tracking creazione char | Bassa |
| TASK-PKT-003 | 0xF8 | C2S | CreateCharacter v2 — come sopra, versione nuova | Perdita tracking creazione char | Bassa |
| TASK-PKT-004 | 0xB0/0xDD | S2C | GumpIgnore callback secondaria mancante | Feature GumpIgnore non funzionante | Media |
| TASK-PKT-005 | 0xDF | S2C | BandageHeal.BuffDebuff callback secondaria mancante | Timing BandageHeal da pacchetti buff/debuff mancante | Alta |

**Azione Junior per TASK-PKT-001**: In `WorldPacketHandler.cs`, aggiungere un viewer S2C per 0xC2 che legga serial e ID del prompt e li salvi nel `TargetingService` (come `_hasPrompt`, `_promptSerial`, `_promptId`).

**Azione Junior per TASK-PKT-004**: Nel metodo che gestisce 0xB0 e 0xDD, aggiungere un check per i gump da ignorare (caricare la lista ignora dalla config e confrontare il gumpId).

**Azione Junior per TASK-PKT-005**: Nel metodo `HandleBuffDebuff`, aggiungere una chiamata a `BandageHealService.OnBuffDebuff(buffId, isAdded)` per il timing del bandage heal.

---

### 2.5 Nuovi handler in TMRazorImproved (non nel legacy)

| Packet ID | Dir | Handler | Note |
|-----------|-----|---------|------|
| 0x55 | S2C | HandleLoginComplete | :bulb: Segnale completamento login |
| 0x65 | S2C | HandleWeather | :bulb: Pacchetto meteo (era solo nei Filters) |
| 0x6C | S2C | HandleTargetCursorFromServer | :bulb: Gestione cursore target |
| 0x6D | S2C | HandlePlayMusic | :bulb: Riproduzione musica |
| 0x6E | S2C | HandleCharacterAnimation | :bulb: Animazioni personaggio |
| 0x83 | S2C | HandleDeleteCharacter | :bulb: Eliminazione personaggio |
| 0x89 | S2C | HandleCorpseEquipment | :bulb: Lista equipaggiamento cadavere |
| 0x8C | S2C | HandleRelayServer | :bulb: Relay login |
| 0x98 | S2C | HandleMobileName | :bulb: Richiesta nome mobile |
| 0xAA | S2C | HandleAttackOK | :bulb: Conferma attacco |
| 0xAD | S2C | HandleEncodedUnicodeSpeech | :bulb: Discorso unicode codificato |
| 0xBD | S2C | HandleClientVersion | :bulb: Negoziazione versione |
| 0xC0 | S2C | HandleGraphicalEffect | :bulb: Effetti grafici |
| 0x54 | S2C | HandlePlaySoundEffect | :bulb: Effetti sonori |

---

## 3. Filters

**Legacy**: `Razor/Filters/` (13 file)
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Handlers/FilterHandler.cs`

| Filter Legacy | Nuovo | Stato |
|---------------|-------|-------|
| `Death.cs` (blocca 0x2C) | Filtro 0x2C registrato | :white_check_mark: |
| `Light.cs` (blocca 0x4E, 0x4F) | Filtri 0x4E, 0x4F registrati | :white_check_mark: |
| `Weather.cs` (blocca 0x65) | Filtro 0x65 registrato | :white_check_mark: |
| `Season.cs` (blocca/riscrive 0xBC) | Filtro 0xBC + SendForcedSeason | :white_check_mark: |
| `SoundFilters.cs` (blocca 0x54 per ID) | Filtro 0x54 con lista ID | :white_check_mark: |
| `MessageFilter.cs` (AsciiMessageFilter) | Filtro 0x1C con match testo | :white_check_mark: |
| `MessageFilter.cs` (LocMessageFilter) | Filtro 0xC1 con lista cliloc | :white_check_mark: |
| `VetRewardGump.cs` (blocca 0xB0/0xDD) | Filtri 0xB0, 0xDD con check contenuto | :white_check_mark: |
| `StaffItems.cs` (blocca 0x1A per ID) | Filtro 0x1A check 0x36FF/0x1183 | :white_check_mark: |
| `StaffNpcs.cs` (blocca 0x78/0x20/0x77 per flags) | Filtri 0x78, 0x20, 0x77 check byte flag | :white_check_mark: |
| `MobileFilter.cs` (riscrive body ID) | Filtri grafici su 0x78/0x20 | :white_check_mark: |
| `WallStaticFilter.cs` (riscrive wall item + **label**) | `MorphGraphic()` in WorldPacketHandler | :warning: Grafico OK ma **label mancanti** |
| `TargetFilterManager.cs` | `TargetFilterService.cs` | :white_check_mark: |

| ID | Feature Filter Mancante | Dettaglio | Impatto |
|----|-------------------------|-----------|---------|
| TASK-FLT-001 | Label Wall Static Field | Legacy `WallStaticFilter` invia un pacchetto `UnicodeMessage` follow-up con label come "[Wall Of Stone]", "[Fire Field]" quando `ShowStaticWallLabels` e abilitato. Il nuovo `MorphGraphic()` cambia solo grafico/hue — nessun pacchetto label inviato | Giocatori non vedranno le label sui campi magici |

**Nuovi filtri in TMRazorImproved** (non nel legacy):
- :bulb: Filtro Footsteps (0x54 con ID suono passi 0x12-0x1A)
- :bulb: Filtro BardMusic (0x6D)
- :bulb: Filtro Block Trade Request (0x6F)
- :bulb: Filtro Block Party Invite (0xBF sub-0x06/0x07 con auto-decline)
- :bulb: Filtro per tipo messaggio (`OverheadMessageType` + `FilteredMessageStrings` configurabili)

---

## 4. Enhanced Services & Agents

### 4.1 AutoLoot

**Legacy**: `Razor/RazorEnhanced/AutoLoot.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/AutoLootService.cs`

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-001 | `GetLootBag()` con ricerca ricorsiva sub-container + waist layer | :warning: Incompleto | Nuovo cerca solo `_worldService.FindItem(config.Container)` — non cerca sub-container o waist layer | Items nella belly pouch non trovati |
| TASK-AGT-002 | Per-item loot bag override (`LootBagOverride`) | :x: Mancante | Ogni item nella lista puo avere il suo container destinazione. Nuovo `LootItem` non ha override per-item | Tutto il loot va in un singolo container |
| TASK-AGT-003 | Auto-open corpses before looting | :x: Mancante | Legacy invia `Items.WaitForContents` per aprire cadaveri prima del loot | Cadaveri non aperti, loot perso |

**Azione Junior per TASK-AGT-003**: In `AutoLootService.cs`, prima di iterare gli items del cadavere, aggiungere una chiamata per aprire il container (inviare doppio-click al serial del cadavere tramite `_packetService`) e attendere il pacchetto 0x3C (ContainerContent).

---

### 4.2 BandageHeal

**Legacy**: `Razor/RazorEnhanced/BandageHeal.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/BandageHealService.cs`

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-004 | `SearchBandage()` cerca belly pouch (waist layer container) | :x: Mancante | Nuovo cerca solo backpack | Bende nella pouch non trovate |
| TASK-AGT-005 | Warning "Low bandage: X left" overhead | :x: Mancante | Nessun warning bende scarse | Utente non avvisato |
| TASK-AGT-006 | Ghost check (`World.Player.IsGhost`) bail-out | :x: Mancante | Nuovo non controlla se player e ghost | Tentativo di fasciare da ghost |

---

### 4.3 Dress

**Legacy**: `Razor/RazorEnhanced/Dress.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/DressService.cs`

**Stato**: :white_check_mark: Parita funzionale buona. Nuovo aggiunge tracking stato dress/undress separato (FR-041).

---

### 4.4 Organizer

**Legacy**: `Razor/RazorEnhanced/Organizer.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/OrganizerService.cs`

**Stato**: :white_check_mark: Parita funzionale buona. Nuovo aggiunge `RunOnce()` e evento `OnComplete`.

---

### 4.5 Restock

**Legacy**: `Razor/RazorEnhanced/Restock.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/RestockService.cs`

**Stato**: :white_check_mark: Parita funzionale buona. Nuovo aggiunge FR-043 `RunOnce()` e FR-044 color filter.

---

### 4.6 Scavenger

**Legacy**: `Razor/RazorEnhanced/Scavenger.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/ScavengerService.cs`

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-007 | Check `MaxWeight` con messaggio utente quando sovrappeso | :x: Mancante | Nuovo non controlla peso (DragDropCoordinator ritorna false silenziosamente) | Scavenger tenta di raccogliere items quando sovrappeso senza avvisare |
| TASK-AGT-008 | `IsLootableTarget` check per saltare cadaveri | :x: Mancante | Nuovo non ha questo check | Potrebbe tentare di raccogliere cadaveri |

---

### 4.7 Vendor

**Legacy**: `Razor/RazorEnhanced/Vendor.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/VendorService.cs`

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-009 | `LastResellList` (tracking separato per layer rivendita vendor) | :x: Mancante | Nuovo traccia solo buy list e sell list | Script che accedono a `Vendor.LastResellList` non avranno equivalente |

---

### 4.8 Friend

**Legacy**: `Razor/RazorEnhanced/Friend.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/FriendsService.cs`

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-010 | `AutoacceptParty` (auto-accetta inviti party da amici) | :x: Mancante logica | `FriendsConfig` ha il campo ma nessun handler implementa l'auto-accept | Feature auto-accept party non funzionante |
| TASK-AGT-011 | `PreventAttack` (previene attacco ad amici) | :x: Mancante logica | Nessun filtro pacchetto per bloccare attacchi ai target amici | Feature prevenzione attacco amici non funzionante |

**Azione Junior per TASK-AGT-010**: In `FriendsHandler.cs` o in un nuovo filtro nel `PacketService`, registrare un filtro sul pacchetto 0xBF sub-command party-invite. Se il serial del mittente e nella lista amici e `AutoacceptParty` e abilitato, inviare automaticamente il pacchetto di accettazione party.

**Azione Junior per TASK-AGT-011**: Registrare un filtro C2S sul pacchetto 0x05 (AttackRequest). Se il serial target e nella lista amici e `PreventAttack` e abilitato, bloccare il pacchetto.

---

### 4.9 DPSMeter

**Legacy**: `Razor/RazorEnhanced/DPSMeter.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/DPSMeterService.cs`

**Stato**: :white_check_mark: Nuovo e piu ricco. Aggiunge rolling-window DPS, MaxDPS, CombatTime, pause/resume, breakdown per-target.

---

### 4.10 DragDrop

**Legacy**: `Razor/RazorEnhanced/DragDropManager.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/DragDropCoordinator.cs`

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-012 | `CorpseToCutSerial` queue (auto-cut cadaveri per risorse) | :x: Mancante | Nessuna integrazione corpse-cutting nel drag-drop flow. `BoneCutterService` e `AutoCarverService` esistono separatamente ma non collegati | Auto-cut cadaveri non funzionante nel flusso drag-drop |
| TASK-AGT-013 | `AutoLootSerialCorpseRefresh` (auto-open cadaveri prima loot) | :x: Mancante | `AutoLootService` non invia `WaitForContents` per aprire cadaveri | Cadaveri non refreshati, loot perso |

---

### 4.11 HotKey

**Legacy**: `Razor/RazorEnhanced/HotKey.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/HotkeyService.cs`

**Stato**: :white_check_mark: Nuovo e significativamente piu ricco con hook keyboard/mouse a basso livello globale (FR-090).

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-014 | `MasterKey` (tasto on/off separato per tutti gli hotkey) | :x: Mancante | Nuovo ha `Hotkey:Toggle` ma nessun "master key" distinto | Trascurabile |

---

### 4.12 Journal

**Legacy**: `Razor/RazorEnhanced/Journal.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/JournalService.cs`

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-015 | Per-script journal instances (`new Journal(size)`) | :no_entry: Discrepanza | Nuovo ha un singolo `JournalService` globale con una coda. Legacy: ogni script ha il suo snapshot delle entries dal momento dell'attivazione | Script che creano multiple istanze Journal per tracciare finestre temporali diverse non funzioneranno correttamente |

**Azione Junior per TASK-AGT-015**: Nella `JournalApi` (scripting), aggiungere un sistema di "snapshot" per script: quando uno script chiama `Journal.Clear()`, salvare un timestamp. Le ricerche successive filtrano solo entries dopo quel timestamp. Questo emula il comportamento legacy senza modificare il service globale.

---

### 4.13 Target

**Legacy**: `Razor/RazorEnhanced/Target.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/TargetingService.cs`

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-016 | `WaitForTargetOrFizzle(delay)` | :x: Mancante | Attende cursore target O suono pacchetto 0x54 (fizzle) | Script PvP che usano questo metodo devono essere adattati |
| TASK-AGT-017 | `PromptAlias` (target per alias string) | :x: Mancante | Target aliased | Feature target per alias mancante |

---

### 4.14 SpellGrid

**Legacy**: `Razor/RazorEnhanced/SpellGrid.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.UI/Views/Windows/SpellGridWindow.xaml` + ViewModel

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-018 | SpellGrid floating panel con bottoni spell configurabili, icone abilita, stati highlight | :warning: Incompleto | Esiste `SpellGridConfig` in Shared e la finestra XAML, ma manca il service di backend che gestisce il grid, le icone e gli highlight | **ALTO** — SpellGrid e uno strumento di combattimento molto usato |

---

### 4.15 SpecialMoves

**Legacy**: `Razor/RazorEnhanced/SpecialMoves.cs`
**Nuovo**: Scripting API `SpecialMovesApi.cs`

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-019 | Mappa completa weapon-to-ability (centinaia di grafici armi -> abilita primary/secondary) | :x: Mancante | Nuovo invia solo pacchetti toggle generici senza sapere quale abilita specifica | SpellGrid non puo mostrare icone abilita corrette |
| TASK-AGT-020 | `PrimaryGumpId` / `SecondaryGumpId` | :x: Mancante | ID icona delle abilita dell'arma corrente per display SpellGrid | Display icone abilita rotto |

---

### 4.16 Config/Settings/Profiles

**Legacy**: `Razor/RazorEnhanced/Config.cs` + `Settings.cs` + `Profiles.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/ConfigService.cs`

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-021 | Profile auto-detection per serial personaggio (`Profiles.PlayerEntry.PlayerSerial`) | :x: Mancante | Profili auto-switch al login non implementato | Utenti con multipli personaggi non avranno auto-switch profilo |

---

### 4.17 SeasonManager

**Legacy**: `Razor/RazorEnhanced/SeasonManager.cs`
**Nuovo**: **MANCANTE INTERAMENTE**

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-022 | Carica `seasons.txt` per remappare grafici tile per stagione | :x: Mancante | Nuovo `StaticsApi` nota "To perfectly match TMRazor we'd need SeasonManager" ma ritorna valori base | Basso — solo shards con stagioni custom |

---

### 4.18 PathFinding

**Legacy**: `Razor/RazorEnhanced/PathFinding.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/PathFindingService.cs`

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-023 | `PathMove.RunPath()` (auto-walk un percorso) | :x: Mancante | Nessun equivalente nel nuovo service | Script che chiamano `PathMove.RunPath()` devono implementare il movimento manualmente |

---

### 4.19 CUO Integration

**Legacy**: `Razor/RazorEnhanced/CUO.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/Scripting/Api/CuoApi.cs`

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-024 | `CUO.LoadMarkers()`, `GoToMarker()`, `FreeView()`, `CloseTMap()`, `ProfilePropertySet()`, `GetSetting()`, `SetGumpOpenLocation()`, `MoveGump()` | :warning: Tutti stub | Legacy usa reflection in-process per chiamare interni ClassicUO. Nuovo logga warning e ritorna no-op (out-of-process) | **Medio** — Script CUO falliranno silenziosamente |

---

### 4.20 UOSteam Engine

**Legacy**: `Razor/RazorEnhanced/UOSteamEngine.cs`
**Nuovo**: UOSteamInterpreter

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-025 | Famiglia comandi `gumplist` (create, activate, delete, move, get, set, print) | :warning: Stub | Tutti i comandi gumplist sono stub | Script UOSteam con manipolazione gump list falliranno |
| TASK-AGT-026 | Comando `findwand` | :warning: Stub | Stub | Script UOSteam con `findwand` falliranno |

---

### 4.21 ScriptRecorder

**Legacy**: `Razor/RazorEnhanced/ScriptRecorder.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/ScriptRecorderService.cs`

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-027 | `CsScriptRecorder` (output recording in C#) | :x: Mancante | Nuovo registra in Python e UOSteam ma non C# | Output recording C# non disponibile |

---

### 4.22 CommandService

**Legacy**: `Razor/Core/Commands.cs`
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/CommandService.cs`

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-AGT-028 | Comandi `-bandage`, `-dress`, `-undress`, `-organizer`, `-restock`, `-scavenger` | :x: Mancanti | Questi comandi text non sono nel nuovo `CommandService` | Utenti che usano comandi text per attivare agenti non potranno farlo |

---

### 4.23 Servizi completamente migrati (nessuna discrepanza)

| Legacy | Nuovo | Stato |
|--------|-------|-------|
| `Razor/RazorEnhanced/Trade.cs` | `SecureTradeService.cs` | :white_check_mark: |
| `Razor/RazorEnhanced/Skills.cs` | `SkillsService.cs` | :white_check_mark: |
| `Razor/RazorEnhanced/EncodedSpeech.cs` | `EncodedSpeechHelper.cs` | :white_check_mark: |
| `Razor/RazorEnhanced/AutoDoc.cs` | `AutoDocService.cs` | :white_check_mark: |
| `Razor/RazorEnhanced/Shards.cs` | `ShardService.cs` | :white_check_mark: |
| `Razor/RazorEnhanced/Statics.cs` | `StaticsApi.cs` | :white_check_mark: |
| `Razor/RazorEnhanced/Multi.cs` | `MultiService.cs` | :white_check_mark: |
| `Razor/RazorEnhanced/Property.cs` | Gestito in `WorldPacketHandler` | :white_check_mark: |
| `Razor/RazorEnhanced/CircularBuffer.cs` | `System.Threading.Channels` | :white_check_mark: |
| `Razor/RazorEnhanced/SyncPrimitives.cs` | `SemaphoreSlim` built-in | :white_check_mark: |
| `Razor/RazorEnhanced/JsonData.cs` | `HotkeyConfig` in `ConfigModels.cs` | :white_check_mark: |

---

## 5. Scripting Engine

### 5.1 Differenze Architetturali

| Feature | Legacy | Nuovo | Stato |
|---------|--------|-------|-------|
| Python engine | IronPython 3.4, `ScriptRuntime` persistente | IronPython 3.4, engine fresco per esecuzione | :no_entry: |
| C# engine | CodeDom/csc.exe, trova metodo `Run()` | Roslyn `CSharpScript.RunAsync` con `ScriptGlobals` | :no_entry: |
| Cancellazione script | `Thread.Abort()` (deprecato .NET 5+) | `CancellationToken` + `Thread.Interrupt()` + `sys.settrace` | :white_check_mark: Migliore |
| Script concorrenti | Multipli script simultaneamente | Uno alla volta (`SemaphoreSlim(1,1)`) | :no_entry: **Regressione** |
| Hot-reload | Per-file `FileSystemWatcher` | Per-directory `FileSystemWatcher` | :white_check_mark: |
| Suspend/Resume | `ManualResetEvent` per script | `volatile bool _isSuspended` controllato in trace | :white_check_mark: |
| Binding hotkey per-script | Hotkey con pass-through per script | Delegato a `IHotkeyService` | :white_check_mark: |
| Liste script | 3 liste separate (Py/Cs/Uos) | Discovery basata su filesystem | :no_entry: Approccio diverso |
| Thread agent | Thread dedicati per AutoLoot, Scavenger, etc. | Delegati alle rispettive interfacce service | :white_check_mark: |
| Debug | Nessun debug | Breakpoint, step, REPL | :bulb: Nuova feature |

| ID | Discrepanza | Impatto |
|----|-------------|---------|
| TASK-SCR-001 | **Script concorrenti non supportati** — legacy permette multipli script Python/C#/UOS simultanei, nuovo esegue uno alla volta | **ALTO** — Utenti che eseguono script paralleli (es. un bot combat + un bot loot) non potranno farlo |

**Azione Junior per TASK-SCR-001**: Modificare `ScriptingService.cs` per usare `SemaphoreSlim(N, N)` dove N > 1, oppure gestire una coda di script con thread pool dedicato. Riferimento: `Razor/RazorEnhanced/EnhancedScript.cs` per il pattern di esecuzione multi-thread.

---

## 6. Scripting API

### 6.1 Mobiles API

**Legacy**: `Razor/RazorEnhanced/Mobile.cs` (API scripting)
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/Scripting/Api/MobilesApi.cs` + `Wrappers.cs`

**Stato**: :white_check_mark: **Parita completa**. Tutti i metodi legacy hanno controparte. Il nuovo aggiunge molti metodi di convenienza (`FindNearest*`, `FilterBy*`, `GetHealthPercent`, etc.).

---

### 6.2 Items API

**Legacy**: `Razor/RazorEnhanced/Item.cs` (API scripting)
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/Scripting/Api/ItemsApi.cs` + `Wrappers.cs`

**Stato**: :white_check_mark: **Parita completa**.

| ID | Discrepanza minore | Dettaglio |
|----|-------------------|-----------|
| TASK-API-001 | `Items.GetProperties(serial, delay)` — versione con delay | Nuovo ha `GetProperties` senza delay (ritorna cached) |

---

### 6.3 Player API

**Legacy**: `Razor/RazorEnhanced/Player.cs` (API scripting)
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/Scripting/Api/PlayerApi.cs`

**Stato**: :white_check_mark: **Parita completa**.

---

### 6.4 Spells API

**Legacy**: `Razor/RazorEnhanced/Spells.cs` (API scripting)
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/Scripting/Api/SpellsApi.cs`

**Stato**: :white_check_mark: **Parita completa**. Nuovo aggiunge `IsCasting`, `WaitCast`, `WaitForMana`, `HasManaToCast`, etc.

---

### 6.5 Target API

**Stato**: :white_check_mark: **Parita completa** (incluso `WaitForTargetOrFizzle`).

---

### 6.6 Journal API

**Stato**: :white_check_mark: **Parita completa**. Nuovo aggiunge `InJournalRegex`, `SearchJournal`, etc.

---

### 6.7 Gumps API

**Stato**: :white_check_mark: **Parita completa**. Nuovo aggiunge `IsGumpVisible`, `GetTextEntry`, `GetSwitches`, etc.

---

### 6.8 Misc API

**Stato**: :white_check_mark: **Parita completa**. Nuovo aggiunge `WaitFor`, `Log`, `Timestamp`, `Random`, liste UOS-compatibili (`CreateList`, `PushList`, etc.).

---

### 6.9 DPSMeter API

**Stato**: :white_check_mark: Nuovo e piu ricco (`GetCurrentDPS`, `GetMaxDPS`, `GetTotalDamage`, `GetCombatTime`, etc.).

---

### 6.10 CUO API — Stub

| ID | Metodo | Stato | Note |
|----|--------|-------|------|
| TASK-API-002 | `CUO.LoadMarkers()` | :warning: Stub | Logga warning, no-op |
| TASK-API-003 | `CUO.GoToMarker()` | :warning: Stub | Logga warning, no-op |
| TASK-API-004 | `CUO.FreeView()` | :warning: Stub | Logga warning, no-op |
| TASK-API-005 | `CUO.ProfilePropertySet()` (3 overload) | :warning: Stub | Logga warning, no-op |
| TASK-API-006 | `CUO.GetSetting()` | :warning: Stub | Logga warning, no-op |
| TASK-API-007 | `CUO.SetGumpOpenLocation()` | :warning: Stub | Logga warning, no-op |
| TASK-API-008 | `CUO.MoveGump()` | :warning: Stub | Logga warning, no-op |
| TASK-API-009 | `CUO.CloseTMap()` | :warning: Stub | Logga warning, no-op |

> **Nota architetturale**: Questi stub sono inevitabili perche TMRazorImproved gira out-of-process rispetto a TmClient. Per implementarli servirebbe un canale IPC aggiuntivo plugin→UI per invocare API interne di ClassicUO.

---

## 7. Macro System

### 7.1 Architettura

**Legacy**: `MacroManager` statico + `MacroAction` classi + 43 classi azione concrete
**Nuovo**: `MacrosService` DI-based, formato testuale, engine program-counter con jump tables

### 7.2 Confronto azioni macro

**Stato**: :white_check_mark: **Tutte le 43 azioni legacy hanno handler corrispondente** nel `MacrosService.ExecuteActionAsync()`.

| Azione Legacy | Comando Nuovo | Stato |
|---------------|---------------|-------|
| ArmDisarmAction | `ARMDISARM` | :white_check_mark: |
| AttackAction | `ATTACK` | :white_check_mark: |
| BandageAction | `BANDAGE` | :white_check_mark: |
| CastSpellAction | `CAST` | :white_check_mark: |
| ClearJournalAction | `CLEARJOURNAL` | :white_check_mark: |
| CommentAction | `//` o `#` | :white_check_mark: |
| DisconnectAction | `DISCONNECT` | :white_check_mark: |
| DoubleClickAction | `DOUBLECLICK` / `DCLICK` | :white_check_mark: |
| DropAction | `DROP` | :white_check_mark: |
| ElseAction | `ELSE` | :white_check_mark: |
| ElseIfAction | `ELSEIF` | :white_check_mark: |
| EndForAction | `ENDFOR` | :white_check_mark: |
| EndIfAction | `ENDIF` | :white_check_mark: |
| EndWhileAction | `ENDWHILE` | :white_check_mark: |
| FlyAction | `FLY` / `LAND` | :white_check_mark: |
| ForAction | `FOR` | :white_check_mark: |
| GumpResponseAction | `RESPONDGUMP` / `RESPONDGUMPEX` | :white_check_mark: |
| IfAction | `IF` | :white_check_mark: |
| InvokeVirtueAction | `INVOKEVIRTUE` | :white_check_mark: |
| MessagingAction | `SAY`/`MSG`/`EMOTE`/`WHISPER`/`YELL` | :white_check_mark: |
| MountAction | `MOUNT` / `DISMOUNT` | :white_check_mark: |
| MoveItemAction | `MOVEITEM` | :white_check_mark: |
| MovementAction | `WALK` / `RUN` / `PATHFIND` | :white_check_mark: |
| PauseAction | `PAUSE` / `WAIT` | :white_check_mark: |
| PickUpAction | `PICKUP` | :white_check_mark: |
| PromptResponseAction | `PROMPTRESPONSE` | :white_check_mark: |
| QueryStringResponseAction | `QUERYSTRINGRESPONSE` | :white_check_mark: |
| RemoveAliasAction | `REMOVEALIAS` | :white_check_mark: |
| RenameMobileAction | `RENAMEMOBILE` | :white_check_mark: |
| ResyncAction | `RESYNC` | :white_check_mark: |
| RunOrganizerOnceAction | `RUNORGANIZER` | :white_check_mark: |
| SetAbilityAction | `SETABILITY` | :white_check_mark: |
| SetAliasAction | `SETALIAS` | :white_check_mark: |
| TargetAction | `TARGET`/`TARGETSELF`/`TARGETLAST`/etc. | :white_check_mark: |
| TargetResourceAction | `TARGETRESOURCE` | :white_check_mark: |
| ToggleWarModeAction | `WARMODE` | :white_check_mark: |
| UseContextMenuAction | `USECONTEXTMENU` | :white_check_mark: |
| UseEmoteAction | `EMOTE` | :white_check_mark: |
| UsePotionAction | `USEPOTIONTYPE` | :white_check_mark: |
| UseSkillAction | `USESKILL` | :white_check_mark: |
| WaitForGumpAction | `WAITFORGUMP` | :white_check_mark: |
| WaitForTargetAction | `WAITFORTARGET` | :white_check_mark: |
| WhileAction | `WHILE` | :white_check_mark: |

**Comandi aggiuntivi nel nuovo** (non nel legacy):
- :bulb: `SINGLECLICK`
- :bulb: `USETYPE`
- :bulb: `EQUIPITEM`
- :bulb: `WAITFORMENU`
- :bulb: `MENURESPONSE`

---

## 8. UI Components

### 8.1 Mappatura Pagine

| # | Legacy | Nuovo | Stato |
|---|--------|-------|-------|
| 1 | `Razor/UI/Razor.cs` (main form) | `MainWindow.xaml` | :white_check_mark: |
| 2 | `SplashScreen.cs` | WPF startup in `App.xaml.cs` | :white_check_mark: |
| 3 | `Languages.cs` | `Strings.resx` + `LocExtension` | :white_check_mark: |
| 4 | AutoLoot tab | `AutoLootPage.xaml` + VM | :white_check_mark: |
| 5 | BandageHeal tab | `BandageHealPage.xaml` + VM | :white_check_mark: |
| 6 | Dress tab | `DressPage.xaml` + VM | :white_check_mark: |
| 7 | Organizer tab | `OrganizerPage.xaml` + VM | :white_check_mark: |
| 8 | Restock tab | `RestockPage.xaml` + VM | :white_check_mark: |
| 9 | Scavenger tab | `ScavengerPage.xaml` + VM | :white_check_mark: |
| 10 | VendorBuy + VendorSell | `VendorPage.xaml` + VM (combinato) | :white_check_mark: |
| 11 | Friends tab | `FriendsPage.xaml` + VM | :white_check_mark: |
| 12 | Skills tab | `SkillsPage.xaml` + VM | :white_check_mark: |
| 13 | Hotkey tab | `HotkeysPage.xaml` + VM | :white_check_mark: |
| 14 | Profile tab | Integrato in `GeneralPage.xaml` | :white_check_mark: |
| 15 | Script tab | `ScriptingPage.xaml` + VM | :warning: Redesign significativo |
| 16 | Target tab | `TargetingPage.xaml` + VM | :white_check_mark: |
| 17 | MacrosUI tab | `MacrosPage.xaml` + VM | :white_check_mark: |
| 18 | Filters tab | `FiltersPage.xaml` + VM | :warning: Feature mancanti |
| 19 | SpellGrid | `SpellGridWindow.xaml` + VM | :warning: Incompleto |
| 20 | Toolbar | `FloatingToolbarWindow.xaml` + VM | :white_check_mark: |
| 21 | EnhancedScriptEditor | Integrato in ScriptingPage (AvalonEdit) | :white_check_mark: |
| 22 | EnhancedGumpInspector | `InspectorPage.xaml` tab Gump | :white_check_mark: |
| 23-26 | Mobile/Item/Static/Object Inspector | `InspectorPage.xaml` tab Entity | :white_check_mark: |
| 27 | RE_MessageBox | `MessageDialog.xaml` | :white_check_mark: |
| 28 | EnhancedLauncher | `GeneralPage.xaml` | :white_check_mark: |
| 29 | ChangeLog | `ChangelogWindow.xaml` | :white_check_mark: |
| 30 | Custom Controls | WPF UI toolkit | :white_check_mark: |

### 8.2 Feature UI Mancanti

| ID | Feature Legacy | Stato | Dettaglio | Impatto |
|----|----------------|-------|-----------|---------|
| TASK-UI-001 | **Script List Management** (lista script con per-script hotkey, loop, autostart, preload, riordinamento) | :x: Mancante | Il nuovo ha solo un editor single-file (AvalonEdit) con New/Open/Save/Run/Stop. Nessuna lista script persistente, nessuna gestione multi-script, nessun loop/autostart/preload/hotkey per-script, nessuna separazione tab Python/UOS/C# | **ALTO** — Workflow multi-script completamente assente |
| TASK-UI-002 | **Hue Override Settings** (Force Speech Hue, System Message Hue, Warning Hue, LT Highlight Hue, Beneficial/Harmful/Neutral Spell Hue) | :x: Mancante | Zero riferimenti a `SpeechHue`, `LTHilight`, `BeneficialSpellHue`, `HarmfulSpellHue`, `NeutralSpellHue` nella nuova UI. Il `HuePickerControl` esiste ma non e collegato | **Medio** — Personalizzazione colori mancante |
| TASK-UI-003 | **Classic Combat/Targeting Tweaks** (Queue Targets, Queue Actions, Smart Last Target, Last Target Text Flags, Range Check LT, Block Dismount, Object Delay, Spell Unequip, Count Stealth Steps, Auto Open Corpses + range, Auto Open Doors + hidden, Force Game Size, Pre-AOS Status Bar) | :x: Mancante | Nessuna di queste impostazioni esiste in `OptionsPage.xaml` o `FiltersPage.xaml` | **ALTO** — Molte impostazioni di combattimento critiche |
| TASK-UI-004 | **Profile Link/Unlink** a personaggio (auto-switch al login) | :x: Mancante | Ha Create, Clone, Rename, Delete profilo ma nessun Link/Unlink player | **Medio** — Auto-switch profilo mancante |
| TASK-UI-005 | **Hotkey Master Key** (enable/disable globale tutti hotkey) | :x: Mancante | Ha action tree, set/clear individuali, pass-through. Nessun master key | **Basso** |
| TASK-UI-006 | **Journal Filter Grid** (DataGridView per configurare filtri testo journal) | :x: Mancante | Ha Graph Filters (morphing) grid ma nessun journal filter grid | **Basso** |
| TASK-UI-007 | **Enhanced Map Path Setting** | :x: Mancante | Nessuna UI per configurare percorso Enhanced Map esterna | **Basso** |
| TASK-UI-008 | **DPS Meter Filter** (min/max damage, serial filter post-stop) | :warning: Da verificare | DPSMeterWindow esiste con Start/Pause/Stop/Clear | **Basso** |
| TASK-UI-009 | **Video Playback & File Browser** | :x: Mancante | `MediaPage` ha start/stop recording e impostazioni. Nessun browser file video o playback | **Basso** |

### 8.3 Nuove pagine UI (non nel legacy)

| Pagina | Note |
|--------|------|
| :bulb: `DashboardPage.xaml` | Dashboard overview real-time |
| :bulb: `CountersPage.xaml` | Contatori items |
| :bulb: `JournalPage.xaml` | Viewer journal dedicato |
| :bulb: `SoundPage.xaml` | Gestione suoni |
| :bulb: `SecureTradePage.xaml` | Monitor trade sicuro |
| :bulb: `PacketLoggerPage.xaml` | Packet logger |
| :bulb: `DisplayPage.xaml` | Impostazioni display |
| :bulb: `MapWindow.xaml` + `MapControl.xaml` | Mappa built-in |
| :bulb: `TargetHPWindow.xaml` | Overlay HP target |
| :bulb: `OverheadMessageOverlay.xaml` | Overlay messaggi combattimento |
| :bulb: `ApiReferenceWindow.xaml` | Reference API scripting |
| :bulb: `SearchOverlay.xaml` | Ricerca globale |
| :bulb: `HuePickerWindow.xaml` + Control | Color picker |
| :bulb: Script debugger (breakpoint, step, continue) | In ScriptingPage |
| :bulb: Script recording | In ScriptingPage |
| :bulb: `EditLootItemWindow.xaml` | Editor regole loot per-item |

**Azione Junior per TASK-UI-001**: Questa e la feature UI piu grande da implementare. Creare un pannello laterale nella `ScriptingPage.xaml` con un `ListView` di script. Ogni script deve avere: nome file, stato (Running/Stopped), checkbox Loop, checkbox AutoStart, checkbox Preload, campo HotKey. Aggiungere i comandi nel `ScriptingViewModel.cs`: AddScript, RemoveScript, MoveUp, MoveDown, SetHotkey. Salvare la configurazione in `ProfileConfig.Scripts`. Riferimento pattern: `Razor/UI/Other/Script.cs`.

**Azione Junior per TASK-UI-003**: Creare una sezione "Combat & Targeting" nell'`OptionsPage.xaml` con i seguenti controlli:
- CheckBox "Queue Targets" → `ProfileConfig.QueueTargets`
- CheckBox "Smart Last Target" → `ProfileConfig.SmartLastTarget`
- CheckBox "Range Check Last Target" + NumericUpDown range → `ProfileConfig.RangeCheckLT`, `ProfileConfig.LTRange`
- CheckBox "Auto Open Corpses" + NumericUpDown range → `ProfileConfig.AutoOpenCorpses`, `ProfileConfig.CorpseRange`
- CheckBox "Auto Open Doors" → `ProfileConfig.AutoOpenDoors`
- CheckBox "Count Stealth Steps" → `ProfileConfig.CountStealthSteps`
- CheckBox "Spell Unequip" → `ProfileConfig.SpellUnequip`
- CheckBox "Block Dismount" → `ProfileConfig.BlockDismount`
Riferimento: `Razor/UI/Filter/Filters.cs` righe 30-213.

---

## 9. Utility & Support Files

### 9.1 Client Adapters

**Legacy**: `Razor/Client/` (4 file)
**Nuovo**: `TMRazorImproved/TMRazorImproved.Core/Services/Adapters/` (4 file)

| Legacy | Nuovo | Stato |
|--------|-------|-------|
| `Client.cs` (abstract base) | Architettura diversa — tutto I/O in `PacketService` + `ClientInteropService` | :no_entry: Cambio architetturale intenzionale |
| `ClassicUO.cs` | `ClassicUOAdapter.cs` (stub thin) | :white_check_mark: |
| `OSIClient.cs` | `OsiClientAdapter.cs` (stub thin) | :white_check_mark: |
| `UOAssist.cs` | `UoAssistAdapter.cs` (implementazione completa WM_USER+200) | :white_check_mark: |

### 9.2 File completamente mancanti (Legacy senza controparte)

| Legacy File | Tipo | Stato | Impatto | Priorita |
|-------------|------|-------|---------|----------|
| `Razor/Core/StealthSteps.cs` | Contatore passi stealth | :x: Mancante | Conteggio passi stealth rotto | Media |
| `Razor/Core/ActionQueue.cs` | Coda azioni generica | :x: Mancante | Nessuna coda double-click/lift/drop sequenziale oltre DragDropCoordinator | Media |
| `Razor/Core/MsgQueue.cs` | Coda messaggi in uscita | :x: Mancante | Nessun queuing messaggi | Bassa |
| `Razor/Core/TypeID.cs` | Struct wrapper con `ItemData` accessor | :x: Mancante | Perdita lookup `ItemData` da Ultima.dll | Media |
| `Razor/Core/SkillIcon.cs` | Icone skill (mastery ability) | :x: Mancante | Tracking icone skill mancante | Bassa |
| `Razor/Core/ZLib.cs` | Compressione ZLib | :x: Mancante | Non necessario se ClassicUO gestisce compressione | Bassa |
| `Razor/Core/Utility.cs` | `InRange`, `Distance`, `Offset`, helpers | :x: Mancante | Metodi utility comuni non portati | Media |
| `Razor/Core/Platform.cs` | Rilevamento piattaforma | :x: Mancante | Non necessario — WPF e solo Windows | Nessuna |
| `Razor/RazorEnhanced/SeasonManager.cs` | Remap grafici tile per stagione | :x: Mancante | Solo shards con stagioni custom | Bassa |
| `Razor/Network/UoWarper.cs` | Automazione client OSI via UO.dll COM | :x: Mancante | Specifico OSI, non necessario per ClassicUO | Nessuna |

### 9.3 File migrati correttamente (conferma parita)

| Legacy | Nuovo | Stato |
|--------|-------|-------|
| `Razor/Core/ScreenCapture.cs` | `ScreenCaptureService.cs` | :white_check_mark: Completo |
| `Razor/Core/VideoCapture.cs` | `VideoCaptureService.cs` | :white_check_mark: Completo |
| `Razor/Core/TitleBar.cs` | `TitleBarService.cs` | :white_check_mark: Completo |
| `Razor/Core/PasswordMemory.cs` | `PasswordService.cs` | :white_check_mark: Migliorato (DPAPI) |
| `Razor/Core/Timer.cs` | `Task.Delay` / async patterns | :white_check_mark: |
| `Razor/Core/DLLImport.cs` + `NativeMethods.cs` | P/Invoke inline nei servizi | :white_check_mark: |
| `Razor/Core/Main.cs` | DI container + `App.xaml.cs` | :white_check_mark: |
| `Razor/Network/PacketLogger.cs` | `PacketLoggerService.cs` | :white_check_mark: |
| `Razor/Network/Ping.cs` | In `WorldPacketHandler` con `Stopwatch` | :white_check_mark: |
| `Razor/Network/UoMod.cs` | `UOModService.cs` | :white_check_mark: |
| `Razor/RazorEnhanced/Proto-Control/*` | `ProtoControlService.cs` | :white_check_mark: |

---

## 10. Riepilogo Priorita

### :red_circle: Priorita CRITICA (bloccano feature principali)

| ID | Descrizione | Area |
|----|-------------|------|
| TASK-SCR-001 | Script concorrenti non supportati (uno alla volta vs multipli) | Scripting Engine |
| TASK-MOB-001/002 | Mobile equipped item access (GetItemOnLayer, FindItemByID, Backpack) | Core Entity |
| TASK-ITEM-001/002/003/004/005 | Container tree traversal (IsChildOf, GetWorldPosition, RootContainer) | Core Entity |
| TASK-PLR-001 | Movement queue (MoveReq/MoveAck/MoveRej, auto-open-doors, stealth steps) | Player |
| TASK-UI-001 | Script List Management (multi-script workflow con hotkey, loop, autostart) | UI |
| TASK-UI-003 | Classic Combat/Targeting Tweaks (queue targets, smart LT, auto-open, spell unequip) | UI |
| TASK-AGT-003/013 | Auto-open corpses before looting | AutoLoot |
| TASK-PKT-005 | BandageHeal.BuffDebuff callback mancante | Packet Handler |

### :orange_circle: Priorita ALTA (feature utente significative)

| ID | Descrizione | Area |
|----|-------------|------|
| TASK-SER-001/002 | Serial helpers (IsMobile/IsItem/IsValid/Parse) | Core |
| TASK-MOB-007 | ProcessPacketFlags (paralyzed/female/poisoned/blessed/warmode/visible) | Core Entity |
| TASK-MOB-008 | OverheadMessage (6 overload) | Core Entity |
| TASK-MOB-009 + TASK-ITEM-018 | Cascading remove (Mobile.Remove + Item.Remove) | Core Entity |
| TASK-SPL-001/002/003 | Spell flags (B/H/N), WordsOfPower, Reagents | Spells |
| TASK-BUF-001 | Buff icon table incompleta (~48 vs 170+) | Buffs |
| TASK-AGT-010/011 | Friend auto-accept party + prevent attack | Friends |
| TASK-AGT-015 | Per-script journal instances | Journal |
| TASK-AGT-018 | SpellGrid service backend | SpellGrid |
| TASK-AGT-019/020 | Weapon-ability mapping + icons | SpecialMoves |
| TASK-UI-002 | Hue Override Settings | UI |
| TASK-UI-004 | Profile Link/Unlink a personaggio | UI |
| TASK-PKT-001 | 0xC2 S2C handler mancante | Packet |
| TASK-PKT-004 | GumpIgnore callback mancante | Packet |

### :yellow_circle: Priorita MEDIA

| ID | Descrizione | Area |
|----|-------------|------|
| TASK-ENT-001 | ContextMenu dictionary su UOEntity | Core Entity |
| TASK-ENT-002 | Body alias per Graphic | Core Entity |
| TASK-MOB-005 | GetNotorietyColor | Mobile |
| TASK-MOB-010/011 | OnNotoChange/OnMapChange hooks | Mobile |
| TASK-MOB-012 | NameToFameKarma lookup table | Mobile |
| TASK-PLR-002 | Menu state (pacchetto 0x7C) | Player |
| TASK-PLR-004 | LastWeaponRight/Left tracking | Player |
| TASK-PLR-005 | Criminal timer | Player |
| TASK-PLR-007 | MaxWeight auto-calc da STR | Player |
| TASK-PLR-010 | LastSkill/LastSpell tracking | Player |
| TASK-PLR-011/012 | Max Resistance caps / MaxDCI | Player |
| TASK-WLD-002/003/004 | FindItems, FindAllByGraphicAndHue, CorpsesInRange | World |
| TASK-WLD-005 | ShardName/OrigPlayerName/AccountName | World |
| TASK-GEO-001/002 | Point3D.Parse, operatori +/- | Geometry |
| TASK-AGT-001 | AutoLoot: ricerca sub-container + waist layer | AutoLoot |
| TASK-AGT-002 | AutoLoot: per-item loot bag override | AutoLoot |
| TASK-AGT-004 | BandageHeal: ricerca belly pouch | BandageHeal |
| TASK-AGT-007 | Scavenger: check weight | Scavenger |
| TASK-AGT-016 | WaitForTargetOrFizzle | Target |
| TASK-AGT-023 | PathMove.RunPath() auto-walk | PathFinding |
| TASK-AGT-024 | CUO API stub (LoadMarkers, GoToMarker, etc.) | CUO |
| TASK-AGT-028 | Comandi text agent mancanti | Commands |
| TASK-FLT-001 | Label Wall Static Field | Filters |

### :green_circle: Priorita BASSA

| ID | Descrizione | Area |
|----|-------------|------|
| TASK-ENT-004 | OnPositionChanging hook | Core Entity |
| TASK-MOB-006 | GetStatusCode | Mobile |
| TASK-MOB-013 | StatsUpdated/PropsUpdated flags | Mobile |
| TASK-MOB-015 | ButtonPoint | Mobile |
| TASK-PLR-003 | DoubleClick logica complessa (auto-equip pozione) | Player |
| TASK-PLR-006 | VisRange default (31 vs 18) | Player |
| TASK-PLR-008 | ForcedSeason | Player |
| TASK-PLR-009 | Local vs Global LightLevel | Player |
| TASK-PLR-013 | QueryString state | Player |
| TASK-WLD-001 | Multis tracking | World |
| TASK-GEO-003/004 | Line2D, Rectangle2D.Contains | Geometry |
| TASK-FAC-001/002 | GetMap, ZTop, GetAverageZ | Facet |
| TASK-BUF-002 | BuffInfo rich object | Buffs |
| TASK-SPL-004/005/006 | GetHue, auto-unequip cast, spells.def loading | Spells |
| TASK-ITEM-006-014 | Auto-property classificazione items (IsResource, IsPotion, etc.) | Item |
| TASK-ITEM-015 | MapItem/CorpseItem subtypes | Item |
| TASK-ITEM-016 | Weapons data loading | Item |
| TASK-ITEM-017 | AutoStackResource | Item |
| TASK-ITEM-019 | HouseRevision/HousePacket | Item |
| TASK-PKT-002/003 | CreateCharacter handler | Packet |
| TASK-AGT-005/006 | BandageHeal: low warning, ghost check | BandageHeal |
| TASK-AGT-008 | Scavenger: IsLootableTarget check | Scavenger |
| TASK-AGT-009 | Vendor: LastResellList | Vendor |
| TASK-AGT-014 | HotKey: MasterKey | HotKey |
| TASK-AGT-021 | Profile auto-detect per serial | Config |
| TASK-AGT-022 | SeasonManager | Seasons |
| TASK-AGT-025/026 | UOSteam gumplist + findwand stubs | UOSteam |
| TASK-AGT-027 | Script recording output C# | ScriptRecorder |
| TASK-API-001 | Items.GetProperties con delay | API |
| TASK-API-002-009 | CUO API stubs (architetturalmente limitati) | API |
| TASK-UI-005 | Hotkey Master Key | UI |
| TASK-UI-006 | Journal Filter Grid | UI |
| TASK-UI-007 | Enhanced Map Path Setting | UI |
| TASK-UI-008 | DPS Meter Filter | UI |
| TASK-UI-009 | Video Playback & File Browser | UI |

---

## Statistiche Finali

| Metrica | Valore |
|---------|--------|
| File legacy analizzati | ~257 |
| File nuovo codice analizzati | ~299 |
| Discrepanze totali identificate | **126** |
| — Priorita Critica | 8 cluster |
| — Priorita Alta | 14 task |
| — Priorita Media | 27 task |
| — Priorita Bassa | 38 task |
| Feature con parita completa | ~75% delle classi/servizi |
| Feature nuove (non nel legacy) | 14+ pagine UI, debug scripting, 14 nuovi packet handler |
| Handler pacchetti con parita | 50 su 55 (91%) |
| Azioni macro con parita | 43 su 43 (100%) |
| Metodi API scripting con parita | ~98% |
