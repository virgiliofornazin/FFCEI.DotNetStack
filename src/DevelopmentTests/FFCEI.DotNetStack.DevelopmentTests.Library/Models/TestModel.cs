using FFCEI.DotNetStack.Models;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.Models;

public class TestModel
    : UniqueIdExternalUniqueIdLogicallyDeletableTimestampTrackedEntityModel
#if TEST_EFCORE_USE_INTERFACES
    , ITestModel
#endif
{
    public bool BoolProperty { get; set; }
    public sbyte Int8Property { get; set; }
    public byte UInt8Property { get; set; }
    public short Int16Property { get; set; }
    public ushort UInt16Property { get; set; }
    public int Int32Property { get; set; }
    public uint UInt32Property { get; set; }
    public long Int64Property { get; set; }
    public ulong UInt64Property { get; set; }
    public float FloatProperty { get; set; }
    public double DoubleProperty { get; set; }
    public decimal DecimalProperty { get; set; }
    public DateOnly DateOnlyProperty { get; set; }
    public TimeOnly TimeOnlyProperty { get; set; }
    public DateTime DateTimeProperty { get; set; }
    public DateTimeOffset DateTimeOffsetProperty { get; set; }
    public TimeSpan TimeSpanProperty { get; set; }
    public Guid GuidProperty { get; set; }
    public string StringProperty { get; set; } = string.Empty;
#pragma warning disable CA1819 // Properties should not return arrays
    public char[] CharArrayProperty { get; set; } = [];
    public byte[] ByteArrayProperty { get; set; } = [];
#pragma warning restore CA1819 // Properties should not return arrays
}
