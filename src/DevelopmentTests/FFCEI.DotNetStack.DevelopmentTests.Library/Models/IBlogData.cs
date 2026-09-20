using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.Models;

public interface IBlogData
   : IDatabaseIdEntityModel, ILogicallyDeletableEntityModel, ITimestampTrackedEntityModel
{
    string ExternalId { get; }
    string DataText { get; }

    long? BlogId { get; }
    IBlog? Blog { get; }
}
