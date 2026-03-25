# Guia directa: inconsistencias actuales y como levantar contenedores

Fecha: 24-03-2026

## 1) Estado actual (inconsistencias eliminadas)

1. Compose oficial unico:

- `infra/compose/compose.yml` + overrides (`compose.dev.yml`, `compose.prod.yml`)

Estado:

- Se elimino `docker-compose.yml` de raiz para evitar divergencia.

2. Plantilla oficial unica de variables:

- `infra/env/.env.example`

Estado:

- Se elimino `.env.example` de raiz para evitar confusion.

3. Configuracion CORS en Docker:

- `appsettings.json`
- `appsettings.Production.json`
- variables de entorno Compose (`Cors__AllowedOriginsCsv`)

Riesgo:

- En contenedores, la fuente de verdad es `Cors__AllowedOriginsCsv` por variable de entorno.

4. `appsettings.Docker.json` se simplifico.

Estado:

- Se removio bloque `Cors` de Docker settings para evitar solape con Compose.

## 2) Decision operativa (ya aplicada)

Fuente de verdad para contenedores:

- Usar solo `infra/compose/*` + `infra/env/.env`.

## 3) Que debes configurar ANTES de levantar cualquier cosa

Desde la carpeta `HoneyBack`:

1. Tener Docker Desktop iniciado.
2. Crear archivo de entorno de trabajo:

- Copiar `infra/env/.env.example` a `infra/env/.env`.

3. En `infra/env/.env` configurar minimo:

- `SA_PASSWORD` (fuerte y valida para SQL Server)
- `JWT_KEY` (>= 32 caracteres)
- `HONEYBALANCE_FRONTEND_CONTEXT` (ruta absoluta local al repo `honeybalance-angular`, con `/`)

4. Si usaras modo produccion local:

- Definir `HONEYBACK_API_IMAGE`
- Definir `HONEYBALANCE_WEB_IMAGE`

5. Verificar configuracion sin levantar:

- `docker compose --env-file infra/env/.env -f infra/compose/compose.yml -f infra/compose/compose.dev.yml config`

## 4) Como levantar segun lo que necesites

## A. Desarrollo completo (Frontend + API + SQL)

Comando:

- `docker compose --env-file infra/env/.env -f infra/compose/compose.yml -f infra/compose/compose.dev.yml up --build`

Endpoints:

- Frontend: `http://localhost:4200`
- API: `http://localhost:5291`
- SQL (host): `localhost,1434`

## B. Solo backend + base de datos (sin frontend en contenedor)

Comando:

- `docker compose --env-file infra/env/.env -f infra/compose/compose.yml -f infra/compose/compose.dev.yml up --build sqlserver honeyback_api`

Uso recomendado:

- Cuando trabajas UI con `ng serve` fuera de Docker.

## C. Produccion-like local (imagenes publicadas, sin build local)

Comando:

- `docker compose --env-file infra/env/.env -f infra/compose/compose.yml -f infra/compose/compose.prod.yml up -d`

Notas:

- Usa imagenes ya construidas y versionadas.
- No expone SQL al host.
- Desactiva Swagger y migraciones automaticas por defecto.

## D. Apagar y limpiar

Parar stack:

- `docker compose --env-file infra/env/.env -f infra/compose/compose.yml -f infra/compose/compose.dev.yml down`

Parar y borrar volumen SQL (pierdes datos):

- `docker compose --env-file infra/env/.env -f infra/compose/compose.yml -f infra/compose/compose.dev.yml down -v`

## 5) Reglas directas para no romper entornos

1. Usar exclusivamente `infra/compose/*` para levantar y apagar.
2. Usar exclusivamente `infra/env/.env.example` como plantilla oficial.
3. Para cambios de CORS en Docker, priorizar `Cors__AllowedOriginsCsv` en variables de entorno.
4. Para produccion, no usar `latest` en imagenes de app; usar tags versionados.

## 6) Siguiente mejora opcional

1. Agregar un script `up-dev`, `up-prod`, `down` para no memorizar comandos largos.
