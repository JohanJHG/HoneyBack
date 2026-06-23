# Guia de configuracion local — HoneyBalance

Esta guia explica como levantar el proyecto completo desde cero en una maquina nueva. No se requiere experiencia previa con las tecnologias del proyecto; solo se necesita seguir los pasos en orden.

----

## Indice

1. [Requisitos previos](#1-requisitos-previos)
2. [Repositorios necesarios](#2-repositorios-necesarios)
3. [Configurar el archivo de variables de entorno](#3-configurar-el-archivo-de-variables-de-entorno)
4. [Levantar los servicios](#4-levantar-los-servicios)
5. [Poblar la base de datos con datos de prueba](#5-poblar-la-base-de-datos-con-datos-de-prueba)
6. [Verificar que todo funciona](#6-verificar-que-todo-funciona)
7. [Credenciales de prueba](#7-credenciales-de-prueba)
8. [Detener los servicios](#8-detener-los-servicios)
9. [Solucion de problemas comunes](#9-solucion-de-problemas-comunes)

---

## 1. Requisitos previos

El proyecto corre completamente dentro de contenedores Docker. La unica herramienta que se necesita instalar es Docker Desktop.

### Docker Desktop

Descarga e instala Docker Desktop segun tu sistema operativo:

- **Windows**: https://docs.docker.com/desktop/install/windows-install/
- **macOS**: https://docs.docker.com/desktop/install/mac-install/
- **Linux**: https://docs.docker.com/desktop/install/linux-install/

Una vez instalado, abre Docker Desktop y asegurate de que este en ejecucion (el icono de la ballena debe aparecer en la barra de tareas o menu bar).

### Git

Verifica que tienes Git instalado abriendo una terminal y ejecutando:

```bash
git --version
```

Si el comando no se reconoce, descarga Git desde https://git-scm.com/downloads e instalalo con las opciones por defecto.

### Verificar que Docker funciona

Abre una terminal y ejecuta:

```bash
docker --version
docker compose version
```

Ambos comandos deben mostrar un numero de version sin errores. Si `docker compose` no se reconoce como subcomando, actualiza Docker Desktop a la version mas reciente.

---

## 2. Repositorios necesarios

El proyecto esta dividido en dos repositorios: el backend (que incluye la API y el microservicio de analiticas) y el frontend en Angular. Ambos son necesarios para levantar la aplicacion completa.

### Clonar los repositorios

Crea una carpeta de trabajo donde quieras alojar el proyecto y ejecuta los siguientes comandos.

```bash
# Backend (API .NET + microservicio de analiticas)
git clone <URL_DEL_REPOSITORIO_BACKEND>

# Frontend (aplicacion Angular)
git clone <URL_DEL_REPOSITORIO_FRONTEND>
```

Resultado esperado: dos carpetas en tu directorio de trabajo, una para cada repositorio.

### Estructura esperada

Despues de clonar, la estructura debe verse similar a esto:

```
mi-carpeta-de-trabajo/
  HoneyBack/                  <- repositorio del backend
    infra/
      compose/
        compose.local.yml
      env/
        .env.example
    honeyanalytics/           <- microservicio de analiticas (incluido en el mismo repo)
    Dockerfile
    HoneyBack.csproj
    ...
  honeybalance-angular/       <- repositorio del frontend
    Dockerfile
    src/
    ...
```

> El nombre de las carpetas puede variar segun como hayas nombrado los directorios al clonar. Lo importante es saber la ruta absoluta de cada uno.

---

## 3. Configurar el archivo de variables de entorno

Se te entregara un archivo `.env` con los valores reales del proyecto. Este archivo contiene claves secretas y configuracion de acceso que no se publican en el repositorio.

### Paso 3.1 — Ubicar el archivo de ejemplo

Dentro del repositorio del backend, existe un archivo de referencia en:

```
HoneyBack/infra/env/.env.example
```

Este archivo muestra todas las variables que el proyecto necesita.

### Paso 3.2 — Colocar el .env recibido

Copia el archivo `.env` que se te entrego y pégalo en la siguiente ruta dentro del repositorio del backend:

```
HoneyBack/infra/env/.env
```

El archivo debe quedar en esa ubicacion exacta. Si la carpeta `env` no existiera, creala manualmente.

### Paso 3.3 — Configurar la ruta del frontend

Abre el archivo `.env` con cualquier editor de texto (Bloc de notas, VS Code, etc.) y busca la variable `HONEYBALANCE_FRONTEND_CONTEXT`. Debes asignarle la ruta absoluta donde clonaste el repositorio del frontend en tu maquina.

**Ejemplo en Windows:**

```
HONEYBALANCE_FRONTEND_CONTEXT=C:/Users/TuUsuario/Desktop/honeybalance-angular
```

**Ejemplo en macOS:**

```
HONEYBALANCE_FRONTEND_CONTEXT=/Users/TuUsuario/Desktop/honeybalance-angular
```

**Ejemplo en Linux:**

```
HONEYBALANCE_FRONTEND_CONTEXT=/home/TuUsuario/honeybalance-angular
```

Notas importantes sobre esta ruta:

- En Windows usa siempre barras inclinadas hacia adelante (`/`), no barras invertidas (`\`).
- La ruta debe apuntar a la raiz del repositorio del frontend, es decir, la carpeta que contiene el archivo `Dockerfile` del frontend.
- No pongas comillas alrededor de la ruta.

### Paso 3.4 — Verificar que el archivo es correcto

El archivo `.env` debe tener al menos estas variables definidas (los valores proporcionados):

```
JWT_KEY=<valor proporcionado>
HONEYBALANCE_FRONTEND_CONTEXT=<tu ruta local al frontend>
HONEYBALANCE_FRONTEND_DOCKERFILE=Dockerfile
```

Las variables de base de datos (`API_DATABASE_URL`, `ANALYTICS_DATABASE_URL`) **no son necesarias** para el setup local porque el archivo `compose.local.yml` crea una base de datos PostgreSQL automaticamente en Docker con credenciales predefinidas. Si el `.env` que recibiste las trae, puedes dejarlas o borrarlas; no afectan el setup local.

---

## 4. Levantar los servicios

Con Docker en ejecucion y el `.env` configurado, es momento de construir y arrancar todos los servicios.

### Paso 4.1 — Abrir una terminal en la raiz del backend

Navega hasta la carpeta raiz del repositorio del backend. Todos los comandos de Docker se ejecutan desde ahi.

```bash
cd HoneyBack
```

### Paso 4.2 — Construir e iniciar todos los servicios

Ejecuta el siguiente comando:

```bash
docker compose -f infra/compose/compose.local.yml --env-file infra/env/.env up --build
```

Este comando hace lo siguiente:

1. Descarga las imagenes base necesarias (PostgreSQL, .NET runtime, Python runtime). Esto puede tardar varios minutos la primera vez segun la velocidad de tu conexion a internet.
2. Construye las imagenes del backend y del frontend desde el codigo fuente.
3. Inicia los contenedores en el orden correcto: primero PostgreSQL, luego la API y el microservicio de analiticas (que dependen de la BD), y finalmente el frontend.
4. La API aplica automaticamente las migraciones de base de datos al arrancar. Esto crea todas las tablas necesarias sin que tengas que hacer nada adicional.

La terminal mostrara los logs de todos los servicios en tiempo real. Los mensajes importantes que indican que todo esta correcto son:

```
postgres     | database system is ready to accept connections
honeyback_api| Application started. Press Ctrl+C to shut down.
honeyanalytics| Application startup complete.
```

El proceso completo puede tardar entre 3 y 8 minutos la primera vez. En ejecuciones siguientes (sin `--build`) tarda entre 30 y 60 segundos.

### Servicios que quedan en ejecucion

| Servicio | Puerto local | Descripcion |
|---|---|---|
| PostgreSQL | 5432 | Base de datos local |
| API .NET | 5291 | Backend principal |
| Frontend Angular | 4200 | Aplicacion web |
| Microservicio analiticas | 8000 | API de analiticas (Python) |

---

## 5. Poblar la base de datos con datos de prueba

La base de datos arranca vacia (solo con la estructura de tablas). El proyecto incluye un seeder que genera 120 usuarios de prueba con transacciones, metas de ahorro, sesiones e informacion de contacto, todo con datos sinteticos realistas.

### Paso 5.1 — Confirmar que la API esta lista

Antes de ejecutar el seeder, verifica que la API este completamente levantada. Abre un navegador y visita:

```
http://localhost:5291/health
```

Debe responder con un JSON que incluya `"status": "Healthy"`. Si el navegador no carga o responde con error, espera un minuto mas y vuelve a intentarlo.

### Paso 5.2 — Ejecutar el seeder

Abre una **nueva terminal** (sin cerrar la que tiene los logs de Docker) y ejecuta:

```bash
docker compose -f infra/compose/compose.local.yml --env-file infra/env/.env exec honeyback_api dotnet HoneyBack.dll --seed
```

Este comando entra al contenedor de la API que ya esta corriendo y ejecuta el proceso de seed. El proceso finaliza solo cuando termina. En la terminal veras mensajes como:

```
Iniciando seed de datos analytics...
120 usuarios creados
120 configuraciones creadas
120 onboardings creados
...
Seed completado.
```

El seeder es idempotente: si lo ejecutas mas de una vez, detecta que los datos ya existen y sale sin duplicar nada.

---

## 6. Verificar que todo funciona

Una vez que el seed termino, verifica cada parte de la aplicacion:

### Frontend

Abre en el navegador: `http://localhost:4200`

Debe cargar la pagina principal de HoneyBalance. Si ves la pantalla de login o el dashboard, el frontend esta funcionando correctamente.

### API — Swagger UI

Abre en el navegador: `http://localhost:5291/swagger`

Debe cargar la documentacion interactiva de la API con todos los endpoints disponibles. Puedes probar endpoints directamente desde esta interfaz.

### Microservicio de analiticas

Abre en el navegador: `http://localhost:8000/analytics/health`

Debe responder con un JSON indicando que el servicio esta activo.

---

## 7. Credenciales de prueba

El seeder crea 120 usuarios con el siguiente patron de credenciales:

| Campo | Valor |
|---|---|
| Email | `seed_user_001@honeybalance.test` hasta `seed_user_120@honeybalance.test` |
| Contrasena | `Seed1234!` |

Por ejemplo, puedes iniciar sesion con:

- Email: `seed_user_001@honeybalance.test`
- Contrasena: `Seed1234!`

O registrar una cuenta nueva directamente desde el frontend.

---

## 8. Detener los servicios

### Detener sin borrar datos

Presiona `Ctrl + C` en la terminal donde corren los logs, o desde otra terminal ejecuta:

```bash
docker compose -f infra/compose/compose.local.yml --env-file infra/env/.env down
```

La proxima vez que ejecutes `up`, la base de datos conservara todos los datos.

### Detener y borrar todos los datos

Si quieres empezar desde cero (base de datos vacia), agrega el flag `-v`:

```bash
docker compose -f infra/compose/compose.local.yml --env-file infra/env/.env down -v
```

Esto elimina el volumen de PostgreSQL. La proxima vez que levantes los servicios, la base de datos estara vacia y deberas ejecutar el seeder nuevamente.

---

## 9. Solucion de problemas comunes

### Docker Desktop no inicia

Asegurate de que Docker Desktop este abierto y corriendo antes de ejecutar cualquier comando. En Windows, busca el icono de la ballena en la barra de tareas del sistema. En Mac, busca el icono en la barra de menu superior.

### Error: "port is already allocated"

Algun puerto que el proyecto necesita (4200, 5291, 5432 u 8000) esta siendo usado por otra aplicacion en tu maquina. Cierra la aplicacion que lo usa o detiene otros contenedores Docker que esten corriendo con `docker ps` y `docker stop <id>`.

### Error: "HONEYBALANCE_FRONTEND_CONTEXT is not set" o "build context path not found"

La ruta configurada en `HONEYBALANCE_FRONTEND_CONTEXT` no existe o esta mal escrita. Verifica:

1. Que la carpeta del frontend existe en la ruta que pusiste.
2. Que usas barras hacia adelante (`/`) en Windows, no barras invertidas (`\`).
3. Que la ruta es absoluta (empieza desde la raiz, por ejemplo `C:/Users/...`).
4. Que la carpeta contiene el archivo `Dockerfile` del frontend.

### La API tarda mucho en arrancar o muestra errores de conexion

La API espera hasta que PostgreSQL este listo antes de conectarse. En maquinas lentas o en la primera ejecucion, esto puede tardar mas de lo usual. Espera al menos 2-3 minutos y observa los logs. Si ves mensajes de reintento como `"Intentando conectar a la base de datos (intento X/15)..."` es normal; la API reintenta hasta 15 veces.

### El seeder falla con error de conexion

El seeder debe ejecutarse con la API completamente levantada. Verifica que `http://localhost:5291/health` responde `Healthy` antes de ejecutar el comando del seeder.

### La primera construccion es muy lenta

La primera vez que ejecutas `--build`, Docker descarga las imagenes base (.NET SDK, Python, PostgreSQL, Node.js) y compila el codigo. Dependiendo de tu velocidad de internet y hardware, esto puede tomar entre 5 y 15 minutos. Las ejecuciones siguientes son mucho mas rapidas porque Docker cachea las capas ya construidas.

### Quiero ver los logs de un servicio especifico

```bash
# Solo logs de la API
docker compose -f infra/compose/compose.local.yml --env-file infra/env/.env logs -f honeyback_api

# Solo logs del frontend
docker compose -f infra/compose/compose.local.yml --env-file infra/env/.env logs -f honeybalance_web

# Solo logs de postgres
docker compose -f infra/compose/compose.local.yml --env-file infra/env/.env logs -f postgres
```
