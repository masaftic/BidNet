using Bidnet.Application.Common.Abstractions;
using Bidnet.Application.Common.Models;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Bidnet.Application.Users.Commands;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty();

        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}

public record RefreshTokenCommand(string Token, string RefreshToken) : IRequest<ErrorOr<AuthenticationResult>>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ErrorOr<AuthenticationResult>>
{
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<ErrorOr<AuthenticationResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RefreshTokenAsync(request.Token, request.RefreshToken);
    }
}
