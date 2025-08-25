namespace PFinance.Services.Identity.Application.Services;

public interface IRandomGenerator
{
    string Generate(int length = 50, bool removeSpecialChars = false);
}