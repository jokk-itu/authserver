namespace AuthServer.Introspection.Abstractions;

internal interface ITokenIntrospection
{
    Task<IntrospectionResponse> GetIntrospection(IntrospectionValidatedRequest request, CancellationToken cancellationToken);
}