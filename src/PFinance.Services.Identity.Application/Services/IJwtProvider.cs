using PFinance.Services.Identity.Application.UserAccount;

namespace PFinance.Services.Identity.Application.Services;

public interface IJwtProvider
{
    public string GenerateJwtToken(UserDto user);
}