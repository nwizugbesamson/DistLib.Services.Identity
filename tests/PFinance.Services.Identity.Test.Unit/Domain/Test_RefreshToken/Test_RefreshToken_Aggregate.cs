using DistLib;
using NSubstitute;
using RefreshTokenDomain = PFinance.Services.Identity.Domain.RefreshToken;
using Shouldly;

namespace PFinance.Services.Identity.Test.Unit.Domain.Test_RefreshToken;

public class Test_RefreshToken_Aggregate
{
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();

    [Fact]
    public void Test_RefreshTokenIsCreatedSuccessfully_WhenDataIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-refresh-token-123";
        var createdAt = DateTime.Parse("2025-02-01 10:00:00");
        var durationInSeconds = 3600; // 1 hour
        
        _dateTimeProvider.UtcNow.Returns(createdAt);
        
        // Act
        Result<RefreshTokenDomain.RefreshToken> result = RefreshTokenDomain.RefreshToken.Create(userId, token, createdAt, durationInSeconds);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.Id.ShouldNotBeNull();
        result.Value.User.ShouldBe(userId);
        result.Value.Token.ShouldBe(token);
        result.Value.CreatedAt.ShouldBe(createdAt);
        result.Value.ExpiresAt.ShouldBe(createdAt.AddSeconds(durationInSeconds));
        result.Value.UsedAt.ShouldBeNull();
        result.Value.RevokedAt.ShouldBeNull();
        result.Value.Revoked.ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void Test_RefreshTokenIsNotCreated_WhenTokenIsInvalid(string token)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createdAt = DateTime.Parse("2025-02-01 10:00:00");
        var durationInSeconds = 3600;
        
        // Act
        Result<RefreshTokenDomain.RefreshToken> result = RefreshTokenDomain.RefreshToken.Create(userId, token, createdAt, durationInSeconds);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe("INVALID_TOKEN");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3600)]
    public void Test_RefreshTokenIsNotCreated_WhenDurationIsInvalid(int durationInSeconds)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-refresh-token-123";
        var createdAt = DateTime.Parse("2025-02-01 10:00:00");
        
        // Act
        Result<RefreshTokenDomain.RefreshToken> result = RefreshTokenDomain.RefreshToken.Create(userId, token, createdAt, durationInSeconds);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldNotBeNull();
        result.Error.Code.ShouldBe("INVALID_TOKEN");
        result.Error.Message.ShouldContain("expired");
    }

    [Fact]
    public void Test_RefreshTokenUseSucceeds_WhenTokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-refresh-token-123";
        var createdAt = DateTime.Parse("2025-02-01 10:00:00");
        var usedAt = DateTime.Parse("2025-02-01 10:30:00");
        var durationInSeconds = 3600;
        
        var refreshToken = RefreshTokenDomain.RefreshToken.Create(userId, token, createdAt, durationInSeconds).Value;
        
        // Act
        Result result = refreshToken.Use(usedAt);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        refreshToken.UsedAt.ShouldBe(usedAt);
    }

    [Fact]
    public void Test_RefreshTokenUseFails_WhenTokenIsRevoked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-refresh-token-123";
        var createdAt = DateTime.Parse("2025-02-01 10:00:00");
        var revokedAt = DateTime.Parse("2025-02-01 10:15:00");
        var usedAt = DateTime.Parse("2025-02-01 10:30:00");
        var durationInSeconds = 3600;
        
        var refreshToken = RefreshTokenDomain.RefreshToken.Create(userId, token, createdAt, durationInSeconds).Value;
        refreshToken.Revoke(revokedAt);
        
        // Act
        Result result = refreshToken.Use(usedAt);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("INVALID_TOKEN");
        result.Error.Message.ShouldContain("revoked");
        refreshToken.UsedAt.ShouldBeNull();
    }

    [Fact]
    public void Test_RefreshTokenUseFails_WhenTokenIsAlreadyUsed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-refresh-token-123";
        var createdAt = DateTime.Parse("2025-02-01 10:00:00");
        var firstUsedAt = DateTime.Parse("2025-02-01 10:30:00");
        var secondUsedAt = DateTime.Parse("2025-02-01 10:45:00");
        var durationInSeconds = 3600;
        
        var refreshToken = RefreshTokenDomain.RefreshToken.Create(userId, token, createdAt, durationInSeconds).Value;
        refreshToken.Use(firstUsedAt);
        
        // Act
        Result result = refreshToken.Use(secondUsedAt);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("INVALID_TOKEN");
        result.Error.Message.ShouldContain("already used");
        refreshToken.UsedAt.ShouldBe(firstUsedAt);
    }

    [Fact]
    public void Test_RefreshTokenUseFails_WhenTokenIsExpired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-refresh-token-123";
        var createdAt = DateTime.Parse("2025-02-01 10:00:00");
        var usedAt = DateTime.Parse("2025-02-01 11:30:00"); // After expiration
        var durationInSeconds = 3600; // Expires at 11:00:00
        
        var refreshToken = RefreshTokenDomain.RefreshToken.Create(userId, token, createdAt, durationInSeconds).Value;
        
        // Act
        Result result = refreshToken.Use(usedAt);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("INVALID_TOKEN");
        result.Error.Message.ShouldContain("expired");
        refreshToken.UsedAt.ShouldBeNull();
    }

    [Fact]
    public void Test_RefreshTokenRevokeSucceeds_WhenTokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-refresh-token-123";
        var createdAt = DateTime.Parse("2025-02-01 10:00:00");
        var revokedAt = DateTime.Parse("2025-02-01 10:30:00");
        var durationInSeconds = 3600;
        
        var refreshToken = RefreshTokenDomain.RefreshToken.Create(userId, token, createdAt, durationInSeconds).Value;
        
        // Act
        Result result = refreshToken.Revoke(revokedAt);
        
        // Assert
        result.IsSuccess.ShouldBeTrue();
        refreshToken.RevokedAt.ShouldBe(revokedAt);
        refreshToken.Revoked.ShouldBeTrue();
    }

    [Fact]
    public void Test_RefreshTokenRevokeFails_WhenTokenIsAlreadyRevoked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = "valid-refresh-token-123";
        var createdAt = DateTime.Parse("2025-02-01 10:00:00");
        var firstRevokedAt = DateTime.Parse("2025-02-01 10:30:00");
        var secondRevokedAt = DateTime.Parse("2025-02-01 10:45:00");
        var durationInSeconds = 3600;
        
        var refreshToken = RefreshTokenDomain.RefreshToken.Create(userId, token, createdAt, durationInSeconds).Value;
        refreshToken.Revoke(firstRevokedAt);
        
        // Act
        Result result = refreshToken.Revoke(secondRevokedAt);
        
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("INVALID_TOKEN");
        result.Error.Message.ShouldContain("already revoked");
        refreshToken.RevokedAt.ShouldBe(firstRevokedAt);
    }



    
}