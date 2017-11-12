using System;

namespace FileExplorerUniversal.Control.Interop
{
    [Flags]
    public enum ExtensionRestrictions
    {
        None = 1,
        InheritManifest = 2,
        Custom = 4
    }
}
