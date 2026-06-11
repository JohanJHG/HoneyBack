import pandas as pd
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import text
from typing import List
from app.schemas.analiticas_schemas import AnaliticasKpis, RetentionPoint, UsageSplit


async def get_analiticas_kpis(db: AsyncSession) -> AnaliticasKpis:
    mau_result = await db.execute(text("""
        SELECT COUNT(DISTINCT "UsuarioID") AS mau
        FROM "Sesiones"
        WHERE "FechaCreacion" >= NOW() - INTERVAL '30 days'
    """))
    mau = mau_result.scalar() or 0

    dau_result = await db.execute(text("""
        SELECT COUNT(DISTINCT "UsuarioID") AS dau
        FROM "Sesiones"
        WHERE "FechaCreacion" >= NOW() - INTERVAL '1 day'
    """))
    dau = dau_result.scalar() or 0

    churn_result = await db.execute(text("""
        WITH prev AS (
            SELECT COUNT(DISTINCT "UsuarioID") AS cnt
            FROM "Sesiones"
            WHERE "FechaCreacion" >= NOW() - INTERVAL '60 days'
              AND "FechaCreacion" <  NOW() - INTERVAL '30 days'
        ),
        curr AS (
            SELECT COUNT(DISTINCT "UsuarioID") AS cnt
            FROM "Sesiones"
            WHERE "FechaCreacion" >= NOW() - INTERVAL '30 days'
        )
        SELECT
            prev.cnt AS prev_cnt,
            curr.cnt AS curr_cnt
        FROM prev, curr
    """))
    churn_row = churn_result.mappings().one_or_none()
    churn_rate = 0.0
    if churn_row and churn_row["prev_cnt"] and churn_row["prev_cnt"] > 0:
        churn_rate = round(
            max(0.0, (churn_row["prev_cnt"] - churn_row["curr_cnt"]) / churn_row["prev_cnt"] * 100),
            1,
        )

    session_result = await db.execute(text("""
        SELECT AVG(
            EXTRACT(EPOCH FROM ("FechaExpiracion" - "FechaCreacion")) / 60.0
        ) AS avg_min
        FROM "Sesiones"
        WHERE "Activa" = false
          AND "FechaExpiracion" > "FechaCreacion"
          AND "FechaCreacion" >= NOW() - INTERVAL '30 days'
    """))
    avg_min = session_result.scalar() or 0.0

    return AnaliticasKpis(
        mau=mau,
        dau=dau,
        churn_rate=churn_rate,
        avg_session_minutes=round(float(avg_min), 1),
    )


async def get_retention(db: AsyncSession, months: int = 12) -> List[RetentionPoint]:
    users_result = await db.execute(text(f"""
        SELECT "UsuarioID", "FechaRegistro"
        FROM "Usuarios"
        WHERE "FechaRegistro" >= NOW() - INTERVAL '{int(months)} months'
    """))
    users_rows = users_result.mappings().all()

    sessions_result = await db.execute(text(f"""
        SELECT DISTINCT "UsuarioID", DATE_TRUNC('month', "FechaCreacion") AS mes
        FROM "Sesiones"
        WHERE "FechaCreacion" >= NOW() - INTERVAL '{int(months)} months'
    """))
    sessions_rows = sessions_result.mappings().all()

    if not users_rows or not sessions_rows:
        return [RetentionPoint(label=f"M{i+1}", value=0.0) for i in range(months)]

    users_df = pd.DataFrame([dict(r) for r in users_rows])
    sessions_df = pd.DataFrame([dict(r) for r in sessions_rows])

    users_df["cohort"] = pd.to_datetime(users_df["FechaRegistro"]).dt.to_period("M")
    sessions_df["mes"] = pd.to_datetime(sessions_df["mes"]).dt.to_period("M")

    merged = sessions_df.merge(
        users_df[["UsuarioID", "cohort"]], on="UsuarioID", how="left"
    )
    merged["period_offset"] = (merged["mes"] - merged["cohort"]).apply(
        lambda x: x.n if hasattr(x, "n") else 0
    )

    cohort_sizes = users_df.groupby("cohort")["UsuarioID"].nunique()
    retention_by_period = merged.groupby("period_offset")["UsuarioID"].nunique()

    result: List[RetentionPoint] = []
    for i in range(months):
        total = cohort_sizes.sum() if cohort_sizes.sum() > 0 else 1
        retained = retention_by_period.get(i, 0)
        pct = round(float(retained) / float(total) * 100, 1)
        result.append(RetentionPoint(label=f"M{i}", value=pct if pct <= 100 else 100.0))

    return result


async def get_usage_split(db: AsyncSession) -> UsageSplit:
    result = await db.execute(text("""
        SELECT
            COUNT(DISTINCT CASE WHEN "ModuloClave" NOT LIKE 'empresa%' THEN "UsuarioID" END) AS personal,
            COUNT(DISTINCT CASE WHEN "ModuloClave" LIKE 'empresa%' THEN "UsuarioID" END) AS empresarial
        FROM "EntornosPersonales"
    """))
    row = result.mappings().one_or_none()

    personal = int(row["personal"] or 0) if row else 0
    empresarial = int(row["empresarial"] or 0) if row else 0
    total = personal + empresarial

    if total == 0:
        return UsageSplit(personal=50.0, empresarial=50.0)

    return UsageSplit(
        personal=round(personal / total * 100, 1),
        empresarial=round(empresarial / total * 100, 1),
    )
