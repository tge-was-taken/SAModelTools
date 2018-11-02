using SAModelLibrary.Exceptions;
using SAModelLibrary.IO;
using SAModelLibrary.Utils;

namespace SAModelLibrary.GeometryFormats.Chunk
{
    /// <summary>
    /// Abstract class for all vertex chunk types.
    /// </summary>
    public abstract class VertexChunk : Chunk32
    {
        private static readonly BitField sWeightStatusField = new BitField( 0, 1 );
        private static readonly BitField sUnusedField       = new BitField( 2, 6 );
        private static readonly BitField sContinueField     = new BitField( 7, 7 );

        /// <summary>
        /// Gets or sets the vertex calculation 'continue' flag.
        /// </summary>
        public bool Continue { get; set; }

        /// <summary>
        /// Gets or sets the weight status for this vertex chunk.
        /// </summary>
        public WeightStatus WeightStatus { get; set; }

        /// <summary>
        /// Gets the number of vertices stored in this vertex chunk.
        /// </summary>
        public abstract int VertexCount { get; }

        /// <summary>
        /// Gets the triangle index base offset for the vertices in this chunk.
        /// </summary>
        public int BaseIndex { get; set; }

        protected override byte GetFlags()
        {
            byte flags = 0;
            sContinueField.Pack( ref flags, ( byte )( Continue ? 1u : 0u ) );
            sWeightStatusField.Pack( ref flags, ( byte )WeightStatus );
            return flags;
        }

        internal override void ReadBody( int size, byte flags, EndianBinaryReader reader )
        {
            Continue = sContinueField.Unpack( flags ) == 1;
            //Debug.Assert( sUnusedField.Unpack( flags ) == 0, "Unused bits in vertex chunk used" );
            WeightStatus = ( WeightStatus )sWeightStatusField.Unpack( flags );

            ushort vertexCount;

            if ( reader.Endianness == Endianness.Big )
            {
                vertexCount = reader.ReadUInt16();
                BaseIndex = reader.ReadUInt16();
            }
            else
            {
                BaseIndex = reader.ReadUInt16();
                vertexCount = reader.ReadUInt16();
            }

            if ( vertexCount > 4096 )
                throw new InvalidGeometryDataException();

            ReadVertices( reader, vertexCount );
        }

        internal override void WriteBody( EndianBinaryWriter writer )
        {
            if ( writer.Endianness == Endianness.Big )
            {
                writer.Write( ( ushort )VertexCount );
                writer.Write( ( ushort )BaseIndex );
            }
            else
            {
                writer.Write( ( ushort ) BaseIndex );
                writer.Write( ( ushort ) VertexCount );
            }

            WriteVertices( writer );
        }

        protected abstract void ReadVertices( EndianBinaryReader reader, int vertexCount );
        protected abstract void WriteVertices( EndianBinaryWriter writer );
    }

    /// <summary>
    /// Vertex chunk with vertices of type <typeparamref name="TVertex"/>.
    /// </summary>
    /// <typeparam name="TVertex">The vertex type associated with this chunk.</typeparam>
    public abstract class VertexChunk<TVertex> : VertexChunk
    {
        /// <summary>
        /// Gets or sets the vertices stored in this vertex chunk.
        /// </summary>
        public TVertex[] Vertices { get; set; }

        /// <inheritdoc />
        public override int VertexCount => Vertices?.Length ?? 0;

        protected override void ReadVertices( EndianBinaryReader reader, int vertexCount )
        {
            Vertices = new TVertex[vertexCount];
            for ( int i = 0; i < vertexCount; i++ )
                ReadVertex( reader, ref Vertices[i] );
        }

        protected override void WriteVertices( EndianBinaryWriter writer )
        {
            for ( int i = 0; i < Vertices.Length; i++ )
                WriteVertex( writer, ref Vertices[i] );
        }

        protected abstract void ReadVertex( EndianBinaryReader reader, ref TVertex vertex );

        protected abstract void WriteVertex( EndianBinaryWriter writer, ref TVertex vertex );
    }

    /// <inheritdoc />
    public class VertexSHChunk : VertexChunk<VertexSH>
    {
        public override ChunkType Type => ChunkType.VertexSH;

        public VertexSHChunk()
        {
        }

        public VertexSHChunk( VertexSH[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexSH vertex )
        {
            vertex.Position = reader.ReadVector4();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexSH vertex )
        {
            writer.Write( vertex.Position );
        }
    }

    /// <inheritdoc />
    public class VertexNSHChunk : VertexChunk<VertexNSH>
    {
        public override ChunkType Type => ChunkType.VertexNSH;

        public VertexNSHChunk()
        {
        }

        public VertexNSHChunk( VertexNSH[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexNSH vertex )
        {
            vertex.Position = reader.ReadVector4();
            vertex.Normal = reader.ReadVector4();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexNSH vertex )
        {
            writer.Write( vertex.Position );
            writer.Write( vertex.Normal );
        }
    }

    /// <inheritdoc />
    public class VertexXYZChunk : VertexChunk<VertexXYZ>
    {
        public override ChunkType Type => ChunkType.VertexXYZ;

        public VertexXYZChunk()
        {
        }

        public VertexXYZChunk( VertexXYZ[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexXYZ vertex )
        {
            vertex.Position = reader.ReadVector3();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexXYZ vertex )
        {
            writer.Write( vertex.Position );
        }
    }

    /// <inheritdoc />
    public class VertexD8888Chunk : VertexChunk<VertexD8888>
    {
        public override ChunkType Type => ChunkType.VertexD8888;

        public VertexD8888Chunk()
        {
        }

        public VertexD8888Chunk( VertexD8888[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexD8888 vertex )
        {
            vertex.Position = reader.ReadVector3();
            vertex.Diffuse = reader.ReadColor();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexD8888 vertex )
        {
            writer.Write( vertex.Position );
            writer.Write( vertex.Diffuse );
        }
    }

    /// <inheritdoc />
    public class VertexUF32Chunk : VertexChunk<VertexUF>
    {
        public override ChunkType Type => ChunkType.VertexUF;

        public VertexUF32Chunk()
        {
        }

        public VertexUF32Chunk( VertexUF[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexUF vertex )
        {
            vertex.Position = reader.ReadVector3();
            vertex.UserFlags = reader.ReadUInt32();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexUF vertex )
        {
            writer.Write( vertex.Position );
            writer.Write( vertex.UserFlags );
        }
    }

    /// <inheritdoc />
    public class VertexNF32Chunk : VertexChunk<VertexNF>
    {
        public override ChunkType Type => ChunkType.VertexNF;

        public VertexNF32Chunk()
        {
        }

        public VertexNF32Chunk( VertexNF[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexNF vertex )
        {
            vertex.Position = reader.ReadVector3();
            vertex.NinjaFlags = reader.ReadUInt32();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexNF vertex )
        {
            writer.Write( vertex.Position );
            writer.Write( vertex.NinjaFlags );
        }
    }

    /// <inheritdoc />
    public class VertexD565S565Chunk : VertexChunk<VertexD565S565>
    {
        public override ChunkType Type => ChunkType.VertexD565S565;
        
        public VertexD565S565Chunk()
        {
        }

        public VertexD565S565Chunk( VertexD565S565[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexD565S565 vertex )
        {
            vertex.Position = reader.ReadVector3();
            vertex.Diffuse = reader.ReadUInt16();
            vertex.Specular = reader.ReadUInt16();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexD565S565 vertex )
        {
            writer.Write( vertex.Position );
            writer.Write( vertex.Diffuse );
            writer.Write( vertex.Specular );
        }
    }

    /// <inheritdoc />
    public class VertexD4444S565Chunk : VertexChunk<VertexD4444S565>
    {
        public override ChunkType Type => ChunkType.VertexD4444S565;


        public VertexD4444S565Chunk()
        {
        }

        public VertexD4444S565Chunk( VertexD4444S565[] vertices )
        {
            Vertices = vertices;
        }


        protected override void ReadVertex( EndianBinaryReader reader, ref VertexD4444S565 vertex )
        {
            vertex.Position = reader.ReadVector3();
            vertex.Diffuse = reader.ReadUInt16();
            vertex.Specular = reader.ReadUInt16();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexD4444S565 vertex )
        {
            writer.Write( vertex.Position );
            writer.Write( vertex.Diffuse );
            writer.Write( vertex.Specular );
        }
    }

    /// <inheritdoc />
    public class VertexD16S16Chunk : VertexChunk<VertexD16S16>
    {
        public override ChunkType Type => ChunkType.VertexD16S16;

        public VertexD16S16Chunk()
        {
        }

        public VertexD16S16Chunk( VertexD16S16[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexD16S16 vertex )
        {
            vertex.Position = reader.ReadVector3();
            vertex.Diffuse = reader.ReadUInt16();
            vertex.Specular = reader.ReadUInt16();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexD16S16 vertex )
        {
            writer.Write( vertex.Position );
            writer.Write( vertex.Diffuse );
            writer.Write( vertex.Specular );
        }
    }

    /// <inheritdoc />
    public class VertexNChunk : VertexChunk<VertexN>
    {
        public override ChunkType Type => ChunkType.VertexN;

        public VertexNChunk()
        {
        }

        public VertexNChunk( VertexN[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexN vertex )
        {
            vertex.Position = reader.ReadVector3();
            vertex.Normal = reader.ReadVector3();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexN vertex )
        {
            writer.Write( vertex.Position );
            writer.Write( vertex.Normal );
        }
    }

    /// <inheritdoc />
    public class VertexND8888Chunk : VertexChunk<VertexND8888>
    {
        public override ChunkType Type => ChunkType.VertexND8888;

        public VertexND8888Chunk()
        {
        }

        public VertexND8888Chunk( VertexND8888[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexND8888 vertex )
        {
            vertex.Position = reader.ReadVector3();
            vertex.Normal = reader.ReadVector3();
            vertex.Diffuse = reader.ReadColor();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexND8888 vertex )
        {
            writer.Write( vertex.Position );
            writer.Write( vertex.Normal );
            writer.Write( vertex.Diffuse );
        }
    }

    /// <inheritdoc />
    public class VertexNUF32Chunk : VertexChunk<VertexNUF>
    {
        public override ChunkType Type => ChunkType.VertexNUF;

        public VertexNUF32Chunk()
        {
        }

        public VertexNUF32Chunk( VertexNUF[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexNUF vertex )
        {
            vertex.Position = reader.ReadVector3();
            vertex.Normal = reader.ReadVector3();
            vertex.UserFlags = reader.ReadUInt32();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexNUF vertex )
        {
            writer.Write( vertex.Position );
            writer.Write( vertex.Normal );
            writer.Write( vertex.UserFlags );
        }
    }

    /// <inheritdoc />
    public class VertexNNFChunk : VertexChunk<VertexNNF>
    {
        public override ChunkType Type => ChunkType.VertexNNF;

        public VertexNNFChunk()
        {
        }

        public VertexNNFChunk( VertexNNF[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexNNF vertex )
        {
            vertex.Position = reader.ReadVector3();
            vertex.Normal = reader.ReadVector3();
            vertex.NinjaFlags = reader.ReadUInt32();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexNNF vertex )
        {
            writer.Write( vertex.Position );
            writer.Write( vertex.Normal );
            writer.Write( vertex.NinjaFlags );
        }
    }

    /// <inheritdoc />
    public class VertexND565S565Chunk : VertexChunk<VertexND565S565>
    {
        public override ChunkType Type => ChunkType.VertexND565S565;

        public VertexND565S565Chunk()
        {
        }

        public VertexND565S565Chunk( VertexND565S565[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexND565S565 vertex )
        {
            vertex.Position = reader.ReadVector3();
            vertex.Normal = reader.ReadVector3();
            vertex.Diffuse = reader.ReadUInt16();
            vertex.Specular = reader.ReadUInt16();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexND565S565 vertex )
        {
            writer.Write( vertex.Position );
            writer.Write( vertex.Normal );
            writer.Write( vertex.Diffuse );
            writer.Write( vertex.Specular );
        }
    }

    /// <inheritdoc />
    public class VertexND4444S565Chunk : VertexChunk<VertexND4444S565>
    {
        public override ChunkType Type => ChunkType.VertexD4444S565;

        public VertexND4444S565Chunk()
        {
        }

        public VertexND4444S565Chunk( VertexND4444S565[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexND4444S565 vertex )
        {
            vertex.Position = reader.ReadVector3();
            vertex.Normal = reader.ReadVector3();
            vertex.Diffuse = reader.ReadUInt16();
            vertex.Specular = reader.ReadUInt16();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexND4444S565 vertex )
        {
            writer.Write( vertex.Position );
            writer.Write( vertex.Normal );
            writer.Write( vertex.Diffuse );
            writer.Write( vertex.Specular );
        }
    }

    /// <inheritdoc />
    public class VertexND16S16Chunk : VertexChunk<VertexND16S16>
    {
        public override ChunkType Type => ChunkType.VertexND16S16;

        public VertexND16S16Chunk()
        {
        }

        public VertexND16S16Chunk( VertexND16S16[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexND16S16 vertex )
        {
            vertex.Position = reader.ReadVector3();
            vertex.Normal = reader.ReadVector3();
            vertex.Diffuse = reader.ReadUInt16();
            vertex.Specular = reader.ReadUInt16();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexND16S16 vertex )
        {
            writer.Write( vertex.Position );
            writer.Write( vertex.Normal );
            writer.Write( vertex.Diffuse );
            writer.Write( vertex.Specular );
        }
    }

    /// <inheritdoc />
    public class VertexN32Chunk : VertexChunk<VertexN32>
    {
        public override ChunkType Type => ChunkType.VertexN32;

        public VertexN32Chunk()
        {
        }

        public VertexN32Chunk( VertexN32[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexN32 vertex )
        {
            vertex.Position = reader.ReadVector3();
            vertex.Normal = reader.ReadUInt32();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexN32 vertex )
        {
            writer.Write( vertex.Position );
            writer.Write( vertex.Normal );
        }
    }

    /// <inheritdoc />
    public class VertexN32D8888Chunk : VertexChunk<VertexN32D8888>
    {
        public override ChunkType Type => ChunkType.VertexN32D8888;

        public VertexN32D8888Chunk()
        {
        }

        public VertexN32D8888Chunk( VertexN32D8888[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexN32D8888 vertex )
        {
            vertex.Position = reader.ReadVector3();
            vertex.Normal = reader.ReadUInt32();
            vertex.Diffuse = reader.ReadColor();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexN32D8888 vertex )
        {
            writer.Write( vertex.Position );
            writer.Write( vertex.Normal );
            writer.Write( vertex.Diffuse );
        }
    }

    /// <inheritdoc />
    public class VertexN32UFChunk : VertexChunk<VertexN32UF>
    {
        public override ChunkType Type => ChunkType.VertexN32UF;

        public VertexN32UFChunk()
        {
        }

        public VertexN32UFChunk( VertexN32UF[] vertices )
        {
            Vertices = vertices;
        }

        protected override void ReadVertex( EndianBinaryReader reader, ref VertexN32UF vertex )
        {
            vertex.Position = reader.ReadVector3();
            vertex.Normal = reader.ReadUInt32();
            vertex.UserFlags = reader.ReadUInt32();
        }

        protected override void WriteVertex( EndianBinaryWriter writer, ref VertexN32UF vertex )
        {
            writer.Write( vertex.Position );
            writer.Write( vertex.Normal );
            writer.Write( vertex.UserFlags );
        }
    }
}
