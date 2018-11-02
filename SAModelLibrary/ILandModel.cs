using SAModelLibrary.IO;

namespace SAModelLibrary
{
    /// <summary>
    /// Provides a common interface for land models.
    /// </summary>
    public interface ILandModel : ISerializableObject
    {
        /// <summary>
        /// Gets or sets the bounding sphere of this land model.
        /// </summary>
        BoundingSphere Bounds { get; set; }

        /// <summary>
        /// Gets or sets the root node of the land models' model hierarchy.
        /// </summary>
        Node RootNode { get; set; }

        /// <summary>
        /// Gets or sets the flags for this land model.
        /// </summary>
        SurfaceFlags Flags { get; set; }
    }
}