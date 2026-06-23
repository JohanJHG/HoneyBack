# Implementación de Analítica de Datos — HoneyBalance

Referencia técnica para replicar este patrón en otros proyectos.

---

## Arquitectura General

```
┌─────────────────────────────────────────────────────────┐
│                     Cliente (Admin)                      │
└────────────────────────┬────────────────────────────────┘
                         │ JWT (Bearer)
          ┌──────────────┼──────────────────┐
          ▼                                  ▼
┌──────────────────┐             ┌───────────────────────┐
│  HoneyBack       │  escribe    │  HoneyAnalytics        │
│  (.NET 8 API)    │ ──────────► │  (Python FastAPI)      │
│                  │             │                        │
│  AuthController  │             │  /analytics/*          │
│  SesionesService │             │  read-only             │
└────────┬─────────┘             └──────────┬────────────┘
         │                                  │
         └──────────────┬───────────────────┘
                        ▼
              ┌──────────────────┐
              │   NeonDB (PG)    │
              │  misma instancia │
              └──────────────────┘
```

**Principio clave:** un solo servicio escribe datos (el API principal), un microservicio separado los lee exclusivamente para analítica. Sin sincronización, sin ETL, sin base de datos secundaria.

---

## Capa de Datos

### Tablas que alimentan la analítica

#### `RegistrosLogin` — tabla central de eventos
```sql
CREATE TABLE "RegistrosLogin" (
    "RegistroLoginID" BIGSERIAL PRIMARY KEY,
    "UsuarioID"       INT NOT NULL REFERENCES "Usuarios",
    "FechaLogin"      TIMESTAMP NOT NULL DEFAULT NOW(),
    "IP"              VARCHAR(45)
);

CREATE INDEX ON "RegistrosLogin" ("FechaLogin");
CREATE INDEX ON "RegistrosLogin" ("UsuarioID");
CREATE INDEX ON "RegistrosLogin" ("UsuarioID", "FechaLogin");
```
Esta tabla es la fuente de verdad para MAU, DAU, actividad diaria, retención y churn.

#### `Sesiones` — duración de sesión
```sql
-- Columnas relevantes para analítica:
"FechaCreacion"   TIMESTAMP
"FechaExpiracion" TIMESTAMP
"Activa"          BOOLEAN   -- false = sesión completada (consultable)
```
La duración promedio de sesión se calcula como `FechaExpiracion - FechaCreacion` solo sobre filas con `Activa = false`. Las sesiones vivas no se tocan.

#### Otras tablas leídas
| Tabla | Campo analítico |
|---|---|
| `Usuarios` | `FechaRegistro`, `Activo`, `Rol` |
| `MensajesContacto` | `Leido` (KPI de mensajes sin revisar) |
| `EntornosPersonales` | `ModuloClave` (split personal vs. empresarial) |

### Cómo se escriben los eventos

**En cada login exitoso** (`AuthController.cs`), el API principal inserta:

```csharp
_context.RegistrosLogin.Add(new RegistroLogin
{
    UsuarioId = usuario.UsuarioId,
    FechaLogin = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
    Ip = ip
});
// También crea una Sesion con Activa=true
// Y ejecuta LimpiarSesionesExpiradasAsync() inline
```

No hay workers ni cron jobs. El evento se registra sincrónicamente en el mismo request de login.

### Limpieza de sesiones

`SesionesService.LimpiarSesionesExpiradasAsync()` corre en cada login:
- Sesiones expiradas recientes → `Activa = false` (se conservan para analytics)
- Sesiones con más de 90 días → `DELETE` definitivo

Esto acota automáticamente la ventana de datos de sesión a 90 días sin jobs externos.

---

## Microservicio HoneyAnalytics

### Stack
- **Runtime:** Python 3.12
- **Framework:** FastAPI 0.115
- **DB driver:** SQLAlchemy 2.0 async + asyncpg
- **Auth:** PyJWT (HS256)
- **Análisis cohort:** Pandas (solo para retención)

### Conexión a base de datos

```python
# database.py
engine = create_async_engine(
    settings.DATABASE_URL,  # postgresql+asyncpg://...
    pool_size=3,
    max_overflow=2,
    pool_timeout=30,
    pool_pre_ping=True,
    connect_args={"ssl": "require"}
)
```

Mismo host NeonDB, driver distinto al del API .NET (`Npgsql` vs `asyncpg`).

### Autenticación

El microservicio **reutiliza el mismo JWT** que emite el API principal. No tiene base de usuarios propia.

```python
# auth.py
def verify_token(token: str = Depends(oauth2_scheme)):
    payload = jwt.decode(token, settings.JWT_SECRET, algorithms=["HS256"],
                         audience=settings.JWT_AUDIENCE,
                         issuer=settings.JWT_ISSUER)
    return payload

def require_admin_or_superadmin(payload = Depends(verify_token)):
    if payload.get("role") not in ("SuperAdmin", "Administrador"):
        raise HTTPException(403)
```

Variables de entorno necesarias: `JWT_SECRET`, `JWT_ISSUER`, `JWT_AUDIENCE` — deben ser idénticas a las del API principal.

### Endpoints

```
GET /analytics/health                          # sin auth
GET /analytics/overview/kpis
GET /analytics/overview/user-growth?months=6
GET /analytics/overview/daily-activity?days=7
GET /analytics/analiticas/kpis?inactivity_days=30
GET /analytics/analiticas/retention?months=12
GET /analytics/analiticas/usage-split
GET /analytics/analiticas/churn-detail?inactivity_days=30
```

Todos los endpoints (excepto `/health`) requieren rol `SuperAdmin` o `Administrador`.

### Queries principales

**MAU / DAU** (misma query, ventana diferente):
```sql
SELECT COUNT(DISTINCT "UsuarioID")
FROM "RegistrosLogin"
WHERE "FechaLogin" >= NOW() - INTERVAL '30 days'  -- o 1 day para DAU
```

**Churn rate:**
```sql
-- Usuarios activos sin login en los últimos N días
SELECT COUNT(*) FROM "Usuarios" u
LEFT JOIN "RegistrosLogin" r ON r."UsuarioID" = u."UsuarioID"
  AND r."FechaLogin" >= NOW() - INTERVAL ':days days'
WHERE u."Activo" = true
GROUP BY u."UsuarioID"
HAVING MAX(r."FechaLogin") IS NULL
```
`churn_rate = churned_users / total_active_users * 100`

**Duración promedio de sesión:**
```sql
SELECT AVG(EXTRACT(EPOCH FROM ("FechaExpiracion" - "FechaCreacion")) / 60)
FROM "Sesiones"
WHERE "Activa" = false
  AND "FechaCreacion" >= NOW() - INTERVAL '30 days'
```

**Crecimiento de usuarios:**
```sql
SELECT DATE_TRUNC('month', "FechaRegistro") AS mes,
       COUNT(*) AS total
FROM "Usuarios"
WHERE "FechaRegistro" >= NOW() - INTERVAL ':months months'
GROUP BY mes ORDER BY mes
```

**Actividad diaria:**
```sql
SELECT DATE_TRUNC('day', "FechaLogin") AS dia,
       COUNT(*) AS total
FROM "RegistrosLogin"
WHERE "FechaLogin" >= NOW() - INTERVAL ':days days'
GROUP BY dia ORDER BY dia
```

**Retención por cohorte (Python + Pandas):**
```python
# Se fetcha: usuarios con FechaRegistro, logins con FechaLogin
# Se calcula: period_offset = meses_desde_registro
# Se agrupa: por cohorte de registro y offset → % retenidos
df = users.merge(logins, on="UsuarioID")
df["cohort"] = df["FechaRegistro"].dt.to_period("M")
df["login_period"] = df["FechaLogin"].dt.to_period("M")
df["offset"] = (df["login_period"] - df["cohort"]).apply(lambda x: x.n)
```
La retención en SQL puro requiere CTEs complejos; aquí se delegó a Pandas por simplicidad.

**Split personal / empresarial:**
```sql
SELECT
  SUM(CASE WHEN "ModuloClave" NOT LIKE 'empresa%' THEN 1 ELSE 0 END) AS personal,
  SUM(CASE WHEN "ModuloClave" LIKE 'empresa%' THEN 1 ELSE 0 END)     AS empresarial
FROM "EntornosPersonales"
```

---

## Estructura de archivos del microservicio

```
honeyanalytics/
├── Dockerfile
├── requirements.txt
└── app/
    ├── main.py          # FastAPI app, routers, CORS, health
    ├── config.py        # pydantic-settings (DATABASE_URL, JWT_*)
    ├── database.py      # engine async, get_db()
    ├── auth.py          # verify_token, require_admin_or_superadmin
    ├── routers/
    │   ├── overview.py      # /overview/*
    │   └── analiticas.py    # /analiticas/*
    ├── services/
    │   ├── overview_service.py
    │   └── analiticas_service.py
    └── schemas/
        ├── overview_schemas.py
        └── analiticas_schemas.py
```

---

## Despliegue

### Docker Compose

```yaml
honeyanalytics:
  image: ${HONEYANALYTICS_IMAGE}
  environment:
    DATABASE_URL: ${ANALYTICS_DATABASE_URL}   # postgresql+asyncpg://...
    JWT_SECRET:   ${JWT_KEY}                  # misma key que el API .NET
    JWT_ISSUER:   HoneyBack
    JWT_AUDIENCE: HoneyBack
  networks:
    - honeyback-network
  healthcheck:
    test: ["CMD", "curl", "-f", "http://localhost:8000/analytics/health"]
    interval: 30s
    start_period: 40s
```

El microservicio comparte red Docker con el API principal pero no necesita comunicarse con él directamente — solo comparten la DB y el secret JWT.

### CI/CD (GitHub Actions)

Trigger: push a `main` con cambios en `honeyanalytics/**`.

```
build → push ghcr.io → SSH a droplet → docker compose pull + up --no-deps → health check
```

El health check del workflow espera 40 segundos y valida `docker inspect --format='{{.State.Health.Status}}'`.

---

## Datos de prueba (Seed)

Para desarrollar sin datos reales existe `AnalyticsSeedService.cs` (invocado con `dotnet run -- --seed`):

| Tipo de dato | Cantidad |
|---|---|
| Usuarios seed | 120 (70 veterans, 30 medios, 20 recientes, 10 inactivos) |
| Sesiones históricas | 360–1080 (`Activa=false`, duración 5–45 min) |
| Transacciones | 2400–7200 por rango de meses |
| Mensajes de contacto | 40 (40% leídos) |

Guard: si ya existe algún `@honeybalance.test` en la DB, el seed no corre.

---

## Decisiones de diseño relevantes

| Decisión | Alternativa descartada | Motivo |
|---|---|---|
| Microservicio Python separado | Endpoints en el API .NET | Separación de concerns; el API .NET no necesita Pandas ni drivers async |
| Misma DB, read-only | DB de analítica separada / data warehouse | Suficiente para la escala actual; sin ETL |
| Queries on-demand | Jobs de pre-cómputo / materialización | Sin Hangfire, sin cron; aceptable mientras el volumen sea bajo |
| JWT compartido | Auth separada para analytics | Reutiliza el sistema de roles existente sin duplicar lógica |
| Pandas para retención | SQL con CTEs recursivos | Más legible; el dataset de cohortes cabe en memoria |
| `Activa=false` como proxy de sesión terminada | Evento explícito de logout | El token JWT ya tiene expiración; se reutiliza como cierre de sesión |

---

## Checklist para replicar en otro proyecto

- [ ] Crear tabla de eventos de login con índice en `(usuario_id, fecha)`
- [ ] Registrar evento en cada login exitoso (inline, sin jobs)
- [ ] Crear tabla de sesiones con `fecha_inicio`, `fecha_fin`, flag `activa`
- [ ] Implementar limpieza de sesiones con soft-delete primero, hard-delete después de N días
- [ ] Crear microservicio FastAPI separado, solo GET, solo lectura
- [ ] Reutilizar el JWT del API principal (mismo secret, mismo issuer/audience)
- [ ] Conectar a la misma DB con driver async (`asyncpg`)
- [ ] Implementar queries de MAU/DAU, churn, duración y crecimiento
- [ ] Añadir health check HTTP en el contenedor
- [ ] Crear seed con datos históricos realistas para desarrollo
