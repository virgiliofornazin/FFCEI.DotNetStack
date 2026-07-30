namespace FFCEI.DotNetStack.AccessControl.Interfaces;

public interface ISecurityTokenValidator
{
    Task<bool> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<bool> ValidateRefreshTokenAsync(
        string token,
        string refreshToken,
        CancellationToken cancellationToken = default);
}
