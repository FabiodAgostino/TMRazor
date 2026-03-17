# Guida alla Migrazione Script TMRazor -> TMRazorImproved

Questa guida documenta le differenze comportamentali e le nuove funzionalità delle API di scripting in TMRazorImproved (.NET 10) rispetto alla versione legacy.

## 1. Misc API

### 1.1 Calcolo delle Distanze
In TMRazorImproved, per coerenza con il motore di gioco di Ultima Online, il metodo `Misc.Distance` utilizza la **distanza di Chebyshev**.

- **Misc.Distance(x1, y1, x2, y2)**: Ritorna la distanza "tile-based" (massimo tra delta X e delta Y). In UO, muoversi in diagonale costa 1 tile esattamente come in orizzontale/verticale, quindi questa è la distanza reale percorsa dal personaggio.
- **Misc.DistanceSqrt(x1, y1, x2, y2)**: Ritorna la **distanza Euclidea** (in linea d'aria). Utile per calcoli geometrici o effetti grafici non legati al movimento.

**Cosa cambia per la migrazione:**
Se i tuoi script legacy si aspettavano una distanza Euclidea da `Misc.Distance`, dovrai aggiornarli utilizzando `Misc.DistanceSqrt`. Tuttavia, nella maggior parte dei casi legati a raggio di attacco, incantesimi o movimento, il valore di Chebyshev è quello corretto.

## 2. Player API

### 2.1 PathFinding
Il metodo `Player.PathFindTo(x, y, z)` è ora pienamente supportato.
- Utilizza il motore di pathfinding interno per calcolare il percorso.
- Il raggio massimo di scansione è configurabile nelle Opzioni (default 200 tile).

## 3. Special Moves
Le abilità speciali (Primary/Secondary) utilizzano ora il pacchetto `0xD7` ottimizzato per lo shard **The Miracle**.
- `SpecialMoves.SetPrimaryAbility()`
- `SpecialMoves.SetSecondaryAbility()`
- `SpecialMoves.HasPrimary` / `SpecialMoves.HasSecondary` per verificare lo stato attivo.
