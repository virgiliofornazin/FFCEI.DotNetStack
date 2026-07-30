namespace FFCEI.DotNetStack.AccessControl.Interfaces;

public interface ISessionManagementPolicies
    : ISessionManagementPolicy
{
    bool RequiredExpiration { get; }
    uint? MaximumSessionsPerTenant { get; }
    uint? MaximumSessionsPerAccount { get; }
    uint? MaximumSessionsPerAccountDevice { get; set; }
    IReadOnlyDictionary<DeviceIdentificationType, ISessionManagementPolicy>? MaximumSessionsPerAccountSpecificDevice { get; }
}
