# Verificación: por qué no está funcionando la conexión a NeonDB

## Resultado de la verificación

Tu error actual **no es de red**, es de **formato de connection string** al construir `NpgsqlConnectionStringBuilder`.

Evidencia del stack trace:

- `Couldn't set <postgresql://... ?sslmode (Parameter '<postgresql://... ?sslmode')`
- `KeyNotFoundException` dentro de `NpgsqlConnectionStringBuilder`

Esto indica que Npgsql está intentando interpretar la cadena como pares `clave=valor`, pero recibe algo con formato inválido para ese parser (además se ve prefijo con `<...>` en el valor).

---

## Causa raíz (en tu caso)

1. La cadena que llega a la app parece venir como `"<postgresql://...>"` (con `<`/`>`), o no normalizada para Npgsql.
2. Npgsql en esta ruta de EF Core está esperando connection string de estilo `Host=...;Port=...;...`.
3. Por eso falla antes de conectarse realmente a Neon.

---

## Cómo debe verse la conexión para esta app (.NET + EF Core + Npgsql)

Usa formato **Npgsql** (recomendado):

```text
Host=ep-still-bonus-anezap2j-pooler.c-6.us-east-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=TU_PASSWORD;SSL Mode=Require;Channel Binding=Require
```

> No uses comillas ni `< >` en el valor real.

Si el password contiene caracteres especiales, mantenlo como texto literal en secrets/env var (sin envolver en `< >`).

---

## Cómo funciona realmente esta conexión en la nube (Neon)

Flujo real:

1. Tu API resuelve el host `ep-...neon.tech`.
2. Tu app abre una conexión TLS a PostgreSQL (`SSL Mode=Require`).
3. En endpoint `-pooler`, la sesión entra al **pooler** de Neon.
4. El pooler enruta al compute de tu proyecto Neon.
5. PostgreSQL ejecuta queries; EF Core materializa resultados en tus entidades.

En resumen: tu app no “sube” datos a Neon por API HTTP; se conecta por protocolo PostgreSQL seguro (TCP+TLS) al endpoint gestionado por Neon.

---

## Migraciones: SQL Server vs PostgreSQL (lo más importante)

Tus migraciones actuales (`Migrations/`) son de SQL Server, y además tu modelo tiene mapeos SQL Server (`getdate()`, `datetime`, `nvarchar(max)`, etc.).

Para Neon/PostgreSQL debes:

1. Mantener provider Npgsql (ya hecho).
2. Adaptar el modelo a tipos/defaults compatibles con PostgreSQL.
   - `getdate()` -> `now()`
   - `datetime` -> `timestamp` (o quitar tipo explícito y dejar que EF lo infiera)
   - `nvarchar(max)` -> `text`
   - `CONVERT([date],getdate())` -> `CURRENT_DATE`
3. Crear una **nueva línea de migraciones PostgreSQL** (no reutilizar la de SQL Server).

---

## Procedimiento recomendado (estándar profesional)

### 1) Configurar secreto correctamente (sin hardcodear)

```powershell
dotnet user-secrets set "ConnectionStrings:conexion" "postgresql://<usuario>:<password>@<host>/<database>?sslmode=require&channel_binding=require"
```

Para producción, usar variable de entorno o Secret Manager del cloud (no `appsettings.json` con secretos).

### 2) Preparar migraciones PostgreSQL

Como vas a migrar de motor, lo correcto es separar/cambiar la línea de migración:

- Respaldar carpeta `Migrations/`
- Crear migración inicial para PostgreSQL después de ajustar el modelo
- Asegurar que el respaldo **no compile** (guardar fuera del proyecto o usar extensión `.bak`)

Ejemplo:

```powershell
dotnet ef migrations add InitialPostgres
```

### 3) Aplicar migración a Neon

```powershell
dotnet ef database update
```

### 4) Endurecer operación (best practices)

1. No ejecutar migraciones automáticas al arranque en producción multi-instancia.
2. Ejecutar migraciones en pipeline/release step controlado.
3. Rotar credenciales y usar usuario dedicado por ambiente.
4. Mantener TLS obligatorio.
5. Monitorear latencia/errores de conexión y timeouts.

---

## Checklist de resolución de tu error actual

- [x] Quitar `<` y `>` del valor real de `ConnectionStrings:conexion`
- [x] Asegurar formato compatible para Npgsql (se soporta URI `postgresql://...` y formato `Host=...;Port=...;...`)
- [x] Reiniciar la app para tomar nueva configuración
- [x] Validar que conecta (sin excepción de parser)
- [x] Rehacer migraciones para PostgreSQL antes de `database update`

### Ejecución real del checklist en esta sesión

1. Se configuró `ConnectionStrings:conexion` en user-secrets (sin hardcodear en `appsettings`).
2. Se respaldaron migraciones SQL Server en `backups/Migrations_sqlserver_20260424100815`.
3. Se ajustó el modelo EF a PostgreSQL (`now()`, `CURRENT_DATE`, `timestamp without time zone`, `text`).
4. Se regeneró la línea de migración con `InitialPostgres`.
5. Se aplicó `dotnet ef database update` sobre Neon exitosamente.

---

## Nota final

Aunque corrijas el connection string, si aplicas migraciones SQL Server sobre Neon/PostgreSQL vas a fallar después por incompatibilidad SQL.  
Primero corrige cadena de conexión, luego corrige modelo/migraciones para PostgreSQL.
