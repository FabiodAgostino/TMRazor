# --- CONFIGURAZIONE ---
$Dataset = "dataset.jsonl"
$ApisCsv = "APIS.csv"
$CheckScript = "Check-Apis.ps1"
$Iteration = 0

Write-Host "--- Avvio Generatore Infinito Razor Enhanced ---" -ForegroundColor Cyan

while($true) {
    $Iteration++
    Write-Host "--- Iterazione n. $Iteration ---" -ForegroundColor Yellow

    # Ogni 2 iterazioni puliamo il contesto
    if ($Iteration % 2 -eq 0) {
        Write-Host "Cleaning context (Giro da 2)..." -ForegroundColor Magenta
        gemini "/clear"
    }

    # Costruiamo il prompt in un unico blocco di testo semplice
    $PromptText = "Analizza @$ApisCsv e @$CheckScript per le API Razor Enhanced. " +
                  "Guarda lo stile di @$Dataset. " +
                  "GENERA 5 NUOVE RIGHE in formato JSONL seguendo queste regole: " +
                  "1. Usa solo API reali. " +
                  "2. Usa API poco battute o quelle principali (Player, Mobiles, Items, Target, Spells, Journal). " +
                  "3. Lingua mista (prompt utente varie lingue, risposte tecniche Python). " +
                  "4. Rispondi SOLO con le 5 righe JSONL crudo. Niente markdown, niente commenti."

    # Esecuzione comando
    $NewLines = gemini $PromptText | Out-String
    
    # Pulizia e filtraggio righe
    # Dividiamo per linea, togliamo spazi/backtick e prendiamo solo righe con {"messages":
    $CleanLines = $NewLines -split "`n" | ForEach-Object { $_.Trim().Trim('`') } | Where-Object { $_ -match '\{"messages":' }

    if ($CleanLines) {
        # Append al file dataset
        $CleanLines | Out-File -FilePath $Dataset -Append -Encoding utf8
        Write-Host "SUCCESS: 5 righe aggiunte!" -ForegroundColor Green
    } else {
        Write-Host "WARNING: Nessuna riga valida generata." -ForegroundColor Red
    }

    # Pausa di sicurezza
    Start-Sleep -Seconds 5
}