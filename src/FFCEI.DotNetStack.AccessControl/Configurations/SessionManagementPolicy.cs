using FFCEI.DotNetStack.AccessControl.Interfaces;

namespace FFCEI.DotNetStack.AccessControl.Configurations;

public class SessionManagementPolicy
    : ISessionManagementPolicy
{
    public uint? MaximumSessions { get; set; }
    public TimeSpan? MinimumDuration { get; set; }
    public TimeSpan? MaximumDuration { get; set; }
}
