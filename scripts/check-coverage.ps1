# Читає звіт Cobertura і падає, якщо рядкове покриття нижче за поріг.
param([int]$Threshold = 70)

$file = Get-ChildItem -Path "$PSScriptRoot/.." -Recurse -Filter coverage.cobertura.xml |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
if (-not $file) { Write-Error 'Не знайдено coverage.cobertura.xml'; exit 1 }

[xml]$xml = Get-Content $file.FullName
$line = [math]::Round([double]$xml.coverage.'line-rate' * 100, 1)
$branch = [math]::Round([double]$xml.coverage.'branch-rate' * 100, 1)
Write-Host "Рядкове покриття: $line %, покриття гілок: $branch %"
if ($line -lt $Threshold) {
    Write-Error "Покриття $line % нижче за поріг $Threshold %"
    exit 1
}
