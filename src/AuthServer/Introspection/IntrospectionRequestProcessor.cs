using AuthServer.Core.RequestProcessing;
using AuthServer.Introspection.Abstractions;
using AuthServer.RequestAccessors.Introspection;

namespace AuthServer.Introspection;

internal class
    IntrospectionRequestProcessor : RequestProcessor<IntrospectionRequest, IntrospectionValidatedRequest, IntrospectionResponse>
{
    private readonly ITokenIntrospection _tokenIntrospection;
    private readonly IRequestValidator<IntrospectionRequest, IntrospectionValidatedRequest> _requestValidator;

    public IntrospectionRequestProcessor(
        ITokenIntrospection tokenIntrospection,
        IRequestValidator<IntrospectionRequest, IntrospectionValidatedRequest> requestValidator)
    {
        _tokenIntrospection = tokenIntrospection;
        _requestValidator = requestValidator;
    }

    protected override async Task<ProcessResult<IntrospectionResponse, ProcessError>> ProcessRequest(
        IntrospectionValidatedRequest request, CancellationToken cancellationToken)
    {
        return await _tokenIntrospection.GetIntrospection(request, cancellationToken);
    }

    protected override async Task<ProcessResult<IntrospectionValidatedRequest, ProcessError>> ValidateRequest(IntrospectionRequest request,
        CancellationToken cancellationToken)
    {
        return await _requestValidator.Validate(request, cancellationToken);
    }
}