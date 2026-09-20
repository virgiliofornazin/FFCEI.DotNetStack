namespace FFCEI.DotNetStack.AccessControl.Interfaces;

public interface IAuthenticationManager
{
    Task<IAuthenticationProcess> BeginAuthenticationProcessAsync(
        string tenantId,
        string? accountLogin = null,
        IDeviceIdentification? deviceIdentification = null,
        IReadOnlyDictionary<string, string>? attributes = null,
        CancellationToken cancellationToken = default);
}
