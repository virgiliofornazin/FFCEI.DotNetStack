using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.EntityModels;

public static class ITimestampTrackedEntityModelExtensionMethods
{
    public static void GenerateEntityModelTimestamps(this ITimestampTrackedEntityModel timestampTrackedModel, DateTimeOffset? timestamp = null)
    {
        ArgumentNullException.ThrowIfNull(timestampTrackedModel, nameof(timestampTrackedModel));

        timestamp ??= DateTimeOffset.UtcNow;

        if (timestampTrackedModel.CreatedTimestamp == default)
        {
            timestampTrackedModel.CreatedTimestamp = timestamp.Value;
        }

        timestampTrackedModel.UpdatedTimestamp = timestamp.Value;
    }
}
