using FFCEI.DotNetStack.Compliance.Common.Attributes;
using FFCEI.DotNetStack.Compliance.Common.Classifications;
using FFCEI.DotNetStack.Compliance.Redactors.PersonalData.Ccpa;

namespace FFCEI.DotNetStack.Compliance.Attributes.PersonalData.Ccpa;

public sealed class ComplianceTagAsCcpaSocialSecurityNumberAttribute()
    : MaskingRedactorAttribute<ComplianceTagAsCcpaSocialSecurityNumberRedactor>(PersonalCcpaDataClassifications.SocialSecurityNumber)
{
}
