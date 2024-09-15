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
    .AddCookie()
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

        options.ClaimActions.MapUniqueJsonKey("auth_time", "auth_time");
        options.ClaimActions.MapUniqueJsonKey("grant_id", "grant_id");
        options.ClaimActions.MapUniqueJsonKey("address", "address");
        options.ClaimActions.MapUniqueJsonKey("given_name", "given_name");
        options.ClaimActions.MapUniqueJsonKey("family_name", "family_name");
        options.ClaimActions.MapUniqueJsonKey("birthdate", "birthdate");
        options.ClaimActions.MapUniqueJsonKey("email", "email");
        options.ClaimActions.MapUniqueJsonKey("phone", "phone");

        options.ClientId = openIdConnectSection.GetValue<string>("ClientId");
        options.ClientSecret = openIdConnectSection.GetValue<string>("ClientSecret");

        string[] resources =
        [
            openIdConnectSection.GetValue<string>("Authority")!
        ];

        string[] scopes = ["openid", "profile", "address", "email", "phone", "authserver:userinfo"];
        foreach (var scope in scopes)
        {
            options.Scope.Add(scope);
        }

        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProviderForSignOut = context =>
            {
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
            }
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

app.Run();