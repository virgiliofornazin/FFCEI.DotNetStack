using FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes.Rules.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes.Rules;

internal class MappingRuleOfType
    : IMappingRuleOfType
{
    public Action<PropertyBuilder, DatabaseProviderType>? PropertyBuilderAction { get; set; }

    public Func<PropertyBuilder, DatabaseProviderType, (string? columnType, string? collation, string? characterSet)>? OverridenColumnMappingAction { get; set; }
}
