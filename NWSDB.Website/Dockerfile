FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NWSDB.Website/NWSDB.Website.csproj", "NWSDB.Website/"]
RUN dotnet restore "NWSDB.Website/NWSDB.Website.csproj"
COPY . .
WORKDIR "/src/NWSDB.Website"
RUN dotnet build "NWSDB.Website.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NWSDB.Website.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NWSDB.Website.dll"]
