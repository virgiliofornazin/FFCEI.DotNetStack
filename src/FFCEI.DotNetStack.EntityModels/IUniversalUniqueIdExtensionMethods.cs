using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public static class IUniversalUniqueIdExtensionMethods
{
    public static void GenerateModelUniversalUniqueId(this IUniversalUniqueIdEntityModel universalUniqueIdEntityModel)
    {
        ArgumentNullException.ThrowIfNull(universalUniqueIdEntityModel, nameof(universalUniqueIdEntityModel));

        if (universalUniqueIdEntityModel.UniversalUniqueId == default)
        {
            universalUniqueIdEntityModel.UniversalUniqueId = Guid.CreateVersion7();
        }
    }
}
