FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Clients/BffApp/Server", "Clients/BffApp/Server/"]
COPY ["Clients/BffApp/Client", "Clients/BffApp/Client/"]
COPY ["Clients/BffApp/Shared", "Clients/BffApp/Shared/"]
COPY ["OIDC/OIDC.Client", "OIDC/OIDC.Client/"]
RUN dotnet restore "Clients/BffApp/Server/Server.csproj"
WORKDIR "/src/Clients/BffApp/Server"
RUN dotnet build "Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Server.dll"]