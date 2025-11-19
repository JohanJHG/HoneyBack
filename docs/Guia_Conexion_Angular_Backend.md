# Guía: Conectar Angular (honeybalance-angular) con Backend .NET 8 + EF Core

Esta guía explica cómo consumir tu API desde el frontend Angular usando los datos reales del proyecto.

- URL base de la API: `http://localhost:5291/api`
- Puerto del frontend Angular (dev): `http://localhost:4200/`
- Proyecto Angular: `honeybalance-angular`
- Carpeta Angular: `C:\Users\johan\Desktop\HONEY\honeybalance-angular`
- CORS del backend ya permite: `http://localhost:4200` y `http://localhost:4201`

---

## 1) Preparación en Angular

1. Instala dependencias (si no lo hiciste):
   - `@angular/common/http` (ya viene con Angular)
2. Arranca el frontend:
   - En la carpeta `C:\Users\johan\Desktop\HONEY\honeybalance-angular` ejecuta: `npm start` o `ng serve`
3. Verifica que el backend esté corriendo en `http://localhost:5291/api`

---

## 2) Configurar la URL de la API en Angular

Crea/actualiza `src/environments/environment.ts`:

```ts
export const environment = {
  production: false,
  apiBase: 'http://localhost:5291/api'
};
```

(En producción usa `environment.prod.ts` con tu URL real.)

En `src/app/app.module.ts` importa `HttpClientModule` una sola vez:

```ts
import { HttpClientModule } from '@angular/common/http';

@NgModule({
  imports: [
    // ...
    HttpClientModule
  ]
})
export class AppModule {}
```

---

## 3) Interfaces TypeScript (modelos)

Crea `src/app/core/models/api-models.ts`:

```ts
export interface Usuario {
  usuarioId?: number;
  nombreCompleto: string;
  email: string;
  passwordHash?: string;
  fechaRegistro?: string; // ISO string
}

export interface Reporte {
  reporteId?: number;
  nombre: string;
  tipoReporte: string;
  descripcion?: string;
  fechaReporte?: string;
  estado?: string;
  usuarioId?: number;
}

export interface Sesion {
  sesionId?: number;
  usuarioId: number;
  tokenSesion: string;
  fechaExpiracion: string; // ISO string
}

export interface MensajeContacto {
  contactoId?: number;
  nombre: string;
  email: string;
  mensaje: string;
  fechaEnvio?: string;
}
```

---

## 4) Servicios Angular para consumir la API

Crea la carpeta `src/app/core/services/` y añade los siguientes servicios.

### 4.1 `usuarios.service.ts`

```ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Usuario } from '../models/api-models';

@Injectable({ providedIn: 'root' })
export class UsuariosService {
  private baseUrl = `${environment.apiBase}/usuarios`;

  constructor(private http: HttpClient) {}

  obtenerTodos(): Observable<Usuario[]> {
    return this.http.get<Usuario[]>(this.baseUrl);
  }

  obtenerPorId(id: number): Observable<Usuario> {
    return this.http.get<Usuario>(`${this.baseUrl}/${id}`);
  }

  obtenerPorEmail(email: string): Observable<Usuario> {
    return this.http.get<Usuario>(`${this.baseUrl}/email/${encodeURIComponent(email)}`);
  }

  crear(usuario: Usuario): Observable<Usuario> {
    return this.http.post<Usuario>(this.baseUrl, usuario);
  }

  actualizar(id: number, usuario: Usuario): Observable<Usuario> {
    return this.http.put<Usuario>(`${this.baseUrl}/${id}`, usuario);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
```

### 4.2 `reportes.service.ts`

```ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Reporte } from '../models/api-models';

@Injectable({ providedIn: 'root' })
export class ReportesService {
  private baseUrl = `${environment.apiBase}/reportes`;

  constructor(private http: HttpClient) {}

  obtenerTodos(): Observable<Reporte[]> {
    return this.http.get<Reporte[]>(this.baseUrl);
  }

  obtenerPorId(id: number): Observable<Reporte> {
    return this.http.get<Reporte>(`${this.baseUrl}/${id}`);
  }

  obtenerPorUsuario(usuarioId: number): Observable<Reporte[]> {
    return this.http.get<Reporte[]>(`${this.baseUrl}/usuario/${usuarioId}`);
  }

  obtenerPorEstado(estado: string): Observable<Reporte[]> {
    return this.http.get<Reporte[]>(`${this.baseUrl}/estado/${estado}`);
  }

  crear(reporte: Reporte): Observable<Reporte> {
    return this.http.post<Reporte>(this.baseUrl, reporte);
  }

  actualizar(id: number, reporte: Reporte): Observable<Reporte> {
    return this.http.put<Reporte>(`${this.baseUrl}/${id}`, reporte);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
```

### 4.3 `sesiones.service.ts`

```ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Sesion } from '../models/api-models';

@Injectable({ providedIn: 'root' })
export class SesionesService {
  private baseUrl = `${environment.apiBase}/sesiones`;

  constructor(private http: HttpClient) {}

  obtenerTodos(): Observable<Sesion[]> {
    return this.http.get<Sesion[]>(this.baseUrl);
  }

  obtenerPorId(id: number): Observable<Sesion> {
    return this.http.get<Sesion>(`${this.baseUrl}/${id}`);
  }

  obtenerPorUsuario(usuarioId: number): Observable<Sesion[]> {
    return this.http.get<Sesion[]>(`${this.baseUrl}/usuario/${usuarioId}`);
  }

  crear(sesion: Sesion): Observable<Sesion> {
    return this.http.post<Sesion>(this.baseUrl, sesion);
  }

  validarToken(token: string): Observable<{ mensaje: string }> {
    return this.http.post<{ mensaje: string }>(`${this.baseUrl}/validar`, token, {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  limpiarExpiradas(): Observable<{ mensaje: string }> {
    return this.http.post<{ mensaje: string }>(`${this.baseUrl}/limpiar-expiradas`, {});
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
```

### 4.4 `mensajes-contacto.service.ts`

```ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MensajeContacto } from '../models/api-models';

@Injectable({ providedIn: 'root' })
export class MensajesContactoService {
  private baseUrl = `${environment.apiBase}/mensajescontacto`;

  constructor(private http: HttpClient) {}

  obtenerTodos(): Observable<MensajeContacto[]> {
    return this.http.get<MensajeContacto[]>(this.baseUrl);
  }

  obtenerPorId(id: number): Observable<MensajeContacto> {
    return this.http.get<MensajeContacto>(`${this.baseUrl}/${id}`);
  }

  crear(mensaje: MensajeContacto): Observable<MensajeContacto> {
    return this.http.post<MensajeContacto>(this.baseUrl, mensaje);
  }

  eliminar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
```

---

## 5) Ejemplos de uso en componentes

Listar usuarios en `usuarios.component.ts`:

```ts
import { Component, OnInit } from '@angular/core';
import { UsuariosService } from '../core/services/usuarios.service';
import { Usuario } from '../core/models/api-models';

@Component({ selector: 'app-usuarios', template: `
  <h2>Usuarios</h2>
  <ul>
    <li *ngFor="let u of usuarios">{{ u.nombreCompleto }} ({{ u.email }})</li>
  </ul>
` })
export class UsuariosComponent implements OnInit {
  usuarios: Usuario[] = [];

  constructor(private usuariosService: UsuariosService) {}

  ngOnInit(): void {
    this.usuariosService.obtenerTodos().subscribe(res => this.usuarios = res);
  }
}
```

Crear reporte en `reporte-form.component.ts`:

```ts
this.reportesService.crear({
  nombre: 'Reporte Mensual',
  tipoReporte: 'Mensual',
  descripcion: 'Detalle del mes',
  usuarioId: 1
}).subscribe(r => console.log('Creado', r));
```

---

## 6) Pruebas rápidas

- Verificar backend: `GET http://localhost:5291/api/pruebas/verificar-conexion`
- Archivo útil: `HoneyBack_API_Tests.http` (reemplaza `@baseUrl` si cambias el puerto)

---

## 7) Análisis de entidades (según backend real)

Origen: modelos en `Models/` y `DbSet<>` en `HoneyBalanceDbContext`.

### Entidad: `Usuario`
- Uso: gestión de usuarios, relación con reportes y sesiones
- Campos: `UsuarioId`, `NombreCompleto`, `Email` (único), `PasswordHash`, `FechaRegistro`
- Relaciones: `Usuario` ? `Reporte` (1:N), `Usuario` ? `Sesione` (1:N)
- Notas: `FechaRegistro` default en BD (`getdate()`)
---

### Entidad: `Reporte`
- Uso: módulo de reportes
- Campos: `ReporteId`, `Nombre`, `TipoReporte`, `Descripcion`, `FechaReporte`, `Estado`, `UsuarioId?`
- Relaciones: `Reporte` ? `Usuario` (N:1, opcional)
- Notas: `Estado` default "Pendiente", `FechaReporte` default en BD (`getdate()`)
---

### Entidad: `Sesione`
- Uso: módulo de sesiones/tokens
- Campos: `SesionId`, `UsuarioId`, `TokenSesion` (único), `FechaExpiracion`
- Relaciones: `Sesione` ? `Usuario` (N:1, requerido)
- Notas: índice único en `TokenSesion`
---

### Entidad: `MensajesContacto`
- Uso: módulo de contacto
- Campos: `ContactoId`, `Nombre`, `Email`, `Mensaje`, `FechaEnvio`
- Relaciones: sin relaciones
- Notas: `FechaEnvio` default en BD (`getdate()`)
---

### Entidades sugeridas
- No se detectan entidades faltantes basadas en los endpoints y el `DbContext` actual.
---

## 8) Resumen de endpoints (para Angular)

Base: `http://localhost:5291/api`

- `GET /usuarios`
- `GET /usuarios/{id}`
- `GET /usuarios/email/{email}`
- `POST /usuarios` (JSON)
- `PUT /usuarios/{id}` (JSON)
- `DELETE /usuarios/{id}`

- `GET /reportes`
- `GET /reportes/{id}`
- `GET /reportes/usuario/{id}`
- `GET /reportes/estado/{estado}`
- `POST /reportes` (JSON)
- `PUT /reportes/{id}` (JSON)
- `DELETE /reportes/{id}`

- `GET /sesiones`
- `GET /sesiones/{id}`
- `GET /sesiones/usuario/{id}`
- `POST /sesiones` (JSON)
- `POST /sesiones/validar` (body: string JSON con comillas)
- `POST /sesiones/limpiar-expiradas`
- `DELETE /sesiones/{id}`

- `GET /mensajescontacto`
- `GET /mensajescontacto/{id}`
- `POST /mensajescontacto` (JSON)
- `DELETE /mensajescontacto/{id}`

Headers comunes:
- `Content-Type: application/json` para `POST`/`PUT`/`POST validar`
- No se requieren tokens en headers (autenticación no habilitada)

---

Con esto tu Angular (`honeybalance-angular`) puede consumir todo el backend en `http://localhost:5291/api` de forma directa.
