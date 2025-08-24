using System.Reflection;

namespace PFinance.Services.Identity.Application;

public static class ApplicationAssembly
{
    /// <summary>
    /// Gets the application assembly.
    /// </summary>
    public static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
}