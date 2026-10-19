using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public abstract class UniversalUniqueIdTimestampTrackedEntityModel
    : EntityModel, IUniversalUniqueIdEntityModel, ITimestampTrackedEntityModel
{
    public Guid UniversalUniqueId { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedTimestamp { get; set; }
    public DateTimeOffset UpdatedTimestamp { get; set; }
}
