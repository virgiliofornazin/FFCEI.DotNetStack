using FFCEI.DotNetStack.Compliance.Common.Attributes;
using FFCEI.DotNetStack.Compliance.Common.Classifications;
using FFCEI.DotNetStack.Compliance.Redactors.SensitiveData;

namespace FFCEI.DotNetStack.Compliance.Attributes.SensitiveData;

public sealed class ComplianceTagAsCriminalRecordAttribute()
    : ReplaceRedactorAttribute<ComplianceTagAsCriminalRecordRedactor>(SensitiveDataClassifications.CriminalRecord)
{
}
