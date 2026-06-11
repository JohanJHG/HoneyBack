# Cómo probar HoneyAnalytics

HoneyAnalytics es un microservicio FastAPI independiente que expone endpoints de analíticas protegidos por el mismo JWT que emite HoneyBack. Conecta directamente a **NeonDB** (PostgreSQL) mediante la variable `ANALYTICS_DATABASE_URL`.

---

## Variables de entorno requeridas

En `infra/env/.env` (basado en `.env.example`), las variables relevantes para HoneyAnalytics son:

| Variable | Descripción |
|---|---|
| `API_DATABASE_URL` | Cadena de conexión NeonDB para **honeyback_api** (Npgsql). **Debe usar el endpoint pooled.** Formato: `postgresql://usuario:password@pooler.neon.tech/honeybalance` |
| `ANALYTICS_DATABASE_URL` | Cadena de conexión NeonDB para **honeyanalytics** (SQLAlchemy). Mismo host/BD que la anterior pero con prefijo `postgresql+asyncpg://`. |
| `JWT_KEY` | Clave secreta del JWT. Debe ser la misma en ambos servicios (el microservicio la recibe como `JWT_SECRET`). |
| `JWT_ISSUER` | Emisor del JWT, por defecto `HoneyBack`. |
| `JWT_AUDIENCE` | Audiencia del JWT, por defecto `HoneyBack`. |

> **Lo que hay que cambiar para arrancar:** `API_DATABASE_URL` y `ANALYTICS_DATABASE_URL` no tienen valores por defecto — ambas deben apuntar a tu NeonDB real. Usan el mismo host y base de datos pero con formato de driver diferente.

---

## Probar en local

### 1. Configurar el .env

Copia el ejemplo si aún no tienes el archivo:

```powershell
Copy-Item infra\env\.env.example infra\env\.env
```

Edita `infra/env/.env` y rellena `ANALYTICS_DATABASE_URL` con tu cadena NeonDB pooled:

```env
ANALYTICS_DATABASE_URL=postgresql+asyncpg://usuario:password@pooler.neon.tech/honeybalance
```

### 2. Levantar los servicios

Desde la raíz del repositorio:

```powershell
docker compose -f infra/compose/compose.yml -f infra/compose/compose.dev.yml --env-file infra/env/.env up --build
```

El flag `--build` reconstruye la imagen de `honeyanalytics` desde `honeyanalytics/Dockerfile`.

HoneyAnalytics quedará disponible en `http://localhost:8000`.

### 3. Verificar que el servicio está vivo

```powershell
Invoke-RestMethod http://localhost:8000/analytics/health
```

Respuesta esperada: `{"status": "ok"}`

### 4. Obtener un token JWT

Primero necesitas hacer login en HoneyBack (disponible en `http://localhost:5291`) con una cuenta de rol **Administrador** o **SuperAdmin**. El token se obtiene del endpoint de autenticación habitual de la API .NET.

### 5. Llamar a los endpoints de analíticas

Todos los endpoints requieren el header `Authorization: Bearer <token>`.

**Swagger interactivo** (más cómodo para probar):
```
http://localhost:8000/  
```

**Endpoints disponibles:**

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/analytics/health` | Health check (no requiere token) |
| GET | `/analytics/overview/kpis` | KPIs generales de la plataforma |
| GET | `/analytics/overview/user-growth?months=6` | Crecimiento de usuarios por mes |
| GET | `/analytics/overview/daily-activity?days=7` | Actividad diaria |
| GET | `/analytics/analiticas/kpis` | KPIs de analíticas detalladas |
| GET | `/analytics/analiticas/retention?months=12` | Retención de usuarios por mes |
| GET | `/analytics/analiticas/usage-split` | Distribución de uso |

**Ejemplo con curl:**

```powershell
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyMCIsImVtYWlsIjoiaG9uZXliYWxhbmNlLnNvcG9ydGVAZ21haWwuY29tIiwibmFtZSI6IkhvbmV5QmFsYW5jZSIsInJvbGUiOiJTdXBlckFkbWluIiwibmJmIjoxNzgwNjYzNzY0LCJleHAiOjE3ODA2NjczNjQsImlhdCI6MTc4MDY2Mzc2NCwiaXNzIjoiSG9uZXlCYWNrIiwiYXVkIjoiSG9uZXlCYWNrIn0.eYCYkoMQrM0kX2qMlqE80Z0MO_pbWud-E7Wi2W_I8Aw"

Invoke-RestMethod `
  -Uri "http://localhost:8000/analytics/overview/kpis" `
  -Headers @{ Authorization = "Bearer $token" }
```

### 6. Qué verificar

- El health check devuelve `{"status": "ok"}`.
- Un token de usuario sin rol admin devuelve `403 Forbidden`.
- Un token expirado o con clave incorrecta devuelve `401 Unauthorized`.
- Los endpoints de datos devuelven JSON con la estructura esperada (no lanza 500).

---

## Probar en producción

### 1. Configurar la imagen en el .env

En el servidor, el `.env` de producción debe tener la imagen publicada:

```env
HONEYANALYTICS_IMAGE=ghcr.io/tu-org/honeybalance-analytics:1.0.0
ANALYTICS_DATABASE_URL=postgresql+asyncpg://usuario:password@pooler.neon.tech/honeybalance
```

> El endpoint pooled de NeonDB es obligatorio en producción para evitar cold-starts en conexiones directas.

### 2. Levantar con el compose de producción

```bash
docker compose -f infra/compose/compose.yml -f infra/compose/compose.prod.yml --env-file infra/env/.env up -d
```

### 3. Verificar el health check en producción

Ajusta el dominio/IP de tu servidor:

```bash
curl https://tu-dominio.com/analytics/health
```

Si hay un reverse proxy (Nginx, Traefik, etc.) asegúrate de que las rutas `/analytics/*` se forwarden al puerto `8000` del contenedor `honeyanalytics`.

### 4. Smoke test con token real

Inicia sesión en la app de producción con un usuario Administrador, copia el token del almacenamiento del navegador (localStorage o cookie), y llama:

```bash
curl -H "Authorization: Bearer <token>" https://tu-dominio.com/analytics/overview/kpis
```

---

## Errores comunes

| Error | Causa probable | Solución |
|---|---|---|
| `500` al arrancar | `ANALYTICS_DATABASE_URL` vacío o incorrecto | Verificar la variable en `.env` y que NeonDB esté accesible |
| `401 Unauthorized` | Token inválido o `JWT_KEY` no coincide con HoneyBack | Confirmar que `JWT_KEY` es idéntico en ambos servicios |
| `403 Forbidden` | Usuario sin rol `Administrador` o `SuperAdmin` | Usar una cuenta con el rol correcto |
| Contenedor no arranca | Imagen no construida o no disponible en el registry | Correr `--build` en dev o verificar la imagen en prod |
| Timeout en queries | Usando endpoint directo de NeonDB en vez del pooled | Cambiar `ANALYTICS_DATABASE_URL` al endpoint `pooler.neon.tech` |
