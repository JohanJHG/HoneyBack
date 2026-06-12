from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession
from typing import List
from app.auth import require_admin_or_superadmin
from app.database import get_db
from app.schemas.analiticas_schemas import AnaliticasKpis, RetentionPoint, UsageSplit, ChurnedUser
from app.services import analiticas_service

router = APIRouter(prefix="/analytics/analiticas", tags=["analiticas"])


@router.get("/kpis", response_model=AnaliticasKpis)
async def analiticas_kpis(
    db: AsyncSession = Depends(get_db),
    _: dict = Depends(require_admin_or_superadmin),
):
    return await analiticas_service.get_analiticas_kpis(db)


@router.get("/retention", response_model=List[RetentionPoint])
async def retention(
    months: int = 12,
    db: AsyncSession = Depends(get_db),
    _: dict = Depends(require_admin_or_superadmin),
):
    return await analiticas_service.get_retention(db, months)


@router.get("/usage-split", response_model=UsageSplit)
async def usage_split(
    db: AsyncSession = Depends(get_db),
    _: dict = Depends(require_admin_or_superadmin),
):
    return await analiticas_service.get_usage_split(db)


@router.get("/churn-detail", response_model=List[ChurnedUser])
async def churn_detail(
    inactivity_days: int = 30,
    db: AsyncSession = Depends(get_db),
    _: dict = Depends(require_admin_or_superadmin),
):
    return await analiticas_service.get_churn_detail(db, inactivity_days)
