using Application.Validation;

namespace Infrastructure.Services.Abstract;

public interface IResourceService
{
    Task<BaseValidationResult> ValidateResources(ICollection<string> resource, string scope);
}