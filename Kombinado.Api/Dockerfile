# Build context: repository root (see docker-compose.yaml)

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first so the dependency layer stays cached while only source code changes
COPY Kombinado.Api/Kombinado.Api.csproj Kombinado.Api/
RUN dotnet restore Kombinado.Api/Kombinado.Api.csproj

COPY Kombinado.Api/ Kombinado.Api/
RUN dotnet publish Kombinado.Api/Kombinado.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

# Non-root user shipped with the official .NET images
USER $APP_UID

ENTRYPOINT ["dotnet", "Kombinado.Api.dll"]
