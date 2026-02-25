$newIconB64 = Get-Content ".\new_icon_base64.txt" -Raw

$files = Get-ChildItem -Path "Razor" -Recurse -Filter "*.resx"

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $modified = $false
    
    # We are looking for forms' embedded icons
    $pattern = '(?s)(<data name="\$?this\.Icon"\s+type="System\.Drawing\.Icon,\s*System\.Drawing"\s+mimetype="application/x-microsoft\.net\.object\.bytearray\.base64">\s*<value>)(.*?)(</value>)'
    
    if ($content -match $pattern) {
        $content = $content -replace $pattern, "`$1$newIconB64`$3"
        $modified = $true
    }
    
    # We are also looking for "Icon1" embedded icons
    $pattern2 = '(?s)(<data name="Icon1"\s+type="System\.Drawing\.Icon,\s*System\.Drawing"\s+mimetype="application/x-microsoft\.net\.object\.bytearray\.base64">\s*<value>)(.*?)(</value>)'
    
    if ($content -match $pattern2) {
        $content = $content -replace $pattern2, "`$1$newIconB64`$3"
        $modified = $true
    }

    if ($modified) {
        Set-Content -Path $file.FullName -Value $content
        Write-Host "Updated $($file.Name)"
    }
}
