using MediatR;

namespace Infrastructure.Requests.ReadClient;

public record ReadClientQuery(string Token) : IRequest<ReadClientResponse>;