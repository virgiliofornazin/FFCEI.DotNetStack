
using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public abstract class DatabaseUniqueIdTimestampTrackedEntityModel
    : EntityModel, IDatabaseUniqueIdEntityModel, ITimestampTrackedEntityModel
{
    public long Id { get; set; }
    public DateTimeOffset CreatedTimestamp { get; set; }
    public DateTimeOffset UpdatedTimestamp { get; set; }
}
