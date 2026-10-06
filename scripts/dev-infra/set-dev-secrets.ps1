<#
.SYNOPSIS
    Tüm MultiShop projelerinin veritabanı bağlantılarını docker-compose altyapısına yönlendirir.

.DESCRIPTION
    Şifreleri kökteki .env dosyasından okur ve her projenin user-secrets'ına yeni bağlantı dizesini yazar.
    Değiştirmeden önce her projenin secrets.json dosyasını "secrets.json.bak-<tarih>" olarak yedekler.
    Şifreler ekrana yazılmaz. Diğer user-secrets değerlerine (client secret, SMTP, POS vb.) dokunulmaz.

    Sıra: 1) docker compose up -d   2) migrate-legacy-data.ps1   3) bu script

.EXAMPLE
    .\scripts\dev-infra\set-dev-secrets.ps1
#>
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')

$envValues = Read-DotEnv
$dotnet = (Get-Command dotnet -ErrorAction SilentlyContinue).Source
if (-not $dotnet) { throw "dotnet komutu bulunamadı." }

$sa = $envValues['MSSQL_SA_PASSWORD']
$sqlBase = "Server=127.0.0.1,$($script:Ports.SqlServer);User ID=sa;Password=$sa;TrustServerCertificate=True"

$settings = @()
foreach ($service in $script:SqlServices) {
    $settings += @{ Name = $service.Name; Project = $service.Project; Key = $service.Key; Value = "$sqlBase;Database=$($service.TargetDb)" }
}
$settings += @{ Name = 'Catalog'; Project = $script:CatalogService.Project; Key = $script:CatalogService.Key; Value = "mongodb://localhost:$($script:Ports.Mongo)" }
$settings += @{ Name = 'Message'; Project = $script:MessageService.Project; Key = $script:MessageService.Key; Value = "Host=localhost;Port=$($script:Ports.Postgres);Database=$($script:MessageService.TargetDb);Username=$($envValues['POSTGRES_USER']);Password=$($envValues['POSTGRES_PASSWORD'])" }

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
Write-Step 'user-secrets güncelleniyor'
foreach ($item in $settings) {
    $secretsPath = Get-UserSecretsPath $item.Project
    if (Test-Path $secretsPath) { Copy-Item $secretsPath "$secretsPath.bak-$stamp" -Force }

    $projectPath = Join-Path $script:RepoRoot $item.Project
    & $dotnet user-secrets set $item.Key $item.Value --project $projectPath | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Fail "$($item.Name): $($item.Key) yazılamadı."; continue }
    Write-Ok ("{0,-9} {1}" -f $item.Name, $item.Key)
}

Write-Host "`nYedekler: %APPDATA%\Microsoft\UserSecrets\<id>\secrets.json.bak-$stamp" -ForegroundColor DarkGray
Write-Host "Geri almak için yedeği secrets.json üzerine kopyalamanız yeterli." -ForegroundColor DarkGray
Write-Host "`nBasket'in Redis portu appsettings.json'da $($script:Ports.Redis) olarak ayarlı (şifresiz)." -ForegroundColor Cyan
Write-Host "Şimdi Visual Studio'da 'New Profile' ile uygulamayı başlatıp test edin." -ForegroundColor Cyan
