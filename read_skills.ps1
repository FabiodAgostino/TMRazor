$bytes = [System.IO.File]::ReadAllBytes('C:\Users\thebe\Desktop\TM Patched\skills.mul')
Write-Host ("HEX[0..79]: " + (($bytes[0..79] | ForEach-Object { $_.ToString('X2') }) -join ' '))
$pos = 0
$i = 0
while ($pos -lt $bytes.Length -and $i -lt 15) {
    $use = $bytes[$pos]; $pos++
    $end = $pos
    while ($end -lt $bytes.Length -and $bytes[$end] -ne 0) { $end++ }
    $name = [System.Text.Encoding]::Latin1.GetString($bytes, $pos, $end - $pos)
    Write-Host "Skill $i (use=$use): '$name'"
    $pos = $end + 1
    $i++
}
