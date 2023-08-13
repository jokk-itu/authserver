﻿using System.Security.Cryptography;
using System.Text;
using Domain;
using Domain.Enums;
using Domain.Extensions;
using Infrastructure;
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

if (args[0] == "resource")
{
  var resources = config.GetSection("Resources").Get<List<Resource>>();
  foreach (var resource in resources)
  {
    Log.Information("Inserting resource {@resource}", resource);
    var scopes = resource.Scopes.Select(x => x.Name).ToList();
    resource.Scopes.Clear();
    foreach (var scopeName in scopes)
    {
      var scope = await identityContext
        .Set<Scope>()
        .SingleAsync(y => y.Name == scopeName);

      resource.Scopes.Add(scope);
    }
    await identityContext.AddAsync(resource);
  }

  await identityContext.SaveChangesAsync();
  Log.Information("Resources inserted");
}
else if (args[0] == "scope")
{
  var scopes = config.GetSection("Scopes").Get<List<Scope>>();
  foreach (var scope in scopes)
  {
    Log.Information("Inserting scope {@scope}", scope);
    await identityContext.AddAsync(scope);
  }

  await identityContext.SaveChangesAsync();
  Log.Information("Scopes inserted");
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
  foreach (var client in clients)
  {
    var clientSecret = client["ClientSecret"];
    var applicationType = client["ApplicationType"].GetEnum<ApplicationType>();
    var tokenEndpointAuthMethod = client["TokenEndpointAuthMethod"].GetEnum<TokenEndpointAuthMethod>();
    var subjectType = client["SubjectType"].GetEnum<SubjectType>();
    var clientUri = client["ClientUri"];
    var backchannelLogoutUri = client["BackchannelLogoutUri"];
    var clientName = client["ClientName"];

    var newClient = new Client
    {
      Name = clientName,
      Secret = clientSecret,
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

    newClient.RedirectUris.Add(new RedirectUri
    {
      Type = RedirectUriType.AuthorizeRedirectUri,
      Uri = client["AuthorizeRedirectUri"]
    });

    newClient.RedirectUris.Add(new RedirectUri
    {
      Type = RedirectUriType.PostLogoutRedirectUri,
      Uri = client["PostLogoutRedirectUri"]
    });

    newClient.ResponseTypes.Add(
      await identityContext.Set<ResponseType>().SingleAsync(x => x.Name == client["ResponseType"]));

    Log.Information("Inserted Client {@client}", newClient);
    await identityContext.Set<Client>().AddAsync(newClient);
  }

  await identityContext.SaveChangesAsync();
  Log.Information("Clients inserted");
}
else
{
  Log.CloseAndFlush();
  throw new NotSupportedException($"Argument is not supported: {args[0]}");
}

Log.CloseAndFlush();