using BidNet.Features.Common.Abstractions;
using BidNet.Features.Common.Models;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace BidNet.Features.Users.Commands;

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
    private readonly ICurrentUserService _userService;

    public RefreshTokenCommandHandler(IIdentityService identityService, ICurrentUserService userService)
    {
        _identityService = identityService;
        _userService = userService;
    }

    public async Task<ErrorOr<AuthenticationResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RefreshTokenAsync(_userService.UserId, request.RefreshToken);
    }
}
