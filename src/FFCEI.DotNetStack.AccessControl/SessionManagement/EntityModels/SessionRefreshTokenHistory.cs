using FFCEI.DotNetStack.EntityModels;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.EntityModels;

public class SessionRefreshTokenHistory
    : DatabaseUniqueIdEntityModel
{
    public long? SessionId { get; set; }
    public Session? Session { get; set; }
    public required string RefreshToken { get; set; }
}
