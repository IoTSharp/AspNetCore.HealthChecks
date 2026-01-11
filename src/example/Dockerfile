#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/example/example.csproj", "src/example/"]
COPY ["src/HealthChecks.Cassandra/HealthChecks.Cassandra.csproj", "src/HealthChecks.Cassandra/"]
COPY ["src/HealthChecks.InfluxDB/HealthChecks.InfluxDB.csproj", "src/HealthChecks.InfluxDB/"]
COPY ["src/HealthChecks.IoTDB/HealthChecks.IoTDB.csproj", "src/HealthChecks.IoTDB/"]
COPY ["src/HealthChecks.NTPServer/HealthChecks.NTPServer.csproj", "src/HealthChecks.NTPServer/"]
RUN dotnet restore "./src/example/example.csproj"
COPY . .
WORKDIR "/src/src/example"
RUN dotnet build "./example.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./example.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "example.dll"]