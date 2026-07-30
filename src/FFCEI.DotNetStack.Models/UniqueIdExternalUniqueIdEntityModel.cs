using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public abstract class UniqueIdExternalUniqueIdEntityModel
    : EntityModel, IUniqueIdEntityModel, IExternalUniqueIdEntityModel
{
    public long Id { get; set; }
    public Guid ExternalUniqueId { get; set; }
}
