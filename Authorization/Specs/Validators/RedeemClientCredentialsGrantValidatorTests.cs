﻿using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Infrastructure.Helpers;
using Infrastructure.Requests.Abstract;
using Infrastructure.Requests.RedeemClientCredentialsGrant;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Specs.Helpers.EntityBuilders;
using Xunit;

namespace Specs.Validators;

public class RedeemClientCredentialsGrantValidatorTests : BaseUnitTest
{
  [Theory]
  [InlineData(0)]
  [InlineData(2)]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidClientAuthentications_ExpectInvalidClient(int count)
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var command = new RedeemClientCredentialsGrantCommand
    {
      ClientAuthentications = Enumerable.Repeat(new ClientAuthentication(), count).ToList(),
      GrantType = GrantTypeConstants.ClientCredentials,
      Scope = "scope"
    };
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemClientCredentialsGrantCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidClient, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidClientId_ExpectInvalidClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var command = new RedeemClientCredentialsGrantCommand
    {
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = "invalid"
        }
      },
      GrantType = GrantTypeConstants.ClientCredentials,
      Scope = "scope"
    };
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemClientCredentialsGrantCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidClient, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_InvalidClientSecret_ExpectInvalidClient()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var client = ClientBuilder
      .Instance()
      .AddSecret(clientSecret)
      .Build();

    await IdentityContext.Set<Client>().AddAsync(client);
    await IdentityContext.SaveChangesAsync();

    var command = new RedeemClientCredentialsGrantCommand
    {
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = client.Id,
          ClientSecret = "invalid"
        }
      },
      GrantType = GrantTypeConstants.ClientCredentials,
      Scope = "scope"
    };
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemClientCredentialsGrantCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.InvalidClient, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_UnsupportedGrantType()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var client = ClientBuilder
      .Instance()
      .AddSecret(clientSecret)
      .Build();

    await IdentityContext.Set<Client>().AddAsync(client);
    await IdentityContext.SaveChangesAsync();

    var command = new RedeemClientCredentialsGrantCommand
    {
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = client.Id,
          ClientSecret = clientSecret
        }
      },
      GrantType = "invalid"
    };
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemClientCredentialsGrantCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.UnsupportedGrantType, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_UnauthorizedClient_GrantTypes()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var scope = ScopeBuilder
      .Instance()
      .Build();

    var clientSecret = CryptographyHelper.GetRandomString(32);
    var client = ClientBuilder
      .Instance()
      .AddSecret(clientSecret)
      .AddScope(scope)
      .Build();

    await IdentityContext.Set<Client>().AddAsync(client);
    await IdentityContext.SaveChangesAsync();

    var command = new RedeemClientCredentialsGrantCommand
    {
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = client.Id,
          ClientSecret = clientSecret
        }
      },
      GrantType = GrantTypeConstants.ClientCredentials,
      Scope = scope.Name
    };
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemClientCredentialsGrantCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_UnauthorizedClient_Scopes()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var clientSecret = CryptographyHelper.GetRandomString(32);
    var client = ClientBuilder
      .Instance()
      .AddSecret(clientSecret)
      .AddGrantType(await IdentityContext.Set<GrantType>()
        .SingleAsync(x => x.Name == GrantTypeConstants.ClientCredentials))
      .Build();

    await IdentityContext.Set<Client>().AddAsync(client);
    await IdentityContext.SaveChangesAsync();

    var command = new RedeemClientCredentialsGrantCommand
    {
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = client.Id,
          ClientSecret = clientSecret
        }
      },
      GrantType = GrantTypeConstants.ClientCredentials,
      Scope = "invalid"
    };
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemClientCredentialsGrantCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

    // Assert
    Assert.True(validationResult.IsError());
    Assert.Equal(ErrorCode.UnauthorizedClient, validationResult.ErrorCode);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ValidateAsync_Ok()
  {
    // Arrange
    var serviceProvider = BuildServiceProvider();
    var scope = await IdentityContext.Set<Scope>().SingleAsync(x => x.Name == ScopeConstants.UserInfo);

    var weatherClientSecret = CryptographyHelper.GetRandomString(32);
    var weatherClient = ClientBuilder
      .Instance()
      .AddSecret(weatherClientSecret)
      .AddClientUri("https://weather.authserver.dk")
      .AddScope(scope)
      .Build();

    var clientSecret = CryptographyHelper.GetRandomString(32);
    var client = ClientBuilder
      .Instance()
      .AddSecret(clientSecret)
      .AddScope(scope)
      .AddGrantType(await IdentityContext.Set<GrantType>()
        .SingleAsync(x => x.Name == GrantTypeConstants.ClientCredentials))
      .Build();

    await IdentityContext.AddAsync(client);
    await IdentityContext.AddAsync(weatherClient);
    await IdentityContext.SaveChangesAsync();

    var command = new RedeemClientCredentialsGrantCommand
    {
      ClientAuthentications = new[]
      {
        new ClientAuthentication
        {
          ClientId = client.Id,
          ClientSecret = clientSecret
        }
      },
      GrantType = GrantTypeConstants.ClientCredentials,
      Scope = scope.Name,
      Resource = new[] { weatherClient.ClientUri }
    };
    var validator = serviceProvider.GetRequiredService<IValidator<RedeemClientCredentialsGrantCommand>>();

    // Act
    var validationResult = await validator.ValidateAsync(command, CancellationToken.None);

    // Assert
    Assert.False(validationResult.IsError());
  }
}