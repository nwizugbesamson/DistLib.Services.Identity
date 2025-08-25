namespace PFinance.Services.Identity.Domain.User;

public interface IUserRepository
{
    Task CreateAsync(User user, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task UpdateAsync(User user, CancellationToken cancellationToken);
}