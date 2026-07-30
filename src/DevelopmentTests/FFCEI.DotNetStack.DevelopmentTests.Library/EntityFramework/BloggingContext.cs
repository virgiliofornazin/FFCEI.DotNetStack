using FFCEI.DotNetStack.EntityFramework;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.EntityFramework;

public class BloggingContext(DbContextOptions<BloggingContext> options, SqliteConnection? connection)
   : EntityModelDbContext(options, Assembly.GetExecutingAssembly())
{
    public SqliteConnection? SqliteDb { get; private set; } = connection;
}
