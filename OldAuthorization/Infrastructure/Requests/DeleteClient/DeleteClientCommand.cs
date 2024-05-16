using MediatR;

namespace Infrastructure.Requests.DeleteClient;

#nullable disable
public record DeleteClientCommand(string ClientId, string Token) : IRequest<DeleteClientResponse>;