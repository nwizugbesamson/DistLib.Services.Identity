namespace PFinance.Services.Identity.Application.Services;

public interface IUserReadService
{
    Task<bool> UserExistsByEmailAsync(string email);
}