using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.Models;

public interface IBlogRow
    : IDatabaseIdEntityModel, ILogicallyDeletableEntityModel, ITimestampTrackedEntityModel
{
    long? LineRow { get; }
    string LineText { get; }

    long? BlogId { get; }
    IBlog? Blog { get; }
}
