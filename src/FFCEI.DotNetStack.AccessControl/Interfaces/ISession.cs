using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.AccessControl.Interfaces;

public interface ISession
    : IUniversalUniqueIdEntityModel, ISessionExpiration
{
    string TenantId { get; }
    string AccountId { get; }
    string Token { get; }
    string RefreshToken { get; }
    IDeviceIdentification DeviceIdentification { get; }
    IReadOnlyDictionary<string, string> Attributes { get; }
}
