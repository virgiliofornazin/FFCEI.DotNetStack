using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public abstract class DatabaseIdUniversalUniqueIdTimestampTrackedEntityModel
    : EntityModel, IDatabaseIdEntityModel, IUniversalUniqueIdEntityModel, ITimestampTrackedEntityModel
{
    public long Id { get; set; }
    public Guid UniversalUniqueId { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedTimestamp { get; set; }
    public DateTimeOffset UpdatedTimestamp { get; set; }
}
