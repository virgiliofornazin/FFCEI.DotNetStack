using FFCEI.DotNetStack.DevelopmentTests.Library.Models;
using FFCEI.DotNetStack.EntityFramework.Interfaces;
using FFCEI.DotNetStack.EntityFramework.Interfaces.Generic;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.EntityFramework;

public interface IBloggingRepositoryContainer
   : IEntityModelRepositoryContainer
{
    IEntityModelRepository<IBlog> Blogs { get; }
    IEntityModelRepository<IBlogRow> BlogRows { get; }
    IEntityModelRepository<ITestModel> TestModels { get; }
    IEntityModelRepository<INullableTestModel> NullableTestModels { get; }
}
