using FFCEI.DotNetStack.AccessControl.Interfaces;

namespace FFCEI.DotNetStack.AccessControl.Configurations;

public sealed class SessionManagementPolicies
    : SessionManagementPolicy, ISessionManagementPolicies
{
    public static readonly SessionManagementPolicies NoPolicy = new();

    public bool RequiredExpiration { get; set; }
    public uint? MaximumSessionsPerTenant { get; set; }
    public uint? MaximumSessionsPerAccount { get; set; }
    public uint? MaximumSessionsPerAccountDevice { get; set; }
    public Dictionary<DeviceIdentificationType, ISessionManagementPolicy>? MaximumSessionsPerAccountSpecificDevice { get; internal set; } = [];
    IReadOnlyDictionary<DeviceIdentificationType, ISessionManagementPolicy>? ISessionManagementPolicies.MaximumSessionsPerAccountSpecificDevice => MaximumSessionsPerAccountSpecificDevice;
}
