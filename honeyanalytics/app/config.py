from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    database_url: str
    jwt_secret: str
    jwt_issuer: str = "HoneyBack"
    jwt_audience: str = "HoneyBack"

    class Config:
        env_file = ".env"


settings = Settings()
