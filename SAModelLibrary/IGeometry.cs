using SAModelLibrary.IO;

namespace SAModelLibrary
{
    /// <summary>
    /// Interface for all geometry types.
    /// </summary>
    public interface IGeometry : ISerializableObject
    {
        /// <summary>
        /// Gets the format of this geometry.
        /// </summary>
        GeometryFormat Format { get; }
    }
}