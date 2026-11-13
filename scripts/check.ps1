# Локальний еквівалент конвеєра: ті самі перевірки, що виконує GitHub Actions.
$ErrorActionPreference = 'Stop'

function Step([string]$name, [scriptblock]$action) {
    Write-Host "==> $name"
    & $action
    if ($LASTEXITCODE -ne 0) { throw "Крок не пройдено: $name" }
}

Step 'Формат (dotnet format --verify-no-changes)' { dotnet format --verify-no-changes }
Step 'Складання Release без попереджень' { dotnet build -c Release -warnaserror }
Step 'Тести з покриттям' { dotnet test -c Release --no-build /p:CollectCoverage=true }
Step 'Поріг покриття' { & "$PSScriptRoot/check-coverage.ps1" -Threshold 70 }
Write-Host 'Усі перевірки пройдено.'
