# ============================================
# Etapa de construcción (Build Stage)
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solo el archivo de proyecto primero para cachear restore
COPY ["HoneyBack.csproj", "./"]
RUN dotnet restore "HoneyBack.csproj"

# Copiar el resto de los archivos fuente
COPY . .

# Compilar en Release
RUN dotnet build "HoneyBack.csproj" -c Release -o /app/build --no-restore

# ============================================
# Etapa de publicación (Publish Stage)
# ============================================
FROM build AS publish
RUN dotnet publish "HoneyBack.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# ============================================
# Etapa final - Runtime optimizado
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Instalar curl para healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Establecer variables de entorno
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_RUNNING_IN_CONTAINER=true

WORKDIR /app

# Exponer puerto HTTP (HTTPS se maneja por reverse proxy en producción)
EXPOSE 80

# Crear usuario no-root para seguridad (opcional pero recomendado)
# RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
# USER appuser

# Copiar archivos publicados desde la etapa anterior
COPY --from=publish /app/publish .

# Health check para Docker y orquestadores
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl --fail http://localhost:80/health || exit 1

# Punto de entrada
ENTRYPOINT ["dotnet", "HoneyBack.dll"]
