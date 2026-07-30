using FFCEI.DotNetStack.AccessControl.Interfaces;

namespace FFCEI.DotNetStack.AccessControl;

public static class ISessionExpirationExtensionMethods
{
    public static bool HasExpiration(this ISessionExpiration session)
    {
        ArgumentNullException.ThrowIfNull(session, nameof(session));

        return session.FinalizedTimestamp.HasValue || session.ExpiresTimestamp.HasValue || session.InactivityExpirationInterval.HasValue;
    }

    public static bool SessionExpired(this ISessionExpiration session, DateTimeOffset? timestamp = null)
    {
        ArgumentNullException.ThrowIfNull(session, nameof(session));

        if (session.FinalizedTimestamp != null)
        {
            return true;
        }

        timestamp ??= DateTimeOffset.UtcNow;

        if ((session.InactivityExpirationInterval != null) && ((timestamp - session.UpdatedTimestamp) > session.InactivityExpirationInterval))
        {
            return true;
        }

        if ((session.ExpiresTimestamp != null) && (timestamp > session.ExpiresTimestamp))
        {
            return true;
        }

        return false;
    }
}
