using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public abstract class UniqueIdExternalUniqueIdTimestampTrackedEntityModel
    : EntityModel, IUniqueIdEntityModel, IExternalUniqueIdEntityModel, ITimestampTrackedEntityModel
{
    public long Id { get; set; }
    public Guid ExternalUniqueId { get; set; }
    public DateTimeOffset CreatedTimestamp { get; set; }
    public DateTimeOffset UpdatedTimestamp { get; set; }
}
