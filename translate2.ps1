$file_path = "c:\Users\fabio.dagostino\Desktop\RazorEnhanced-release-1.0\Razor\RazorEnhanced\UI\Strings.it.resx"

$translations = @{
    # Scripting Tab (from screenshot)
    'MainForm.pythonScriptingTab.Text' = 'Python'
    'MainForm.filename.Text' = 'Nome File'
    'MainForm.status.Text' = 'Stato'
    'MainForm.loop.Text' = 'Ciclo'
    'MainForm.autostart.Text' = 'A.S.'
    'MainForm.wait.Text' = 'Attendi'
    'MainForm.hotkey.Text' = 'Hotkeys'
    'MainForm.heypass.Text' = 'PassKey'
    'MainForm.index.Text' = '#'
    'MainForm.preload.Text' = 'Precarica'
    
    'MainForm.InspectGumpsButton.Text' = 'Ispeziona Gump'
    'MainForm.InspectContextButton.Text' = 'Ispeziona'
    
    'MainForm.groupBox30.Text' = 'Info Script'
    'MainForm.scriptloopmodecheckbox.Text' = 'Modalità Ciclo'
    'MainForm.scriptwaitmodecheckbox.Text' = 'Attendi ricarica'
    'MainForm.scriptautostartcheckbox.Text' = 'Autostart al Login'
    'MainForm.scriptpreload.Text' = 'Precarica'
    
    'MainForm.scriptOperationsBox.Text' = 'Operazioni Script'
    'MainForm.buttonAddScript.Text' = 'Aggiungi'
    'MainForm.buttonRemoveScript.Text' = 'Rimuovi'
    'MainForm.buttonScriptDown.Text' = 'Giù'
    'MainForm.buttonScriptTo.Text' = 'A'
    'MainForm.buttonScriptUp.Text' = 'Su'
    'MainForm.buttonScriptEditorNew.Text' = 'Nuovo'
    'MainForm.buttonScriptEditor.Text' = 'Modifica'
    
    'MainForm.groupBox42.Text' = 'Ricerca'
    'MainForm.scripterrorlogCheckBox.Text' = 'Log Errori Script'
    'MainForm.showscriptmessageCheckBox.Text' = 'Mostra Messaggi Errore'
    'MainForm.scriptshowStartStopCheckBox.Text' = 'Mostra Messaggio Start/Stop'
    'MainForm.scriptPacketLogCheckBox.Text' = 'Abilita Log Pacchetti'
    'MainForm.autoScriptReload.Text' = 'Ricarica Script Auto'

    # UOS Scripting Tab Headers
    'MainForm.uosScriptingTab.Text' = 'UOS'
    'MainForm.columnHeader1.Text' = 'Nome File'
    'MainForm.columnHeader2.Text' = 'Stato'
    'MainForm.columnHeader3.Text' = 'Ciclo'
    'MainForm.columnHeader4.Text' = 'A.S.'
    'MainForm.columnHeader5.Text' = 'Attendi'
    'MainForm.columnHeader6.Text' = 'Hotkeys'
    'MainForm.columnHeader7.Text' = 'PassKey'
    'MainForm.columnHeader8.Text' = '#'
    'MainForm.columnHeader19.Text' = 'Precarica'

    # C# Scripting Tab Headers
    'MainForm.csScriptingTab.Text' = 'C#'
    'MainForm.columnHeader10.Text' = 'Nome File'
    'MainForm.columnHeader11.Text' = 'Stato'
    'MainForm.columnHeader12.Text' = 'Ciclo'
    'MainForm.columnHeader13.Text' = 'A.S.'
    'MainForm.columnHeader14.Text' = 'Attendi'
    'MainForm.columnHeader15.Text' = 'Hotkeys'
    'MainForm.columnHeader16.Text' = 'PassKey'
    'MainForm.columnHeader17.Text' = '#'
    'MainForm.columnHeader20.Text' = 'Precarica'

    # Video/Screenshot Tab (from screenshot)
    'MainForm.label12.Text' = 'Formato Immagine:'
    'MainForm.capNow.Text' = 'Scatta Foto Ora'
    'MainForm.radioUO.Text' = 'Solo UO'
    'MainForm.radioFull.Text' = 'Schermo Intero'
    'MainForm.screenAutoCap.Text' = 'Foto Automatica Morte'
    'MainForm.dispTime.Text' = 'Includi Data/Ora su immagini'
    'MainForm.videoRecStatuslabel.Text' = 'Inattivo'
    'MainForm.label64.Text' = 'Stato Rec:'
    'MainForm.groupBox40.Text' = 'Riproduzione'
    'MainForm.videosettinggroupBox.Text' = 'Impostazioni Video'
    'MainForm.label63.Text' = 'Codec:'
    'MainForm.label62.Text' = 'FPS: '
    'MainForm.groupBox15.Text' = 'File'
    
    # ToolStripMenuItem on Context Menus
    'MainForm.modifyToolStripMenuItem.Text' = 'Modifica'
    'MainForm.addToolStripMenuItem.Text' = 'Aggiungi'
    'MainForm.removeToolStripMenuItem.Text' = 'Rimuovi'
    'MainForm.openToolStripMenuItem.Text' = 'Apri'
    'MainForm.moveUpToolStripMenuItem.Text' = 'Sposta Su'
    'MainForm.moveDownToolStripMenuItem.Text' = 'Sposta Giù'
    'MainForm.moveToToolStripMenuItem.Text' = 'Sposta In'
    'MainForm.flagsToolStripMenuItem.Text' = 'Opzioni'
    'MainForm.loopModeToolStripMenuItem.Text' = 'Modalità Ciclo'
    'MainForm.preloadToolStripMenuItem.Text' = 'Precarica'
    'MainForm.waitBeforeInterruptToolStripMenuItem.Text' = 'Attendi Ricarica'
    'MainForm.autoStartAtLoginToolStripMenuItem.Text' = 'Autostart al Login'
    'MainForm.playToolStripMenuItem.Text' = 'Start/Riproduci'
    'MainForm.stopToolStripMenuItem.Text' = 'Stop'
}

[xml]$doc = Get-Content $file_path -Raw
$changed = 0

foreach ($node in $doc.root.data) {
    if ($translations.Contains($node.name) -and $node.value -ne $translations[$node.name]) {
        $node.value = $translations[$node.name]
        $changed++
    }
}

Write-Host "Updated $changed translations."

$settings = New-Object System.Xml.XmlWriterSettings
$settings.Indent = $true
$settings.IndentChars = "  "
$settings.Encoding = [System.Text.Encoding]::UTF8

$writer = [System.Xml.XmlWriter]::Create($file_path, $settings)
$doc.WriteTo($writer)
$writer.Flush()
$writer.Close()
