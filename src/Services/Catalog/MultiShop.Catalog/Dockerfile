# 1. AŞAMA: Çalışma Zamanı (Runtime) İmajı
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
USER app

# 2. AŞAMA: Derleme (Build) İmajı
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# DEĞİŞEN KISIM: Proje dosya adını MultiShop.Catalog.csproj olarak güncelledik
COPY ["MultiShop.Catalog.csproj", "."]
RUN dotnet restore "MultiShop.Catalog.csproj"

# Geri kalan tüm dosyaları kopyala
COPY . .

# DEĞİŞEN KISIM: Build komutundaki proje adını güncelledik
RUN dotnet build "MultiShop.Catalog.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 3. AŞAMA: Yayınlama (Publish)
# DEĞİŞEN KISIM: Publish komutundaki proje adını güncelledik
FROM build AS publish
RUN dotnet publish "MultiShop.Catalog.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 4. AŞAMA: Final İmajı
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# DEĞİŞEN KISIM: Uygulamanın giriş noktasındaki DLL adını güncelledik
ENTRYPOINT ["dotnet", "watch", "run", "--project", "MultiShop.Catalog.csproj", "--urls", "http://0.0.0.0:80"]