using PFinance.Services.Identity.Application.UserAccount;

namespace PFinance.Services.Identity.Application.Services;

public interface IUserReadService
{
    Task<bool> UserExistsByEmailAsync(string email);
    Task<UserApplicationProfileDto?> GetUserApplicationProfileByEmailAsync(string email);
}