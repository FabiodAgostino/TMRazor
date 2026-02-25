$file_path = "c:\Users\fabio.dagostino\Desktop\RazorEnhanced-release-1.0\Razor\RazorEnhanced\UI\Strings.it.resx"

$translations = @{
    'MainForm.tabPage2.Text' = 'Barra Statistiche'
    'MainForm.groupBox39.Text' = 'Opacità'
    'MainForm.groupBox25.Text' = 'Generale'
    'MainForm.lockToolBarCheckBox.Text' = 'Blocca Barra'
    'MainForm.autoopenToolBarCheckBox.Text' = 'Apri al Login'
    'MainForm.closeToolBarButton.Text' = 'Chiudi'
    'MainForm.openToolBarButton.Text' = 'Apri'
    'MainForm.groupBox4.Text' = 'Layout'
    'MainForm.showtitheToolBarCheckBox.Text' = 'Mostra Decime'
    'MainForm.label43.Text' = 'Slot:'
    'MainForm.label41.Text' = 'Dimensione:'
    'MainForm.showfollowerToolBarCheckBox.Text' = 'Mostra Follower'
    'MainForm.showweightToolBarCheckBox.Text' = 'Mostra Peso'
    'MainForm.showmanaToolBarCheckBox.Text' = 'Mostra Mana'
    'MainForm.showstaminaToolBarCheckBox.Text' = 'Mostra Stamina'
    'MainForm.showhitsToolBarCheckBox.Text' = 'Mostra Vita'
    'MainForm.label2.Text' = 'Stile:'
    'MainForm.groupBox26.Text' = 'Conteggio Oggetti'
    'MainForm.label38.Text' = '-1 per tutti'
    'MainForm.label37.Text' = 'Nome:'
    'MainForm.toolboxcountClearButton.Text' = 'Svuota Slot'
    'MainForm.toolboxcountTargetButton.Text' = 'Ottieni Dati'
    'MainForm.label36.Text' = 'Avviso:'
    'MainForm.toolboxcountHueWarningCheckBox.Text' = 'Mostra Avviso'
    'MainForm.label35.Text' = 'Colore:'
    'MainForm.label18.Text' = 'Grafica:'
    'MainForm.tabPage3.Text' = 'Griglia Magie'
    'MainForm.groupBox38.Text' = 'Opacità'
    'MainForm.groupBox37.Text' = 'Layout'
    'MainForm.label80.Text' = 'Stile:'
    'MainForm.label53.Text' = 'Slot O:'
    'MainForm.label49.Text' = 'Slot V:'
    'MainForm.groupBox36.Text' = 'Oggetto Griglia'
    'MainForm.label65.Text' = 'Script'
    'MainForm.label44.Text' = 'Bordo: '
    'MainForm.label52.Text' = 'Abilità/Magia:'
    'MainForm.label51.Text' = 'Slot:'
    'MainForm.label45.Text' = 'Gruppo:'
    'MainForm.groupBox35.Text' = 'Generale'
    'MainForm.setSpellBarOrigin.Text' = 'Imposta Origine'
    'MainForm.gridlock_CheckBox.Text' = 'Blocca Griglia'
    'MainForm.gridopenlogin_CheckBox.Text' = 'Apri al Login'
    'MainForm.gridclose_button.Text' = 'Chiudi'
    'MainForm.gridopen_button.Text' = 'Apri'
    'MainForm.groupBox8.Text' = 'Chiave Master'
    'MainForm.hotkeyMasterClearButton.Text' = 'Cancella'
    'MainForm.hotkeyMasterSetButton.Text' = 'Imposta'
    'MainForm.label42.Text' = 'Tasto:'
    'MainForm.groupBox28.Text' = 'Generale'
    'MainForm.hotkeyMDisableButton.Text' = 'Disabilita'
    'MainForm.hotkeyMEnableButton.Text' = 'Abilita'
    'MainForm.hotkeyKeyMasterLabel.Text' = 'Tasto ON/OFF: Nessuno'
    'MainForm.hotkeyStatusLabel.Text' = 'Stato: Abilitato'
    'MainForm.hotkeypassCheckBox.Text' = 'Passa Tasto a UO'
    'MainForm.hotkeyClearButton.Text' = 'Cancella'
    'MainForm.hotkeySetButton.Text' = 'Imposta'
    'MainForm.label39.Text' = 'Tasto:'
    'MainForm.label79.Text' = 'X'
}

[xml]$doc = Get-Content $file_path -Raw
$changed = 0

foreach ($node in $doc.root.data) {
    $name = $node.name
    if ($translations.Contains($name)) {
        if ($node.value -ne $translations[$name]) {
            $node.value = $translations[$name]
            $changed++
        }
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
