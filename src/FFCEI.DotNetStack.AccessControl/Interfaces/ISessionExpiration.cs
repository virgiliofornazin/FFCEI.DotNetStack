using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.AccessControl.Interfaces;

public interface ISessionExpiration
    : ITimestampTrackedEntityModel
{
    TimeSpan? InactivityExpirationInterval { get; internal set; }
    DateTimeOffset? ExpiresTimestamp { get; internal set; }
    DateTimeOffset? FinalizedTimestamp { get; }
}
