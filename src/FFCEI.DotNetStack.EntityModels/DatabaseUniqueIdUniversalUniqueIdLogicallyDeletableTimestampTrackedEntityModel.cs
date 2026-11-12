using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public abstract class DatabaseUniqueIdUniversalUniqueIdLogicallyDeletableTimestampTrackedEntityModel
    : EntityModel, IDatabaseUniqueIdEntityModel, IUniversalUniqueIdEntityModel, ILogicallyDeletableEntityModel, ITimestampTrackedEntityModel
{
    public long Id { get; set; }
    public Guid UniversalUniqueId { get; set; } = IUniversalUniqueIdEntityModelExtensionMethods.GenerateModelUniversalUniqueId();
    public bool IsLogicallyDeleted { get; set; }
    public DateTimeOffset? LogicallyDeletedTimestamp { get; set; }
    public bool WasLogicallyDeletedCascaded { get; set; }
    public DateTimeOffset CreatedTimestamp { get; set; }
    public DateTimeOffset UpdatedTimestamp { get; set; }
}
