namespace SAModelLibrary
{
    /// <summary>
    /// Geometry formats.
    /// </summary>
    public enum GeometryFormat
    {
        /// <summary>
        /// Unknown geometry type.
        /// </summary>
        Unknown,

        /// <summary>
        /// Abstract geometry type. Used for editing and cross compiling between different concrete formats.
        /// </summary>
        Abstract,

        /// <summary>
        /// Basic geometry format. Used in older games utilizing Ninja, such as Sonic Adventure.
        /// </summary>
        Basic,

        /// <summary>
        /// Used in Sonic Adventure DX. Same as <see cref="Basic"/>, but with minor format layout differences.
        /// </summary>
        BasicDX,

        /// <summary>
        /// Chunk geometry format. Used in newer games utilizing Ninja, such as Sonic Adventure 2.
        /// </summary>
        Chunk,

        /// <summary>
        /// GC geometry format. Optimized for GC/Wii. Used in SA2B, SA2PC.
        /// </summary>
        GC
    }
}