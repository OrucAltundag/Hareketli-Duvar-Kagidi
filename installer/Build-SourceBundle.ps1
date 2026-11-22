param(
    [string]$CacheDirectory = (Join-Path $env:TEMP 'HDK-source-pool')
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root = Split-Path $PSScriptRoot
$dist = Join-Path $root 'dist/1.0.1'
$manifest = Join-Path $PSScriptRoot 'SourceArchives.json'
$name = 'HareketliDuvarKagidi-1.0.1-sources-partial.zip'
[IO.Directory]::CreateDirectory($CacheDirectory) | Out-Null
[IO.Directory]::CreateDirectory($dist) | Out-Null
$temporary = Join-Path $dist ([guid]::NewGuid().ToString() + '.tmp')

function Add-SourceFile($archive, $path, $entryName) {
    [IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
        $archive, $path, $entryName, [IO.Compression.CompressionLevel]::NoCompression) | Out-Null
}

try {
    $zip = [IO.Compression.ZipFile]::Open($temporary, [IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($entry in (Get-Content $manifest -Raw -Encoding UTF8 | ConvertFrom-Json)) {
            $path = Join-Path $CacheDirectory $entry.file
            if (!(Test-Path -LiteralPath $path) -or (Get-FileHash $path).Hash -ne $entry.sha256) {
                Invoke-WebRequest -UseBasicParsing -Uri $entry.url -OutFile $path
            }
            if ((Get-FileHash $path).Hash -ne $entry.sha256) { throw "Kaynak özeti uyuşmuyor: $($entry.file)" }
            Add-SourceFile $zip $path ('archives/' + $entry.file)
        }
        Add-SourceFile $zip $manifest 'SourceArchives.json'
        Add-SourceFile $zip (Join-Path $root 'THIRD_PARTY_NOTICES.md') 'THIRD_PARTY_NOTICES.md'
        foreach ($file in Get-ChildItem (Join-Path $root 'licenses') -File) {
            Add-SourceFile $zip $file.FullName ('licenses/' + $file.Name)
        }
    } finally {
        $zip.Dispose()
    }
    $destination = Join-Path $dist $name
    if ([IO.File]::Exists($destination)) {
        [IO.File]::Replace($temporary, $destination, [System.Management.Automation.Language.NullString]::Value)
    } else {
        [IO.File]::Move($temporary, $destination)
    }
    $digest = (Get-FileHash $destination).Hash
    [IO.File]::WriteAllText("$destination.sha256", "$digest  $name" + [Environment]::NewLine)
} finally {
    if ([IO.File]::Exists($temporary)) { [IO.File]::Delete($temporary) }
}
