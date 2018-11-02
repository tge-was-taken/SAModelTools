using System.Numerics;
using SAModelLibrary.Maths;

namespace SAModelLibrary.GeometryFormats.Chunk
{
    /// <summary>
    /// Vertex containing only position data, but stored as a vector 4 for optimization.
    /// </summary>
    public struct VertexSH
    {
        /// <summary>
        /// The vertex position vector. W should be 1.
        /// </summary>
        public Vector4 Position;

        public override string ToString()
        {
            return Position.ToString();
        }
    }

    /// <summary>
    /// Vertex containing position and normal data, but with both stored as a vector 4 for optimization.
    /// </summary>
    public struct VertexNSH
    {
        /// <summary>
        /// The vertex position vector. W should be 1.
        /// </summary>
        public Vector4 Position;

        /// <summary>
        /// The vertex normal vector. W should be 0.
        /// </summary>
        public Vector4 Normal;

        public override string ToString()
        {
            return $"{Position} {Normal}";
        }
    }

    /// <summary>
    /// Vertex containing only position data.
    /// </summary>
    public struct VertexXYZ
    {
        /// <summary>
        /// The vertex position vector.
        /// </summary>
        public Vector3 Position;

        public override string ToString()
        {
            return $"{Position}";
        }
    }

    /// <summary>
    /// Vertex containing position and diffuse color.
    /// </summary>
    public struct VertexD8888
    {
        /// <summary>
        /// The vertex position vector.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex diffuse color in ARGB format.
        /// </summary>
        public Color Diffuse;

        public override string ToString()
        {
            return $"{Position} {Diffuse}";
        }
    }

    /// <summary>
    /// Vertex containing position and user specified flags.
    /// </summary>
    public struct VertexUF
    {
        /// <summary>
        /// The vertex position vector.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The user flags associated with the vertex.
        /// </summary>
        public uint UserFlags;

        public override string ToString()
        {
            return $"{Position} {UserFlags:X8}";
        }
    }

    /// <summary>
    /// Vertex containing position and ninja extension flags.
    /// </summary>
    public struct VertexNF
    {
        /// <summary>
        /// The vertex position vector.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The ninja extension flags associated with the vertex.
        /// </summary>
        public uint NinjaFlags;

        public override string ToString()
        {
            return $"{Position} {NinjaFlags:X8}";
        }
    }

    /// <summary>
    /// Vertex containing position, diffuse and specular color.
    /// </summary>
    public struct VertexD565S565
    {
        /// <summary>
        /// The vertex position vector.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex diffuse color in R5G6B5 format.
        /// </summary>
        public ushort Diffuse;

        /// <summary>
        /// The vertex specular color in R5G6B5 format.
        /// </summary>
        public ushort Specular;

        public override string ToString()
        {
            return $"{Position} {Diffuse:X4} {Specular:X4}";
        }
    }

    /// <summary>
    /// Vertex containing position, diffuse and specular color.
    /// </summary>
    public struct VertexD4444S565
    {
        /// <summary>
        /// The vertex position vector.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex diffuse color in A4R4G4B4 format.
        /// </summary>
        public ushort Diffuse;

        /// <summary>
        /// The vertex specular color in R5G6B5 format.
        /// </summary>
        public ushort Specular;

        public override string ToString()
        {
            return $"{Position} {Diffuse:X4} {Specular:X4}";
        }
    }

    /// <summary>
    /// Vertex containing position, diffuse and specular color.
    /// </summary>
    public struct VertexD16S16
    {
        /// <summary>
        /// The vertex position vector.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex diffuse color in D16 format.
        /// </summary>
        public ushort Diffuse;

        /// <summary>
        /// The vertex specular color in D16 format.
        /// </summary>
        public ushort Specular;

        public override string ToString()
        {
            return $"{Position} {Diffuse:X4} {Specular:X4}";
        }
    }

    /// <summary>
    /// Vertex containing position and normals.
    /// </summary>
    public struct VertexN
    {
        /// <summary>
        /// The vertex position vector.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex normal vector.
        /// </summary>
        public Vector3 Normal;

        public override string ToString()
        {
            return $"{Position} {Normal}";
        }
    }

    /// <summary>
    /// Vertex containing position, normals and diffuse color.
    /// </summary>
    public struct VertexND8888
    {
        /// <summary>
        /// The vertex position vector.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex normal vector.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// The vertex diffuse color in ARGB format.
        /// </summary>
        public Color Diffuse;

        public override string ToString()
        {
            return $"{Position} {Normal} {Diffuse}";
        }
    }


    /// <summary>
    /// Vertex containing position, normals and user specified flags.
    /// </summary>
    public struct VertexNUF
    {
        /// <summary>
        /// The vertex position vector.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex normal vector.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// The user specified flags associated with the vertex.
        /// </summary>
        public uint UserFlags;

        public override string ToString()
        {
            return $"{Position} {Normal} {UserFlags:X8}";
        }
    }

    /// <summary>
    /// Vertex containing position, normals and ninja extension flags.
    /// </summary>
    public struct VertexNNF
    {
        /// <summary>
        /// The vertex position vector.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex normal vector.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// The ninja extension flags associated with the vertex.
        /// </summary>
        public uint NinjaFlags;

        public override string ToString()
        {
            return $"{Position} {Normal} {NinjaFlags:X8}";
        }
    }


    /// <summary>
    /// Vertex containing position, normals, diffuse and specular color.
    /// </summary>
    public struct VertexND565S565
    {
        /// <summary>
        /// The vertex position vector.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex normal vector.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// The vertex diffuse color in R5G6B5 format.
        /// </summary>
        public ushort  Diffuse;

        /// <summary>
        /// The vertex specular color in R5G6B5 format.
        /// </summary>
        public ushort  Specular;

        public override string ToString()
        {
            return $"{Position} {Normal} {Diffuse:X4} {Specular:X4}";
        }
    }

    /// <summary>
    /// Vertex containing position, normals, diffuse and specular color.
    /// </summary>
    public struct VertexND4444S565
    {
        /// <summary>
        /// The vertex position vector.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex normal vector.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// The vertex diffuse color in A4R4G4B4 format.
        /// </summary>
        public ushort  Diffuse;

        /// <summary>
        /// The vertex specular color in R5G6B5 format.
        /// </summary>
        public ushort  Specular;

        public override string ToString()
        {
            return $"{Position} {Normal} {Diffuse:X4} {Specular:X4}";
        }
    }

    /// <summary>
    /// Vertex containing position, normals, diffuse and specular color.
    /// </summary>
    public struct VertexND16S16
    {
        /// <summary>
        /// The vertex position vector.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex normal vector.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// The vertex diffuse color in D16 format.
        /// </summary>
        public ushort Diffuse;

        /// <summary>
        /// The vertex specular color in D16 format.
        /// </summary>
        public ushort Specular;

        public override string ToString()
        {
            return $"{Position} {Normal} {Diffuse:X4} {Specular:X4}";
        }
    }

    /// <summary>
    /// Vertex containing position and compressed normals.
    /// </summary>
    public struct VertexN32
    {
        /// <summary>
        /// The vertex position vector.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex normal vector, in X10-Y10-Z10 format.
        /// </summary>
        public uint Normal;

        public override string ToString()
        {
            return $"{Position} {Normal:X8}";
        }
    }

    /// <summary>
    /// Vertex containing position, compressed normals and diffuse color.
    /// </summary>
    public struct VertexN32D8888
    {
        /// <summary>
        /// The vertex position vector.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex normal vector, in X10-Y10-Z10 format.
        /// </summary>
        public uint Normal;

        /// <summary>
        /// The vertex diffuse color in ARGB format.
        /// </summary>
        public Color Diffuse;

        public override string ToString()
        {
            return $"{Position} {Normal:X8} {Diffuse}";
        }
    }

    /// <summary>
    /// Vertex containing position, compressed normals and user specified flags.
    /// </summary>
    public struct VertexN32UF
    {
        /// <summary>
        /// The vertex position vector.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// The vertex normal vector, in X10-Y10-Z10 format.
        /// </summary>
        public uint Normal;

        /// <summary>
        /// The user specified flags associated with the vertex.
        /// </summary>
        public uint UserFlags;

        public override string ToString()
        {
            return $"{Position} {Normal:X8} {UserFlags:X8}";
        }
    }
}
