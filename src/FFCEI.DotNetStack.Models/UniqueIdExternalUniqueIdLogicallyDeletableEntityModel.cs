using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public abstract class UniqueIdExternalUniqueIdLogicallyDeletableEntityModel
    : EntityModel, IUniqueIdEntityModel, IExternalUniqueIdEntityModel, ILogicallyDeletableEntityModel
{
    public long Id { get; set; }
    public Guid ExternalUniqueId { get; set; }
    public bool IsLogicallyDeleted { get; set; }
    public DateTimeOffset? LogicallyDeletedTimestamp { get; set; }
    public bool WasLogicallyDeletedCascaded { get; set; }
}
