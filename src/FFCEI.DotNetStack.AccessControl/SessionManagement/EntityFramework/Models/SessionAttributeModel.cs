using FFCEI.DotNetStack.Models;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.EntityFramework.Models;

public class SessionAttributeModel
    : DatabaseIdEntityModel
{
    public long? SessionId { get; set; }
    public SessionModel? Session { get; set; }
    public required string AttributeName { get; set; }
    public required string AttributeValue { get; set; }
}
