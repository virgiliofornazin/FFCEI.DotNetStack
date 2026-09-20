using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public abstract class DatabaseIdEntityModel
    : EntityModel, IDatabaseIdEntityModel
{
    public long Id { get; set; }
}
