using FFCEI.DotNetStack.AccessControl.SessionManagement.EntityModels;
using FFCEI.DotNetStack.EntityFramework;
using FFCEI.DotNetStack.EntityFramework.Interfaces.Generic;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.EntityFramework;

public sealed class SessionManagerEntityModelRepositoryContainer(SessionManagerDbContext dbContext)
    : EntityModelRepositoryContainer(dbContext)
{
    public IEntityModelRepository<Session> Sessions => Repository(ref field);
    public IEntityModelRepository<SessionAttribute> SessionAttributes => Repository(ref field);
    public IEntityModelRepository<DeviceIdentification> DeviceIdentifications => Repository(ref field);
}
