using BidNet.Features.Auth.Models;
using BidNet.Features.Users.Abstractions;
using BidNet.Shared.Services;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace BidNet.Features.Auth.Commands;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}

public record RefreshTokenCommand(string RefreshToken) : IRequest<ErrorOr<AuthenticationResult>>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ErrorOr<AuthenticationResult>>
{
    private readonly IIdentityService _identityService;
    private readonly ICurrentUserService _currentUserService;

    public RefreshTokenCommandHandler(IIdentityService identityService, ICurrentUserService currentUserService)
    {
        _identityService = identityService;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<AuthenticationResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RefreshTokenAsync(_currentUserService.UserId, request.RefreshToken);
    }
}
