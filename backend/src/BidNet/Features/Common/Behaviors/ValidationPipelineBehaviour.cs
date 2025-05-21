using ErrorOr;
using FluentValidation;
using MediatR;

namespace BidNet.Features.Common.Behaviors;

public sealed class ValidationPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly IValidator<TRequest>? _validator;

    public ValidationPipelineBehavior(IValidator<TRequest>? validator = null)
    {
        _validator = validator;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validator is null)
        {
            return await next(cancellationToken);
        }

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (validationResult.IsValid)
        {
            return await next(cancellationToken);
        }

        var errors = validationResult.Errors.ConvertAll(validationFailure =>
                Error.Validation(validationFailure.PropertyName,
                                 validationFailure.ErrorMessage));

        return (dynamic)errors;
    }
}