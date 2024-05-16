using MediatR;

namespace Infrastructure.Requests.ReadClient;

public record ReadClientQuery(string ClientId, string Token) : IRequest<ReadClientResponse>;