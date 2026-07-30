using FFCEI.DotNetStack.AccessControl.Interfaces;

namespace FFCEI.DotNetStack.AccessControl.Configurations;

public sealed class SessionManagementPolicies
    : SessionManagementPolicy, ISessionManagementPolicies
{
    public static readonly SessionManagementPolicies NoPolicy = new();

    public bool RequiredExpiration { get; set; }
    public long? MaximumSessionsPerTenant { get; set; }
    public long? MaximumSessionsPerAccount { get; set; }
    public long? MaximumSessionsPerAccountDevice { get; set; }
    public Dictionary<DeviceIdentificationType, ISessionManagementPolicy>? MaximumSessionsPerAccountSpecificDevice { get; internal set; } = [];
    IReadOnlyDictionary<DeviceIdentificationType, ISessionManagementPolicy>? ISessionManagementPolicies.MaximumSessionsPerAccountSpecificDevice => MaximumSessionsPerAccountSpecificDevice;
}
