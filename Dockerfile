

# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["HoneyBack.csproj", "./"]
RUN dotnet restore "HoneyBack.csproj"

# Copiar el resto de los archivos y compilar
COPY . .
RUN dotnet build "HoneyBack.csproj" -c Release -o /app/build

# Publicar la aplicación
FROM build AS publish
RUN dotnet publish "HoneyBack.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa final - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copiar archivos publicados
COPY --from=publish /app/publish .

# Punto de entrada
ENTRYPOINT ["dotnet", "HoneyBack.dll"]
