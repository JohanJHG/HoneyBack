# 🔧 Instrucciones de Implementación - Aislamiento de Datos por Usuario

## Backend .NET 8 + Entity Framework Core

**Fecha:** 20 de enero de 2026  
**Objetivo:** Garantizar que cada usuario solo acceda a sus propios datos

---

## 📋 Índice de Cambios

1. [Crear Helper para Extraer UserId del Token](#1-crear-helper-para-extraer-userid-del-token)
2. [Modificar MetasAhorroController](#2-modificar-metasahorrocontroller)
3. [Modificar TemplatesController](#3-modificar-templatescontroller)
4. [Modificar ConfiguracionesUsuarioController](#4-modificar-configuraboracionesusuariocontroller)
5. [Modificar CategoriasTransaccionesController](#5-modificar-categoriastransaccionescontroller)
6. [Modificar EstadisticasMensualesController](#6-modificar-estadisticasmensualescontroller)
7. [Modificar ReportesController](#7-modificar-reportescontroller)
8. [Modificar SesionesController](#8-modificar-sesionescontroller)
9. [Modificar MensajesContactoController](#9-modificar-mensajescontactocontroller)
10. [Modificar UsuariosController](#10-modificar-usuarioscontroller)

---

## 1. Crear Helper para Extraer UserId del Token

### Crear archivo: `Extensions/ClaimsPrincipalExtensions.cs`

```csharp
using System.Security.Claims;

namespace HoneyBack.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Extrae el UserId del token JWT de forma segura.
        /// Busca en los claims estándar: "UserId", "sub", NameIdentifier
        /// </summary>
        /// <param name="user">ClaimsPrincipal del usuario autenticado</param>
        /// <returns>UserId o null si no se encuentra</returns>
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            // Intentar obtener de claim personalizado "UserId"
            var userIdClaim = user.FindFirst("UserId")
                ?? user.FindFirst("userId")
                ?? user.FindFirst("sub")
                ?? user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            return null;
        }

        /// <summary>
        /// Obtiene el UserId o lanza excepción si no existe
        /// </summary>
        public static int GetUserIdRequired(this ClaimsPrincipal user)
        {
            var userId = user.GetUserId();
            if (!userId.HasValue)
            {
                throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario del token");
            }
            return userId.Value;
        }
    }
}
```

### Registrar en `Program.cs` (si es necesario)

No requiere registro, solo agregar `using HoneyBack.Extensions;` en los controllers.

---

## 2. Modificar MetasAhorroController

### Agregar using al inicio:

```csharp
using HoneyBack.Extensions;
```

### 2.1 ELIMINAR o MODIFICAR endpoint global `GET /api/MetasAhorro`

**Opción A - Eliminar (recomendado):**

```csharp
// ELIMINAR este método completamente
// [HttpGet]
// public async Task<ActionResult<IEnumerable<MetaAhorroResponseDto>>> ObtenerTodos()
```

**Opción B - Convertir a filtrado por usuario del token:**

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<MetaAhorroResponseDto>>> ObtenerMisMetas()
{
    try
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
            return Unauthorized(new { mensaje = "Usuario no autenticado" });

        var metas = await _metasService.ObtenerPorUsuarioAsync(userId.Value);
        var response = metas.Select(m => MapToDto(m));
        return Ok(response);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { mensaje = "Error al obtener metas", error = ex.Message });
    }
}
```

### 2.2 MODIFICAR `GET /api/MetasAhorro/{id}` - Validar propiedad

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<MetaAhorroResponseDto>> ObtenerPorId(int id)
{
    try
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
            return Unauthorized(new { mensaje = "Usuario no autenticado" });

        var meta = await _metasService.ObtenerPorIdAsync(id);
        if (meta == null)
            return NotFound(new { mensaje = "Meta no encontrada" });

        // ⚠️ VALIDACIÓN DE PROPIEDAD
        if (meta.UsuarioId != userId.Value)
            return Forbid(); // 403 - No tiene permiso

        return Ok(MapToDto(meta));
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { mensaje = "Error al obtener meta", error = ex.Message });
    }
}
```

### 2.3 MODIFICAR `GET /api/MetasAhorro/usuario/{usuarioId}` - Validar que solo acceda a sus datos

```csharp
[HttpGet("usuario/{usuarioId}")]
public async Task<ActionResult<IEnumerable<MetaAhorroResponseDto>>> ObtenerPorUsuario(int usuarioId)
{
    try
    {
        var tokenUserId = User.GetUserId();
        if (!tokenUserId.HasValue)
            return Unauthorized(new { mensaje = "Usuario no autenticado" });

        // ⚠️ VALIDACIÓN: Solo puede consultar SUS metas
        if (usuarioId != tokenUserId.Value)
            return Forbid(); // 403

        var metas = await _metasService.ObtenerPorUsuarioAsync(usuarioId);
        var response = metas.Select(m => MapToDto(m));
        return Ok(response);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { mensaje = "Error al obtener metas del usuario", error = ex.Message });
    }
}
```

### 2.4 MODIFICAR `POST /api/MetasAhorro` - Usar userId del token

```csharp
[HttpPost]
public async Task<ActionResult<MetaAhorroResponseDto>> Crear([FromBody] MetaAhorroCreateDto metaDto)
{
    try
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.GetUserId();
        if (!userId.HasValue)
            return Unauthorized(new { mensaje = "Usuario no autenticado" });

        var meta = new MetasAhorro
        {
            // ⚠️ USAR userId DEL TOKEN, NO del DTO
            UsuarioId = userId.Value,
            Nombre = metaDto.Nombre,
            Descripcion = metaDto.Descripcion,
            Categoria = metaDto.Categoria ?? "otro",
            MontoObjetivo = metaDto.MontoObjetivo,
            MontoActual = metaDto.MontoActual ?? 0,
            FechaObjetivo = metaDto.FechaObjetivo,
            Color = metaDto.Color ?? "#FFD8A9",
            Icono = metaDto.Icono,
            Prioridad = metaDto.Prioridad ?? 0
        };

        var metaCreada = await _metasService.CrearAsync(meta);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = metaCreada.MetaId }, MapToDto(metaCreada));
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { mensaje = "Error al crear meta", error = ex.Message });
    }
}
```

### 2.5 MODIFICAR `PUT /api/MetasAhorro/{id}` - Validar propiedad

```csharp
[HttpPut("{id}")]
public async Task<ActionResult<MetaAhorroResponseDto>> Actualizar(int id, [FromBody] MetaAhorroUpdateDto metaDto)
{
    try
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.GetUserId();
        if (!userId.HasValue)
            return Unauthorized(new { mensaje = "Usuario no autenticado" });

        // ⚠️ VALIDAR PROPIEDAD ANTES DE ACTUALIZAR
        var metaExistente = await _metasService.ObtenerPorIdAsync(id);
        if (metaExistente == null)
            return NotFound(new { mensaje = "Meta no encontrada" });

        if (metaExistente.UsuarioId != userId.Value)
            return Forbid(); // 403

        var meta = new MetasAhorro
        {
            Nombre = metaDto.Nombre,
            Descripcion = metaDto.Descripcion,
            Categoria = metaDto.Categoria ?? "otro",
            MontoObjetivo = metaDto.MontoObjetivo,
            MontoActual = metaDto.MontoActual,
            FechaObjetivo = metaDto.FechaObjetivo,
            Color = metaDto.Color,
            Icono = metaDto.Icono,
            Prioridad = metaDto.Prioridad,
            Activa = metaDto.Activa,
            Completada = metaDto.Completada
        };

        var metaActualizada = await _metasService.ActualizarAsync(id, meta);
        return Ok(MapToDto(metaActualizada));
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { mensaje = "Error al actualizar meta", error = ex.Message });
    }
}
```

### 2.6 MODIFICAR `PATCH /api/MetasAhorro/{id}/monto` - Validar propiedad

```csharp
[HttpPatch("{id}/monto")]
public async Task<ActionResult<MetaAhorroResponseDto>> ActualizarMonto(int id, [FromBody] decimal nuevoMonto)
{
    try
    {
        if (nuevoMonto < 0)
            return BadRequest(new { mensaje = "El monto no puede ser negativo" });

        var userId = User.GetUserId();
        if (!userId.HasValue)
            return Unauthorized(new { mensaje = "Usuario no autenticado" });

        // ⚠️ VALIDAR PROPIEDAD
        var metaExistente = await _metasService.ObtenerPorIdAsync(id);
        if (metaExistente == null)
            return NotFound(new { mensaje = "Meta no encontrada" });

        if (metaExistente.UsuarioId != userId.Value)
            return Forbid();

        var metaActualizada = await _metasService.ActualizarMontoAsync(id, nuevoMonto);
        return Ok(MapToDto(metaActualizada));
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { mensaje = "Error al actualizar monto", error = ex.Message });
    }
}
```

### 2.7 MODIFICAR `DELETE /api/MetasAhorro/{id}` - Validar propiedad

```csharp
[HttpDelete("{id}")]
public async Task<ActionResult> Eliminar(int id)
{
    try
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
            return Unauthorized(new { mensaje = "Usuario no autenticado" });

        // ⚠️ VALIDAR PROPIEDAD
        var meta = await _metasService.ObtenerPorIdAsync(id);
        if (meta == null)
            return NotFound(new { mensaje = "Meta no encontrada" });

        if (meta.UsuarioId != userId.Value)
            return Forbid();

        await _metasService.EliminarAsync(id);
        return Ok(new { mensaje = "Meta eliminada exitosamente" });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { mensaje = "Error al eliminar meta", error = ex.Message });
    }
}
```

---

## 3. Modificar TemplatesController

### Aplicar el mismo patrón que MetasAhorroController:

| Endpoint                                 | Acción                                                      |
| ---------------------------------------- | ----------------------------------------------------------- |
| `GET /api/Templates`                     | Eliminar o filtrar por userId del token                     |
| `GET /api/Templates/{id}`                | Validar propiedad antes de retornar                         |
| `GET /api/Templates/usuario/{usuarioId}` | Validar que `usuarioId == User.GetUserId()`                 |
| `POST /api/Templates`                    | Usar `User.GetUserId()` en lugar de `templateDto.UsuarioId` |
| `PUT /api/Templates/{id}`                | Validar propiedad antes de actualizar                       |
| `DELETE /api/Templates/{id}`             | Validar propiedad antes de eliminar                         |

### Código de ejemplo para POST:

```csharp
[HttpPost]
public async Task<ActionResult<TemplateResponseDto>> Crear([FromBody] TemplateCreateDto templateDto)
{
    try
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.GetUserId();
        if (!userId.HasValue)
            return Unauthorized(new { mensaje = "Usuario no autenticado" });

        // Validar nombre único para ESTE usuario
        if (await _templatesService.ExisteNombreAsync(templateDto.Nombre, userId.Value))
        {
            return Conflict(new { mensaje = "Ya existe un template con ese nombre" });
        }

        var template = new Template
        {
            UsuarioId = userId.Value, // ⚠️ DEL TOKEN
            Nombre = templateDto.Nombre,
            CategoriaId = templateDto.CategoriaId,
            Monto = templateDto.Monto,
            Descripcion = templateDto.Descripcion,
            Tipo = templateDto.Tipo
        };

        var templateCreado = await _templatesService.CrearAsync(template);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = templateCreado.TemplateId }, MapToDto(templateCreado));
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { mensaje = "Error al crear template", error = ex.Message });
    }
}
```

---

## 4. Modificar ConfiguracionesUsuarioController

### Cambios requeridos:

| Endpoint                                              | Acción                                         |
| ----------------------------------------------------- | ---------------------------------------------- |
| `GET /api/ConfiguracionesUsuario`                     | **ELIMINAR** - No debe existir endpoint global |
| `GET /api/ConfiguracionesUsuario/{id}`                | Validar propiedad                              |
| `GET /api/ConfiguracionesUsuario/usuario/{usuarioId}` | Validar `usuarioId == User.GetUserId()`        |
| `POST /api/ConfiguracionesUsuario`                    | Usar `User.GetUserId()`                        |
| `PUT /api/ConfiguracionesUsuario/{id}`                | Validar propiedad                              |
| `DELETE /api/ConfiguracionesUsuario/{id}`             | Validar propiedad                              |

---

## 5. Modificar CategoriasTransaccionesController

### Cambios requeridos:

```csharp
// GET global - Cambiar para filtrar por usuario
[HttpGet]
public async Task<ActionResult<IEnumerable<CategoriaTransaccionResponseDto>>> ObtenerMisCategorias()
{
    try
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
            return Unauthorized(new { mensaje = "Usuario no autenticado" });

        // Retorna categorías del sistema + las del usuario
        var categorias = await _categoriasService.ObtenerPorUsuarioAsync(userId.Value);
        var response = categorias.Select(c => MapToDto(c));
        return Ok(response);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { mensaje = "Error al obtener categorías", error = ex.Message });
    }
}

// GET por tipo - Filtrar también por usuario
[HttpGet("tipo/{tipo}")]
public async Task<ActionResult<IEnumerable<CategoriaTransaccionResponseDto>>> ObtenerPorTipo(string tipo)
{
    try
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
            return Unauthorized(new { mensaje = "Usuario no autenticado" });

        // ⚠️ Agregar filtro de usuario en el servicio o aquí
        var categorias = await _categoriasService.ObtenerPorTipoYUsuarioAsync(tipo, userId.Value);
        var response = categorias.Select(c => MapToDto(c));
        return Ok(response);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { mensaje = "Error al obtener categorías por tipo", error = ex.Message });
    }
}
```

**Nota:** Puede ser necesario agregar un nuevo método en el servicio:

```csharp
// En ICategoriasTransaccionesService
Task<IEnumerable<CategoriasTransaccione>> ObtenerPorTipoYUsuarioAsync(string tipo, int usuarioId);

// Implementación
public async Task<IEnumerable<CategoriasTransaccione>> ObtenerPorTipoYUsuarioAsync(string tipo, int usuarioId)
{
    return await _context.CategoriasTransacciones
        .Where(c => c.Tipo == tipo && (c.UsuarioId == usuarioId || c.EsSistema == true))
        .OrderBy(c => c.Nombre)
        .ToListAsync();
}
```

---

## 6. Modificar EstadisticasMensualesController

### Cambios requeridos:

| Endpoint                              | Acción                                  |
| ------------------------------------- | --------------------------------------- |
| `GET /api/EstadisticasMensuales`      | **ELIMINAR**                            |
| `GET /api/EstadisticasMensuales/{id}` | Validar propiedad                       |
| Todos los endpoints con `{usuarioId}` | Validar `usuarioId == User.GetUserId()` |
| `POST`                                | Usar `User.GetUserId()`                 |
| `PUT/DELETE`                          | Validar propiedad                       |

---

## 7. Modificar ReportesController

### ⚠️ CRÍTICO: Agregar autorización

```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize] // ⚠️ AGREGAR ESTO
public class ReportesController : ControllerBase
```

### Eliminar `[AllowAnonymous]` de todos los endpoints y aplicar el mismo patrón de validación.

---

## 8. Modificar SesionesController

### Cambios requeridos:

```csharp
// GET global - Filtrar por usuario
[HttpGet]
public async Task<ActionResult<IEnumerable<Sesione>>> ObtenerMisSesiones()
{
    try
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
            return Unauthorized(new { mensaje = "Usuario no autenticado" });

        var sesiones = await _sesionesService.ObtenerPorUsuarioAsync(userId.Value);
        return Ok(sesiones);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { mensaje = "Error al obtener sesiones", error = ex.Message });
    }
}
```

---

## 9. Modificar MensajesContactoController

### Agregar autorización:

```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize] // ⚠️ AGREGAR (excepto POST para mensajes de visitantes)
public class MensajesContactoController : ControllerBase
```

### Si los mensajes de contacto deben ser públicos (formulario de contacto):

- `POST` puede ser `[AllowAnonymous]`
- `GET` y `DELETE` deben requerir autorización

---

## 10. Modificar UsuariosController

### Cambios requeridos:

```csharp
// GET global - ELIMINAR o restringir a administradores
[HttpGet]
[Authorize(Roles = "Admin")] // Si tienes roles
public async Task<ActionResult<IEnumerable<UsuarioResponseDto>>> ObtenerTodos()
{
    // Solo para administradores
}

// GET por ID - Solo puede ver su propio perfil
[HttpGet("{id}")]
public async Task<ActionResult<UsuarioResponseDto>> ObtenerPorId(int id)
{
    try
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
            return Unauthorized(new { mensaje = "Usuario no autenticado" });

        // Solo puede ver su propio perfil
        if (id != userId.Value)
            return Forbid();

        var usuario = await _usuariosService.ObtenerPorIdAsync(id);
        if (usuario == null)
            return NotFound(new { mensaje = "Usuario no encontrado" });

        return Ok(MapToDto(usuario));
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { mensaje = "Error al obtener usuario", error = ex.Message });
    }
}
```

---

## 📝 Resumen de Patrones a Aplicar

### Patrón 1: Validación de Usuario Autenticado

```csharp
var userId = User.GetUserId();
if (!userId.HasValue)
    return Unauthorized(new { mensaje = "Usuario no autenticado" });
```

### Patrón 2: Validación de Propiedad (GET/PUT/DELETE por ID)

```csharp
var recurso = await _service.ObtenerPorIdAsync(id);
if (recurso == null)
    return NotFound(new { mensaje = "Recurso no encontrado" });

if (recurso.UsuarioId != userId.Value)
    return Forbid(); // 403
```

### Patrón 3: Creación con UserId del Token (POST)

```csharp
var entidad = new Entidad
{
    UsuarioId = userId.Value, // DEL TOKEN, no del body
    // ... otros campos del DTO
};
```

### Patrón 4: Validación de Consulta por Usuario

```csharp
// En endpoints /usuario/{usuarioId}
if (usuarioId != userId.Value)
    return Forbid();
```

---

## ✅ Checklist de Implementación

- [ ] Crear `Extensions/ClaimsPrincipalExtensions.cs`
- [ ] Agregar `using HoneyBack.Extensions;` a todos los controllers
- [ ] Modificar `MetasAhorroController`
- [ ] Modificar `TemplatesController`
- [ ] Modificar `ConfiguracionesUsuarioController`
- [ ] Modificar `CategoriasTransaccionesController`
- [ ] Modificar `EstadisticasMensualesController`
- [ ] Modificar `ReportesController` (agregar [Authorize])
- [ ] Modificar `SesionesController`
- [ ] Modificar `MensajesContactoController`
- [ ] Modificar `UsuariosController`
- [ ] Probar con dos usuarios diferentes
- [ ] Verificar que no hay acceso cruzado

---

## 🧪 Pruebas Recomendadas

1. **Login con Usuario A**, crear una meta
2. **Login con Usuario B**, intentar:
   - `GET /api/MetasAhorro/{id de meta de A}` → Debe dar 403
   - `PUT /api/MetasAhorro/{id de meta de A}` → Debe dar 403
   - `DELETE /api/MetasAhorro/{id de meta de A}` → Debe dar 403
3. Repetir para cada controller

---

**Cuando tengas tu backend real disponible, compártelo y lo revisaré para aplicar estos cambios específicamente a tu código.**
