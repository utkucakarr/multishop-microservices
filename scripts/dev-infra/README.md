# Geliştirme Altyapısı (docker-compose)

MultiShop'un ihtiyaç duyduğu tüm veritabanları ve altyapı servisleri kökteki `docker-compose.yml` ile tek komutla çalışır. Uygulama servisleri (Catalog, Basket, ... , WebUI) Visual Studio'dan **"New Profile"** ile başlatılır.

| Servis | Container | Bilgisayardan erişim | Kullanan |
|--------|-----------|----------------------|----------|
| SQL Server 2022 | `multishop-sqlserver` | `127.0.0.1,14330` (kullanıcı `sa`) | Identity, Order, Cargo, Comment, Discount, Payment |
| MongoDB 8 | `multishop-mongo` | `mongodb://localhost:27018` | Catalog |
| PostgreSQL 17 | `multishop-postgres` | `localhost:5440` | Message |
| Redis 7 | `multishop-redis` | `localhost:6380` | Basket |
| RabbitMQ 4 | `multishop-rabbitmq` | AMQP `5673`, panel http://localhost:15673 | (mesajlaşma, ileride) |
| Seq | `multishop-seq` | http://localhost:8091 | (merkezi loglar, ileride) |

Tüm portlar yalnızca `127.0.0.1`'e bağlıdır (Redis, MongoDB ve Seq geliştirme için şifresizdir; aynı ağdaki başka cihazlar erişemez). Portlar bilerek standart dışıdır; bilgisayarda kurulu SQL Server / MongoDB / PostgreSQL servisleri ve başka projelerin container'larıyla çakışmaz. Veriler adlandırılmış Docker volume'lerinde kalıcıdır (`docker compose down` verileri silmez; `docker compose down -v` siler).

## İlk kurulum

PowerShell'de, repo kök klasöründe:

```powershell
# 1) Şifre dosyası
Copy-Item .env.example .env      # sonra .env içindeki şifreleri değiştirin

# 2) Altyapıyı başlat ve hepsinin "healthy" olmasını bekle
docker compose up -d
docker compose ps

# 3a) Eski veritabanlarından veri taşınacaksa (bir kerelik)
.\scripts\dev-infra\migrate-legacy-data.ps1

# 4) Projelerin bağlantılarını yeni altyapıya yönlendir
.\scripts\dev-infra\set-dev-secrets.ps1

# 5) Migration'ları uygula (taşınan veride eksik kalanları; boş kurulumda tüm şemayı oluşturur)
.\scripts\dev-infra\apply-migrations.ps1
```

Taşınacak eski veri yoksa (yeni bilgisayar) 3a adımını atlayın.

> PowerShell script çalıştırmaya izin vermiyorsa bir kez: `Set-ExecutionPolicy -Scope CurrentUser RemoteSigned`

## Script'ler

| Script | Ne yapar |
|--------|----------|
| `migrate-legacy-data.ps1` | Projelerin **mevcut** user-secrets bağlantılarındaki veritabanlarını yeni container'lara kopyalar (SQL: COPY_ONLY yedek + geri yükleme, Mongo: mongodump/mongorestore, PostgreSQL: pg_dump/pg_restore). Kaynaklarda yalnızca okuma yapar. Hedef doluysa atlar; `-Force` ile üzerine yazar. |
| `set-dev-secrets.ps1` | `.env`'deki şifrelerle tüm projelerin bağlantı dizelerini user-secrets'a yazar. Önce `secrets.json.bak-<tarih>` yedeği alır. Diğer secret'lara dokunmaz. |
| `set-identity-secrets.ps1 -AdminEmail <e-posta>` | IdentityServer client secret'larını (Visitor, WebUI) rastgele üretip IdentityServer ve WebUI'a aynı değeri yazar; admin yapılacak hesabın e-postasını ayarlar; eski anahtarları (SharedSecret, JwtSettings:Key, Maneger/Admin client) siler. `-CreateAdmin`: hesap yoksa oluşturulsun (kullanıcı adı ve şifre sorulur). Önce yedek alır, değerleri ekrana yazmaz. |
| `apply-migrations.ps1` | `dotnet ef database update`'i tüm EF projeleri için sırayla çalıştırır. |
| `common.ps1` | Ortak yardımcılar (doğrudan çalıştırılmaz). |

## Sık karşılaşılanlar

- **`container name "/multishop-redis" is already in use`** — Eski elle oluşturulmuş Redis container'ı: `docker rm -f multishop-redis` (içinde yalnızca sepet verisi vardı).
- **SQL Server "unhealthy"** — `.env`'deki `MSSQL_SA_PASSWORD` SQL Server şifre kuralına uymuyor olabilir (en az 8 karakter; büyük/küçük harf, rakam, sembol). Şifre ilk açılışta volume'e yazılır; değiştirmek için `docker compose down`, `docker volume rm multishop_sqlserver-data`, ardından tekrar `up`.
- **Docker Desktop kapanıyor** — Bellek yetersizliği. Eski SQL Server container'ları (`IdentityDb`, `OrderDb`, ...) taşıma doğrulandıktan sonra durdurulabilir: `docker stop IdentityDb OrderDb CargoDb CommentDb DiscountDb`.
