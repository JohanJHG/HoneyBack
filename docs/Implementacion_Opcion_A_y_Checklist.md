# Implementacion ejecutada: Opcion A + checklist de portabilidad

Fecha: 24-03-2026

Este documento describe exactamente que se cambio, por que se cambio y como usarlo en desarrollo continuo y en produccion.

## Objetivo aplicado

Se ejecuto la Opcion A propuesta:

- Estructura de infraestructura separada por entorno.
- Configuracion portable para compartir contenedores entre equipos.
- Desacoplar runtime de localhost fijo.

Tambien se ejecuto el checklist recomendado en lo que es implementable dentro del repositorio.

## Cambios realizados (archivo por archivo)

### 1) Nueva estructura de infraestructura

Archivos nuevos:

- `infra/compose/compose.yml`
- `infra/compose/compose.dev.yml`
- `infra/compose/compose.prod.yml`
- `infra/env/.env.example`

Que hace cada uno:

- `compose.yml`: base comun para todos los entornos (servicios, red, volumen, healthchecks, variables comunes).
- `compose.dev.yml`: override para desarrollo (puertos abiertos, build local de API y frontend).
- `compose.prod.yml`: override para produccion (imagenes versionadas, Swagger off, migraciones automáticas off, sin puertos de DB/API al host).
- `infra/env/.env.example`: contrato de variables para todos los equipos.

Justificacion:

- Evita mezclar necesidades de desarrollo con produccion en un unico compose.
- Permite evolucionar el proyecto en desarrollo constante sin perder estandar de despliegue.

### 2) Docker Compose principal endurecido para desarrollo compartible

Archivo modificado:

- `docker-compose.yml`

Cambios clave:

- Eliminada ruta absoluta al frontend.
- Eliminado `container_name` para evitar conflictos entre proyectos/entornos.
- Eliminados bloques `deploy.resources` (no efectivos en Compose clasico fuera de Swarm).
- CORS y flags movidos a variables de entorno.
- Build del frontend ahora usa variables:
  - `HONEYBALANCE_FRONTEND_CONTEXT`
  - `HONEYBALANCE_FRONTEND_DOCKERFILE`

Justificacion:

- El archivo vuelve portable y parametrizable por equipo.
- Mantiene foco en desarrollo local, pero sin atar el stack a una sola maquina.

### 3) Backend .NET: configuracion segura por entorno

Archivo modificado:

- `Program.cs`

Cambios clave:

- CORS ahora admite `Cors:AllowedOriginsCsv` ademas de arreglo en config.
- Se agrego flag `Database:ApplyMigrationsAtStartup`.
- Se agrego flag `Swagger:Enabled`.
- Defaults inteligentes:
  - En produccion: Swagger y migraciones automaticas desactivadas por defecto.
  - En desarrollo: activadas por defecto.

Justificacion:

- Desarrollo sigue agil (sin pasos manuales extras innecesarios).
- Produccion evita riesgos operativos (exposicion de Swagger y carreras por migraciones al escalar replicas).

### 4) Frontend desacoplado de localhost

Archivos modificados:

- `../Desktop/honeybalance/honeybalance-angular/src/environments/environment.prod.ts`
- `../Desktop/honeybalance/honeybalance-angular/src/app/core/interceptors/auth.interceptor.ts`
- `../Desktop/honeybalance/honeybalance-angular/nginx.conf`

Cambios clave:

- `environment.prod.ts` ahora usa `apiBase: '/api'`.
- Interceptor ya no depende de `localhost:5291`; usa `environment.apiBase`.
- Nginx agrega proxy `location /api/` hacia `honeyback_api`.

Justificacion:

- El frontend containerizado puede hablar con backend sin hardcode de host/puerto.
- Se reduce friccion entre equipos y se evita el classico problema "funciona en mi maquina".

### 5) Contrato de variables de entorno actualizado

Archivo modificado:

- `.env.example`

Cambios clave:

- Nuevas variables JWT completas (`JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_EXPIRES_MINUTES`).
- CORS unificado a `CORS_ALLOWED_ORIGINS`.
- Variables de contexto para build frontend.
- Flags de runtime (`SWAGGER_ENABLED`, `APPLY_MIGRATIONS_AT_STARTUP`, `MSSQL_PID`).

Justificacion:

- Define claramente que necesita el proyecto para funcionar en distintos equipos.

## Validacion ejecutada

Se validaron los compose sin levantar servicios (resolucion de variables y sintaxis):

- Base + dev: OK
- Base + prod: OK
- `docker-compose.yml` principal: OK

## Checklist ejecutado (estado real)

1. Cero rutas absolutas en Compose/Docker.

- Estado: COMPLETADO.

2. Cero referencias hardcodeadas a localhost en build de produccion.

- Estado: COMPLETADO para el frontend de produccion (`/api`) e interceptor.
- Nota: `environment.ts` (desarrollo) mantiene localhost, lo cual es correcto para flujo local no containerizado.

3. `.env.example` completo y actualizado.

- Estado: COMPLETADO.

4. `.env` y secretos fuera de Git.

- Estado: COMPLETADO (ya estaba ignorado y se mantiene).

5. Compose separado por entorno.

- Estado: COMPLETADO (`compose.yml`, `compose.dev.yml`, `compose.prod.yml`).

6. Imagenes construidas en CI y publicadas en registry.

- Estado: PENDIENTE (requiere pipeline CI/CD externo al codigo).

7. Variables obligatorias validadas al inicio.

- Estado: PARCIALMENTE COMPLETADO.
- Implementado: flags y manejo de JWT/CORS mejorados.
- Pendiente recomendado: validacion fail-fast explicita de todas las variables obligatorias en arranque.

8. DB no expuesta publicamente en produccion.

- Estado: COMPLETADO en `compose.prod.yml` (sin mapeo de puerto SQL al host).

## Como usar desde ahora

### Desarrollo continuo (recomendado para equipo)

1. Copiar variables de ejemplo:

- `infra/env/.env.example` -> `infra/env/.env`

2. Ajustar en `.env`:

- `HONEYBALANCE_FRONTEND_CONTEXT` con la ruta local del repo frontend en cada equipo.

3. Levantar stack dev:

- `docker compose --env-file infra/env/.env -f infra/compose/compose.yml -f infra/compose/compose.dev.yml up --build`

Nota:

- Para desarrollo rapido de UI, se puede seguir usando `ng serve` fuera de contenedor y dejar API+SQL en compose.

### Produccion (explicito)

1. Publicar imagenes versionadas en registry:

- `HONEYBACK_API_IMAGE`
- `HONEYBALANCE_WEB_IMAGE`

2. Definir secretos fuera de repo (CI secrets, vault, plataforma cloud).

3. Levantar stack prod:

- `docker compose --env-file infra/env/.env -f infra/compose/compose.yml -f infra/compose/compose.prod.yml up -d`

4. Recomendado adicional para produccion:

- Reverse proxy/Ingress con TLS.
- Ejecutar migraciones como job de despliegue (no al boot de API).
- Telemetria y alertas de healthchecks.

## Recomendaciones explicitas para el siguiente paso

Para que en produccion funcione bien y en desarrollo no frene al equipo:

- Mantener dos ritmos claros:
  - Dev: mas flexibilidad, logs verbosos, puertos abiertos.
  - Prod: imagenes inmutables, secretos externos, minima exposicion de puertos.

- Mover construccion de imagenes a CI:
  - Evita diferencias por Docker local y versiones de dependencias.

- Definir una politica de versionado de imagenes:
  - Nunca usar tags ambiguos para releases criticos.

- Consolidar documentacion de onboarding:
  - Un unico documento con variables minimas, comandos y troubleshooting para nuevos equipos.
