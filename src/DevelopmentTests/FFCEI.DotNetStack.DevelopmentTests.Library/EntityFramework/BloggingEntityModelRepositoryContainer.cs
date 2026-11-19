using FFCEI.DotNetStack.DevelopmentTests.Library.EntityModels;
using FFCEI.DotNetStack.DevelopmentTests.Library.EntityModels.Testing;
using FFCEI.DotNetStack.EntityFramework;
using FFCEI.DotNetStack.EntityFramework.Interfaces.Generic;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.EntityFramework;

public class BloggingEntityModelRepositoryContainer(BloggingContext context)
   : EntityModelRepositoryContainer(context)
#if TEST_EFCORE_USE_INTERFACES
    , IBloggingEntityModelRepositoryContainer
#endif
{
#if TEST_EFCORE_MAPPINGS
    public IEntityModelRepository<IFullMapping> FullMappingEntities => Repository<FullMapping, IFullMapping>(ref field);
    public IReadOnlyEntityModelRepository<IFullMappingView> FullMappingViewEntities => ReadOnlyRepository<FullMappingView, IFullMappingView>(ref field);
#endif
#if TEST_EFCORE_USE_INTERFACES
    public IEntityModelRepository<IBlog> BlogEntities => Repository<Blog, IBlog>(ref field);
    public IEntityModelRepository<IBlogRow> BlogRowEntities => Repository<BlogRow, IBlogRow>(ref field);
    public IEntityModelRepository<IBlogData> BlogDataEntities => Repository<BlogData, IBlogData>(ref field);
    public IEntityModelRepository<ITestAllDataTypes> TestAllDataTypesEntities => Repository<TestAllDataTypes, ITestAllDataTypes>(ref field);
    public IEntityModelRepository<INullableTestAllDataTypes> NullableTestAllDataTypeEntities => Repository<NullableTestAllDataTypes, INullableTestAllDataTypes>(ref field);
#else
    public IEntityModelRepository<TestAllDataTypes> TestAllDataTypesEntities => Repository(ref field);
    public IEntityModelRepository<NullableTestAllDataTypes> NullableTestAllDataTypeEntities => Repository(ref field);
    public IEntityModelRepository<Blog> BlogEntities => Repository(ref field);
    public IEntityModelRepository<BlogRow> BlogRowEntities => Repository(ref field);
    public IEntityModelRepository<BlogData> BlogDataEntities => Repository(ref field);
#endif
}
