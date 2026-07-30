namespace FFCEI.DotNetStack.Models.Interfaces;

public interface IUniqueIdEntityModel
    : IEntityModel
{
    long Id { get; set; }
}
