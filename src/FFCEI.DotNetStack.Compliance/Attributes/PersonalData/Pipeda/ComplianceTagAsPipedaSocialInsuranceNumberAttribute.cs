using FFCEI.DotNetStack.Compliance.Common.Attributes;
using FFCEI.DotNetStack.Compliance.Common.Classifications;
using FFCEI.DotNetStack.Compliance.Redactors.PersonalData.Pipeda;

namespace FFCEI.DotNetStack.Compliance.Attributes.PersonalData.Pipeda;

public sealed class ComplianceTagAsPipedaSocialInsuranceNumberAttribute()
    : MaskingRedactorAttribute<ComplianceTagAsPipedaSocialInsuranceNumberRedactor>(PersonalPipedaDataClassifications.SocialInsuranceNumber)
{
}
