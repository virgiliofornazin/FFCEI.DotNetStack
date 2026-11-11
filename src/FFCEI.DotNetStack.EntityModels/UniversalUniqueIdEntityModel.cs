using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public abstract class UniversalUniqueIdEntityModel
    : EntityModel, IUniversalUniqueIdEntityModel
{
    public Guid UniversalUniqueId { get; set; } = Guid.CreateVersion7();
}
