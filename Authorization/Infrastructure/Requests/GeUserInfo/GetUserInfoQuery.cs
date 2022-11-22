using MediatR;

namespace Infrastructure.Requests.GeUserInfo;

#nullable disable
public class GetUserInfoQuery : IRequest<GetUserInfoResponse>
{
  public string AccessToken { get; init; }
}