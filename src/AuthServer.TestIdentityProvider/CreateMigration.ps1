dotnet build
dotnet ef migrations add $args[0] -c AuthorizationDbContext -s AuthServer.TestIdentityProvider.csproj