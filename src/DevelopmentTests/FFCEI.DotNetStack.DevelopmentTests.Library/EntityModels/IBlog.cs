using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.EntityModels;

public interface IBlog
   : IDatabaseIdEntityModel, IUniversalUniqueIdEntityModel, ILogicallyDeletableEntityModel, ITimestampTrackedEntityModel
{
    string Name { get; }
#pragma warning disable CA1056 // URI-like properties should not be strings
    string Url { get; }
#pragma warning restore CA1056 // URI-like properties should not be strings

    IReadOnlyCollection<IBlogRow> BlogRows { get; }
    IBlogData? BlogData { get; }
}
