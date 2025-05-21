using BidNet.Features.Auth.Models;
using BidNet.Features.Users.Abstractions;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace BidNet.Features.Auth.Commands;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty();
    }
}

public record LoginCommand(string Email, string Password) : IRequest<ErrorOr<AuthenticationResult>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ErrorOr<AuthenticationResult>>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<ErrorOr<AuthenticationResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.AuthenticateAsync(request.Email, request.Password);
    }
}
