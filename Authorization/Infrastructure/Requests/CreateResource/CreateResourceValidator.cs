using Application;
using Application.Validation;
using System.Net;
using Domain;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Infrastructure.Requests.CreateResource;
public class CreateResourceValidator : IValidator<CreateResourceCommand>
{
  private readonly IdentityContext _identityContext;

  public CreateResourceValidator(IdentityContext identityContext)
  {
    _identityContext = identityContext;
  }

  public async Task<ValidationResult> IsValidAsync(CreateResourceCommand value)
  {
    if(string.IsNullOrWhiteSpace(value.ResourceName))
      return GetInvalidResourceMetadataResult("resource_name is invalid");

    if (await IsScopesInvalidAsync(value))
      return GetInvalidResourceMetadataResult("scope is invalid");

    return new ValidationResult(HttpStatusCode.OK);
  }

  private async Task<bool> IsScopesInvalidAsync(CreateResourceCommand command)
  {
    if (!command.Scopes.Any())
      return true;

    var scopes = await _identityContext
      .Set<Scope>()
      .Where(x => command.Scopes.Contains(x.Name))
      .ToListAsync();

    if (!command.Scopes.All(x => scopes.Any(y => y.Name == x)))
      return true;

    return false;
  }

  private static ValidationResult GetInvalidResourceMetadataResult(string errorDescription)
  {
    return new ValidationResult(ErrorCode.InvalidResourceMetadata, errorDescription, HttpStatusCode.BadRequest);
  }
}
