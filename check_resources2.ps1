Add-Type -AssemblyName 'System.Windows.Forms'

$sat = [System.Reflection.Assembly]::LoadFile('C:/Users/thebe/Documents/GitHub/TMRazor/bin/x64/Debug/it/RazorEnhanced.resources.dll')
$rm = New-Object System.Resources.ResourceManager("Assistant.RazorEnhanced.UI.Strings", $sat)

$itCulture = New-Object System.Globalization.CultureInfo("it-IT")
$enCulture = New-Object System.Globalization.CultureInfo("en-US")

$keyToTest = "MainForm.moreOptTab.Text"

$itVal = $rm.GetString($keyToTest, $itCulture)
$enVal = $rm.GetString($keyToTest, $enCulture)

Write-Host "IT value: $itVal"
Write-Host "EN value: $enVal"

# Also test from main exe
$exe = [System.Reflection.Assembly]::LoadFile('C:/Users/thebe/Documents/GitHub/TMRazor/bin/x64/Debug/RazorEnhanced.exe')
$rmMain = New-Object System.Resources.ResourceManager("Assistant.RazorEnhanced.UI.Strings", $exe)
$itValMain = $rmMain.GetString($keyToTest, $itCulture)
Write-Host "IT value from main assembly (with satellite lookup): $itValMain"
