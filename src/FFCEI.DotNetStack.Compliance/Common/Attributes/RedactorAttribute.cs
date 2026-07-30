using Microsoft.Extensions.Compliance.Classification;

namespace FFCEI.DotNetStack.Compliance.Common.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public abstract class RedactorAttribute(DataClassification classification, Type redactorType)
    : DataClassificationAttribute(classification)
{
    public Type RedactorType { get; private set; } = redactorType;
}
