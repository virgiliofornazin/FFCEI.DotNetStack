using FFCEI.DotNetStack.EntityModels;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.EntityModels;

public class SessionAttributeEntityModel
    : DatabaseUniqueIdEntityModel
{
    public long? SessionId { get; set; }
    public SessionEntityModel? Session { get; set; }
    public required string AttributeName { get; set; }
    public required string AttributeValue { get; set; }
}
