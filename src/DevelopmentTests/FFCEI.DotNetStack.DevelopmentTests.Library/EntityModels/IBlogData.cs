using FFCEI.DotNetStack.EntityModels.Interfaces;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.EntityModels;

public interface IBlogData
   : IDatabaseIdEntityModel, ILogicallyDeletableEntityModel, ITimestampTrackedEntityModel
{
    string ExternalId { get; }
    string DataText { get; }

    long? BlogId { get; }
    IBlog? Blog { get; }
}
