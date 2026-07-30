using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public abstract class ExternalUniqueIdTimestampTrackedEntityModel
    : EntityModel, IExternalUniqueIdEntityModel, ITimestampTrackedEntityModel
{
    public Guid ExternalUniqueId { get; set; }
    public DateTimeOffset CreatedTimestamp { get; set; }
    public DateTimeOffset UpdatedTimestamp { get; set; }
}
