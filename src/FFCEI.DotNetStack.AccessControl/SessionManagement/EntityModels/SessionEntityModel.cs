using FFCEI.DotNetStack.AccessControl.Interfaces;
using FFCEI.DotNetStack.EntityModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.EntityModels;

[Index(nameof(TenantId), nameof(Token), IsUnique = true)]
public class SessionEntityModel
    : DatabaseIdUniversalUniqueIdLogicallyDeletableTimestampTrackedEntityModel, ISessionExpiration
{
    public required string TenantId { get; set; }
    public required string AccountId { get; set; }
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public long? DeviceIdentificationId { get; set; }
    public DeviceIdentificationEntityModel? DeviceIdentification { get; set; }
    public TimeSpan? InactivityExpirationInterval { get; set; }
    public DateTimeOffset? ExpiresTimestamp { get; set; }
    public DateTimeOffset? FinalizedTimestamp
    {
        get => LogicallyDeletedTimestamp;
        set => LogicallyDeletedTimestamp = value;
    }
    public Collection<SessionAttributeEntityModel> Attributes { get; } = [];
    public Collection<SessionRefreshTokenHistoryEntityModel> RefreshTokenHistory { get; } = [];
}
