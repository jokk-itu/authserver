using AuthorizationServer.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthorizationServer.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services, AuthenticationConfiguration configuration)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IgnoreTrailingSlashWhenValidatingAudience = true,
            ValidIssuer = configuration.Issuer,
            ValidAudience = configuration.Audience
        };
        services.AddSingleton(tokenValidationParameters);

        services.AddAuthentication("OpenId")
            .AddJwtBearer("OpenId", config =>
            {
                config.IncludeErrorDetails = true; //DEVELOP READY
                config.RequireHttpsMetadata = false; //DEVELOP READY
                config.TokenValidationParameters = tokenValidationParameters;
                config.Validate();
            });
        
        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddApiVersioning(config => { config.ReportApiVersions = true; });
        services.AddVersionedApiExplorer(config =>
        {
            config.GroupNameFormat = "'v'VVV";
            config.SubstituteApiVersionInUrl = true;
        });
        services.AddEndpointsApiExplorer();
        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen();
        services.AddOptions<ConfigureSwaggerOptions>();
        return services;
    }

    public static IServiceCollection AddDatastore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ClientManager>();
        services.AddScoped<ResourceManager>();
        services.AddScoped<KeyPairManager>();
        services.AddDbContext<IdentityContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("SqlServer"),
                optionsBuilder => { optionsBuilder.EnableRetryOnFailure(10, TimeSpan.FromSeconds(2), null); });
        });

        services.AddScoped<IdentityContext>();

        services.AddIdentityCore<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<IdentityContext>();

        return services;
    }
}