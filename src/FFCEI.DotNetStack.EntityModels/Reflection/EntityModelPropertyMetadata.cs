using FFCEI.DotNetStack.Framework;
using System.Diagnostics;
using System.Reflection;

namespace FFCEI.DotNetStack.EntityModels.Reflection;

[DebuggerDisplay("Property: {Name,nq} [type: {CSharpTypeName,nq}]")]
public sealed class EntityModelPropertyMetadata
{
    internal EntityModelPropertyMetadata(PropertyInfo propertyInfo)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        PropertyInfo = propertyInfo;
    }

    public string Name => PropertyInfo.Name;
    public PropertyInfo PropertyInfo { get; private set; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public string CSharpTypeName => PropertyInfo?.PropertyType != null ? PropertyInfo.PropertyType.CSharpTypeName() : "null";
}
