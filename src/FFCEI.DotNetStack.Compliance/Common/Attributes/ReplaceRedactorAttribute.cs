using FFCEI.DotNetStack.Compliance.Common.Redactors;
using Microsoft.Extensions.Compliance.Classification;

namespace FFCEI.DotNetStack.Compliance.Common.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public abstract class ReplaceRedactorAttribute<TRedactor>(DataClassification classification)
    : RedactorAttribute(classification, typeof(TRedactor)) where TRedactor : ReplaceRedactor
{
}
