FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NWSDB.Api/NWSDB.Api.csproj", "NWSDB.Api/"]
RUN dotnet restore "NWSDB.Api/NWSDB.Api.csproj"
COPY . .
WORKDIR "/src/NWSDB.Api"
RUN dotnet build "NWSDB.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NWSDB.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NWSDB.Api.dll"]
