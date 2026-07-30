using FFCEI.DotNetStack.Compliance.Common.Attributes;
using FFCEI.DotNetStack.Compliance.Common.Classifications;
using FFCEI.DotNetStack.Compliance.Redactors.SensitiveData.Lgpd;

namespace FFCEI.DotNetStack.Compliance.Attributes.SensitiveData.Lgpd;

public sealed class ComplianceTagAsLgpdOrganizationMembershipAttribute()
    : ReplaceRedactorAttribute<ComplianceTagAsLgpdOrganizationMembershipRedactor>(SensitiveLgpdDataClassifications.OrganizationMembership)
{
}
