using BidNet.Domain.Entities;
using BidNet.Domain.Enums;
using BidNet.Features.Common.Abstractions;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace BidNet.Features.Users.Commands;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.Role)
            .IsInEnum()
            .WithMessage("Invalid user role. Allowed values are: Bidder, Seller.")
            .Must(role => role != UserRole.Admin)
            .WithMessage("Admin role is not allowed for registration.");
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

    public Task<ErrorOr<User>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return _identityService.CreateUserAsync(
            request.Username,
            request.Email,
            request.Password,
            request.Role);
    }
}
