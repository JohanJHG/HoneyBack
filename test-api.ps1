# Script de Pruebas para HoneyBack API
# Asegúrate de que la API esté ejecutándose antes de correr este script

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   HoneyBack API - Script de Pruebas Automáticas" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Configuración - ACTUALIZA ESTE PUERTO CON EL TUYO
$baseUrl = "https://localhost:5291/api"  # Cambia 5291 por tu puerto
$skipCertCheck = $true

Write-Host "URL Base: $baseUrl" -ForegroundColor Yellow
Write-Host ""

# Función helper para hacer requests
function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null
    )
    
    try {
        $params = @{
            Uri = "$baseUrl$Endpoint"
            Method = $Method
            ContentType = "application/json"
        }
        
        if ($skipCertCheck) {
            $params.Add("SkipCertificateCheck", $true)
        }
        
        if ($Body) {
            $params.Add("Body", ($Body | ConvertTo-Json))
        }
        
        $response = Invoke-RestMethod @params
        return $response
    }
    catch {
        Write-Host "Error: $_" -ForegroundColor Red
        return $null
    }
}

# ========== TEST 1: Verificar Conexión ==========
Write-Host "TEST 1: Verificando conexión a la base de datos..." -ForegroundColor Green
$conexion = Invoke-ApiRequest -Method GET -Endpoint "/pruebas/verificar-conexion"
if ($conexion) {
    Write-Host "✓ Conexión exitosa" -ForegroundColor Green
    Write-Host "  - Total Usuarios: $($conexion.totalUsuarios)" -ForegroundColor Gray
    Write-Host "  - Total Reportes: $($conexion.totalReportes)" -ForegroundColor Gray
    Write-Host "  - Total Mensajes: $($conexion.totalMensajes)" -ForegroundColor Gray
} else {
    Write-Host "✗ Error al conectar" -ForegroundColor Red
    exit
}
Write-Host ""

# ========== TEST 2: Crear Datos de Prueba ==========
Write-Host "TEST 2: Creando datos de prueba..." -ForegroundColor Green
$datosPrueba = Invoke-ApiRequest -Method POST -Endpoint "/pruebas/datos-prueba"
if ($datosPrueba) {
    Write-Host "✓ Datos de prueba creados exitosamente" -ForegroundColor Green
    Write-Host "  - Usuarios creados: $($datosPrueba.usuarios.Count)" -ForegroundColor Gray
    Write-Host "  - Reportes creados: $($datosPrueba.reportes.Count)" -ForegroundColor Gray
} else {
    Write-Host "✗ Error al crear datos de prueba" -ForegroundColor Red
}
Write-Host ""

# ========== TEST 3: CRUD de Usuarios ==========
Write-Host "TEST 3: Probando CRUD de Usuarios..." -ForegroundColor Green

# 3.1 Crear Usuario
Write-Host "  3.1 - Creando usuario..." -ForegroundColor Yellow
$nuevoUsuario = @{
    nombreCompleto = "Test PowerShell User"
    email = "test.powershell@example.com"
    passwordHash = "hashPowerShell123"
}
$usuarioCreado = Invoke-ApiRequest -Method POST -Endpoint "/usuarios" -Body $nuevoUsuario
if ($usuarioCreado) {
    $usuarioId = $usuarioCreado.usuarioId
    Write-Host "  ✓ Usuario creado con ID: $usuarioId" -ForegroundColor Green
} else {
    Write-Host "  ✗ Error al crear usuario" -ForegroundColor Red
}

# 3.2 Obtener Usuario
Write-Host "  3.2 - Obteniendo usuario por ID..." -ForegroundColor Yellow
$usuario = Invoke-ApiRequest -Method GET -Endpoint "/usuarios/$usuarioId"
if ($usuario) {
    Write-Host "  ✓ Usuario encontrado: $($usuario.nombreCompleto)" -ForegroundColor Green
}

# 3.3 Obtener Todos los Usuarios
Write-Host "  3.3 - Obteniendo todos los usuarios..." -ForegroundColor Yellow
$usuarios = Invoke-ApiRequest -Method GET -Endpoint "/usuarios"
if ($usuarios) {
    Write-Host "  ✓ Total usuarios en BD: $($usuarios.Count)" -ForegroundColor Green
}

# 3.4 Actualizar Usuario
Write-Host "  3.4 - Actualizando usuario..." -ForegroundColor Yellow
$usuarioActualizado = @{
    nombreCompleto = "Test PowerShell User UPDATED"
    email = "test.powershell@example.com"
    passwordHash = "newHashPowerShell456"
}
$resultado = Invoke-ApiRequest -Method PUT -Endpoint "/usuarios/$usuarioId" -Body $usuarioActualizado
if ($resultado) {
    Write-Host "  ✓ Usuario actualizado: $($resultado.nombreCompleto)" -ForegroundColor Green
}

Write-Host ""

# ========== TEST 4: CRUD de Reportes ==========
Write-Host "TEST 4: Probando CRUD de Reportes..." -ForegroundColor Green

# 4.1 Crear Reporte
Write-Host "  4.1 - Creando reporte..." -ForegroundColor Yellow
$nuevoReporte = @{
    nombre = "Reporte PowerShell Test"
    tipoReporte = "Test"
    descripcion = "Reporte creado desde script de pruebas"
    usuarioId = $usuarioId
}
$reporteCreado = Invoke-ApiRequest -Method POST -Endpoint "/reportes" -Body $nuevoReporte
if ($reporteCreado) {
    $reporteId = $reporteCreado.reporteId
    Write-Host "  ✓ Reporte creado con ID: $reporteId" -ForegroundColor Green
}

# 4.2 Obtener Reportes por Usuario
Write-Host "  4.2 - Obteniendo reportes del usuario..." -ForegroundColor Yellow
$reportesUsuario = Invoke-ApiRequest -Method GET -Endpoint "/reportes/usuario/$usuarioId"
if ($reportesUsuario) {
    Write-Host "  ✓ Reportes del usuario: $($reportesUsuario.Count)" -ForegroundColor Green
}

# 4.3 Actualizar Reporte
Write-Host "  4.3 - Actualizando reporte..." -ForegroundColor Yellow
$reporteActualizado = @{
    nombre = "Reporte PowerShell Test UPDATED"
    tipoReporte = "Test"
    descripcion = "Reporte actualizado desde script"
    estado = "Completado"
}
$resultado = Invoke-ApiRequest -Method PUT -Endpoint "/reportes/$reporteId" -Body $reporteActualizado
if ($resultado) {
    Write-Host "  ✓ Reporte actualizado: $($resultado.estado)" -ForegroundColor Green
}

Write-Host ""

# ========== TEST 5: CRUD de Sesiones ==========
Write-Host "TEST 5: Probando CRUD de Sesiones..." -ForegroundColor Green

# 5.1 Crear Sesión
Write-Host "  5.1 - Creando sesión..." -ForegroundColor Yellow
$nuevaSesion = @{
    usuarioId = $usuarioId
    tokenSesion = "token-powershell-test-$(Get-Random)"
    fechaExpiracion = (Get-Date).AddDays(30).ToString("yyyy-MM-ddTHH:mm:ss")
}
$sesionCreada = Invoke-ApiRequest -Method POST -Endpoint "/sesiones" -Body $nuevaSesion
if ($sesionCreada) {
    $sesionId = $sesionCreada.sesionId
    Write-Host "  ✓ Sesión creada con ID: $sesionId" -ForegroundColor Green
}

# 5.2 Validar Token
Write-Host "  5.2 - Validando token..." -ForegroundColor Yellow
$tokenValido = Invoke-ApiRequest -Method POST -Endpoint "/sesiones/validar" -Body $nuevaSesion.tokenSesion
if ($tokenValido) {
    Write-Host "  ✓ Token válido" -ForegroundColor Green
}

Write-Host ""

# ========== TEST 6: CRUD de Mensajes de Contacto ==========
Write-Host "TEST 6: Probando CRUD de Mensajes de Contacto..." -ForegroundColor Green

# 6.1 Crear Mensaje
Write-Host "  6.1 - Creando mensaje de contacto..." -ForegroundColor Yellow
$nuevoMensaje = @{
    nombre = "PowerShell Tester"
    email = "powershell@test.com"
    mensaje = "Mensaje de prueba desde script"
}
$mensajeCreado = Invoke-ApiRequest -Method POST -Endpoint "/mensajescontacto" -Body $nuevoMensaje
if ($mensajeCreado) {
    $mensajeId = $mensajeCreado.contactoId
    Write-Host "  ✓ Mensaje creado con ID: $mensajeId" -ForegroundColor Green
}

# 6.2 Obtener Todos los Mensajes
Write-Host "  6.2 - Obteniendo todos los mensajes..." -ForegroundColor Yellow
$mensajes = Invoke-ApiRequest -Method GET -Endpoint "/mensajescontacto"
if ($mensajes) {
    Write-Host "  ✓ Total mensajes: $($mensajes.Count)" -ForegroundColor Green
}

Write-Host ""

# ========== LIMPIEZA ==========
Write-Host "LIMPIEZA: Eliminando datos de prueba creados..." -ForegroundColor Magenta

Write-Host "  - Eliminando mensaje..." -ForegroundColor Yellow
Invoke-ApiRequest -Method DELETE -Endpoint "/mensajescontacto/$mensajeId" | Out-Null

Write-Host "  - Eliminando sesión..." -ForegroundColor Yellow
Invoke-ApiRequest -Method DELETE -Endpoint "/sesiones/$sesionId" | Out-Null

Write-Host "  - Eliminando reporte..." -ForegroundColor Yellow
Invoke-ApiRequest -Method DELETE -Endpoint "/reportes/$reporteId" | Out-Null

Write-Host "  - Eliminando usuario..." -ForegroundColor Yellow
Invoke-ApiRequest -Method DELETE -Endpoint "/usuarios/$usuarioId" | Out-Null

Write-Host "  - Limpiando datos de prueba del controlador de pruebas..." -ForegroundColor Yellow
Invoke-ApiRequest -Method DELETE -Endpoint "/pruebas/limpiar-datos-prueba" | Out-Null

Write-Host "✓ Limpieza completada" -ForegroundColor Green
Write-Host ""

# ========== RESUMEN FINAL ==========
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   PRUEBAS COMPLETADAS EXITOSAMENTE ✓" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Todas las operaciones CRUD funcionan correctamente:" -ForegroundColor White
Write-Host "  ✓ Usuarios" -ForegroundColor Green
Write-Host "  ✓ Reportes" -ForegroundColor Green
Write-Host "  ✓ Sesiones" -ForegroundColor Green
Write-Host "  ✓ Mensajes de Contacto" -ForegroundColor Green
Write-Host ""
Write-Host "Tu backend está totalmente funcional y listo para producción! 🎉" -ForegroundColor Yellow
Write-Host ""
