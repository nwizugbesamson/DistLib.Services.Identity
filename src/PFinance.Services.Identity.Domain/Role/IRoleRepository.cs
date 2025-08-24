using PFinance.Services.Identity.Common.Enums;

namespace PFinance.Services.Identity.Domain.Role;

public interface IRoleRepository
{
    Task CreateAsync(Role role);
    Task<Role?> GetByIdAsync(Guid id);
    Task<Role?> GetByNameAsync(UserRole name);
    Task UpdateAsync(Role role);
}