using System.Net;
using Application;
using Application.Validation;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Requests.CreateScope;
public class CreateScopeValidator : IValidator<CreateScopeCommand>
{
  private readonly IdentityContext _identityContext;

  public CreateScopeValidator(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<ValidationResult> IsValidAsync(CreateScopeCommand value)
  {
    if (await IsScopeNameInvalid(value))
      return new ValidationResult(ErrorCode.InvalidScopeMetadata, "scope is invalid", HttpStatusCode.BadRequest);

    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<bool> IsScopeNameInvalid(CreateScopeCommand command)
  {
    if (string.IsNullOrWhiteSpace(command.ScopeName))
      return true;

    return await _identityContext
      .Set<Scope>()
      .AnyAsync(x => x.Name == command.ScopeName);
  }
}
