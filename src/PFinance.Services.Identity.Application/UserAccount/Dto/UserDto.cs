using PFinance.Services.Identity.Common.Enums;

namespace PFinance.Services.Identity.Application.UserAccount;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<string> Permissions { get; set; } = new List<string>();
}

public class UserApplicationProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public UserRole Role { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<string> Permissions { get; set; } = new List<string>();
}