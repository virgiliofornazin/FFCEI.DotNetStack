using FFCEI.DotNetStack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FFCEI.DotNetStack.EntityFramework.Interceptors;

public sealed class GenerateAutomaticPropertyValuesSaveChangesInterceptor
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));

        GenerateAutomaticPropertyValues(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventData, nameof(eventData));

        GenerateAutomaticPropertyValues(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void GenerateAutomaticPropertyValues(DbContext? context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var currentTimestamp = DateTimeOffset.UtcNow;
        var entries = context.ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is EntityModel entityModel)
            {
                entityModel.GenerateModelAutomaticPropertiesForSave(currentTimestamp);
            }
        }
    }
}
