using DistLib;
using PFinance.Services.Identity.Application.Services;
using PFinance.Services.Identity.Domain.RefreshToken;

namespace PFinance.Services.Identity.Application.UserAccount.Commands.SignInPassword;

public class SignInPasswordCommandHandler(
    IDateTimeProvider dateTimeProvider, 
    IJwtProvider jwtProvider, 
    IUserReadService userReadService, 
    IPasswordService passwordService,
    IRandomGenerator randomGenerator,
    IRefreshTokenRepository refreshTokenRepository, 
    IUnitOfWork unitOfWork) : ICommandHandler<SignInPassword, Result<AuthDto>>
{
    private readonly IUserReadService _userReadService = userReadService;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly IRandomGenerator _randomGenerator = randomGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<AuthDto>> Handle(SignInPassword request, CancellationToken cancellationToken)
    {
        // user readService get UserApplicationProfileDto by email => id, email, role, password hash, permissions
        var userProfile = await _userReadService.GetUserApplicationProfileByEmailAsync(request.Email);
        if (userProfile is null)
        {
            return Result.Failure<AuthDto>(new Error("USER_NOT_FOUND", "User not found"));
        }
        
        // check password -> password hasher
        bool isPasswordValid = _passwordService.VerifyPassword(request.Password, userProfile.PasswordHash);
        if (!isPasswordValid)
        {
            return Result.Failure<AuthDto>(new Error("INVALID_CREDENTIALS", "Invalid email or password"));
        }
        
        // Generate access token (id, email, role, claims) - jwt service
        string accessToken = _jwtProvider.GenerateAccessToken(userProfile.Id, userProfile.Email, userProfile.Role, new List<string>());
        
        // generate refresh token (random generator)
        string refreshTokenValue = _randomGenerator.Generate();
        DateTime createdAt = _dateTimeProvider.UtcNow;
        var refreshTokenResult = RefreshToken.Create(userProfile.Id, refreshTokenValue, createdAt, _jwtProvider.RefreshExpiration);
        if (refreshTokenResult.IsFailure)
        {
            return Result.Failure<AuthDto>(refreshTokenResult.Error);
        }
        var refreshToken = refreshTokenResult.Value;
        // store refresh token
        await _refreshTokenRepository.CreateAsync(refreshToken, cancellationToken);
        
        // store unit of work
        await _unitOfWork.SaveChangesAsync();
        
        // populate and return auth dto
        var authDto = new AuthDto
        {
            Email = userProfile.Email,
            Role = userProfile.Role,
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = _jwtProvider.TokenExpiration
        };
        
        return Result.Success(authDto);
    }
}
