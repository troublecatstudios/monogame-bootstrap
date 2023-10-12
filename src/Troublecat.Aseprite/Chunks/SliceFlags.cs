using System;

namespace Troublecat.Aseprite.Chunks {
    [Flags]
    public enum SliceFlags : ushort {
        IsNinePatch = 1,
        HasPivot = 2
    }
}
