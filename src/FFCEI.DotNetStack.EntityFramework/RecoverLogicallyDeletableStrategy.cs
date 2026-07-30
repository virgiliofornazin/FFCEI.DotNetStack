namespace FFCEI.DotNetStack.EntityFramework;

internal enum RecoverLogicallyDeletableStrategy
{
    Unspecified = 0,
    RecoverExisting = 1,
    DetachExistingAttachNew = 2
}

