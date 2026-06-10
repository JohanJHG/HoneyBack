from pydantic import BaseModel
from typing import List


class OverviewKpis(BaseModel):
    total_usuarios: int
    usuarios_activos: int
    contactos_sin_revisar: int


class ChartPoint(BaseModel):
    label: str
    count: int
