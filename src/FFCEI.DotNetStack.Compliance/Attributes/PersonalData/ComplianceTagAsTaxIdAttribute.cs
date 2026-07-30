using FFCEI.DotNetStack.Compliance.Common.Attributes;
using FFCEI.DotNetStack.Compliance.Common.Classifications;
using FFCEI.DotNetStack.Compliance.Redactors.PersonalData;

namespace FFCEI.DotNetStack.Compliance.Attributes.PersonalData;

#pragma warning disable CA1813 // Avoid unsealed attributes
public class ComplianceTagAsTaxIdAttribute()
#pragma warning restore CA1813 // Avoid unsealed attributes
    : MaskingRedactorAttribute<ComplianceTagAsTaxIdRedactor>(PersonalDataClassifications.TaxId)
{
}
