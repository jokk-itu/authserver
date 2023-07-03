FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Clients/WebApp", "Clients/WebApp/"]
COPY ["OIDC/OIDC.Client", "OIDC/OIDC.Client/"]
RUN dotnet restore "Clients/WebApp/App.csproj"
WORKDIR "/src/Clients/WebApp"
RUN dotnet build "App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "App.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "App.dll"]