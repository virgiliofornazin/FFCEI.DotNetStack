using FFCEI.DotNetStack.Compliance.Common.Attributes;
using FFCEI.DotNetStack.Compliance.Common.Classifications;
using FFCEI.DotNetStack.Compliance.Redactors.PersonalData.Lgpd;

namespace FFCEI.DotNetStack.Compliance.Attributes.PersonalData.Lgpd;

public sealed class ComplianceTagAsLgpdRgAttribute()
    : MaskingRedactorAttribute<ComplianceTagAsLgpdRgRedactor>(PersonalLgpdDataClassifications.Rg)
{
}
