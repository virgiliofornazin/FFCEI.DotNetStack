using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public abstract class ExternalUniqueIdLogicallyDeletableTimestampTrackedEntityModel
    : EntityModel, IExternalUniqueIdEntityModel, ITimestampTrackedEntityModel, ILogicallyDeletableEntityModel
{
    public Guid ExternalUniqueId { get; set; }
    public bool IsLogicallyDeleted { get; set; }
    public DateTimeOffset? LogicallyDeletedTimestamp { get; set; }
    public bool WasLogicallyDeletedCascaded { get; set; }
    public DateTimeOffset CreatedTimestamp { get; set; }
    public DateTimeOffset UpdatedTimestamp { get; set; }
}
