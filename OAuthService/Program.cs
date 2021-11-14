using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OAuthService;
using OAuthService.Repositories;

var builder = WebApplication.CreateBuilder(args);

//CONFIGURE SERVICE CONTAINER
var services = builder.Services;
services.AddControllers();

services.AddDataProtection();

services.AddApiVersioning(config => { config.ReportApiVersions = true; });
services.AddVersionedApiExplorer(config =>
{
    config.GroupNameFormat = "'v'VVV";
    config.SubstituteApiVersionInUrl = true;
});
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddOptions<ConfigureSwaggerOptions>();

services.Configure<AuthenticationConfiguration>(builder.Configuration.GetSection("Identity"));

services.AddScoped<ClientManager>();

var tokenValidationParameters = new TokenValidationParameters
{
    ValidateAudience = true,
    ValidateIssuer = true,
    IgnoreTrailingSlashWhenValidatingAudience = true,
    IssuerSigningKey =
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Identity:TokenSecret"])),
    ValidIssuer = builder.Configuration["Identity:Issuer"],
    ValidAudience = builder.Configuration["Identity:Audience"]
};
services.AddSingleton(tokenValidationParameters);

services.AddAuthentication("OAuth")
    .AddJwtBearer("OAuth", config =>
    {
        config.IncludeErrorDetails = false; //PRODUCTION READY
        config.RequireHttpsMetadata = false; //DEVELOP READY
        config.TokenValidationParameters = tokenValidationParameters;
        config.Validate();
    });

services.AddDbContext<IdentityContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"),
        optionsBuilder => { optionsBuilder.EnableRetryOnFailure(10, TimeSpan.FromSeconds(2), null); });
});

services.AddScoped<IdentityContext>();

services.AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<IdentityContext>();

services.AddAuthorization(config => { });

var app = builder.Build();


//CONFIGURE PIPELINE
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var version in provider.ApiVersionDescriptions)
        options.SwaggerEndpoint(
            $"/swagger/{version.GroupName}/swagger.json",
            version.GroupName.ToUpper());
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


//MAIN METHOD
app.Run();