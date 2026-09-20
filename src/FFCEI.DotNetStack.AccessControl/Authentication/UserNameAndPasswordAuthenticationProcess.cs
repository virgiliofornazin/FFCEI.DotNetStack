using FFCEI.DotNetStack.AccessControl.Interfaces;

namespace FFCEI.DotNetStack.AccessControl.Authentication;

public sealed class UserNameAndPasswordAuthenticationProcess : IAuthenticationProcess
{
    public AuthenticationProcessType ProcessType => AuthenticationProcessType.UserNameAndPassword;
}
