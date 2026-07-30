using FFCEI.DotNetStack.Compliance.Common.Attributes;
using FFCEI.DotNetStack.Compliance.Common.Classifications;

namespace FFCEI.DotNetStack.Compliance.Attributes.PrivacyInformation.Iso27701;

public sealed class ComplianceTagAsIso27701PiiPrincipalIdentifierAttribute()
    : ClassificationTagAttribute(PrivacyInformationIso27701Classifications.PiiPrincipalIdentifier)
{
}
