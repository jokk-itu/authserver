using System.Security.Cryptography;
using System.Text;
using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

var builder = new ConfigurationBuilder()
  .SetBasePath(Directory.GetCurrentDirectory())
  .AddJsonFile("config.json", optional: false)
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
    foreach (var scopeName in resource.Scopes.Select(x => x.Name))
    {
      var scope = await identityContext
        .Set<Scope>()
        .SingleOrDefaultAsync(y => y.Name == scopeName);
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
  var privateKey = config.GetSection("IdentityConfiguration").GetValue<string>("PrivateKey");
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
  Log.Information("Rotating JWK");
}
else if (args[0] == "migration")
{
  Log.Information("Migration starting");
  await identityContext.Database.MigrateAsync();
  Log.Information("Migration finished");
}
else
{
  Log.CloseAndFlush();
  throw new NotSupportedException($"Argument is not supported: {args[0]}");
}

Log.CloseAndFlush();