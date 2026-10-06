<#
.SYNOPSIS
    Eski veritabanlarındaki MultiShop verilerini docker-compose ile açılan yeni container'lara kopyalar.

.DESCRIPTION
    Kaynak bağlantı bilgileri projelerin user-secrets dosyalarından okunur; şifreler ekrana yazılmaz.
    Kaynaklarda yalnızca okuma yapılır (COPY_ONLY yedek, mongodump, pg_dump); eski veriler değişmez.

      SQL Server (Identity, Order, Cargo, Comment, Discount, Payment) -> multishop-sqlserver (14330)
      MongoDB    (Catalog)                                             -> multishop-mongo     (27018)
      PostgreSQL (Message)                                             -> multishop-postgres  (5440)

    Sıra: 1) docker compose up -d   2) bu script   3) set-dev-secrets.ps1

.PARAMETER Force
    Hedefte aynı veritabanı zaten doluysa üzerine yazar. Varsayılan: atlar.

.EXAMPLE
    .\scripts\dev-infra\migrate-legacy-data.ps1
#>
param([switch]$Force)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'common.ps1')

$script:Docker = Get-Docker
function Invoke-Docker {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]]$DockerArgs)
    $output = & $script:Docker @DockerArgs
    if ($LASTEXITCODE -ne 0) { throw "docker $($DockerArgs[0]) başarısız oldu (çıkış kodu $LASTEXITCODE)." }
    return $output
}

function Get-FirstValue([hashtable]$Table, [string[]]$Keys) {
    foreach ($key in $Keys) { if ($Table[$key]) { return $Table[$key] } }
    return $null
}

function Find-ContainerForSqlServer([string]$ServerName, [string]$DataSource) {
    # Docker'daki SQL Server'da @@SERVERNAME, container ID'sinin ilk 12 karakteridir.
    foreach ($line in (Invoke-Docker ps --format '{{.ID}} {{.Names}}')) {
        $id, $name = $line.Split(' ', 2)
        if ($ServerName -and ($id.StartsWith($ServerName.ToLowerInvariant()) -or $ServerName.ToLowerInvariant().StartsWith($id))) { return $name }
    }
    # Yedek yöntem: bağlantı dizesindeki porta yayınlanmış container
    if ($DataSource -match ',(\d+)$') {
        $port = $Matches[1]
        foreach ($line in (Invoke-Docker ps --format '{{.Names}} {{.Ports}}')) {
            if ($line -match ":$port->1433/tcp") { return $line.Split(' ')[0] }
        }
    }
    throw "Kaynak SQL Server bir Docker container'ında görünüyor ama hangisi olduğu bulunamadı ($DataSource)."
}

function Copy-SqlDatabase($Service, [string]$SaPassword, [string]$WorkDir) {
    $source = Get-UserSecret $Service.Project $Service.Key
    if (-not $source) { Write-Skip "$($Service.Name): user-secrets içinde $($Service.Key) yok."; return 'skipped' }

    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $source
    if ($builder.DataSource -match "[,:]$($script:Ports.SqlServer)$") { Write-Skip "$($Service.Name): zaten yeni SQL Server'ı gösteriyor."; return 'skipped' }
    $sourceDb = $builder.InitialCatalog
    if (-not $sourceDb) { throw "$($Service.Name): bağlantı dizesinde veritabanı adı yok." }
    $builder.InitialCatalog = 'master'
    $builder.ConnectTimeout = 15
    $sourceMaster = $builder.ConnectionString

    $targetDb = $Service.TargetDb
    $target = "Server=127.0.0.1,$($script:Ports.SqlServer);Database=master;User ID=sa;Password=$SaPassword;TrustServerCertificate=True;Connect Timeout=15"
    $existing = Invoke-SqlQuery $target "SELECT DB_ID(N'$targetDb')" -Scalar
    if (($existing -isnot [DBNull]) -and -not $Force) { Write-Skip "$($Service.Name): hedefte $targetDb zaten var (üzerine yazmak için -Force)."; return 'skipped' }

    $info = (Invoke-SqlQuery $sourceMaster @"
SELECT @@SERVERNAME AS ServerName,
       CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(4000)) AS BackupPath,
       (SELECT TOP 1 host_platform FROM sys.dm_os_host_info) AS Platform,
       DB_ID(N'$($sourceDb.Replace("'", "''"))') AS DbId
"@).Rows[0]
    if ($info.DbId -is [DBNull]) { Write-Skip "$($Service.Name): kaynakta '$sourceDb' veritabanı yok ($($builder.DataSource))."; return 'skipped' }

    $fileName = "$($targetDb)_kt5.bak"
    $localFile = Join-Path $WorkDir $fileName
    $escapedSourceDb = $sourceDb.Replace(']', ']]')

    if ($info.Platform -eq 'Linux') {
        $container = Find-ContainerForSqlServer $info.ServerName $builder.DataSource
        $remote = "/var/opt/mssql/data/$fileName"
        Invoke-SqlQuery $sourceMaster "BACKUP DATABASE [$escapedSourceDb] TO DISK = N'$remote' WITH COPY_ONLY, INIT" | Out-Null
        Invoke-Docker cp "$($container):$remote" $localFile | Out-Null
        Invoke-Docker exec -u root $container rm -f $remote | Out-Null
        $origin = "container $container"
    }
    else {
        $remote = Join-Path $info.BackupPath $fileName
        Invoke-SqlQuery $sourceMaster "BACKUP DATABASE [$escapedSourceDb] TO DISK = N'$remote' WITH COPY_ONLY, INIT" | Out-Null
        try { Copy-Item $remote $localFile -Force }
        catch { throw "$($Service.Name): yedek dosyası okunamadı ($remote). PowerShell'i 'Yönetici olarak' açıp tekrar deneyin." }
        Remove-Item $remote -ErrorAction SilentlyContinue
        $origin = "yerel SQL Server ($($builder.DataSource))"
    }

    Invoke-Docker cp $localFile "multishop-sqlserver:/tmp/$fileName" | Out-Null
    Invoke-Docker exec -u root multishop-sqlserver chmod 644 "/tmp/$fileName" | Out-Null

    $moves = @()
    $dataIndex = 0; $logIndex = 0
    foreach ($row in (Invoke-SqlQuery $target "RESTORE FILELISTONLY FROM DISK = N'/tmp/$fileName'").Rows) {
        if ($row.Type -eq 'L') {
            $suffix = if ($logIndex -eq 0) { '' } else { "_$logIndex" }; $logIndex++
            $moves += "MOVE N'$($row.LogicalName)' TO N'/var/opt/mssql/data/$($targetDb)_log$suffix.ldf'"
        }
        else {
            $suffix = if ($dataIndex -eq 0) { '' } else { "_$dataIndex" }; $dataIndex++
            $moves += "MOVE N'$($row.LogicalName)' TO N'/var/opt/mssql/data/$targetDb$suffix.mdf'"
        }
    }
    Invoke-SqlQuery $target "RESTORE DATABASE [$targetDb] FROM DISK = N'/tmp/$fileName' WITH $($moves -join ', '), REPLACE, RECOVERY" | Out-Null
    Invoke-SqlQuery $target "ALTER AUTHORIZATION ON DATABASE::[$targetDb] TO sa" | Out-Null
    Invoke-Docker exec -u root multishop-sqlserver rm -f "/tmp/$fileName" | Out-Null
    Remove-Item $localFile -ErrorAction SilentlyContinue

    $tableCount = Invoke-SqlQuery $target "SELECT COUNT(*) FROM [$targetDb].sys.tables" -Scalar
    Write-Ok "$($Service.Name): '$sourceDb' ($origin) -> $targetDb ($tableCount tablo)"
    return 'copied'
}

function Copy-MongoDatabase {
    $source = Get-UserSecret $script:CatalogService.Project $script:CatalogService.Key
    if (-not $source) { Write-Skip "Catalog: user-secrets içinde $($script:CatalogService.Key) yok."; return 'skipped' }
    if ($source -match ":$($script:Ports.Mongo)(/|$|\?)") { Write-Skip "Catalog: zaten yeni MongoDB'yi gösteriyor."; return 'skipped' }

    $appSettings = Get-Content (Join-Path $script:RepoRoot 'src\Services\Catalog\MultiShop.Catalog\appsettings.json') -Raw -Encoding UTF8
    if ($appSettings -notmatch '"DatabaseName"\s*:\s*"([^"]+)"') { throw "Catalog appsettings.json içinde DatabaseName bulunamadı." }
    $databaseName = $Matches[1]

    $existing = Invoke-Docker exec multishop-mongo mongosh --quiet --eval "db.getSiblingDB('$databaseName').getCollectionNames().length"
    if ([int]($existing | Select-Object -Last 1) -gt 0 -and -not $Force) { Write-Skip "Catalog: hedefte $databaseName zaten dolu (üzerine yazmak için -Force)."; return 'skipped' }

    # Container içinden bilgisayardaki MongoDB'ye host.docker.internal ile ulaşılır.
    $sourceUri = $source -replace '(?i)(//|@)(localhost|127\.0\.0\.1)(?=[:/,?]|$)', '$1host.docker.internal'
    $dbArgument = if ($sourceUri -match '^mongodb(\+srv)?://[^/]+/[^?/]+') { '' } else { "--db=$databaseName" }
    $dropArgument = if ($Force) { '--drop' } else { '' }
    Invoke-Docker exec multishop-mongo sh -c "mongodump --uri='$sourceUri' $dbArgument --archive --quiet | mongorestore --archive --quiet $dropArgument --nsInclude='$databaseName.*'" | Out-Null

    $collections = Invoke-Docker exec multishop-mongo mongosh --quiet --eval "db.getSiblingDB('$databaseName').getCollectionNames().length"
    $products = Invoke-Docker exec multishop-mongo mongosh --quiet --eval "db.getSiblingDB('$databaseName').getCollection('Products').countDocuments()"
    Write-Ok "Catalog: $databaseName -> multishop-mongo ($($collections | Select-Object -Last 1) koleksiyon, Products: $($products | Select-Object -Last 1))"
    return 'copied'
}

function Copy-PostgresDatabase([hashtable]$EnvValues, [string]$WorkDir) {
    $source = Get-UserSecret $script:MessageService.Project $script:MessageService.Key
    if (-not $source) { Write-Skip "Message: user-secrets içinde $($script:MessageService.Key) yok."; return 'skipped' }

    $cs = ConvertFrom-ConnectionString $source
    $sourceHost = Get-FirstValue $cs @('host', 'server')
    $sourcePort = Get-FirstValue $cs @('port'); if (-not $sourcePort) { $sourcePort = '5432' }
    $sourceDb = Get-FirstValue $cs @('database', 'db')
    $sourceUser = Get-FirstValue $cs @('username', 'user id', 'userid', 'user', 'uid')
    $sourcePassword = Get-FirstValue $cs @('password', 'pwd')
    if ($sourcePort -eq "$($script:Ports.Postgres)") { Write-Skip "Message: zaten yeni PostgreSQL'i gösteriyor."; return 'skipped' }

    $targetDb = $script:MessageService.TargetDb
    $pgUser = $EnvValues['POSTGRES_USER']
    $tableCount = Invoke-Docker exec multishop-postgres psql -U $pgUser -d $targetDb -tAc "SELECT count(*) FROM information_schema.tables WHERE table_schema = 'public'"
    if ([int](($tableCount | Select-Object -Last 1).Trim()) -gt 0) {
        if (-not $Force) { Write-Skip "Message: hedefte $targetDb zaten dolu (üzerine yazmak için -Force)."; return 'skipped' }
        Invoke-Docker exec multishop-postgres dropdb -U $pgUser --force $targetDb | Out-Null
        Invoke-Docker exec multishop-postgres createdb -U $pgUser $targetDb | Out-Null
    }

    $dumpName = 'message_kt5.dump'
    $localPgDump = Get-ChildItem 'C:\Program Files\PostgreSQL\*\bin\pg_dump.exe' -ErrorAction SilentlyContinue | Sort-Object FullName -Descending | Select-Object -First 1
    if ($localPgDump -and $sourceHost -in @('localhost', '127.0.0.1')) {
        $localFile = Join-Path $WorkDir $dumpName
        $env:PGPASSWORD = $sourcePassword
        try { & $localPgDump.FullName -h $sourceHost -p $sourcePort -U $sourceUser -d $sourceDb -Fc -f $localFile }
        finally { Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue }
        if ($LASTEXITCODE -ne 0) { throw "Message: pg_dump başarısız oldu." }
        Invoke-Docker cp $localFile "multishop-postgres:/tmp/$dumpName" | Out-Null
        Remove-Item $localFile -ErrorAction SilentlyContinue
    }
    else {
        $containerHost = if ($sourceHost -in @('localhost', '127.0.0.1')) { 'host.docker.internal' } else { $sourceHost }
        Invoke-Docker exec -e "PGPASSWORD=$sourcePassword" multishop-postgres pg_dump -h $containerHost -p $sourcePort -U $sourceUser -d $sourceDb -Fc -f "/tmp/$dumpName" | Out-Null
    }

    Invoke-Docker exec multishop-postgres pg_restore -U $pgUser -d $targetDb --no-owner --no-privileges "/tmp/$dumpName" | Out-Null
    Invoke-Docker exec multishop-postgres rm -f "/tmp/$dumpName" | Out-Null

    $tables = Invoke-Docker exec multishop-postgres psql -U $pgUser -d $targetDb -tAc "SELECT count(*) FROM information_schema.tables WHERE table_schema = 'public'"
    Write-Ok "Message: '$sourceDb' -> $targetDb ($((($tables | Select-Object -Last 1)).Trim()) tablo)"
    return 'copied'
}

# ---------------------------------------------------------------------------

$envValues = Read-DotEnv
foreach ($name in 'multishop-sqlserver', 'multishop-mongo', 'multishop-postgres') { Assert-ContainerRunning $name }

$workDir = Join-Path $env:TEMP 'multishop-kt5'
New-Item -ItemType Directory -Force -Path $workDir | Out-Null
$results = [ordered]@{}

Write-Step 'SQL Server veritabanları'
foreach ($service in $script:SqlServices) {
    try { $results[$service.Name] = Copy-SqlDatabase $service $envValues['MSSQL_SA_PASSWORD'] $workDir }
    catch { Write-Fail "$($service.Name): $($_.Exception.Message)"; $results[$service.Name] = 'failed' }
}

Write-Step 'MongoDB (Catalog)'
try { $results['Catalog'] = Copy-MongoDatabase } catch { Write-Fail "Catalog: $($_.Exception.Message)"; $results['Catalog'] = 'failed' }

Write-Step 'PostgreSQL (Message)'
try { $results['Message'] = Copy-PostgresDatabase $envValues $workDir } catch { Write-Fail "Message: $($_.Exception.Message)"; $results['Message'] = 'failed' }

Remove-Item $workDir -Recurse -Force -ErrorAction SilentlyContinue

Write-Step 'Özet'
$results.GetEnumerator() | ForEach-Object { '    {0,-10} {1}' -f $_.Key, $_.Value } | Write-Host
if ($results.Values -contains 'failed') {
    Write-Host "`nBazı veritabanları taşınamadı. Hata mesajlarını kontrol edip script'i tekrar çalıştırabilirsiniz; taşınanlar atlanır." -ForegroundColor Yellow
    exit 1
}
Write-Host "`nSonraki adım: .\scripts\dev-infra\set-dev-secrets.ps1" -ForegroundColor Cyan
