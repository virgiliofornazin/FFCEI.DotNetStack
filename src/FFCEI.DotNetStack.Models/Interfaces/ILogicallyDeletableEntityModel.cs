namespace FFCEI.DotNetStack.Models.Interfaces;

public interface ILogicallyDeletableEntityModel
    : IEntityModel
{
    bool IsLogicallyDeleted { get; set; }
    DateTimeOffset? LogicallyDeletedTimestamp { get; set; }
    bool WasLogicallyDeletedCascaded { get; set; }
}
