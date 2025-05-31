# Stage 1: Build
# Kullanacağınız .NET SDK sürümünü belirtin (projenizle uyumlu olmalı)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# .csproj dosyasını ve varsa solution dosyasını kopyala
# Proje adınız FinTrackWebApi.csproj ise:
COPY ["FinTrackWebApi.csproj", "./"]
# Eğer projeniz başka projelere bağımlıysa, onları da kopyalayın
# Örnek: COPY ["../SharedLibrary/SharedLibrary.csproj", "../SharedLibrary/"]

# Bağımlılıkları yükle
RUN dotnet restore "./FinTrackWebApi.csproj"

# Tüm proje dosyalarını kopyala
COPY . .

# Projeyi publish et
# Proje adınız FinTrackWebApi.csproj ise:
RUN dotnet publish "FinTrackWebApi.csproj" -c Release -o /app/publish --no-restore

# Stage 2: Runtime
# Kullanacağınız .NET Runtime sürümünü belirtin (SDK ile uyumlu olmalı)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Uygulamanın dinleyeceği port (Kestrel varsayılanı 8080 veya 80'dir, 5000/5001 eski sürümlerdeydi)
# ASPNETCORE_URLS ile hangi portta dinleyeceğini belirtiyoruz.
# Dockerfile'da EXPOSE edilen port ile ASPNETCORE_URLS'deki port aynı olmalı.
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "FinTrackWebApi.dll"]