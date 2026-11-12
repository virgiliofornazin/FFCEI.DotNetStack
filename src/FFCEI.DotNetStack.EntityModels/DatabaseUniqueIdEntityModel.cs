using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public abstract class DatabaseUniqueIdEntityModel
    : EntityModel, IDatabaseUniqueIdEntityModel
{
    public long Id { get; set; }
}
