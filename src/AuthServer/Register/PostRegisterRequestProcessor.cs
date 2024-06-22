using AuthServer.Core.RequestProcessing;
using AuthServer.Register.Abstractions;
using AuthServer.RequestAccessors.Register;
using System.Transactions;

namespace AuthServer.Register;

internal class PostRegisterRequestProcessor : RequestProcessor<RegisterRequest, PostRegisterValidatedRequest, PostRegisterResponse>
{
	private readonly IRequestValidator<RegisterRequest, PostRegisterValidatedRequest> _requestValidator;
    private readonly IPostRegisterProcessor _postRegisterProcessor;

    public PostRegisterRequestProcessor(
		IRequestValidator<RegisterRequest, PostRegisterValidatedRequest> requestValidator,
		IPostRegisterProcessor postRegisterProcessor)
    {
        _requestValidator = requestValidator;
        _postRegisterProcessor = postRegisterProcessor;
    }

	protected override async Task<ProcessResult<PostRegisterResponse, ProcessError>> ProcessRequest(PostRegisterValidatedRequest request, CancellationToken cancellationToken)
    {
        using var transactionScope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled);
        var result = await _postRegisterProcessor.Register(request, cancellationToken);
        transactionScope.Complete();
        return result;
    }

	protected override async Task<ProcessResult<PostRegisterValidatedRequest, ProcessError>> ValidateRequest(RegisterRequest request, CancellationToken cancellationToken)
	{
		return await _requestValidator.Validate(request, cancellationToken);
	}
}