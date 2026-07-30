using FFCEI.DotNetStack.Compliance.Common.Attributes;
using FFCEI.DotNetStack.Compliance.Common.Classifications;
using FFCEI.DotNetStack.Compliance.Redactors.Security;

namespace FFCEI.DotNetStack.Compliance.Attributes.Security;

public sealed class ComplianceTagAsPasswordAttribute()
    : ReplaceRedactorAttribute<ComplianceTagAsPasswordRedactor>(SecurityClassifications.Password)
{
}
