using FFCEI.DotNetStack.EntityModels;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.EntityModels;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class SessionAttribute
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix
    : DatabaseUniqueIdEntityModel
{
    public long? SessionId { get; set; }
    public Session? Session { get; set; }
    public required string AttributeName { get; set; }
    public required string AttributeValue { get; set; }
}
