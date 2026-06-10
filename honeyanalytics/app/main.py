from contextlib import asynccontextmanager
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.routers import overview, analiticas
from app.database import engine


@asynccontextmanager
async def lifespan(app: FastAPI):
    yield
    await engine.dispose()


app = FastAPI(
    title="HoneyBalance Analytics",
    version="1.0.0",
    docs_url="/analytics/docs",
    openapi_url="/analytics/openapi.json",
    lifespan=lifespan,
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["GET"],
    allow_headers=["Authorization", "Content-Type"],
)

app.include_router(overview.router)
app.include_router(analiticas.router)


@app.get("/analytics/health", tags=["health"])
async def health():
    return {"status": "ok"}
