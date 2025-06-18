using BidNet.Features.Authentication.Models;
using BidNet.Features.Authentication.Services;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace BidNet.Features.Authentication.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<ErrorOr<TokenDto>>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ErrorOr<TokenDto>>
{
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }
    
    public async Task<ErrorOr<TokenDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await _tokenService.RefreshTokenAsync(request.RefreshToken);
    }
}
