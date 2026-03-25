# Analisis de estructura Docker (Frontend + Backend)

Fecha: 24-03-2026
Alcance analizado:

- `honeybalance-angular` (frontend Angular)
- `HoneyBack` (backend .NET 8 + SQL Server)

Este analisis se realizo revisando `docker-compose.yml`, `Dockerfile`, `.dockerignore`, `nginx.conf`, `Program.cs`, `appsettings*.json` y configuracion del frontend.

## Resumen ejecutivo

La causa principal de que en unos equipos funcione y en otros no es el acoplamiento a entorno local:

- Rutas absolutas de Windows en Compose.
- URLs hardcodeadas a `localhost` en frontend/interceptor.
- Mezcla de configuracion de desarrollo con intenciones de produccion.
- Estrategia de variables de entorno incompleta para despliegue compartible.

El backend y el frontend tienen buenas bases (multi-stage build, usuario no-root, healthcheck), pero faltan patrones de portabilidad y separacion de entornos.

## Hallazgos principales (priorizados)

### 1) Critico: ruta absoluta para construir frontend en Compose

Archivo: `HoneyBack/docker-compose.yml`

Se usa:

- `context: ../../../Desktop/honeybalance/honeybalance-angular`

Problema:

- Solo funciona en una ruta concreta de una maquina concreta.
- En otro equipo, el path no existira.

Impacto:

- Build falla aunque el codigo sea correcto.

Recomendacion:

- Unificar frontend y backend bajo un mismo repo raiz o usar rutas relativas portables entre carpetas del proyecto.
- Alternativa: mantener repos separados y usar un compose en un tercer repo de infraestructura que clone ambos en rutas fijas.

### 2) Critico: frontend acoplado a localhost

Archivos:

- `honeybalance-angular/src/environments/environment.ts`
- `honeybalance-angular/src/environments/environment.prod.ts`
- `honeybalance-angular/src/app/core/interceptors/auth.interceptor.ts`

Problema:

- `apiBase` apunta a `http://localhost:5291/api` incluso en `environment.prod.ts`.
- El interceptor solo agrega token si URL contiene `localhost:5291`.

Impacto:

- En contenedor compartido o produccion, el host cambia y deja de autenticar correctamente.

Recomendacion:

- El interceptor debe basarse en `environment.apiBase`, no en `localhost`.
- En frontend containerizado, usar API por path relativo (`/api`) y resolver con reverse proxy (Nginx) para evitar CORS y hosts hardcodeados.

### 3) Alto: Compose actual es de desarrollo, no de produccion

Archivo: `HoneyBack/docker-compose.yml`

Problemas:

- Exposicion de puertos de DB y API orientada a local (`1434`, `5291`, `4200`).
- `deploy.resources` en Compose clasico suele ignorarse fuera de Swarm.
- Uso de `container_name` reduce flexibilidad para escalar y puede causar conflictos entre proyectos.
- Falta estrategia clara de perfiles/archivos separados dev/prod.

Recomendacion:

- Separar:
  - `compose.yml` base (sin puertos publicos innecesarios)
  - `compose.dev.yml` (puertos, hot reload, debug)
  - `compose.prod.yml` (sin DB expuesta, reverse proxy, secretos externos)
- Eliminar `container_name` salvo necesidad muy puntual.

### 4) Alto: gestion de secretos parcialmente correcta pero incompleta

Archivos:

- `HoneyBack/.env.example`
- `HoneyBack/.gitignore`

Estado actual positivo:

- Existe `.env.example`.
- `.env` esta ignorado en Git.

Riesgo:

- Secretos criticos (`SA_PASSWORD`, `JWT_KEY`) dependen de `.env` local sin pipeline estandar de inyeccion.
- No hay estrategia formal para produccion (secret manager / vault / CI variables protegidas).

Recomendacion:

- Mantener `.env.example` sin secretos reales.
- Inyectar secretos en produccion via plataforma (Azure App Service/Container Apps/K8s Secrets/Key Vault/CI secrets).
- Documentar obligatorios y validarlos al arranque (fail-fast con mensajes claros).

### 5) Medio: CORS y networking no alineados con arquitectura containerizada

Archivos:

- `HoneyBack/Program.cs`
- `HoneyBack/appsettings.Docker.json`
- `HoneyBack/docker-compose.yml`

Problema:

- CORS esta orientado a localhost fijo.
- Si frontend y backend viven bajo el mismo dominio/reverse proxy, CORS deja de ser necesario para navegacion normal.

Recomendacion:

- Para produccion: frontend y API bajo mismo host con Nginx/Ingress, evitando CORS cruzado.
- Para desarrollo: permitir origenes parametrizados via variable de entorno (lista), no hardcode fijo.

### 6) Medio: Swagger habilitado en todos los entornos

Archivo: `HoneyBack/Program.cs`

Problema:

- Exponer Swagger sin restriccion en produccion aumenta superficie de descubrimiento.

Recomendacion:

- Restringir Swagger por entorno o protegerlo con autenticacion/red privada.

### 7) Medio: migraciones automaticas al arranque

Archivo: `HoneyBack/Program.cs`

Problema:

- Aplicar migraciones al iniciar la API funciona en local, pero en produccion puede generar condiciones de carrera al escalar replicas.

Recomendacion:

- Ejecutar migraciones como paso de release (job/command separado) antes de escalar servicio.

### 8) Bajo/Medio: salud de SQL con dependencia de tooling interno

Archivo: `HoneyBack/docker-compose.yml`

Problema:

- El healthcheck usa `sqlcmd` en ruta especifica; puede variar segun imagen/tag o cambios futuros.

Recomendacion:

- Verificar en CI que ese healthcheck sigue siendo valido para la version fijada.
- Fijar tag de imagen mas estable en vez de `2022-latest` para evitar sorpresas.

## Buenas practicas ya presentes

- Dockerfiles multi-stage en frontend y backend.
- Backend ejecuta como usuario no-root.
- `.dockerignore` en ambos proyectos.
- Healthcheck para API y SQL.
- Variables sensibles extraidas a entorno (no hardcode directo en codigo).

## Diferencia clave: contenedor local vs contenedor de produccion

En local normalmente se tolera:

- puertos abiertos al host,
- credenciales de prueba,
- rutas locales,
- logs verbosos.

En produccion se exige:

- configuracion inyectada externamente,
- secretos gestionados fuera del repo,
- imagenes inmutables y reproducibles,
- red privada interna entre servicios,
- observabilidad y politicas de seguridad.

## Estructura recomendada para compartir correctamente

### Opcion A (recomendada): carpeta de infraestructura comun

Propuesta:

- `/infra/compose/compose.yml` (base)
- `/infra/compose/compose.dev.yml`
- `/infra/compose/compose.prod.yml`
- `/infra/env/.env.example`

Reglas:

- Sin rutas absolutas.
- Solo paths relativos al repo.
- Config por entorno en archivos override.

### Opcion B: mantener repos separados con contrato claro

- Crear un repo "deployment" que consume imagenes publicadas (`image:`) en lugar de `build:` local.
- Front y back se construyen en CI, se publican en registry y compose de despliegue solo referencia tags versionados.

## Acciones concretas recomendadas (orden sugerido)

1. Eliminar ruta absoluta del build del frontend en `docker-compose.yml`.
2. Corregir frontend para no depender de `localhost:5291` (environment + interceptor).
3. Separar Compose dev/prod y remover `container_name`.
4. Definir estrategia de secretos para produccion (CI/CD + secret store).
5. Ajustar CORS a variables parametrizadas y/o arquitectura same-origin con proxy.
6. Proteger Swagger en produccion.
7. Mover migraciones EF a job de despliegue.
8. Pinnear imagenes (`node`, `nginx`, `dotnet`, `mssql`) a versiones no `latest`.

## Checklist rapido de portabilidad

- [ ] Cero rutas absolutas en Compose/Docker.
- [ ] Cero referencias hardcodeadas a localhost en build de produccion.
- [ ] `.env.example` completo y actualizado.
- [ ] `.env` y secretos fuera de Git.
- [ ] Compose separado por entorno.
- [ ] Imagenes construidas en CI y publicadas en registry.
- [ ] Variables obligatorias validadas al inicio.
- [ ] DB no expuesta publicamente en produccion.

---

Si quieres, en un siguiente paso puedo generar una propuesta concreta de:

- `compose.yml` base,
- `compose.dev.yml` para tu flujo local,
- `compose.prod.yml` listo para despliegue compartible,
- y ajustes minimos de frontend/backend para eliminar el acoplamiento a localhost.
