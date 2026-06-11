from pydantic import BaseModel
from typing import List


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
