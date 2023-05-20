FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Authorization/Application", "Application/"]
COPY ["Authorization/Domain", "Domain/"]
COPY ["Authorization/Infrastructure", "Infrastructure/"]
COPY ["Authorization/ConfigurationApp", "ConfigurationApp/"]
RUN dotnet restore "ConfigurationApp/ConfigurationApp.csproj"
WORKDIR "/src/ConfigurationApp"
RUN dotnet build "ConfigurationApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConfigurationApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConfigurationApp.dll"]