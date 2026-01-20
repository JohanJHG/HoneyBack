# RECUPERACION DE CONTRASENA - API Backend

Documentacion tecnica para integracion con frontend Angular.

---

## Endpoints

### POST /api/auth/forgot-password

Solicita un codigo de recuperacion de contrasena. El codigo se envia al email registrado.

**URL:** `http://localhost:5291/api/auth/forgot-password`

**Autenticacion:** No requerida (publico)

**Request Body:**
```json
{
  "email": "usuario@ejemplo.com"
}
```

**Validaciones:**
- `email`: Requerido, formato email valido, maximo 255 caracteres

**Response 200 OK:**
```json
{
  "mensaje": "Si el email esta registrado, recibiras un codigo de recuperacion en tu correo."
}
```

**Notas de seguridad:**
- La respuesta es SIEMPRE la misma, independientemente de si el email existe o no
- Esto previene ataques de enumeracion de emails
- El frontend debe mostrar el mensaje generico sin modificaciones

**Comportamiento interno:**
- Si el email existe: genera codigo de 6 digitos, invalida tokens anteriores, envia email
- Si el email no existe: retorna el mismo mensaje sin accion adicional
- El codigo expira en 15 minutos
- Solo puede haber un codigo activo por usuario

---

### POST /api/auth/reset-password

Restablece la contrasena usando el codigo de verificacion recibido por email.

**URL:** `http://localhost:5291/api/auth/reset-password`

**Autenticacion:** No requerida (publico)

**Request Body:**
```json
{
  "token": "123456",
  "newPassword": "NuevaContrasena123"
}
```

**Validaciones:**
- `token`: Requerido, exactamente 6 digitos numericos
- `newPassword`: Requerido, minimo 6 caracteres, maximo 100 caracteres

**Response 200 OK:**
```json
{
  "mensaje": "Contrasena restablecida exitosamente. Ya puedes iniciar sesion."
}
```

**Response 400 Bad Request (codigo invalido o expirado):**
```json
{
  "mensaje": "El codigo es invalido o ha expirado. Solicita uno nuevo."
}
```

**Response 400 Bad Request (validacion fallida):**
```json
{
  "errors": {
    "Token": ["El codigo debe tener exactamente 6 digitos"],
    "NewPassword": ["La contrasena debe tener al menos 6 caracteres"]
  }
}
```

**Response 500 Internal Server Error:**
```json
{
  "mensaje": "Error interno del servidor"
}
```

---

## Flujo de Integracion

### Paso 1: Usuario solicita recuperacion
1. Usuario ingresa su email en el formulario
2. Frontend llama a `POST /api/auth/forgot-password`
3. Mostrar mensaje: "Si el email esta registrado, recibiras un codigo de recuperacion en tu correo."
4. Mostrar campo para ingresar codigo de 6 digitos

### Paso 2: Usuario ingresa codigo y nueva contrasena
1. Usuario ingresa el codigo recibido por email
2. Usuario ingresa nueva contrasena (minimo 6 caracteres)
3. Frontend llama a `POST /api/auth/reset-password`
4. Si es exitoso: mostrar mensaje de exito y redirigir a login
5. Si falla: mostrar mensaje de error apropiado

---

## Ejemplos con curl

### Solicitar codigo de recuperacion
```bash
curl -X POST http://localhost:5291/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email": "usuario@ejemplo.com"}'
```

### Restablecer contrasena
```bash
curl -X POST http://localhost:5291/api/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{"token": "123456", "newPassword": "NuevaContrasena123"}'
```

---

## Ejemplos con fetch (JavaScript/TypeScript)

### Solicitar codigo de recuperacion
```typescript
async function forgotPassword(email: string): Promise<{ mensaje: string }> {
  const response = await fetch('http://localhost:5291/api/auth/forgot-password', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ email })
  });
  return response.json();
}
```

### Restablecer contrasena
```typescript
async function resetPassword(token: string, newPassword: string): Promise<{ mensaje: string }> {
  const response = await fetch('http://localhost:5291/api/auth/reset-password', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ token, newPassword })
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.mensaje || 'Error al restablecer contrasena');
  }
  
  return response.json();
}
```

---

## Modelos TypeScript para Angular

```typescript
// DTOs para recuperacion de contrasena
export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

export interface ApiResponse {
  mensaje: string;
}

export interface ValidationErrorResponse {
  errors: {
    [key: string]: string[];
  };
}
```

---

## Servicio Angular (ejemplo)

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private baseUrl = 'http://localhost:5291/api/auth';

  constructor(private http: HttpClient) {}

  forgotPassword(email: string): Observable<{ mensaje: string }> {
    return this.http.post<{ mensaje: string }>(
      `${this.baseUrl}/forgot-password`,
      { email }
    );
  }

  resetPassword(token: string, newPassword: string): Observable<{ mensaje: string }> {
    return this.http.post<{ mensaje: string }>(
      `${this.baseUrl}/reset-password`,
      { token, newPassword }
    );
  }
}
```

---

## Reglas de Negocio

| Aspecto | Valor |
|---------|-------|
| Longitud del codigo | 6 digitos numericos |
| Expiracion del codigo | 15 minutos |
| Codigos activos por usuario | 1 (anteriores se invalidan) |
| Contrasena minima | 6 caracteres |
| Contrasena maxima | 100 caracteres |
| Algoritmo de hash | BCrypt (work factor 12) |

---

## Consideraciones de Seguridad

1. **No revelar existencia de email**: El endpoint forgot-password siempre retorna el mismo mensaje
2. **Codigo de un solo uso**: Una vez usado, el codigo se invalida
3. **Expiracion automatica**: Codigos expiran a los 15 minutos
4. **Invalidacion de codigos previos**: Al solicitar nuevo codigo, los anteriores se marcan como usados
5. **Generacion criptografica**: El codigo se genera con RandomNumberGenerator (CSPRNG)
6. **HTTPS obligatorio en produccion**: Nunca enviar credenciales sin cifrado

---

## Codigos de Respuesta HTTP

| Codigo | Descripcion | Cuando ocurre |
|--------|-------------|---------------|
| 200 | OK | Operacion exitosa |
| 400 | Bad Request | Validacion fallida, codigo invalido/expirado |
| 500 | Internal Server Error | Error no controlado en el servidor |

---

## Configuracion del Email (para equipo backend/devops)

El servicio de email usa Resend. Para desarrollo local:

```bash
# Configurar API key en user secrets
dotnet user-secrets set "Resend:ApiKey" "<tu-api-key>"

# Opcional: configurar remitente (por defecto usa onboarding@resend.dev)
dotnet user-secrets set "Resend:FromEmail" "noreply@tudominio.com"
dotnet user-secrets set "Resend:FromName" "HoneyBalance"
```

Para produccion, usar variables de entorno:
- `Resend__ApiKey`
- `Resend__FromEmail`
- `Resend__FromName`

---

Ultima actualizacion: 2026-01-09
