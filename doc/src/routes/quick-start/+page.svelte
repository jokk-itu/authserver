<script lang="ts">
    import Highlight from "svelte-highlight";
    import csharp from "svelte-highlight/languages/csharp";
  
    const code = `
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddAuthServer();
    builder.Services.AddSingleton<IDistributedCache, Cache>();
    builder.Services.AddScoped<IUserClaimService, UserClaimService>();
    builder.Services.AddScoped<IUsernameResolver, UsernameResolver>();

    builder.AddOptions<JwksDocument>(jwksDocument =>
    {
        var securityKey = new RsaSecurityKey(RSA.Create(3072));
        jwksDocument.SigningKeys = [new JwksDocument.SigningKey(securityKey, SigningAlg.RsaSha256)];
    });

    builder.AddOptions<DiscoveryDocument>(discoveryDocument =>
    {
        discoveryDocument.Issuer = "https://idp.authserver.dk";
    });

    builder.AddOptions<UserInteraction>(userInteraction =>
    {
        userInteraction.LoginUri = "https://idp.authserver.dk/login";
        userInteraction.ConsentUri = "https://idp.authserver.dk/consent";
        userInteraction.AccountSelectionUri = "https://idp.authserver.dk/account-selection";
    });

    var app = builder.Build();
    
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapAuthorizeEndpoint();
    app.MapTokenEndpoint();
    app.MapDiscoveryDocumentEndpoint();
    app.MapJwksDocumentEndpoint();`;
  </script>

<h1 class="text-4xl mb-2">Quick Start</h1>
<p class="mb-4">
    Explain the setup with Dependency Injection
    <br />
    UI URI options
    <br />
    Discovery and JWKS options
    <br />
    Explain how to get migrations to work
</p>

<Highlight language={csharp} {code} />