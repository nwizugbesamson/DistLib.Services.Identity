using PFinance.Services.Identity.Common.Enums;

namespace PFinance.Services.Identity.Domain.Role;

public interface IRoleRepository
{
    Task CreateAsync(Role role, CancellationToken cancellationToken);
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Role?> GetByNameAsync(UserRole name, CancellationToken cancellationToken);
    Task UpdateAsync(Role role, CancellationToken cancellationToken);
}