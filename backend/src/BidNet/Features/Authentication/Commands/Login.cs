using BidNet.Domain.Entities;
using BidNet.Features.Authentication.Models;
using BidNet.Features.Authentication.Services;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Authentication.Commands;

public record LoginCommand(string Email, string Password) : IRequest<ErrorOr<AuthResponse>>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, ErrorOr<AuthResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        UserManager<User> userManager, 
        SignInManager<User> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public async Task<ErrorOr<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
            return Error.NotFound(description: "User not found");
        
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        
        if (!result.Succeeded)
            return Error.Validation(description: "Invalid credentials");
        
        var roles = await _userManager.GetRolesAsync(user);
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
