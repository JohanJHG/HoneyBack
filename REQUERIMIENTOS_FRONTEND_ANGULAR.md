# ?? REQUERIMIENTOS FRONTEND ANGULAR - HONEYBACK

## ?? OBJETIVO GENERAL
Implementar en el frontend Angular todas las funcionalidades de autenticación y gestión que ya están completamente funcionales en el backend HoneyBack API (.NET 8).

---

## ?? ESTADO ACTUAL DEL BACKEND

### ? Endpoints Disponibles y Funcionando

#### ?? Autenticación (AuthController)
| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| POST | `/api/auth/login` | Login con email y password | Público |
| POST | `/api/auth/register` | Registro de nuevos usuarios | Público |
| GET | `/api/auth/me` | Obtener usuario autenticado actual | ?? Bearer |
| POST | `/api/auth/cambiar-password` | Cambiar contraseña | ?? Bearer |
| POST | `/api/auth/logout` | Cerrar sesión e invalidar token | ?? Bearer |

#### ?? Usuarios (UsuariosController)
| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/usuarios` | Lista de usuarios (sin PasswordHash) | ?? Bearer |
| GET | `/api/usuarios/{id}` | Usuario por ID | ?? Bearer |
| GET | `/api/usuarios/email/{email}` | Usuario por email | ?? Bearer |
| PUT | `/api/usuarios/{id}` | Actualizar usuario | ?? Bearer |
| DELETE | `/api/usuarios/{id}` | Eliminar usuario | ?? Bearer |

#### ?? Sesiones (SesionesController)
| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/sesiones` | Lista de sesiones | ?? Bearer |
| GET | `/api/sesiones/{id}` | Sesión por ID | ?? Bearer |
| GET | `/api/sesiones/usuario/{usuarioId}` | Sesiones por usuario | ?? Bearer |
| POST | `/api/sesiones/validar` | Validar token de sesión | Público |
| POST | `/api/sesiones/limpiar-expiradas` | Limpiar sesiones vencidas | Público |
| DELETE | `/api/sesiones/{id}` | Eliminar sesión | ?? Bearer |

#### ?? Reportes (ReportesController)
| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/reportes` | Lista de reportes | Público |
| GET | `/api/reportes/{id}` | Reporte por ID | Público |
| GET | `/api/reportes/usuario/{usuarioId}` | Reportes por usuario | Público |
| GET | `/api/reportes/estado/{estado}` | Reportes por estado | Público |
| POST | `/api/reportes` | Crear reporte | Público |
| PUT | `/api/reportes/{id}` | Actualizar reporte | ?? Bearer |
| DELETE | `/api/reportes/{id}` | Eliminar reporte | ?? Bearer |

#### ?? Mensajes de Contacto (MensajesContactoController)
| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/mensajescontacto` | Lista de mensajes | Público |
| GET | `/api/mensajescontacto/{id}` | Mensaje por ID | Público |
| POST | `/api/mensajescontacto` | Crear mensaje de contacto | Público |
| DELETE | `/api/mensajescontacto/{id}` | Eliminar mensaje | Público |

---

## ?? CONFIGURACIÓN DEL BACKEND

### Autenticación JWT
- **Esquema:** Bearer Token
- **Formato:** JWT (JSON Web Token)
- **Expiración:** 60 minutos
- **Header requerido:** `Authorization: Bearer {token}`

### CORS
- **Orígenes permitidos:** 
  - `http://localhost:4200`
  - `http://localhost:4201`
- **Headers:** Todos permitidos
- **Métodos:** Todos permitidos
- **Credentials:** Habilitadas

### Estructura de Respuestas

#### Login Response
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-01-19T15:30:00.000Z",
  "usuario": {
    "usuarioId": 1,
    "nombreCompleto": "Juan Pérez",
    "email": "juan@example.com",
    "fechaRegistro": "2025-01-01T00:00:00.000Z"
  }
}
```

#### Register Response
```json
{
  "usuarioId": 1,
  "nombreCompleto": "Juan Pérez",
  "email": "juan@example.com",
  "fechaRegistro": "2025-01-19T14:30:00.000Z"
}
```

#### Usuario Response (GET /api/auth/me)
```json
{
  "usuarioId": 1,
  "nombreCompleto": "Juan Pérez",
  "email": "juan@example.com",
  "fechaRegistro": "2025-01-01T00:00:00.000Z"
}
```

---

## ?? REQUERIMIENTOS FUNCIONALES

### 1. SERVICIOS (Services)

#### 1.1 AuthService (`src/app/services/auth.service.ts`)

**Responsabilidades:**
- Gestión de autenticación y sesión
- Almacenamiento y recuperación de tokens
- Comunicación con endpoints de auth del backend

**Métodos requeridos:**

```typescript
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  usuario: Usuario;
}

export interface RegisterRequest {
  nombreCompleto: string;
  email: string;
  password: string;
}

export interface RegisterResponse {
  usuarioId: number;
  nombreCompleto: string;
  email: string;
  fechaRegistro: string;
}

export interface Usuario {
  usuarioId: number;
  nombreCompleto: string;
  email: string;
  fechaRegistro: string;
}

export interface ChangePasswordRequest {
  passwordActual: string;
  passwordNueva: string;
}

export class AuthService {
  // Autenticación
  login(request: LoginRequest): Observable<LoginResponse>;
  register(request: RegisterRequest): Observable<RegisterResponse>;
  logout(): Observable<void>;
  
  // Información de usuario
  getCurrentUser(): Observable<Usuario>;
  refreshUserData(): Observable<Usuario>;
  
  // Cambio de contraseña
  changePassword(request: ChangePasswordRequest): Observable<void>;
  
  // Estado de autenticación
  isAuthenticated(): boolean;
  isTokenExpired(): boolean;
  
  // Gestión de tokens
  getToken(): string | null;
  getTokenExpirationDate(): Date | null;
  setToken(token: string, expiresAt: string): void;
  removeToken(): void;
  
  // Usuario almacenado
  getStoredUser(): Usuario | null;
  setStoredUser(user: Usuario): void;
  removeStoredUser(): void;
}
```

**Almacenamiento en LocalStorage:**
- `honeyback_token`: JWT token
- `honeyback_user`: Usuario serializado
- `honeyback_token_expiry`: Fecha de expiración ISO 8601

---

#### 1.2 ApiService (`src/app/services/api.service.ts`)

**Responsabilidades:**
- Cliente HTTP base para todas las peticiones
- Gestión de URL base
- Transformación de respuestas

**Métodos requeridos:**

```typescript
export class ApiService {
  private baseUrl = 'http://localhost:5291/api';
  
  // Métodos HTTP genéricos
  get<T>(endpoint: string, options?: any): Observable<T>;
  post<T>(endpoint: string, body: any, options?: any): Observable<T>;
  put<T>(endpoint: string, body: any, options?: any): Observable<T>;
  delete<T>(endpoint: string, options?: any): Observable<T>;
  
  // URLs de endpoints
  getAuthUrl(): string; // /api/auth
  getUsuariosUrl(): string; // /api/usuarios
  getReportesUrl(): string; // /api/reportes
  getSesionesUrl(): string; // /api/sesiones
  getMensajesContactoUrl(): string; // /api/mensajescontacto
}
```

---

#### 1.3 NotificationService (`src/app/services/notification.service.ts`)

**Responsabilidades:**
- Mostrar notificaciones/toasts al usuario
- Gestión de mensajes de éxito, error, warning e info

**Métodos requeridos:**

```typescript
export class NotificationService {
  success(message: string, title?: string): void;
  error(message: string, title?: string): void;
  warning(message: string, title?: string): void;
  info(message: string, title?: string): void;
}
```

**Librería recomendada:** `ngx-toastr` o Angular Material Snackbar

---

### 2. INTERCEPTORES (Interceptors)

#### 2.1 AuthInterceptor (`src/app/interceptors/auth.interceptor.ts`)

**Responsabilidades:**
- Agregar automáticamente el token Bearer a las peticiones
- Excluir endpoints públicos
- Manejar errores de autenticación

**Funcionalidad:**

```typescript
export class AuthInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // 1. Obtener token del AuthService
    const token = this.authService.getToken();
    
    // 2. Endpoints públicos (no agregar token)
    const publicEndpoints = [
      '/api/auth/login',
      '/api/auth/register',
      '/api/sesiones/validar'
    ];
    
    // 3. Si tiene token y no es endpoint público, agregar header
    if (token && !this.isPublicEndpoint(req.url, publicEndpoints)) {
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
    
    // 4. Continuar con la petición
    return next.handle(req).pipe(
      catchError(error => {
        // Manejar errores de autenticación
        if (error.status === 401) {
          // Token inválido o expirado
          this.authService.logout();
          this.router.navigate(['/login']);
        }
        return throwError(() => error);
      })
    );
  }
}
```

---

### 3. GUARDS (Route Guards)

#### 3.1 AuthGuard (`src/app/guards/auth.guard.ts`)

**Responsabilidades:**
- Proteger rutas que requieren autenticación
- Redirigir a login si no está autenticado
- Guardar URL original para redirección post-login

**Funcionalidad:**

```typescript
export class AuthGuard implements CanActivate {
  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean | UrlTree | Observable<boolean | UrlTree> {
    
    // 1. Verificar si está autenticado
    if (this.authService.isAuthenticated() && !this.authService.isTokenExpired()) {
      return true;
    }
    
    // 2. No autenticado: guardar URL y redirigir a login
    this.authService.removeToken();
    this.authService.removeStoredUser();
    
    // Guardar URL original
    const returnUrl = state.url;
    this.router.navigate(['/login'], { 
      queryParams: { returnUrl } 
    });
    
    return false;
  }
}
```

---

### 4. COMPONENTES (Components)

#### 4.1 LoginComponent (`src/app/components/auth/login/`)

**Template:** `login.component.html`
**Styles:** `login.component.scss`
**Logic:** `login.component.ts`

**Requerimientos:**

**Formulario Reactivo:**
```typescript
loginForm = this.fb.group({
  email: ['', [Validators.required, Validators.email]],
  password: ['', [Validators.required, Validators.minLength(6)]]
});
```

**Funcionalidades:**
- Validación en tiempo real
- Mostrar mensajes de error de validación
- Deshabilitar botón mientras se procesa
- Mostrar spinner/loading durante login
- Link a registro
- Link a "¿Olvidaste tu contraseña?"

**Flujo de Login:**
```typescript
onSubmit(): void {
  if (this.loginForm.invalid) return;
  
  this.loading = true;
  this.authService.login(this.loginForm.value).subscribe({
    next: (response) => {
      // Guardar token y usuario
      this.authService.setToken(response.token, response.expiresAt);
      this.authService.setStoredUser(response.usuario);
      
      // Notificar éxito
      this.notificationService.success(
        `Bienvenido, ${response.usuario.nombreCompleto}!`
      );
      
      // Redirigir
      const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
      this.router.navigate([returnUrl]);
    },
    error: (error) => {
      this.loading = false;
      if (error.status === 401) {
        this.notificationService.error('Credenciales inválidas');
      } else {
        this.notificationService.error('Error al iniciar sesión');
      }
    }
  });
}
```

**Validaciones de UI:**
- Email inválido ? "Ingrese un email válido"
- Email requerido ? "El email es requerido"
- Password requerido ? "La contraseña es requerida"
- Password muy corta ? "La contraseña debe tener al menos 6 caracteres"

---

#### 4.2 RegisterComponent (`src/app/components/auth/register/`)

**Requerimientos:**

**Formulario Reactivo:**
```typescript
registerForm = this.fb.group({
  nombreCompleto: ['', [Validators.required, Validators.minLength(3)]],
  email: ['', [Validators.required, Validators.email]],
  password: ['', [Validators.required, Validators.minLength(6)]],
  confirmPassword: ['', [Validators.required]]
}, { validators: this.passwordMatchValidator });
```

**Validador Personalizado:**
```typescript
passwordMatchValidator(form: AbstractControl): ValidationErrors | null {
  const password = form.get('password');
  const confirmPassword = form.get('confirmPassword');
  
  if (password?.value !== confirmPassword?.value) {
    return { passwordMismatch: true };
  }
  return null;
}
```

**Funcionalidades:**
- Validación de coincidencia de passwords
- Validación de fortaleza de password (opcional)
- Verificar disponibilidad de email (async validator)
- Link a login
- Mostrar términos y condiciones

**Flujo de Registro:**
```typescript
onSubmit(): void {
  if (this.registerForm.invalid) return;
  
  this.loading = true;
  const { confirmPassword, ...registerData } = this.registerForm.value;
  
  this.authService.register(registerData).subscribe({
    next: (response) => {
      this.notificationService.success(
        'Registro exitoso. Por favor inicia sesión.'
      );
      
      // Opción 1: Login automático
      this.autoLogin(registerData.email, registerData.password);
      
      // Opción 2: Redirigir a login
      // this.router.navigate(['/login']);
    },
    error: (error) => {
      this.loading = false;
      if (error.status === 409) {
        this.notificationService.error('El email ya está registrado');
      } else {
        this.notificationService.error('Error al registrar usuario');
      }
    }
  });
}
```

---

#### 4.3 ProfileComponent (`src/app/components/profile/`)

**Requerimientos:**

**Funcionalidades:**
- Mostrar información del usuario actual
- Editar nombre completo y email
- Link/botón para cambiar contraseña
- Mostrar fecha de registro
- Ver sesiones activas (opcional)

**Obtener Usuario:**
```typescript
ngOnInit(): void {
  this.authService.getCurrentUser().subscribe({
    next: (user) => {
      this.user = user;
      this.loadProfileForm();
    },
    error: (error) => {
      this.notificationService.error('Error al cargar perfil');
      this.router.navigate(['/login']);
    }
  });
}
```

---

#### 4.4 ChangePasswordComponent (`src/app/components/auth/change-password/`)

**?? NUEVO - No existe actualmente**

**Requerimientos:**

**Formulario Reactivo:**
```typescript
changePasswordForm = this.fb.group({
  passwordActual: ['', [Validators.required]],
  passwordNueva: ['', [Validators.required, Validators.minLength(6)]],
  confirmPasswordNueva: ['', [Validators.required]]
}, { validators: this.passwordMatchValidator });
```

**Funcionalidades:**
- Validar que la nueva password es diferente de la actual
- Validar coincidencia de confirmación
- Mostrar/ocultar passwords (toggle visibility)
- Indicador de fortaleza de password

**Flujo de Cambio:**
```typescript
onSubmit(): void {
  if (this.changePasswordForm.invalid) return;
  
  this.loading = true;
  const request: ChangePasswordRequest = {
    passwordActual: this.changePasswordForm.value.passwordActual!,
    passwordNueva: this.changePasswordForm.value.passwordNueva!
  };
  
  this.authService.changePassword(request).subscribe({
    next: () => {
      this.notificationService.success('Contraseña actualizada exitosamente');
      
      // Opcional: Cerrar sesión y pedir re-login
      this.authService.logout().subscribe(() => {
        this.router.navigate(['/login']);
      });
    },
    error: (error) => {
      this.loading = false;
      if (error.status === 400) {
        this.notificationService.error('La contraseña actual es incorrecta');
      } else {
        this.notificationService.error('Error al cambiar contraseña');
      }
    }
  });
}
```

---

#### 4.5 ForgotPasswordComponent (`src/app/components/auth/forgot-password/`)

**?? NUEVO - No existe en backend ni frontend**

**Requerimientos:**

**Paso 1: Solicitar Recuperación**

**Formulario:**
```typescript
forgotPasswordForm = this.fb.group({
  email: ['', [Validators.required, Validators.email]]
});
```

**Endpoint a crear en backend:**
```
POST /api/auth/forgot-password
Body: { "email": "usuario@example.com" }
Response: { "message": "Si el email existe, recibirás un código de recuperación" }
```

**Flujo:**
```typescript
onSubmit(): void {
  if (this.forgotPasswordForm.invalid) return;
  
  this.loading = true;
  this.authService.forgotPassword(this.forgotPasswordForm.value.email).subscribe({
    next: (response) => {
      this.notificationService.info(response.message);
      this.showTokenInput = true; // Mostrar campo para ingresar token
    },
    error: () => {
      this.loading = false;
      this.notificationService.error('Error al procesar solicitud');
    }
  });
}
```

---

#### 4.6 ResetPasswordComponent (`src/app/components/auth/reset-password/`)

**?? NUEVO - No existe en backend ni frontend**

**Requerimientos:**

**Paso 2: Restablecer con Token**

**Formulario:**
```typescript
resetPasswordForm = this.fb.group({
  token: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
  newPassword: ['', [Validators.required, Validators.minLength(6)]],
  confirmPassword: ['', [Validators.required]]
}, { validators: this.passwordMatchValidator });
```

**Endpoint a crear en backend:**
```
POST /api/auth/reset-password
Body: { "token": "123456", "newPassword": "newpassword123" }
Response: { "message": "Contraseña restablecida exitosamente" }
```

**Flujo:**
```typescript
onSubmit(): void {
  if (this.resetPasswordForm.invalid) return;
  
  this.loading = true;
  const request = {
    token: this.resetPasswordForm.value.token!,
    newPassword: this.resetPasswordForm.value.newPassword!
  };
  
  this.authService.resetPassword(request).subscribe({
    next: () => {
      this.notificationService.success('Contraseña restablecida exitosamente');
      this.router.navigate(['/login']);
    },
    error: (error) => {
      this.loading = false;
      if (error.status === 400) {
        this.notificationService.error('Token inválido o expirado');
      } else {
        this.notificationService.error('Error al restablecer contraseña');
      }
    }
  });
}
```

---

#### 4.7 NavbarComponent / HeaderComponent

**Requerimientos:**

**Funcionalidades:**
- Mostrar nombre del usuario actual
- Avatar con iniciales del nombre
- Menú dropdown con opciones:
  - Mi perfil
  - Cambiar contraseña
  - Cerrar sesión
- Indicador visual si está autenticado

**Logout:**
```typescript
onLogout(): void {
  this.authService.logout().subscribe({
    next: () => {
      this.notificationService.info('Sesión cerrada correctamente');
      this.router.navigate(['/login']);
    },
    error: () => {
      // Forzar logout local aunque falle el backend
      this.authService.removeToken();
      this.authService.removeStoredUser();
      this.router.navigate(['/login']);
    }
  });
}
```

---

### 5. CONFIGURACIÓN DE RUTAS

**Archivo:** `src/app/app.routes.ts`

```typescript
export const routes: Routes = [
  // Rutas públicas
  { 
    path: 'login', 
    component: LoginComponent,
    title: 'Iniciar Sesión - HoneyBack'
  },
  { 
    path: 'register', 
    component: RegisterComponent,
    title: 'Registrarse - HoneyBack'
  },
  { 
    path: 'forgot-password', 
    component: ForgotPasswordComponent,
    title: 'Recuperar Contraseña - HoneyBack'
  },
  { 
    path: 'reset-password/:token', 
    component: ResetPasswordComponent,
    title: 'Restablecer Contraseña - HoneyBack'
  },
  
  // Rutas protegidas
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [AuthGuard],
    title: 'Dashboard - HoneyBack'
  },
  {
    path: 'profile',
    component: ProfileComponent,
    canActivate: [AuthGuard],
    title: 'Mi Perfil - HoneyBack'
  },
  {
    path: 'change-password',
    component: ChangePasswordComponent,
    canActivate: [AuthGuard],
    title: 'Cambiar Contraseña - HoneyBack'
  },
  {
    path: 'usuarios',
    component: UsuariosListComponent,
    canActivate: [AuthGuard],
    title: 'Usuarios - HoneyBack'
  },
  {
    path: 'reportes',
    component: ReportesComponent,
    canActivate: [AuthGuard],
    title: 'Reportes - HoneyBack'
  },
  
  // Redirecciones
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: '**', redirectTo: 'dashboard' }
];
```

---

### 6. MODELOS/INTERFACES (Models)

**Archivo:** `src/app/models/auth.models.ts`

```typescript
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
  usuario: Usuario;
}

export interface RegisterRequest {
  nombreCompleto: string;
  email: string;
  password: string;
}

export interface RegisterResponse {
  usuarioId: number;
  nombreCompleto: string;
  email: string;
  fechaRegistro: string;
}

export interface Usuario {
  usuarioId: number;
  nombreCompleto: string;
  email: string;
  fechaRegistro: string;
}

export interface ChangePasswordRequest {
  passwordActual: string;
  passwordNueva: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}
```

---

### 7. VARIABLES DE ENTORNO

**Archivo:** `src/environments/environment.ts`

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5291/api',
  storage: {
    tokenKey: 'honeyback_token',
    userKey: 'honeyback_user',
    tokenExpiryKey: 'honeyback_token_expiry'
  }
};
```

**Archivo:** `src/environments/environment.prod.ts`

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://api.honeyback.com/api', // URL de producción
  storage: {
    tokenKey: 'honeyback_token',
    userKey: 'honeyback_user',
    tokenExpiryKey: 'honeyback_token_expiry'
  }
};
```

---

## ?? TAREAS BACKEND PENDIENTES

### Para Recuperación de Contraseña

#### 1. Crear Tabla PasswordResets

```sql
CREATE TABLE PasswordResets (
    ResetId BIGINT PRIMARY KEY IDENTITY(1,1),
    UsuarioId INT NOT NULL,
    Token NVARCHAR(100) NOT NULL UNIQUE,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    FechaExpiracion DATETIME NOT NULL,
    Usado BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(UsuarioID)
);

CREATE INDEX IX_PasswordResets_Token ON PasswordResets(Token);
CREATE INDEX IX_PasswordResets_UsuarioId ON PasswordResets(UsuarioId);
```

#### 2. Crear Modelo PasswordReset

```csharp
// Models/PasswordReset.cs
public class PasswordReset
{
    public long ResetId { get; set; }
    public int UsuarioId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaExpiracion { get; set; }
    public bool Usado { get; set; }
    
    public virtual Usuario Usuario { get; set; } = null!;
}
```

#### 3. Crear DTOs

```csharp
// DTOs/Auth/ForgotPasswordDto.cs
public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}

// DTOs/Auth/ResetPasswordDto.cs
public class ResetPasswordDto
{
    [Required]
    [StringLength(100)]
    public string Token { get; set; } = null!;
    
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;
}
```

#### 4. Agregar Endpoints en AuthController

```csharp
/// <summary>
/// Solicitar recuperación de contraseña
/// </summary>
[HttpPost("forgot-password")]
[AllowAnonymous]
public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
{
    // 1. Buscar usuario por email
    var usuario = await _usuariosService.ObtenerPorEmailAsync(request.Email);
    
    // Por seguridad, siempre devolver el mismo mensaje
    var message = "Si el email existe, recibirás un código de recuperación";
    
    if (usuario != null)
    {
        // 2. Generar token único (6 dígitos)
        var token = GenerateRandomToken();
        
        // 3. Guardar en tabla PasswordResets
        var passwordReset = new PasswordReset
        {
            UsuarioId = usuario.UsuarioId,
            Token = token,
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = DateTime.UtcNow.AddMinutes(15),
            Usado = false
        };
        await _passwordResetService.CrearAsync(passwordReset);
        
        // 4. Enviar email con token (o log en desarrollo)
        // await _emailService.SendPasswordResetEmail(usuario.Email, token);
        Console.WriteLine($"Token de recuperación para {usuario.Email}: {token}");
    }
    
    return Ok(new { message });
}

/// <summary>
/// Restablecer contraseña con token
/// </summary>
[HttpPost("reset-password")]
[AllowAnonymous]
public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto request)
{
    // 1. Buscar token en BD
    var passwordReset = await _passwordResetService.ObtenerPorTokenAsync(request.Token);
    
    if (passwordReset == null || passwordReset.Usado)
    {
        return BadRequest(new { message = "Token inválido" });
    }
    
    // 2. Verificar que no haya expirado
    if (DateTime.UtcNow > passwordReset.FechaExpiracion)
    {
        return BadRequest(new { message = "Token expirado" });
    }
    
    // 3. Obtener usuario
    var usuario = await _usuariosService.ObtenerPorIdAsync(passwordReset.UsuarioId);
    if (usuario == null)
    {
        return NotFound(new { message = "Usuario no encontrado" });
    }
    
    // 4. Hashear nueva contraseña
    usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
    await _usuariosService.ActualizarAsync(usuario.UsuarioId, usuario);
    
    // 5. Marcar token como usado
    passwordReset.Usado = true;
    await _passwordResetService.ActualizarAsync(passwordReset);
    
    // 6. Opcional: Invalidar todas las sesiones del usuario
    await _sesionesService.EliminarTodasPorUsuarioAsync(usuario.UsuarioId);
    
    return Ok(new { message = "Contraseña restablecida exitosamente" });
}

private string GenerateRandomToken()
{
    var random = new Random();
    return random.Next(100000, 999999).ToString();
}
```

---

## ?? CHECKLIST DE IMPLEMENTACIÓN

### ? Prioridad Crítica (Inmediato)

- [ ] **AuthService**
  - [ ] Método `login()`
  - [ ] Método `register()`
  - [ ] Método `logout()`
  - [ ] Método `getCurrentUser()`
  - [ ] Método `isAuthenticated()`
  - [ ] Gestión de localStorage

- [ ] **AuthInterceptor**
  - [ ] Agregar Bearer token automáticamente
  - [ ] Excluir endpoints públicos
  - [ ] Manejar error 401

- [ ] **AuthGuard**
  - [ ] Proteger rutas
  - [ ] Guardar returnUrl
  - [ ] Verificar token no expirado

- [ ] **LoginComponent**
  - [ ] Formulario reactivo
  - [ ] Validaciones
  - [ ] Integración con AuthService
  - [ ] Manejo de errores

- [ ] **RegisterComponent**
  - [ ] Formulario reactivo
  - [ ] Validación de passwords
  - [ ] Integración con AuthService

- [ ] **NavbarComponent**
  - [ ] Mostrar usuario actual
  - [ ] Botón logout
  - [ ] Menú dropdown

### ?? Prioridad Alta (Corto Plazo)

- [ ] **ChangePasswordComponent**
  - [ ] Crear componente nuevo
  - [ ] Formulario reactivo
  - [ ] Integración con backend

- [ ] **ProfileComponent**
  - [ ] Mostrar datos del usuario
  - [ ] Editar perfil
  - [ ] Link a cambiar contraseña

- [ ] **NotificationService**
  - [ ] Integrar ngx-toastr
  - [ ] Métodos success/error/warning/info

- [ ] **Manejo de Errores**
  - [ ] Mensajes de error descriptivos
  - [ ] Logging de errores
  - [ ] Retry logic (opcional)

### ?? Prioridad Media (Medio Plazo)

- [ ] **ForgotPasswordComponent**
  - [ ] Crear componente nuevo
  - [ ] Solicitar recuperación
  - [ ] UI para ingresar token

- [ ] **ResetPasswordComponent**
  - [ ] Crear componente nuevo
  - [ ] Formulario de restablecimiento
  - [ ] Validaciones

- [ ] **Backend: Forgot Password**
  - [ ] Tabla PasswordResets
  - [ ] Modelo PasswordReset
  - [ ] Endpoint POST /api/auth/forgot-password
  - [ ] Servicio para generar tokens

- [ ] **Backend: Reset Password**
  - [ ] Endpoint POST /api/auth/reset-password
  - [ ] Validación de tokens
  - [ ] Invalidación de sesiones (opcional)

### ?? Prioridad Baja (Largo Plazo)

- [ ] **Email Service (Backend)**
  - [ ] Integración con proveedor de email
  - [ ] Templates de email
  - [ ] Envío de tokens por email

- [ ] **Validadores Async**
  - [ ] Verificar disponibilidad de email
  - [ ] Validar formato de nombres

- [ ] **UX Improvements**
  - [ ] Password strength indicator
  - [ ] Show/hide password toggle
  - [ ] Remember me checkbox
  - [ ] Loading states mejorados

- [ ] **Testing**
  - [ ] Unit tests para servicios
  - [ ] Unit tests para componentes
  - [ ] E2E tests para flujos críticos

---

## ?? TESTING MANUAL

### Casos de Prueba Críticos

#### 1. Login
- [ ] Login exitoso con credenciales válidas
- [ ] Login fallido con email incorrecto
- [ ] Login fallido con password incorrecta
- [ ] Token se guarda en localStorage
- [ ] Redirect a returnUrl después de login
- [ ] Redirect a dashboard si no hay returnUrl

#### 2. Registro
- [ ] Registro exitoso con datos válidos
- [ ] Error con email duplicado
- [ ] Validación de passwords no coinciden
- [ ] Validación de email inválido
- [ ] Login automático después de registro

#### 3. Autenticación
- [ ] Token se envía en headers de peticiones protegidas
- [ ] Logout invalida el token en backend
- [ ] Logout limpia localStorage
- [ ] Redirect a login después de logout
- [ ] AuthGuard bloquea rutas si no autenticado

#### 4. Cambio de Contraseña
- [ ] Cambio exitoso con password actual correcta
- [ ] Error con password actual incorrecta
- [ ] Validación de password nueva muy corta
- [ ] Validación de confirmación no coincide

#### 5. Errores HTTP
- [ ] 401 ? Logout automático + redirect a login
- [ ] 403 ? Mensaje de permiso denegado
- [ ] 500 ? Mensaje de error del servidor
- [ ] Network error ? Mensaje de error de conexión

---

## ?? MÉTRICAS DE ÉXITO

### Funcionales
- ? Todos los endpoints del backend se consumen correctamente
- ? Autenticación JWT funciona end-to-end
- ? Guards protegen rutas sensibles
- ? Interceptor agrega token automáticamente
- ? Manejo de errores adecuado en toda la app

### UX
- ? Tiempo de login < 2 segundos
- ? Notificaciones claras y descriptivas
- ? Validaciones en tiempo real
- ? Estados de loading visibles
- ? Navegación intuitiva

### Seguridad
- ? Tokens no expuestos en URLs
- ? PasswordHash nunca visible en frontend
- ? Logout invalida token en backend
- ? Tokens expirados manejados correctamente
- ? HTTPS en producción (obligatorio)

---

## ?? RECURSOS Y REFERENCIAS

### Documentación Backend
- API Base URL: `http://localhost:5291/api`
- Swagger UI: `http://localhost:5291/swagger`
- Archivo: `INFORME_CAMBIOS_AUTENTICACION.md`
- Archivo: `GUIA_SWAGGER_BEARER.md`

### Librerías Recomendadas
- **Toasts/Notifications:** `ngx-toastr` o `@angular/material/snack-bar`
- **HTTP Client:** `@angular/common/http` (built-in)
- **Reactive Forms:** `@angular/forms` (built-in)
- **Router:** `@angular/router` (built-in)

### Enlaces Útiles
- **JWT.io:** https://jwt.io/ (decodificar tokens)
- **Angular Docs:** https://angular.dev/
- **TypeScript Docs:** https://www.typescriptlang.org/docs/
- **RxJS Docs:** https://rxjs.dev/

---

## ?? RESULTADO ESPERADO

Al completar todos los requerimientos, el frontend Angular deberá:

1. ? **Autenticación completa** con login, registro, logout
2. ? **Gestión de tokens JWT** automática y transparente
3. ? **Protección de rutas** con guards funcionales
4. ? **Cambio de contraseña** funcional
5. ? **Recuperación de contraseña** (si se implementa backend)
6. ? **Manejo robusto de errores** en toda la aplicación
7. ? **UX fluida** con notificaciones y estados de loading
8. ? **Integración 100%** con el backend HoneyBack API

---

**Estado del Backend:** ? Completamente funcional y listo

**Estado del Frontend:** ? Pendiente de implementación

**Prioridad:** ?? CRÍTICA

**Tiempo Estimado:** 2-3 días de desarrollo + 1 día de testing

---

*Documento generado para el proyecto HoneyBack - Frontend Angular*
*Backend: .NET 8 con JWT Authentication*
*Última actualización: 2025-01-19*
