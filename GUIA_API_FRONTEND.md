# 🎯 Guía de API Backend - HoneyBalance

Esta guía explica de forma simple **qué datos enviar y recibir** desde tu frontend al consumir el backend de HoneyBalance.

---

## 📋 Tabla de Contenidos

1. [Autenticación (Auth)](#1-autenticación-auth)
2. [Categorías de Transacciones](#2-categorías-de-transacciones)
3. [Transacciones](#3-transacciones)
4. [Metas de Ahorro](#4-metas-de-ahorro)
5. [Estadísticas Mensuales](#5-estadísticas-mensuales)
6. [Reportes](#6-reportes)
7. [Usuarios](#7-usuarios)
8. [Mensajes de Contacto](#8-mensajes-de-contacto)
9. [Templates](#9-templates)
10. [Configuraciones de Usuario](#10-configuraciones-de-usuario)
11. [Datos Innecesarios que Puedes Omitir](#datos-innecesarios-que-puedes-omitir)

---

## 🔐 1. Autenticación (Auth)

**Controlador:** `api/auth`

### 1.1 Login (Iniciar Sesión)
- **Ruta:** `POST /api/auth/login`
- **Autenticación:** ❌ No requiere

**Lo que ENVÍAS:**
```json
{
  "email": "usuario@example.com",
  "password": "contraseña123"
}
```

**Lo que RECIBES:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-03-15T10:30:00Z",
  "usuario": {
    "usuarioId": 1,
    "nombreCompleto": "Juan Pérez",
    "email": "usuario@example.com",
    "fechaRegistro": "2024-01-01T00:00:00Z"
  }
}
```

**🔑 Importante:** Guarda el `token` para usarlo en todas las demás peticiones.

---

### 1.2 Registro (Crear Cuenta)
- **Ruta:** `POST /api/auth/register`
- **Autenticación:** ❌ No requiere

**Lo que ENVÍAS:**
```json
{
  "nombreCompleto": "Juan Pérez",
  "email": "usuario@example.com",
  "password": "contraseña123"
}
```

**Lo que RECIBES:**
```json
{
  "usuarioId": 1,
  "nombreCompleto": "Juan Pérez",
  "email": "usuario@example.com",
  "fechaRegistro": "2024-01-01T00:00:00Z"
}
```

---

### 1.3 Obtener Usuario Actual
- **Ruta:** `GET /api/auth/me`
- **Autenticación:** ✅ Requiere Token

**Lo que RECIBES:**
```json
{
  "usuarioId": 1,
  "nombreCompleto": "Juan Pérez",
  "email": "usuario@example.com",
  "fechaRegistro": "2024-01-01T00:00:00Z"
}
```

---

### 1.4 Cambiar Contraseña
- **Ruta:** `POST /api/auth/cambiar-password`
- **Autenticación:** ✅ Requiere Token

**Lo que ENVÍAS:**
```json
{
  "passwordActual": "contraseñaVieja",
  "passwordNueva": "contraseñaNueva123"
}
```

---

### 1.5 Cerrar Sesión
- **Ruta:** `POST /api/auth/logout`
- **Autenticación:** ✅ Requiere Token

**Sin body, solo envía el token en el header.**

---

### 1.6 Recuperar Contraseña (Solicitar código)
- **Ruta:** `POST /api/auth/forgot-password`
- **Autenticación:** ❌ No requiere

**Lo que ENVÍAS:**
```json
{
  "email": "usuario@example.com"
}
```

---

### 1.7 Restablecer Contraseña
- **Ruta:** `POST /api/auth/reset-password`
- **Autenticación:** ❌ No requiere

**Lo que ENVÍAS:**
```json
{
  "token": "ABC123",
  "nuevaPassword": "nuevaContraseña123"
}
```

---

## 💰 2. Categorías de Transacciones

**Controlador:** `api/categoriastransacciones`

### 2.1 Obtener Mis Categorías
- **Ruta:** `GET /api/categoriastransacciones`
- **Autenticación:** ✅ Requiere Token

**Lo que RECIBES (lista):**
```json
[
  {
    "categoriaId": 1,
    "nombre": "Salario",
    "tipo": "Ingreso",
    "color": "#4CAF50",
    "icono": "💼",
    "esSistema": true,
    "usuarioId": null,
    "activa": true
  },
  {
    "categoriaId": 5,
    "nombre": "Comida",
    "tipo": "Gasto",
    "color": "#FF5722",
    "icono": "🍔",
    "esSistema": false,
    "usuarioId": 1,
    "activa": true
  }
]
```

---

### 2.2 Crear Categoría
- **Ruta:** `POST /api/categoriastransacciones`
- **Autenticación:** ✅ Requiere Token

**Lo que ENVÍAS:**
```json
{
  "nombre": "Streaming",
  "tipo": "Gasto",
  "color": "#9C27B0",
  "icono": "📺",
  "activa": true
}
```

**📌 Notas:**
- ❌ **NO envíes** `usuarioId` - se toma automáticamente del token
- ❌ **NO envíes** `esSistema` - siempre es false para categorías creadas por usuarios
- El `color` debe ser formato hexadecimal: `#RRGGBB`

---

### 2.3 Actualizar Categoría
- **Ruta:** `PUT /api/categoriastransacciones/{id}`
- **Autenticación:** ✅ Requiere Token

**Lo que ENVÍAS:**
```json
{
  "nombre": "Streaming Premium",
  "tipo": "Gasto",
  "color": "#9C27B0",
  "icono": "🎬",
  "activa": true
}
```

**⚠️ Importante:** Solo puedes actualizar TUS categorías (no las del sistema).

---

### 2.4 Eliminar Categoría
- **Ruta:** `DELETE /api/categoriastransacciones/{id}`
- **Autenticación:** ✅ Requiere Token

---

### 2.5 Obtener por Tipo
- **Ruta:** `GET /api/categoriastransacciones/tipo/{tipo}`
- **Autenticación:** ✅ Requiere Token
- **Tipos válidos:** `Ingreso` o `Gasto`

---

## 💸 3. Transacciones

**Controlador:** `api/transacciones`

### 3.1 Obtener Mis Transacciones
- **Ruta:** `GET /api/transacciones`
- **Autenticación:** ✅ Requiere Token

**Lo que RECIBES (lista):**
```json
[
  {
    "transaccionId": 1,
    "usuarioId": 1,
    "nombre": "Pago de salario",
    "monto": 1500.00,
    "tipo": "Ingreso",
    "fecha": "2024-03-01",
    "categoria": "Salario",
    "fechaCreacion": "2024-03-01T10:00:00Z"
  },
  {
    "transaccionId": 2,
    "usuarioId": 1,
    "nombre": "Compra supermercado",
    "monto": 50.50,
    "tipo": "Gasto",
    "fecha": "2024-03-05",
    "categoria": "Comida",
    "fechaCreacion": "2024-03-05T18:30:00Z"
  }
]
```

---

### 3.2 Crear Transacción
- **Ruta:** `POST /api/transacciones`
- **Autenticación:** ✅ Requiere Token

**Lo que ENVÍAS:**
```json
{
  "nombre": "Compra de libros",
  "monto": 35.00,
  "tipo": "Gasto",
  "fecha": "2024-03-10",
  "categoria": "Educación"
}
```

**📌 Notas:**
- ❌ **NO envíes** `usuarioId` - se toma del token
- La `fecha` es formato `YYYY-MM-DD`
- `tipo` debe ser: `"Ingreso"` o `"Gasto"`

---

### 3.3 Actualizar Transacción
- **Ruta:** `PUT /api/transacciones/{id}`
- **Autenticación:** ✅ Requiere Token

**Lo que ENVÍAS (todos opcionales):**
```json
{
  "nombre": "Compra de libros técnicos",
  "monto": 40.00,
  "tipo": "Gasto",
  "fecha": "2024-03-11",
  "categoria": "Educación"
}
```

---

### 3.4 Eliminar Transacción
- **Ruta:** `DELETE /api/transacciones/{id}`
- **Autenticación:** ✅ Requiere Token

---

### 3.5 Filtros Útiles

**Por Tipo:**
- `GET /api/transacciones/usuario/{usuarioId}/tipo/{tipo}`
- Ejemplo: `/api/transacciones/usuario/1/tipo/Gasto`

**Por Categoría:**
- `GET /api/transacciones/usuario/{usuarioId}/categoria/{categoria}`
- Ejemplo: `/api/transacciones/usuario/1/categoria/Comida`

**Por Período:**
- `GET /api/transacciones/usuario/{usuarioId}/periodo?fechaInicio=2024-01-01&fechaFin=2024-03-31`

---

## 🎯 4. Metas de Ahorro

**Controlador:** `api/metasahorro`

### 4.1 Obtener Mis Metas
- **Ruta:** `GET /api/metasahorro`
- **Autenticación:** ✅ Requiere Token

**Lo que RECIBES (lista):**
```json
[
  {
    "metaId": 1,
    "usuarioId": 1,
    "nombre": "Vacaciones 2024",
    "descripcion": "Viaje a la playa",
    "categoria": "vacaciones",
    "montoObjetivo": 2000.00,
    "montoActual": 500.00,
    "fechaInicio": "2024-01-01",
    "fechaObjetivo": "2024-12-31",
    "fechaCompletada": null,
    "color": "#FF9800",
    "icono": "🏖️",
    "prioridad": 8,
    "activa": true,
    "completada": false,
    "porcentajeAvance": 25.00
  }
]
```

---

### 4.2 Crear Meta
- **Ruta:** `POST /api/metasahorro`
- **Autenticación:** ✅ Requiere Token

**Lo que ENVÍAS:**
```json
{
  "nombre": "Nuevo Celular",
  "descripcion": "iPhone 15 Pro",
  "categoria": "tecnologia",
  "montoObjetivo": 1200.00,
  "montoActual": 0,
  "fechaObjetivo": "2024-12-31",
  "color": "#2196F3",
  "icono": "📱",
  "prioridad": 7
}
```

**📌 Categorías válidas:**
- `ahorro`, `vivienda`, `vacaciones`, `educacion`, `vehiculo`, `emergencia`, `tecnologia`, `otro`

**📌 Notas:**
- ❌ **NO envíes** `usuarioId` - se toma del token
- `prioridad` va de 0 a 10
- `porcentajeAvance` se calcula automáticamente

---

### 4.3 Actualizar Meta
- **Ruta:** `PUT /api/metasahorro/{id}`
- **Autenticación:** ✅ Requiere Token

**Lo que ENVÍAS (todos los campos):**
```json
{
  "nombre": "Nuevo Celular",
  "descripcion": "iPhone 15 Pro Max",
  "categoria": "tecnologia",
  "montoObjetivo": 1500.00,
  "montoActual": 300.00,
  "fechaObjetivo": "2024-12-31",
  "color": "#2196F3",
  "icono": "📱",
  "prioridad": 9,
  "activa": true,
  "completada": false
}
```

---

### 4.4 Eliminar Meta
- **Ruta:** `DELETE /api/metasahorro/{id}`
- **Autenticación:** ✅ Requiere Token

---

### 4.5 Filtros Útiles

**Solo Metas Activas:**
- `GET /api/metasahorro/usuario/{usuarioId}/activas`

**Solo Metas Completadas:**
- `GET /api/metasahorro/usuario/{usuarioId}/completadas`

---

## 📊 5. Estadísticas Mensuales

**Controlador:** `api/estadisticasmensuales`

### 5.1 Obtener Mis Estadísticas
- **Ruta:** `GET /api/estadisticasmensuales`
- **Autenticación:** ✅ Requiere Token

**Lo que RECIBES (lista):**
```json
[
  {
    "estadisticaId": 1,
    "usuarioId": 1,
    "anio": 2024,
    "mes": 3,
    "totalIngresos": 2000.00,
    "totalGastos": 1200.00,
    "balance": 800.00,
    "numTransacciones": 25,
    "categoriaMayorGastoId": 5,
    "categoriaMayorGastoNombre": "Comida",
    "montoMayorGasto": 400.00,
    "fechaCalculo": "2024-03-31T23:59:59Z"
  }
]
```

---

### 5.2 Crear Estadística
- **Ruta:** `POST /api/estadisticasmensuales`
- **Autenticación:** ✅ Requiere Token

**Lo que ENVÍAS:**
```json
{
  "usuarioId": 1,
  "anio": 2024,
  "mes": 3,
  "totalIngresos": 2000.00,
  "totalGastos": 1200.00,
  "numTransacciones": 25,
  "categoriaMayorGastoId": 5,
  "montoMayorGasto": 400.00
}
```

**📌 Nota:** `balance` se calcula automáticamente (ingresos - gastos)

---

### 5.3 Obtener por Período
- **Ruta:** `GET /api/estadisticasmensuales/usuario/{usuarioId}/periodo/{anio}/{mes}`
- **Autenticación:** ✅ Requiere Token
- **Ejemplo:** `/api/estadisticasmensuales/usuario/1/periodo/2024/3`

---

## 📄 6. Reportes

**Controlador:** `api/reportes`

### 6.1 Obtener Mis Reportes
- **Ruta:** `GET /api/reportes`
- **Autenticación:** ✅ Requiere Token

**Lo que RECIBES (lista):**
```json
[
  {
    "reporteId": 1,
    "usuarioId": 1,
    "nombre": "Reporte Mensual Marzo",
    "tipoReporte": "mensual",
    "descripcion": "Resumen de gastos e ingresos",
    "estado": "Completado",
    "fechaGeneracion": "2024-03-31T10:00:00Z"
  }
]
```

---

### 6.2 Crear Reporte
- **Ruta:** `POST /api/reportes`
- **Autenticación:** ✅ Requiere Token

**Lo que ENVÍAS:**
```json
{
  "nombre": "Reporte Anual 2024",
  "tipoReporte": "anual",
  "descripcion": "Análisis completo del año",
  "usuarioId": 1
}
```

**📌 Tipos de reporte:** `mensual`, `trimestral`, `anual`, `personalizado`

---

## 👤 7. Usuarios

**Controlador:** `api/usuarios`

### 7.1 Obtener Usuario por ID
- **Ruta:** `GET /api/usuarios/{id}`
- **Autenticación:** ✅ Requiere Token

**Lo que RECIBES:**
```json
{
  "usuarioId": 1,
  "nombreCompleto": "Juan Pérez",
  "email": "usuario@example.com",
  "fechaRegistro": "2024-01-01T00:00:00Z"
}
```

**⚠️ Nota:** Solo puedes ver TU propio perfil.

---

### 7.2 Actualizar Usuario
- **Ruta:** `PUT /api/usuarios/{id}`
- **Autenticación:** ✅ Requiere Token

**Lo que ENVÍAS:**
```json
{
  "nombreCompleto": "Juan Carlos Pérez",
  "email": "nuevo@example.com",
  "password": "nuevaContraseña"
}
```

**📌 Nota:** `password` es opcional (solo si quieres cambiarla).

---

## 📧 8. Mensajes de Contacto

**Controlador:** `api/mensajescontacto`

### 8.1 Enviar Mensaje
- **Ruta:** `POST /api/mensajescontacto`
- **Autenticación:** ❌ No requiere (público)

**Lo que ENVÍAS:**
```json
{
  "nombre": "María García",
  "email": "maria@example.com",
  "mensaje": "Tengo una consulta sobre mi cuenta..."
}
```

---

### 8.2 Obtener Mensajes (Solo Admin)
- **Ruta:** `GET /api/mensajescontacto`
- **Autenticación:** ✅ Requiere Token (solo administradores)

---

## 📋 9. Templates

**Controlador:** `api/templates`

Los templates son transacciones predefinidas que puedes reutilizar.

### 9.1 Obtener Mis Templates
- **Ruta:** `GET /api/templates`
- **Autenticación:** ✅ Requiere Token

**Lo que RECIBES (lista):**
```json
[
  {
    "templateId": 1,
    "usuarioId": 1,
    "categoriaId": 5,
    "nombre": "Pago Netflix",
    "monto": 15.99,
    "descripcion": "Suscripción mensual",
    "frecuencia": "mensual",
    "activo": true,
    "fechaCreacion": "2024-01-01T00:00:00Z"
  }
]
```

---

### 9.2 Crear Template
- **Ruta:** `POST /api/templates`
- **Autenticación:** ✅ Requiere Token

**Lo que ENVÍAS:**
```json
{
  "usuarioId": 1,
  "categoriaId": 5,
  "nombre": "Pago Spotify",
  "monto": 9.99,
  "descripcion": "Suscripción Premium",
  "frecuencia": "mensual"
}
```

**📌 Frecuencias válidas:** `diaria`, `semanal`, `mensual`, `anual`, `unica`

---

## ⚙️ 10. Configuraciones de Usuario

**Controlador:** `api/configuracionesusuario`

### 10.1 Obtener Mis Configuraciones
- **Ruta:** `GET /api/configuracionesusuario`
- **Autenticación:** ✅ Requiere Token

**Lo que RECIBES:**
```json
{
  "configuracionId": 1,
  "usuarioId": 1,
  "moneda": "USD",
  "idiomaPreferido": "es",
  "tema": "oscuro",
  "notificacionesEmail": true,
  "notificacionesPush": false,
  "formatoFecha": "DD/MM/YYYY",
  "zonaHoraria": "America/Mexico_City"
}
```

---

### 10.2 Actualizar Configuraciones
- **Ruta:** `PUT /api/configuracionesusuario/{id}`
- **Autenticación:** ✅ Requiere Token

**Lo que ENVÍAS:**
```json
{
  "moneda": "MXN",
  "idiomaPreferido": "es",
  "tema": "claro",
  "notificacionesEmail": true,
  "notificacionesPush": true,
  "formatoFecha": "DD/MM/YYYY",
  "zonaHoraria": "America/Mexico_City"
}
```

---

## ❌ Datos Innecesarios que Puedes Omitir

### Para CREAR recursos (POST):

1. **NO envíes estos campos** - Se generan automáticamente:
   - ❌ `fechaCreacion`
   - ❌ `fechaRegistro`
   - ❌ `fechaGeneracion`
   - ❌ IDs (`usuarioId`, `transaccionId`, etc.)

2. **NO envíes** `usuarioId` cuando estás autenticado:
   - Se toma automáticamente del token JWT
   - Aplica para: Transacciones, Categorías, Metas, Templates

3. **NO envíes** campos calculados:
   - ❌ `balance` (en estadísticas)
   - ❌ `porcentajeAvance` (en metas)

### Para ACTUALIZAR recursos (PUT):

1. **NO envíes** campos que no puedes cambiar:
   - ❌ IDs
   - ❌ `fechaCreacion`
   - ❌ `usuarioId`
   - ❌ `esSistema` (en categorías)

### Campos Opcionales que SÍ puedes omitir si no los usas:

- `descripcion` (en reportes, metas)
- `color` e `icono` (en categorías, metas)
- `categoria` (en transacciones)
- `fechaObjetivo` (en metas)
- `prioridad` (en metas)

---

## 🔒 Cómo Autenticarte

En **cada petición** (excepto login y register), debes enviar el token en el header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Ejemplo con JavaScript (Fetch):

```javascript
const token = localStorage.getItem('token');

fetch('http://localhost:5000/api/transacciones', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
})
.then(response => response.json())
.then(data => console.log(data));
```

---

## 📝 Resumen Rápido

### Datos que NUNCA envías al backend:
- ✅ IDs generados automáticamente
- ✅ Fechas de creación/registro
- ✅ Campos calculados (balance, porcentaje)
- ✅ `usuarioId` cuando estás autenticado

### Datos que SIEMPRE debes enviar:
- ✅ Token JWT (excepto login/register)
- ✅ Campos marcados como `[Required]`

### Respuestas útiles del backend:
- ✅ Objetos completos con todos los campos
- ✅ Mensajes de error claros
- ✅ Códigos HTTP estándar (200, 401, 404, 500)

---

## 🎨 Formato de Colores

Los colores deben ser hexadecimales:
- ✅ `#FF5722` (Correcto)
- ❌ `red` (Incorrecto)
- ❌ `rgb(255, 87, 34)` (Incorrecto)

---

## 📅 Formato de Fechas

- **Fechas simples:** `YYYY-MM-DD` (ej: `2024-03-15`)
- **Fechas con hora (respuestas):** ISO 8601 (ej: `2024-03-15T10:30:00Z`)

---

## ✅ Validaciones Importantes

1. **Email:** Debe ser válido (ej: `usuario@dominio.com`)
2. **Contraseña:** Mínimo 6 caracteres
3. **Monto:** Debe ser mayor a 0
4. **Tipo (transacción):** Solo `"Ingreso"` o `"Gasto"`
5. **Mes:** Entre 1 y 12
6. **Año:** Entre 2020 y 2100
7. **Prioridad (metas):** Entre 0 y 10

---

Esta guía cubre los puntos esenciales para consumir el backend desde tu frontend. ¡Enfócate en enviar solo lo necesario y el backend hará el resto! 🚀
