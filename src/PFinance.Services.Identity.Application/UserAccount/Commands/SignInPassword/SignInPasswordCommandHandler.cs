using DistLib;
using PFinance.Services.Identity.Application.Services;
using PFinance.Services.Identity.Domain.RefreshToken;

namespace PFinance.Services.Identity.Application.UserAccount.Commands.SignInPassword;

public class SignInPasswordCommandHandler(IDateTimeProvider dateTimeProvider, IJwtProvider jwtProvider, 
    IUserReadService userReadService, IRefreshTokenRepository refreshTokenRepository, 
    IUnitOfWork unitOfWork) : ICommandHandler<SignInPassword, Result<AuthDto>>
{
    private readonly IUserReadService _userReadService = userReadService;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IJwtProvider  _jwtProvider = jwtProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public Task<Result<AuthDto>> Handle(SignInPassword request, CancellationToken cancellationToken)
    {
        // user readService get UserApplicationProfileDto by email => id, email, role, password hash, permissions
        
        // check password -> password hasher
        
        // Generate access token (id, email, role, claims) - jwt service
        
        // generate refresh token (random generator)
        
        // store refresh token
        
        // store unit of work
        
        // populate and return auth dto
        
        throw new NotImplementedException();
    }
}
