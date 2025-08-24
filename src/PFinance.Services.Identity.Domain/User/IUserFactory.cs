using DistLib;
using PFinance.Services.Identity.Common.Enums;

namespace PFinance.Services.Identity.Domain.User;

public interface IUserFactory
{
    Result<User> CreateUser(string email, string passwordHash, UserRole role, DateTime dateTime);
}