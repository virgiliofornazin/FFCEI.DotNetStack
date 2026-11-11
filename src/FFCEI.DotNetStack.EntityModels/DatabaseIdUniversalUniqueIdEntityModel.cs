using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public abstract class DatabaseIdUniversalUniqueIdEntityModel
    : EntityModel, IDatabaseIdEntityModel, IUniversalUniqueIdEntityModel
{
    public long Id { get; set; }
    public Guid UniversalUniqueId { get; set; } = Guid.CreateVersion7();
}
