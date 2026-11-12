using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public abstract class UniversalUniqueIdLogicallyDeletableTimestampTrackedEntityModel
    : EntityModel, IUniversalUniqueIdEntityModel, ILogicallyDeletableEntityModel, ITimestampTrackedEntityModel
{
    public Guid UniversalUniqueId { get; set; } = IUniversalUniqueIdEntityModelExtensionMethods.GenerateModelUniversalUniqueId();
    public bool IsLogicallyDeleted { get; set; }
    public DateTimeOffset? LogicallyDeletedTimestamp { get; set; }
    public bool WasLogicallyDeletedCascaded { get; set; }
    public DateTimeOffset CreatedTimestamp { get; set; }
    public DateTimeOffset UpdatedTimestamp { get; set; }
}
