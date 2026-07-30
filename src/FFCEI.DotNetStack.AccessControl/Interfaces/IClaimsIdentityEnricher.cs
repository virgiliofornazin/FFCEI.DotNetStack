using System.Security.Claims;

namespace FFCEI.DotNetStack.AccessControl.Interfaces;

public interface IClaimsIdentityEnricher
{
    Task EnrichAsync(
        ClaimsIdentity identity,
        CancellationToken cancellationToken = default);
}
