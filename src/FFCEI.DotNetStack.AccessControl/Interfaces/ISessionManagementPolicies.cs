namespace FFCEI.DotNetStack.AccessControl.Interfaces;

public interface ISessionManagementPolicies
    : ISessionManagementPolicy
{
    bool RequiredExpiration { get; }
    long? MaximumSessionsPerTenant { get; }
    long? MaximumSessionsPerAccount { get; }
    long? MaximumSessionsPerAccountDevice { get; set; }
    IReadOnlyDictionary<DeviceIdentificationType, ISessionManagementPolicy>? MaximumSessionsPerAccountSpecificDevice { get; }
}
