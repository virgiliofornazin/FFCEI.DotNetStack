using FFCEI.DotNetStack.Compliance.Common.Attributes;
using FFCEI.DotNetStack.Compliance.Common.Classifications;
using FFCEI.DotNetStack.Compliance.Redactors.PersonalData;

namespace FFCEI.DotNetStack.Compliance.Attributes.PersonalData;

public sealed class ComplianceTagAsPassportNumberAttribute()
    : MaskingRedactorAttribute<ComplianceTagAsPassportNumberRedactor>(PersonalDataClassifications.PassportNumber)
{
}
