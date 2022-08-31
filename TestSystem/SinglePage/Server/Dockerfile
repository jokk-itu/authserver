#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["TestSystem/SinglePage/Server/Server.csproj", "TestSystem/SinglePage/Server/"]
COPY ["TestSystem/SinglePage/Client/Client.csproj", "TestSystem/SinglePage/Client/"]
RUN dotnet restore "TestSystem/SinglePage/Server/Server.csproj"
COPY . .
WORKDIR "/src/TestSystem/SinglePage/Server"
RUN dotnet build "Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Server.dll"]