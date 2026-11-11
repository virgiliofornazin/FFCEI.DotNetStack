using FFCEI.DotNetStack.EntityModels;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.EntityModels;

public class SessionRefreshTokenHistoryEntityModel
    : DatabaseIdEntityModel
{
    public long? SessionId { get; set; }
    public SessionEntityModel? Session { get; set; }
    public required string RefreshToken { get; set; }
}
