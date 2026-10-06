# MultiShop geliştirme altyapısı script'lerinin ortak yardımcıları.
# Windows PowerShell 5.1 ve PowerShell 7 ile çalışır.

$script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

# Hangi projenin hangi veritabanını kullandığı ve yeni altyapıdaki karşılığı
$script:SqlServices = @(
    @{ Name = 'Identity'; Project = 'src\Identity\MultiShop.IdentityServer\MultiShop.IdentityServer.csproj';             Key = 'ConnectionStrings:DefaultConnection'; TargetDb = 'MultiShopIdentityDb' },
    @{ Name = 'Order';    Project = 'src\Services\Order\Presentation\MultiShop.Order.WebApi\MultiShop.Order.WebApi.csproj'; Key = 'ConnectionStrings:OrderConnection';   TargetDb = 'MultiShopOrderDb' },
    @{ Name = 'Cargo';    Project = 'src\Services\Cargo\MultiShop.Cargo.WebApi\MultiShop.Cargo.WebApi.csproj';             Key = 'ConnectionStrings:DefaultConnection'; TargetDb = 'MultiShopCargoDb' },
    @{ Name = 'Comment';  Project = 'src\Services\Comment\MultiShop.Comment\MultiShop.Comment.csproj';                     Key = 'ConnectionStrings:DefaultConnection'; TargetDb = 'MultiShopCommentDb' },
    @{ Name = 'Discount'; Project = 'src\Services\Discount\MultiShop.Discount\MultiShop.Discount.csproj';                  Key = 'ConnectionStrings:DefaultConnection'; TargetDb = 'MultiShopDiscountDb' },
    @{ Name = 'Payment';  Project = 'src\Services\Payment\MultiShop.Payment\MultiShop.Payment.csproj';                     Key = 'ConnectionStrings:DefaultConnection'; TargetDb = 'MultiShopPaymentDb' }
)
$script:CatalogService = @{ Name = 'Catalog'; Project = 'src\Services\Catalog\MultiShop.Catalog\MultiShop.Catalog.csproj'; Key = 'DataBaseSettings:ConnectionString' }
$script:MessageService = @{ Name = 'Message'; Project = 'src\Services\Message\MultiShop.Message\MultiShop.Message.csproj'; Key = 'ConnectionStrings:DefaultConnection'; TargetDb = 'MultiShopMessageDb' }

$script:Ports = @{ SqlServer = 14330; Mongo = 27018; Postgres = 5440; Redis = 6380 }

function Write-Step([string]$Message) { Write-Host "`n==> $Message" -ForegroundColor Cyan }
function Write-Ok([string]$Message)   { Write-Host "    [OK] $Message" -ForegroundColor Green }
function Write-Skip([string]$Message) { Write-Host "    [ATLANDI] $Message" -ForegroundColor Yellow }
function Write-Fail([string]$Message) { Write-Host "    [HATA] $Message" -ForegroundColor Red }

function Read-DotEnv {
    $path = Join-Path $script:RepoRoot '.env'
    if (-not (Test-Path $path)) {
        throw ".env bulunamadı. Kökteki .env.example dosyasını .env olarak kopyalayıp şifreleri girin."
    }
    $values = @{}
    foreach ($line in Get-Content $path -Encoding UTF8) {
        $trimmed = $line.Trim()
        if ($trimmed -eq '' -or $trimmed.StartsWith('#')) { continue }
        $index = $trimmed.IndexOf('=')
        if ($index -lt 1) { continue }
        $values[$trimmed.Substring(0, $index).Trim()] = $trimmed.Substring($index + 1).Trim()
    }
    foreach ($key in 'MSSQL_SA_PASSWORD', 'POSTGRES_PASSWORD') {
        if (-not $values[$key]) { throw ".env içinde $key tanımlı değil." }
        if ($values[$key] -match '[;''"$# ]') { throw (".env içindeki $key şu karakterleri içermemeli: " + '; '' " $ # boşluk') }
    }
    if (-not $values['POSTGRES_USER']) { $values['POSTGRES_USER'] = 'multishop' }
    return $values
}

function Get-UserSecretsPath([string]$ProjectRelativePath) {
    $csproj = Join-Path $script:RepoRoot $ProjectRelativePath
    [xml]$xml = Get-Content $csproj -Encoding UTF8
    $id = ($xml.Project.PropertyGroup | ForEach-Object { $_.UserSecretsId } | Where-Object { $_ }) | Select-Object -First 1
    if (-not $id) { throw "$ProjectRelativePath içinde UserSecretsId yok." }
    return Join-Path $env:APPDATA "Microsoft\UserSecrets\$id\secrets.json"
}

# user-secrets'tan bir değeri okur; hem düz ("A:B": "x") hem iç içe ({"A": {"B": "x"}}) yazımı destekler.
function Get-UserSecret([string]$ProjectRelativePath, [string]$Key) {
    $path = Get-UserSecretsPath $ProjectRelativePath
    if (-not (Test-Path $path)) { return $null }
    $json = Get-Content $path -Raw -Encoding UTF8 | ConvertFrom-Json
    $flat = $json.PSObject.Properties | Where-Object { $_.Name -eq $Key } | Select-Object -First 1
    if ($flat) { return [string]$flat.Value }
    $node = $json
    foreach ($part in $Key.Split(':')) {
        if ($null -eq $node) { return $null }
        $prop = $node.PSObject.Properties | Where-Object { $_.Name -eq $part } | Select-Object -First 1
        if (-not $prop) { return $null }
        $node = $prop.Value
    }
    return [string]$node
}

# "Key=Value;Key2=Value2" biçimindeki bağlantı dizesini sözlüğe çevirir (anahtarlar küçük harf).
function ConvertFrom-ConnectionString([string]$ConnectionString) {
    $result = @{}
    foreach ($part in $ConnectionString.Split(';')) {
        $index = $part.IndexOf('=')
        if ($index -lt 1) { continue }
        $result[$part.Substring(0, $index).Trim().ToLowerInvariant()] = $part.Substring($index + 1).Trim()
    }
    return $result
}

function Get-Docker {
    $docker = Get-Command docker -ErrorAction SilentlyContinue
    if (-not $docker) { throw "docker komutu bulunamadı. Docker Desktop açık mı?" }
    return $docker.Source
}

function Assert-ContainerRunning([string]$Name) {
    $state = & (Get-Docker) inspect -f '{{.State.Status}}' $Name 2>$null
    if ($state -ne 'running') { throw "$Name container'ı çalışmıyor. Önce kökte 'docker compose up -d' çalıştırın." }
}

function Invoke-SqlQuery([string]$ConnectionString, [string]$Sql, [switch]$Scalar) {
    $connection = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
    $connection.Open()
    try {
        $command = $connection.CreateCommand()
        $command.CommandText = $Sql
        $command.CommandTimeout = 600
        if ($Scalar) { return $command.ExecuteScalar() }
        $table = New-Object System.Data.DataTable
        $table.Load($command.ExecuteReader())
        return ,$table
    }
    finally { $connection.Close() }
}
