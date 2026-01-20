# API de Metas de Ahorro

Documentacion de endpoints para gestion de metas de ahorro.

**Base URL:** `http://localhost:5291/api/metasahorro`

**Autenticacion:** Todos los endpoints requieren Bearer Token (JWT)

---

## Endpoints

| Metodo | Ruta | Descripcion |
|--------|------|-------------|
| GET | `/api/metasahorro` | Obtener todas las metas |
| GET | `/api/metasahorro/{id}` | Obtener meta por ID |
| GET | `/api/metasahorro/usuario/{usuarioId}` | Obtener metas de un usuario |
| GET | `/api/metasahorro/usuario/{usuarioId}/activas` | Obtener metas activas de un usuario |
| GET | `/api/metasahorro/usuario/{usuarioId}/completadas` | Obtener metas completadas de un usuario |
| POST | `/api/metasahorro` | Crear nueva meta |
| PUT | `/api/metasahorro/{id}` | Actualizar meta |
| PATCH | `/api/metasahorro/{id}/monto` | Actualizar solo el monto actual |
| POST | `/api/metasahorro/{id}/completar` | Marcar meta como completada |
| DELETE | `/api/metasahorro/{id}` | Eliminar meta |

---

## Schemas

### MetaAhorroCreateDto (Request para crear)

```json
{
  "nombre": "string",           // Requerido, max 200 caracteres
  "descripcion": "string",      // Opcional, max 1000 caracteres
  "montoObjetivo": 0.00,        // Requerido, debe ser > 0
  "montoActual": 0.00,          // Opcional, default 0, no puede ser negativo
  "usuarioId": 0,               // Requerido
  "fechaObjetivo": "2026-12-31",// Opcional, formato YYYY-MM-DD
  "color": "#FFD8A9",           // Opcional, formato hexadecimal
  "icono": "string",            // Opcional, max 50 caracteres
  "prioridad": 0                // Opcional, rango 0-10
}
```

### MetaAhorroUpdateDto (Request para actualizar)

```json
{
  "nombre": "string",           // Requerido, max 200 caracteres
  "descripcion": "string",      // Opcional
  "montoObjetivo": 0.00,        // Requerido, debe ser > 0
  "montoActual": 0.00,          // Opcional
  "fechaObjetivo": "2026-12-31",// Opcional
  "color": "#FFD8A9",           // Opcional
  "icono": "string",            // Opcional
  "prioridad": 0,               // Opcional, rango 0-10
  "activa": true,               // Opcional
  "completada": false           // Opcional
}
```

### MetaAhorroResponseDto (Response)

```json
{
  "metaId": 1,
  "usuarioId": 1,
  "nombre": "Vacaciones 2026",
  "descripcion": "Ahorro para viaje a Europa",
  "montoObjetivo": 5000000.00,
  "montoActual": 1500000.00,
  "fechaInicio": "2026-01-01",
  "fechaObjetivo": "2026-12-31",
  "fechaCompletada": null,
  "color": "#FFD8A9",
  "icono": "airplane",
  "prioridad": 5,
  "activa": true,
  "completada": false,
  "porcentajeAvance": 30.00
}
```

---

## Detalle de Endpoints

### GET /api/metasahorro

Obtiene todas las metas de ahorro.

**Response 200:**
```json
[
  {
    "metaId": 1,
    "usuarioId": 1,
    "nombre": "Vacaciones 2026",
    "montoObjetivo": 5000000.00,
    "montoActual": 1500000.00,
    "porcentajeAvance": 30.00,
    ...
  }
]
```

---

### GET /api/metasahorro/{id}

Obtiene una meta por su ID.

**Parametros:**
- `id` (int): ID de la meta

**Response 200:**
```json
{
  "metaId": 1,
  "usuarioId": 1,
  "nombre": "Vacaciones 2026",
  ...
}
```

**Response 404:**
```json
{
  "mensaje": "Meta no encontrada"
}
```

---

### GET /api/metasahorro/usuario/{usuarioId}

Obtiene todas las metas de un usuario.

**Parametros:**
- `usuarioId` (int): ID del usuario

**Response 200:**
```json
[
  {
    "metaId": 1,
    "usuarioId": 1,
    "nombre": "Vacaciones 2026",
    ...
  },
  {
    "metaId": 2,
    "usuarioId": 1,
    "nombre": "Carro nuevo",
    ...
  }
]
```

---

### GET /api/metasahorro/usuario/{usuarioId}/activas

Obtiene solo las metas activas (no completadas) de un usuario.

**Parametros:**
- `usuarioId` (int): ID del usuario

---

### GET /api/metasahorro/usuario/{usuarioId}/completadas

Obtiene solo las metas completadas de un usuario.

**Parametros:**
- `usuarioId` (int): ID del usuario

---

### POST /api/metasahorro

Crea una nueva meta de ahorro.

**Request Body:**
```json
{
  "nombre": "Vacaciones 2026",
  "descripcion": "Ahorro para viaje a Europa",
  "montoObjetivo": 5000000.00,
  "montoActual": 0,
  "usuarioId": 1,
  "fechaObjetivo": "2026-12-31",
  "color": "#FFD8A9",
  "icono": "airplane",
  "prioridad": 5
}
```

**Response 201:**
```json
{
  "metaId": 1,
  "usuarioId": 1,
  "nombre": "Vacaciones 2026",
  "montoObjetivo": 5000000.00,
  "montoActual": 0,
  "porcentajeAvance": 0,
  ...
}
```

**Response 400 (validacion fallida):**
```json
{
  "errors": {
    "Nombre": ["El nombre de la meta es requerido"],
    "MontoObjetivo": ["El monto objetivo debe ser mayor a 0"]
  }
}
```

---

### PUT /api/metasahorro/{id}

Actualiza una meta existente.

**Parametros:**
- `id` (int): ID de la meta

**Request Body:**
```json
{
  "nombre": "Vacaciones Europa 2026",
  "descripcion": "Ahorro para viaje actualizado",
  "montoObjetivo": 6000000.00,
  "montoActual": 2000000.00,
  "fechaObjetivo": "2026-12-31",
  "color": "#E8A87C",
  "icono": "airplane",
  "prioridad": 8,
  "activa": true,
  "completada": false
}
```

**Response 200:**
```json
{
  "metaId": 1,
  "nombre": "Vacaciones Europa 2026",
  "montoObjetivo": 6000000.00,
  "montoActual": 2000000.00,
  "porcentajeAvance": 33.33,
  ...
}
```

**Response 404:**
```json
{
  "mensaje": "Meta no encontrada"
}
```

---

### PATCH /api/metasahorro/{id}/monto

Actualiza unicamente el monto actual de una meta.

**Parametros:**
- `id` (int): ID de la meta

**Request Body:**
```json
2500000.00
```

**Response 200:**
```json
{
  "metaId": 1,
  "montoActual": 2500000.00,
  "porcentajeAvance": 50.00,
  ...
}
```

**Response 400:**
```json
{
  "mensaje": "El monto no puede ser negativo"
}
```

---

### POST /api/metasahorro/{id}/completar

Marca una meta como completada.

**Parametros:**
- `id` (int): ID de la meta

**Response 200:**
```json
{
  "mensaje": "Meta marcada como completada exitosamente"
}
```

**Response 404:**
```json
{
  "mensaje": "Meta no encontrada"
}
```

---

### DELETE /api/metasahorro/{id}

Elimina una meta.

**Parametros:**
- `id` (int): ID de la meta

**Response 200:**
```json
{
  "mensaje": "Meta eliminada exitosamente"
}
```

**Response 404:**
```json
{
  "mensaje": "Meta no encontrada"
}
```

---

## Ejemplos con curl

### Crear meta
```bash
curl -X POST http://localhost:5291/api/metasahorro \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {tu-token}" \
  -d '{
    "nombre": "Vacaciones 2026",
    "montoObjetivo": 5000000,
    "usuarioId": 1
  }'
```

### Obtener metas de un usuario
```bash
curl -X GET http://localhost:5291/api/metasahorro/usuario/1 \
  -H "Authorization: Bearer {tu-token}"
```

### Actualizar monto
```bash
curl -X PATCH http://localhost:5291/api/metasahorro/1/monto \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {tu-token}" \
  -d '2500000'
```

### Marcar como completada
```bash
curl -X POST http://localhost:5291/api/metasahorro/1/completar \
  -H "Authorization: Bearer {tu-token}"
```

---

## Validaciones

| Campo | Regla |
|-------|-------|
| nombre | Requerido, max 200 caracteres |
| descripcion | Opcional, max 1000 caracteres |
| montoObjetivo | Requerido, debe ser > 0 |
| montoActual | Opcional, no puede ser negativo |
| usuarioId | Requerido (solo en crear) |
| fechaObjetivo | Opcional, formato YYYY-MM-DD |
| color | Opcional, formato hexadecimal (#RRGGBB) |
| icono | Opcional, max 50 caracteres |
| prioridad | Opcional, rango 0-10 |

---

## Codigos de Respuesta

| Codigo | Descripcion |
|--------|-------------|
| 200 | Operacion exitosa |
| 201 | Recurso creado |
| 400 | Error de validacion |
| 401 | No autorizado (token invalido o ausente) |
| 404 | Meta no encontrada |
| 500 | Error interno del servidor |

---

Ultima actualizacion: 2026-01-09
