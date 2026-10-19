using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public abstract class DatabaseIdUniversalUniqueIdEntityModel
    : EntityModel, IDatabaseIdEntityModel, IUniversalUniqueIdEntityModel
{
    public long Id { get; set; }
    public Guid UniversalUniqueId { get; set; } = Guid.CreateVersion7();
}
