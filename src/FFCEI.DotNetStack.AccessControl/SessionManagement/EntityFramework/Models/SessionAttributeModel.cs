using FFCEI.DotNetStack.Models;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.EntityFramework.Models;

public class SessionAttributeModel
    : UniqueIdEntityModel
{
    public long? SessionId { get; set; }
    public SessionModel? Session { get; set; }
    public required string AttributeName { get; set; }
    public required string AttributeValue { get; set; }
}
