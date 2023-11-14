﻿using MediatR;

namespace Infrastructure.Requests.RedeemAuthorizationCodeGrant;

#nullable disable
public class RedeemAuthorizationCodeGrantCommand : IRequest<RedeemAuthorizationCodeGrantResponse>
{
  public string GrantType { get; init; }
  public string Code { get; init; }

  public string ClientId { get; init; }

  public string ClientSecret { get; init; }

  public string RedirectUri { get; init; }

  public string CodeVerifier { get; init; }
  public ICollection<string> Resource { get; init; }
}
