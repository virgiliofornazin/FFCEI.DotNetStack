using FFCEI.DotNetStack.DevelopmentTests.Library.Models;
using FFCEI.DotNetStack.DevelopmentTests.Library.TestModels;
using FFCEI.DotNetStack.EntityFramework;
using FFCEI.DotNetStack.EntityFramework.Interfaces.Generic;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.EntityFramework;

public class BloggingEntityModelRepositoryContainer(BloggingContext context)
   : EntityModelRepositoryContainer(context)
#if TEST_EFCORE_USE_INTERFACES
    , IBloggingRepositoryContainer
#endif
{
#if TEST_EFCORE_MAPPINGS
    public IEntityModelRepository<IFullMapping> FullMappingModels => Repository<FullMapping, IFullMapping>(ref field);
    public IReadOnlyEntityModelRepository<IFullMappingView> FullMappingViewModels => ReadOnlyRepository<FullMappingView, IFullMappingView>(ref field);
#endif
#if TEST_EFCORE_USE_INTERFACES
    public IEntityModelRepository<IBlog> Blogs => Repository<Blog, IBlog>(ref field);
    public IEntityModelRepository<IBlogRow> BlogRows => Repository<BlogRow, IBlogRow>(ref field);
    public IEntityModelRepository<IBlogData> BlogDatas => Repository<BlogData, IBlogData>(ref field);
    public IEntityModelRepository<ITestModel> TestModels => Repository<TestModel, ITestModel>(ref field);
    public IEntityModelRepository<INullableTestModel> NullableTestModels => Repository<NullableTestModel, INullableTestModel>(ref field);
#else
    public IEntityModelRepository<TestModel> TestModels => Repository(ref field);
    public IEntityModelRepository<NullableTestModel> NullableTestModels => Repository(ref field);
    public IEntityModelRepository<Blog> Blogs => Repository(ref field);
    public IEntityModelRepository<BlogRow> BlogRows => Repository(ref field);
    public IEntityModelRepository<BlogData> BlogDatas => Repository(ref field);
#endif
}
