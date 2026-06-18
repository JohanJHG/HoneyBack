# Migración NeonDB → DigitalOcean Managed PostgreSQL

> Estado actual: tu DB local (`honeybalance`) ya tiene el backup de NeonDB.  
> Meta: subir esos datos a DO y actualizar el proyecto.

---

## 1. Subir datos locales a DigitalOcean

### Paso 1 — Obtén tu connection string de DigitalOcean

En el panel de DO ve a **Databases → tu cluster → Connection Details**.  
Selecciona **Connection string** y copia el valor. Se verá así:

```
postgresql://doadmin:AVNS_xxxxx@db-postgresql-nyc3-12345-do-user-123456-0.b.db.ondigitalocean.com:25060/defaultdb?sslmode=require
```

Necesitas estos datos sueltos también:
- **Host**: `db-postgresql-nyc3-xxxxx.b.db.ondigitalocean.com`
- **Port**: `25060`
- **User**: `doadmin`
- **Password**: la que aparece en el panel
- **Database**: `defaultdb` (o crea una nueva llamada `honeybalance`)

### Paso 2 — (Opcional) Crear base de datos `honeybalance` en DO

Si quieres usar el mismo nombre que local, conéctate primero y créala:

```bash
psql "postgresql://doadmin:TU_PASSWORD@HOST:25060/defaultdb?sslmode=require" \
  -c "CREATE DATABASE honeybalance;"
```

Si no, puedes usar `defaultdb` directamente (cambia el nombre en los comandos siguientes).

### Paso 3 — Exportar tu DB local

```bash
pg_dump -U postgres -d honeybalance \
  --no-owner --no-acl \
  -F p -f honeybalance_backup.sql
```

> `--no-owner --no-acl`: evita que intente asignar permisos del usuario local que no existen en DO.

### Paso 4 — Importar a DigitalOcean

```bash
psql "postgresql://doadmin:TU_PASSWORD@HOST:25060/honeybalance?sslmode=require" \
  -f honeybalance_backup.sql
```

Verás mensajes de tipo `CREATE TABLE`, `INSERT`, etc. Si hay errores de permisos menores (como `COMMENT ON EXTENSION`), son inofensivos.

### Paso 5 — Verificar que llegaron los datos

```bash
psql "postgresql://doadmin:TU_PASSWORD@HOST:25060/honeybalance?sslmode=require" \
  -c "\dt"
```

Debes ver tus tablas: `Usuarios`, `Transacciones`, `RegistrosLogin`, etc.

---

## 2. Cambios en el proyecto

### Variables de entorno — formato de connection string

DigitalOcean usa SSL obligatorio. Las dos variables que debes cambiar son:

**API .NET** — variable `API_DATABASE_URL` (o `ConnectionStrings__conexion`):
```
# NeonDB (antes)
postgresql://usuario:pass@pooler.neon.tech/honeybalance

# DigitalOcean (nuevo)
postgresql://doadmin:TU_PASSWORD@HOST:25060/honeybalance?sslmode=require
```

**Analytics Python** — variable `ANALYTICS_DATABASE_URL`:
```
# NeonDB (antes)
postgresql+asyncpg://usuario:pass@pooler.neon.tech/honeybalance

# DigitalOcean (nuevo)
postgresql+asyncpg://doadmin:TU_PASSWORD@HOST:25060/honeybalance?ssl=require
```

> Nota: asyncpg usa `ssl=require`, Npgsql usa `sslmode=require`. Son distintos.

### ¿Hay cambios de código?

**No.** El proyecto ya está bien estructurado:

- La API .NET lee `ConnectionStrings:conexion` desde variable de entorno → `Program.cs:24`
- Analytics lee `ANALYTICS_DATABASE_URL` (o `database_url`) desde `.env` → `config.py:5`
- `database.py` ya tiene `connect_args={"ssl": "require"}` → sigue funcionando

El único cambio es en las **variables de entorno**, no en el código.

### Un detalle: `ssl` en database.py de Analytics

Tu `database.py` tiene `connect_args={"ssl": "require"}` hardcodeado. Eso es correcto y funciona con DigitalOcean. No necesitas tocarlo.

---

## 3. Cambios en el VPS (producción)

Sí, debes actualizar las variables de entorno en el VPS. Dependiendo de cómo estén configuradas:

### Si usas un archivo `.env` en el VPS

```bash
# Conéctate al VPS
ssh usuario@tu-vps-ip

# Edita el archivo .env
nano /ruta/a/tu/proyecto/.env
```

Cambia:
```env
API_DATABASE_URL=postgresql://doadmin:TU_PASSWORD@HOST:25060/honeybalance?sslmode=require
ANALYTICS_DATABASE_URL=postgresql+asyncpg://doadmin:TU_PASSWORD@HOST:25060/honeybalance?ssl=require
```

Luego reinicia los contenedores:
```bash
docker compose -f compose.prod.yml down
docker compose -f compose.prod.yml up -d
```

### Si usas variables de entorno del sistema (sin .env)

```bash
export API_DATABASE_URL="postgresql://doadmin:TU_PASSWORD@HOST:25060/honeybalance?sslmode=require"
export ANALYTICS_DATABASE_URL="postgresql+asyncpg://doadmin:TU_PASSWORD@HOST:25060/honeybalance?ssl=require"
```

Para que persistan entre reinicios, agrégalas a `/etc/environment` o al archivo de servicio systemd.

---

## 4. Checklist final

- [ ] Backup exportado con `pg_dump`
- [ ] Datos importados a DO con `psql`
- [ ] Tablas verificadas con `\dt`
- [ ] `.env` actualizado en el VPS (ambas variables)
- [ ] Contenedores reiniciados en el VPS
- [ ] Probar login en producción para confirmar que funciona

---

## 5. ¿Preguntas frecuentes?

**¿Necesito correr las migraciones de EF Core después?**  
No, si el dump incluye la estructura completa. Si `APPLY_MIGRATIONS_AT_STARTUP=true`, EF Core detectará que las tablas ya existen y no hará nada destructivo.

**¿DigitalOcean necesita SSL obligatorio?**  
Sí. Por eso el `?sslmode=require` en la connection string es obligatorio, de lo contrario la conexión es rechazada.

**¿Puedo usar el mismo nombre de DB `honeybalance` en DO?**  
Sí, solo créala primero (Paso 2). DO crea por defecto `defaultdb`, pero puedes crear las que quieras.
