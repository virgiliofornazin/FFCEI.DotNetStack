using Microsoft.Extensions.Compliance.Classification;

namespace FFCEI.DotNetStack.Compliance.Common.Classifications;

public static class PersonalDataClassifications
{
    private const string TaxonomyName = "PersonalData";

    public static DataClassification Address { get; } = new(TaxonomyName, nameof(Address));
    public static DataClassification DateOfBirth { get; } = new(TaxonomyName, nameof(DateOfBirth));
    public static DataClassification DeviceIdentifier { get; } = new(TaxonomyName, nameof(DeviceIdentifier));
    public static DataClassification DriverLicenseNumber { get; } = new(TaxonomyName, nameof(DriverLicenseNumber));
    public static DataClassification Email { get; } = new(TaxonomyName, nameof(Email));
    public static DataClassification FullName { get; } = new(TaxonomyName, nameof(FullName));
    public static DataClassification IpAddress { get; } = new(TaxonomyName, nameof(IpAddress));
    public static DataClassification NationalIdentifier { get; } = new(TaxonomyName, nameof(NationalIdentifier));
    public static DataClassification PassportNumber { get; } = new(TaxonomyName, nameof(PassportNumber));
    public static DataClassification Phone { get; } = new(TaxonomyName, nameof(Phone));
    public static DataClassification TaxId { get; } = new(TaxonomyName, nameof(TaxId));
}
