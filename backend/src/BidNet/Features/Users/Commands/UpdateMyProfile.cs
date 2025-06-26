using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Shared.Services;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Users.Commands;

public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(100);

        When(x => !string.IsNullOrEmpty(x.NewPassword), () =>
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage("Current password is required when setting a new password");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(50);
        });
    }
}

public record UpdateMyProfileCommand(string UserName, string Email, string? CurrentPassword, string? NewPassword) 
    : IRequest<ErrorOr<User>>;

public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, ErrorOr<User>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<User> _userManager;

    public UpdateMyProfileCommandHandler(
        AppDbContext dbContext, 
        ICurrentUserService currentUserService,
        UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _userManager = userManager;
    }

    public async Task<ErrorOr<User>> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(_currentUserService.UserId.ToString());
        
        if (user is null)
        {
            return Error.NotFound(description: "User not found");
        }

        // Check for email uniqueness if email is being changed
        if (user.Email != request.Email)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null && existingUser.Id != _currentUserService.UserId)
            {
                return Error.Conflict(description: "Email is already in use");
            }
        }

        // Check for username uniqueness if username is being changed
        if (user.UserName != request.UserName)
        {
            var existingUser = await _userManager.FindByNameAsync(request.UserName);
            if (existingUser != null && existingUser.Id != _currentUserService.UserId)
            {
                return Error.Conflict(description: "UserName is already in use");
            }
        }

        // Update password if requested
        if (!string.IsNullOrEmpty(request.NewPassword) && !string.IsNullOrEmpty(request.CurrentPassword))
        {
            // Verify current password
            var passwordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
            if (!passwordValid)
            {
                return Error.Validation(description: "Current password is incorrect");
            }

            // Change password
            var changePasswordResult = await _userManager.ChangePasswordAsync(
                user, 
                request.CurrentPassword, 
                request.NewPassword);
                
            if (!changePasswordResult.Succeeded)
            {
                var errors = changePasswordResult.Errors.Select(e => e.Description);
                return Error.Validation(description: string.Join(", ", errors));
            }
        }

        // Update user details
        user.UpdateProfile(request.UserName, request.Email);
        
        // Update user with UserManager
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = updateResult.Errors.Select(e => e.Description);
            return Error.Validation(description: string.Join(", ", errors));
        }
        
        return user;
    }
}
