using System.Net;
using Application.Validation;
using Domain;
using Infrastructure.Builders.Abstractions;
using MediatR;

namespace Infrastructure.Requests.CreateScope;
public class CreateScopeHandler : IRequestHandler<CreateScopeCommand, CreateScopeResponse>
{
  private readonly IdentityContext _identityContext;
  private readonly IValidator<CreateScopeCommand> _validator;
  private readonly ITokenBuilder _tokenBuilder;

  public CreateScopeHandler(IdentityContext identityContext, IValidator<CreateScopeCommand> validator, ITokenBuilder tokenBuilder)
  {
    _identityContext = identityContext;
    _validator = validator;
    _tokenBuilder = tokenBuilder;
  }

  public async Task<CreateScopeResponse> Handle(CreateScopeCommand request, CancellationToken cancellationToken)
  {
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
    if (validationResult.IsError())
      return new CreateScopeResponse(validationResult.ErrorCode, validationResult.ErrorDescription, HttpStatusCode.BadRequest);

    var scope = new Scope
    {
      Name = request.ScopeName
    };
    await _identityContext
      .Set<Scope>()
      .AddAsync(scope, cancellationToken: cancellationToken);

    await _identityContext.SaveChangesAsync(cancellationToken: cancellationToken);
    var token = _tokenBuilder.BuildScopeRegistrationAccessToken(scope.Id.ToString());
    return new CreateScopeResponse(HttpStatusCode.Created)
    {
      Id = scope.Id,
      ScopeName = scope.Name,
      ScopeRegistrationAccessToken = token
    };
  }
}