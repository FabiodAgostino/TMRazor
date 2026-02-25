Add-Type -AssemblyName 'System.Windows.Forms'

# Check main assembly
$exe = [System.Reflection.Assembly]::LoadFile('C:/Users/thebe/Documents/GitHub/TMRazor/bin/x64/Debug/RazorEnhanced.exe')
$stream = $exe.GetManifestResourceStream('Assistant.RazorEnhanced.UI.Strings.resources')
Write-Host "Main assembly stream length: $($stream.Length)"
$reader = New-Object System.Resources.ResourceReader($stream)
$dict = New-Object System.Collections.Hashtable
$reader.CopyTo($dict)
Write-Host "Main assembly entry count: $($dict.Count)"
$dict.GetEnumerator() | Select-Object -First 3 | ForEach-Object { Write-Host "  $($_.Key) = $($_.Value)" }
$reader.Dispose()

Write-Host ""

# Check satellite assembly
$sat = [System.Reflection.Assembly]::LoadFile('C:/Users/thebe/Documents/GitHub/TMRazor/bin/x64/Debug/it/RazorEnhanced.resources.dll')
$stream2 = $sat.GetManifestResourceStream('Assistant.RazorEnhanced.UI.Strings.resources')
Write-Host "Satellite stream length: $($stream2.Length)"
$reader2 = New-Object System.Resources.ResourceReader($stream2)
$dict2 = New-Object System.Collections.Hashtable
$reader2.CopyTo($dict2)
Write-Host "Satellite entry count: $($dict2.Count)"
$dict2.GetEnumerator() | Select-Object -First 3 | ForEach-Object { Write-Host "  $($_.Key) = $($_.Value)" }
$reader2.Dispose()
