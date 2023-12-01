using Application;
using Application.Validation;
using Domain;
using Infrastructure.Services.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ResourceService : IResourceService
{
    private readonly IdentityContext _identityContext;

    public ResourceService(
        IdentityContext identityContext)
    {
        _identityContext = identityContext;
    }

    public async Task<BaseValidationResult> ValidateResources(ICollection<string> resource, string scope)
    {
        if (!resource.Any())
        {
            return new BaseValidationResult(ErrorCode.InvalidTarget, "resource is empty");
        }

        var scopes = scope.Split(' ');
        var resourcesExisting = await _identityContext
            .Set<Resource>()
            .Where(r => resource.Contains(r.Uri))
            .Where(r => r.Scopes.AsQueryable().Any(s => scopes.Contains(s.Name)))
            .CountAsync();

        var isResourcesValid = resourcesExisting == resource.Count;
        if (isResourcesValid)
        {
            return new BaseValidationResult();
        }

        return new BaseValidationResult(ErrorCode.InvalidTarget, "resource is invalid");
    }
}