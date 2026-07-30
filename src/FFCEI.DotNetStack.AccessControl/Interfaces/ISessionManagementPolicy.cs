namespace FFCEI.DotNetStack.AccessControl.Interfaces;

public interface ISessionManagementPolicy
{
    long? MaximumSessions { get; }
    TimeSpan? MinimumDuration { get; }
    TimeSpan? MaximumDuration { get; }
}
