using FFCEI.DotNetStack.AccessControl.SessionManagement.EntityModels;
using FFCEI.DotNetStack.EntityFramework;
using FFCEI.DotNetStack.EntityFramework.Interfaces.Generic;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.EntityFramework;

public sealed class SessionManagerEntityModelRepositoryContainer(SessionManagerDbContext dbContext)
    : EntityModelRepositoryContainer(dbContext)
{
    public IEntityModelRepository<Session> SessionEntities => Repository(ref field);
    public IEntityModelRepository<SessionAttribute> SessionAttributeEntities => Repository(ref field);
    public IEntityModelRepository<SessionRefreshTokenHistory> SessionRefreshTokenHistoryEntities => Repository(ref field);
    public IEntityModelRepository<DeviceIdentification> DeviceIdentificationEntities => Repository(ref field);
}
