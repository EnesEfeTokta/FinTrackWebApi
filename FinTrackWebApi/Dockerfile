FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

WORKDIR /app

COPY *.sln .
COPY FinTrackWebApi/*.csproj ./FinTrackWebApi/

RUN dotnet restore "FinTrackWebApi/FinTrackWebApi.csproj"

COPY . .

WORKDIR "/app/FinTrackWebApi"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime-env

WORKDIR /app

COPY --from=build-env /app/publish .

# Uygulamanızın dinleyeceği portları açın
# HTTP kullanıyorsanız
EXPOSE 8080
# HTTPS kullanıyorsanız
EXPOSE 8081 

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS="http://+:8080;https://+:8081" 

ENTRYPOINT ["dotnet", "FinTrackWebApi.dll"]