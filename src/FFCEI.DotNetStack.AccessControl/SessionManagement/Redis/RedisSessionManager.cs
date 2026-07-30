using FFCEI.DotNetStack.AccessControl.Interfaces;
using FFCEI.DotNetStack.AccessControl.SessionManagement.Common;
using FFCEI.DotNetStack.Caching;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Globalization;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.Redis;

public sealed class RedisSessionManager(string connectionString, ILogger logger)
    : SessionManager(logger)
{
#pragma warning disable CA2213 // Disposable fields should be disposed
    private readonly RedisResilientConnectionFactory _connectionFactory = new(connectionString);
#pragma warning restore CA2213 // Disposable fields should be disposed

    private static Session Map(RedisValue[] results)
    {
        // TODO implementar...

        var result = new Session()
        {
            TenantId = results[3].ToString(),
            AccountId = results[4].ToString(),
            ExternalUniqueId = ValueConversions.GuidFromString(results[5].ToString()),
            Token = results[6].ToString(),
            RefreshToken = results[7].ToString(),
            DeviceIdentification = new DeviceIdentification()
            {
                UserAgent = results[8].ToString(),
                Application = results[9].ToString(),
                OperatingSystem = results[10].ToString(),
                /*
                MultiTouchPoints = results[11].ToString(),
                IdentificationType = results[12].ToString(),
                Mobile = results[13].ToString(),
                */
            }
            /*
            ExpiresTimestamp = results[14].ToString(),
            InactivityExpirationInterval = results[15].ToString(),
            FinalizedTimestamp = results[16].ToString(),
            CreatedTimestamp = results[17].ToString(),
            UpdatedTimestamp = results[18].ToString(),
            */
            /*
            Attributes = attributes.ToDictionary(attribute => attribute.AttributeName, item => item.AttributeValue)
            */
        };

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

        var redisDatabase = await _connectionFactory.GetDatabaseAsync().ConfigureAwait(false);

        // TODO

        var luaScript = LuaScript.Prepare(@"

            local IndexSessionTenant = 'idx:tenant:'..@TenantId..':updated'
            local ExternalUniqueIds = redis.call('ZRANGE', IndexSessionTenant, @StartIndex, @EndIndex)

            return ExternalUniqueIds

            local IndexTokenGlobal = 'idx:token:'..SessionToken
            redis.call('ZADD', IndexTokenGlobal, @NowAsUnixMilliseconds, @ExternalUniqueId)

            local IndexSessionGlobal = 'idx:sessions:updated'
            redis.call('ZADD', IndexSessionGlobal, @NowAsUnixMilliseconds, @ExternalUniqueId)

            local IndexSessionTenant = 'idx:tenant:'..@TenantId..':updated'
            redis.call('ZADD', IndexSessionTenant, @NowAsUnixMilliseconds, @ExternalUniqueId)

            local IndexSessionTenantAccount = 'idx:tenant:'..@TenantId..':account:'..SessionAccountId..':updated'
            redis.call('ZADD', IndexSessionTenantAccount, @NowAsUnixMilliseconds, @ExternalUniqueId)

            local IndexSessionTenantAccountDevice = 'idx:tenant:'..@TenantId..':account:'..SessionAccountId..':device:'..SessionDeviceIdentificationDeviceType..':updated'
            redis.call('ZADD', IndexSessionTenantAccountDevice, @NowAsUnixMilliseconds, @ExternalUniqueId)

        ");

        var luaScriptParameters = new LuaScriptParameters(tenantId, accountId, deviceIdentificationType, policies);

        var luaScriptResult = await redisDatabase.ScriptEvaluateAsync(luaScript, luaScriptParameters).ConfigureAwait(false);

        var externalUniqueIdStrings = (RedisValue[])luaScriptResult!;

        if (externalUniqueIdStrings.Length == 0)
        {
            return;
        }

        var externalUniqueIds = new List<Guid>();

        foreach (var externalUniqueIdString in externalUniqueIdStrings ?? [])
        {
            if (externalUniqueIdString.IsNullOrEmpty)
            {
                continue;
            }

            var externalUniqueId = ValueConversions.GuidFromString(externalUniqueIdString.ToString());

            externalUniqueIds.Add(externalUniqueId);
        }

        /*
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
        */
    }

    protected override async Task<IReadOnlyCollection<ISession>> GetSessionsByTokenAsync(
        string? tenantId,
        string token,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentException.ThrowIfNullOrWhiteSpace(token, nameof(token));

        var redisDatabase = await _connectionFactory.GetDatabaseAsync().ConfigureAwait(false);

        var luaScript = LuaScript.Prepare(@"
            local unpack = table.unpack or unpack

            local TenantIdLocal = @TenantId
            local Tenants = {}

            if (not TenantIdLocal or TenantIdLocal == false)
                local IndexTokenGlobal = 'idx:token:'..@Token

                Tenants = redis.call('ZRANGE', IndexTokenGlobal, 0, -1, 'REV')

                if Tenants and #Tenants > 0 then
                    TenantIdLocal = Tenants[1]
                end
            end

            if (not TenantIdLocal or TenantIdLocal == false) then
                return { 'CALL_STATUS', 'NOK_INVALID_TOKEN' }
            end

            if Tenants and #Tenants > 1 then
                local MultipleResponse = { 'CALL_STATUS', 'OK', 'COUNT', tostring(#Tenants) }

                for i = 1, #Tenants do
                    local TenantValue = Tenants[i]
        
                    table.insert(MultipleResponse, 'TENANT_' .. i)
                    table.insert(MultipleResponse, tostring(TenantValue))
                end

                return MultipleResponse
            end

            local SessionTokenKey = 'token:'..TenantIdLocal..':'..@Token
            local SessionExternalUniqueId = redis.call('GET', SessionTokenKey)

            if (not SessionExternalUniqueId or SessionExternalUniqueId == false) then
                return { 'CALL_STATUS', 'NOK_INVALID_TOKEN' }
            end

            local SessionKey = 'session:'..TenantIdLocal..':'..SessionExternalUniqueId
            local SessionExists = redis.call('HMGET', SessionKey, 'CreatedTimestamp', 'FinalizedTimestamp')
            local SessionCreatedTimestamp = SessionExists[1]
            local SessionFinalizedTimestamp = SessionExists[2]

            if (not SessionCreatedTimestamp or SessionCreatedTimestamp == false) then
                redis.call('DEL', SessionTokenKey)

                return { 'CALL_STATUS', 'NOK_INVALID' }
            end

            if SessionFinalizedTimestamp then
                redis.call('DEL', SessionTokenKey)

                return { 'CALL_STATUS', 'NOK_EXPIRED' }
            end

            local SessionData = redis.call('HMGET', SessionKey)
            local FinalResponse = { 'CALL_STATUS', 'OK', 'COUNT', '1' }

            for i = 1, #SessionData do
                table.insert(FinalResponse, SessionData[i])
            end

            return FinalResponse
        ");

        var luaScriptParameters = new LuaScriptParameters(tenantId, token);
        var luaScriptResult = await redisDatabase.ScriptEvaluateAsync(luaScript, luaScriptParameters).ConfigureAwait(false);

        var results = (RedisValue[])luaScriptResult!;
        var resultStatus = results[0].ToString();

        if (resultStatus != "OK")
        {
            if ((resultStatus == "NOK_INVALID_TOKEN") ||
                (resultStatus == "NOK_INVALID") ||
                (resultStatus == "NOK_EXPIRED"))
            {
                return [];
            }

            throw new InvalidOperationException($"Unexpected error");
        }

        var resultCount = int.Parse(results[1].ToString(), CultureInfo.InvariantCulture);

        if (resultCount == 1)
        {
            var session = Map(results);

            return [session];
        }

        var tenantIds = results.Skip(2);
        var sessions = new List<ISession>();

        foreach (var tenantIdString in tenantIds ?? [])
        {
            if (tenantIdString.IsNullOrEmpty)
            {
                continue;
            }

            tenantId = tenantIdString.ToString();

            var tenantResults = await GetSessionsByTokenAsync(tenantId, token, cancellationToken).ConfigureAwait(false);
            var session = tenantResults.FirstOrDefault();

            if (session != null)
            {
                sessions.Add(session);
            }
        }

        return sessions.AsReadOnly();
    }

    protected override async Task TouchSessionAsync(
        ISession session,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(session, nameof(session));

        var redisDatabase = await _connectionFactory.GetDatabaseAsync().ConfigureAwait(false);

        var luaScript = LuaScript.Prepare(@"
            local SessionKey = 'session:'..@TenantId..':'..@ExternalUniqueId
            local SessionExists = redis.call('HMGET', SessionKey, 'CreatedTimestamp', 'FinalizedTimestamp', 'AccountId', 'DeviceIdentification:DeviceType', 'Token')
            local SessionCreatedTimestamp = SessionExists[1]
            local SessionFinalizedTimestamp = SessionExists[2]
            local SessionAccountId = SessionExists[3]
            local SessionDeviceIdentificationDeviceType = SessionExists[4]
            local SessionToken = SessionExists[5]

            if (not SessionCreatedTimestamp or SessionCreatedTimestamp == false) then
                return 'NOK_INVALID'
            end

            if SessionFinalizedTimestamp then
                return 'OK'
            end

            redis.call('HSET', SessionKey,
                'UpdatedTimestamp', @Now)

            local IndexTokenGlobal = 'idx:token:'..SessionToken
            redis.call('ZADD', IndexTokenGlobal, @NowAsUnixMilliseconds, @ExternalUniqueId)

            local IndexSessionGlobal = 'idx:sessions:updated'
            redis.call('ZADD', IndexSessionGlobal, @NowAsUnixMilliseconds, @ExternalUniqueId)

            local IndexSessionTenant = 'idx:tenant:'..@TenantId..':updated'
            redis.call('ZADD', IndexSessionTenant, @NowAsUnixMilliseconds, @ExternalUniqueId)

            local IndexSessionTenantAccount = 'idx:tenant:'..@TenantId..':account:'..SessionAccountId..':updated'
            redis.call('ZADD', IndexSessionTenantAccount, @NowAsUnixMilliseconds, @ExternalUniqueId)

            local IndexSessionTenantAccountDevice = 'idx:tenant:'..@TenantId..':account:'..SessionAccountId..':device:'..SessionDeviceIdentificationDeviceType..':updated'
            redis.call('ZADD', IndexSessionTenantAccountDevice, @NowAsUnixMilliseconds, @ExternalUniqueId)

            return 'OK'
        ");

        session.TouchModelTimestamps();

        var luaScriptParameters = new LuaScriptParameters(session);
        var luaScriptResult = await redisDatabase.ScriptEvaluateAsync(luaScript, luaScriptParameters).ConfigureAwait(false);

        var resultStatus = (string)luaScriptResult!;

        if (resultStatus != "OK")
        {
            if (resultStatus == "NOK_INVALID")
            {
                return;
            }

            throw new InvalidOperationException($"Unexpected error");
        }
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

        var newSession = new Session()
        {
            TenantId = tenantId,
            AccountId = accountId,
            Token = token,
            RefreshToken = refreshToken,
            DeviceIdentification = new(deviceIdentification),
            InactivityExpirationInterval = inactivityExpirationInterval,
            ExpiresTimestamp = expiresTimestamp,
            Attributes = ((attributes != null) && (attributes.Count > 0)) ? attributes.ToDictionary() : []
        };

        newSession.GenerateModelAutomaticPropertiesForSave(DateTimeOffset.UtcNow);

        ValidatePolicies(newSession, newSession.DeviceIdentification, policies);

        var redisDatabase = await _connectionFactory.GetDatabaseAsync().ConfigureAwait(false);

        var luaScript = LuaScript.Prepare(@"
            local unpack = table.unpack or unpack

            local SessionKey = 'session:'..@TenantId..':'..@ExternalUniqueId
            local SessionExists = redis.call('HMGET', SessionKey, 'CreatedTimestamp', 'FinalizedTimestamp')
            local SessionCreatedTimestamp = SessionExists[1]
            local SessionFinalizedTimestamp = SessionExists[2]

            if SessionCreatedTimestamp and (not SessionFinalizedTimestamp or SessionFinalizedTimestamp == false) then
                return 'NOK_DUPLICATED_EXTERNALUNIQUEID'
            end

            local TokenKey = 'token:'..@TenantId..':'..@Token
            local TokenExists = redis.call('EXISTS', TokenKey)

            if TokenExists > 0 then
                return 'NOK_DUPLICATED_TOKEN'
            end

            local hsetArgs =
            {
                'TenantId', @TenantId,
                'ExternalUniqueId', @ExternalUniqueId,
                'Token', @Token,
                'CreatedTimestamp', @Now,
                'UpdatedTimestamp', @Now
            }

            local optionalFields =
            {
                { 'AccountId', @AccountId },
                { 'DeviceIdentification:UserAgent', @DeviceIdentificationUserAgent },
                { 'DeviceIdentification:Application', @DeviceIdentificationApplication },
                { 'DeviceIdentification:OperatingSystem', @DeviceIdentificationOperatingSystem },
                { 'DeviceIdentification:MultiTouchPoints', @DeviceIdentificationMultiTouchPoints },
                { 'DeviceIdentification:DeviceType', @DeviceIdentificationDeviceType },
                { 'DeviceIdentification:Mobile', @DeviceIdentificationMobile },
                { 'ExpiresTimestamp', @ExpiresTimestamp },
                { 'InactivityExpirationInterval', @InactivityExpirationInterval },
                { 'FinalizedTimestamp', @FinalizedTimestamp }
            }

            for _, field in ipairs(optionalFields) do
                local fieldName = field[1]
                local fieldValue = field[2]

                if fieldValue and fieldValue ~= false and fieldValue ~= '' then
                    table.insert(hsetArgs, fieldName)
                    table.insert(hsetArgs, tostring(fieldValue))
                end
            end

            if @Attributes and @Attributes ~= false and @Attributes ~= '' then
                local AttributesTable = cjson.decode(@Attributes)

                for attributeName, attributeValue in pairs(AttributesTable) do
                    if attributeValue ~= nil and attributeValue ~= false and attributeValue ~= '' then
                        table.insert(hsetArgs, 'Attributes:' .. attributeName)
                        table.insert(hsetArgs, tostring(attributeValue))
                    end
                end
            end

            redis.call('HSET', SessionKey, unpack(hsetArgs))

            redis.call('SET', TokenKey, @ExternalUniqueId)

            local IndexTokenGlobal = 'idx:token:'..@Token
            redis.call('ZADD', IndexTokenGlobal, @NowAsUnixMilliseconds, @ExternalUniqueId)

            local IndexSessionGlobal = 'idx:sessions:updated'
            redis.call('ZADD', IndexSessionGlobal, @NowAsUnixMilliseconds, @ExternalUniqueId)

            local IndexSessionTenant = 'idx:tenant:'..@TenantId..':updated'
            redis.call('ZADD', IndexSessionTenant, @NowAsUnixMilliseconds, @ExternalUniqueId)

            local IndexSessionTenantAccount = 'idx:tenant:'..@TenantId..':account:'..@AccountId..':updated'
            redis.call('ZADD', IndexSessionTenantAccount, @NowAsUnixMilliseconds, @ExternalUniqueId)

            local IndexSessionTenantAccountDevice = 'idx:tenant:'..@TenantId..':account:'..@AccountId..':device:'..@DeviceIdentificationDeviceType..':updated'
            redis.call('ZADD', IndexSessionTenantAccountDevice, @NowAsUnixMilliseconds, @ExternalUniqueId)

            return 'OK'
        ");

        var luaScriptParameters = new LuaScriptParameters(newSession);
        var luaScriptResult = await redisDatabase.ScriptEvaluateAsync(luaScript, luaScriptParameters).ConfigureAwait(false);

        var resultStatus = (string)luaScriptResult!;

        if (resultStatus != "OK")
        {
            throw resultStatus switch
            {
                "NOK_DUPLICATED_EXTERNALUNIQUEID" => new InvalidOperationException($"Duplicated external unique id {luaScriptParameters.ExternalUniqueId}"),
                "NOK_DUPLICATED_TOKEN" => new InvalidOperationException($"Duplicated token {luaScriptParameters.Token}"),
                _ => new InvalidOperationException($"Unexpected error")
            };
        }

        await ExceptionProtector.ExecuteTaskAsync(ExpireExceedingSessionsUsingPolicies(tenantId, accountId, newSession.DeviceIdentification.DeviceType,
            policies, cancellationToken)).ConfigureAwait(false);

        return newSession;
    }

    public override async Task UpdateSessionTokenAsync(
        string tenantId,
        Guid externalUniqueId,
        string token,
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(tenantId, nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(token, nameof(token));
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken, nameof(refreshToken));

        var redisDatabase = await _connectionFactory.GetDatabaseAsync().ConfigureAwait(false);

        var luaScript = LuaScript.Prepare(@"
            local SessionKey = 'session:'..@TenantId..':'..@ExternalUniqueId
            local SessionExists = redis.call('HMGET', SessionKey, 'CreatedTimestamp', 'FinalizedTimestamp', 'AccountId', 'DeviceIdentification:DeviceType', 'Token')
            local SessionCreatedTimestamp = SessionExists[1]
            local SessionFinalizedTimestamp = SessionExists[2]
            local SessionAccountId = SessionExists[3]
            local SessionDeviceIdentificationDeviceType = SessionExists[4]
            local SessionToken = SessionExists[5]

            if (not SessionCreatedTimestamp or SessionCreatedTimestamp == false) then
                return 'NOK_INVALID'
            end

            if SessionFinalizedTimestamp then
                return 'NOK_EXPIRED'
            end

            local TokenKey = 'token:'..@TenantId..':'..@Token
            local TokenKeyExists = redis.call('EXISTS', TokenKey)

            if TokenKeyExists > 0 then
                return 'NOK_DUPLICATED_TOKEN'
            end

            local SessionTokenKey = 'token:'..@TenantId..':'..SessionToken

            redis.call('DEL', SessionTokenKey)

            redis.call('HSET', SessionKey,
                'Token', @Token,
                'UpdatedTimestamp', @Now)

            local IndexSessionTokenGlobal = 'idx:token:'..SessionToken
            redis.call('ZREM', IndexSessionTokenGlobal, @ExternalUniqueId)

            local IndexTokenGlobal = 'idx:token:'..@Token
            redis.call('ZADD', IndexTokenGlobal, @NowAsUnixMilliseconds, @ExternalUniqueId)

            local IndexSessionGlobal = 'idx:sessions:updated'
            redis.call('ZADD', IndexSessionGlobal, @NowAsUnixMilliseconds, @ExternalUniqueId)

            local IndexSessionTenant = 'idx:tenant:'..@TenantId..':updated'
            redis.call('ZADD', IndexSessionTenant, @NowAsUnixMilliseconds, @ExternalUniqueId)

            local IndexSessionTenantAccount = 'idx:tenant:'..@TenantId..':account:'..SessionAccountId..':updated'
            redis.call('ZADD', IndexSessionTenantAccount, @NowAsUnixMilliseconds, @ExternalUniqueId)

            local IndexSessionTenantAccountDevice = 'idx:tenant:'..@TenantId..':account:'..SessionAccountId..':device:'..SessionDeviceIdentificationDeviceType..':updated'
            redis.call('ZADD', IndexSessionTenantAccountDevice, @NowAsUnixMilliseconds, @ExternalUniqueId)

            return 'OK'
        ");

        var luaScriptParameters = new LuaScriptParameters(tenantId, externalUniqueId, token, refreshToken);
        var luaScriptResult = await redisDatabase.ScriptEvaluateAsync(luaScript, luaScriptParameters).ConfigureAwait(false);

        var resultStatus = (string)luaScriptResult!;

        if (resultStatus != "OK")
        {
            throw resultStatus switch
            {
                "NOK_INVALID" => new InvalidOperationException($"Invalid session {luaScriptParameters.ExternalUniqueId}"),
                "NOK_EXPIRED" => new InvalidOperationException($"Expired session {luaScriptParameters.ExternalUniqueId}"),
                "NOK_DUPLICATED_TOKEN" => new InvalidOperationException($"Duplicated token {luaScriptParameters.Token}"),
                _ => new InvalidOperationException($"Unexpected error")
            };
        }
    }

    public override async Task ExpireSessionByTokensAsync(
        string tenantId,
        IEnumerable<string> tokens,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(tenantId, nameof(tenantId));
        ArgumentNullExceptionExtensionMethods.ThrowArgumentNullExceptionIfAny(tokens, nameof(tokens));

        var redisDatabase = await _connectionFactory.GetDatabaseAsync().ConfigureAwait(false);

        var luaScript = LuaScript.Prepare(@"
            local SessionTokenKey = 'token:'..@TenantId..':'..@Token
            local SessionExternalUniqueId = redis.call('GET', SessionTokenKey)

            if (not SessionExternalUniqueId or SessionExternalUniqueId == false) then
                return 'NOK_INVALID_TOKEN'
            end

            local SessionKey = 'session:'..@TenantId..':'..SessionExternalUniqueId
            local SessionExists = redis.call('HMGET', SessionKey, 'CreatedTimestamp', 'FinalizedTimestamp', 'AccountId', 'DeviceIdentification:DeviceType')
            local SessionCreatedTimestamp = SessionExists[1]
            local SessionFinalizedTimestamp = SessionExists[2]
            local SessionAccountId = SessionExists[3]
            local SessionDeviceIdentificationDeviceType = SessionExists[4]
            
            if (not SessionCreatedTimestamp or SessionCreatedTimestamp == false) then
                redis.call('DEL', SessionTokenKey)

                return 'NOK_INVALID'
            end

            if SessionFinalizedTimestamp then
                redis.call('DEL', SessionTokenKey)

                return 'OK'
            end

            redis.call('HSET', SessionKey,
                'UpdatedTimestamp', @Now,
                'FinalizedTimestamp', @Now)

            redis.call('EXPIRE', SessionKey, 86400)

            local IndexSessionTokenGlobal = 'idx:token:'..@Token
            redis.call('ZREM', IndexSessionTokenGlobal, @ExternalUniqueId)

            local IndexSessionGlobal = 'idx:sessions:updated'
            redis.call('ZREM', IndexSessionGlobal, SessionExternalUniqueId)

            local IndexSessionTenant = 'idx:tenant:'..@TenantId..':updated'
            redis.call('ZREM', IndexSessionTenant, SessionExternalUniqueId)

            local IndexSessionTenantAccount = 'idx:tenant:'..@TenantId..':account:'..SessionAccountId..':updated'
            redis.call('ZREM', IndexSessionTenantAccount, SessionExternalUniqueId)

            local IndexSessionTenantAccountDevice = 'idx:tenant:'..@TenantId..':account:'..SessionAccountId..':device:'..SessionDeviceIdentificationDeviceType..':updated'
            redis.call('ZREM', IndexSessionTenantAccountDevice, SessionExternalUniqueId)

            return 'OK'
        ");

        foreach (var token in tokens)
        {
            var luaScriptParameters = new LuaScriptParameters(tenantId, token);
            var luaScriptResult = await redisDatabase.ScriptEvaluateAsync(luaScript, luaScriptParameters).ConfigureAwait(false);

            var resultStatus = (string)luaScriptResult!;

            if (resultStatus != "OK")
            {
                if ((resultStatus == "NOK_INVALID_TOKEN") ||
                    (resultStatus == "NOK_INVALID"))
                {
                    continue;
                }

                throw new InvalidOperationException($"Unexpected error");
            }
        }
    }

    public override async Task ExpireSessionByExternalUniqueIdsAsync(
        string tenantId,
        IEnumerable<Guid> externalUniqueIds,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(tenantId, nameof(tenantId));

        var redisDatabase = await _connectionFactory.GetDatabaseAsync().ConfigureAwait(false);

        var luaScript = LuaScript.Prepare(@"
            local SessionKey = 'session:'..@TenantId..':'..@ExternalUniqueId
            local SessionExists = redis.call('HMGET', SessionKey, 'CreatedTimestamp', 'FinalizedTimestamp', 'AccountId', 'DeviceIdentification:DeviceType', 'Token')
            local SessionCreatedTimestamp = SessionExists[1]
            local SessionFinalizedTimestamp = SessionExists[2]
            local SessionAccountId = SessionExists[3]
            local SessionDeviceIdentificationDeviceType = SessionExists[4]
            local SessionToken = SessionExists[5]

            if (not SessionCreatedTimestamp or SessionCreatedTimestamp == false) then
                return 'NOK_INVALID'
            end

            if SessionFinalizedTimestamp then
                return 'OK'
            end

            local SessionTokenKey = 'token:'..@TenantId..':'..SessionToken

            redis.call('DEL', SessionTokenKey)

            redis.call('HSET', SessionKey,
                'UpdatedTimestamp', @Now,
                'FinalizedTimestamp', @Now)

            redis.call('EXPIRE', SessionKey, 86400)

            local IndexSessionTokenGlobal = 'idx:token:'..SessionToken
            redis.call('ZREM', IndexSessionTokenGlobal, @ExternalUniqueId)

            local IndexSessionGlobal = 'idx:sessions:updated'
            redis.call('ZREM', IndexSessionGlobal, @ExternalUniqueId)

            local IndexSessionTenant = 'idx:tenant:'..@TenantId..':updated'
            redis.call('ZREM', IndexSessionTenant, @ExternalUniqueId)

            local IndexSessionTenantAccount = 'idx:tenant:'..@TenantId..':account:'..SessionAccountId..':updated'
            redis.call('ZREM', IndexSessionTenantAccount, @ExternalUniqueId)

            local IndexSessionTenantAccountDevice = 'idx:tenant:'..@TenantId..':account:'..SessionAccountId..':device:'..SessionDeviceIdentificationDeviceType..':updated'
            redis.call('ZREM', IndexSessionTenantAccountDevice, @ExternalUniqueId)

            return 'OK'
        ");

        foreach (var externalUniqueId in externalUniqueIds)
        {
            var luaScriptParameters = new LuaScriptParameters(tenantId, externalUniqueId);
            var luaScriptResult = await redisDatabase.ScriptEvaluateAsync(luaScript, luaScriptParameters).ConfigureAwait(false);

            var resultStatus = (string)luaScriptResult!;

            if (resultStatus != "OK")
            {
                if (resultStatus == "NOK_INVALID")
                {
                    continue;
                }

                throw new InvalidOperationException($"Unexpected error");
            }
        }
    }

    public override async Task<ISession?> GetSessionByExternalUniqueIdAsync(
        string tenantId,
        Guid externalUniqueId,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(tenantId, nameof(tenantId));

        var redisDatabase = await _connectionFactory.GetDatabaseAsync().ConfigureAwait(false);

        var luaScript = LuaScript.Prepare(@"
            local SessionKey = 'session:'..@TenantId..':'..@ExternalUniqueId
            local SessionExists = redis.call('HMGET', SessionKey, 'CreatedTimestamp', 'FinalizedTimestamp')
            local SessionCreatedTimestamp = SessionExists[1]
            local SessionFinalizedTimestamp = SessionExists[2]
            
            if (not SessionCreatedTimestamp or SessionCreatedTimestamp == false) then
                return { 'CALL_STATUS', 'NOK_INVALID' }
            end

            if SessionFinalizedTimestamp then
                return { 'CALL_STATUS', 'NOK_EXPIRED' }
            end

            local SessionData = redis.call('HGETALL', SessionKey)
            local FinalResponse = { 'CALL_STATUS', 'OK', 'COUNT', '1' }

            for i = 1, #SessionData do
                table.insert(FinalResponse, SessionData[i])
            end

            return FinalResponse
        ");

        var luaScriptParameters = new LuaScriptParameters(tenantId, externalUniqueId);
        var luaScriptResult = await redisDatabase.ScriptEvaluateAsync(luaScript, luaScriptParameters).ConfigureAwait(false);

        var results = (RedisValue[])luaScriptResult!;
        var resultStatus = results[0].ToString();

        if (resultStatus != "OK")
        {
            if ((resultStatus == "NOK_INVALID") ||
                (resultStatus == "NOK_EXPIRED"))
            {
                return null;
            }

            throw new InvalidOperationException($"Unexpected error");
        }

        var session = Map(results);

        return session;
    }

    public override async Task<IReadOnlyCollection<ISession>> GetSessionsAsync(
        string tenantId,
        int? pageIndex = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(tenantId, nameof(tenantId));

        var redisDatabase = await _connectionFactory.GetDatabaseAsync().ConfigureAwait(false);

        var luaScript = LuaScript.Prepare(@"
            local IndexSessionTenant = 'idx:tenant:'..@TenantId..':updated'
            local ExternalUniqueIds = redis.call('ZRANGE', IndexSessionTenant, @StartIndex, @EndIndex)

            return ExternalUniqueIds
        ");

        var luaScriptParameters = PagingAlgorithms.ComputeStartIndexEndIndex(pageIndex, pageSize, out var startIndex, out var endIndex, forcePaging: true) && startIndex.HasValue && endIndex.HasValue ?
            new LuaScriptParameters(tenantId, startIndex.Value, endIndex.Value) : new LuaScriptParameters(tenantId);

        var luaScriptResult = await redisDatabase.ScriptEvaluateAsync(luaScript, luaScriptParameters).ConfigureAwait(false);

        var externalUniqueIdStrings = (RedisValue[])luaScriptResult!;

        if (externalUniqueIdStrings.Length == 0)
        {
            return [];
        }

        var sessions = new List<ISession>();

        foreach (var externalUniqueIdString in externalUniqueIdStrings ?? [])
        {
            if (externalUniqueIdString.IsNullOrEmpty)
            {
                continue;
            }

            var externalUniqueId = ValueConversions.GuidFromString(externalUniqueIdString.ToString());
            var session = await GetSessionByExternalUniqueIdAsync(tenantId, externalUniqueId, cancellationToken).ConfigureAwait(false);

            if (session != null)
            {
                sessions.Add(session);
            }
        }

        return sessions.AsReadOnly();
    }

    public override async Task<IReadOnlyCollection<ISession>> GetSessionsByAccountAsync(
        string tenantId,
        string accountId,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        ArgumentNullException.ThrowIfNull(tenantId, nameof(tenantId));
        ArgumentException.ThrowIfNullOrWhiteSpace(accountId, nameof(accountId));

        var redisDatabase = await _connectionFactory.GetDatabaseAsync().ConfigureAwait(false);

        var luaScript = LuaScript.Prepare(@"
            local IndexSessionTenantAccount = 'idx:tenant:'..@TenantId..':account:'..@AccountId..':updated'
            local ExternalUniqueIds = redis.call('ZRANGE', IndexSessionTenantAccount, 0, -1)

            return ExternalUniqueIds
        ");

        var luaScriptParameters = new LuaScriptParameters(tenantId, accountId, isAccount: true);
        var luaScriptResult = await redisDatabase.ScriptEvaluateAsync(luaScript, luaScriptParameters).ConfigureAwait(false);

        var externalUniqueIdStringss = (RedisValue[])luaScriptResult!;

        if (externalUniqueIdStringss.Length == 0)
        {
            return [];
        }

        var sessions = new List<ISession>();

        foreach (var externalUniqueIdString in externalUniqueIdStringss ?? [])
        {
            if (externalUniqueIdString.IsNullOrEmpty)
            {
                continue;
            }

            var externalUniqueId = ValueConversions.GuidFromString(externalUniqueIdString.ToString());
            var session = await GetSessionByExternalUniqueIdAsync(tenantId, externalUniqueId, cancellationToken).ConfigureAwait(false);

            if (session != null)
            {
                sessions.Add(session);
            }
        }

        return sessions.AsReadOnly();
    }

    protected override void OnDispose()
        => _connectionFactory.DisposeSafelyIfNeeded();

    protected override ValueTask OnDisposeAsync()
        => _connectionFactory.DisposeSafelyIfNeededAsync();
}
