using System.Diagnostics;
using System.Net;
using Application.Validation;
using MediatR;
using Microsoft.Extensions.Logging;
using Infrastructure.Requests;

namespace Infrastructure.PipelineBehaviors;
public class ValidatorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IRequest<TResponse>
  where TResponse : Response
{
  private readonly IValidator<TRequest> _validator;
  private readonly ILogger<ValidatorBehavior<TRequest, TResponse>> _logger;

  public ValidatorBehavior(IValidator<TRequest> validator, ILogger<ValidatorBehavior<TRequest, TResponse>> logger)
  {
    _validator = validator;
    _logger = logger;
  }

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    var stopWatch = Stopwatch.StartNew();
    try
    {
      var validationResult = await _validator.ValidateAsync(request, cancellationToken);
      if (!validationResult.IsError())
      {
        _logger.LogInformation("Validated {Request} successfully, took {ElapsedTime}", nameof(TRequest), stopWatch.ElapsedMilliseconds);
        return await next();
      }

      _logger.LogInformation(
        "Validated {Request} with {ErrorCode} and {ErrorDescription}, took {ElapsedTime}",
        nameof(TRequest),
        validationResult.ErrorCode,
        validationResult.ErrorDescription,
        stopWatch.ElapsedMilliseconds);

      if (Activator.CreateInstance(
            typeof(TResponse),
            validationResult.ErrorCode,
            validationResult.ErrorDescription,
            validationResult.StatusCode) is TResponse response)
      {
        return response;
      }
      throw new InvalidOperationException($"Error occurred instantiating {typeof(TResponse)}");
    }
    catch (Exception e)
    {
      _logger.LogError(e, "Error occurred during validation, took {ElapsedTime}", stopWatch.ElapsedMilliseconds);
      if (Activator.CreateInstance(typeof(TResponse), HttpStatusCode.BadRequest) is not TResponse response)
      {
        throw new AggregateException(e, new InvalidOperationException($"Error occurred instantiating {typeof(TResponse)}"));
      }

      return response;
    }
  }
}
