using System;

namespace SAModelLibrary
{
    [Flags]
    public enum FilterMode
    {
        Point     = 0,
        Bilinear  = 1 << 0,
        Trilinear = 1 << 1,
        Blend     = Trilinear | Bilinear
    }
}