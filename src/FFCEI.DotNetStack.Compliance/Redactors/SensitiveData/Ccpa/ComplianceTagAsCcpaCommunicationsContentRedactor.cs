using FFCEI.DotNetStack.Compliance.Common.Redactors;

namespace FFCEI.DotNetStack.Compliance.Redactors.SensitiveData.Ccpa;

public sealed class ComplianceTagAsCcpaCommunicationsContentRedactor()
    : ReplaceRedactor("***")
{
}
