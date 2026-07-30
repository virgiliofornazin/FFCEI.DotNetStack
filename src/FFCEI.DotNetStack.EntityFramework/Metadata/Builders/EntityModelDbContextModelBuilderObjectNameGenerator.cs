using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FFCEI.DotNetStack.EntityFramework.Metadata.Builders;

internal sealed class EntityModelDbContextModelBuilderObjectNameGenerator
{
    private readonly IConventionEntityTypeBuilder _entityBuilder;
    private readonly string _entityNameForGeneration;

    public EntityModelDbContextModelBuilderObjectNameGenerator(IConventionEntityTypeBuilder entityBuilder)
    {
        ArgumentNullException.ThrowIfNull(entityBuilder, nameof(entityBuilder));

        _entityBuilder = entityBuilder;
        _entityNameForGeneration = _entityBuilder.Metadata.ShortName();
    }

    public EntityModelDbContextModelBuilderObjectNameGenerator(EntityTypeBuilder entityBuilder)
    {
        ArgumentNullException.ThrowIfNull(entityBuilder, nameof(entityBuilder));

        _entityBuilder = entityBuilder.GetInfrastructure<IConventionEntityTypeBuilder>();
        _entityNameForGeneration = _entityBuilder.Metadata.ShortName();

        ArgumentNullException.ThrowIfNull(_entityBuilder, nameof(_entityBuilder));
    }

    private string Generate(string attributeType, int counter)
        => $"{_entityNameForGeneration}_{attributeType}_{counter}";

    private int ReadCounterValue(string counterName)
    {
        var annotation = _entityBuilder.Metadata.FindAnnotation(counterName);

        return annotation?.Value is int value ? value : 0;
    }

    private void WriteCounterValue(string counterName, int counterValue)
    {
        _ = _entityBuilder.HasAnnotation(counterName, counterValue);
    }

    private int NextCounterValue(string counterName)
    {
        var result = ReadCounterValue(counterName) + 1;

        WriteCounterValue(counterName, result);

        return result;
    }

    private int NextForeignKeyCounter()
        => NextCounterValue("FFCEIModelBuilder:Counters:FK");

    private int NextUniqueKeyCounter()
        => NextCounterValue("FFCEIModelBuilder:Counters:UK");

    private int NextIndexCounter()
        => NextCounterValue("FFCEIModelBuilder:Counters:IX");

    public string GenerateForeignKeyName(int? index = null)
        => Generate("FK", index ?? NextForeignKeyCounter());

    public string GenerateUniqueKeyName(int? index = null)
        => Generate("UK", index ?? NextUniqueKeyCounter());

    public string GenerateIndexName(int? index = null)
        => Generate("IX", index ?? NextIndexCounter());
}
