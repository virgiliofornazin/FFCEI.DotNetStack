using FFCEI.DotNetStack.Models;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.EntityFramework.Models;

public class SessionRefreshTokenHistoryModel
    : UniqueIdEntityModel
{
    public long? SessionId { get; set; }
    public SessionModel? Session { get; set; }
    public required string RefreshToken { get; set; }
}
