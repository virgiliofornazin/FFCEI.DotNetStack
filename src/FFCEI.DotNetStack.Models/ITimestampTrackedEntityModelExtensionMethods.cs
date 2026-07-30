using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.Models;

public static class ITimestampTrackedEntityModelExtensionMethods
{
    public static void TouchModelTimestamps(this ITimestampTrackedEntityModel timestampTrackedModel, DateTimeOffset? timestamp = null)
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
