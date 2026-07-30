using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace FFCEI.DotNetStack.Models.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
public sealed class ForeignKeyReferenceAttribute
    : Attribute
{
    public string EntityPropertyName { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string? _idPropertyName;

    public string IdPropertyName
    {
        get => _idPropertyName ?? string.Empty;
        set => _idPropertyName = value;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IdPropertyNameHasValue
        => _idPropertyName != null;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string? _reverseNavigationPropertyName;

    public string ReverseNavigationPropertyName
    {
        get => _reverseNavigationPropertyName ?? string.Empty;
        set => _reverseNavigationPropertyName = value;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool ReverseNavigationPropertyNameHasValue
        => _reverseNavigationPropertyName != null;


    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string? _constraintName;

    public string ConstraintName
    {
        get => _constraintName ?? string.Empty;
        set => _constraintName = value;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool ConstraintNameHasValue
        => _constraintName != null;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool? _isRequired;

    public bool IsRequired
    {
        get => _isRequired ?? false;
        set => _isRequired = value;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsRequiredHasValue
        => _isRequired.HasValue;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private DeleteBehavior? _deleteBehavior;

    public DeleteBehavior DeleteBehavior
    {
        get => _deleteBehavior ?? default;
        set => _deleteBehavior = value;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool DeleteBehaviorHasValue
        => _deleteBehavior.HasValue;

    public ForeignKeyReferenceAttribute(string entityPropertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityPropertyName, nameof(entityPropertyName));

        EntityPropertyName = entityPropertyName;
    }
}
