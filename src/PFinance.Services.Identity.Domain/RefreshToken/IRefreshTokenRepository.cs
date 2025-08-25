namespace PFinance.Services.Identity.Domain.RefreshToken;

public interface IRefreshTokenRepository
{
    public Task<RefreshToken?> GetAsync(string refreshToken, CancellationToken cancellationToken);
    public Task CreateAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
}