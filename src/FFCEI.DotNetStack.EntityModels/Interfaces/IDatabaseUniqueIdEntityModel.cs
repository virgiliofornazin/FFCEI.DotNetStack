namespace FFCEI.DotNetStack.EntityModels.Interfaces;

public interface IDatabaseUniqueIdEntityModel
    : IEntityModel
{
    long Id { get; set; }
}
