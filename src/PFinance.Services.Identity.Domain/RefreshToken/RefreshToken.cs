using DistLib;
using DistLib.Domain;

namespace PFinance.Services.Identity.Domain.RefreshToken;

public class RefreshToken : Aggregate<AggregateId>
{
    public Guid User { get; private set; }
    public string Token { get; private set; }
    public DateRange Period { get; private set; }

    public bool Revoked => Period.HasEndDate;
    
    private RefreshToken(Guid user, string token, DateRange period) =>
        (User, Token, Period) = (user, token, period);
    
    public static Result<RefreshToken> Create(Guid user, string token, DateTime dateTime)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Result.Failure<RefreshToken>(new Error("INVALID_TOKEN", ""));
        
        var periodResult = DateRange.Create(dateTime);
        if (periodResult.IsFailure)
            return Result.Failure<RefreshToken>(periodResult.Error);
        
        return Result.Success(new RefreshToken(user, token, periodResult.Value));
    }

    public Result Revoke(DateTime dateTime)
    {
        if (Revoked)
            return Result.Failure<RefreshToken>(new Error("INVALID_TOKEN", "cannot revoke invalid token"));
        
        var periodResult = DateRange.Create(dateTime);
        if (periodResult.IsFailure)
            return Result.Failure<RefreshToken>(periodResult.Error);
        
        Period = periodResult.Value;
        return Result.Success();
    }
}