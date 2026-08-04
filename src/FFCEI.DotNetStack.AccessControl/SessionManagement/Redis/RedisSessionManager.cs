using FFCEI.DotNetStack.AccessControl.Interfaces;
using FFCEI.DotNetStack.AccessControl.SessionManagement.Common;
using FFCEI.DotNetStack.Caching;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.Redis;

public sealed class RedisSessionManager(string connectionString, ILogger logger)
    : SessionManager(logger)
{
#pragma warning disable CA2213 // Disposable fields should be disposed
    private readonly RedisResilientConnectionFactory _connectionFactory = new(connectionString);
#pragma warning restore CA2213 // Disposable fields should be disposed

    private const string AttributeFieldPrefix = "Attributes:";

    private static string GetRequiredStringField(Dictionary<string, RedisResult> sessionFields, string fieldName)
        => sessionFields.TryGetValue(fieldName, out var value) ? value.ToString() : throw new InvalidOperationException($"Missing required session field '{fieldName}'");

    private static long GetRequiredIntField(Dictionary<string, RedisResult> sessionFields, string fieldName)
        => sessionFields.TryGetValue(fieldName, out var value) ? (int)value : throw new InvalidOperationException($"Missing required session field '{fieldName}'");

    private static long GetRequiredLongField(Dictionary<string, RedisResult> sessionFields, string fieldName)
        => sessionFields.TryGetValue(fieldName, out var value) ? (long)value : throw new InvalidOperationException($"Missing required session field '{fieldName}'");

    private static string? GetOptionalStringField(Dictionary<string, RedisResult> sessionFields, string fieldName)
        => sessionFields.TryGetValue(fieldName, out var value) ? value.ToString() : null;

    private static int? GetOptionalIntField(Dictionary<string, RedisResult> sessionFields, string fieldName)
        => sessionFields.TryGetValue(fieldName, out var value) ? (int?)value : null;

    private static long? GetOptionalLongField(Dictionary<string, RedisResult> sessionFields, string fieldName)
        => sessionFields.TryGetValue(fieldName, out var value) ? (long?)value : null;

    private static Dictionary<string, string> ExtractAttributes(Dictionary<string, RedisResult> sessionFields)
    {
        var attributes = new Dictionary<string, string>();

        foreach (var sessionField in sessionFields)
        {
            if (sessionField.Key.StartsWith(AttributeFieldPrefix, StringComparison.Ordinal))
            {
                attributes[sessionField.Key[AttributeFieldPrefix.Length..]] = sessionField.Value.ToString();
            }
        }

        return attributes;
    }

    private static Session Map(Dictionary<string, RedisResult> sessionFields)
    {
        var deviceType = Enum.TryParse<DeviceIdentificationType>(GetOptionalStringField(sessionFields, "DeviceIdentification:DeviceType"), out var parsedDeviceType)
            ? parsedDeviceType
            : DeviceIdentificationType.Unknown;

        var result = new Session()
        {
            TenantId = GetRequiredStringField(sessionFields, "TenantId"),
            AccountId = GetOptionalStringField(sessionFields, "AccountId") ?? string.Empty,
            ExternalUniqueId = ValueConversions.GuidFromString(GetRequiredStringField(sessionFields, "ExternalUniqueId")),
            Token = GetRequiredStringField(sessionFields, "Token"),
            RefreshToken = GetRequiredStringField(sessionFields, "RefreshToken"),
            DeviceIdentification = new DeviceIdentification()
            {
                UserAgent = GetOptionalStringField(sessionFields, "DeviceIdentification:UserAgent"),
                Application = GetOptionalStringField(sessionFields, "DeviceIdentification:Application"),
                OperatingSystem = GetOptionalStringField(sessionFields, "DeviceIdentification:OperatingSystem"),
                MultiTouchPoints = GetOptionalIntField(sessionFields, "DeviceIdentification:MultiTouchPoints"),
                DeviceType = deviceType,
                Mobile = GetOptionalIntField(sessionFields, "DeviceIdentification:Mobile") is int mobileValue && (mobileValue != 0)
            },
            ExpiresTimestamp = ValueConversions.TryDateTimeOffsetUtcFromUnixMilliseconds(GetOptionalLongField(sessionFields, "ExpiresTimestamp")),
            InactivityExpirationInterval = ValueConversions.TryTimeSpanFromMilliseconds(GetOptionalLongField(sessionFields, "InactivityExpirationInterval")),
            FinalizedTimestamp = ValueConversions.TryDateTimeOffsetUtcFromUnixMilliseconds(GetOptionalLongField(sessionFields, "FinalizedTimestamp")),
            CreatedTimestamp = ValueConversions.DateTimeOffsetUtcFromSerialWithMilliseconds(GetRequiredLongField(sessionFields, "CreatedTimestamp")),
            UpdatedTimestamp = ValueConversions.DateTimeOffsetUtcFromSerialWithMilliseconds(GetRequiredLongField(sessionFields, "UpdatedTimestamp")),
            Attributes = ExtractAttributes(sessionFields)
        };

        return result;
    }

    private static async Task CollectOverflowExternalUniqueIdsAsync(IDatabase redisDatabase, string indexKey, long maxSessions,
        Dictionary<Guid, string> sessions)
    {
        var luaScript = LuaScript.Prepare(@"
            local ExternalUniqueIds = redis.call('ZRANGE', @IndexKey, 0, -1)
            local Overflow = #ExternalUniqueIds - @MaxCount

            if Overflow <= 0 then
                return {}
            end

            local Result = {}

            for i = 1, Overflow do
                local ExternalUniqueId = ExternalUniqueIds[i]
                local TenantId = redis.call('HGET', 'idx:sessions:tenants', ExternalUniqueId)

                if TenantId then
                    table.insert(Result, ExternalUniqueId)
                    table.insert(Result, TenantId)
                end
            end

            return Result
        ");

        var luaScriptResult = await redisDatabase.ScriptEvaluateAsync(luaScript, new { IndexKey = indexKey, MaxCount = maxSessions }).ConfigureAwait(false);

        if (luaScriptResult.IsNull)
        {
            return;
        }

        var overflowExternalUniqueIds = luaScriptResult.ToDictionary();

        foreach (var overflowId in overflowExternalUniqueIds)
        {
            var externalUniqueId = ValueConversions.GuidFromString(overflowId.Key.ToString());
            var tenantId = overflowId.Value.ToString()!;

            sessions[externalUniqueId] = tenantId;
        }
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
        var sessions = new Dictionary<Guid, string>();

        if ((policies.MaximumSessionsPerAccountSpecificDevice != null) &&
            policies.MaximumSessionsPerAccountSpecificDevice.TryGetValue(deviceIdentificationType, out var devicePolicy) &&
            (devicePolicy.MaximumSessions != null) && (devicePolicy.MaximumSessions > 0))
        {
            await CollectOverflowExternalUniqueIdsAsync(redisDatabase,
                $"idx:tenant:{tenantId}:account:{accountId}:device:{deviceIdentificationType}:updated",
                devicePolicy.MaximumSessions.Value, sessions).ConfigureAwait(false);
        }
        else if ((policies.MaximumSessionsPerAccountDevice != null) && (policies.MaximumSessionsPerAccountDevice > 0))
        {
            await CollectOverflowExternalUniqueIdsAsync(redisDatabase,
                $"idx:tenant:{tenantId}:account:{accountId}:device:{deviceIdentificationType}:updated",
                policies.MaximumSessionsPerAccountDevice.Value, sessions).ConfigureAwait(false);
        }

        if ((policies.MaximumSessionsPerAccount != null) && (policies.MaximumSessionsPerAccount > 0))
        {
            await CollectOverflowExternalUniqueIdsAsync(redisDatabase,
                $"idx:tenant:{tenantId}:account:{accountId}:updated",
                policies.MaximumSessionsPerAccount.Value, sessions).ConfigureAwait(false);
        }

        if ((policies.MaximumSessionsPerTenant != null) && (policies.MaximumSessionsPerTenant > 0))
        {
            await CollectOverflowExternalUniqueIdsAsync(redisDatabase,
                $"idx:tenant:{tenantId}:updated",
                policies.MaximumSessionsPerTenant.Value, sessions).ConfigureAwait(false);
        }

        if ((policies.MaximumSessions != null) && (policies.MaximumSessions > 0))
        {
            await CollectOverflowExternalUniqueIdsAsync(redisDatabase,
                "idx:sessions:updated",
                policies.MaximumSessions.Value, sessions).ConfigureAwait(false);
        }

        foreach (var session in sessions)
        {
            await ExpireSessionByExternalUniqueIdsAsync(session.Value, [session.Key], cancellationToken).ConfigureAwait(false);
        }
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

            if (not TenantIdLocal or TenantIdLocal == false) then
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

            local SessionData = redis.call('HGETALL', SessionKey)
            local FinalResponse = { 'CALL_STATUS', 'OK', 'COUNT', '1' }

            for i = 1, #SessionData do
                table.insert(FinalResponse, SessionData[i])
            end

            return FinalResponse
        ");

        var luaScriptParameters = new LuaScriptParameters(tenantId, token);
        var luaScriptResult = await redisDatabase.ScriptEvaluateAsync(luaScript, luaScriptParameters).ConfigureAwait(false);

        var sessionFields = ExtractCallStatusCountAndFields(luaScriptResult, out var callStatus, out var count);

        if (callStatus != "OK")
        {
            if ((callStatus == "NOK_INVALID") ||
                (callStatus == "NOK_EXPIRED"))
            {
                return [];
            }

            throw new InvalidOperationException($"Unexpected error");
        }

        if (count == "1")
        {
            var session = Map(sessionFields);

            return [session];
        }

        var sessions = new List<ISession>();

        foreach (var sessionField in sessionFields)
        {
            if (string.IsNullOrWhiteSpace(sessionField.Key))
            {
                continue;
            }

            tenantId = sessionField.Value.ToString();

            if (string.IsNullOrWhiteSpace(tenantId))
            {
                continue;
            }

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
                'RefreshToken', @RefreshToken,
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

            redis.call('HSET', 'idx:sessions:tenants',
                @ExternalUniqueId, @TenantId)

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

            if @Token ~= SessionToken then
                local TokenKeyExists = redis.call('EXISTS', TokenKey)

                if TokenKeyExists > 0 then
                    return 'NOK_DUPLICATED_TOKEN'
                end

                local SessionTokenKey = 'token:'..@TenantId..':'..SessionToken

                redis.call('DEL', SessionTokenKey)

                local IndexSessionTokenGlobal = 'idx:token:'..SessionToken
                redis.call('ZREM', IndexSessionTokenGlobal, @ExternalUniqueId)
            end

            redis.call('SET', TokenKey, @ExternalUniqueId)

            redis.call('HSET', SessionKey,
                'Token', @Token,
                'RefreshToken', @RefreshToken,
                'UpdatedTimestamp', @Now)

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

            redis.call('DEL', SessionTokenKey)

            redis.call('HDEL', 'idx:sessions:tenants', @ExternalUniqueId)

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

            redis.call('HDEL', 'idx:sessions:tenants', @ExternalUniqueId)

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

        var sessionFields = ExtractCallStatusCountAndFields(luaScriptResult, out var callStatus, out var count);

        if (callStatus != "OK")
        {
            if ((callStatus == "NOK_INVALID") ||
                (callStatus == "NOK_EXPIRED"))
            {
                return null;
            }

            throw new InvalidOperationException($"Unexpected error");
        }

        if (count != "1")
        {
            throw new InvalidOperationException($"Unexpected error");
        }

        var session = Map(sessionFields);

        return session;
    }

    private static Dictionary<string, RedisResult> ExtractCallStatusCountAndFields(RedisResult? luaScriptResult, out string callStatus, out string count)
    {
        ArgumentNullException.ThrowIfNull(luaScriptResult, nameof(luaScriptResult));

        var dictionary = luaScriptResult.ToDictionary();

        callStatus = dictionary.TryGetValue("CALL_STATUS", out var redisValueCallStatus) ? redisValueCallStatus.ToString() : string.Empty;
        count = dictionary.TryGetValue("COUNT", out var redisValueCount) ? redisValueCount.ToString() : string.Empty;

        _ = dictionary.Remove("CALL_STATUS");
        _ = dictionary.Remove("COUNT");

        return dictionary;
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
