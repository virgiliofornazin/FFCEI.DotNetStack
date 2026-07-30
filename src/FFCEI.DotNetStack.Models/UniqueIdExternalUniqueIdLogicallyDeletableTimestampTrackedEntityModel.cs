using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public abstract class UniqueIdExternalUniqueIdLogicallyDeletableTimestampTrackedEntityModel
    : EntityModel, IUniqueIdEntityModel, IExternalUniqueIdEntityModel, ITimestampTrackedEntityModel, ILogicallyDeletableEntityModel
{
    public long Id { get; set; }
    public Guid ExternalUniqueId { get; set; }
    public bool IsLogicallyDeleted { get; set; }
    public DateTimeOffset? LogicallyDeletedTimestamp { get; set; }
    public bool WasLogicallyDeletedCascaded { get; set; }
    public DateTimeOffset CreatedTimestamp { get; set; }
    public DateTimeOffset UpdatedTimestamp { get; set; }
}
