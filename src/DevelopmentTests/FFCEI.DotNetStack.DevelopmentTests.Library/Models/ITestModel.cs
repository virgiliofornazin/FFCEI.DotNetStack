using FFCEI.DotNetStack.Models.Interfaces;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.Models;

public interface ITestModel
    : IDatabaseIdEntityModel, IUniversalUniqueIdEntityModel, ILogicallyDeletableEntityModel, ITimestampTrackedEntityModel
{
    bool BoolProperty { get; set; }
    sbyte Int8Property { get; set; }
    byte UInt8Property { get; set; }
    short Int16Property { get; set; }
    ushort UInt16Property { get; set; }
    int Int32Property { get; set; }
    uint UInt32Property { get; set; }
    long Int64Property { get; set; }
    ulong UInt64Property { get; set; }
    float FloatProperty { get; set; }
    double DoubleProperty { get; set; }
    decimal DecimalProperty { get; set; }
    DateOnly DateOnlyProperty { get; set; }
    TimeOnly TimeOnlyProperty { get; set; }
    DateTime DateTimeProperty { get; set; }
    DateTimeOffset DateTimeOffsetProperty { get; set; }
    TimeSpan TimeSpanProperty { get; set; }
    Guid GuidProperty { get; set; }
    string StringProperty { get; set; }
#pragma warning disable CA1819 // Properties should not return arrays
    char[] CharArrayProperty { get; set; }
    byte[] ByteArrayProperty { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
}
