# Guía de configuración para colaboradores — HoneyBalance

Esta guía está dirigida a cualquier persona que quiera correr el proyecto **HoneyBalance** (backend + frontend) en su máquina local, conectándose a la base de datos compartida en NeonDB.

---

## Requisitos previos

Instala las siguientes herramientas antes de continuar:

| Herramienta | Versión mínima | Descarga |
|---|---|---|
| .NET SDK | 8.0 | https://dotnet.microsoft.com/download/dotnet/8.0 |
| Node.js | 18+ (recomendado 22) | https://nodejs.org |
| Git | Cualquier versión reciente | https://git-scm.com |
| Angular CLI | 17+ | `npm install -g @angular/cli` |

> **Verificar instalaciones:**
> ```bash
> dotnet --version      # debe mostrar 8.x.x
> node --version        # debe mostrar v18+ o v22+
> npm --version
> ng version
> ```

---

## Paso 1 — Clonar el repositorio

```bash
# Backend
git clone <URL-del-repo-backend> HoneyBack

# Frontend (si está en repositorio separado)
git clone <URL-del-repo-frontend> honeybalance
```

> Pídele a Johan la URL exacta de los repositorios.

---

## Paso 2 — Configurar el backend

### 2.1 Restaurar dependencias NuGet

```bash
cd HoneyBack
dotnet restore
```

### 2.2 Configurar los secretos de desarrollo (.NET User Secrets)

El proyecto usa **dotnet user-secrets** para guardar credenciales sensibles sin commitearlas al repositorio. Debes configurar los siguientes valores en tu máquina.

#### ¿Por qué user-secrets y no editar appsettings.json?

`appsettings.json` está en el repositorio y no debe contener credenciales reales. `dotnet user-secrets` guarda los valores en una carpeta local de tu sistema operativo (fuera del repo), únicamente accesibles en modo Development.

#### Ejecutar los siguientes comandos (pídele a Johan los valores reales):

```bash
cd HoneyBack

# Cadena de conexión a NeonDB (pídele esta URL a Johan)
dotnet user-secrets set "ConnectionStrings:conexion" "postgresql://neondb_owner:<PASSWORD>@<HOST>/neondb?sslmode=require&channel_binding=require"

ConnectionStrings:conexion = postgresql://neondb_owner:npg_BHUtGQu2h5Is@ep-still-bonus-anezap2j-pooler.c-6.us-east-1.aws.neon.tech/neondb?sslmode=require&channel_binding=require
# Clave JWT — DEBE ser la misma que usa Johan para que los tokens sean compatibles
dotnet user-secrets set "Jwt:Key" "<CLAVE-JWT-QUE-COMPARTE-JOHAN>"

# Email (opcional — si no lo configuras, los emails simplemente no se envían)
dotnet user-secrets set "Resend:ApiKey" "<API-KEY-RESEND>"
dotnet user-secrets set "Resend:FromEmail" "noreply@softbalance.com.co"
dotnet user-secrets set "Resend:FromName" "HoneyBalance"
```

> **Importante sobre la clave JWT:**
> Todos los desarrolladores del proyecto deben usar **exactamente la misma `Jwt:Key`**.
> Si usan claves diferentes, los tokens generados en una máquina no serán válidos en la otra.

#### Verificar que los secrets quedaron bien:

```bash
dotnet user-secrets list
```

Debes ver algo como:

```
ConnectionStrings:conexion = postgresql://neondb_owner:...
Jwt:Key = ...
Resend:ApiKey = ...
```

#### Dónde se guardan los user-secrets en tu sistema

| Sistema operativo | Ubicación |
|---|---|
| Windows | `%APPDATA%\Microsoft\UserSecrets\79a9a79e-9815-4782-b235-5790d99de80f\secrets.json` |
| macOS / Linux | `~/.microsoft/usersecrets/79a9a79e-9815-4782-b235-5790d99de80f/secrets.json` |

El ID `79a9a79e-9815-4782-b235-5790d99de80f` corresponde al proyecto HoneyBack y está definido en `HoneyBack.csproj`.

---

### 2.3 Alternativa: configurar sin user-secrets (variables de entorno)

Si prefieres no usar user-secrets, puedes definir las mismas claves como **variables de entorno del sistema** usando doble guion bajo (`__`) como separador:

**Windows (PowerShell):**
```powershell
$env:ConnectionStrings__conexion = "postgresql://neondb_owner:..."
$env:Jwt__Key = "<CLAVE-JWT>"
```

**macOS / Linux (bash):**
```bash
export ConnectionStrings__conexion="postgresql://neondb_owner:..."
export Jwt__Key="<CLAVE-JWT>"
```

> Las variables de entorno tienen prioridad sobre `appsettings.json` pero menor prioridad que user-secrets.

---

### 2.4 Verificar la conexión a NeonDB y arrancar el backend

```bash
cd HoneyBack
dotnet run
```

Si todo está bien verás en la consola:

```
✅ Conexión a la base de datos establecida exitosamente.
✅ La base de datos está actualizada. No hay migraciones pendientes.
Now listening on: http://localhost:5291
```

> **Las migraciones ya están aplicadas en NeonDB.** No necesitas ejecutar `dotnet ef database update` — el backend las aplica automáticamente al arrancar si detecta cambios pendientes.

#### Verificar con el health endpoint:

```bash
curl http://localhost:5291/health
```

Respuesta esperada:
```json
{"status":"Healthy","checks":[{"name":"postgresql","status":"Healthy"}]}
```

#### Acceder a Swagger (documentación interactiva de la API):

Una vez arrancado, abre en el navegador:

```
http://localhost:5291/swagger
```

---

## Paso 3 — Configurar el frontend

### 3.1 Instalar dependencias npm

```bash
cd honeybalance/honeybalance-angular
npm install
```

> Esto puede tardar 1-3 minutos la primera vez.

### 3.2 Verificar la URL del backend

El frontend ya está configurado para apuntar al backend en `http://localhost:5291/api`.
No necesitas cambiar nada si corres el backend en el puerto por defecto.

El archivo relevante es `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiBase: 'http://localhost:5291/api',
};
```

### 3.3 Arrancar el frontend

```bash
cd honeybalance/honeybalance-angular
ng serve
```

O con npm:

```bash
npm start
```

Una vez arrancado, abre en el navegador:

```
http://localhost:4200
```

---

## Paso 4 — Flujo completo de verificación

Sigue este flujo para confirmar que todo funciona de extremo a extremo:

1. **Arrancar el backend** → `dotnet run` en `HoneyBack/`
2. **Arrancar el frontend** → `ng serve` en `honeybalance-angular/`
3. **Abrir** `http://localhost:4200`
4. **Registrarse** con un email y contraseña nuevos
5. **Iniciar sesión** con las credenciales recién creadas
6. **Crear una transacción** desde el dashboard
7. **Verificar en NeonDB** que el registro aparece (ver guía `Visualizar_Datos_NeonDB.md`)

---

## Referencia rápida — Todos los secretos necesarios

| Clave | Descripción | Obligatorio |
|---|---|---|
| `ConnectionStrings:conexion` | URL de conexión a NeonDB (PostgreSQL cloud) | ✅ Sí |
| `Jwt:Key` | Clave para firmar/verificar tokens JWT. **Debe ser igual en todos los colaboradores** | ✅ Sí |
| `Resend:ApiKey` | API key del servicio de email Resend | ❌ Opcional |
| `Resend:FromEmail` | Email remitente para envío de correos | ❌ Opcional |
| `Resend:FromName` | Nombre del remitente | ❌ Opcional |

> Si no configuras los valores de Resend, el backend arranca igual — simplemente los emails (recuperación de contraseña) no se enviarán y verás un warning en los logs.

---

## Puertos por defecto

| Servicio | Puerto | URL |
|---|---|---|
| Backend (API) | 5291 | http://localhost:5291 |
| Swagger | 5291 | http://localhost:5291/swagger |
| Health check | 5291 | http://localhost:5291/health |
| Frontend (Angular) | 4200 | http://localhost:4200 |

---

## Paquetes NuGet del backend (referencia)

El `dotnet restore` los instala automáticamente. Solo como referencia:

| Paquete | Versión | Uso |
|---|---|---|
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 8.0.11 | Proveedor de PostgreSQL para EF Core |
| `Microsoft.EntityFrameworkCore` | 8.0.11 | ORM principal |
| `Microsoft.EntityFrameworkCore.Tools` | 8.0.11 | Comandos `dotnet ef` |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.0.11 | Autenticación JWT |
| `BCrypt.Net-Next` | 4.0.3 | Hashing de contraseñas |
| `AspNetCore.HealthChecks.NpgSql` | 8.0.2 | Health check para PostgreSQL |
| `Swashbuckle.AspNetCore` | 6.8.1 | Swagger/OpenAPI |
| `Resend` | 0.2.1 | Servicio de email |

---

## Paquetes npm del frontend (principales)

El `npm install` los instala automáticamente:

| Paquete | Versión | Uso |
|---|---|---|
| `@angular/core` | ^20.3.15 | Framework principal |
| `@angular/material` | ^20.2.12 | Componentes UI |
| `rxjs` | ~7.8.0 | Programación reactiva |
| `ngx-toastr` | ^19.1.0 | Notificaciones toast |
| `chart.js` | ^4.5.1 | Gráficas |
| `jspdf` | ^4.2.0 | Exportar PDF |

---

## Solución de problemas frecuentes

### Error: `address already in use` al arrancar el backend

El puerto 5291 está ocupado por otro proceso. En PowerShell:

```powershell
Stop-Process -Id (Get-NetTCPConnection -LocalPort 5291).OwningProcess -Force
```

En macOS/Linux:
```bash
kill -9 $(lsof -ti:5291)
```

### Error: `Failed to connect to database` al arrancar el backend

Verifica que los user-secrets estén configurados correctamente:

```bash
dotnet user-secrets list
```

Si no aparece `ConnectionStrings:conexion`, repite el Paso 2.2.

### Error `CORS` en el navegador

El backend permite por defecto `http://localhost:4200` y `http://localhost:4201`. Si usas otro puerto, pídele a Johan que lo agregue en `appsettings.json` bajo `Cors:AllowedOrigins`.

### El frontend muestra datos vacíos después de iniciar sesión

La base de datos NeonDB es compartida pero los datos son por usuario. Si es la primera vez que te registras, no habrá datos todavía — crea algunas transacciones o metas desde la app.

### Error `401 Unauthorized` en peticiones al backend

El token JWT expiró (duración: 60 minutos) o la `Jwt:Key` configurada en tu máquina es diferente a la del servidor. Verifica que estás usando la misma clave que Johan.
