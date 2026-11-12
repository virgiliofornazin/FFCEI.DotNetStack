using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public abstract class DatabaseUniqueIdUniversalUniqueIdEntityModel
    : EntityModel, IDatabaseUniqueIdEntityModel, IUniversalUniqueIdEntityModel
{
    public long Id { get; set; }
    public Guid UniversalUniqueId { get; set; } = IUniversalUniqueIdEntityModelExtensionMethods.GenerateModelUniversalUniqueId();
}
