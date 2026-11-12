using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public static class ILogicallyDeletableEntityModelExtensionMethods
{
    public static void GenerateEntityModelLogicallyDeletedFlags(this ILogicallyDeletableEntityModel logicallyDeletableEntityModel, DateTimeOffset? timestamp = null)
    {
        ArgumentNullException.ThrowIfNull(logicallyDeletableEntityModel, nameof(logicallyDeletableEntityModel));

        timestamp ??= DateTimeOffset.UtcNow;

        if (logicallyDeletableEntityModel.IsLogicallyDeleted)
        {
            logicallyDeletableEntityModel.LogicallyDeletedTimestamp ??= timestamp;
        }
        else
        {
            logicallyDeletableEntityModel.LogicallyDeletedTimestamp = null;
            logicallyDeletableEntityModel.WasLogicallyDeletedCascaded = false;
        }
    }
}
