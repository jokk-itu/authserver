using Application.Validation;
using Infrastructure.Requests;
using Mapster;

namespace Infrastructure.Mappers;

#nullable disable
public static class ValidationResultMapper
{
  private static readonly TypeAdapterConfig ValidationResultToResponseConfig = new();

  static ValidationResultMapper()
  {
    ValidationResultToResponseConfig
      .NewConfig<ValidationResult, Response>()
      .MapToConstructor(true);
  }

  public static T ToResponse<T>(this ValidationResult result) where T : Response
  {
    return result.Adapt<T>(ValidationResultToResponseConfig);
  }
}