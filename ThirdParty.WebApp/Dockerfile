FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ThirdParty.WebApp/ThirdParty.WebApp.csproj", "ThirdParty.WebApp/"]
RUN dotnet restore "ThirdParty.WebApp/ThirdParty.WebApp.csproj"
COPY . .
WORKDIR "/src/ThirdParty.WebApp"
RUN dotnet build "ThirdParty.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ThirdParty.WebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ThirdParty.WebApp.dll"]
