using FFCEI.DotNetStack.AccessControl.Interfaces;
using FFCEI.DotNetStack.AccessControl.SessionManagement.Common;
using FFCEI.DotNetStack.AccessControl.SessionManagement.EntityFramework.Models;
using FFCEI.DotNetStack.EntityFramework;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.EntityFramework;

public sealed class EntityFrameworkSessionManager(IServiceProvider serviceProvider, ILogger logger)
    : SessionManager(logger)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private static Session Map(SessionModel model)
    {
        var deviceIdentification = model.DeviceIdentification;
        var attributes = model.Attributes;

        var result = new Session()
        {
            UniversalUniqueId = model.UniversalUniqueId,
            TenantId = model.TenantId,
            AccountId = model.AccountId,
            Token = model.Token,
            RefreshToken = model.RefreshToken,
            ExpiresTimestamp = model.ExpiresTimestamp,
            InactivityExpirationInterval = model.InactivityExpirationInterval,
            FinalizedTimestamp = model.FinalizedTimestamp,
            CreatedTimestamp = model.CreatedTimestamp,
            UpdatedTimestamp = model.UpdatedTimestamp,
            Attributes = attributes.ToDictionary(attribute => attribute.AttributeName, item => item.AttributeValue)
        };

        if (deviceIdentification != null)
        {
            result.DeviceIdentification = new()
            {
                UserAgent = deviceIdentification.UserAgent,
                Application = deviceIdentification.Application,
                OperatingSystem = deviceIdentification.OperatingSystem,
                MultiTouchPoints = deviceIdentification.MultiTouchPoints,
                DeviceType = deviceIdentification.DeviceType,
                Mobile = deviceIdentification.Mobile
            };
        }

        return result;
    }

    protected override async Task ExpireExceedingSessionsUsingPolicies(
        string tenantId,
        string accountId,
        DeviceIdentificationType deviceIdentificationType,
        ISessionManagementPolicies policies,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenantId, nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(accountId, nameof(accountId));
        ArgumentNullException.ThrowIfNull(policies, nameof(policies));

        List<SessionModel> sessions = [];

        using var scopeAndService = _serviceProvider.CreateScopeAndGetRequiredService<SessionManagerEntityModelRepositoryContainer>();

        if ((policies.MaximumSessionsPerAccountSpecificDevice != null) &&
            (policies.MaximumSessionsPerAccountSpecificDevice.TryGetValue(deviceIdentificationType, out var policy) &&
            (policy.MaximumSessions != null) &&
            (policy.MaximumSessions > 0)))
        {
            sessions = await scopeAndService.RequiredService.Sessions
                .Include(session => session.DeviceIdentification)
                .Where(session => session.TenantId == tenantId && session.AccountId == accountId &&
                    session.DeviceIdentification != null && session.DeviceIdentification.DeviceType == deviceIdentificationType)
                .OrderBy(session => session.CreatedTimestamp)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            await ExpireSessionsInDatabase(sessions, scopeAndService, policy.MaximumSessions.Value, cancellationToken).ConfigureAwait(false);
        }
        else if ((policies.MaximumSessionsPerAccountDevice != null) &&
                (policies.MaximumSessionsPerAccountDevice > 0))
        {
            sessions = await scopeAndService.RequiredService.Sessions
                .Include(session => session.DeviceIdentification)
                .Where(session => session.TenantId == tenantId && session.AccountId == accountId &&
                    session.DeviceIdentification != null && session.DeviceIdentification.DeviceType == deviceIdentificationType)
                .OrderBy(session => session.CreatedTimestamp)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            await ExpireSessionsInDatabase(sessions, scopeAndService, policies.MaximumSessionsPerAccountDevice.Value, cancellationToken).ConfigureAwait(false);
        }

        if ((policies.MaximumSessionsPerAccount != null) &&
            (policies.MaximumSessionsPerAccount > 0))
        {
            sessions = await scopeAndService.RequiredService.Sessions
                .Where(session => session.TenantId == tenantId && session.AccountId == accountId)
                .OrderBy(session => session.CreatedTimestamp)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            await ExpireSessionsInDatabase(sessions, scopeAndService, policies.MaximumSessionsPerAccount.Value, cancellationToken).ConfigureAwait(false);
        }

        if ((policies.MaximumSessionsPerTenant != null) &&
            (policies.MaximumSessionsPerTenant > 0))
        {
            sessions = await scopeAndService.RequiredService.Sessions
                .Where(session => session.TenantId == tenantId)
                .OrderBy(session => session.CreatedTimestamp)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            await ExpireSessionsInDatabase(sessions, scopeAndService, policies.MaximumSessionsPerTenant.Value, cancellationToken).ConfigureAwait(false);
        }

        if ((policies.MaximumSessions != null) &&
            (policies.MaximumSessions > 0))
        {
            sessions = await scopeAndService.RequiredService.Sessions
                .OrderBy(session => session.CreatedTimestamp)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            await ExpireSessionsInDatabase(sessions, scopeAndService, policies.MaximumSessions.Value, cancellationToken).ConfigureAwait(false);
        }

        _ = await scopeAndService.RequiredService.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task ExpireSessionsInDatabase(List<SessionModel> sessions,
        ScopeAndServiceResult<SessionManagerEntityModelRepositoryContainer> scopeAndService,
        long maximumSessions, CancellationToken cancellationToken)
    {
        if (sessions.Count > maximumSessions)
        {
            var expired = sessions.Count - maximumSessions;

            while (expired > 0)
            {
                var take = expired > int.MaxValue ? int.MaxValue : expired;
                var expiredSessions = sessions.Take((int)take);

                await scopeAndService.RequiredService.Sessions.DeleteManyExistingAsync(expiredSessions, saveChanges: false,
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                expired -= take;
            }
        }
    }

    protected override async Task<IReadOnlyCollection<ISession>> GetSessionsByTokenAsync(
        string? tenantId,
        string token,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentException.ThrowIfNullOrWhiteSpace(token, nameof(token));

        using var scopeAndService = _serviceProvider.CreateScopeAndGetRequiredService<SessionManagerEntityModelRepositoryContainer>();

        var query = scopeAndService.RequiredService.Sessions.Where(session =>
            session.Token == token);

        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            query = query.Where(session => session.TenantId == tenantId);
        }

        query = query.OrderByDescending(session => session.UpdatedTimestamp);

        var existingSessions = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

        return existingSessions.Select(session => Map(session)).ToList().AsReadOnly();
    }

    protected override async Task TouchSessionAsync(
        ISession session,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(session, nameof(session));

        var universalUniqueId = session.UniversalUniqueId;

        using var scopeAndService = _serviceProvider.CreateScopeAndGetRequiredService<SessionManagerEntityModelRepositoryContainer>();

        var existingSession = await scopeAndService.RequiredService.Sessions.FirstOrDefaultAsync(session =>
            session.UniversalUniqueId == universalUniqueId, cancellationToken).ConfigureAwait(false);

        if (existingSession == null)
        {
            return;
        }

        existingSession.UpdatedTimestamp = DateTimeOffset.UtcNow;

        _ = await scopeAndService.RequiredService.SaveChangesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public override async Task<ISession> NewSessionAsync(
        string tenantId,
        string accountId,
        string token,
        string refreshToken,
        ISessionManagementPolicies policies,
        IDeviceIdentification? deviceIdentification = null,
        TimeSpan? inactivityExpirationInterval = null,
        DateTimeOffset? expiresTimestamp = null,
        IReadOnlyDictionary<string, string>? attributes = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(tenantId, nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(accountId, nameof(accountId));
        ArgumentException.ThrowIfNullOrWhiteSpace(token, nameof(token));
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken, nameof(refreshToken));
        ArgumentNullException.ThrowIfNull(policies, nameof(policies));

        using var scopeAndService = _serviceProvider.CreateScopeAndGetRequiredService<SessionManagerEntityModelRepositoryContainer>();

        var hasToken = await scopeAndService.RequiredService.Sessions.AnyAsync(
            session => session.TenantId == tenantId && session.Token == token, cancellationToken).ConfigureAwait(false);

        if (hasToken)
        {
            throw new InvalidOperationException($"Duplicated token {token}");
        }

        var newSession = new SessionModel()
        {
            TenantId = tenantId,
            AccountId = accountId,
            Token = token,
            RefreshToken = refreshToken,
            DeviceIdentification = new(deviceIdentification),
            InactivityExpirationInterval = inactivityExpirationInterval,
            ExpiresTimestamp = expiresTimestamp,
        };

        newSession.GenerateModelAutomaticPropertiesForSave(DateTimeOffset.UtcNow);

        ValidatePolicies(newSession, newSession.DeviceIdentification, policies);

        if (attributes != null)
        {
            foreach (var attribute in attributes)
            {
                var mappedAttribute = new SessionAttributeModel()
                {
                    Session = newSession,
                    AttributeName = attribute.Key,
                    AttributeValue = attribute.Value
                };

                newSession.Attributes.Add(mappedAttribute);
            }
        }

        await scopeAndService.RequiredService.Sessions.AddNewAsync(newSession, cancellationToken: cancellationToken).ConfigureAwait(false);

        await ExceptionProtector.ExecuteTaskAsync(ExpireExceedingSessionsUsingPolicies(tenantId, accountId, newSession.DeviceIdentification.DeviceType,
            policies, cancellationToken)).ConfigureAwait(false);

        return Map(newSession);
    }

    public override async Task UpdateSessionTokenAsync(
        string tenantId,
        Guid universalUniqueId,
        string token,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(tenantId, nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(token, nameof(token));
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken, nameof(refreshToken));

        using var scopeAndService = _serviceProvider.CreateScopeAndGetRequiredService<SessionManagerEntityModelRepositoryContainer>();

        var existingSession = await scopeAndService.RequiredService.Sessions.FirstOrDefaultAsync(session =>
            session.TenantId == tenantId && session.UniversalUniqueId == universalUniqueId, cancellationToken).ConfigureAwait(false);

        if (existingSession == null)
        {
            throw new InvalidOperationException($"Invalid session {universalUniqueId}");
        }

        var hasToken = await scopeAndService.RequiredService.Sessions.AnyAsync(session =>
            session.TenantId == tenantId && session.Token == token && session.UniversalUniqueId != universalUniqueId, cancellationToken).ConfigureAwait(false);

        if (hasToken)
        {
            throw new InvalidOperationException($"Duplicated token {token}");
        }

        existingSession.Token = token;
        existingSession.RefreshToken = refreshToken;

        _ = await scopeAndService.RequiredService.SaveChangesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public override async Task ExpireSessionByTokensAsync(
        string tenantId,
        IEnumerable<string> tokens,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(tenantId, nameof(tenantId));
        ArgumentNullExceptionExtensionMethods.ThrowArgumentNullExceptionIfAny(tokens, nameof(tokens));

        using var scopeAndService = _serviceProvider.CreateScopeAndGetRequiredService<SessionManagerEntityModelRepositoryContainer>();

        var tokenSet = tokens.ToHashSet();

        var existingSessions = await scopeAndService.RequiredService.Sessions.Where(session =>
            session.TenantId == tenantId && tokenSet.Contains(session.Token)).ToListAsync(cancellationToken).ConfigureAwait(false);

        if (existingSessions.Count > 0)
        {
            await scopeAndService.RequiredService.Sessions.DeleteManyExistingAsync(existingSessions, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    public override async Task ExpireSessionByUniversalUniqueIdsAsync(
        string tenantId,
        IEnumerable<Guid> universalUniqueIds,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(tenantId, nameof(tenantId));

        using var scopeAndService = _serviceProvider.CreateScopeAndGetRequiredService<SessionManagerEntityModelRepositoryContainer>();

        var universalUniqueIdSet = universalUniqueIds.ToHashSet();

        var existingSessions = await scopeAndService.RequiredService.Sessions.Where(session =>
            session.TenantId == tenantId && universalUniqueIdSet.Contains(session.UniversalUniqueId)).ToListAsync(cancellationToken).ConfigureAwait(false);

        if (existingSessions.Count > 0)
        {
            await scopeAndService.RequiredService.Sessions.DeleteManyExistingAsync(existingSessions, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    public override async Task<ISession?> GetSessionByUniversalUniqueIdAsync(
        string tenantId,
        Guid universalUniqueId,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(tenantId, nameof(tenantId));

        using var scopeAndService = _serviceProvider.CreateScopeAndGetRequiredService<SessionManagerEntityModelRepositoryContainer>();

        var existingSession = await scopeAndService.RequiredService.Sessions.FirstOrDefaultAsync(session =>
            session.TenantId == tenantId && session.UniversalUniqueId == universalUniqueId, cancellationToken).ConfigureAwait(false);

        return existingSession == null ? null : Map(existingSession);
    }

    public override async Task<IReadOnlyCollection<ISession>> GetSessionsAsync(
        string tenantId,
        int? pageIndex = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(tenantId, nameof(tenantId));

        using var scopeAndService = _serviceProvider.CreateScopeAndGetRequiredService<SessionManagerEntityModelRepositoryContainer>();

        var existingSessions = await scopeAndService.RequiredService.Sessions.Where(session => session.TenantId == tenantId)
            .SkipTakeByPage(pageIndex, pageSize).ToListAsync(cancellationToken).ConfigureAwait(false);

        return existingSessions.Select(session => Map(session)).ToList().AsReadOnly();
    }

    public override async Task<IReadOnlyCollection<ISession>> GetSessionsByAccountAsync(
        string tenantId,
        string accountId,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(tenantId, nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(accountId, nameof(accountId));

        using var scopeAndService = _serviceProvider.CreateScopeAndGetRequiredService<SessionManagerEntityModelRepositoryContainer>();

        var existingSessions = await scopeAndService.RequiredService.Sessions.Where(session =>
            session.TenantId == tenantId && session.AccountId == accountId).ToListAsync(cancellationToken).ConfigureAwait(false);

        return existingSessions.Select(session => Map(session)).ToList().AsReadOnly();
    }
}
