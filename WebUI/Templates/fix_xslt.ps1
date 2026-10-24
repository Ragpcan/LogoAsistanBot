$files = Get-ChildItem -Path "C:\Users\RAGIP\Downloads" -Filter "*Zirve*.xslt"
foreach ($f in $files) {
    Write-Host "Processing $($f.FullName)"
    $text = [System.IO.File]::ReadAllText($f.FullName)
    # Remove fixed row & table heights that cause giant gaps
    $text = [System.Text.RegularExpressions.Regex]::Replace($text, "height\s*:\s*\d+(\.\d+)?(px|%)?;?", "", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
    # Remove empty spacer paragraphs
    $text = $text.Replace("<p>&#160;</p>", "")
    [System.IO.File]::WriteAllText($f.FullName, $text)
    
    # Also copy to project templates
    $target = Join-Path "C:\Users\RAGIP\Desktop\AntyGravity\LogoCrmTelegramIntegration\src\Infrastructure\Templates" $f.Name
    [System.IO.File]::WriteAllText($target, $text)
}
Write-Host "All Zirve XSLT files cleaned successfully!"
