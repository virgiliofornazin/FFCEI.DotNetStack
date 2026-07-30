using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FFCEI.DotNetStack.EntityFramework.Storage.DataProtection;

public class DataProtectionKeyDbContext(DbContextOptions<DataProtectionKeyDbContext> options)
    : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
}
