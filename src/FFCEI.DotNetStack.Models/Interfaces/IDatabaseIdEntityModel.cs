namespace FFCEI.DotNetStack.Models.Interfaces;

public interface IDatabaseIdEntityModel
    : IEntityModel
{
    long Id { get; set; }
}
