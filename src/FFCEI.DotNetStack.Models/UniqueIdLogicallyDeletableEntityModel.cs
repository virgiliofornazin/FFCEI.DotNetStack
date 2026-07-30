using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public abstract class UniqueIdLogicallyDeletableEntityModel
    : EntityModel, IUniqueIdEntityModel, ILogicallyDeletableEntityModel
{
    public long Id { get; set; }
    public bool IsLogicallyDeleted { get; set; }
    public DateTimeOffset? LogicallyDeletedTimestamp { get; set; }
    public bool WasLogicallyDeletedCascaded { get; set; }
}
