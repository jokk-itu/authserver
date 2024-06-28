using AuthServer.Core;
using AuthServer.Core.Request;
using AuthServer.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Register.DeleteClient;

internal class DeleteRegisterRequestProcessor : IRequestProcessor<DeleteRegisterValidatedRequest, Unit>
{
    private readonly AuthorizationDbContext _authorizationDbContext;

    public DeleteRegisterRequestProcessor(
        AuthorizationDbContext authorizationDbContext)
    {
        _authorizationDbContext = authorizationDbContext;
    }

    public async Task<Unit> Process(DeleteRegisterValidatedRequest request, CancellationToken cancellationToken)
    {
        await _authorizationDbContext
            .Set<Client>()
            .Where(c => c.Id == request.ClientId)
            .ExecuteDeleteAsync(cancellationToken);

        return Unit.Value;
    }
}