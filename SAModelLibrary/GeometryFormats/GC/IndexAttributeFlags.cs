using System;

namespace SAModelLibrary.GeometryFormats.GC
{
    /// <summary>
    /// Represents all applicable flags pertaining to index buffer attributes.
    /// </summary>
    [Flags]
    public enum IndexAttributeFlags : ushort
    {
        Bit0          = 1 << 0, // Unused
        Bit1          = 1 << 1, // Unused
        Position16BitIndex = 1 << 2,
        HasPosition   = 1 << 3,
        Normal16BitIndex   = 1 << 4,
        HasNormal     = 1 << 5,
        Color16BitIndex    = 1 << 6,
        HasColor      = 1 << 7,
        Bit8          = 1 << 8, // Unused
        Bit9          = 1 << 9, // Unused
        UV16BitIndex       = 1 << 10,
        HasUV         = 1 << 11,
        Bit12         = 1 << 12, // Unused
        Bit13         = 1 << 13, // Unused
        Bit14         = 1 << 14, // Unused
        Bit15         = 1 << 15, // Unused
    }
}