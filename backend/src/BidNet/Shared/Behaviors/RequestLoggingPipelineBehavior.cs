using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BidNet.Features.Common.Behaviors;

public sealed class RequestLoggingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> _logger;

    public RequestLoggingPipelineBehavior(ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;

        _logger.LogInformation("Handling request: {RequestName}", requestName);

        TResponse result = await next(cancellationToken);

        if (result.IsError)
        {
            _logger.LogError("Request {RequestName} failed with errors: {Errors}", requestName, result.Errors);
        }
        else
        {
            _logger.LogInformation("Request {RequestName} succeeded", requestName);
        }

        return result;
    }
}