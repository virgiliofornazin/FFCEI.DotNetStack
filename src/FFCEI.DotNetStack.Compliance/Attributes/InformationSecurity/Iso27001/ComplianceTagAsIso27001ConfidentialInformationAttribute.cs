using FFCEI.DotNetStack.Compliance.Common.Attributes;
using FFCEI.DotNetStack.Compliance.Common.Classifications;

namespace FFCEI.DotNetStack.Compliance.Attributes.InformationSecurity.Iso27001;

public sealed class ComplianceTagAsIso27001ConfidentialInformationAttribute()
    : ClassificationTagAttribute(InformationSecurityIso27001Classifications.ConfidentialInformation)
{
}
