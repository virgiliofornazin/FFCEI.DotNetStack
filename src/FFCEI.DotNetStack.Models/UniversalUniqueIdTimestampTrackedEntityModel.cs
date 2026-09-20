using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public abstract class UniversalUniqueIdTimestampTrackedEntityModel
    : EntityModel, IUniversalUniqueIdEntityModel, ITimestampTrackedEntityModel
{
    public Guid UniversalUniqueId { get; set; }
    public DateTimeOffset CreatedTimestamp { get; set; }
    public DateTimeOffset UpdatedTimestamp { get; set; }
}
