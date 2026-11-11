namespace FFCEI.DotNetStack.EntityModels.Interfaces;

public interface ITimestampTrackedEntityModel
    : IEntityModel
{
    DateTimeOffset CreatedTimestamp { get; set; }
    DateTimeOffset UpdatedTimestamp { get; set; }
}
