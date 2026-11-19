using FFCEI.DotNetStack.DevelopmentTests.Library.EntityModels;
using FFCEI.DotNetStack.EntityFramework.Interfaces;
using FFCEI.DotNetStack.EntityFramework.Interfaces.Generic;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.EntityFramework;

public interface IBloggingEntityModelRepositoryContainer
   : IEntityModelRepositoryContainer
{
    IEntityModelRepository<IBlog> BlogEntities { get; }
    IEntityModelRepository<IBlogRow> BlogRowEntities { get; }
    IEntityModelRepository<IBlogData> BlogDataEntities { get; }
    IEntityModelRepository<ITestAllDataTypes> TestAllDataTypesEntities { get; }
    IEntityModelRepository<INullableTestAllDataTypes> NullableTestAllDataTypeEntities { get; }
}
