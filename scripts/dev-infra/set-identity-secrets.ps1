<#
.SYNOPSIS
    IdentityServer client secret'larını üretir ve admin kullanıcısını ayarlar (KT-7).

.DESCRIPTION
    - Visitor ve WebUI client'ları için rastgele secret üretir; aynı değeri hem IdentityServer'ın hem WebUI'ın
      user-secrets'ına yazar. Secret zaten varsa dokunmaz (-Regenerate ile yenilenir).
    - Admin yapılacak kullanıcının e-posta adresini (AdminUser:Email) IdentityServer'a yazar. IdentityServer her
      açılışta bu adresteki kullanıcıya Admin rolü verir.
    - Artık kullanılmayan anahtarları siler: ClientSecrets:SharedSecret, JwtSettings:Key (IdentityServer);
      ClientSettings:MultiShopManegerClient, ClientSettings:MultiShopAdminClient (WebUI).
    - Değiştirmeden önce iki projenin secrets.json dosyasını yedekler. Hiçbir değer ekrana yazılmaz.

.PARAMETER AdminEmail
    Admin yapılacak kullanıcının e-posta adresi (mevcut hesabının adresi).

.PARAMETER CreateAdmin
    Bu e-postayla bir kullanıcı yoksa IdentityServer onu oluştursun; kullanıcı adı ve şifre sorulur.

.PARAMETER Regenerate
    Var olan client secret'ları da yenile.

.EXAMPLE
    .\scripts\dev-infra\set-identity-secrets.ps1 -AdminEmail ben@ornek.com
#>
param(
    [Parameter(Mandatory = $true)] [string]$AdminEmail,
    [switch]$CreateAdmin,
    [switch]$Regenerate
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')

$identity = 'src\Identity\MultiShop.IdentityServer\MultiShop.IdentityServer.csproj'
$webui = 'src\Web\MultiShop.WebUI\MultiShop.WebUI.csproj'

# 64 karakter hex: komut satırında "-" ile başlayıp seçenek sanılma ya da kaçış gerektirme riski yok.
function New-ClientSecret {
    $bytes = New-Object byte[] 32
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    return -join ($bytes | ForEach-Object { $_.ToString('x2') })
}

function Set-Secret([string]$Project, [string]$Key, [string]$Value) {
    & dotnet user-secrets set $Key $Value --project (Join-Path $script:RepoRoot $Project) | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "$Key yazılamadı ($Project)." }
}

function Remove-Secret([string]$Project, [string]$Key) {
    if ($null -ne (Get-UserSecret $Project $Key)) {
        & dotnet user-secrets remove $Key --project (Join-Path $script:RepoRoot $Project) | Out-Null
        Write-Ok "Silindi: $Key"
    }
}

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
Write-Step 'secrets.json yedekleniyor'
foreach ($project in $identity, $webui) {
    $path = Get-UserSecretsPath $project
    if (Test-Path $path) {
        Copy-Item $path "$path.bak-$stamp" -Force
        Write-Ok "$(Split-Path $project -Leaf) -> secrets.json.bak-$stamp"
    }
}

Write-Step 'Client secret''ları'
$clients = @(
    @{ Name = 'Visitor'; IdentityKey = 'ClientSecrets:Visitor'; WebUIKey = 'ClientSettings:MultiShopVisitorClient:ClientSecret' },
    @{ Name = 'WebUI';   IdentityKey = 'ClientSecrets:WebUI';   WebUIKey = 'ClientSettings:MultiShopWebUIClient:ClientSecret' }
)
foreach ($client in $clients) {
    $secret = Get-UserSecret $identity $client.IdentityKey
    if ($Regenerate -or [string]::IsNullOrEmpty($secret)) {
        $secret = New-ClientSecret
        Set-Secret $identity $client.IdentityKey $secret
        Write-Ok "$($client.Name): yeni secret üretildi"
    }
    else {
        Write-Skip "$($client.Name): secret zaten var, WebUI ile eşitleniyor"
    }
    Set-Secret $webui $client.WebUIKey $secret
}

Write-Step 'Admin kullanıcısı'
Set-Secret $identity 'AdminUser:Email' $AdminEmail
Write-Ok "AdminUser:Email = $AdminEmail"
if ($CreateAdmin) {
    $userName = Read-Host 'Admin kullanıcı adı'
    $securePassword = Read-Host 'Admin şifresi (en az 6 karakter; büyük/küçük harf, rakam ve sembol içermeli)' -AsSecureString
    $bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($securePassword)
    try { $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr) } finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) }
    Set-Secret $identity 'AdminUser:UserName' $userName
    Set-Secret $identity 'AdminUser:Password' $password
    Write-Ok 'AdminUser:UserName ve AdminUser:Password yazıldı (kullanıcı IdentityServer ilk açıldığında oluşturulur)'
}

Write-Step 'Kullanılmayan anahtarlar'
Remove-Secret $identity 'ClientSecrets:SharedSecret'
Remove-Secret $identity 'JwtSettings:Key'
foreach ($key in 'ClientSettings:MultiShopManegerClient:ClientId', 'ClientSettings:MultiShopManegerClient:ClientSecret',
                 'ClientSettings:MultiShopAdminClient:ClientId', 'ClientSettings:MultiShopAdminClient:ClientSecret') {
    Remove-Secret $webui $key
}

Write-Host "`nGeri almak için yedeği (secrets.json.bak-$stamp) secrets.json üzerine kopyalamanız yeterli." -ForegroundColor DarkGray
Write-Host "Şimdi IdentityServer'ı ve WebUI'ı yeniden başlatın; tüm kullanıcıların bir kez yeniden giriş yapması gerekir." -ForegroundColor Cyan
