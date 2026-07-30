using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.EntityFramework;

internal static partial class EntityFrameworkLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "DbContext '{DbContextType}' and RepositoryContainer '{EntityModelRepositoryContainerType}' configured")]
    internal static partial void DbContextAndRepositoryConfigured(ILogger logger, Type dbContextType, Type entityModelRepositoryContainerType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Recovering logically deleted entity '{EntityType}' using strategy {Strategy}")]
    internal static partial void RecoveringLogicallyDeletedEntity(ILogger logger, string entityType, RecoverLogicallyDeletableStrategy strategy);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Cascading logical-delete recovery from '{EntityType}' to {DetailCount} detail(s) via navigation '{NavigationProperty}'")]
    internal static partial void CascadingLogicallyDeletedRecovery(ILogger logger, string entityType, int detailCount, string navigationProperty);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Cascading logical-delete from '{EntityType}' to detail '{DetailEntityType}' via navigation '{NavigationProperty}'")]
    internal static partial void CascadedLogicalDelete(ILogger logger, string entityType, string detailEntityType, string navigationProperty);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Cascading logical-delete undo from '{EntityType}' to detail '{DetailEntityType}' via navigation '{NavigationProperty}'")]
    internal static partial void CascadedLogicalDeleteUndo(ILogger logger, string entityType, string detailEntityType, string navigationProperty);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Cleared foreign key of detail '{DetailEntityType}' due to logical delete of '{EntityType}' via navigation '{NavigationProperty}'")]
    internal static partial void ClearedForeignKeyOnLogicalDelete(ILogger logger, string entityType, string detailEntityType, string navigationProperty);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Renamed index '{OldName}' to '{NewName}' on entity '{EntityType}'")]
    internal static partial void RenamedIndex(ILogger logger, string oldName, string newName, string entityType);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Mapped entity '{EntityType}' to table '{TableName}'")]
    internal static partial void MappedTable(ILogger logger, string entityType, string tableName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Mapped entity '{EntityType}' to view '{ViewName}'")]
    internal static partial void MappedView(ILogger logger, string entityType, string viewName);

    [LoggerMessage(Level = LogLevel.Information, Message = "EF Second Level Cache configured")]
    internal static partial void EFSecondLevelCacheConfigured(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Data Protection configured (using provider '{dataProtectionProvider}')")]
    internal static partial void DataProtectionConfigured(ILogger logger, string dataProtectionProvider);
}
