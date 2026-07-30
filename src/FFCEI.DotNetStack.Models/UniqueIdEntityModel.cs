using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public abstract class UniqueIdEntityModel
    : EntityModel, IUniqueIdEntityModel
{
    public long Id { get; set; }
}
