namespace FFCEI.DotNetStack.Models.Interfaces;

public interface IExternalUniqueIdEntityModel
    : IEntityModel
{
    Guid ExternalUniqueId { get; set; }
}
