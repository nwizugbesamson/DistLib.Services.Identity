namespace PFinance.Services.Identity.Domain.User;

public interface IUserRepository
{
    Task CreateAsync(User user);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(string id);
    Task UpdateAsync(User user);
}