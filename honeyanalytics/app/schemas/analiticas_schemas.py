from pydantic import BaseModel
from typing import List, Optional


class AnaliticasKpis(BaseModel):
    mau: int
    dau: int
    churn_rate: float
    avg_session_minutes: float


class RetentionPoint(BaseModel):
    label: str
    value: float


class UsageSplit(BaseModel):
    personal: float
    empresarial: float


class ChurnedUser(BaseModel):
    user_id: int
    nombre: str
    email: str
    ultima_actividad: Optional[str]
    dias_inactivo: int
    criterio: str
