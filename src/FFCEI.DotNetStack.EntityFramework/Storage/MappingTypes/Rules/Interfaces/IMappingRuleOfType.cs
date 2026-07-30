using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FFCEI.DotNetStack.EntityFramework.Storage.MappingTypes.Rules.Interfaces;

internal interface IMappingRuleOfType
{
    Action<PropertyBuilder, DatabaseProviderType>? PropertyBuilderAction { get; }

    Func<PropertyBuilder, DatabaseProviderType, (string? columnType, string? collation, string? characterSet)>? OverridenColumnMappingAction { get; }
}
