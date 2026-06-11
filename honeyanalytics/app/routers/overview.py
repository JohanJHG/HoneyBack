from fastapi import APIRouter, Depends
from sqlalchemy.ext.asyncio import AsyncSession
from typing import List
from app.auth import require_admin_or_superadmin
from app.database import get_db
from app.schemas.overview_schemas import OverviewKpis, ChartPoint
from app.services import overview_service

router = APIRouter(prefix="/analytics/overview", tags=["overview"])


@router.get("/kpis", response_model=OverviewKpis)
async def overview_kpis(
    db: AsyncSession = Depends(get_db),
    _: dict = Depends(require_admin_or_superadmin),
):
    return await overview_service.get_overview_kpis(db)


@router.get("/user-growth", response_model=List[ChartPoint])
async def user_growth(
    months: int = 6,
    db: AsyncSession = Depends(get_db),
    _: dict = Depends(require_admin_or_superadmin),
):
    return await overview_service.get_user_growth(db, months)


@router.get("/daily-activity", response_model=List[ChartPoint])
async def daily_activity(
    days: int = 7,
    db: AsyncSession = Depends(get_db),
    _: dict = Depends(require_admin_or_superadmin),
):
    return await overview_service.get_daily_activity(db, days)
