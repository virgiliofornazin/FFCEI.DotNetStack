using FFCEI.DotNetStack.Compliance.Common.Attributes;
using FFCEI.DotNetStack.Compliance.Common.Classifications;
using FFCEI.DotNetStack.Compliance.Redactors.PersonalData.Appi;

namespace FFCEI.DotNetStack.Compliance.Attributes.PersonalData.Appi;

public sealed class ComplianceTagAsAppiPensionNumberAttribute()
    : MaskingRedactorAttribute<ComplianceTagAsAppiPensionNumberRedactor>(PersonalAppiDataClassifications.PensionNumber)
{
}
