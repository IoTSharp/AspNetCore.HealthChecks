#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/example/example.csproj", "src/example/"]
COPY ["src/HealthChecks.Cassandra/HealthChecks.Cassandra.csproj", "src/HealthChecks.Cassandra/"]
COPY ["src/HealthChecks.InfluxDB/HealthChecks.InfluxDB.csproj", "src/HealthChecks.InfluxDB/"]
RUN dotnet restore "src/example/example.csproj"
COPY . .
WORKDIR "/src/src/example"
RUN dotnet build "example.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "example.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "example.dll"]