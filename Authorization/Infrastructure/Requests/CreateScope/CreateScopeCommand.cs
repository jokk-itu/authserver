using MediatR;

namespace Infrastructure.Requests.CreateScope;

#nullable disable
public class CreateScopeCommand : IRequest<CreateScopeResponse>
{
  public string ScopeName { get; init; }
}