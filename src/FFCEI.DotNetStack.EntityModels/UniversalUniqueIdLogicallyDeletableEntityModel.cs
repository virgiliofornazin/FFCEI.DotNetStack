using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public abstract class UniversalUniqueIdLogicallyDeletableEntityModel
    : EntityModel, IUniversalUniqueIdEntityModel, ILogicallyDeletableEntityModel
{
    public Guid UniversalUniqueId { get; set; } = Guid.CreateVersion7();
    public bool IsLogicallyDeleted { get; set; }
    public DateTimeOffset? LogicallyDeletedTimestamp { get; set; }
    public bool WasLogicallyDeletedCascaded { get; set; }
}
