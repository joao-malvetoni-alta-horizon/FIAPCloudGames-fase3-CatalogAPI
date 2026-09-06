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

# Agente APM New Relic: o pacote NuGet NewRelic.Agent publica o agente em /app/newrelic.
# O profiler do CoreCLR só é carregado quando estas variáveis estão presentes, então a
# imagem já sobe instrumentada; sem NEW_RELIC_LICENSE_KEY o agente apenas não conecta.
# A license key NÃO fica aqui: vem de Kubernetes Secret (ver k8s/deployment.yaml).
ENV CORECLR_ENABLE_PROFILING=1 \
    CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
    CORECLR_NEWRELIC_HOME=/app/newrelic \
    CORECLR_PROFILER_PATH=/app/newrelic/libNewRelicProfiler.so \
    NEW_RELIC_APP_NAME=FCG-CatalogAPI \
    NEW_RELIC_DISTRIBUTED_TRACING_ENABLED=true \
    NEW_RELIC_APPLICATION_LOGGING_ENABLED=true \
    NEW_RELIC_APPLICATION_LOGGING_FORWARDING_ENABLED=true \
    NEW_RELIC_APPLICATION_LOGGING_LOCAL_DECORATING_ENABLED=true

ENTRYPOINT ["dotnet", "CatalogAPI.API.dll"]
