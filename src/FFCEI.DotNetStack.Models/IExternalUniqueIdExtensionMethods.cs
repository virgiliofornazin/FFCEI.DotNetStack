using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public static class IExternalUniqueIdExtensionMethods
{
    public static Guid GenerateModelExternalUniqueId(this IExternalUniqueIdEntityModel externalUniqueIdEntityModel)
    {
        ArgumentNullException.ThrowIfNull(externalUniqueIdEntityModel, nameof(externalUniqueIdEntityModel));

        if (externalUniqueIdEntityModel.ExternalUniqueId == default)
        {
            externalUniqueIdEntityModel.ExternalUniqueId = Guid.CreateVersion7();
        }

        return externalUniqueIdEntityModel.ExternalUniqueId;
    }
}
