namespace PFinance.Services.Identity.Application.UserAccount;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<string> Permissions { get; set; }
}

public class UserApplicationProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public IEnumerable<string> Permissions { get; set; }
}