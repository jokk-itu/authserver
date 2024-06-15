namespace AuthServer.Introspection;

internal interface ITokenIntrospection
{
    Task<IntrospectionResponse> GetIntrospection(IntrospectionValidatedRequest request, CancellationToken cancellationToken);
}