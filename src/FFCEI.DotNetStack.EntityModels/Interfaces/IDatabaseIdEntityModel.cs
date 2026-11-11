namespace FFCEI.DotNetStack.EntityModels.Interfaces;

public interface IDatabaseIdEntityModel
    : IEntityModel
{
    long Id { get; set; }
}
