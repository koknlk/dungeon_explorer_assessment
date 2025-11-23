FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DungeonExplorerBackend.csproj", "."]
RUN dotnet restore "DungeonExplorerBackend.csproj"
COPY . .
RUN dotnet build "DungeonExplorerBackend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DungeonExplorerBackend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN mkdir -p /app/data
ENTRYPOINT ["dotnet", "DungeonExplorerBackend.dll"]