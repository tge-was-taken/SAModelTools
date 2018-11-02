namespace SAModelLibrary.GeometryFormats.GC
{
    /// <summary>
    /// Represents the primitive types supported by GX.
    /// </summary>
    public enum GXPrimitive : byte
    {
        /// <summary>
        /// Original name: GX_POINTS.
        /// </summary>
        Points = 0xb8,

        /// <summary>
        /// Original name: GX_LINES.
        /// </summary>
        Lines = 0xa8,

        /// <summary>
        /// Original name: GX_LINESTRIP.
        /// </summary>
        LineStrip = 0xb0,

        /// <summary>
        /// Original name: GX_TRIANGLES.
        /// </summary>
        Triangles = 0x90,

        /// <summary>
        /// Original name: GX_TRIANGLESTRIP.
        /// </summary>
        TriangleStrip = 0x98,

        /// <summary>
        /// Original name: GX_TRIANGLEFAN.
        /// </summary>
        TriangleFan = 0xa0,

        /// <summary>
        /// Original name: GX_QUADS.
        /// </summary>
        Quads = 0x80
    }
}