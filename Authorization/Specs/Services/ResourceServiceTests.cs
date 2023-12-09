using Application;
using Infrastructure.Helpers;
using Infrastructure.Services.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers.EntityBuilders;
using Xunit;
using Domain;
using Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Specs.Services;

public class ResourceServiceTests : BaseUnitTest
{
    [Fact]
    public async Task ValidateResource_EmptyResource_ExpectInvalidTarget()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var resourceService = serviceProvider.GetRequiredService<IResourceService>();

        // Act
        var validationResult = await resourceService.ValidateResources(new List<string>(), string.Empty);

        // Assert
        Assert.True(validationResult.IsError());
        Assert.Equal(ErrorCode.InvalidTarget, validationResult.ErrorCode);
    }

    [Fact]
    public async Task ValidateResource_NonExistingResource_ExpectInvalidTarget()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var resourceService = serviceProvider.GetRequiredService<IResourceService>();

        // Act
        var validationResult = await resourceService.ValidateResources(new List<string>{"https://weather.authserver.dk"}, string.Empty);

        // Assert
        Assert.True(validationResult.IsError());
        Assert.Equal(ErrorCode.InvalidTarget, validationResult.ErrorCode);
    }

    [Fact]
    public async Task ValidateResource_ExistingResource_Ok()
    {
        // Arrange
        var serviceProvider = BuildServiceProvider();
        var resourceService = serviceProvider.GetRequiredService<IResourceService>();
        var resource = ResourceBuilder
            .Instance()
            .AddSecret(CryptographyHelper.GetRandomString(32))
            .AddScope(await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.OpenId))
            .Build();
        await IdentityContext.AddAsync(resource);
        await IdentityContext.SaveChangesAsync();

        // Act
        var validationResult = await resourceService.ValidateResources(new List<string>{resource.Uri}, ScopeConstants.OpenId);

        // Assert
        Assert.False(validationResult.IsError());
    }
}