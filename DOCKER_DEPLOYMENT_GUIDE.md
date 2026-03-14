# 📦 GUÍA DE DESPLIEGUE DOCKER - HoneyBack API

## 📋 Tabla de Contenidos

1. [Requisitos Previos](#1-requisitos-previos)
2. [Configuración Local (Sin Docker)](#2-configuración-local-sin-docker)
3. [Construcción del Contenedor](#3-construcción-del-contenedor)
4. [Ejecución con Docker Compose](#4-ejecución-con-docker-compose)
5. [Pruebas y Verificación](#5-pruebas-y-verificación)
6. [Publicación en Docker Hub](#6-publicación-en-docker-hub)
7. [Ejecutar desde Docker Hub](#7-ejecutar-desde-docker-hub)
8. [Solución de Problemas](#8-solución-de-problemas)
9. [Configuración de Producción](#9-configuración-de-producción)

---

## 1. Requisitos Previos

### Software Necesario

| Software | Versión Mínima | Verificar Instalación |
|----------|---------------|----------------------|
| Docker Desktop | 4.0+ | `docker --version` |
| Docker Compose | 2.0+ | `docker compose version` |
| .NET SDK | 8.0 | `dotnet --version` |
| Git (opcional) | 2.0+ | `git --version` |

### Verificación Rápida

```powershell
# Ejecutar en PowerShell
docker --version
docker compose version
dotnet --version
```

### Configuración de Docker Desktop

1. Abrir **Docker Desktop**
2. Ir a **Settings** → **Resources**
3. Asignar al menos:
   - **CPUs:** 2
   - **Memory:** 4 GB (SQL Server requiere 2GB mínimo)
   - **Disk:** 20 GB
4. Guardar y reiniciar Docker

---

## 2. Configuración Local (Sin Docker)

### 2.1 Configurar User Secrets (Desarrollo)

```powershell
# Navegar al directorio del proyecto
cd C:\Users\johan\source\repos\HoneyBack

# Configurar JWT Key (requerido)
dotnet user-secrets set "Jwt:Key" "Tu-Clave-Secreta-Segura-De-Al-Menos-32-Caracteres!"

# Configurar Resend API Key (opcional - para emails)
dotnet user-secrets set "Resend:ApiKey" "re_xxxxxxxxxxxx"

# Verificar secrets configurados
dotnet user-secrets list
```

### 2.2 Ejecutar Localmente

```powershell
# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run

# La API estará disponible en:
# - http://localhost:5291
# - Swagger UI: http://localhost:5291/swagger
```

### 2.3 Verificar Funcionamiento Local

```powershell
# Health check
Invoke-RestMethod -Uri "http://localhost:5291/health" -Method GET

# Verificar Swagger
Start-Process "http://localhost:5291/swagger"
```

---

## 3. Construcción del Contenedor

### 3.1 Construir Imagen Individual

```powershell
# Navegar al directorio del proyecto
cd C:\Users\johan\source\repos\HoneyBack

# Construir imagen
docker build -t honeyback-api:latest .

# Verificar imagen creada
docker images | Select-String "honeyback"
```

### 3.2 Verificar Tamaño de Imagen

```powershell
docker images honeyback-api:latest --format "{{.Repository}}:{{.Tag}} - {{.Size}}"
```

**Tamaño esperado:** ~220-280 MB (imagen optimizada con multi-stage build)

---

## 4. Ejecución con Docker Compose

### 4.1 Variables de Entorno (Opcional pero Recomendado)

Crear archivo `.env` en el directorio raíz:

```env
# .env - Variables de entorno para Docker Compose
SA_PASSWORD=Tu-Password-Seguro-2024!
JWT_KEY=Tu-JWT-Key-Segura-De-Al-Menos-32-Caracteres!
ASPNETCORE_ENVIRONMENT=Development
```

> ⚠️ **IMPORTANTE:** Agregar `.env` a `.gitignore` para no subir credenciales.

### 4.2 Iniciar Servicios

```powershell
# Navegar al directorio del proyecto
cd C:\Users\johan\source\repos\HoneyBack

# Iniciar todos los servicios (SQL Server + API)
docker compose up -d

# Ver logs en tiempo real
docker compose logs -f

# Ver solo logs de la API
docker compose logs -f honeyback_api
```

### 4.3 Verificar Estado de Servicios

```powershell
# Ver estado de contenedores
docker compose ps

# Resultado esperado:
# NAME              STATUS                   PORTS
# honeyback_api     Up (healthy)             0.0.0.0:5291->80/tcp
# honeyback_sql     Up (healthy)             0.0.0.0:1434->1433/tcp
```

### 4.4 Detener Servicios

```powershell
# Detener sin eliminar datos
docker compose stop

# Detener y eliminar contenedores (mantiene volúmenes)
docker compose down

# Detener y eliminar TODO (incluyendo volúmenes de BD)
docker compose down -v
```

---

## 5. Pruebas y Verificación

### 5.1 Health Check

```powershell
# Verificar salud de la API
Invoke-RestMethod -Uri "http://localhost:5291/health" -Method GET | ConvertTo-Json

# Respuesta esperada:
# {
#   "status": "Healthy",
#   "checks": [
#     {
#       "name": "sqlserver",
#       "status": "Healthy"
#     }
#   ]
# }
```

### 5.2 Verificar Swagger UI

```powershell
# Abrir Swagger en navegador
Start-Process "http://localhost:5291/swagger"
```

### 5.3 Probar Endpoints

```powershell
# Registro de usuario
$body = @{
    nombreCompleto = "Usuario Test"
    email = "test@example.com"
    password = "Password123!"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5291/api/auth/register" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"

# Login
$loginBody = @{
    email = "test@example.com"
    password = "Password123!"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5291/api/auth/login" `
    -Method POST `
    -Body $loginBody `
    -ContentType "application/json"

# Guardar token
$token = $response.token
Write-Host "Token: $token"

# Usar token para endpoint protegido
$headers = @{
    Authorization = "Bearer $token"
}

Invoke-RestMethod -Uri "http://localhost:5291/api/auth/me" `
    -Method GET `
    -Headers $headers
```

### 5.4 Verificar Conexión a SQL Server

```powershell
# Conectar al contenedor de SQL Server
docker exec -it honeyback_sql /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U sa -P "Johan3208365126*" -C `
    -Q "SELECT name FROM sys.databases"

# Verificar tablas
docker exec -it honeyback_sql /opt/mssql-tools18/bin/sqlcmd `
    -S localhost -U sa -P "Johan3208365126*" -C `
    -Q "USE HoneyBalanceDB; SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME"

# Resultado esperado: 12 tablas incluyendo Usuarios, Transacciones, MetasAhorro, etc.
```

---

## 6. Publicación en Docker Hub

### 6.1 Crear Cuenta en Docker Hub

1. Ir a [https://hub.docker.com](https://hub.docker.com)
2. Crear cuenta gratuita
3. Verificar email

### 6.2 Login en Docker Hub

```powershell
# Login (se abrirá ventana de autenticación o pedir credenciales)
docker login

# O con credenciales directas
docker login -u TU_USUARIO_DOCKERHUB
```

### 6.3 Etiquetar Imagen

```powershell
# Formato: docker tag imagen-local:tag usuario/repositorio:tag

# Ejemplo con tu usuario (cambiar 'johanjhg' por tu usuario)
docker tag honeyback-api:latest johanjhg/honeyback-api:latest
docker tag honeyback-api:latest johanjhg/honeyback-api:v1.0.0

# Verificar tags
docker images | Select-String "honeyback"
```

### 6.4 Push a Docker Hub

```powershell   
# Subir imagen con tag latest
docker push johanjhg/honeyback-api:latest

# Subir versión específica
docker push johanjhg/honeyback-api:v1.0.0
```

### 6.5 Verificar en Docker Hub

1. Ir a [https://hub.docker.com](https://hub.docker.com)
2. Ir a **Repositories**
3. Verificar que aparece `honeyback-api`

---

## 7. Ejecutar desde Docker Hub

### 7.1 En Cualquier Máquina con Docker

```powershell
# Descargar imagen
docker pull johanjhg/honeyback-api:latest

# Ejecutar con SQL Server existente
docker run -d `
    --name honeyback-api `
    -p 5291:80 `
    -e "ASPNETCORE_ENVIRONMENT=Development" `
    -e "ConnectionStrings__conexion=Server=TU_SQL_SERVER;Database=HoneyBalanceDB;User Id=sa;Password=Johan3208365126*;TrustServerCertificate=True;Connection Timeout=60" `
    -e "Jwt__Key=Johan3208365126*-SecureKey-2026-HoneyBalance!" `
    -e "Jwt__Issuer=HoneyBack" `
    -e "Jwt__Audience=HoneyBack" `
    johanjhg/honeyback-api:latest
```

### 7.2 Con Docker Compose en Otra Máquina

Crear `docker-compose.yml`:

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: honeyback_sql
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=Johan3208365126*
      - MSSQL_PID=Express
    ports:
      - "1434:1433"
    volumes:
      - sql_data:/var/opt/mssql
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Johan3208365126*' -Q 'SELECT 1' -C -b -o /dev/null"]
      interval: 10s
      timeout: 5s
      retries: 10
      start_period: 60s

  honeyback_api:
    image: johanjhg/honeyback-api:latest  # Cambiar por tu usuario
    container_name: honeyback_api
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:80
      - ConnectionStrings__conexion=Server=sqlserver;Database=HoneyBalanceDB;User Id=sa;Password=Johan3208365126*;TrustServerCertificate=True;Encrypt=True;MultipleActiveResultSets=true;Connection Timeout=60
      - Jwt__Issuer=HoneyBack
      - Jwt__Audience=HoneyBack
      - Jwt__Key=Johan3208365126*-SecureKey-2024-HoneyBalance!
    ports:
      - "5291:80"
    depends_on:
      sqlserver:
        condition: service_healthy

volumes:
  sql_data:
```

```powershell
# Ejecutar
docker compose up -d
```

---

## 8. Solución de Problemas

### 8.1 Error: "Cannot connect to SQL Server"

```powershell
# Verificar que SQL Server está corriendo
docker compose ps

# Ver logs de SQL Server
docker compose logs sqlserver

# Esperar más tiempo al inicio (SQL Server tarda ~60s)
# Verificar healthcheck
docker inspect honeyback_sql --format='{{.State.Health.Status}}'

# Debe retornar: healthy
```

### 8.2 Error: "Port already in use"

```powershell
# Ver qué usa el puerto 5291
netstat -ano | findstr "5291"

# Cambiar puerto en docker-compose.yml
# ports:
#   - "5292:80"  # Usar otro puerto
```

### 8.3 Error: "EF Migrations failed"

```powershell
# Ver logs detallados
docker compose logs honeyback_api

# Si el problema persiste, reconstruir desde cero
docker compose down -v
docker compose build --no-cache
docker compose up -d
```

### 8.4 Reconstruir Todo desde Cero

```powershell
# Eliminar todo
docker compose down -v
docker system prune -f
docker volume prune -f

# Reconstruir
docker compose build --no-cache
docker compose up -d
```

### 8.5 Ver Logs en Tiempo Real

```powershell
# Todos los servicios
docker compose logs -f

# Solo API
docker compose logs -f honeyback_api

# Solo SQL Server
docker compose logs -f sqlserver
```

---

## 9. Configuración de Producción

### 9.1 Variables de Entorno Seguras

**NUNCA** incluir credenciales en `docker-compose.yml` para producción.

```powershell
# Usar Docker secrets o variables de entorno del sistema
$env:SA_PASSWORD = "Password-Muy-Seguro-2024!"
$env:JWT_KEY = "Clave-JWT-Super-Segura-Produccion!"
```

### 9.2 Archivo .env para Producción

```env
# .env.production
SA_PASSWORD=Password-Produccion-Muy-Seguro!
JWT_KEY=JWT-Key-Produccion-Super-Segura-32-Chars!
ASPNETCORE_ENVIRONMENT=Production
```

### 9.3 HTTPS con Reverse Proxy

Para producción, usar Nginx o Traefik como reverse proxy:

```yaml
# docker-compose.production.yml
services:
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
      - ./certs:/etc/ssl/certs
    depends_on:
      - honeyback_api
```

### 9.4 CORS para Producción

Editar `appsettings.Production.json`:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://tu-dominio.com",
      "https://app.tu-dominio.com"
    ]
  }
}
```

---

## 📊 Resumen de Comandos

| Acción | Comando |
|--------|---------|
| Construir imagen | `docker build -t honeyback-api:latest .` |
| Iniciar servicios | `docker compose up -d` |
| Ver logs | `docker compose logs -f` |
| Detener servicios | `docker compose down` |
| Login Docker Hub | `docker login` |
| Tag imagen | `docker tag honeyback-api:latest usuario/honeyback-api:v1.0.0` |
| Push imagen | `docker push usuario/honeyback-api:v1.0.0` |
| Pull imagen | `docker pull usuario/honeyback-api:latest` |
| Ver estado | `docker compose ps` |
| Reconstruir todo | `docker compose down -v && docker compose build --no-cache && docker compose up -d` |

---

## ✅ Checklist Final

- [ ] Docker Desktop instalado y configurado
- [ ] Imagen construida exitosamente
- [ ] SQL Server inicia sin errores y está "healthy"
- [ ] API responde en `/health` con status "Healthy"
- [ ] Swagger UI accesible en `http://localhost:5291/swagger`
- [ ] Endpoints de Auth funcionan (registro, login)
- [ ] Base de datos tiene 12 tablas creadas
- [ ] Login en Docker Hub (si vas a publicar)
- [ ] Imagen taggeada correctamente
- [ ] Push completado (opcional)
- [ ] Imagen visible en Docker Hub (opcional)

---

## 🔧 Actualizaciones Recientes (2026-02-16)

### ✅ Problemas Resueltos

1. **SQL Server Healthcheck mejorado** - Ahora usa `CMD-SHELL` con sintaxis correcta
2. **Connection Timeout aumentado** - De 30s a 60s para mejor estabilidad
3. **Paquetes NuGet actualizados** - Todos los paquetes a versión 8.0.11
4. **Migraciones regeneradas** - Una migración completa que crea todas las tablas
5. **Sistema de reintentos mejorado** - 15 reintentos con mejor logging

Ver detalles completos en `DOCKER_FIX_REPORT.md`.

---

## 📚 Soporte

- **Documentación .NET:** https://docs.microsoft.com/dotnet
- **Docker Docs:** https://docs.docker.com
- **EF Core:** https://docs.microsoft.com/ef/core
- **SQL Server Docker:** https://hub.docker.com/_/microsoft-mssql-server

---

**Generado:** 2025-01-21  
**Última Actualización:** 2026-02-16  
**Versión:** 1.1.0  
**Estado:** 🟢 LISTO PARA PRODUCCIÓN
