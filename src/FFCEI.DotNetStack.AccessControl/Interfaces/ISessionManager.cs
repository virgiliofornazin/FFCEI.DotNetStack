namespace FFCEI.DotNetStack.AccessControl.Interfaces;

public interface ISessionManager
    : ISecurityTokenValidator, IDisposable, IAsyncDisposable
{
    Task<ISession> NewSessionAsync(
        string tenantId,
        string accountId,
        string token,
        string refreshToken,
        ISessionManagementPolicies policies,
        IDeviceIdentification? deviceIdentification = null,
        TimeSpan? inactivityExpirationInterval = null,
        DateTimeOffset? expiresTimestamp = null,
        IReadOnlyDictionary<string, string>? attributes = null,
        CancellationToken cancellationToken = default);

    Task UpdateSessionTokenAsync(
        string tenantId,
        Guid universalUniqueId,
        string token,
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task ExpireSessionByTokensAsync(
        string tenantId,
        IEnumerable<string> tokens,
        CancellationToken cancellationToken = default);

    Task ExpireSessionByUniversalUniqueIdsAsync(
        string tenantId,
        IEnumerable<Guid> universalUniqueIds,
        CancellationToken cancellationToken = default);

    Task<ISession?> GetSessionByTokenAsync(
        string tenantId,
        string token,
        CancellationToken cancellationToken = default);

    Task<ISession?> GetSessionByUniversalUniqueIdAsync(
        string tenantId,
        Guid universalUniqueId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ISession>> GetSessionsAsync(
        string tenantId,
        int? pageIndex = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ISession>> GetSessionsByAccountAsync(
        string tenantId,
        string accountId,
        CancellationToken cancellationToken = default);
}
