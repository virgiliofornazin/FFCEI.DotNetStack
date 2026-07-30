using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public abstract class ExternalUniqueIdEntityModel
    : EntityModel, IExternalUniqueIdEntityModel
{
    public Guid ExternalUniqueId { get; set; }
}
