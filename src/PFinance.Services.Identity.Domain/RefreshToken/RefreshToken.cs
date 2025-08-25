using DistLib;
using DistLib.Domain;

namespace PFinance.Services.Identity.Domain.RefreshToken;

public class RefreshToken : Aggregate<AggregateId>
{
    public Guid User { get; private set; }
    public string Token { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public bool Revoked => RevokedAt.HasValue;
    
    private RefreshToken(AggregateId id, Guid user, string token, DateTime createdAt, DateTime expiresAt) =>
        (Id, User, Token, CreatedAt, ExpiresAt) = (id, user, token, createdAt, expiresAt);
    
    public static Result<RefreshToken> Create(Guid user, string token, DateTime dateTime, int durationInSeconds)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Result.Failure<RefreshToken>(new Error("INVALID_TOKEN", ""));
        
        DateTime expiresAt = dateTime.AddSeconds(durationInSeconds);
        if (expiresAt <= dateTime)
            return Result.Failure<RefreshToken>(new Error("INVALID_TOKEN", "attempted to create expired refresh token"));
        
        return Result.Success(new RefreshToken(AggregateId.New(), user, token, dateTime, expiresAt));
    }

    public Result Use(DateTime usedAt)
    {
        if (Revoked)
            return Result.Failure(new Error("INVALID_TOKEN", "Token is revoked."));

        if (UsedAt.HasValue)
            return Result.Failure(new Error("INVALID_TOKEN", "Token already used."));
        
        if (usedAt >= ExpiresAt)
            return Result.Failure(new Error("INVALID_TOKEN", "Token is expired."));

        UsedAt = usedAt;
        return Result.Success();
    }

    public Result Revoke(DateTime revokedAt)
    {
        if (Revoked)
            return Result.Failure(new Error("INVALID_TOKEN", "Token already revoked."));
        
        RevokedAt = revokedAt;
        return Result.Success();
    }
    
}