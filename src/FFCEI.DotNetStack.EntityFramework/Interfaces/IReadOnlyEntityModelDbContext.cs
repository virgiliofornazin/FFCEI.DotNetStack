namespace FFCEI.DotNetStack.EntityFramework.Interfaces;

public interface IReadOnlyEntityModelDbContext
    : IDisposable
{
    DatabaseProviderType DatabaseProvider { get; }
    string DatabaseProviderName { get; }
}
