using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public abstract class UniversalUniqueIdEntityModel
    : EntityModel, IUniversalUniqueIdEntityModel
{
    public Guid UniversalUniqueId { get; set; } = Guid.CreateVersion7();
}
