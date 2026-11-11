using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.EntityModels;

public interface IBlogRow
    : IDatabaseIdEntityModel, ILogicallyDeletableEntityModel, ITimestampTrackedEntityModel
{
    long? LineRow { get; }
    string LineText { get; }

    long? BlogId { get; }
    IBlog? Blog { get; }
}
