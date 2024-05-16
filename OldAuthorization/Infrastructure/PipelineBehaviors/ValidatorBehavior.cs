using System.Diagnostics;
using System.Net;
using Application;
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

  private const string ErrorDescription = "uncaught error occurred";

  public ValidatorBehavior(IValidator<TRequest> validator, ILogger<ValidatorBehavior<TRequest, TResponse>> logger)
  {
    _validator = validator;
    _logger = logger;
  }

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    var requestName = typeof(TRequest).Name;
    var stopWatch = Stopwatch.StartNew();
    try
    {
      var validationResult = await _validator.ValidateAsync(request, cancellationToken);
      if (!validationResult.IsError())
      {
        _logger.LogInformation("Validated {Request} successfully, took {ElapsedTime} ms", requestName, stopWatch.ElapsedMilliseconds);
        return await next();
      }

      _logger.LogInformation(
        "Validated {Request} with error {ErrorCode} and description {ErrorDescription}, took {ElapsedTime} ms",
        requestName,
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
      throw new InvalidOperationException($"Error occurred instantiating {requestName}");
    }
    catch (Exception e)
    {
      _logger.LogError(e, "Error occurred during validation of {Request}, took {ElapsedTime}", requestName, stopWatch.ElapsedMilliseconds);
      if (Activator.CreateInstance(typeof(TResponse), ErrorCode.ServerError, ErrorDescription, HttpStatusCode.BadRequest) is not TResponse response)
      {
        throw new AggregateException(e, new InvalidOperationException($"Error occurred instantiating {requestName}"));
      }

      return response;
    }
  }
}
