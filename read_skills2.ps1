$mul = [System.IO.File]::ReadAllBytes('C:\Users\thebe\Desktop\TM Patched\skills.mul')
$idx = [System.IO.File]::ReadAllBytes('C:\Users\thebe\Desktop\TM Patched\skills.idx')

$entryCount = $idx.Length / 12
Write-Host "Total skills: $entryCount"

for ($i = 0; $i -lt [Math]::Min($entryCount, 20); $i++) {
    $base = $i * 12
    $offset = [BitConverter]::ToInt32($idx, $base)
    $size   = [BitConverter]::ToInt32($idx, $base + 4)
    if ($offset -lt 0 -or $size -le 1) { Write-Host "Skill $i : invalid (off=$offset size=$size)"; continue }
    $use    = $mul[$offset]
    $nameBytes = $mul[($offset + 1)..($offset + $size - 1)]
    # strip trailing null if present
    $nameLen = $nameBytes.Length
    while ($nameLen -gt 0 -and $nameBytes[$nameLen-1] -eq 0) { $nameLen-- }
    $name = [System.Text.Encoding]::Latin1.GetString($nameBytes, 0, $nameLen)
    Write-Host "Skill $i (use=$use): '$name'"
}
