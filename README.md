# HoneyBalance

HoneyBalance es una aplicacion web de gestion de finanzas personales orientada al contexto colombiano. Permite a los usuarios registrar ingresos y gastos, establecer metas de ahorro, analizar sus habitos financieros y acceder a herramientas de planificacion personalizadas. La plataforma incluye un panel de administracion completo y un microservicio de analiticas para el seguimiento operativo de la aplicacion.

---

## Que puede hacer

### Para el usuario final

**Transacciones**
El usuario registra cada ingreso o gasto con nombre, monto, categoria y fecha. Puede filtrar por tipo, categoria o rango de fechas, y consultar resumenes mensuales con el balance resultante.

**Metas de ahorro**
Creacion de objetivos de ahorro con monto objetivo, fecha limite, categoria, color e icono. El sistema lleva el seguimiento del progreso y permite marcar metas como completadas o pausarlas segun prioridad.

**Entornos personales**
Modulo de herramientas financieras interactivas. Cada entorno es una calculadora o simulador especializado:

- Presupuesto personal: distribucion de ingresos por categorias de gasto
- Ahorro por meta: simulacion del tiempo necesario para alcanzar un objetivo
- Deudas personales: calculo de cuotas e intereses sobre deudas activas
- Gastos hormiga: identificacion y proyeccion de gastos recurrentes pequenos
- Fondo de emergencia: calculo del colchon financiero recomendado
- Suscripciones: inventario y costo total de servicios recurrentes

**Dashboard personal**
Vista consolidada con el resumen financiero del mes en curso, transacciones recientes y acceso rapido a todas las secciones.

**Configuracion de cuenta**
El usuario puede cambiar su contrasena, ajustar el tema de la interfaz (oscuro o claro), seleccionar su moneda preferida (COP, USD o EUR), y personalizar el formato de fecha y zona horaria.

### Para administradores

**Panel de administracion**
Gestion de usuarios registrados con busqueda, paginacion y filtros por estado. Los administradores pueden ver el detalle de cada cuenta, y los SuperAdmin pueden cambiar el rol de cualquier usuario.

**Gestion de mensajes de contacto**
Bandeja de entrada de mensajes enviados por usuarios y visitantes a traves del formulario de contacto. Permite marcar mensajes como leidos y responder directamente por correo electronico desde el panel.

**Dashboard de analiticas**
Panel exclusivo para administradores con metricas operativas de la plataforma:

- KPIs generales: total de usuarios, usuarios activos, tasa de churn, sesiones promedio
- Crecimiento de usuarios registrados por mes
- Actividad diaria de la plataforma
- Analisis de retencion mensual
- Distribucion de uso por modulos
- Detalle de usuarios en riesgo de abandono

---

## Arquitectura

El proyecto esta compuesto por tres servicios independientes que se comunican entre si:

```
[Navegador]
    |
    | HTTP
    v
[Angular SPA]          Puerto 4200
    |
    | REST / JWT
    v
[API .NET 8]           Puerto 5291
    |           \
    |            \-- JWT compartido
    |                     |
    v                     v
[PostgreSQL]    [Microservicio analiticas - Python FastAPI]   Puerto 8000
```

- El **frontend Angular** consume la API .NET para todas las operaciones del usuario. No tiene acceso directo a la base de datos.
- La **API .NET** maneja autenticacion, logica de negocio y persistencia. Emite los tokens JWT que usa toda la plataforma.
- El **microservicio de analiticas** (Python / FastAPI) ejecuta consultas de lectura sobre la misma base de datos. Reutiliza el JWT emitido por la API principal para validar que el solicitante es administrador.
- La **base de datos PostgreSQL** es compartida por ambos servicios backend.

En produccion, los tres servicios corren en contenedores Docker sobre DigitalOcean, coordinados con Docker Compose. Las migraciones de base de datos se aplican automaticamente al arrancar la API.

---

## Stack tecnologico

| Capa | Tecnologia |
|---|---|
| Frontend | Angular 17 con lazy loading y routing por modulo |
| Backend API | ASP.NET Core 8 (C#), Entity Framework Core 8 |
| Microservicio analiticas | Python 3.12, FastAPI, SQLAlchemy async, Pandas |
| Base de datos | PostgreSQL 16 |
| Autenticacion | JWT Bearer + Refresh Tokens + Google OAuth 2.0 |
| Email transaccional | Resend |
| Contenedores | Docker, Docker Compose |
| CI/CD | GitHub Actions |
| Infraestructura | DigitalOcean (base de datos administrada + droplets) |

---

## Roles de usuario

La plataforma maneja tres niveles de acceso:

| Rol | Acceso |
|---|---|
| **Usuario** | Dashboard personal, transacciones, metas, entornos personales y configuracion de cuenta |
| **Administrador** | Todo lo anterior mas el panel de administracion: gestion de usuarios y mensajes de contacto |
| **SuperAdmin** | Todo lo anterior mas la capacidad de cambiar roles a otros usuarios |

El rol se asigna desde el panel de administracion. Por defecto, todo usuario registrado recibe el rol de Usuario.

---

## Repositorios

El proyecto esta dividido en dos repositorios:

| Repositorio | Contenido |
|---|---|
| `HoneyBack` | API .NET 8, microservicio de analiticas (incluido en la misma repo), Dockerfiles, Docker Compose, migraciones EF Core |
| `honeybalance-angular` | Aplicacion frontend Angular, componentes, servicios, guards y estilos |

---

## Levantar el proyecto en local

La guia completa de instalacion y configuracion paso a paso esta disponible en:

```
HoneyBack/SETUP_LOCAL.md
```

Incluye los requisitos previos, la configuracion del archivo de variables de entorno, los comandos para levantar todos los servicios con Docker Compose y el proceso para poblar la base de datos con datos de prueba.

---

## Seguridad

- Autenticacion con JWT de corta duracion (60 minutos) y Refresh Tokens rotativos
- Rate limiting en endpoints de autenticacion (5 intentos por IP cada 5 minutos) y en la API general (100 solicitudes por minuto)
- Cabeceras de seguridad HTTP aplicadas por middleware en todas las respuestas
- Contrasenas almacenadas con BCrypt
- Acceso a datos estrictamente aislado por usuario: cada endpoint valida que el recurso solicitado pertenece al usuario autenticado
- CORS configurado explicitamente por origen permitido
