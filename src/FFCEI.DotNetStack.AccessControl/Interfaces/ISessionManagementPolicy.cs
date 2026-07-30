namespace FFCEI.DotNetStack.AccessControl.Interfaces;

public interface ISessionManagementPolicy
{
    uint? MaximumSessions { get; }
    TimeSpan? MinimumDuration { get; }
    TimeSpan? MaximumDuration { get; }
}
