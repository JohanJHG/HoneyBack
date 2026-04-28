# Cómo visualizar los datos de tu base de datos en NeonDB

## Opciones disponibles

Tienes tres formas principales de explorar y consultar los datos de tu base de datos NeonDB desde el proyecto HoneyBalance.

---

## 1. Panel web de NeonDB (sin instalar nada)

Es la opción más rápida para explorar tablas y ejecutar SQL directo.

### Pasos

1. Ve a [console.neon.tech](https://console.neon.tech) e inicia sesión.
2. Selecciona tu proyecto en la lista.
3. En el menú lateral izquierdo haz clic en **"Tables"** para ver las tablas con una interfaz visual tipo hoja de cálculo.
4. Para ejecutar SQL libre, haz clic en **"SQL Editor"** en el menú lateral.

### Consultas útiles para HoneyBalance

```sql
-- Ver todos los usuarios registrados
SELECT "UsuarioId", "NombreCompleto", "Email", "FechaRegistro"
FROM "Usuarios"
ORDER BY "FechaRegistro" DESC;

-- Ver transacciones de un usuario específico
SELECT t."TransaccionId", t."Nombre", t."Monto", t."Tipo", t."Fecha", t."Categoria"
FROM "Transacciones" t
WHERE t."UsuarioId" = 1
ORDER BY t."Fecha" DESC;

-- Ver metas de ahorro activas
SELECT "MetaId", "Nombre", "MontoObjetivo", "MontoActual", "FechaObjetivo", "Activa", "Completada"
FROM "MetasAhorros"
WHERE "Activa" = true
ORDER BY "Prioridad" DESC;

-- Ver configuraciones de usuario
SELECT c."ConfiguracionId", u."Email", c."Tema", c."MonedaPreferida", c."Idioma", c."FechaCreacion"
FROM "ConfiguracionesUsuarios" c
JOIN "Usuarios" u ON u."UsuarioId" = c."UsuarioId";

-- Ver sesiones activas
SELECT s."SesionId", u."Email", s."FechaExpiracion"
FROM "Sesiones" s
JOIN "Usuarios" u ON u."UsuarioId" = s."UsuarioId"
ORDER BY s."FechaExpiracion" DESC;

-- Ver entornos personales por módulo
SELECT "EntornoId", "ModuloClave", "DatosJson", "FechaCreacion"
FROM "EntornosPersonales"
WHERE "UsuarioId" = 1
ORDER BY "ModuloClave";

-- Resumen de tablas y cantidad de registros
SELECT 'Usuarios' AS tabla, COUNT(*) AS registros FROM "Usuarios"
UNION ALL SELECT 'Transacciones', COUNT(*) FROM "Transacciones"
UNION ALL SELECT 'MetasAhorros', COUNT(*) FROM "MetasAhorros"
UNION ALL SELECT 'ConfiguracionesUsuarios', COUNT(*) FROM "ConfiguracionesUsuarios"
UNION ALL SELECT 'Sesiones', COUNT(*) FROM "Sesiones"
UNION ALL SELECT 'EntornosPersonales', COUNT(*) FROM "EntornosPersonales"
UNION ALL SELECT 'MensajesContactos', COUNT(*) FROM "MensajesContactos";
```

---

## 2. Desde la terminal con `psql`

Si tienes PostgreSQL instalado localmente, puedes conectarte directamente desde la terminal.

### Cadena de conexión

La cadena de conexión está guardada en tus user-secrets de .NET. Para verla:

```bash
cd c:\Users\johan\source\repos\HoneyBack
dotnet user-secrets list
```

Busca el valor de `ConnectionStrings:conexion`. Tiene el formato:

```
postgresql://neondb_owner:<password>@<host>/neondb?sslmode=require&channel_binding=require
```

### Conectarse con psql

```bash
psql "postgresql://neondb_owner:<password>@<host>/neondb?sslmode=require"
```

### Comandos útiles dentro de psql

```sql
-- Listar todas las tablas
\dt

-- Describir una tabla específica
\d "Usuarios"

-- Salir
\q
```

---

## 3. Desde VS Code con la extensión SQLTools

Esta opción permite explorar las tablas y ejecutar SQL directamente desde el editor.

### Instalación

1. Abre VS Code.
2. Ve a extensiones (Ctrl+Shift+X) y busca **"SQLTools"**.
3. Instala también el driver **"SQLTools PostgreSQL/Cockroach Driver"**.

### Configurar la conexión

1. En el panel de SQLTools (icono de base de datos en la barra lateral), haz clic en **"Add New Connection"**.
2. Selecciona **PostgreSQL**.
3. Completa los campos con los datos de tu NeonDB:

| Campo | Valor |
|---|---|
| **Connection name** | HoneyBalance NeonDB |
| **Server** | `ep-still-bonus-anezap2j-pooler.c-6.us-east-1.aws.neon.tech` |
| **Port** | `5432` |
| **Database** | `neondb` |
| **Username** | `neondb_owner` |
| **Password** | (el que aparece en tus user-secrets) |
| **SSL** | Required |

4. Haz clic en **"Test Connection"** para verificar y luego **"Save"**.

### Uso

- Expande la conexión en el panel para navegar por las tablas.
- Haz clic derecho en una tabla → **"Select TOP 10"** para ver los primeros registros.
- Abre un archivo `.sql` nuevo para ejecutar consultas libres (Ctrl+Enter).

---

## Tablas del proyecto HoneyBalance

| Tabla | Descripción |
|---|---|
| `Usuarios` | Cuentas de usuario registradas |
| `Transacciones` | Ingresos y gastos de cada usuario |
| `MetasAhorros` | Metas de ahorro activas y completadas |
| `ConfiguracionesUsuarios` | Preferencias: tema, idioma, moneda, zona horaria |
| `Sesiones` | Tokens JWT activos (se limpian automáticamente) |
| `EntornosPersonales` | Datos JSON flexibles por módulo (presupuesto, deudas, etc.) |
| `MensajesContactos` | Mensajes enviados desde el formulario de contacto |
| `PasswordResetTokens` | Códigos de recuperación de contraseña (expiran en 15 min) |

---

## Verificar que los datos se están guardando

Después de registrarte o crear una transacción desde la app, puedes confirmar en el SQL Editor de NeonDB con:

```sql
-- ¿Llegó el registro del usuario?
SELECT * FROM "Usuarios" ORDER BY "FechaRegistro" DESC LIMIT 1;

-- ¿Se guardó la transacción?
SELECT * FROM "Transacciones" ORDER BY "FechaCreacion" DESC LIMIT 5;
```

Si ves registros, la persistencia en NeonDB funciona correctamente.
