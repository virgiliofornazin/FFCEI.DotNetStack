
using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public abstract class DatabaseIdTimestampTrackedEntityModel
    : EntityModel, IDatabaseIdEntityModel, ITimestampTrackedEntityModel
{
    public long Id { get; set; }
    public DateTimeOffset CreatedTimestamp { get; set; }
    public DateTimeOffset UpdatedTimestamp { get; set; }
}
