using BidNet.Domain.Entities;
using BidNet.Domain.Enums;
using BidNet.Features.Users.Abstractions;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace BidNet.Features.Auth.Commands;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(50);
    }
}

public record RegisterCommand(string Username, string Email, string Password, UserRole Role = UserRole.Bidder) 
    : IRequest<ErrorOr<User>>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<User>>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<ErrorOr<User>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.CreateUserAsync(
            request.Username,
            request.Email,
            request.Password,
            request.Role);
    }
}
