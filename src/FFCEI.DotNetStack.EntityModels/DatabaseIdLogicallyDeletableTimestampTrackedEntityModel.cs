using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public abstract class DatabaseIdLogicallyDeletableTimestampTrackedEntityModel
    : EntityModel, IDatabaseIdEntityModel, ILogicallyDeletableEntityModel, ITimestampTrackedEntityModel
{
    public long Id { get; set; }
    public bool IsLogicallyDeleted { get; set; }
    public DateTimeOffset? LogicallyDeletedTimestamp { get; set; }
    public bool WasLogicallyDeletedCascaded { get; set; }
    public DateTimeOffset CreatedTimestamp { get; set; }
    public DateTimeOffset UpdatedTimestamp { get; set; }
}
