using FFCEI.DotNetStack.DevelopmentTests.Library.EntityModels;
using FFCEI.DotNetStack.EntityFramework.Interfaces;
using FFCEI.DotNetStack.EntityFramework.Interfaces.Generic;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.EntityFramework;

public interface IBloggingEntityModelRepositoryContainer
   : IEntityModelRepositoryContainer
{
    IEntityModelRepository<IBlog> Blogs { get; }
    IEntityModelRepository<IBlogRow> BlogRows { get; }
    IEntityModelRepository<IBlogData> BlogDatas { get; }
    IEntityModelRepository<ITestAllDataTypes> TestModels { get; }
    IEntityModelRepository<INullableTestAllDataTypes> NullableTestModels { get; }
}
