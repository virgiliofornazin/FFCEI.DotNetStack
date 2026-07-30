using FFCEI.DotNetStack.AccessControl.Interfaces;
using FFCEI.DotNetStack.Framework;
using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.Common;

public abstract class SessionManager(ILogger logger)
    : ISessionManager
{
    private bool _disposed;
    public ILogger Logger { get; internal set; } = logger;

    protected void ThrowIfDisposed()
        => ObjectDisposedException.ThrowIf(_disposed, this);

    protected static void ValidatePolicies(
        ISessionExpiration sessionExpiration,
        IDeviceIdentification deviceIdentification,
        ISessionManagementPolicies policies)
    {
        ArgumentNullException.ThrowIfNull(sessionExpiration, nameof(sessionExpiration));
        ArgumentNullException.ThrowIfNull(deviceIdentification, nameof(deviceIdentification));
        ArgumentNullException.ThrowIfNull(policies, nameof(policies));

        if (!sessionExpiration.HasExpiration() && policies.RequiredExpiration)
        {
            throw new InvalidOperationException("No expiration defined");
        }

        var minimumDuration = policies.MinimumDuration;
        var maximumDuration = policies.MaximumDuration;

        ISessionManagementPolicy? devicePolicy = null;

        var hasDevicePolicy = policies.MaximumSessionsPerAccountSpecificDevice?.TryGetValue(deviceIdentification.DeviceType, out devicePolicy) ?? false;

        if (hasDevicePolicy && (devicePolicy != null))
        {
            minimumDuration = devicePolicy!.MinimumDuration;
            maximumDuration = devicePolicy!.MaximumDuration;
        }

        if (sessionExpiration.ExpiresTimestamp.HasValue)
        {
            var expirationInterval = sessionExpiration.ExpiresTimestamp.Value - sessionExpiration.CreatedTimestamp;

            if ((minimumDuration != null) && (expirationInterval < minimumDuration))
            {
                expirationInterval = minimumDuration.Value;
            }

            if ((maximumDuration != null) && (expirationInterval > maximumDuration))
            {
                expirationInterval = maximumDuration.Value;
            }

            sessionExpiration.ExpiresTimestamp = sessionExpiration.CreatedTimestamp.Add(expirationInterval);
        }
        else
        {
            if (maximumDuration != null)
            {
                sessionExpiration.ExpiresTimestamp = sessionExpiration.CreatedTimestamp.Add(maximumDuration.Value);
            }
        }

        if (sessionExpiration.InactivityExpirationInterval.HasValue)
        {
            var expirationInterval = sessionExpiration.InactivityExpirationInterval;

            if ((minimumDuration != null) && (expirationInterval < minimumDuration))
            {
                expirationInterval = minimumDuration.Value;
            }

            if ((maximumDuration != null) && (expirationInterval > maximumDuration))
            {
                expirationInterval = maximumDuration.Value;
            }

            sessionExpiration.InactivityExpirationInterval = expirationInterval;
        }
        else
        {
            sessionExpiration.InactivityExpirationInterval = maximumDuration;
        }
    }

    protected abstract Task ExpireExceedingSessionsUsingPolicies(
        string tenantId,
        string accountId,
        DeviceIdentificationType deviceIdentificationType,
        ISessionManagementPolicies policies,
        CancellationToken cancellationToken = default);

    protected abstract Task<IReadOnlyCollection<ISession>> GetSessionsByTokenAsync(
        string? tenantId,
        string token,
        CancellationToken cancellationToken = default);

    protected abstract Task TouchSessionAsync(
        ISession session,
        CancellationToken cancellationToken = default);

    protected virtual Task ExpireSessionAsync(
        ISession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session, nameof(session));

        return ExpireSessionByExternalUniqueIdsAsync(session.TenantId, [session.ExternalUniqueId], cancellationToken);
    }

    public abstract Task<ISession> NewSessionAsync(
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

    public abstract Task UpdateSessionTokenAsync(
        string tenantId,
        Guid externalUniqueId,
        string token,
        string refreshToken,
        CancellationToken cancellationToken = default);

    public abstract Task ExpireSessionByTokensAsync(
        string tenantId,
        IEnumerable<string> tokens,
        CancellationToken cancellationToken = default);

    public abstract Task ExpireSessionByExternalUniqueIdsAsync(
        string tenantId,
        IEnumerable<Guid> externalUniqueIds,
        CancellationToken cancellationToken = default);

    public virtual async Task<ISession?> GetSessionByTokenAsync(
        string tenantId,
        string token,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var sessions = await GetSessionsByTokenAsync(tenantId, token, cancellationToken).ConfigureAwait(false);

        return sessions.FirstOrDefault();
    }

    public abstract Task<ISession?> GetSessionByExternalUniqueIdAsync(
        string tenantId,
        Guid externalUniqueId,
        CancellationToken cancellationToken = default);

    public abstract Task<IReadOnlyCollection<ISession>> GetSessionsAsync(
        string tenantId,
        int? pageIndex = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default);

    public abstract Task<IReadOnlyCollection<ISession>> GetSessionsByAccountAsync(
        string tenantId,
        string accountId,
        CancellationToken cancellationToken = default);

    public async Task<bool> ValidateTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentException.ThrowIfNullOrWhiteSpace(token, nameof(token));

        var foundSessions = await GetSessionsByTokenAsync(null, token, cancellationToken).ConfigureAwait(false);

        foreach (var foundSession in foundSessions)
        {
            var expired = foundSession.SessionExpired();

            if (expired)
            {
                await ExpireSessionAsync(foundSession, cancellationToken).ConfigureAwait(false);
            }
        }

        var validSession = foundSessions.FirstOrDefault(session => !session.SessionExpired());

        if (validSession == null)
        {
            return false;
        }

        await TouchSessionAsync(validSession, cancellationToken).ConfigureAwait(false);

        return true;
    }

    public async Task<bool> ValidateRefreshTokenAsync(
        string token,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentException.ThrowIfNullOrWhiteSpace(token, nameof(token));
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken, nameof(refreshToken));

        var foundSessions = await GetSessionsByTokenAsync(null, token, cancellationToken).ConfigureAwait(false);

        foreach (var foundSession in foundSessions)
        {
            var expired = foundSession.SessionExpired();

            if (expired)
            {
                await ExpireSessionAsync(foundSession, cancellationToken).ConfigureAwait(false);
            }
        }

        var validSession = foundSessions.FirstOrDefault(session => session.Token == refreshToken && !session.SessionExpired());

        if (validSession == null)
        {
            return false;
        }

        await TouchSessionAsync(validSession, cancellationToken).ConfigureAwait(false);

        return true;
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            OnDispose();

            _disposed = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await ExceptionProtector.ExecuteValueTaskAsync(OnDisposeAsync()).ConfigureAwait(false);

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    protected virtual void OnDispose()
    {
    }

    protected virtual ValueTask OnDisposeAsync()
        => ValueTask.CompletedTask;
}
