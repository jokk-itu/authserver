using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Collections.Specialized;
using Microsoft.IdentityModel.JsonWebTokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddAuthorization();
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "Client.Identity";
    })
    .AddOpenIdConnect(options =>
    {
        var openIdConnectSection = builder.Configuration.GetSection("OpenIdConnect");

        options.Authority = openIdConnectSection.GetValue<string>("Authority");
        options.TokenValidationParameters.ValidIssuer = openIdConnectSection.GetValue<string>("Authority");
        options.TokenValidationParameters.ValidAudience = openIdConnectSection.GetValue<string>("ClientId");
        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "roles";
        options.TokenValidationParameters.SignatureValidator = (token, _) => new JsonWebToken(token);

        options.GetClaimsFromUserInfoEndpoint = true;
        options.DisableTelemetry = true;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;

        options.ClaimActions.MapUniqueJsonKey("grant_id", "grant_id");
        options.ClaimActions.MapUniqueJsonKey("address", "address");
        options.ClaimActions.MapUniqueJsonKey("phone", "phone");

        options.ClientId = openIdConnectSection.GetValue<string>("ClientId");
        options.ClientSecret = openIdConnectSection.GetValue<string>("ClientSecret");

        string[] resources =
        [
            openIdConnectSection.GetValue<string>("Authority")!
        ];

        string[] scopes = ["address", "email", "phone", "authserver:userinfo"];
        foreach (var scope in scopes)
        {
            options.Scope.Add(scope);
        }

        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = context =>
            {
                // TODO Customize ResponseMode (form_post, query and fragment)

                return Task.CompletedTask;
            },
            OnRedirectToIdentityProviderForSignOut = context =>
            {
                if (context.Properties.GetParameter<bool>("interactive"))
                {
                    context.ProtocolMessage.RemoveParameter("id_token_hint");
                }
                context.ProtocolMessage.Parameters.Add("client_id", context.Options.ClientId);
                return Task.CompletedTask;
            },
            OnAuthorizationCodeReceived = context =>
            {
                var nameValueCollection = new NameValueCollection();
                foreach (var resource in resources)
                {
                    nameValueCollection.Add(OpenIdConnectParameterNames.Resource, resource);
                }

                context.TokenEndpointRequest!.SetParameters(nameValueCollection);

                return Task.CompletedTask;
            },
        };
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseHsts();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapGet("/api/login", (HttpContext httpContext, string? prompt, string? responseMode) =>
{
    if (httpContext.User.Identity?.IsAuthenticated == true)
    {
        return Results.Redirect("~/");
    }
    else if (prompt is not null)
    {
        return Results.Challenge(new OpenIdConnectChallengeProperties
        {
            Prompt = prompt
        }, [OpenIdConnectDefaults.AuthenticationScheme]);
    }

    return Results.Challenge(new OpenIdConnectChallengeProperties(), [OpenIdConnectDefaults.AuthenticationScheme]);
});
app.MapGet("/api/logout/silent", async httpContext =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await httpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
});
app.MapGet("/api/logout/interactive", async httpContext =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await httpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new OpenIdConnectChallengeProperties(null, new Dictionary<string, object?>
    {
        { "interactive", true }
    }));
});

app.Run();