using BidNet.Domain.Entities;
using BidNet.Features.Authentication.Models;
using BidNet.Features.Authentication.Services;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace BidNet.Features.Authentication.Commands;

public record RegisterCommand(string Username, string Email, string Password, string Role) : IRequest<ErrorOr<AuthResponse>>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Role).NotEmpty();
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ErrorOr<AuthResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(
        UserManager<User> userManager, 
        ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<ErrorOr<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new User { UserName = request.Username, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);
        
        if (!result.Succeeded)
        {
            return Error.Validation(description: string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        
        await _userManager.AddToRoleAsync(user, request.Role);
        var roles = new[] { request.Role };
        var authResult = await _tokenService.GenerateTokensAsync(user, roles);
        
        return new AuthResponse
        {
            UserId = user.Id,
            Username = user.UserName!,
            Email = user.Email!,
            Roles = roles,
            AccessToken = authResult.AccessToken,
            RefreshToken = authResult.RefreshToken,
            TokenExpiration = authResult.ExpiresAt
        };
    }
}
