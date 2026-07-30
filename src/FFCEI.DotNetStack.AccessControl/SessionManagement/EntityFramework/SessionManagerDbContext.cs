using FFCEI.DotNetStack.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.EntityFramework;

public sealed class SessionManagerDbContext(DbContextOptions options, Assembly rootAssembly)
    : EntityModelDbContext(options, rootAssembly)
{
}
