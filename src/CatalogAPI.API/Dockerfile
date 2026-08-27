FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /source

COPY ["src/CatalogAPI.Domain/CatalogAPI.Domain.csproj", "src/CatalogAPI.Domain/"]
COPY ["src/CatalogAPI.Application/CatalogAPI.Application.csproj", "src/CatalogAPI.Application/"]
COPY ["src/CatalogAPI.Infrastructure/CatalogAPI.Infrastructure.csproj", "src/CatalogAPI.Infrastructure/"]
COPY ["src/CatalogAPI.API/CatalogAPI.API.csproj", "src/CatalogAPI.API/"]

RUN dotnet restore "src/CatalogAPI.API/CatalogAPI.API.csproj"

COPY . .

WORKDIR "/source/src/CatalogAPI.API"
RUN dotnet publish "CatalogAPI.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CatalogAPI.API.dll"]
