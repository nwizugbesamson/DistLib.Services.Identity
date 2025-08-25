using PFinance.Services.Identity.Common.Enums;

namespace PFinance.Services.Identity.Application.UserAccount;

public class AuthDto
{
    public string Email { get; set; }
    public UserRole Role { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public long ExpiresAt { get; set; }
}