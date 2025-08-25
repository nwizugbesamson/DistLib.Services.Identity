namespace PFinance.Services.Identity.Application.Services;

public interface IPasswordService
{
    string HashPassword(string password);
}