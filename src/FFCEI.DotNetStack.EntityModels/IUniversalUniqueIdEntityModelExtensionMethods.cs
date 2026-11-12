using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public static class IUniversalUniqueIdEntityModelExtensionMethods
{
    public static void GenerateEntityModelUniversalUniqueId(this IUniversalUniqueIdEntityModel universalUniqueIdEntityModel)
    {
        ArgumentNullException.ThrowIfNull(universalUniqueIdEntityModel, nameof(universalUniqueIdEntityModel));

        if (universalUniqueIdEntityModel.UniversalUniqueId == default)
        {
            universalUniqueIdEntityModel.UniversalUniqueId = GenerateModelUniversalUniqueId();
        }
    }

    public static Guid GenerateModelUniversalUniqueId() => Guid.CreateVersion7();
}
