FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ProtectedResources/WeatherService/Api", "ProtectedResources/WeatherService/Api"]
COPY ["ProtectedResources/WeatherService/Contracts", "ProtectedResources/WeatherService/Contracts"]
RUN dotnet restore "ProtectedResources/WeatherService/Api/Api.csproj"
WORKDIR "/src/ProtectedResources/WeatherService/Api"
RUN dotnet build "Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]