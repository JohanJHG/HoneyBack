
## ?? Estructura del Proyecto

```
HoneyBack/
??? Controllers/
?   ??? UsuariosController.cs
?   ??? ReportesController.cs
?   ??? SesionesController.cs
?   ??? MensajesContactoController.cs
?   ??? PruebasController.cs
??? Models/
?   ??? Usuario.cs
?   ??? Reporte.cs
?   ??? Sesione.cs
?   ??? MensajesContacto.cs
?   ??? HoneyBalanceDbContext.cs
??? Servicios/
?   ??? IUsuariosService.cs + UsuariosService.cs
?   ??? IReportesService.cs + ReportesService.cs
?   ??? ISesionesService.cs + SesionesService.cs
?   ??? IMensajesContactoService.cs + MensajesContactoService.cs
??? Program.cs
??? appsettings.json
```

---

## ?? Configuración

### 1. Cadena de Conexión

La cadena de conexión está en `appsettings.json`:

```json
"ConnectionStrings": {
  "conexion": "Server=JOHAN\\SQLEXPRESS;Database=HoneyBalanceDB;Integrated Security=true;TrustServerCertificate=True"
}
```

### 2. CORS

Configurado para aceptar peticiones desde Angular:
- `http://localhost:4200` (puerto por defecto de Angular)
- `http://localhost:4201` (puerto alternativo)

Para agregar más orígenes, edita `Program.cs`:

```csharp
policy.WithOrigins("http://localhost:4200", "http://localhost:4201", "TU_URL_AQUI")
```

---

## ?? Cómo Probar la API


Usa el archivo `HoneyBack_API_Tests.http`:

1. Abre el archivo
2. Actualiza `@baseUrl` con tu puerto
3. Haz clic en "Send Request" sobre cada endpoint

## ?? Endpoints Principales

### Usuarios
```
GET    /api/usuarios              - Obtener todos
GET    /api/usuarios/{id}         - Obtener por ID
GET    /api/usuarios/email/{email} - Obtener por Email
POST   /api/usuarios              - Crear nuevo
PUT    /api/usuarios/{id}         - Actualizar
DELETE /api/usuarios/{id}         - Eliminar
```

### Reportes
```
GET    /api/reportes                  - Obtener todos
GET    /api/reportes/{id}             - Obtener por ID
GET    /api/reportes/usuario/{id}     - Obtener por usuario
GET    /api/reportes/estado/{estado}  - Obtener por estado
POST   /api/reportes                  - Crear nuevo
PUT    /api/reportes/{id}             - Actualizar
DELETE /api/reportes/{id}             - Eliminar
```

### Sesiones
```
GET    /api/sesiones                     - Obtener todas
GET    /api/sesiones/{id}                - Obtener por ID
GET    /api/sesiones/usuario/{id}        - Obtener por usuario
POST   /api/sesiones                     - Crear nueva
POST   /api/sesiones/validar             - Validar token
POST   /api/sesiones/limpiar-expiradas   - Limpiar expiradas
DELETE /api/sesiones/{id}                - Eliminar
```

### Mensajes de Contacto
```
GET    /api/mensajescontacto     - Obtener todos
GET    /api/mensajescontacto/{id} - Obtener por ID
POST   /api/mensajescontacto     - Crear nuevo
DELETE /api/mensajescontacto/{id} - Eliminar
```

Controlador de Pruebas

Endpoints especiales para testing:

```http
# Verificar conexión a BD
GET /api/pruebas/verificar-conexion
    
# Crear datos de prueba
POST /api/pruebas/datos-prueba

# Limpiar datos de prueba
DELETE /api/pruebas/limpiar-datos-prueba
```

---

## ?? Ejemplo de Uso desde Angular

```typescript
// usuario.service.ts
import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UsuarioService {
  private apiUrl = 'https://localhost:7000/api/usuarios';

  constructor(private http: HttpClient) { }

  obtenerTodos(): Observable<any[]> {
    return this.http.get<any[]>(this.apiUrl);
  }

  obtenerPorId(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  crear(usuario: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, usuario);
  }

  actualizar(id: number, usuario: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, usuario);
  }

  eliminar(id: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }
}
```

---

## ?? Validaciones Implementadas

- ? Validación de email duplicado al crear usuarios
- ? Validación de modelo con `ModelState`
- ? Validación de existencia antes de actualizar/eliminar
- ? Validación de tokens de sesión con fecha de expiración
- ? Manejo de referencias circulares en JSON

---


## ?? Soporte

Para más información sobre Entity Framework Core:
- [Documentación oficial de EF Core](https://docs.microsoft.com/ef/core/)
- [ASP.NET Core Web API](https://docs.microsoft.com/aspnet/core/web-api/)

---

**¡Tu backend está completamente funcional y listo para ser consumido desde Angular! ??**

## Autenticación JWT

- Configuración en `appsettings.json` / `appsettings.Development.json`:
```json
"Jwt": {
  "Issuer": "HoneyBack",
  "Audience": "HoneyBack",
  "ExpiresInMinutes": 60
}
```
- Configurar Key en desarrollo (no en el repo):
```
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<clave-256-bits>"
```
- Endpoints:
  - POST `/api/auth/login` Body: `{ "email": "...", "password": "..." }`
    - 200: `{ token, expiresAt, user }`
    - 404: usuario no encontrado
    - 401: contraseña incorrecta
  - GET `/api/auth/me` [Authorize] con header `Authorization: Bearer <token>`
- Swagger: botón Authorize disponible (esquema Bearer).
- Expiración: 1 hora. `ClockSkew = 0`.
- Nota migración: `api/sesiones` sigue disponible pero es deprecable; usar JWT.
