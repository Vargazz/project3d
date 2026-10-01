# ── Stage 1: Build ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar arquivos de projeto primeiro (aproveita cache do Docker)
COPY src/Project3D.Domain/Project3D.Domain.csproj          src/Project3D.Domain/
COPY src/Project3D.Application/Project3D.Application.csproj src/Project3D.Application/
COPY src/Project3D.Infrastructure/Project3D.Infrastructure.csproj src/Project3D.Infrastructure/
COPY src/Project3D.API/Project3D.API.csproj                 src/Project3D.API/

# Restaurar dependências
RUN dotnet restore src/Project3D.API/Project3D.API.csproj

# Copiar todo o código-fonte
COPY src/ src/

# Publicar em modo Release
RUN dotnet publish src/Project3D.API/Project3D.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Criar pasta de uploads locais
RUN mkdir -p /app/uploads /app/wwwroot

# Copiar artefatos publicados
COPY --from=build /app/publish .

EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Project3D.API.dll"]
