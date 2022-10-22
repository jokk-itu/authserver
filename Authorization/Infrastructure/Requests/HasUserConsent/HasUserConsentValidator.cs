using System.Net;
using Application;
using Application.Validation;
using Domain;
using Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.HasUserConsent;
public class HasUserConsentValidator : IValidator<HasUserConsentQuery>
{
  private readonly IdentityContext _identityContext;
  private readonly UserManager<User> _userManager;

  public HasUserConsentValidator(
    IdentityContext identityContext,
    UserManager<User> userManager)
  {
    _identityContext = identityContext;
    _userManager = userManager;
  }

  public async Task<ValidationResult> ValidateAsync(HasUserConsentQuery value, CancellationToken cancellationToken = default)
  {
    if (await IsUserInvalid(value))
      return new ValidationResult(ErrorCode.InvalidRequest, "username is invalid", HttpStatusCode.BadRequest);

    if(await IsScopeInvalid(value))
      return new ValidationResult(ErrorCode.InvalidScope, "scope is invalid", HttpStatusCode.BadRequest);

    if (await IsClientInvalid(value))
      return new ValidationResult(ErrorCode.InvalidClient, "client_id is invalid", HttpStatusCode.BadRequest);

    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<bool> IsUserInvalid(HasUserConsentQuery query)
  {
    if (!string.IsNullOrWhiteSpace(query.Username)
        && !string.IsNullOrWhiteSpace(query.Password))
      return true;

    var user = await _userManager.FindByNameAsync(query.Username);
    if (user is null)
      return true;

    return !await _userManager.CheckPasswordAsync(user, query.Password);
  }

  private async Task<bool> IsScopeInvalid(HasUserConsentQuery query)
  {
    if (!query.Scopes.Contains(ScopeConstants.OpenId))
      return true;

    foreach (var scope in query.Scopes)
    {
      if (!await _identityContext.Set<Scope>().AnyAsync(x => x.Name == scope))
        return true;
    }

    return false;
  }

  private async Task<bool> IsClientInvalid(HasUserConsentQuery query)
  {
    if (!string.IsNullOrWhiteSpace(query.Username))
      return true;

    var client = await _identityContext
      .Set<Client>()
      .SingleOrDefaultAsync(x => x.Id == query.ClientId);

    return client is null;
  }
}
