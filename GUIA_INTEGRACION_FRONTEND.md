# ?? GUÍA DE INTEGRACIÓN FRONTEND - NUEVOS MÓDULOS API

## ?? Fecha: 2025-01-19
## ?? Versión Backend: v2.0 - Expansión Completa

---

## ?? TABLA DE CONTENIDOS

1. [Resumen de Cambios](#resumen-de-cambios)
2. [Configuración Inicial](#configuración-inicial)
3. [Autenticación y Seguridad](#autenticación-y-seguridad)
4. [Módulos Implementados](#módulos-implementados)
5. [Modelos TypeScript](#modelos-typescript)
6. [Servicios Angular](#servicios-angular)
7. [Interceptores HTTP](#interceptores-http)
8. [Ejemplos de Uso](#ejemplos-de-uso)
9. [Manejo de Errores](#manejo-de-errores)
10. [Testing](#testing)

---

## ?? RESUMEN DE CAMBIOS

Se han agregado **5 nuevos módulos** al backend con **41 endpoints** totalmente funcionales:

| Módulo | Endpoints | Requiere Auth | Funcionalidad Principal |
|--------|-----------|---------------|-------------------------|
| **CategoriasTransacciones** | 8 | ? Sí | Gestión de categorías de ingresos/gastos |
| **MetasAhorro** | 9 | ? Sí | Control de metas financieras |
| **EstadisticasMensuales** | 9 | ? Sí | Estadísticas y reportes mensuales |
| **Templates** | 8 | ? Sí | Plantillas de transacciones reutilizables |
| **ConfiguracionesUsuario** | 7 | ? Sí | Preferencias y configuración del usuario |

**Base URL del API:** `http://localhost:5291/api` (desarrollo)

---

## ?? CONFIGURACIÓN INICIAL

### 1. Actualizar Environment

**`src/environments/environment.ts`**
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5291/api',
  endpoints: {
    // Endpoints existentes
    auth: '/auth',
    usuarios: '/usuarios',
    reportes: '/reportes',
    sesiones: '/sesiones',
    mensajesContacto: '/mensajescontacto',
    
    // NUEVOS ENDPOINTS
    categorias: '/categoriasTransacciones',
    metas: '/metasAhorro',
    estadisticas: '/estadisticasMensuales',
    templates: '/templates',
    configuraciones: '/configuracionesUsuario'
  },
  jwt: {
    tokenKey: 'honey_token',
    expiresAtKey: 'honey_token_expires',
    userKey: 'honey_user'
  }
};
```

**`src/environments/environment.prod.ts`**
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://api.honeybalance.com/api', // Cambiar por tu dominio
  endpoints: {
    auth: '/auth',
    usuarios: '/usuarios',
    reportes: '/reportes',
    sesiones: '/sesiones',
    mensajesContacto: '/mensajescontacto',
    categorias: '/categoriasTransacciones',
    metas: '/metasAhorro',
    estadisticas: '/estadisticasMensuales',
    templates: '/templates',
    configuraciones: '/configuracionesUsuario'
  },
  jwt: {
    tokenKey: 'honey_token',
    expiresAtKey: 'honey_token_expires',
    userKey: 'honey_user'
  }
};
```

### 2. Instalar Dependencias (si es necesario)

```bash
# Si aún no tienes estos paquetes
npm install --save date-fns
npm install --save-dev @types/node
```

---

## ?? AUTENTICACIÓN Y SEGURIDAD

### ?? IMPORTANTE: Todos los nuevos endpoints requieren JWT

Todos los 41 nuevos endpoints están protegidos con `[Authorize]`. Debes incluir el token JWT en cada petición.

### Estructura del Token JWT

```typescript
interface JwtPayload {
  sub: string;        // ID del usuario
  email: string;      // Email del usuario
  name: string;       // Nombre completo
  exp: number;        // Timestamp de expiración
  iss: string;        // Issuer (HoneyBack)
  aud: string;        // Audience (HoneyBackAPI)
}
```

### Servicio de Autenticación Actualizado

**`src/app/core/services/auth.service.ts`**

```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  usuario: UsuarioResponse;
}

export interface UsuarioResponse {
  usuarioId: number;
  nombreCompleto: string;
  email: string;
  fechaRegistro: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.apiUrl;
  private currentUserSubject = new BehaviorSubject<UsuarioResponse | null>(this.getUserFromStorage());
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {}

  // Login
  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(
      `${this.apiUrl}${environment.endpoints.auth}/login`,
      credentials
    ).pipe(
      tap(response => this.setSession(response))
    );
  }

  // Registro
  register(data: any): Observable<UsuarioResponse> {
    return this.http.post<UsuarioResponse>(
      `${this.apiUrl}${environment.endpoints.auth}/register`,
      data
    );
  }

  // Obtener usuario actual (requiere token)
  getCurrentUser(): Observable<UsuarioResponse> {
    return this.http.get<UsuarioResponse>(
      `${this.apiUrl}${environment.endpoints.auth}/me`,
      { headers: this.getAuthHeaders() }
    ).pipe(
      tap(user => this.currentUserSubject.next(user))
    );
  }

  // Cambiar contraseña
  changePassword(oldPassword: string, newPassword: string): Observable<any> {
    return this.http.post(
      `${this.apiUrl}${environment.endpoints.auth}/cambiar-password`,
      { passwordActual: oldPassword, passwordNuevo: newPassword },
      { headers: this.getAuthHeaders() }
    );
  }

  // Logout
  logout(): Observable<any> {
    return this.http.post(
      `${this.apiUrl}${environment.endpoints.auth}/logout`,
      {},
      { headers: this.getAuthHeaders() }
    ).pipe(
      tap(() => this.clearSession())
    );
  }

  // Gestión de sesión
  private setSession(authResult: LoginResponse): void {
    localStorage.setItem(environment.jwt.tokenKey, authResult.token);
    localStorage.setItem(environment.jwt.expiresAtKey, authResult.expiresAt);
    localStorage.setItem(environment.jwt.userKey, JSON.stringify(authResult.usuario));
    this.currentUserSubject.next(authResult.usuario);
  }

  private clearSession(): void {
    localStorage.removeItem(environment.jwt.tokenKey);
    localStorage.removeItem(environment.jwt.expiresAtKey);
    localStorage.removeItem(environment.jwt.userKey);
    this.currentUserSubject.next(null);
  }

  private getUserFromStorage(): UsuarioResponse | null {
    const userStr = localStorage.getItem(environment.jwt.userKey);
    return userStr ? JSON.parse(userStr) : null;
  }

  // Obtener token
  getToken(): string | null {
    return localStorage.getItem(environment.jwt.tokenKey);
  }

  // Headers con autorización
  getAuthHeaders(): HttpHeaders {
    const token = this.getToken();
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': token ? `Bearer ${token}` : ''
    });
  }

  // Verificar si está logueado
  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;

    const expiresAt = localStorage.getItem(environment.jwt.expiresAtKey);
    if (!expiresAt) return false;

    return new Date(expiresAt) > new Date();
  }

  // Obtener ID del usuario actual
  getCurrentUserId(): number | null {
    const user = this.currentUserSubject.value;
    return user ? user.usuarioId : null;
  }
}
```

### Interceptor HTTP para Token Automático

**`src/app/core/interceptors/auth.interceptor.ts`**

```typescript
import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.getToken();

    // Agregar token si existe y la petición es a nuestro API
    if (token && req.url.includes('/api/')) {
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }

    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // Token inválido o expirado
          this.authService.logout().subscribe(() => {
            this.router.navigate(['/login']);
          });
        }
        return throwError(() => error);
      })
    );
  }
}
```

**Registrar en `app.config.ts` o `app.module.ts`:**

```typescript
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthInterceptor } from './core/interceptors/auth.interceptor';

// En providers:
providers: [
  {
    provide: HTTP_INTERCEPTORS,
    useClass: AuthInterceptor,
    multi: true
  }
]
```

---

## ?? MODELOS TYPESCRIPT

### Archivo: `src/app/shared/models/api.models.ts`

```typescript
// ========================================
// CATEGORÍAS DE TRANSACCIONES
// ========================================

export interface CategoriaTransaccion {
  categoriaId: number;
  nombre: string;
  tipo: 'Ingreso' | 'Gasto';
  color?: string;
  icono?: string;
  esSistema?: boolean;
  usuarioId?: number;
  activa?: boolean;
}

export interface CategoriaTransaccionCreate {
  nombre: string;
  tipo: 'Ingreso' | 'Gasto';
  color?: string;
  icono?: string;
  esSistema?: boolean;
  usuarioId?: number;
  activa?: boolean;
}

export interface CategoriaTransaccionUpdate {
  nombre: string;
  tipo: 'Ingreso' | 'Gasto';
  color?: string;
  icono?: string;
  activa?: boolean;
}

// ========================================
// METAS DE AHORRO
// ========================================

export interface MetaAhorro {
  metaId: number;
  usuarioId: number;
  nombre: string;
  descripcion?: string;
  montoObjetivo: number;
  montoActual: number;
  fechaInicio: string; // DateOnly (yyyy-MM-dd)
  fechaObjetivo?: string; // DateOnly (yyyy-MM-dd)
  fechaCompletada?: string; // DateTime ISO
  color?: string;
  icono?: string;
  prioridad?: number;
  activa?: boolean;
  completada?: boolean;
  porcentajeAvance: number; // Calculado por el backend
}

export interface MetaAhorroCreate {
  usuarioId: number;
  nombre: string;
  descripcion?: string;
  montoObjetivo: number;
  montoActual?: number;
  fechaObjetivo?: string; // DateOnly (yyyy-MM-dd)
  color?: string;
  icono?: string;
  prioridad?: number;
}

export interface MetaAhorroUpdate {
  nombre: string;
  descripcion?: string;
  montoObjetivo: number;
  montoActual?: number;
  fechaObjetivo?: string;
  color?: string;
  icono?: string;
  prioridad?: number;
  activa?: boolean;
  completada?: boolean;
}

// ========================================
// ESTADÍSTICAS MENSUALES
// ========================================

export interface EstadisticaMensual {
  estadisticaId: number;
  usuarioId: number;
  anio: number;
  mes: number; // 1-12
  totalIngresos: number;
  totalGastos: number;
  balance: number; // Calculado: totalIngresos - totalGastos
  numTransacciones: number;
  categoriaMayorGastoId?: number;
  categoriaMayorGastoNombre?: string;
  montoMayorGasto?: number;
  fechaCalculo: string; // DateTime ISO
}

export interface EstadisticaMensualCreate {
  usuarioId: number;
  anio: number;
  mes: number;
  totalIngresos?: number;
  totalGastos?: number;
  numTransacciones?: number;
  categoriaMayorGastoId?: number;
  montoMayorGasto?: number;
}

export interface EstadisticaMensualUpdate {
  totalIngresos?: number;
  totalGastos?: number;
  numTransacciones?: number;
  categoriaMayorGastoId?: number;
  montoMayorGasto?: number;
}

// ========================================
// TEMPLATES (PLANTILLAS)
// ========================================

export interface Template {
  templateId: number;
  usuarioId: number;
  nombre: string;
  categoriaId: number;
  categoriaNombre?: string;
  monto: number;
  descripcion?: string;
  tipo: 'Ingreso' | 'Gasto';
  frecuenciaUso: number;
  fechaCreacion: string; // DateTime ISO
  fechaUltimoUso?: string; // DateTime ISO
  activo: boolean;
}

export interface TemplateCreate {
  usuarioId: number;
  nombre: string;
  categoriaId: number;
  monto: number;
  descripcion?: string;
  tipo: 'Ingreso' | 'Gasto';
}

export interface TemplateUpdate {
  nombre: string;
  categoriaId: number;
  monto: number;
  descripcion?: string;
  tipo: 'Ingreso' | 'Gasto';
  activo?: boolean;
}

// ========================================
// CONFIGURACIONES DE USUARIO
// ========================================

export interface ConfiguracionUsuario {
  configuracionId: number;
  usuarioId: number;
  notificacionesEmail: boolean;
  notificacionesPush: boolean;
  tema: 'light' | 'dark' | 'auto';
  idioma: string; // 'es', 'en', etc.
  timezone: string; // 'America/Bogota', etc.
  formatoFecha: string; // 'DD/MM/YYYY', 'MM/DD/YYYY', etc.
  primeraVez: boolean;
  configuracionPersonalizada?: string; // JSON string
  fechaActualizacion: string; // DateTime ISO
}

export interface ConfiguracionUsuarioCreate {
  usuarioId: number;
  notificacionesEmail?: boolean;
  notificacionesPush?: boolean;
  tema?: 'light' | 'dark' | 'auto';
  idioma?: string;
  timezone?: string;
  formatoFecha?: string;
  primeraVez?: boolean;
}

export interface ConfiguracionUsuarioUpdate {
  notificacionesEmail?: boolean;
  notificacionesPush?: boolean;
  tema?: 'light' | 'dark' | 'auto';
  idioma?: string;
  timezone?: string;
  formatoFecha?: string;
  primeraVez?: boolean;
  configuracionPersonalizada?: string;
}

// ========================================
// RESPUESTAS GENÉRICAS
// ========================================

export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}

export interface ApiError {
  mensaje: string;
  error?: string;
}
```

---

## ?? SERVICIOS ANGULAR

### 1. Servicio de Categorías

**`src/app/core/services/categorias.service.ts`**

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  CategoriaTransaccion, 
  CategoriaTransaccionCreate, 
  CategoriaTransaccionUpdate 
} from '../../shared/models/api.models';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class CategoriasService {
  private apiUrl = `${environment.apiUrl}${environment.endpoints.categorias}`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  // Obtener todas las categorías
  getAll(): Observable<CategoriaTransaccion[]> {
    return this.http.get<CategoriaTransaccion[]>(this.apiUrl, {
      headers: this.authService.getAuthHeaders()
    });
  }

  // Obtener por ID
  getById(id: number): Observable<CategoriaTransaccion> {
    return this.http.get<CategoriaTransaccion>(`${this.apiUrl}/${id}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  // Obtener por usuario
  getByUsuario(usuarioId: number): Observable<CategoriaTransaccion[]> {
    return this.http.get<CategoriaTransaccion[]>(`${this.apiUrl}/usuario/${usuarioId}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  // Obtener por tipo (Ingreso/Gasto)
  getByTipo(tipo: 'Ingreso' | 'Gasto'): Observable<CategoriaTransaccion[]> {
    return this.http.get<CategoriaTransaccion[]>(`${this.apiUrl}/tipo/${tipo}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  // Obtener activas por usuario
  getActivasByUsuario(usuarioId: number): Observable<CategoriaTransaccion[]> {
    return this.http.get<CategoriaTransaccion[]>(`${this.apiUrl}/usuario/${usuarioId}/activas`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  // Crear categoría
  create(categoria: CategoriaTransaccionCreate): Observable<CategoriaTransaccion> {
    return this.http.post<CategoriaTransaccion>(this.apiUrl, categoria, {
      headers: this.authService.getAuthHeaders()
    });
  }

  // Actualizar categoría
  update(id: number, categoria: CategoriaTransaccionUpdate): Observable<CategoriaTransaccion> {
    return this.http.put<CategoriaTransaccion>(`${this.apiUrl}/${id}`, categoria, {
      headers: this.authService.getAuthHeaders()
    });
  }

  // Eliminar categoría
  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, {
      headers: this.authService.getAuthHeaders()
    });
  }
}
```

### 2. Servicio de Metas de Ahorro

**`src/app/core/services/metas-ahorro.service.ts`**

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { MetaAhorro, MetaAhorroCreate, MetaAhorroUpdate } from '../../shared/models/api.models';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class MetasAhorroService {
  private apiUrl = `${environment.apiUrl}${environment.endpoints.metas}`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  getAll(): Observable<MetaAhorro[]> {
    return this.http.get<MetaAhorro[]>(this.apiUrl, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getById(id: number): Observable<MetaAhorro> {
    return this.http.get<MetaAhorro>(`${this.apiUrl}/${id}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getByUsuario(usuarioId: number): Observable<MetaAhorro[]> {
    return this.http.get<MetaAhorro[]>(`${this.apiUrl}/usuario/${usuarioId}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getActivasByUsuario(usuarioId: number): Observable<MetaAhorro[]> {
    return this.http.get<MetaAhorro[]>(`${this.apiUrl}/usuario/${usuarioId}/activas`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getCompletadasByUsuario(usuarioId: number): Observable<MetaAhorro[]> {
    return this.http.get<MetaAhorro[]>(`${this.apiUrl}/usuario/${usuarioId}/completadas`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  create(meta: MetaAhorroCreate): Observable<MetaAhorro> {
    return this.http.post<MetaAhorro>(this.apiUrl, meta, {
      headers: this.authService.getAuthHeaders()
    });
  }

  update(id: number, meta: MetaAhorroUpdate): Observable<MetaAhorro> {
    return this.http.put<MetaAhorro>(`${this.apiUrl}/${id}`, meta, {
      headers: this.authService.getAuthHeaders()
    });
  }

  // Actualizar solo el monto actual
  updateMonto(id: number, nuevoMonto: number): Observable<MetaAhorro> {
    return this.http.patch<MetaAhorro>(`${this.apiUrl}/${id}/monto`, nuevoMonto, {
      headers: this.authService.getAuthHeaders()
    });
  }

  // Marcar como completada
  completar(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/completar`, {}, {
      headers: this.authService.getAuthHeaders()
    });
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, {
      headers: this.authService.getAuthHeaders()
    });
  }
}
```

### 3. Servicio de Estadísticas

**`src/app/core/services/estadisticas.service.ts`**

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  EstadisticaMensual, 
  EstadisticaMensualCreate, 
  EstadisticaMensualUpdate 
} from '../../shared/models/api.models';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class EstadisticasService {
  private apiUrl = `${environment.apiUrl}${environment.endpoints.estadisticas}`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  getAll(): Observable<EstadisticaMensual[]> {
    return this.http.get<EstadisticaMensual[]>(this.apiUrl, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getById(id: number): Observable<EstadisticaMensual> {
    return this.http.get<EstadisticaMensual>(`${this.apiUrl}/${id}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getByUsuario(usuarioId: number): Observable<EstadisticaMensual[]> {
    return this.http.get<EstadisticaMensual[]>(`${this.apiUrl}/usuario/${usuarioId}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  // Obtener estadística de un período específico
  getByPeriodo(usuarioId: number, anio: number, mes: number): Observable<EstadisticaMensual> {
    return this.http.get<EstadisticaMensual>(
      `${this.apiUrl}/usuario/${usuarioId}/periodo/${anio}/${mes}`,
      { headers: this.authService.getAuthHeaders() }
    );
  }

  // Obtener estadísticas de un año completo
  getByAnio(usuarioId: number, anio: number): Observable<EstadisticaMensual[]> {
    return this.http.get<EstadisticaMensual[]>(
      `${this.apiUrl}/usuario/${usuarioId}/anio/${anio}`,
      { headers: this.authService.getAuthHeaders() }
    );
  }

  create(estadistica: EstadisticaMensualCreate): Observable<EstadisticaMensual> {
    return this.http.post<EstadisticaMensual>(this.apiUrl, estadistica, {
      headers: this.authService.getAuthHeaders()
    });
  }

  update(id: number, estadistica: EstadisticaMensualUpdate): Observable<EstadisticaMensual> {
    return this.http.put<EstadisticaMensual>(`${this.apiUrl}/${id}`, estadistica, {
      headers: this.authService.getAuthHeaders()
    });
  }

  // Recalcular estadística de un período
  recalcular(usuarioId: number, anio: number, mes: number): Observable<any> {
    return this.http.post(
      `${this.apiUrl}/usuario/${usuarioId}/recalcular/${anio}/${mes}`,
      {},
      { headers: this.authService.getAuthHeaders() }
    );
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, {
      headers: this.authService.getAuthHeaders()
    });
  }
}
```

### 4. Servicio de Templates

**`src/app/core/services/templates.service.ts`**

```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Template, TemplateCreate, TemplateUpdate } from '../../shared/models/api.models';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class TemplatesService {
  private apiUrl = `${environment.apiUrl}${environment.endpoints.templates}`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  getAll(): Observable<Template[]> {
    return this.http.get<Template[]>(this.apiUrl, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getById(id: number): Observable<Template> {
    return this.http.get<Template>(`${this.apiUrl}/${id}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getByUsuario(usuarioId: number): Observable<Template[]> {
    return this.http.get<Template[]>(`${this.apiUrl}/usuario/${usuarioId}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getActivosByUsuario(usuarioId: number): Observable<Template[]> {
    return this.http.get<Template[]>(`${this.apiUrl}/usuario/${usuarioId}/activos`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  // Obtener los templates más usados
  getMasUsados(usuarioId: number, cantidad: number = 10): Observable<Template[]> {
    const params = new HttpParams().set('cantidad', cantidad.toString());
    return this.http.get<Template[]>(
      `${this.apiUrl}/usuario/${usuarioId}/mas-usados`,
      { 
        headers: this.authService.getAuthHeaders(),
        params: params
      }
    );
  }

  create(template: TemplateCreate): Observable<Template> {
    return this.http.post<Template>(this.apiUrl, template, {
      headers: this.authService.getAuthHeaders()
    });
  }

  update(id: number, template: TemplateUpdate): Observable<Template> {
    return this.http.put<Template>(`${this.apiUrl}/${id}`, template, {
      headers: this.authService.getAuthHeaders()
    });
  }

  // Registrar uso de un template (incrementa contador)
  registrarUso(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/usar`, {}, {
      headers: this.authService.getAuthHeaders()
    });
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, {
      headers: this.authService.getAuthHeaders()
    });
  }
}
```

### 5. Servicio de Configuraciones

**`src/app/core/services/configuraciones.service.ts`**

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  ConfiguracionUsuario, 
  ConfiguracionUsuarioCreate, 
  ConfiguracionUsuarioUpdate 
} from '../../shared/models/api.models';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class ConfiguracionesService {
  private apiUrl = `${environment.apiUrl}${environment.endpoints.configuraciones}`;
  private currentConfigSubject = new BehaviorSubject<ConfiguracionUsuario | null>(null);
  public currentConfig$ = this.currentConfigSubject.asObservable();

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  getAll(): Observable<ConfiguracionUsuario[]> {
    return this.http.get<ConfiguracionUsuario[]>(this.apiUrl, {
      headers: this.authService.getAuthHeaders()
    });
  }

  getById(id: number): Observable<ConfiguracionUsuario> {
    return this.http.get<ConfiguracionUsuario>(`${this.apiUrl}/${id}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  // Obtener configuración del usuario (1:1 relación)
  getByUsuarioId(usuarioId: number): Observable<ConfiguracionUsuario> {
    return this.http.get<ConfiguracionUsuario>(`${this.apiUrl}/usuario/${usuarioId}`, {
      headers: this.authService.getAuthHeaders()
    }).pipe(
      tap(config => this.currentConfigSubject.next(config))
    );
  }

  create(configuracion: ConfiguracionUsuarioCreate): Observable<ConfiguracionUsuario> {
    return this.http.post<ConfiguracionUsuario>(this.apiUrl, configuracion, {
      headers: this.authService.getAuthHeaders()
    }).pipe(
      tap(config => this.currentConfigSubject.next(config))
    );
  }

  update(id: number, configuracion: ConfiguracionUsuarioUpdate): Observable<ConfiguracionUsuario> {
    return this.http.put<ConfiguracionUsuario>(`${this.apiUrl}/${id}`, configuracion, {
      headers: this.authService.getAuthHeaders()
    }).pipe(
      tap(config => this.currentConfigSubject.next(config))
    );
  }

  // Marcar usuario como veterano (PrimeraVez = false)
  marcarComoVeterano(usuarioId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/usuario/${usuarioId}/marcar-veterano`, {}, {
      headers: this.authService.getAuthHeaders()
    });
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`, {
      headers: this.authService.getAuthHeaders()
    });
  }

  // Helper: Obtener configuración actual desde BehaviorSubject
  getCurrentConfig(): ConfiguracionUsuario | null {
    return this.currentConfigSubject.value;
  }
}
```

---

## ?? EJEMPLOS DE USO EN COMPONENTES

### Ejemplo 1: Cargar Categorías Activas

**`src/app/components/categorias/categorias.component.ts`**

```typescript
import { Component, OnInit } from '@angular/core';
import { CategoriasService } from '../../core/services/categorias.service';
import { AuthService } from '../../core/services/auth.service';
import { CategoriaTransaccion } from '../../shared/models/api.models';

@Component({
  selector: 'app-categorias',
  templateUrl: './categorias.component.html'
})
export class CategoriasComponent implements OnInit {
  categorias: CategoriaTransaccion[] = [];
  loading = false;
  error: string | null = null;

  constructor(
    private categoriasService: CategoriasService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.cargarCategorias();
  }

  cargarCategorias(): void {
    const usuarioId = this.authService.getCurrentUserId();
    if (!usuarioId) return;

    this.loading = true;
    this.categoriasService.getActivasByUsuario(usuarioId).subscribe({
      next: (data) => {
        this.categorias = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.mensaje || 'Error al cargar categorías';
        this.loading = false;
      }
    });
  }

  crearCategoria(): void {
    const usuarioId = this.authService.getCurrentUserId();
    if (!usuarioId) return;

    const nuevaCategoria = {
      nombre: 'Salud',
      tipo: 'Gasto' as const,
      color: '#FF5722',
      icono: 'medical',
      usuarioId: usuarioId,
      activa: true
    };

    this.categoriasService.create(nuevaCategoria).subscribe({
      next: (categoria) => {
        console.log('Categoría creada:', categoria);
        this.cargarCategorias(); // Recargar lista
      },
      error: (err) => {
        console.error('Error al crear categoría:', err);
      }
    });
  }
}
```

### Ejemplo 2: Gestionar Metas de Ahorro

**`src/app/components/metas/metas.component.ts`**

```typescript
import { Component, OnInit } from '@angular/core';
import { MetasAhorroService } from '../../core/services/metas-ahorro.service';
import { AuthService } from '../../core/services/auth.service';
import { MetaAhorro, MetaAhorroCreate } from '../../shared/models/api.models';

@Component({
  selector: 'app-metas',
  templateUrl: './metas.component.html'
})
export class MetasComponent implements OnInit {
  metasActivas: MetaAhorro[] = [];
  metasCompletadas: MetaAhorro[] = [];

  constructor(
    private metasService: MetasAhorroService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.cargarMetas();
  }

  cargarMetas(): void {
    const usuarioId = this.authService.getCurrentUserId();
    if (!usuarioId) return;

    // Cargar metas activas
    this.metasService.getActivasByUsuario(usuarioId).subscribe({
      next: (data) => {
        this.metasActivas = data;
      },
      error: (err) => console.error('Error:', err)
    });

    // Cargar metas completadas
    this.metasService.getCompletadasByUsuario(usuarioId).subscribe({
      next: (data) => {
        this.metasCompletadas = data;
      },
      error: (err) => console.error('Error:', err)
    });
  }

  crearMeta(): void {
    const usuarioId = this.authService.getCurrentUserId();
    if (!usuarioId) return;

    const nuevaMeta: MetaAhorroCreate = {
      usuarioId: usuarioId,
      nombre: 'Vacaciones 2025',
      descripcion: 'Viaje a Europa',
      montoObjetivo: 5000000,
      montoActual: 0,
      fechaObjetivo: '2025-12-31',
      color: '#4CAF50',
      icono: 'flight',
      prioridad: 5
    };

    this.metasService.create(nuevaMeta).subscribe({
      next: (meta) => {
        console.log('Meta creada:', meta);
        this.cargarMetas();
      },
      error: (err) => console.error('Error:', err)
    });
  }

  actualizarMonto(metaId: number, nuevoMonto: number): void {
    this.metasService.updateMonto(metaId, nuevoMonto).subscribe({
      next: (meta) => {
        console.log('Monto actualizado:', meta);
        if (meta.completada) {
          alert(`¡Meta "${meta.nombre}" completada! ??`);
        }
        this.cargarMetas();
      },
      error: (err) => console.error('Error:', err)
    });
  }
}
```

### Ejemplo 3: Dashboard con Estadísticas

**`src/app/components/dashboard/dashboard.component.ts`**

```typescript
import { Component, OnInit } from '@angular/core';
import { EstadisticasService } from '../../core/services/estadisticas.service';
import { AuthService } from '../../core/services/auth.service';
import { EstadisticaMensual } from '../../shared/models/api.models';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  estadisticaActual: EstadisticaMensual | null = null;
  estadisticasAnio: EstadisticaMensual[] = [];

  constructor(
    private estadisticasService: EstadisticasService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.cargarEstadisticas();
  }

  cargarEstadisticas(): void {
    const usuarioId = this.authService.getCurrentUserId();
    if (!usuarioId) return;

    const fechaActual = new Date();
    const anioActual = fechaActual.getFullYear();
    const mesActual = fechaActual.getMonth() + 1;

    // Estadística del mes actual
    this.estadisticasService.getByPeriodo(usuarioId, anioActual, mesActual).subscribe({
      next: (data) => {
        this.estadisticaActual = data;
        console.log('Balance mes actual:', data.balance);
      },
      error: (err) => {
        if (err.status === 404) {
          console.log('No hay estadísticas para este mes');
        }
      }
    });

    // Estadísticas del año completo
    this.estadisticasService.getByAnio(usuarioId, anioActual).subscribe({
      next: (data) => {
        this.estadisticasAnio = data;
        this.generarGrafico(data);
      },
      error: (err) => console.error('Error:', err)
    });
  }

  generarGrafico(estadisticas: EstadisticaMensual[]): void {
    // Aquí integrarías con Chart.js, ApexCharts, etc.
    const labels = estadisticas.map(e => `${e.mes}/${e.anio}`);
    const ingresos = estadisticas.map(e => e.totalIngresos);
    const gastos = estadisticas.map(e => e.totalGastos);
    
    console.log('Datos para gráfico:', { labels, ingresos, gastos });
  }
}
```

### Ejemplo 4: Aplicar Configuraciones del Usuario

**`src/app/app.component.ts`**

```typescript
import { Component, OnInit } from '@angular/core';
import { ConfiguracionesService } from './core/services/configuraciones.service';
import { AuthService } from './core/services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  
  constructor(
    private configuracionesService: ConfiguracionesService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Cargar configuración cuando el usuario se loguea
    this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.cargarConfiguracion(user.usuarioId);
      }
    });
  }

  cargarConfiguracion(usuarioId: number): void {
    this.configuracionesService.getByUsuarioId(usuarioId).subscribe({
      next: (config) => {
        // Aplicar tema
        this.aplicarTema(config.tema);

        // Mostrar onboarding si es primera vez
        if (config.primeraVez) {
          this.mostrarOnboarding();
        }

        console.log('Configuración cargada:', config);
      },
      error: (err) => {
        // Si no existe configuración, crear una por defecto
        if (err.status === 404) {
          this.crearConfiguracionPorDefecto(usuarioId);
        }
      }
    });
  }

  aplicarTema(tema: string): void {
    document.body.classList.remove('theme-light', 'theme-dark');
    
    if (tema === 'auto') {
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      tema = prefersDark ? 'dark' : 'light';
    }
    
    document.body.classList.add(`theme-${tema}`);
  }

  mostrarOnboarding(): void {
    this.router.navigate(['/onboarding']);
  }

  crearConfiguracionPorDefecto(usuarioId: number): void {
    const configDefault = {
      usuarioId: usuarioId,
      tema: 'dark' as const,
      idioma: 'es',
      notificacionesEmail: true,
      notificacionesPush: true
    };

    this.configuracionesService.create(configDefault).subscribe({
      next: (config) => {
        console.log('Configuración creada:', config);
        this.aplicarTema(config.tema);
      },
      error: (err) => console.error('Error:', err)
    });
  }
}
```

### Ejemplo 5: Usar Templates en Transacciones

**`src/app/components/transacciones/nueva-transaccion.component.ts`**

```typescript
import { Component, OnInit } from '@angular/core';
import { TemplatesService } from '../../core/services/templates.service';
import { AuthService } from '../../core/services/auth.service';
import { Template } from '../../shared/models/api.models';

@Component({
  selector: 'app-nueva-transaccion',
  templateUrl: './nueva-transaccion.component.html'
})
export class NuevaTransaccionComponent implements OnInit {
  templatesMasUsados: Template[] = [];
  transaccion = {
    categoriaId: 0,
    monto: 0,
    descripcion: '',
    tipo: 'Gasto'
  };

  constructor(
    private templatesService: TemplatesService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.cargarTemplatesFrecuentes();
  }

  cargarTemplatesFrecuentes(): void {
    const usuarioId = this.authService.getCurrentUserId();
    if (!usuarioId) return;

    this.templatesService.getMasUsados(usuarioId, 5).subscribe({
      next: (data) => {
        this.templatesMasUsados = data;
      },
      error: (err) => console.error('Error:', err)
    });
  }

  aplicarTemplate(template: Template): void {
    // Aplicar datos del template al formulario
    this.transaccion.categoriaId = template.categoriaId;
    this.transaccion.monto = template.monto;
    this.transaccion.descripcion = template.descripcion || '';
    this.transaccion.tipo = template.tipo;

    // Registrar uso del template
    this.templatesService.registrarUso(template.templateId).subscribe({
      next: () => {
        console.log('Uso registrado para template:', template.nombre);
      },
      error: (err) => console.error('Error:', err)
    });
  }

  guardarComoTemplate(): void {
    const usuarioId = this.authService.getCurrentUserId();
    if (!usuarioId) return;

    const nuevoTemplate = {
      usuarioId: usuarioId,
      nombre: 'Mi plantilla',
      categoriaId: this.transaccion.categoriaId,
      monto: this.transaccion.monto,
      descripcion: this.transaccion.descripcion,
      tipo: this.transaccion.tipo as 'Ingreso' | 'Gasto'
    };

    this.templatesService.create(nuevoTemplate).subscribe({
      next: (template) => {
        console.log('Template creado:', template);
        alert('Plantilla guardada exitosamente');
      },
      error: (err) => console.error('Error:', err)
    });
  }
}
```

---

## ?? MANEJO DE ERRORES

### Servicio de Manejo de Errores

**`src/app/core/services/error-handler.service.ts`**

```typescript
import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

export interface ApiError {
  mensaje: string;
  error?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ErrorHandlerService {

  handleError(error: HttpErrorResponse): string {
    if (error.error instanceof ErrorEvent) {
      // Error del lado del cliente
      return `Error: ${error.error.message}`;
    } else {
      // Error del lado del servidor
      const apiError = error.error as ApiError;
      
      switch (error.status) {
        case 400:
          return apiError.mensaje || 'Datos inválidos';
        case 401:
          return 'No autorizado. Por favor inicia sesión.';
        case 403:
          return 'No tienes permisos para realizar esta acción';
        case 404:
          return apiError.mensaje || 'Recurso no encontrado';
        case 409:
          return apiError.mensaje || 'Conflicto: el recurso ya existe';
        case 500:
          return 'Error del servidor. Intenta más tarde.';
        default:
          return apiError.mensaje || 'Error desconocido';
      }
    }
  }

  showError(error: HttpErrorResponse): void {
    const message = this.handleError(error);
    // Aquí puedes integrar con tu sistema de notificaciones
    console.error(message);
    alert(message); // Reemplazar con tu componente de toast/snackbar
  }
}
```

### Uso en Componentes

```typescript
import { ErrorHandlerService } from '../../core/services/error-handler.service';

constructor(private errorHandler: ErrorHandlerService) {}

cargarDatos(): void {
  this.service.getAll().subscribe({
    next: (data) => {
      this.datos = data;
    },
    error: (err) => {
      this.errorHandler.showError(err);
    }
  });
}
```

---

## ?? TESTING

### Ejemplo de Test para Servicio

**`src/app/core/services/categorias.service.spec.ts`**

```typescript
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { CategoriasService } from './categorias.service';
import { AuthService } from './auth.service';
import { CategoriaTransaccion } from '../../shared/models/api.models';

describe('CategoriasService', () => {
  let service: CategoriasService;
  let httpMock: HttpTestingController;
  let authService: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['getAuthHeaders']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        CategoriasService,
        { provide: AuthService, useValue: authServiceSpy }
      ]
    });

    service = TestBed.inject(CategoriasService);
    httpMock = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should retrieve active categories for user', () => {
    const mockCategorias: CategoriaTransaccion[] = [
      {
        categoriaId: 1,
        nombre: 'Alimentación',
        tipo: 'Gasto',
        color: '#FF5722',
        activa: true
      }
    ];

    service.getActivasByUsuario(1).subscribe(categorias => {
      expect(categorias.length).toBe(1);
      expect(categorias[0].nombre).toBe('Alimentación');
    });

    const req = httpMock.expectOne(`${service['apiUrl']}/usuario/1/activas`);
    expect(req.request.method).toBe('GET');
    req.flush(mockCategorias);
  });
});
```

---

## ?? ESTRUCTURA DE CARPETAS RECOMENDADA

```
src/app/
??? core/
?   ??? guards/
?   ?   ??? auth.guard.ts
?   ??? interceptors/
?   ?   ??? auth.interceptor.ts
?   ??? services/
?       ??? auth.service.ts
?       ??? categorias.service.ts           ? NUEVO
?       ??? metas-ahorro.service.ts         ? NUEVO
?       ??? estadisticas.service.ts         ? NUEVO
?       ??? templates.service.ts            ? NUEVO
?       ??? configuraciones.service.ts      ? NUEVO
?       ??? error-handler.service.ts
?
??? shared/
?   ??? models/
?   ?   ??? api.models.ts                   ? ACTUALIZADO
?   ??? components/
?
??? features/
?   ??? categorias/                         ? NUEVO MÓDULO
?   ?   ??? categorias.module.ts
?   ?   ??? categorias.component.ts
?   ?   ??? categorias.component.html
?   ?
?   ??? metas/                              ? NUEVO MÓDULO
?   ?   ??? metas.module.ts
?   ?   ??? lista-metas.component.ts
?   ?   ??? crear-meta.component.ts
?   ?   ??? detalle-meta.component.ts
?   ?
?   ??? estadisticas/                       ? NUEVO MÓDULO
?   ?   ??? estadisticas.module.ts
?   ?   ??? dashboard.component.ts
?   ?   ??? graficos.component.ts
?   ?
?   ??? templates/                          ? NUEVO MÓDULO
?   ?   ??? templates.module.ts
?   ?   ??? lista-templates.component.ts
?   ?   ??? crear-template.component.ts
?   ?
?   ??? configuracion/                      ? NUEVO MÓDULO
?       ??? configuracion.module.ts
?       ??? perfil.component.ts
?
??? app-routing.module.ts
```

---

## ??? GUARD DE AUTENTICACIÓN

**`src/app/core/guards/auth.guard.ts`**

```typescript
import { Injectable } from '@angular/core';
import { Router, CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    if (this.authService.isLoggedIn()) {
      return true;
    }

    // Redirigir a login con return URL
    this.router.navigate(['/login'], { 
      queryParams: { returnUrl: state.url } 
    });
    return false;
  }
}
```

**Uso en rutas:**

```typescript
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { 
    path: 'categorias', 
    component: CategoriasComponent,
    canActivate: [AuthGuard]
  },
  { 
    path: 'metas', 
    component: MetasComponent,
    canActivate: [AuthGuard]
  },
  // ... más rutas protegidas
];
```

---

## ?? CHECKLIST DE INTEGRACIÓN

### Configuración Base
- [ ] Actualizar `environment.ts` con nuevos endpoints
- [ ] Configurar interceptor JWT
- [ ] Implementar guard de autenticación
- [ ] Crear modelos TypeScript

### Servicios
- [ ] Implementar `CategoriasService`
- [ ] Implementar `MetasAhorroService`
- [ ] Implementar `EstadisticasService`
- [ ] Implementar `TemplatesService`
- [ ] Implementar `ConfiguracionesService`
- [ ] Implementar `ErrorHandlerService`

### Componentes
- [ ] Crear módulo de categorías
- [ ] Crear módulo de metas
- [ ] Crear dashboard con estadísticas
- [ ] Crear módulo de templates
- [ ] Crear módulo de configuración

### Testing
- [ ] Tests unitarios para servicios
- [ ] Tests de integración para componentes
- [ ] Tests E2E para flujos completos

### Seguridad
- [ ] Validar tokens en todas las peticiones
- [ ] Implementar refresh token (opcional)
- [ ] Sanitizar inputs del usuario
- [ ] Validar permisos en frontend

---

## ?? PRÓXIMOS PASOS

### Inmediatos
1. **Copiar modelos TypeScript** a tu proyecto
2. **Implementar servicios** uno por uno
3. **Crear componentes básicos** para cada módulo
4. **Configurar rutas** con guards de autenticación

### Optimizaciones
5. Implementar **caché local** con `BehaviorSubject`
6. Agregar **loading states** y **skeleton loaders**
7. Implementar **notificaciones toast** para errores
8. Crear **componentes reutilizables** (tablas, formularios)

### Avanzado
9. Implementar **PWA** con Service Workers
10. Agregar **gráficos interactivos** (ApexCharts, Chart.js)
11. Implementar **drag & drop** para templates
12. Agregar **modo offline** con IndexedDB

---

## RECURSOS

### Endpoints Base
- **Desarrollo:** `http://localhost:5291/api`
- **Swagger:** `http://localhost:5291/swagger`

### Autenticación
- **Tipo:** JWT Bearer Token
- **Header:** `Authorization: Bearer {token}`
- **Expiración:** 60 minutos
- **Storage:** LocalStorage

### Códigos de Estado
- `200` - OK
- `201` - Created
- `400` - Bad Request (validación)
- `401` - Unauthorized (sin token o expirado)
- `403` - Forbidden (sin permisos)
- `404` - Not Found
- `409` - Conflict (duplicado)
- `500` - Internal Server Error

---

## ? RESUMEN

**?? Total de servicios:** 5 nuevos  
**?? Total de endpoints:** 41 nuevos  
**?? Autenticación:** JWT obligatorio  
**?? Modelos TypeScript:** 15 interfaces  
**?? Componentes sugeridos:** 10+ nuevos

**Estado:** ? **LISTO PARA INTEGRAR**

---

**Fecha de generación:** 2025-01-19  
**Versión API:** v2.0  
**Compatibilidad:** Angular 15+ / .NET 8


