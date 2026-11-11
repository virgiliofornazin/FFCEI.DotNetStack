namespace FFCEI.DotNetStack.EntityModels.Interfaces;

public interface ILogicallyDeletableEntityModel
    : IEntityModel
{
    bool IsLogicallyDeleted { get; set; }
    DateTimeOffset? LogicallyDeletedTimestamp { get; set; }
    bool WasLogicallyDeletedCascaded { get; set; }
}
