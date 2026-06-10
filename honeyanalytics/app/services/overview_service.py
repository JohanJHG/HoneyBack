from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import text
from typing import List
from app.schemas.overview_schemas import OverviewKpis, ChartPoint


async def get_overview_kpis(db: AsyncSession) -> OverviewKpis:
    result = await db.execute(text("""
        SELECT
            COUNT(*) AS total,
            COUNT(*) FILTER (WHERE "Activo" = true) AS activos
        FROM "Usuarios"
    """))
    row = result.mappings().one()

    contactos = await db.execute(text("""
        SELECT COUNT(*) AS sin_revisar
        FROM "MensajesContacto"
        WHERE "Leido" = false
    """))
    c = contactos.mappings().one()

    return OverviewKpis(
        total_usuarios=row["total"],
        usuarios_activos=row["activos"],
        contactos_sin_revisar=c["sin_revisar"],
    )


async def get_user_growth(db: AsyncSession, months: int = 6) -> List[ChartPoint]:
    result = await db.execute(text(f"""
        SELECT
            TO_CHAR(DATE_TRUNC('month', "FechaRegistro"), 'Mon') AS mes,
            COUNT(*) AS total
        FROM "Usuarios"
        WHERE "FechaRegistro" >= NOW() - INTERVAL '{int(months)} months'
        GROUP BY DATE_TRUNC('month', "FechaRegistro"), mes
        ORDER BY DATE_TRUNC('month', "FechaRegistro")
    """))
    rows = result.mappings().all()
    return [ChartPoint(label=r["mes"], count=r["total"]) for r in rows]


async def get_daily_activity(db: AsyncSession, days: int = 7) -> List[ChartPoint]:
    result = await db.execute(text(f"""
        SELECT
            TO_CHAR(DATE_TRUNC('day', "FechaCreacion"), 'Dy') AS dia,
            COUNT(*) AS total
        FROM "Sesiones"
        WHERE "FechaCreacion" >= NOW() - INTERVAL '{int(days)} days'
        GROUP BY DATE_TRUNC('day', "FechaCreacion"), dia
        ORDER BY DATE_TRUNC('day', "FechaCreacion")
    """))
    rows = result.mappings().all()
    return [ChartPoint(label=r["dia"], count=r["total"]) for r in rows]
