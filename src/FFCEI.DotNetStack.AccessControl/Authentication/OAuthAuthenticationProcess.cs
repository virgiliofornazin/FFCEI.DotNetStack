using FFCEI.DotNetStack.AccessControl.Interfaces;

namespace FFCEI.DotNetStack.AccessControl.Authentication;

public sealed class OAuthAuthenticationProcess : IAuthenticationProcess
{
    public AuthenticationProcessType ProcessType => AuthenticationProcessType.OAuth;
}
