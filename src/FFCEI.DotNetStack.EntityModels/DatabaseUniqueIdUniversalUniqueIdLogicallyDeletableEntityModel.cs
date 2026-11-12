using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public abstract class DatabaseUniqueIdUniversalUniqueIdLogicallyDeletableEntityModel
    : EntityModel, IDatabaseUniqueIdEntityModel, IUniversalUniqueIdEntityModel, ILogicallyDeletableEntityModel
{
    public long Id { get; set; }
    public Guid UniversalUniqueId { get; set; } = IUniversalUniqueIdEntityModelExtensionMethods.GenerateModelUniversalUniqueId();
    public bool IsLogicallyDeleted { get; set; }
    public DateTimeOffset? LogicallyDeletedTimestamp { get; set; }
    public bool WasLogicallyDeletedCascaded { get; set; }
}
