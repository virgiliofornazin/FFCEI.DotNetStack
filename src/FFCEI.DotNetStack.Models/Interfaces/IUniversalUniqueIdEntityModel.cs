namespace FFCEI.DotNetStack.Models.Interfaces;

public interface IUniversalUniqueIdEntityModel
    : IEntityModel
{
    Guid UniversalUniqueId { get; set; }
}
