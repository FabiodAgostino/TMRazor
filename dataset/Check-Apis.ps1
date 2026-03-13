# Specifica i percorsi dei file
$csvPath = "APIS.csv"
$jsonlPath = "dataset.jsonl"

Write-Host "Caricamento dei dati e analisi tipi in corso..." -ForegroundColor Cyan

# Carica le API dal CSV
$apis = Import-Csv -Path $csvPath

# Dizionari di supporto
$apiCounts = [ordered]@{}
$validApisByModule = @{} # Modulo -> HashSet di Metodi/Proprietà
$methodReturnTypes = @{} # "Modulo.Metodo" -> Tipo di Ritorno (es. "Item")

# Moduli statici conosciuti (per proteggere il for-loop dall'ereditare un tipo modulo)
$knownStaticModules = New-Object System.Collections.Generic.HashSet[string]
@(
    "Player","Target","Misc","Items","Mobiles","Gumps","Journal","Spells",
    "AutoLoot","Dress","Scavenger","Restock","Organizer","BandageHeal",
    "BuyAgent","SellAgent","Vendor","PathFinding","Statics","Sound",
    "PacketLogger","DPSMeter","Trade","CUO"
) | ForEach-Object { [void]$knownStaticModules.Add($_) }

# FIX 3: Proprietà reali di Gumps.GumpData (la CSV dice solo "Varie")
$gumpDataKnownProps = @(
    "gumpId","serial","x","y","gumpDefinition","gumpStrings","hasResponse",
    "buttonid","switches","text","textID","gumpLayout","gumpText","gumpData",
    "layoutPieces","stringList"
)

# Popola le strutture dati dal CSV
foreach ($api in $apis) {
    $modulo = $api.Modulo
    $nomeApi = $api.'Nome API'
    $tipoRitorno = $api.'Tipo Ritorno'
    if ([string]::IsNullOrWhiteSpace($nomeApi)) { continue }

    if (-not $validApisByModule.Contains($modulo)) {
        $validApisByModule[$modulo] = New-Object System.Collections.Generic.HashSet[string]
    }

    # FIX 2: Gestione entry multi-valore (es. "StopIfStuck, Timeout, X, Y" oppure "LastBuyList, LastResellList")
    $nomiSingoli = @($nomeApi -split ',\s*' | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne '' })
    $cleanReturnType = $tipoRitorno -replace '^List\[(.+)\]$', '$1'

    foreach ($nomeS in $nomiSingoli) {
        $key = "$modulo.$nomeS"
        if (-not $apiCounts.Contains($key)) { $apiCounts[$key] = 0 }
        [void]$validApisByModule[$modulo].Add($nomeS)
        # Per entry multi-valore il tipo ritorno è "Misto", utile solo per entry singole
        if ($nomiSingoli.Count -eq 1) {
            $methodReturnTypes[$key] = $cleanReturnType
        }
    }
}

# FIX 3: Sovrascritura delle proprietà GumpData con quelle reali (ignorato "Varie" del CSV)
if (-not $validApisByModule.Contains("Gumps.GumpData")) {
    $validApisByModule["Gumps.GumpData"] = New-Object System.Collections.Generic.HashSet[string]
}
foreach ($p in $gumpDataKnownProps) {
    [void]$validApisByModule["Gumps.GumpData"].Add($p)
}

# Whitelist Python / .NET
$pythonWhitelist = New-Object System.Collections.Generic.HashSet[string]
@(
    "append", "extend", "insert", "remove", "pop", "clear", "index", "count", "sort", "reverse", "copy",
    "keys", "values", "items", "get", "update", "popitem", "setdefault", "add", "discard", "split", "join",
    "lower", "upper", "format", "startswith", "endswith", "replace", "strip", "read", "write", "close",
    "json", "loads", "dumps", "sleep", "time", "now", "exit", "X", "Y", "Z", "Width", "Height", "Success",
    "Count", "Length", "Value", "Serial", "Name", "Color", "Amount", "ItemID", "Hue", "Position",
    "Remove", "Contains", "Find", "Where", "Any", "All", "First", "Last", "ToList", "ToArray",
    "Groups", "Index", "strftime", "strptime", "reconfigure"
) | ForEach-Object { [void]$pythonWhitelist.Add($_) }

$inventedMethods = @()
$lineNum = 0

# Leggi e analizza il file JSONL
$datasetLines = Get-Content -Path $jsonlPath -Encoding UTF8
foreach ($line in $datasetLines) {
    $lineNum++
    if ([string]::IsNullOrWhiteSpace($line)) { continue }

    try {
        $jsonObj = $line | ConvertFrom-Json -ErrorAction Stop
        $assistantMessages = @($jsonObj.messages | Where-Object { $_.role -eq 'assistant' })

        foreach ($msg in $assistantMessages) {
            $code = $msg.content
            if ([string]::IsNullOrWhiteSpace($code)) { continue }

            # FIX 5: Salta risposte in prosa senza codice Python (es. rifiuti di API inesistenti)
            $hasPythonCode = ($code -match "(?m)^\s*(import |from |def |class |for |while |if )") -or
                             ($code -match "(?m)^\s*\w+\s*=\s*[A-Za-z_]") -or
                             ($code -match "(?m)^(    |\t)\w")
            if (-not $hasPythonCode) { continue }

            # --- FASE 1: INFERENZA DEI TIPI (Simbolo -> Classe) ---
            # FIX 1: Hashtable CASE-SENSITIVE per evitare "target" -> "Target", "item" -> "Items"
            $symbolTable = New-Object System.Collections.Hashtable ([System.StringComparer]::Ordinal)
            $symbolTable["Player"]  = "Player"
            $symbolTable["Target"]  = "Target"
            $symbolTable["Misc"]    = "Misc"
            $symbolTable["Items"]   = "Items"
            $symbolTable["Mobiles"] = "Mobiles"

            $codeLines = $code -split "\r?\n"
            foreach ($cLine in $codeLines) {
                # Pattern: var = Modulo.Metodo() o var = Modulo.Proprietà
                if ($cLine -match "\b([a-zA-Z_]\w*)\s*=\s*([a-zA-Z_]\w*(?:\.[a-zA-Z_]\w*)*)\.([a-zA-Z_]\w*)") {
                    $varName = $Matches[1]
                    $callMod = $Matches[2]
                    $callMem = $Matches[3]
                    $fullCall = "$callMod.$callMem"

                    if ($methodReturnTypes.Contains($fullCall)) {
                        $symbolTable[$varName] = $methodReturnTypes[$fullCall]
                    }
                    elseif ($validApisByModule.Contains($fullCall)) { # Caso costruttore tipo Mobiles.Filter()
                        $symbolTable[$varName] = $fullCall
                    }
                }
                # Pattern: for var in listVar
                # FIX 4: Non propagare il tipo se listVar è un modulo statico (es. "Player" in "for x in Player.Backpack.Contains")
                if ($cLine -match "for\s+([a-zA-Z_]\w*)\s+in\s+([a-zA-Z_]\w*)") {
                    $loopVar = $Matches[1]
                    $listVar = $Matches[2]
                    if ($symbolTable.Contains($listVar)) {
                        $listVarType = $symbolTable[$listVar]
                        # Propaga solo se listVar è un'istanza tipizzata, non un modulo statico
                        if ($listVarType -ne $listVar -and -not $knownStaticModules.Contains($listVar)) {
                            $symbolTable[$loopVar] = $listVarType
                        }
                    }
                }
            }

            # --- FASE 2: VALIDAZIONE ---
            # Cerchiamo tutti i pattern oggetto.membro
            $matches = [regex]::Matches($code, "\b([a-zA-Z_]\w*)\.([a-zA-Z_]\w*)\b")
            foreach ($match in $matches) {
                $obj = $match.Groups[1].Value
                $member = $match.Groups[2].Value
                $isInvented = $false
                $reason = ""

                # 1. Se l'oggetto è nella Symbol Table, sappiamo la sua classe!
                if ($symbolTable.Contains($obj)) {
                    $inferredClass = $symbolTable[$obj]

                    # Se la classe è conosciuta nel CSV
                    if ($validApisByModule.Contains($inferredClass)) {
                        if (-not $validApisByModule[$inferredClass].Contains($member) -and
                            -not $validApisByModule.Contains("$inferredClass.$member") -and
                            -not $pythonWhitelist.Contains($member)) {
                            $isInvented = $true
                            $reason = "L'oggetto '$obj' e' di tipo '$inferredClass', ma questa classe non ha il membro '$member' (Casing errato?)"
                        }
                    }
                }
                # 2. Se l'oggetto sembra un modulo statico (es. Player, Items)
                # FIX: controlla che inizi con maiuscola — evita "item" -> "Item", "target" -> "Target"
                elseif ($validApisByModule.Contains($obj) -and $obj -cmatch "^[A-Z]") {
                    if (-not $validApisByModule[$obj].Contains($member) -and
                        -not $validApisByModule.Contains("$obj.$member") -and
                        -not $pythonWhitelist.Contains($member)) {
                        $isInvented = $true
                        $reason = "Il modulo '$obj' non contiene '$member'."
                    }
                }
                # 3. Fallback per variabili sconosciute (solo se non esiste proprio da nessuna parte)
                else {
                    if (-not $pythonWhitelist.Contains($member) -and
                        -not $validApisByModule.Values.ForEach({ $_.Contains($member) }) -contains $true) {

                        if ($obj -notmatch "^(self|config|Scripts|utilities|glossary|items|spells|tameables|mobs|enemies|info|bi|je|re|sys|os|time|math)$") {
                            $isInvented = $true
                            $reason = "Impossibile determinare il tipo di '$obj' e '$member' non esiste in nessun modulo API."
                        }
                    }
                }

                if ($isInvented) {
                    $inventedMethods += [PSCustomObject]@{ Line = $lineNum; Object = $obj; Member = $member; Reason = $reason }
                }

                # Conteggio per statistiche originali
                $staticKey = "$obj.$member"
                if ($apiCounts.Contains($staticKey)) { $apiCounts[$staticKey]++ }
                elseif ($symbolTable.Contains($obj)) {
                    $instanceKey = "$($symbolTable[$obj]).$member"
                    if ($apiCounts.Contains($instanceKey)) { $apiCounts[$instanceKey]++ }
                }
            }
        }
    }
    catch { Write-Warning "Riga ${lineNum}: Errore parsing JSON" }
}

# Output Risultati
$usedApis = $apiCounts.GetEnumerator() | Where-Object { $_.Value -gt 0 } | Sort-Object Value -Descending
Write-Host "`n========================================" -ForegroundColor Yellow
Write-Host " API UTILIZZATE (Ordinate per frequenza)" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
foreach ($api in $usedApis) { Write-Host ("[{0,3}] {1}" -f $api.Value, $api.Name) -ForegroundColor Green }

$unusedApis = $apiCounts.GetEnumerator() | Where-Object { $_.Value -eq 0 } | Sort-Object Name
Write-Host "`n========================================" -ForegroundColor DarkYellow
Write-Host " API MAI UTILIZZATE NEL DATASET" -ForegroundColor DarkYellow
Write-Host "========================================" -ForegroundColor DarkYellow
foreach ($api in $unusedApis) { Write-Host "  $($api.Name)" -ForegroundColor DarkYellow }

if ($inventedMethods.Count -gt 0) {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host " API INVENTATE O SOSPETTE RILEVATE" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    foreach ($inv in $inventedMethods | Sort-Object Line | Select-Object -Unique Object, Member, Line, Reason) {
        Write-Host "Riga $($inv.Line): $($inv.Object).$($inv.Member) -> $($inv.Reason)" -ForegroundColor Red
    }
}

$totApis = $apiCounts.Count
$totUsed = @($usedApis).Count
$totUnused = @($unusedApis).Count
$totInvented = ($inventedMethods | Select-Object -Unique Object, Member, Line).Count

Write-Host "`n--- STATISTICHE ---" -ForegroundColor Cyan
Write-Host "Totale API nel CSV      : $totApis"
Write-Host "API Utilizzate          : $totUsed"
Write-Host "API Mai Utilizzate      : $totUnused" -ForegroundColor $(if ($totUnused -gt 0) { "DarkYellow" } else { "Gray" })
Write-Host "API Inventate rilevate  : $totInvented" -ForegroundColor $(if ($totInvented -gt 0) { "Red" } else { "Gray" })
