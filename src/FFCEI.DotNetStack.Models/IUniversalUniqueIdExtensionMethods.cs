using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public static class IUniversalUniqueIdExtensionMethods
{
    public static Guid GenerateModelUniversalUniqueId(this IUniversalUniqueIdEntityModel universalUniqueIdEntityModel)
    {
        ArgumentNullException.ThrowIfNull(universalUniqueIdEntityModel, nameof(universalUniqueIdEntityModel));

        if (universalUniqueIdEntityModel.UniversalUniqueId == default)
        {
            universalUniqueIdEntityModel.UniversalUniqueId = Guid.CreateVersion7();
        }

        return universalUniqueIdEntityModel.UniversalUniqueId;
    }
}
