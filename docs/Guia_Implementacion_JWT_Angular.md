# Guía Completa: Implementación JWT en Frontend Angular

## ?? Resumen Ejecutivo

Tu backend **HoneyBack** (.NET 8) ya está configurado con JWT. Esta guía adapta tu proyecto Angular existente (`honeybalance-angular`) para integrar autenticación JWT sin romper lo que ya funciona.

### Backend Configurado
- ? Endpoint: `POST http://localhost:5291/api/auth/login`
- ? Endpoint: `GET http://localhost:5291/api/auth/me`
- ? JWT configurado con:
  - Issuer: "HoneyBack"
  - Audience: "HoneyBack"
  - Key: "Johan3208365126*-SecureKey-2024-HoneyBalance!"
  - Expires: 60 minutos

### Respuesta del Backend al Login
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-11-18T10:00:00Z",
  "user": {
    "id": 1,
    "nombreCompleto": "Juan Pérez",
    "email": "juan@example.com"
  }
}
```

---

## ?? Objetivos

1. ? Mantener servicios existentes (`UsuariosService`, `ReportesService`, etc.)
2. ? Agregar autenticación JWT
3. ? Proteger rutas con guards
4. ? Interceptar peticiones HTTP para agregar tokens
5. ? Manejar errores 401 automáticamente

---

## ?? Estructura a Crear/Modificar

```
C:\Users\johan\Desktop\HONEY\honeybalance-angular\src\app\
??? core/
?   ??? models/
?   ?   ??? auth.models.ts                    ? NUEVO
?   ??? services/
?   ?   ??? auth.service.ts                   ? NUEVO
?   ?   ??? token.service.ts                  ? NUEVO
?   ?   ??? usuarios.service.ts               ? EXISTENTE (mantener)
?   ?   ??? reportes.service.ts               ? EXISTENTE (mantener)
?   ?   ??? sesiones.service.ts               ? EXISTENTE (mantener)
?   ?   ??? mensajes-contacto.service.ts      ? EXISTENTE (mantener)
?   ??? interceptors/
?   ?   ??? auth.interceptor.ts               ? NUEVO
?   ?   ??? error.interceptor.ts              ? NUEVO
?   ??? guards/
?       ??? auth.guard.ts                     ? NUEVO
?       ??? no-auth.guard.ts                  ? NUEVO
??? features/
?   ??? auth/
?       ??? login/
?           ??? login.component.ts            ? NUEVO
?           ??? login.component.html          ? NUEVO
?           ??? login.component.css           ? NUEVO
??? app.module.ts                             ? MODIFICAR
??? app-routing.module.ts                     ? MODIFICAR
??? environments/
    ??? environment.ts                        ? YA EXISTE (verificar)
```

---

## ?? Paso 1: Instalar Dependencias

```bash
cd C:\Users\johan\Desktop\HONEY\honeybalance-angular
npm install jwt-decode
```

---

## ?? Paso 2: Crear Modelos de Autenticación

### Archivo: `src/app/core/models/auth.models.ts`

```typescript
// Modelos específicos para autenticación JWT

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  user: {
    id: number;
    nombreCompleto: string;
    email: string;
  };
}

export interface RegisterRequest {
  nombreCompleto: string;
  email: string;
  passwordHash: string; // Backend espera 'passwordHash', no 'password'
}

export interface JwtPayload {
  sub: string;           // UsuarioId
  email: string;
  name: string;          // NombreCompleto
  exp: number;
  iss: string;
  aud: string;
}
```

---

## ?? Paso 3: Servicio de Tokens

### Archivo: `src/app/core/services/token.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { jwtDecode } from 'jwt-decode';
import { JwtPayload } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class TokenService {
  private readonly TOKEN_KEY = 'honeybalance_token';
  private readonly EXPIRATION_KEY = 'honeybalance_token_exp';
  private readonly USER_KEY = 'honeybalance_user';

  constructor() {}

  // Guardar token y datos del usuario
  setToken(token: string, expiresAt: string, user: any): void {
    localStorage.setItem(this.TOKEN_KEY, token);
    localStorage.setItem(this.EXPIRATION_KEY, expiresAt);
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }

  // Obtener token
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  // Eliminar token y datos del usuario
  removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.EXPIRATION_KEY);
    localStorage.removeItem(this.USER_KEY);
  }

  // Verificar si el token existe
  hasToken(): boolean {
    return !!this.getToken();
  }

  // Decodificar token
  decodeToken(): JwtPayload | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      return jwtDecode<JwtPayload>(token);
    } catch (error) {
      console.error('Error al decodificar token:', error);
      return null;
    }
  }

  // Verificar si el token está expirado
  isTokenExpired(): boolean {
    const expirationDate = localStorage.getItem(this.EXPIRATION_KEY);
    if (!expirationDate) return true;

    const now = new Date();
    const expDate = new Date(expirationDate);
    return now >= expDate;
  }

  // Obtener información del usuario guardada
  getUserInfo(): any {
    const userStr = localStorage.getItem(this.USER_KEY);
    if (!userStr) return null;

    try {
      return JSON.parse(userStr);
    } catch {
      return null;
    }
  }

  // Verificar si el token es válido
  isTokenValid(): boolean {
    return this.hasToken() && !this.isTokenExpired();
  }
}
```

---

## ?? Paso 4: Servicio de Autenticación

### Archivo: `src/app/core/services/auth.service.ts`

```typescript
import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { TokenService } from './token.service';
import { LoginRequest, LoginResponse, RegisterRequest } from '../models/auth.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = `${environment.apiBase}/auth`;
  
  // BehaviorSubject para manejar el estado de autenticación
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(
    this.tokenService.isTokenValid()
  );
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  // BehaviorSubject para información del usuario
  private currentUserSubject = new BehaviorSubject<any>(
    this.tokenService.getUserInfo()
  );
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private tokenService: TokenService,
    private router: Router
  ) {
    // Verificar token al iniciar
    this.checkTokenValidity();
  }

  /**
   * Login de usuario
   */
  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.API_URL}/login`, credentials)
      .pipe(
        tap(response => {
          // Guardar token y usuario
          this.tokenService.setToken(response.token, response.expiresAt, response.user);
          
          // Actualizar estado
          this.isAuthenticatedSubject.next(true);
          this.currentUserSubject.next(response.user);
          
          console.log('Login exitoso:', response.user.nombreCompleto);
        }),
        catchError(this.handleError)
      );
  }

  /**
   * Registro de usuario (opcional, si implementas el endpoint en backend)
   */
  register(userData: RegisterRequest): Observable<any> {
    return this.http.post(`${environment.apiBase}/usuarios`, userData)
      .pipe(
        catchError(this.handleError)
      );
  }

  /**
   * Logout
   */
  logout(): void {
    this.tokenService.removeToken();
    this.isAuthenticatedSubject.next(false);
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
    console.log('Sesión cerrada');
  }

  /**
   * Verificar si el usuario está autenticado
   */
  isAuthenticated(): boolean {
    return this.tokenService.isTokenValid();
  }

  /**
   * Obtener información del usuario actual
   */
  getCurrentUser(): any {
    return this.tokenService.getUserInfo();
  }

  /**
   * Obtener ID del usuario actual
   */
  getCurrentUserId(): number | null {
    const userInfo = this.tokenService.getUserInfo();
    return userInfo ? userInfo.id : null;
  }

  /**
   * Verificar validez del token periódicamente
   */
  private checkTokenValidity(): void {
    if (!this.tokenService.isTokenValid()) {
      this.logout();
    }
  }

  /**
   * Refrescar estado de autenticación
   */
  refreshAuthState(): void {
    const isValid = this.tokenService.isTokenValid();
    this.isAuthenticatedSubject.next(isValid);
    
    if (isValid) {
      this.currentUserSubject.next(this.tokenService.getUserInfo());
    } else {
      this.currentUserSubject.next(null);
    }
  }

  /**
   * Obtener información completa del usuario desde el backend
   */
  getMe(): Observable<any> {
    return this.http.get(`${this.API_URL}/me`).pipe(
      catchError(this.handleError)
    );
  }

  /**
   * Manejo de errores HTTP
   */
  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'Ocurrió un error desconocido';

    if (error.error instanceof ErrorEvent) {
      // Error del lado del cliente
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Error del lado del servidor
      if (error.status === 401) {
        errorMessage = 'Credenciales inválidas';
      } else if (error.status === 400) {
        errorMessage = error.error?.mensaje || 'Datos inválidos';
      } else if (error.status === 404) {
        errorMessage = error.error?.mensaje || 'Usuario no encontrado';
      } else if (error.status === 500) {
        errorMessage = 'Error en el servidor. Intente más tarde';
      } else {
        errorMessage = error.error?.mensaje || `Error ${error.status}`;
      }
    }

    console.error('Error en AuthService:', errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}
```

---

## ?? Paso 5: Interceptores HTTP

### Archivo: `src/app/core/interceptors/auth.interceptor.ts`

```typescript
import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { TokenService } from '../services/token.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  constructor(private tokenService: TokenService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Obtener token
    const token = this.tokenService.getToken();

    // Si existe token y la petición es a la API, agregarlo
    if (token && request.url.includes('localhost:5291')) {
      request = request.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }

    return next.handle(request);
  }
}
```

### Archivo: `src/app/core/interceptors/error.interceptor.ts`

```typescript
import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { TokenService } from '../services/token.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(
    private tokenService: TokenService,
    private router: Router
  ) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401) {
          // Token inválido o expirado
          console.warn('Token inválido o expirado. Cerrando sesión...');
          this.tokenService.removeToken();
          this.router.navigate(['/login']);
        }

        return throwError(() => error);
      })
    );
  }
}
```

---

## ?? Paso 6: Guards de Rutas

### Archivo: `src/app/core/guards/auth.guard.ts`

```typescript
import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {
    
    if (this.authService.isAuthenticated()) {
      return true;
    }

    // Redirigir a login si no está autenticado
    console.warn('Acceso denegado. Redirigiendo a login...');
    this.router.navigate(['/login'], {
      queryParams: { returnUrl: state.url }
    });
    return false;
  }
}
```

### Archivo: `src/app/core/guards/no-auth.guard.ts`

```typescript
import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class NoAuthGuard implements CanActivate {

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(): boolean {
    if (!this.authService.isAuthenticated()) {
      return true;
    }

    // Si ya está autenticado, redirigir al dashboard/home
    console.warn('Ya está autenticado. Redirigiendo...');
    this.router.navigate(['/']); // Ajusta la ruta según tu app
    return false;
  }
}
```

---

## ?? Paso 7: Componente de Login

### Archivo: `src/app/features/auth/login/login.component.ts`

```typescript
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  loading = false;
  error = '';
  returnUrl = '/';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Inicializar formulario
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(3)]]
    });

    // Obtener URL de retorno
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  get f() {
    return this.loginForm.controls;
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      return;
    }

    this.loading = true;
    this.error = '';

    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        console.log('Login exitoso, redirigiendo...');
        this.router.navigateByUrl(this.returnUrl);
      },
      error: (error) => {
        console.error('Error en login:', error);
        this.error = error.message || 'Error al iniciar sesión';
        this.loading = false;
      }
    });
  }
}
```

### Archivo: `src/app/features/auth/login/login.component.html`

```html
<div class="login-container">
  <div class="login-card">
    <h2>Iniciar Sesión</h2>
    <p class="subtitle">HoneyBalance</p>

    <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
      <!-- Email -->
      <div class="form-group">
        <label for="email">Email</label>
        <input
          type="email"
          id="email"
          formControlName="email"
          class="form-control"
          [class.is-invalid]="f['email'].invalid && f['email'].touched"
          placeholder="correo@ejemplo.com"
        />
        <div class="invalid-feedback" *ngIf="f['email'].invalid && f['email'].touched">
          <span *ngIf="f['email'].errors?.['required']">El email es requerido</span>
          <span *ngIf="f['email'].errors?.['email']">Email inválido</span>
        </div>
      </div>

      <!-- Password -->
      <div class="form-group">
        <label for="password">Contraseña</label>
        <input
          type="password"
          id="password"
          formControlName="password"
          class="form-control"
          [class.is-invalid]="f['password'].invalid && f['password'].touched"
          placeholder="********"
        />
        <div class="invalid-feedback" *ngIf="f['password'].invalid && f['password'].touched">
          <span *ngIf="f['password'].errors?.['required']">La contraseña es requerida</span>
          <span *ngIf="f['password'].errors?.['minlength']">Mínimo 3 caracteres</span>
        </div>
      </div>

      <!-- Error Message -->
      <div class="alert alert-danger" *ngIf="error">
        {{ error }}
      </div>

      <!-- Submit Button -->
      <button
        type="submit"
        class="btn btn-primary btn-block"
        [disabled]="loading || loginForm.invalid"
      >
        <span *ngIf="loading" class="spinner-border spinner-border-sm mr-1"></span>
        {{ loading ? 'Iniciando sesión...' : 'Iniciar Sesión' }}
      </button>
    </form>
  </div>
</div>
```

### Archivo: `src/app/features/auth/login/login.component.css`

```css
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: linear-gradient(135deg, #FFC107 0%, #FF9800 100%);
  padding: 20px;
}

.login-card {
  background: white;
  padding: 2.5rem;
  border-radius: 15px;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.15);
  width: 100%;
  max-width: 420px;
}

.login-card h2 {
  text-align: center;
  margin-bottom: 0.5rem;
  color: #333;
  font-weight: 600;
}

.subtitle {
  text-align: center;
  color: #FF9800;
  margin-bottom: 2rem;
  font-weight: 500;
}

.form-group {
  margin-bottom: 1.5rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #555;
  font-weight: 500;
}

.form-control {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-size: 1rem;
  transition: border-color 0.3s;
}

.form-control:focus {
  outline: none;
  border-color: #FF9800;
  box-shadow: 0 0 0 3px rgba(255, 152, 0, 0.1);
}

.form-control.is-invalid {
  border-color: #dc3545;
}

.invalid-feedback {
  color: #dc3545;
  font-size: 0.875rem;
  margin-top: 0.25rem;
  display: block;
}

.alert {
  padding: 0.75rem;
  margin-bottom: 1rem;
  border-radius: 8px;
}

.alert-danger {
  background-color: #f8d7da;
  border: 1px solid #f5c6cb;
  color: #721c24;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-primary {
  background: linear-gradient(135deg, #FF9800 0%, #FF6F00 100%);
  color: white;
  width: 100%;
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(255, 152, 0, 0.3);
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.spinner-border-sm {
  width: 1rem;
  height: 1rem;
  border-width: 0.2em;
}
```

---

## ?? Paso 8: Configurar app.module.ts

### Archivo: `src/app/app.module.ts`

```typescript
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { ReactiveFormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

// Interceptores
import { AuthInterceptor } from './core/interceptors/auth.interceptor';
import { ErrorInterceptor } from './core/interceptors/error.interceptor';

// Componentes
import { LoginComponent } from './features/auth/login/login.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    // ... tus componentes existentes
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    ReactiveFormsModule
  ],
  providers: [
    // Interceptor de autenticación (agrega Bearer token)
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    },
    // Interceptor de errores (maneja 401)
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ErrorInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
```

---

## ?? Paso 9: Configurar Rutas (app-routing.module.ts)

### Archivo: `src/app/app-routing.module.ts`

```typescript
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Guards
import { AuthGuard } from './core/guards/auth.guard';
import { NoAuthGuard } from './core/guards/no-auth.guard';

// Componentes
import { LoginComponent } from './features/auth/login/login.component';
// Importa tus componentes existentes aquí

const routes: Routes = [
  // Rutas públicas (sin autenticación)
  {
    path: 'login',
    component: LoginComponent,
    canActivate: [NoAuthGuard] // Si ya está autenticado, redirige
  },

  // Rutas protegidas (requieren autenticación)
  {
    path: '',
    canActivate: [AuthGuard], // Protege todas las rutas hijas
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      // Agrega tus rutas existentes aquí:
      // { path: 'dashboard', component: DashboardComponent },
      // { path: 'reportes', component: ReportesComponent },
      // etc.
    ]
  },

  // Ruta wildcard
  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
```

---

## ?? Paso 10: Verificar environment.ts

### Archivo: `src/environments/environment.ts`

```typescript
export const environment = {
  production: false,
  apiBase: 'http://localhost:5291/api'
};
```

---

## ?? Paso 11: Uso en Componentes

### Ejemplo: Mostrar usuario actual en navbar

```typescript
import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  template: `
    <nav>
      <div class="user-info" *ngIf="currentUser">
        <span>Hola, {{ currentUser.nombreCompleto }}</span>
        <button (click)="logout()">Cerrar Sesión</button>
      </div>
    </nav>
  `
})
export class NavbarComponent implements OnInit {
  currentUser: any = null;

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    // Suscribirse al usuario actual
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
  }

  logout(): void {
    this.authService.logout();
  }
}
```

### Ejemplo: Crear reporte con usuario autenticado

```typescript
import { Component } from '@angular/core';
import { ReportesService } from '../../core/services/reportes.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-crear-reporte',
  template: `...`
})
export class CrearReporteComponent {
  
  constructor(
    private reportesService: ReportesService,
    private authService: AuthService
  ) {}

  crearReporte(): void {
    const usuarioId = this.authService.getCurrentUserId();
    
    const nuevoReporte = {
      nombre: 'Mi Reporte',
      tipoReporte: 'Mensual',
      descripcion: 'Descripción del reporte',
      usuarioId: usuarioId
    };

    this.reportesService.crear(nuevoReporte).subscribe({
      next: (reporte) => {
        console.log('Reporte creado:', reporte);
      },
      error: (error) => {
        console.error('Error:', error);
      }
    });
  }
}
```

---

## ?? Paso 12: Pruebas

### 1. Crear usuario de prueba en la base de datos

```sql
INSERT INTO Usuarios (NombreCompleto, Email, PasswordHash, FechaRegistro)
VALUES ('Test User', 'test@example.com', 'password123', GETDATE());
```

### 2. Probar login

```
http://localhost:4200/login

Email: test@example.com
Password: password123
```

### 3. Verificar token en DevTools

Abre la consola del navegador (F12) y ejecuta:

```javascript
localStorage.getItem('honeybalance_token')
```

### 4. Verificar que el token se agrega a las peticiones

1. Abre DevTools (F12)
2. Ve a la pestaña "Network"
3. Realiza una petición a la API
4. Verifica que en "Request Headers" aparezca:
   ```
   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```

---

## ?? Endpoints Protegidos vs Públicos

### Públicos (sin token)
- `POST /api/auth/login`
- `GET /api/reportes` (todos)
- `GET /api/reportes/{id}`
- `POST /api/reportes` (crear sin auth)

### Protegidos (requieren Bearer token)
- `GET /api/auth/me`
- `PUT /api/reportes/{id}`
- `DELETE /api/reportes/{id}`

---

## ?? Flujo Completo de Autenticación

1. **Usuario ingresa credenciales** en el formulario de login
2. **LoginComponent** llama a `AuthService.login()`
3. **AuthService** envía `POST /api/auth/login` al backend
4. **Backend** valida credenciales y retorna `{ token, expiresAt, user }`
5. **TokenService** guarda token en `localStorage`
6. **AuthService** actualiza `isAuthenticatedSubject` y `currentUserSubject`
7. **Usuario es redirigido** a la ruta protegida
8. **AuthInterceptor** intercepta todas las peticiones HTTP y agrega `Authorization: Bearer {token}`
9. **Backend** valida el token JWT en cada petición
10. Si el token es válido, el backend responde con los datos solicitados
11. Si el token es inválido o expiró, el backend retorna `401 Unauthorized`
12. **ErrorInterceptor** captura el error 401 y cierra sesión automáticamente

---

## ?? Notas Importantes

### 1. Backend usa password en texto plano (temporalmente)
El `AuthController` compara `user.PasswordHash == request.Password` directamente. **Migra a BCrypt en producción**.

### 2. CORS ya configurado
Tu backend permite `http://localhost:4200` y `http://localhost:4201` en el archivo `Program.cs`.

### 3. Token expira en 60 minutos
Configura un sistema de refresh token si necesitas sesiones más largas.

### 4. Servicios existentes NO necesitan modificación
El interceptor agrega automáticamente el token a todas las peticiones. Tus servicios existentes (`UsuariosService`, `ReportesService`, etc.) funcionarán sin cambios.

### 5. LocalStorage vs SessionStorage
Esta implementación usa `localStorage` para persistir la sesión entre pestañas. Si prefieres que la sesión expire al cerrar el navegador, cambia `localStorage` por `sessionStorage` en `TokenService`.

---

## ? Checklist de Implementación

- [ ] Instalar `jwt-decode` con npm
- [ ] Crear `auth.models.ts`
- [ ] Crear `token.service.ts`
- [ ] Crear `auth.service.ts`
- [ ] Crear `auth.interceptor.ts`
- [ ] Crear `error.interceptor.ts`
- [ ] Crear `auth.guard.ts`
- [ ] Crear `no-auth.guard.ts`
- [ ] Crear carpeta `features/auth/login/`
- [ ] Crear `login.component.ts`
- [ ] Crear `login.component.html`
- [ ] Crear `login.component.css`
- [ ] Configurar interceptores en `app.module.ts`
- [ ] Configurar rutas con guards en `app-routing.module.ts`
- [ ] Verificar `environment.ts` tiene `apiBase`
- [ ] Crear usuario de prueba en BD
- [ ] Probar login
- [ ] Probar rutas protegidas
- [ ] Probar logout
- [ ] Verificar token en peticiones HTTP (DevTools > Network)
- [ ] Agregar botón de logout en navbar (opcional)

---

## ?? Comandos de Ejecución

```bash
# Terminal 1: Backend
cd C:\Users\johan\source\repos\HoneyBack
dotnet run

# Terminal 2: Frontend
cd C:\Users\johan\Desktop\HONEY\honeybalance-angular
ng serve

# Abrir navegador
http://localhost:4200
```

---

## ?? Próximos Pasos (Opcional)

1. **Implementar Registro de Usuarios**
   - Crear `register.component.ts/html/css`
   - Endpoint ya existe: `POST /api/usuarios`

2. **Implementar Refresh Token**
   - Agregar endpoint en backend para renovar tokens
   - Interceptar 401 y reintentar con nuevo token

3. **Migrar a BCrypt en Backend**
   - Instalar `BCrypt.Net-Next` en backend
   - Modificar `AuthController` para usar hashing seguro

4. **Agregar Roles/Permisos**
   - Extender JWT con claims de roles
   - Crear guards específicos por rol

5. **Implementar "Recordarme"**
   - Agregar checkbox en login
   - Extender duración del token o usar refresh token

---

## ?? Recursos Adicionales

- [JWT.io - Decodificar tokens](https://jwt.io/)
- [Angular HttpClient](https://angular.io/guide/http)
- [Angular Router Guards](https://angular.io/guide/router#preventing-unauthorized-access)
- [RxJS BehaviorSubject](https://rxjs.dev/api/index/class/BehaviorSubject)

---

## ?? Solución de Problemas

### Error: "Cannot find module 'jwt-decode'"
```bash
npm install jwt-decode
```

### Error: CORS en el navegador
Verifica que el backend esté configurado con CORS para `http://localhost:4200` en `Program.cs`.

### Token no se agrega a las peticiones
Verifica que `AuthInterceptor` esté registrado en `app.module.ts` y que la URL de la petición incluya `localhost:5291`.

### Login exitoso pero redirige a login nuevamente
Verifica que `AuthGuard` esté usando `this.authService.isAuthenticated()` correctamente y que el token no esté expirado.

### Usuario no puede acceder a rutas protegidas después de login
Abre DevTools > Application > Local Storage y verifica que existan:
- `honeybalance_token`
- `honeybalance_token_exp`
- `honeybalance_user`

---

¡Listo! Tu frontend Angular ahora está completamente integrado con autenticación JWT del backend HoneyBack. ??
