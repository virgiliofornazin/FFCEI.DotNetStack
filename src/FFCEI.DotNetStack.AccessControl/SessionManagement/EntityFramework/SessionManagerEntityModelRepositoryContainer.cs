using FFCEI.DotNetStack.AccessControl.SessionManagement.EntityFramework.Models;
using FFCEI.DotNetStack.EntityFramework;
using FFCEI.DotNetStack.EntityFramework.Interfaces.Generic;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.EntityFramework;

public sealed class SessionManagerEntityModelRepositoryContainer(SessionManagerDbContext dbContext)
    : EntityModelRepositoryContainer(dbContext)
{
    public IEntityModelRepository<SessionModel> Sessions => Repository(ref field);
    public IEntityModelRepository<SessionAttributeModel> SessionAttributes => Repository(ref field);
    public IEntityModelRepository<DeviceIdentificationModel> DeviceIdentifications => Repository(ref field);
}
