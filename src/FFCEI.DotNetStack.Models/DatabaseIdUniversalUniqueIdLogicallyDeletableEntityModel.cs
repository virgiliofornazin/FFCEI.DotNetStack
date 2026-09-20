using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public abstract class DatabaseIdUniversalUniqueIdLogicallyDeletableEntityModel
    : EntityModel, IDatabaseIdEntityModel, IUniversalUniqueIdEntityModel, ILogicallyDeletableEntityModel
{
    public long Id { get; set; }
    public Guid UniversalUniqueId { get; set; }
    public bool IsLogicallyDeleted { get; set; }
    public DateTimeOffset? LogicallyDeletedTimestamp { get; set; }
    public bool WasLogicallyDeletedCascaded { get; set; }
}
