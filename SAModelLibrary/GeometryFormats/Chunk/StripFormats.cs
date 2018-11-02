using System.Numerics;
using SAModelLibrary.Maths;

// ReSharper disable InconsistentNaming
#pragma warning disable S101 // Types should be named in camel case
#pragma warning disable S1104 // Fields should not have public accessibility

namespace SAModelLibrary.GeometryFormats.Chunk
{
    /// <summary>
    /// Format 1.
    /// </summary>
    public struct StripIndex
    {
        /// <summary>
        /// Vertex index.
        /// </summary>
        public ushort Index;

        public StripIndex(ushort index)
        {
            Index = index;
        }
    }

    /// <summary>
    /// Format 1.
    /// </summary>
    public struct StripIndex2
    {
        /// <summary>
        /// Vertex index.
        /// </summary>
        public ushort Index;
    }

    /// <summary>
    /// Format 2.
    /// </summary>
    public struct StripIndexUVN
    {
        /// <summary>
        /// Vertex index.
        /// </summary>
        public ushort Index;

        /// Texture UV coordinate. Range 0-255.
        /// </summary>
        public Vector2<short> UV;

        public StripIndexUVN( ushort index, Vector2 uv )
        {
            Index = index;
            UV    = UVCodec.Encode255( uv );
        }
        public StripIndexUVN( ushort index, Vector2<short> uv )
        {
            Index = index;
            UV = uv;
        }
    }

    /// <summary>
    /// Format 2.
    /// </summary>
    public struct StripIndexUVH
    {
        /// <summary>
        /// Vertex index.
        /// </summary>
        public ushort Index;

        /// Texture UV coordinate. Range 0-1023.
        /// </summary>
        public Vector2<short> UV;

        public StripIndexUVH(ushort index, Vector2 uv)
        {
            Index = index;
            UV = UVCodec.Encode1023( uv );
        }
    }

    /// <summary>
    /// Format 3.
    /// </summary>
    public struct StripIndexVN
    {
        /// <summary>
        /// Vertex index.
        /// </summary>
        public ushort Index;

        /// <summary>
        /// Face normal vector.
        /// </summary>
        public Vector3<short> Normal;
    }

    /// <summary>
    /// Format 4.
    /// </summary>
    public struct StripIndexUVNVN
    {
        /// <summary>
        /// Vertex index.
        /// </summary>
        public ushort Index;

        /// Texture UV coordinates. Range 0-255.
        /// </summary>
        public Vector2<short> UV;

        /// <summary>
        /// Face normal vector.
        /// </summary>
        public Vector3<short> Normal;
    }

    /// <summary>
    /// Format 4.
    /// </summary>
    public struct StripIndexUVHVN
    {
        /// <summary>
        /// Vertex index.
        /// </summary>
        public ushort Index;

        /// Texture UV coordinates. Range 0-1023.
        /// </summary>
        public Vector2<short> UV;

        /// <summary>
        /// Face normal vector.
        /// </summary>
        public Vector3<short> Normal;
    }

    /// <summary>
    /// Format 5.
    /// </summary>
    public struct StripIndexD8
    {
        /// <summary>
        /// Vertex index.
        /// </summary>
        public ushort Index;

        /// <summary>
        /// Diffuse color.
        /// </summary>
        public Color Color;
    }

    /// <summary>
    /// Format 6.
    /// </summary>
    public struct StripIndexUVND8
    {
        /// <summary>
        /// Vertex index.
        /// </summary>
        public ushort Index;

        /// Texture UV coordinates. Range 0-255.
        /// </summary>
        public Vector2<short> UV;

        /// <summary>
        /// Diffuse color.
        /// </summary>
        public Color Color;
    }

    /// <summary>
    /// Format 6.
    /// </summary>
    public struct StripIndexUVHD8
    {
        /// <summary>
        /// Vertex index.
        /// </summary>
        public ushort Index;

        /// <summary>
        /// Texture UV coordinates. Range 0-1023.
        /// </summary>
        public Vector2<short> UV;

        /// <summary>
        /// Diffuse color.
        /// </summary>
        public Color Color;
    }

    /// <summary>
    /// Format 7.
    /// </summary>
    public struct StripIndexUVN2
    {
        /// <summary>
        /// Vertex index.
        /// </summary>
        public ushort Index;

        /// <summary>
        /// Texture UV coordinates. Range 0-255.
        /// </summary>
        public Vector2<short> UV;

        /// <summary>
        /// Second texture UV coordinates. Range 0-255.
        /// </summary>
        public Vector2<short> UV2;
    }

    /// <summary>
    /// Format 7.
    /// </summary>
    public struct StripIndexUVH2
    {
        /// <summary>
        /// Vertex index.
        /// </summary>
        public ushort Index;

        /// <summary>
        /// Texture UV coordinate. Range 0-1023.
        /// </summary>
        public Vector2<short> UV;

        /// <summary>
        /// Second texture UV coordinate. Range 0-1023.
        /// </summary>
        public Vector2<short> UV2;
    }
}

#pragma warning restore S1104 // Fields should not have public accessibility
#pragma warning restore S101 // Types should be named in camel case
