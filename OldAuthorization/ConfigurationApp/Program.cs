using System.Net;
using System.Security.Cryptography;
using System.Text;
using Domain;
using Domain.Enums;
using Domain.Extensions;
using Infrastructure;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
  ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
  ?? "Development";

var builder = new ConfigurationBuilder()
  .SetBasePath(Directory.GetCurrentDirectory())
  .AddJsonFile($"appsettings.{environment}.json", optional: false)
  .AddEnvironmentVariables();

Log.Logger = new LoggerConfiguration()
  .Enrich.FromLogContext()
  .WriteTo.Console()
  .CreateLogger();

var config = builder.Build();
var sqliteConnectionString = config.GetConnectionString("SQLite");
var dbContextOptionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
if (!string.IsNullOrWhiteSpace(sqliteConnectionString))
{
  dbContextOptionsBuilder.UseSqlite(sqliteConnectionString);
}

var identityContext = new IdentityContext(dbContextOptionsBuilder.Options);

if (args[0] == "scope")
{
  var scopes = config.GetSection("Scopes").Get<List<Scope>>();
  foreach (var scope in scopes)
  {
    Log.Information("Inserting scope {@scope}", scope);
    await identityContext.AddAsync(scope);
  }

  await identityContext.SaveChangesAsync();
  Log.Information("{Amount} Scopes inserted", scopes.Count);
}
else if (args[0] == "rotate")
{
  Log.Information("Rotating JWK");
  using var rsa = new RSACryptoServiceProvider(4096);
  var privateKey = config.GetSection("Identity").GetValue<string>("PrivateKey");
  var password = Encoding.Default.GetBytes(privateKey);
  var pbeParameters = new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, 10);
  var encryptedPrivateKey = rsa.ExportEncryptedPkcs8PrivateKey(password, pbeParameters);
  var publicKey = rsa.ExportParameters(false);
  var jwk = new Jwk
  {
    PrivateKey = encryptedPrivateKey,
    Modulus = publicKey.Modulus!,
    Exponent = publicKey.Exponent!,
    CreatedTimestamp = DateTime.UtcNow
  };
  await identityContext.Set<Jwk>().AddAsync(jwk);
  await identityContext.SaveChangesAsync();
  Log.Information("Rotated JWK");
}
else if (args[0] == "migration")
{
  Log.Information("Migration starting");
  await identityContext.Database.MigrateAsync();
  Log.Information("Migration finished");
}
else if (args[0] == "cleanclient")
{
  Log.Information("Clean Clients started");
  var clients = await identityContext.Set<Client>().ToListAsync();
  identityContext.RemoveRange(clients);
  await identityContext.SaveChangesAsync();
  Log.Information("Cleaned {amount} Clients", clients.Count);
}
else if (args[0] == "client")
{
  var clients = config.GetSection("Clients").GetChildren();
  var newClients = new List<Client>();
  foreach (var client in clients)
  {
    var clientId = client["ClientId"];
    var clientSecret = client["ClientSecret"];
    var applicationType = client["ApplicationType"].GetEnum<ApplicationType>();
    var tokenEndpointAuthMethod = client["TokenEndpointAuthMethod"].GetEnum<TokenEndpointAuthMethod>();
    var subjectType = client["SubjectType"]?.GetEnum<SubjectType>();
    var clientUri = client["ClientUri"];
    var backchannelLogoutUri = client["BackchannelLogoutUri"];
    var clientName = client["ClientName"];

    var newClient = new Client
    {
      Id = clientId,
      Name = clientName,
      Secret = BCrypt.HashPassword(clientSecret, BCrypt.GenerateSalt()),
      ApplicationType = applicationType,
      TokenEndpointAuthMethod = tokenEndpointAuthMethod,
      SubjectType = subjectType,
      ClientUri = clientUri,
      BackchannelLogoutUri = backchannelLogoutUri
    };

    var scopes = client["Scope"].Split(' ');
    foreach (var scope in scopes)
    {
      newClient.Scopes.Add(
        await identityContext.Set<Scope>().SingleAsync(x => x.Name == scope));
    }

    foreach (var grantType in client.GetSection("GrantTypes").GetChildren())
    {
      newClient.GrantTypes.Add(
        await identityContext.Set<GrantType>().SingleAsync(x => x.Name == grantType.Value));
    }

    if (!string.IsNullOrWhiteSpace(client["AuthorizeRedirectUri"]))
    {
      newClient.RedirectUris.Add(new RedirectUri
      {
        Type = RedirectUriType.AuthorizeRedirectUri,
        Uri = client["AuthorizeRedirectUri"]
      });
    }

    if (!string.IsNullOrWhiteSpace(client["PostLogoutRedirectUri"]))
    {
      newClient.RedirectUris.Add(new RedirectUri
      {
        Type = RedirectUriType.PostLogoutRedirectUri,
        Uri = client["PostLogoutRedirectUri"]
      });
    }

    if (!string.IsNullOrWhiteSpace(client["ResponseType"]))
    {
      newClient.ResponseTypes.Add(
        await identityContext.Set<ResponseType>().SingleAsync(x => x.Name == client["ResponseType"]));
    }

    Log.Information("Inserted Client {@client}", newClient);
    newClients.Add(newClient);
  }
  await identityContext.Set<Client>().AddRangeAsync(newClients);
  await identityContext.SaveChangesAsync();
  Log.Information("{Amount} Clients inserted", newClients.Count);
}
else
{
  Log.CloseAndFlush();
  throw new NotSupportedException($"Argument is not supported: {args[0]}");
}

Log.CloseAndFlush();