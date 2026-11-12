using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public abstract class DatabaseUniqueIdUniversalUniqueIdTimestampTrackedEntityModel
    : EntityModel, IDatabaseUniqueIdEntityModel, IUniversalUniqueIdEntityModel, ITimestampTrackedEntityModel
{
    public long Id { get; set; }
    public Guid UniversalUniqueId { get; set; } = IUniversalUniqueIdEntityModelExtensionMethods.GenerateModelUniversalUniqueId();
    public DateTimeOffset CreatedTimestamp { get; set; }
    public DateTimeOffset UpdatedTimestamp { get; set; }
}
