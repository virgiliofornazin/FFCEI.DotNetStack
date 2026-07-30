using Microsoft.Extensions.Compliance.Classification;

namespace FFCEI.DotNetStack.Compliance.Common.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public abstract class ClassificationTagAttribute(DataClassification classification)
    : DataClassificationAttribute(classification)
{
}
