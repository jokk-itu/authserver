using Domain;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder()
  .SetBasePath(Directory.GetCurrentDirectory())
  .AddJsonFile("config.json", optional: false)
  .AddEnvironmentVariables();

var config = builder.Build();
var sqliteConnectionString = config.GetConnectionString("Sqlite");
var sqlServerConnectionString = config.GetConnectionString("SqlServer");
var dbContextOptionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
if (!string.IsNullOrWhiteSpace(sqliteConnectionString))
{
  dbContextOptionsBuilder.UseSqlite(sqliteConnectionString);
}
else if (!string.IsNullOrWhiteSpace(sqlServerConnectionString))
{
  dbContextOptionsBuilder.UseSqlServer(sqlServerConnectionString);
}

var identityContext = new IdentityContext(dbContextOptionsBuilder.Options);

if (args[0] == "resource")
{
  var resources = config.GetSection("Resources").Get<List<Resource>>();
  foreach (var resource in resources)
  {
    await identityContext.AddAsync(resource);
  }

  await identityContext.SaveChangesAsync();
}
else if (args[0] == "scope")
{
  var scopes = config.GetSection("Scopes").Get<List<Scope>>();
  foreach (var scope in scopes)
  {
    await identityContext.AddAsync(scope);
  }

  await identityContext.SaveChangesAsync();
}
else if (args[0] == "rotate")
{
  throw new NotImplementedException();
}
else if (args[0] == "migration")
{
  await identityContext.Database.MigrateAsync();
}
else
{
  throw new NotSupportedException();
}