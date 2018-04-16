$releaseFolder = Join-Path -Path $PSScriptRoot  -ChildPath 'Release'
$outputFolder  = Join-Path -Path $releaseFolder -ChildPath 'CSVvvv/nodes/plugins'
$sourceFolder  = Join-Path -Path $PSScriptRoot  -ChildPath 'src'
$packFolder    = Join-Path -Path $releaseFolder -ChildPath 'CSVvvv'
$releaseFile   = Join-Path -Path $releaseFolder -ChildPath 'CSVvvv.zip'

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

Get-ChildItem $releaseFolder -Recurse `
    | Where-Object { $_.Name -in $filesToRemove } `
    | Remove-Item -Force -Confirm:$false

Compress-Archive -Path $packFolder -DestinationPath $releaseFile | Out-Null

Remove-Item -Path $packFolder -Recurse -Force -Confirm:$false
