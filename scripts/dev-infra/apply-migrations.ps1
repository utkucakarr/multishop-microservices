<#
.SYNOPSIS
    Tüm EF Core migration'larını (Identity, Order, Cargo, Comment, Discount, Payment, Message) uygular.

.DESCRIPTION
    Bağlantı dizeleri projelerin user-secrets'ından okunur (önce set-dev-secrets.ps1 çalıştırılmış olmalı).
    Taşınmış veritabanlarında yalnızca eksik migration'ları uygular; boş bir kurulumda şemayı sıfırdan oluşturur.
    Catalog (MongoDB) ve Basket (Redis) migration kullanmaz.

.EXAMPLE
    .\scripts\dev-infra\apply-migrations.ps1
#>
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')

$targets = @(
    @{ Name = 'Identity'; Project = 'src\Identity\MultiShop.IdentityServer' },
    @{ Name = 'Order';    Project = 'src\Services\Order\Infrastructure\MultiShop.Order.Persistence'; Startup = 'src\Services\Order\Presentation\MultiShop.Order.WebApi' },
    @{ Name = 'Cargo';    Project = 'src\Services\Cargo\MultiShop.Cargo.DataAccessLayer';           Startup = 'src\Services\Cargo\MultiShop.Cargo.WebApi' },
    @{ Name = 'Comment';  Project = 'src\Services\Comment\MultiShop.Comment' },
    @{ Name = 'Discount'; Project = 'src\Services\Discount\MultiShop.Discount' },
    @{ Name = 'Payment';  Project = 'src\Services\Payment\MultiShop.Payment' },
    @{ Name = 'Message';  Project = 'src\Services\Message\MultiShop.Message' }
)

& dotnet ef --version | Out-Null
if ($LASTEXITCODE -ne 0) { throw "dotnet-ef aracı yok. Kurulum: dotnet tool install --global dotnet-ef" }

$failed = $false
foreach ($target in $targets) {
    Write-Step "$($target.Name)"
    $arguments = @('ef', 'database', 'update', '--project', (Join-Path $script:RepoRoot $target.Project))
    if ($target.Startup) { $arguments += @('--startup-project', (Join-Path $script:RepoRoot $target.Startup)) }
    & dotnet @arguments | Where-Object { $_ -match 'Applying migration|No migrations were applied|Done\.|error|Error' } | ForEach-Object { "    $_" } | Write-Host
    if ($LASTEXITCODE -ne 0) { Write-Fail "$($target.Name): migration uygulanamadı."; $failed = $true } else { Write-Ok $target.Name }
}

if ($failed) { exit 1 }
