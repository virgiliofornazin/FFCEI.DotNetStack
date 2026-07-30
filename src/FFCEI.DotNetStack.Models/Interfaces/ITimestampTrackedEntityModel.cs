namespace FFCEI.DotNetStack.Models.Interfaces;

public interface ITimestampTrackedEntityModel
    : IEntityModel
{
    DateTimeOffset CreatedTimestamp { get; set; }
    DateTimeOffset UpdatedTimestamp { get; set; }
}
