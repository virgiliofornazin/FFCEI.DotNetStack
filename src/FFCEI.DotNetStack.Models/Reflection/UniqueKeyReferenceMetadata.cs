using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FFCEI.DotNetStack.Models.Reflection;

[DebuggerDisplay("UniqueKeyReference: [Properties: {Properties?.Count}]")]
public class UniqueKeyReferenceMetadata
{
    internal UniqueKeyReferenceMetadata(IndexAttribute attribute, IReadOnlyCollection<ModelPropertyMetadata> properties)
    {
        ArgumentNullException.ThrowIfNull(attribute, nameof(attribute));
        ArgumentNullException.ThrowIfNull(properties, nameof(properties));

        var propertiesList = new List<UniqueKeyReferenceColumnMetadata>();

        for (int propertyIndex = 0; propertyIndex < attribute.PropertyNames.Count; ++propertyIndex)
        {
            var propertyName = attribute.PropertyNames[propertyIndex];
            var propertyMetadata = properties.FirstOrDefault(property => property.Name == propertyName);

            if (propertyMetadata == null)
            {
                throw new InvalidOperationException($"Index property '{propertyName}' not found...");
            }

            var isDescending = attribute.AllDescending ? true : attribute.IsDescending?[propertyIndex] ?? false;

            propertiesList.Add(new(propertyMetadata, isDescending));
        }

        Properties = propertiesList.AsReadOnly();
    }

    public IReadOnlyCollection<UniqueKeyReferenceColumnMetadata> Properties { get; private set; }
}
