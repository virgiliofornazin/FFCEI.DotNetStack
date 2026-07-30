using System.Security.Claims;

namespace FFCEI.DotNetStack.AccessControl.Interfaces;

public interface IClaimsIdentityOffloader
{
    Task<ClaimsIdentity> OffloadAsync(
        ClaimsIdentity identity,
        CancellationToken cancellationToken = default);
}
