using SAModelLibrary.IO;

namespace SAModelLibrary.GeometryFormats.Basic
{
    /// <summary>
    /// Common interface for all basic primitive types.
    /// </summary>
    public interface IPrimitive : ISerializableObject
    {
        /// <summary>
        /// Gets the primitive type of the primitive.
        /// </summary>
        PrimitiveType PrimitiveType { get; }
    }
}