$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot '../HareketliDuvarKagidi.csproj'
$output = Join-Path $PSScriptRoot '../dist/1.0.1/win-x64'
dotnet publish $project -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:PublishTrimmed=false -o $output
if ($LASTEXITCODE -ne 0) { throw 'Self-contained yayın başarısız.' }
$notices = Join-Path $PSScriptRoot '../THIRD_PARTY_NOTICES.md'
$licenses = Join-Path $PSScriptRoot '../licenses'
foreach ($file in @('LibVLCSharp-LICENSE.txt', 'VLC-COPYING-LGPL.txt', 'VLC-COPYING-GPL.txt',
    'VLC-AUTHORS.txt', 'VLC-THANKS.txt', 'NET-Runtime-LICENSE.TXT', 'NET-Runtime-THIRD-PARTY-NOTICES.TXT',
    'NET-Desktop-LICENSE.txt', 'NET-WPF-LICENSE.TXT', 'NET-WPF-THIRD-PARTY-NOTICES.TXT',
    'NET-WinForms-LICENSE.TXT', 'NET-WinForms-THIRD-PARTY-NOTICES.TXT', 'Inno-Setup-LICENSE.txt',
    'SOURCES.json', 'VLC-BINARY-INVENTORY.json', 'VLC-SOURCE-MAP.json')) {
    if (-not (Test-Path (Join-Path $licenses $file))) { throw "Eksik üçüncü taraf bildirimi: $file" }
}
foreach ($entry in (Get-Content (Join-Path $licenses 'SOURCES.json') -Raw -Encoding UTF8 | ConvertFrom-Json)) {
    $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $licenses $entry.file)).Hash
    if ($hash -ne $entry.sha256) { throw "Bildirim özeti uyuşmuyor: $($entry.file)" }
}
Copy-Item -LiteralPath $notices -Destination $output -Force
Copy-Item -LiteralPath $licenses -Destination $output -Recurse -Force
foreach ($file in @('HareketliDuvarKagidi.exe', 'coreclr.dll', 'hostfxr.dll', 'LibVLCSharp.dll', 'libvlc/win-x64/libvlc.dll', 'libvlc/win-x64/libvlccore.dll')) {
    if (-not (Test-Path (Join-Path $output $file))) { throw "Eksik yayın dosyası: $file" }
}
if (-not (Get-ChildItem (Join-Path $output "libvlc/win-x64/plugins") -Filter *.dll -File -Recurse)) {
    throw "Native VLC eklentileri eksik."
}
$command = Get-Command ISCC.exe -ErrorAction SilentlyContinue
$compilerPath = if ($command) { $command.Source } else { $null }
if (-not $compilerPath) {
    foreach ($base in @(${env:ProgramFiles(x86)}, $env:ProgramFiles, "$env:LOCALAPPDATA\Programs")) {
        foreach ($folder in Get-ChildItem -LiteralPath $base -Directory -Filter 'Inno Setup*' -ErrorAction SilentlyContinue) {
            $candidate = Join-Path $folder.FullName 'ISCC.exe'
            if (Test-Path $candidate) { $compilerPath = $candidate; break }
        }
        if ($compilerPath) { break }
    }
}
if ($compilerPath) {
    & (Join-Path $PSScriptRoot 'Build-SourceBundle.ps1')
    & $compilerPath (Join-Path $PSScriptRoot 'Setup.iss')
    if ($LASTEXITCODE -ne 0) { throw 'Inno Setup derlemesi başarısız.' }
} else {
    Write-Warning 'Inno Setup bulunamadı. Yayın paketi hazır; Setup.exe üretilemedi.'
}

