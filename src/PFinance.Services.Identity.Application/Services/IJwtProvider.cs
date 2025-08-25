using PFinance.Services.Identity.Application.UserAccount;
using PFinance.Services.Identity.Common.Enums;

namespace PFinance.Services.Identity.Application.Services;

public interface IJwtProvider
{
    string GenerateAccessToken(Guid userId, string email, UserRole role, IEnumerable<string> any);
    long TokenExpiration { get; }
    long RefreshExpiration { get; set; }
}