using FFCEI.DotNetStack.AccessControl.Interfaces;
using FFCEI.DotNetStack.EntityModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FFCEI.DotNetStack.AccessControl.SessionManagement.EntityModels;

[Index(nameof(TenantId), nameof(Token), IsUnique = true)]
#pragma warning disable CA1724 // Type names should not match namespaces
public class Session
#pragma warning restore CA1724 // Type names should not match namespaces
    : DatabaseUniqueIdUniversalUniqueIdLogicallyDeletableTimestampTrackedEntityModel, ISessionExpiration
{
    public required string TenantId { get; set; }
    public required string AccountId { get; set; }
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public long? DeviceIdentificationId { get; set; }
    public DeviceIdentification? DeviceIdentification { get; set; }
    public TimeSpan? InactivityExpirationInterval { get; set; }
    public DateTimeOffset? ExpiresTimestamp { get; set; }
    public DateTimeOffset? FinalizedTimestamp
    {
        get => LogicallyDeletedTimestamp;
        set => LogicallyDeletedTimestamp = value;
    }
    public Collection<SessionAttribute> Attributes { get; } = [];
    public Collection<SessionRefreshTokenHistory> RefreshTokenHistory { get; } = [];
}
