$releaseFolder = Join-Path -Path $PSScriptRoot  -ChildPath 'Release'
$outputFolder  = Join-Path -Path $releaseFolder -ChildPath 'packs/CSVvvv/nodes/plugins'
$sourceFolder  = Join-Path -Path $PSScriptRoot  -ChildPath 'src'
$packFolder    = Join-Path -Path $releaseFolder -ChildPath 'packs'

$filesToRemove = @(
    'System.ComponentModel.Composition.CodePlex.dll'
    'VVVV.Core.dll'
    'VVVV.Utils.dll'
)

if (!(Test-Path -Path $releaseFolder)) {
    New-Item -Path $releaseFolder -ItemType Container -Force -Confirm:$false | Out-Null
}

Get-ChildItem -Path $releaseFolder | Remove-Item -Force -Recurse -Confirm:$false

& dotnet build -o $outputFolder -c Release --no-incremental $sourceFolder

$dllPath = Join-Path -Path $outputFolder -ChildPath 'CSVvvv.dll'
$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dllPath).ProductVersion

$releaseFile   = Join-Path -Path $releaseFolder -ChildPath ('CSVvvv_{0}.zip' -f $version)

Get-ChildItem $releaseFolder -Recurse `
    | Where-Object { $_.Name -in $filesToRemove } `
    | Remove-Item -Force -Confirm:$false

Compress-Archive -Path $packFolder -DestinationPath $releaseFile | Out-Null

Remove-Item -Path $packFolder -Recurse -Force -Confirm:$false
