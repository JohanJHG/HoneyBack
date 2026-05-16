# Diagramas UML — HoneyBalance
## Todos los diagramas en código Mermaid listos para pegar en https://mermaid.live

---

## DIAGRAMA 1 — Clases: Dominio del Backend (Modelo de Datos)

```mermaid
classDiagram
    direction TB

    class Usuario {
        +int usuarioId
        +String nombreCompleto
        +String email
        -String passwordHash
        +DateTime fechaRegistro
        +verificarPassword(password String) bool
        +generarJwt() String
        +generarRefreshToken() String
    }

    class Transaccion {
        +int transaccionId
        +int usuarioId
        +String nombre
        +Decimal monto
        +String tipo
        +Date fecha
        +String categoria
        +DateTime fechaCreacion
    }

    class MetaAhorro {
        +int metaId
        +int usuarioId
        +String nombre
        +String descripcion
        +Decimal montoObjetivo
        +Decimal montoActual
        +DateTime fechaInicio
        +DateTime fechaObjetivo
        +DateTime fechaCompletada
        +String color
        +String icono
        +int prioridad
        +bool activa
        +bool completada
        +Decimal porcentajeAvance
        +calcularPorcentaje() Decimal
        +completar() void
        +actualizarMonto(monto Decimal) void
    }

    class ConfiguracionUsuario {
        +int configuracionId
        +int usuarioId
        +String monedaPreferida
        +String idioma
        +String tema
        +String formatoFecha
        +String nombreUsuario
        +String avatarUrl
        +bool esVeterano
        +DateTime fechaCreacion
        +DateTime fechaActualizacion
        +marcarVeterano() void
    }

    class EntornoPersonal {
        +int entornoId
        +int usuarioId
        +String moduloClave
        +String titulo
        +String subtitulo
        +String valorPrincipal
        +String etiqueta
        +String datosJson
        +DateTime fechaCreacion
        +DateTime fechaActualizacion
        +deserializarDatos() Object
    }

    class MensajeContacto {
        +int contactoId
        +String nombre
        +String email
        +String mensaje
        +DateTime fechaEnvio
    }

    class RefreshToken {
        +int id
        +int usuarioId
        +String token
        +DateTime expiracion
        +bool usado
        +esValido() bool
    }

    Usuario "1" --> "0..*" Transaccion : registra
    Usuario "1" --> "0..*" MetaAhorro : gestiona
    Usuario "1" --> "1" ConfiguracionUsuario : configura
    Usuario "1" --> "0..*" EntornoPersonal : opera
    Usuario "1" --> "0..*" RefreshToken : posee
```

---

## DIAGRAMA 2 — Clases: Capa de Servicios (Frontend Angular)

```mermaid
classDiagram
    direction LR

    class AuthService {
        -isAuthenticatedSubject BehaviorSubject~bool~
        -currentUserSubject BehaviorSubject~User~
        +isAuthenticated$ Observable~bool~
        +currentUser$ Observable~User~
        +login(credentials LoginRequest) Observable~LoginResponse~
        +register(userData RegisterRequest) Observable~any~
        +logout() void
        +refreshAccessToken() Observable~bool~
        +changePassword(req ChangePasswordRequest) Observable~void~
        +forgotPassword(email String) Observable~any~
        +resetPassword(req ResetPasswordRequest) Observable~any~
        +isAuthenticated() bool
        +getCurrentUserId() number
        +getMe() Observable~any~
        +refreshUserData() Observable~any~
    }

    class TokenService {
        -TOKEN_KEY String
        -REFRESH_KEY String
        -EXP_KEY String
        -USER_KEY String
        +setToken(token String, expiresAt String, userInfo any) void
        +getToken() String
        +removeToken() void
        +isTokenValid() bool
        +getUserInfo() JwtPayload
        +setRefreshToken(token String) void
        +getRefreshToken() String
    }

    class MetasService {
        -metas Signal~Meta[]~
        -loading Signal~bool~
        -backendDisponible Signal~bool~
        -ultimoError Signal~string~
        +totalAhorrado Signal~number~
        +totalObjetivo Signal~number~
        +progresoGeneral Signal~number~
        +metasActivas Signal~Meta[]~
        +metasCompletadas Signal~Meta[]~
        +createMeta(data MetaAhorroCreateDto) Observable~Meta~
        +updateMeta(id string, data MetaAhorroUpdateDto) Observable~Meta~
        +actualizarMonto(id string, monto number) Observable~Meta~
        +completarMeta(id string) Observable~bool~
        +deleteMeta(id string) Observable~bool~
        +cargarMetasDelBackend() Observable~Meta[]~
        +sincronizarPendientes() Observable~void~
        +tienePendientes() bool
        +reloadForCurrentUser() void
        +clear() void
    }

    class TransaccionesStoreService {
        -transacciones Signal~Transaccion[]~
        -loading Signal~bool~
        -error Signal~string~
        +loadAll() void
        +add(dto TransaccionCreateDto) Observable~Transaccion~
        +update(id number, dto TransaccionUpdateDto) Observable~Transaccion~
        +delete(id number) Observable~void~
    }

    class TransaccionesService {
        +obtenerMisTransacciones() Observable~TransaccionResponseDto[]~
        +obtenerPorId(id number) Observable~TransaccionResponseDto~
        +crear(dto TransaccionCreateDto) Observable~TransaccionResponseDto~
        +actualizar(id number, dto TransaccionUpdateDto) Observable~TransaccionResponseDto~
        +eliminar(id number) Observable~void~
        +obtenerResumenMensual(uid number, anio number, mes number) Observable~ResumenMensualDto~
    }

    class ConfiguracionesUsuarioService {
        -configSubject BehaviorSubject~ConfiguracionUsuario~
        +config$ Observable~ConfiguracionUsuario~
        +obtenerConfiguracionActual() Observable~ConfiguracionUsuario~
        +actualizarConfiguracion(datos ActualizarConfiguracionDto) Observable~ConfiguracionUsuario~
        +marcarComoVeterano() Observable~void~
        +reloadForCurrentUser() void
        +clear() void
    }

    class UserStorageService {
        -userId string
        +setItem(key String, value any) void
        +getItem~T~(key String) T
        +removeItem(key String) void
        +clear() void
        +migrateFromLegacy() void
    }

    class ThemeService {
        -currentTheme Signal~string~
        +init() void
        +toggleTheme() void
        +setTheme(tema string) void
        +getTheme() string
    }

    class ToastService {
        +success(mensaje String, titulo String) void
        +warning(mensaje String, titulo String) void
        +error(mensaje String, titulo String) void
        +info(mensaje String, titulo String) void
    }

    class EntornosApiService {
        -cache$ Observable~EntornoPersonalResponseDto[]~
        +obtenerTodos() Observable~EntornoPersonalResponseDto[]~
        +obtenerPorModulo(clave String) Observable~EntornoPersonalResponseDto[]~
        +crear(dto EntornoPersonalCreateDto) Observable~EntornoPersonalResponseDto~
        +actualizar(id number, dto EntornoPersonalUpdateDto) Observable~EntornoPersonalResponseDto~
        +eliminar(id number) Observable~void~
        +invalidateCache() void
    }

    class MensajesContactoService {
        +enviarMensaje(mensaje MensajeContacto) Observable~any~
    }

    AuthService --> TokenService : usa
    AuthService --> ToastService : notifica
    AuthService --> ConfiguracionesUsuarioService : limpia al logout
    AuthService --> MetasService : sincroniza al login
    MetasService --> UserStorageService : persiste local
    MetasService --> ToastService : notifica
    TransaccionesStoreService --> TransaccionesService : delega HTTP
    ConfiguracionesUsuarioService --> UserStorageService : fallback local
    ConfiguracionesUsuarioService --> ThemeService : aplica tema
    EntornosApiService --> UserStorageService : cache local
```

---

## DIAGRAMA 3 — Clases: DTOs de la API REST

```mermaid
classDiagram
    direction TB

    class LoginRequest {
        +String email
        +String password
    }

    class LoginResponse {
        +String token
        +String expiresAt
        +String refreshToken
        +UsuarioInfo usuario
    }

    class UsuarioInfo {
        +int usuarioId
        +String nombreCompleto
        +String email
        +String fechaRegistro
    }

    class RegisterRequest {
        +String nombreCompleto
        +String email
        +String password
    }

    class ChangePasswordRequest {
        +String passwordActual
        +String passwordNueva
    }

    class ForgotPasswordRequest {
        +String email
    }

    class ResetPasswordRequest {
        +String token
        +String newPassword
    }

    class TransaccionCreateDto {
        +String nombre
        +Decimal monto
        +String tipo
        +String fecha
        +String categoria
        +int usuarioId
    }

    class TransaccionUpdateDto {
        +String nombre
        +Decimal monto
        +String tipo
        +String fecha
        +String categoria
    }

    class TransaccionResponseDto {
        +int transaccionId
        +int usuarioId
        +String nombre
        +Decimal monto
        +String tipo
        +String fecha
        +String categoria
        +String fechaCreacion
    }

    class MetaAhorroCreateDto {
        +String nombre
        +String descripcion
        +Decimal montoObjetivo
        +Decimal montoActual
        +int usuarioId
        +String fechaObjetivo
        +String color
        +String icono
        +int prioridad
    }

    class MetaAhorroUpdateDto {
        +String nombre
        +Decimal montoObjetivo
        +Decimal montoActual
        +String fechaObjetivo
        +String color
        +String icono
        +int prioridad
        +bool activa
        +bool completada
    }

    class MetaAhorroResponseDto {
        +int metaId
        +int usuarioId
        +String nombre
        +Decimal montoObjetivo
        +Decimal montoActual
        +String fechaInicio
        +String fechaObjetivo
        +String fechaCompletada
        +String color
        +String icono
        +int prioridad
        +bool activa
        +bool completada
        +Decimal porcentajeAvance
    }

    class ActualizarConfiguracionDto {
        +String monedaPreferida
        +String idioma
        +String tema
        +String formatoFecha
        +String nombreUsuario
        +String avatarUrl
    }

    class EntornoPersonalCreateDto {
        +String moduloClave
        +String titulo
        +String subtitulo
        +String valorPrincipal
        +String datosJson
    }

    LoginResponse --> UsuarioInfo : contiene
    LoginRequest ..> LoginResponse : produce
    RegisterRequest ..> LoginResponse : produce
```

---

## DIAGRAMA 4 — Secuencia: Login y Refresh Automático de JWT

```mermaid
sequenceDiagram
    autonumber
    actor U as Usuario
    participant FE as LoginModal (Angular)
    participant AS as AuthService
    participant AI as AuthInterceptor
    participant TS as TokenService
    participant BE as Backend .NET
    participant DB as SQL Server

    rect rgb(220, 240, 255)
        Note over U,DB: FLUJO PRINCIPAL — LOGIN EXITOSO
        U->>FE: Ingresa email + contraseña
        FE->>FE: Valida formulario (Reactive Forms)
        FE->>AS: login({ email, password })
        AS->>BE: POST /api/auth/login
        BE->>DB: SELECT usuario WHERE email = ?
        DB-->>BE: Usuario encontrado
        BE->>BE: BCrypt.Verify(password, hash)
        BE->>BE: Genera JWT (exp corta) + RefreshToken (exp larga)
        BE-->>AS: { token, expiresAt, refreshToken, usuario }
        AS->>TS: setToken(token, expiresAt, userInfo)
        TS->>TS: localStorage.setItem(honeybalance_token)
        TS->>TS: localStorage.setItem(honeybalance_token_exp)
        TS->>TS: localStorage.setItem(honeybalance_refresh_token)
        AS->>AS: isAuthenticated$.next(true)
        AS->>AS: currentUser$.next(usuario)
        AS-->>FE: Login OK
        FE->>FE: Router.navigate('/dashboard')
    end

    rect rgb(255, 240, 220)
        Note over U,DB: FLUJO ALTERNATIVO — CREDENCIALES INCORRECTAS
        U->>FE: Ingresa credenciales erroneas
        FE->>AS: login({ email, password })
        AS->>BE: POST /api/auth/login
        BE->>DB: SELECT usuario WHERE email = ?
        DB-->>BE: Usuario encontrado
        BE->>BE: BCrypt.Verify() FALSE
        BE-->>AS: 401 Unauthorized
        AS-->>FE: Error generico
        FE->>FE: ToastService.error("Credenciales incorrectas")
    end

    rect rgb(220, 255, 220)
        Note over U,DB: FLUJO — TOKEN EXPIRADO (REFRESH AUTOMATICO)
        U->>FE: Realiza cualquier accion autenticada
        FE->>AI: HttpRequest a /api/...
        AI->>TS: isTokenValid() FALSE (expirado)
        AI->>BE: POST /api/auth/refresh { refreshToken }
        BE->>DB: Valida refreshToken vigente
        DB-->>BE: Token valido
        BE->>BE: Genera nuevo JWT
        BE-->>AI: { newToken, newExpiresAt }
        AI->>TS: setToken(newToken, newExpiresAt, userInfo)
        AI->>BE: Reintenta request original con nuevo JWT
        BE-->>FE: 200 OK — Respuesta esperada
        Note over AI: Usuario no nota interrupcion
    end
```

---

## DIAGRAMA 5 — Secuencia: Registrar Transacción (CU-08)

```mermaid
sequenceDiagram
    autonumber
    actor U as Usuario
    participant TC as TransaccionesComponent
    participant TF as TransaccionFormComponent
    participant TS as TransaccionesStoreService
    participant HTTP as TransaccionesService (HTTP)
    participant AI as AuthInterceptor
    participant BE as Backend .NET
    participant DB as SQL Server
    participant DASH as DashboardComponent

    U->>TC: Presiona "Nueva Transaccion" (FAB)
    TC->>TF: Abre formulario (modal)
    U->>TF: Selecciona tipo (ingreso/gasto), completa campos
    U->>TF: Presiona "Guardar"
    TF->>TF: Valida: monto > 0, nombre no vacio
    TF->>TS: add(TransaccionCreateDto)
    TS->>HTTP: crear(dto)
    HTTP->>AI: HttpRequest POST /api/transacciones
    AI->>AI: Agrega Authorization: Bearer token
    AI->>BE: POST /api/transacciones { nombre, monto, tipo, fecha, categoria, usuarioId }
    BE->>BE: Valida JWT extrae usuarioId
    BE->>BE: Valida modelo (monto > 0, tipo valido)
    BE->>DB: INSERT INTO Transacciones
    DB-->>BE: transaccionId asignado
    BE-->>HTTP: 201 Created { TransaccionResponseDto }
    HTTP-->>TS: TransaccionResponseDto
    TS->>TS: transacciones.update([...prev, nueva])
    TS-->>TF: Exito
    TF->>TF: Cierra formulario
    TF->>TF: ToastService.success("Transaccion registrada")
    TS-->>DASH: Signal actualizado recalcula KPIs
    DASH->>DASH: Actualiza: totalIngresos, totalGastos, balanceNeto

    alt Error de red o servidor
        BE-->>HTTP: 4xx / 5xx
        HTTP-->>TS: Error
        TS-->>TF: Error
        TF->>TF: ToastService.error("No se pudo guardar la transaccion")
        Note over TF: Formulario permanece abierto
    end
```

---

## DIAGRAMA 6 — Secuencia: Crear Meta de Ahorro Offline-First (CU-12)

```mermaid
sequenceDiagram
    autonumber
    actor U as Usuario
    participant MC as MetasComponent
    participant MF as MetaFormComponent
    participant MS as MetasService
    participant USS as UserStorageService
    participant BE as Backend .NET
    participant DB as SQL Server

    U->>MC: Presiona "Nueva Meta"
    MC->>MF: Abre formulario
    U->>MF: Completa: nombre, montoObjetivo, fechaObjetivo, color, icono
    U->>MF: Presiona "Crear Meta"
    MF->>MF: Valida: nombre no vacio, montoObjetivo > 0

    rect rgb(220, 240, 255)
        Note over MS,USS: PASO 1 — Persistencia local inmediata (Offline-First)
        MF->>MS: createMeta(MetaAhorroCreateDto)
        MS->>MS: Genera id local (uuid)
        MS->>USS: setItem('metas_userId', [...metas, nuevaMeta])
        MS->>MS: metas.update([...prev, { ...nueva, sincronizadoConBackend: false }])
        MS-->>MF: Meta visible en UI inmediatamente
        MF->>MF: Cierra formulario
        MF->>MF: ToastService.success("Meta creada")
    end

    rect rgb(220, 255, 220)
        Note over MS,DB: PASO 2 — Intento de sincronizacion con backend
        MS->>BE: POST /api/MetasAhorro { MetaAhorroCreateDto }
        BE->>DB: INSERT INTO MetasAhorro
        DB-->>BE: metaId: 42, porcentajeAvance: 0.0
        BE-->>MS: 201 Created { MetaAhorroResponseDto }
        MS->>MS: Actualiza meta: metaId=42, sincronizadoConBackend=true
        MS->>USS: Actualiza localStorage con metaId real
        Note over MS: Signal actualizado con datos del backend
    end

    alt Backend no disponible (sin conexion)
        BE-->>MS: Network Error / Timeout
        MS->>MS: backendDisponible.set(false)
        MS->>MS: Meta permanece con sincronizadoConBackend=false
        MS-->>MC: ToastService.warning("Meta guardada localmente")
        Note over MS,BE: Al reconectar (proximo login)
        MS->>MS: sincronizarPendientes()
        MS->>BE: POST /api/MetasAhorro (metas pendientes)
        BE-->>MS: metaId real asignado
        MS->>MS: Marca como sincronizada
    end
```

---

## DIAGRAMA 7 — Secuencia: Recuperar Contraseña (CU-04)

```mermaid
sequenceDiagram
    autonumber
    actor U as Usuario
    participant FPM as ForgotPasswordModal
    participant AS as AuthService
    participant BE as Backend .NET
    participant EMAIL as Servicio de Email
    participant DB as SQL Server

    U->>FPM: Abre modal "Olvidaste tu contraseña?"
    U->>FPM: Ingresa email y presiona "Enviar enlace"
    FPM->>FPM: Valida formato de email (Reactive Forms)
    FPM->>AS: forgotPassword(email)
    AS->>BE: POST /api/auth/forgot-password { email }
    BE->>DB: SELECT usuario WHERE email = ?

    alt Email existe en el sistema
        DB-->>BE: Usuario encontrado
        BE->>BE: Genera token unico de recuperacion (GUID + expiracion)
        BE->>DB: INSERT INTO ResetTokens { token, userId, expiracion }
        BE->>EMAIL: Envia email con enlace: /reset-password?token=guid
        EMAIL-->>U: Email recibido con enlace
    else Email NO existe
        DB-->>BE: Sin resultado
        Note over BE: No revela si el email existe (seguridad)
    end

    BE-->>AS: 200 OK (respuesta generica siempre)
    AS-->>FPM: OK
    FPM->>FPM: Muestra: "Si el correo existe, recibiras un enlace"

    rect rgb(220, 255, 220)
        Note over U,DB: RESTABLECIMIENTO DE CONTRASEÑA
        U->>U: Abre enlace del email
        U->>FPM: Ingresa nueva contraseña + confirmacion
        U->>FPM: Presiona "Restablecer"
        FPM->>AS: resetPassword({ token, newPassword })
        AS->>BE: POST /api/auth/reset-password { token, newPassword }
        BE->>DB: SELECT token WHERE token = ? AND expiracion > NOW()
        DB-->>BE: Token valido
        BE->>BE: BCrypt.Hash(newPassword)
        BE->>DB: UPDATE Usuarios SET passwordHash = ?
        BE->>DB: DELETE FROM ResetTokens WHERE token = ?
        BE-->>AS: 200 OK
        AS-->>FPM: Exito
        FPM->>FPM: ToastService.success("Contraseña restablecida")
        FPM->>FPM: Router.navigate('/') abre modal de login
    end

    alt Token expirado o invalido
        BE-->>AS: 400 Bad Request
        AS-->>FPM: Error
        FPM->>FPM: ToastService.error("El enlace ha expirado")
    end
```

---

## DIAGRAMA 8 — Actividades: Flujo de Autenticación (Login + Guard + Refresh)

```mermaid
flowchart TD
    START([Inicio]) --> A[Usuario abre la aplicacion]
    A --> B{Token en localStorage?}

    B -- NO --> C[Muestra Landing Page publica]
    C --> D[Usuario abre modal de Login]
    D --> E[Completa email y contraseña]
    E --> F{Formulario valido?}
    F -- NO --> G[Muestra errores de validacion]
    G --> E
    F -- SI --> H[POST /api/auth/login]
    H --> I{Respuesta del backend?}
    I -- 401 --> J[Toast: Credenciales incorrectas]
    J --> E
    I -- 200 --> K[Almacena JWT + RefreshToken en localStorage]
    K --> L[Actualiza isAuthenticated$ = true]
    L --> M[Redirige a /dashboard]

    B -- SI --> N{TokenService.isTokenValid?}
    N -- NO --> O[AuthInterceptor detecta expiracion]
    O --> P[POST /api/auth/refresh]
    P --> Q{Refresh exitoso?}
    Q -- SI --> R[Actualiza JWT en localStorage]
    R --> M
    Q -- NO --> S[logout — Limpia localStorage]
    S --> C

    N -- SI --> M

    M --> T[AuthGuard valida isAuthenticated$]
    T --> U{Autenticado?}
    U -- NO --> V[Toast: Debes iniciar sesion]
    V --> C
    U -- SI --> W[Carga Dashboard]
    W --> X{esVeterano = false?}
    X -- SI --> Y[Activa Tour Interactivo]
    Y --> Z[Usuario completa o salta el tour]
    Z --> AA[POST /api/configuraciones/veterano]
    AA --> AB[Dashboard activo]
    X -- NO --> AB
    AB([Fin])
```

---

## DIAGRAMA 9 — Actividades: Registro de Transacción (CU-08)

```mermaid
flowchart TD
    START([Inicio]) --> A[Usuario presiona Nueva Transaccion]
    A --> B[Abre TransaccionFormComponent]
    B --> C[Selecciona tipo: Ingreso o Gasto]
    C --> D[Completa: nombre, monto, fecha, categoria]
    D --> E[Presiona Guardar]
    E --> F{Monto > 0?}
    F -- NO --> G[Error: El monto debe ser mayor a cero]
    G --> D
    F -- SI --> H{Nombre no vacio?}
    H -- NO --> I[Error: El nombre es requerido]
    I --> D
    H -- SI --> J[TransaccionesStoreService.add]
    J --> K[AuthInterceptor agrega Bearer token]
    K --> L[POST /api/transacciones]
    L --> M{Respuesta del backend?}

    M -- 201 Created --> N[Recibe TransaccionResponseDto con ID]
    N --> O[Store actualiza Signal de transacciones]
    O --> P[Cierra formulario]
    P --> Q[Toast: Transaccion registrada]
    Q --> R[DashboardComponent recalcula KPIs]
    R --> S[Actualiza: TotalIngresos, TotalGastos, BalanceNeto]
    S --> END1([Fin])

    M -- 401 --> T[Interceptor intenta refresh token]
    T --> U{Refresh OK?}
    U -- SI --> L
    U -- NO --> V[logout redirect home]

    M -- 400 --> W[Toast: Datos invalidos]
    W --> D

    M -- 5xx --> X[Toast: Error del servidor]
    X --> END1
```

---

## DIAGRAMA 10 — Actividades: Ciclo de Vida de Meta de Ahorro (CU-12, CU-13, CU-14)

```mermaid
flowchart TD
    START([Inicio]) --> A[Usuario presiona Nueva Meta]
    A --> B[Completa: nombre, montoObjetivo, fechaObjetivo, color, icono]
    B --> C{Formulario valido?}
    C -- NO --> D[Muestra errores y espera correccion]
    D --> B
    C -- SI --> E[Guarda en localStorage inmediatamente]
    E --> F[Meta visible en UI con 0% progreso]
    F --> G[Intenta POST /api/MetasAhorro]
    G --> H{Backend disponible?}
    H -- SI --> I[Recibe metaId real del backend]
    I --> J[Actualiza localStorage con metaId y sincronizadoConBackend=true]
    H -- NO --> K[Meta queda en estado pendiente local]
    K --> L[Toast: Guardada localmente, se sincronizara]

    J --> M{Usuario actualiza el monto?}
    L --> M
    M -- SI --> N[Usuario ingresa nuevo monto actual]
    N --> O{Monto >= 0?}
    O -- NO --> P[Error de validacion]
    P --> N
    O -- SI --> Q[PATCH /api/MetasAhorro/id/monto]
    Q --> R[Backend recalcula porcentajeAvance]
    R --> S{porcentaje >= 100%?}
    S -- SI --> T[Muestra sugerencia: Completar meta?]
    T --> U2{Usuario confirma?}
    U2 -- SI --> V[POST /api/MetasAhorro/id/completar]
    V --> W[Backend registra fechaCompletada, completada=true]
    W --> X[Meta se mueve a seccion Completadas]
    X --> Y[Toast con felicitacion]
    U2 -- NO --> M
    S -- NO --> Z[Actualiza barra de progreso visualmente]
    Z --> M

    M -- NO --> AA{Usuario edita la meta?}
    AA -- SI --> AB[Abre formulario de edicion precargado]
    AB --> AC[PUT /api/MetasAhorro/id]
    AC --> M
    AA -- NO --> AD{Usuario elimina la meta?}
    AD -- SI --> AE[Dialogo de confirmacion]
    AE --> AF{Confirma?}
    AF -- SI --> AG[DELETE /api/MetasAhorro/id]
    AG --> AH[Elimina del Signal y localStorage]
    AH --> END1([Fin])
    AF -- NO --> M
    AD -- NO --> END1
    Y --> END1
```

---

## DIAGRAMA 11 — Actividades: Configuración de Preferencias de Usuario (CU-18)

```mermaid
flowchart TD
    START([Inicio]) --> A[Usuario navega a /dashboard/settings]
    A --> B[ConfiguracionesUsuarioService.obtenerConfiguracionActual]
    B --> C{Datos disponibles en backend?}
    C -- SI --> D[Carga config del backend]
    C -- NO --> E[Carga config desde localStorage como fallback]
    D --> F[Pantalla de Settings con valores actuales]
    E --> F

    F --> G{Que cambia el usuario?}

    G -- Tema --> H[Selecciona: Oscuro / Claro]
    H --> I[ThemeService.setTheme aplica inmediatamente]
    I --> J[CSS Custom Properties actualizadas en tiempo real]
    J --> G

    G -- Moneda --> K[Selecciona: COP / USD / EUR]
    K --> L2[Todos los montos se reformatean]
    L2 --> G

    G -- Perfil --> M2[Edita nombre, nombreUsuario, avatar]
    M2 --> N2{Subio imagen?}
    N2 -- SI --> O2[Convierte imagen a base64]
    O2 --> P2[Almacena en avatarUrl]
    N2 -- NO --> P2
    P2 --> G

    G -- Guardar --> Q2[Presiona Guardar Cambios]
    Q2 --> R2[PUT /api/configuraciones/id con ActualizarConfiguracionDto]
    R2 --> S2{Backend responde?}
    S2 -- 200 --> T2[Actualiza config en Signal local]
    T2 --> U3[Header y UserMenu reflejan los cambios]
    U3 --> V2[Toast: Configuracion guardada]
    V2 --> END1([Fin])
    S2 -- Error --> W2[Guarda en localStorage con sincronizadoConBackend=false]
    W2 --> X2[Toast: Guardada localmente]
    X2 --> END1
```

---

## DIAGRAMA 12 — Casos de Uso (Alto Nivel)

```mermaid
graph LR
    UP(["👤 Usuario Publico"])
    UA(["👤 Usuario Autenticado"])
    BE(["⚙️ Backend .NET"])

    subgraph SYS["🖥️ HoneyBalance — Sistema"]
        subgraph AUTH["Autenticacion"]
            CU01["CU-01 Registrarse"]
            CU02["CU-02 Iniciar sesion"]
            CU03["CU-03 Cerrar sesion"]
            CU04["CU-04 Recuperar contraseña"]
            CU05["CU-05 Cambiar contraseña"]
        end

        subgraph DASH["Dashboard Personal"]
            CU06["CU-06 Ver resumen financiero"]
            CU07["CU-07 Tour interactivo"]
        end

        subgraph TRANS["Transacciones"]
            CU08["CU-08 Registrar ingreso/gasto"]
            CU09["CU-09 Editar transaccion"]
            CU10["CU-10 Eliminar transaccion"]
            CU11["CU-11 Filtrar transacciones"]
        end

        subgraph METAS["Metas de Ahorro"]
            CU12["CU-12 Crear meta"]
            CU13["CU-13 Editar/Actualizar/Completar"]
            CU14["CU-14 Eliminar meta"]
        end

        subgraph ENT["Entornos Personales (6 modulos)"]
            CU15["CU-15 Gestionar entorno"]
            CU16["CU-16 Exportar entorno a PDF"]
        end

        subgraph EMP["Modulo Empresa"]
            CU17["CU-17 Operar empresa"]
        end

        subgraph CFG["Configuracion y Contacto"]
            CU18["CU-18 Configurar preferencias"]
            CU19["CU-19 Enviar mensaje de contacto"]
        end
    end

    UP --> CU01 & CU02 & CU04 & CU19
    UA --> CU03 & CU05 & CU06 & CU07
    UA --> CU08 & CU09 & CU10 & CU11
    UA --> CU12 & CU13 & CU14
    UA --> CU15 & CU16
    UA --> CU17 & CU18 & CU19

    CU01 & CU02 & CU04 & CU05 --> BE
    CU06 & CU07 & CU08 & CU09 & CU10 --> BE
    CU12 & CU13 & CU14 --> BE
    CU18 & CU19 --> BE
```

---

## DIAGRAMA 13 — Componentes: Arquitectura Modular del Frontend

```mermaid
graph TD
    subgraph APP["honeybalance-angular/src/app/"]

        subgraph CORE["📦 core/ — Servicios Globales Singleton"]
            GU["guards/<br/>authGuard"]
            INT["interceptors/<br/>AuthInterceptor"]
            MODELS["models/<br/>api-models · auth.models<br/>configuracion-usuario.model"]
            SVC["services/<br/>AuthService · TokenService<br/>MetasService · ThemeService<br/>ToastService · UserStorageService<br/>ConfiguracionesUsuarioService<br/>EntornosApiService"]
            UTILS["utils/<br/>logger.util"]
        end

        subgraph FEATURES["📦 features/ — Modulos por Dominio"]
            HOME["home/<br/>HomeComponent"]
            DASHF["dashboard/<br/>DashboardComponent<br/>MiResumenComponent<br/>GraficaFlujoCajaComponent"]
            TRANSF["transacciones/<br/>TransaccionesComponent<br/>TransaccionFormComponent"]
            METASF["metas/<br/>MetasComponent<br/>MetaFormComponent<br/>MetaCardComponent"]
            ENTF["entornos-personales/<br/>EntornosHubComponent<br/>PresupuestoPersonalComponent<br/>+5 modulos"]
            EMPF["empresa/<br/>EmpresaComponent (layout)<br/>EmpresaDashboardComponent<br/>+9 sub-paginas"]
            SETTINGSF["settings/<br/>SettingsComponent"]
        end

        subgraph SHARED["📦 shared/ — Componentes Reutilizables"]
            COMP["components/<br/>ButtonComponent · IconComponent<br/>CategorySelectorComponent"]
            DIRECTIVES["directives/"]
            PIPES["pipes/"]
            MODALS_SH["modals/<br/>EditProfileModalComponent"]
        end

        subgraph LAYOUT["📦 layout/ — Estructura Global"]
            HEADER["header/<br/>HeaderComponent"]
            FOOTER["footer/<br/>FooterComponent"]
            MODALS_L["modals/<br/>LoginModalComponent<br/>RegisterModalComponent<br/>ForgotPasswordModal<br/>ChangePasswordModal<br/>ContactModal · ReportModal"]
        end

        subgraph CONFIG["📦 app.config.ts + app.routes.ts"]
            ROUTES["Rutas publicas: /<br/>Rutas privadas (authGuard):<br/>/dashboard, /empresa"]
            PROVIDERS["Providers:<br/>HttpClient · Router<br/>Angular Material<br/>ngx-toastr"]
        end
    end

    FEATURES --> CORE
    FEATURES --> SHARED
    LAYOUT --> CORE
    LAYOUT --> SHARED
    SHARED --> CORE
    FEATURES --> LAYOUT
    APP --> CONFIG
```

---

## DIAGRAMA 14 — Despliegue

```mermaid
graph TB
    subgraph BROWSER["🌐 NAVEGADOR DEL USUARIO"]
        SPA["Angular SPA<br/>(JavaScript + HTML + CSS)<br/>Ejecutado en el motor V8"]
        LS_STORE["localStorage<br/>(honeybalance_token<br/>honeybalance_user<br/>metas_userId<br/>entornos_userId)"]
        SPA <--> LS_STORE
    end

    subgraph SERVER["🖥️ SERVIDOR DE PRODUCCION"]
        subgraph PROXY["NGINX / IIS — Reverse Proxy (Puerto 443 HTTPS)"]
            STATIC["Archivos Estaticos<br/>/dist/honeybalance-angular/<br/>(index.html · main.js · styles.css)"]
            REDIRECT["Regla de proxy:<br/>/api/* → localhost:5291"]
        end

        subgraph DOTNET["⚙️ .NET 8 Kestrel (Puerto 5291)"]
            CONTROLLERS["Controllers:<br/>AuthController<br/>TransaccionesController<br/>MetasAhorroController<br/>ConfiguracionesController<br/>EntornosPersonalesController<br/>MensajesContactoController"]
            MIDDLEWARE["Middlewares:<br/>JWT Auth<br/>Error Handler Global<br/>Rate Limiting<br/>CORS"]
            EF["Entity Framework Core"]
        end

        subgraph DB_SERVER["🗄️ SQL Server (Puerto 1433)"]
            TABLES["Tablas:<br/>Usuarios<br/>Transacciones<br/>MetasAhorro<br/>ConfiguracionUsuario<br/>EntornosPersonales<br/>MensajesContacto<br/>RefreshTokens"]
        end
    end

    BROWSER -- "HTTPS :443<br/>REST + JSON + JWT" --> PROXY
    PROXY -- "Sirve archivos estaticos" --> STATIC
    PROXY -- "HTTP interno :5291" --> DOTNET
    DOTNET -- "EF Core / ADO.NET :1433" --> DB_SERVER

    subgraph DEV["💻 ENTORNO DE DESARROLLO"]
        DEV_FE["ng serve :4200<br/>(Angular CLI)"]
        DEV_BE["dotnet run :5291<br/>(.NET CLI)"]
        DEV_DB["SQL Server local :1433"]
    end

    DEV_FE -- "HTTP :5291 (CORS)" --> DEV_BE
    DEV_BE --> DEV_DB
```

---

## DIAGRAMA 15 — Estados: MetaAhorro

```mermaid
stateDiagram-v2
    [*] --> PendienteLocal : createMeta() guardada en localStorage

    PendienteLocal --> Activa : POST /api/MetasAhorro OK metaId asignado
    PendienteLocal --> PendienteLocal : Backend no disponible (reintenta en proxima conexion)

    Activa --> Activa : actualizarMonto() PATCH porcentaje < 100%
    Activa --> CasiCompleta : actualizarMonto() porcentajeAvance >= 100%
    Activa --> Editada : updateMeta() PUT /api/MetasAhorro/id
    Activa --> [*] : deleteMeta() DELETE Eliminada de DB y localStorage

    CasiCompleta --> Activa : Usuario rechaza completar (ajusta monto)
    CasiCompleta --> Completada : completarMeta() POST completar

    Editada --> Activa : Actualizacion exitosa

    Completada --> [*] : deleteMeta() Eliminada de historial

    note right of PendienteLocal
        Estado transitorio:
        Visible en UI pero pendiente de backend.
        Deuda tecnica HT-07
    end note

    note right of Completada
        Estado terminal.
        No puede revertirse.
        Se mueve a seccion Metas Completadas
    end note
```

---

## DIAGRAMA 16a — Estados: Suscripción (Entorno Personal)

```mermaid
stateDiagram-v2
    [*] --> Activa : createSuscripcion() Estado inicial al registrar

    Activa --> Activa : Fecha de pago alcanzada (renovacion automatica conceptual)
    Activa --> Cancelada : cancelarSuscripcion() estado = cancelada
    Activa --> [*] : eliminarSuscripcion()

    Cancelada --> Activa : reactivarSuscripcion() estado = activa
    Cancelada --> [*] : eliminarSuscripcion()

    note right of Activa
        Contribuye al total
        mensual y anual
        de suscripciones activas
    end note
```

---

## DIAGRAMA 16b — Estados: Sincronización de Datos (Patrón Offline-First)

```mermaid
stateDiagram-v2
    [*] --> SoloLocal : Datos creados sin conexion sincronizadoConBackend = false

    SoloLocal --> Sincronizando : sincronizarPendientes() Llamada HTTP al backend
    SoloLocal --> [*] : deleteMeta() Eliminado solo en localStorage

    Sincronizando --> Sincronizado : Backend responde 201 metaId asignado
    Sincronizando --> SoloLocal : Network Error / Timeout Permanece pendiente
    Sincronizando --> ConflictoVersion : Backend retorna 409 Dato ya existe con otro ID

    ConflictoVersion --> Sincronizado : Resolucion manual o estrategia merge

    Sincronizado --> Sincronizado : CRUD posterior conectado PUT/PATCH/DELETE
    Sincronizado --> [*] : deleteMeta() Eliminado en backend y local

    note right of SoloLocal
        Riesgo: Si el usuario
        borra el localStorage,
        los datos se pierden.
        (Deuda tecnica HT-07, HT-08)
    end note
```

---

## INSTRUCCIONES DE USO

1. **Mermaid Live Editor (Recomendado):**
   - Ir a https://mermaid.live
   - Copiar el código de cada diagrama (SIN las líneas ` ```mermaid ` y ` ``` `)
   - Pegar en el panel izquierdo
   - El diagrama se renderiza al instante en el panel derecho
   - Clic en "Actions" → "PNG" o "SVG" para exportar

2. **VS Code:**
   - Instalar extensión "Markdown Preview Mermaid Support"
   - Abrir este archivo .md
   - Ctrl + Shift + V para previsualizar

3. **GitHub / GitLab:**
   - Subir este archivo .md al repositorio
   - Los bloques mermaid se renderizan automáticamente
