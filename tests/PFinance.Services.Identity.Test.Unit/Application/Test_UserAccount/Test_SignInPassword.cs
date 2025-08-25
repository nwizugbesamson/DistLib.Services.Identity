using DistLib;
using NSubstitute;
using PFinance.Services.Identity.Application.Services;
using PFinance.Services.Identity.Application.UserAccount;
using PFinance.Services.Identity.Application.UserAccount.Commands.SignInPassword;
using PFinance.Services.Identity.Common.Enums;
using PFinance.Services.Identity.Domain.RefreshToken;
using Shouldly;

namespace PFinance.Services.Identity.Test.Unit.Application.Test_UserAccount;

public class Test_SignInPassword
{
    private readonly IRandomGenerator _randomGenerator =  Substitute.For<IRandomGenerator>();
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();
    private readonly IUserReadService _userReadService = Substitute.For<IUserReadService>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IJwtProvider _jwtProvider = Substitute.For<IJwtProvider>();
    private readonly IRefreshTokenRepository _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly SignInPasswordCommandHandler _sut;

    public Test_SignInPassword()
    {
        _sut = new SignInPasswordCommandHandler(_dateTimeProvider, _jwtProvider, 
            _userReadService,_passwordService , _randomGenerator ,_refreshTokenRepository, _unitOfWork);
    }

    [Fact]
    public async Task Test_SignInSucceeds_WhenCredentialsAreValid()
    {
        // Arrange
        var email = "test.user@pfinance.com";
        var password = "password123";
        var userId = Guid.NewGuid();
        var role = UserRole.Admin;
        var hashedPassword = "hashed_password_123";
        var accessToken = "access_token_123";
        var refreshToken = "refresh_token_123";
        var createdAt = DateTime.Parse("2025-02-01 10:00:00");
        var expiresAt = createdAt.AddHours(1);

        var command = new SignInPassword(email, password);
        var userProfile = new UserApplicationProfileDto
        {
            Id = userId,
            Email = email,
            Role = role,
            PasswordHash = hashedPassword,
            CreatedAt = createdAt,
        };

        _userReadService.GetUserApplicationProfileByEmailAsync(email).Returns(userProfile);
        _passwordService.VerifyPassword(password, hashedPassword).Returns(true);
        _jwtProvider.GenerateAccessToken(userId, email, role, Arg.Any<IEnumerable<string>>()).Returns(accessToken);
        _dateTimeProvider.UtcNow.Returns(createdAt);

        var refreshTokenEntity = RefreshToken.Create(userId, refreshToken, createdAt, 3600).Value;
        _refreshTokenRepository.CreateAsync(refreshTokenEntity, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        _unitOfWork.SaveChangesAsync().Returns(Task.CompletedTask);

        // Act
        Result<AuthDto> result = await Act(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.AccessToken.ShouldBe(accessToken);
        result.Value.RefreshToken.ShouldBe(refreshToken);

        await _userReadService.Received(1).GetUserApplicationProfileByEmailAsync(email);
        _passwordService.Received(1).VerifyPassword(password, hashedPassword);
        _jwtProvider.Received(1).GenerateAccessToken(userId, email, role, Arg.Any<IEnumerable<string>>());
        await _refreshTokenRepository.Received(1).CreateAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Test_SignInFails_WhenUserNotFound()
    {
        // Arrange
        var email = "nonexistent@pfinance.com";
        var password = "password123";
        var command = new SignInPassword(email, password);

        _userReadService.GetUserApplicationProfileByEmailAsync(email).Returns(Task.FromResult<UserApplicationProfileDto?>(null));

        // Act
        Result<AuthDto> result = await Act(command);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("USER_NOT_FOUND");
        await _userReadService.Received(1).GetUserApplicationProfileByEmailAsync(email);
        _passwordService.DidNotReceive().VerifyPassword(Arg.Any<string>(), Arg.Any<string>());
        _jwtProvider.DidNotReceive().GenerateAccessToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<UserRole>(), Arg.Any<IEnumerable<string>>());
        await _refreshTokenRepository.DidNotReceive().CreateAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task Test_SignInFails_WhenPasswordIsIncorrect()
    {
        // Arrange
        var email = "test.user@pfinance.com";
        var password = "wrongpassword";
        var userId = Guid.NewGuid();
        var role = UserRole.Admin;
        var hashedPassword = "hashed_password_123";
        var command = new SignInPassword(email, password);
        
        var userProfile = new UserApplicationProfileDto
        {
            Id = userId,
            Email = email,
            Role = role,
            PasswordHash = hashedPassword,
        };

        _userReadService.GetUserApplicationProfileByEmailAsync(email).Returns(userProfile);
        _passwordService.VerifyPassword(password, hashedPassword).Returns(false);

        // Act
        Result<AuthDto> result = await Act(command);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("INVALID_CREDENTIALS");
        await _userReadService.Received(1).GetUserApplicationProfileByEmailAsync(email);
        _passwordService.Received(1).VerifyPassword(password, hashedPassword);
        _jwtProvider.DidNotReceive().GenerateAccessToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<UserRole>(), Arg.Any<IEnumerable<string>>());
        await _refreshTokenRepository.DidNotReceive().CreateAsync(Arg.Any<RefreshToken>(),  Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    // Act
    private async Task<Result<AuthDto>> Act(SignInPassword cmd) => await _sut.Handle(cmd, CancellationToken.None);
}