# MultiShop Mikroservis Projesi 

[![CI](https://github.com/utkucakarr/multishop-microservices/actions/workflows/ci.yml/badge.svg)](https://github.com/utkucakarr/multishop-microservices/actions/workflows/ci.yml)

## Proje Hakkında

**MultiShop**, kullanıcıların oturum açarak veya ziyaretçi olarak siteye giriş yapmalarını sağlayan kapsamlı bir e-ticaret platformudur. Kullanıcılar, ürünler içerisinden diledikleri ürünleri arayabilir, listeleyebilir ve sepetlerine ekleyebilirler. Alışveriş sürecinin sonunda, kullanıcılar siparişlerini güvenle oluşturabilir ve takip edebilirler.

Bu proje, mikro hizmetler mimarisi kullanılarak geliştirilmiştir ve **ASP.NET Core 6.0 Web API** ile inşa edilmiştir.

---

# Kullanılan Teknolojiler - Yöntem - Kütüphane vb.

## Backend Teknolojileri
- ASP.NET Core Web API - MVC
- IdentityServer4
- JSON Web Token (JWT) & OAuth 2.0
- RabbitMQ
- SignalR
- Ocelot Gateway
- MailKit
- EF Core
- Dapper
- Docker
- Garanti Sanal Post Hizmeti

## Frontend Teknolojileri
- HTML
- CSS
- JavaScript
- Bootstrap

## Veritabanları
- MongoDB
- SQL Server
- PostgreSQL
- Redis

## Mimari ve Tasarım Desenleri
- N-Tier Architecture
- Onion Architecture
- CQRS
- Mediator
- Repository Design Pattern

## Mikroservisler
-	Basket - Docker Redis
-	Cargo - Docker MsSql
-	Catalog - MongoDb (ürün görselleri: yerel disk ya da Google Cloud Storage)
-	Comment - Docker MsSql
-	Discount - Local MsSql Dapper ORM
-	Message - PostgreSql
-	IdentityServer4 -Docker MsSql
-	RabbitMq
-	SignalR
-	RapidApi
## Görseller

![LoginPage](https://github.com/user-attachments/assets/92107a10-717f-4186-b541-8d557d82324c)
![Anasayfa](https://github.com/user-attachments/assets/4a8034cb-eb9a-469b-b6cd-439599caad59)
![BasketPage](https://github.com/user-attachments/assets/c9880ff5-e873-4b37-8827-03335c32addc)
![Payment](https://github.com/user-attachments/assets/98b2a7cd-d3e1-44e9-af64-fd231cadeb56)
![Kategori](https://github.com/user-attachments/assets/a8e834e6-05b0-45c1-a23a-2cfd8e20a6a9)
![UserPanel](https://github.com/user-attachments/assets/cbd6dc7e-c119-470b-9901-0c1f55d817b6)
