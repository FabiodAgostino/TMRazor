Add-Type -AssemblyName PresentationCore
$inputPath = "C:\Users\fabio.dagostino\Desktop\RazorEnhanced-release-1.0\Content\LOGO_TM_no3.webp"
$pngOut = "C:\Users\fabio.dagostino\Desktop\RazorEnhanced-release-1.0\Content\LOGO_TM_no3.png"

try {
    $uri = New-Object System.Uri($inputPath)
    $decoder = [System.Windows.Media.Imaging.BitmapDecoder]::Create($uri, 'PreserveIconColor', 'Default')
    $encoder = New-Object System.Windows.Media.Imaging.PngBitmapEncoder
    $encoder.Frames.Add($decoder.Frames[0])
    $fs = [System.IO.File]::OpenWrite($pngOut)
    $encoder.Save($fs)
    $fs.Close()
    Write-Host "Success converting to PNG"
} catch {
    Write-Host "Failed: $_"
}
