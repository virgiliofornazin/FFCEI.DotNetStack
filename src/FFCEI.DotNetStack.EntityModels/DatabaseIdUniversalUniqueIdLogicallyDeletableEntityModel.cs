using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public abstract class DatabaseIdUniversalUniqueIdLogicallyDeletableEntityModel
    : EntityModel, IDatabaseIdEntityModel, IUniversalUniqueIdEntityModel, ILogicallyDeletableEntityModel
{
    public long Id { get; set; }
    public Guid UniversalUniqueId { get; set; } = Guid.CreateVersion7();
    public bool IsLogicallyDeleted { get; set; }
    public DateTimeOffset? LogicallyDeletedTimestamp { get; set; }
    public bool WasLogicallyDeletedCascaded { get; set; }
}
