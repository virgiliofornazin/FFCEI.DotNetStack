using FFCEI.DotNetStack.Compliance.Common.Attributes;
using FFCEI.DotNetStack.Compliance.Common.Classifications;
using FFCEI.DotNetStack.Compliance.Redactors.SensitiveData.Ccpa;

namespace FFCEI.DotNetStack.Compliance.Attributes.SensitiveData.Ccpa;

public sealed class ComplianceTagAsCcpaCommunicationsContentAttribute()
    : ReplaceRedactorAttribute<ComplianceTagAsCcpaCommunicationsContentRedactor>(SensitiveCcpaDataClassifications.CommunicationsContent)
{
}
