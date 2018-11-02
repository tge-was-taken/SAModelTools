using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAModelLibrary
{
    /// <summary>
    /// Represents various primitive types that can be used by a mesh.
    /// </summary>
    public enum PrimitiveType
    {
        /// <summary>
        /// Triangle primitive type. 3 vertices per triangle.
        /// </summary>
        Triangles,

        /// <summary>
        /// Quad primitive type. 4 vertices per quad.
        /// </summary>
        Quads,

        /// <summary>
        /// NGon primitive type. N vertices per NGon
        /// </summary>
        NGons,

        /// <summary>
        /// Triangle strip primitive types. 2 vertices + N-2 vertices per triangle.
        /// </summary>
        Strips
    }
}
