using Infrastructure.Requests.Abstract;
using MediatR;

namespace Infrastructure.Requests.CreateAuthorizationGrant;

#nullable disable
public class CreateAuthorizationGrantCommand : AuthorizeRequest, IRequest<CreateAuthorizationGrantResponse>
{
  public string UserId { get; set; }
}