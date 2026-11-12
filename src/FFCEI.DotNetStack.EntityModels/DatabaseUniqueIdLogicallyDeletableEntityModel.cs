using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public abstract class DatabaseUniqueIdLogicallyDeletableEntityModel
    : EntityModel, IDatabaseUniqueIdEntityModel, ILogicallyDeletableEntityModel
{
    public long Id { get; set; }
    public bool IsLogicallyDeleted { get; set; }
    public DateTimeOffset? LogicallyDeletedTimestamp { get; set; }
    public bool WasLogicallyDeletedCascaded { get; set; }
}
