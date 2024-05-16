using MediatR;

namespace Infrastructure.Requests.GetUserInfo;

#nullable disable
public class GetUserInfoQuery : IRequest<GetUserInfoResponse>
{
  public string AccessToken { get; init; }
}