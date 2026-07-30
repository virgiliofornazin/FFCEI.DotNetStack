using FFCEI.DotNetStack.Compliance.Common.Attributes;
using FFCEI.DotNetStack.Compliance.Common.Classifications;

namespace FFCEI.DotNetStack.Compliance.Attributes.InformationSecurity.Iso27001;

public sealed class ComplianceTagAsIso27001InternalInformationAttribute()
    : ClassificationTagAttribute(InformationSecurityIso27001Classifications.InternalInformation)
{
}
