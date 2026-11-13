# Щоденний ритуал: формат, складання Release без попереджень, тести, і лише тоді коміт.
# Приклад: .\scripts\commit.ps1 "feat(domain): таблиця переходів"
# Для комітів лише документів (файли .md, ci.yml): .\scripts\commit.ps1 -DocsOnly "docs(spec): ..."
param(
    [Parameter(Mandatory)][string]$Message,
    [switch]$DocsOnly
)
$ErrorActionPreference = 'Stop'

function Step([string]$name, [scriptblock]$action) {
    Write-Host "==> $name"
    & $action
    if ($LASTEXITCODE -ne 0) { throw "Крок не пройдено: $name. Коміт не створено." }
}

if (-not $DocsOnly) {
    Step 'dotnet format' { dotnet format }
    Step 'dotnet build -c Release -warnaserror' { dotnet build -c Release -warnaserror }
    Step 'dotnet test' { dotnet test -c Release --no-build }
}
git add -A
git commit -m $Message
git --no-pager log --oneline -1
