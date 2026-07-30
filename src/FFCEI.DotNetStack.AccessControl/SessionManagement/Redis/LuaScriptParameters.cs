using FFCEI.DotNetStack.AccessControl.Interfaces;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Serialization;
using System.Diagnostics;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.Redis;

internal sealed class LuaScriptParameters
{
    public LuaScriptParameters(ISession session)
    {
        TenantId = session.TenantId;
        AccountId = session.AccountId;
        ExternalUniqueId = ValueConversions.GuidToString(session.ExternalUniqueId);
        Token = session.Token;
        RefreshToken = session.RefreshToken;
        DeviceIdentificationUserAgent = session.DeviceIdentification.UserAgent;
        DeviceIdentificationApplication = session.DeviceIdentification.Application;
        DeviceIdentificationOperatingSystem = session.DeviceIdentification.OperatingSystem;
        DeviceIdentificationMultiTouchPoints = session.DeviceIdentification.MultiTouchPoints;
        DeviceIdentificationDeviceType = session.DeviceIdentification.DeviceType.ToString();
        DeviceIdentificationMobile = session.DeviceIdentification.Mobile.ToZeroOrOne();
        ExpiresTimestamp = ValueConversions.TryDateTimeOffsetUtcToUnixMilliseconds(session.ExpiresTimestamp);
        InactivityExpirationInterval = ValueConversions.TryTimeSpanToMilliseconds(session.InactivityExpirationInterval);
        FinalizedTimestamp = ValueConversions.TryDateTimeOffsetUtcToUnixMilliseconds(session.FinalizedTimestamp);
        CreatedTimestamp = ValueConversions.DateTimeOffsetUtcToUnixMilliseconds(session.CreatedTimestamp);
        UpdatedTimestamp = ValueConversions.DateTimeOffsetUtcToUnixMilliseconds(session.UpdatedTimestamp);
        Attributes = JsonSerializer.OptimizedForMessagingInstance.SerializedCollectionToString(session.Attributes);
    }

    public LuaScriptParameters(string tenantId, string accountId, DeviceIdentificationType deviceIdentificationType, ISessionManagementPolicies policies)
    {
        MaximumSessions = policies.MaximumSessions;
        MaximumSessionsPerTenant = policies.MaximumSessionsPerTenant;
        MaximumSessionsPerAccount = policies.MaximumSessionsPerAccount;
        MaximumSessionsPerAccountDevice = policies?.MaximumSessionsPerAccountDevice;

        if ((policies!.MaximumSessionsPerAccountSpecificDevice != null) &&
            (policies.MaximumSessionsPerAccountSpecificDevice.TryGetValue(deviceIdentificationType, out var policy) &&
            (policy.MaximumSessions != null) &&
            (policy.MaximumSessions > 0)))
        {
            MaximumSessionsPerAccountDeviceSpecific = policy.MaximumSessions.Value;
        }
    }

    public LuaScriptParameters(string tenantId, Guid externalUniqueId, string? token = null, string? refreshToken = null)
    {
        TenantId = tenantId;
        ExternalUniqueId = ValueConversions.GuidToString(externalUniqueId);
        Token = token;
        RefreshToken = refreshToken;
    }

    public LuaScriptParameters(string? tenantId, string token)
    {
        TenantId = tenantId!;
        Token = token;
    }

    public LuaScriptParameters(string tenantId, string accountId, bool isAccount)
    {
        if (!isAccount)
        {
            throw new InvalidOperationException("isAccount must be true");
        }

        TenantId = tenantId;
        AccountId = accountId;
    }

    public LuaScriptParameters(string tenantId)
    {
        TenantId = tenantId;
    }

    public LuaScriptParameters(string tenantId, int startIndex, int endIndex)
    {
        TenantId = tenantId;
        StartIndex = startIndex;
        EndIndex = endIndex;
    }

    public LuaScriptParameters(string tenantId, IEnumerable<Guid> externalUniqueIds)
    {
        TenantId = tenantId;
        ExternalUniqueIds = JsonSerializer.OptimizedForMessagingInstance.SerializedCollectionToString(externalUniqueIds.Select(externalUniqueId => ValueConversions.GuidToString(externalUniqueId)));
    }

    public LuaScriptParameters(string tenantId, IEnumerable<string> tokens)
    {
        TenantId = tenantId;
        Tokens = JsonSerializer.OptimizedForMessagingInstance.SerializedCollectionToString(tokens.ToArray());
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    public long Now => ValueConversions.DateTimeOffsetUtcToSerialWithMilliseconds(_now);
    public long NowAsUnixMilliseconds => ValueConversions.DateTimeOffsetUtcToUnixMilliseconds(_now);
    public string TenantId { get; internal set; } = null!;
    public string? AccountId { get; internal set; }
    public string? ExternalUniqueId { get; internal set; }
    public string? Token { get; internal set; }
    public string? RefreshToken { get; internal set; }
    public string? DeviceIdentificationUserAgent { get; internal set; }
    public string? DeviceIdentificationApplication { get; internal set; }
    public string? DeviceIdentificationOperatingSystem { get; internal set; }
    public int? DeviceIdentificationMultiTouchPoints { get; internal set; }
    public string? DeviceIdentificationDeviceType { get; internal set; }
    public int? DeviceIdentificationMobile { get; internal set; }
    public long? ExpiresTimestamp { get; internal set; }
    public long? InactivityExpirationInterval { get; internal set; }
    public long? FinalizedTimestamp { get; internal set; }
    public long CreatedTimestamp { get; internal set; }
    public long UpdatedTimestamp { get; internal set; }
    public int? StartIndex { get; internal set; }
    public int? EndIndex { get; internal set; }
    public string? Attributes { get; internal set; }
    public long? MaximumSessions { get; internal set; }
    public long? MaximumSessionsPerTenant { get; internal set; }
    public long? MaximumSessionsPerAccount { get; internal set; }
    public long? MaximumSessionsPerAccountDevice { get; internal set; }
    public long? MaximumSessionsPerAccountDeviceSpecific { get; internal set; }
    public string? ExternalUniqueIds { get; internal set; }
    public string? Tokens { get; internal set; }
}
