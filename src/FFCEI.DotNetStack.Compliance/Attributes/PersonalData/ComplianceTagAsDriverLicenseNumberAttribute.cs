using FFCEI.DotNetStack.Compliance.Common.Attributes;
using FFCEI.DotNetStack.Compliance.Common.Classifications;
using FFCEI.DotNetStack.Compliance.Redactors.PersonalData;

namespace FFCEI.DotNetStack.Compliance.Attributes.PersonalData;

public sealed class ComplianceTagAsDriverLicenseNumberAttribute()
    : MaskingRedactorAttribute<ComplianceTagAsDriverLicenseNumberRedactor>(PersonalDataClassifications.DriverLicenseNumber)
{
}
