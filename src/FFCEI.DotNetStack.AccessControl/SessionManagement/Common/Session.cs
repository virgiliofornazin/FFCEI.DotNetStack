using FFCEI.DotNetStack.AccessControl.Interfaces;
using FFCEI.DotNetStack.Models;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.Common;

internal sealed class Session
    : UniversalUniqueIdTimestampTrackedEntityModel, ISession
{
    public required string TenantId { get; set; }
    public required string AccountId { get; set; }
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public DeviceIdentification DeviceIdentification { get; internal set; } = new();
    IDeviceIdentification ISession.DeviceIdentification => DeviceIdentification;
    public TimeSpan? InactivityExpirationInterval { get; set; }
    public DateTimeOffset? ExpiresTimestamp { get; set; }
    public DateTimeOffset? FinalizedTimestamp { get; set; }
    public Dictionary<string, string> Attributes { get; internal set; } = [];
    IReadOnlyDictionary<string, string> ISession.Attributes => Attributes;
}
