using FFCEI.DotNetStack.Compliance.Common.Attributes;
using FFCEI.DotNetStack.Compliance.Common.Classifications;
using FFCEI.DotNetStack.Compliance.Redactors.SensitiveData.Appi;

namespace FFCEI.DotNetStack.Compliance.Attributes.SensitiveData.Appi;

public sealed class ComplianceTagAsAppiCrimeVictimHistoryAttribute()
    : ReplaceRedactorAttribute<ComplianceTagAsAppiCrimeVictimHistoryRedactor>(SensitiveAppiDataClassifications.CrimeVictimHistory)
{
}
