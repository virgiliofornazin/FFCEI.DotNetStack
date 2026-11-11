using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public abstract class DatabaseIdEntityModel
    : EntityModel, IDatabaseIdEntityModel
{
    public long Id { get; set; }
}
