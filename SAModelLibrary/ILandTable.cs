using System.Collections.Generic;
using SAModelLibrary.IO;

namespace SAModelLibrary
{
    /// <summary>
    /// Common interface for all land table formats.
    /// </summary>
    public interface ILandTable : ISerializableObject
    {
        /// <summary>
        /// Gets the number of models in the land table.
        /// </summary>
        short ModelCount { get; }

        /// <summary>
        /// Gets or sets the model cull range.
        /// </summary>
        float CullRange { get; set; }

        /// <summary>
        /// Gets the models contained within the land table.
        /// </summary>
        IEnumerable<ILandModel> Models { get; }

        /// <summary>
        /// Gets or sets the texture package file name.
        /// </summary>
        string TexturePakFileName { get; set; }

        /// <summary>
        /// Gets or sets the list of texture references.
        /// </summary>
        TextureReferenceList Textures { get; set; }
    }
}