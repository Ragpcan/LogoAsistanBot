$files = Get-ChildItem -Path "C:\Users\RAGIP\Downloads" -Filter "*Zirve*.xslt"
foreach ($f in $files) {
    Write-Host "Restoring QR code and cleaning row layout heights for $($f.FullName)"
    $text = [System.IO.File]::ReadAllText($f.FullName)
    
    # 1. Restore multiline & singleline QRCode JS syntax
    $text = [System.Text.RegularExpressions.Regex]::Replace($text, "width\s*:\s*140\s*,\s*,", "width : 140,`r`nheight : 140,", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    $text = $text.Replace("{ width: 140, , correctLevel:", "{ width: 140, height: 140, correctLevel:")
    $text = $text.Replace("{ width: 140,  correctLevel:", "{ width: 140, height: 140, correctLevel:")
    
    # 2. Restore qrvalue div tag completely
    $text = $text.Replace('<div height: 30px; width: 30px; display: none;">', '<div id="qrvalue" style="visibility: hidden; height: 30px; width: 30px; display: none;">')
    $text = $text.Replace('<div height: 30px; width: 30px; display:none">', '<div id="qrvalue" style="visibility: hidden; height: 30px; width: 30px; display: none;">')

    # 3. Specifically target ONLY table row layout heights (e.g. style="height: 416px;", style="height: 225.6px;", style="height: 689.2px;", style="height: 122.75px;")
    $text = [System.Text.RegularExpressions.Regex]::Replace($text, "(<tr[^>]*)\s*style=""height:\s*\d+(\.\d+)?(px|%)?;?""", "${1}", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    $text = [System.Text.RegularExpressions.Regex]::Replace($text, "(<table[^>]*style=""[^""]*)height:\s*\d+(\.\d+)?(px|%)?;?", "${1}", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

    # Clean up empty style attributes if any (style="")
    $text = $text.Replace('style=""', '').Replace('style=" "', '')
    
    # Write back to Downloads
    [System.IO.File]::WriteAllText($f.FullName, $text)
    
    # Copy to project templates
    $target = Join-Path "C:\Users\RAGIP\Desktop\AntyGravity\LogoCrmTelegramIntegration\src\Infrastructure\Templates" $f.Name
    [System.IO.File]::WriteAllText($target, $text)
}
Write-Host "QR Code restored and layout heights cleaned successfully!"
