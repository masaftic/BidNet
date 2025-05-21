using BidNet.Data.Persistence;
using BidNet.Domain.Entities;
using BidNet.Shared.Abstractions;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BidNet.Features.Users.Commands;

public class UpdateMyProfileCommandValidator : AbstractValidator<UpdateMyProfileCommand>
{
    public UpdateMyProfileCommandValidator()
    {
        RuleFor(x => x.Username)
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

public record UpdateMyProfileCommand(string Username, string Email, string? CurrentPassword, string? NewPassword) 
    : IRequest<ErrorOr<User>>;

public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, ErrorOr<User>>
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMyProfileCommandHandler(AppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<User>> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == _currentUserService.UserId, cancellationToken);
        
        if (user is null)
        {
            return Error.NotFound(description: "User not found");
        }

        // Check for email uniqueness if email is being changed
        if (user.Email != request.Email)
        {
            var emailExists = await _dbContext.Users.AnyAsync(
                u => u.Email == request.Email && u.Id != _currentUserService.UserId, 
                cancellationToken);
                
            if (emailExists)
            {
                return Error.Conflict(description: "Email is already in use");
            }
        }

        // Check for username uniqueness if username is being changed
        if (user.Username != request.Username)
        {
            var usernameExists = await _dbContext.Users.AnyAsync(
                u => u.Username == request.Username && u.Id != _currentUserService.UserId, 
                cancellationToken);
                
            if (usernameExists)
            {
                return Error.Conflict(description: "Username is already in use");
            }
        }

        // Update password if requested
        if (!string.IsNullOrEmpty(request.NewPassword) && !string.IsNullOrEmpty(request.CurrentPassword))
        {
            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return Error.Validation(description: "Current password is incorrect");
            }

            // Hash and set new password
            user.UpdatePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
        }

        // Update user details
        user.UpdateProfile(request.Username, request.Email);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return user;
    }
}
