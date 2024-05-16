using AuthServer.Entities;
using AuthServer.Enums;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Tests.Core.EntityBuilders;
public class ClientBuilder(DbContext dbContext)
{
    public async Task<Client> GetNativeClient()
    {
        var client = new Client("PinguNativeApp", ApplicationType.Native, TokenEndpointAuthMethod.None);
        await dbContext.Set<Client>().AddAsync(client);
        await dbContext.SaveChangesAsync();
        return client;
    }

    public async Task<Client> GetBasicWebClient()
    {
        var client = new Client("PinguBasicWebApp", ApplicationType.Web, TokenEndpointAuthMethod.ClientSecretBasic);
        await dbContext.Set<Client>().AddAsync(client);
        await dbContext.SaveChangesAsync();
        return client;
    }

    public async Task<Client> GetPrivateKeyJwtWebClient(string jwks)
    {
        var client = new Client("PinguPrivateKeyJwtWebApp", ApplicationType.Web, TokenEndpointAuthMethod.PrivateKeyJwt)
        {
            Jwks = jwks
        };
        await dbContext.Set<Client>().AddAsync(client);
        await dbContext.SaveChangesAsync();
        return client;
    }
}