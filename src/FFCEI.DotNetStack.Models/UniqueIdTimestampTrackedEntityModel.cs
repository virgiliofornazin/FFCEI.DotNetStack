
using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public abstract class UniqueIdTimestampTrackedEntityModel
    : EntityModel, IUniqueIdEntityModel, ITimestampTrackedEntityModel
{
    public long Id { get; set; }
    public DateTimeOffset CreatedTimestamp { get; set; }
    public DateTimeOffset UpdatedTimestamp { get; set; }
}
