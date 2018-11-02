using System;
using System.Linq;
using SAModelLibrary.IO;
using SAModelLibrary.Utils;

namespace SAModelLibrary.GeometryFormats.Chunk
{
    /// <summary>
    /// Represents a generic triangle strip with indices of type <typeparamref name="TIndex"/>.
    /// </summary>
    /// <typeparam name="TIndex"></typeparam>
    public class Strip<TIndex> where TIndex : struct
    {
        /// <summary>
        /// Gets or sets if the initial winding order of the strip is reversed.
        /// </summary>
        public bool Reversed { get; set; }

        /// <summary>
        /// Gets or sets the indices array.
        /// </summary>
        public TIndex[] Indices { get; set; }

        /// <summary>
        /// Gets or sets the triangle user flags array. Note that this is for every triangle and not for every index.
        /// </summary>
        public ushort[][] UserFlags { get; set; }

        /// <summary>
        /// Construct a new instance of <see cref="Strip{TIndex}"/>.
        /// </summary>
        /// <param name="reversed"></param>
        /// <param name="indices"></param>
        /// <param name="userFlags"></param>
        public Strip( bool reversed, TIndex[] indices, ushort[][] userFlags = null )
        {
            Reversed = reversed;
            Indices = indices;
            UserFlags = userFlags;
        }
    }

    /// <summary>
    /// Base class for all strip chunks.
    /// </summary>
    public abstract class StripChunkBase : Chunk16
    {
        protected static readonly BitField sStripCountField = new BitField( 0, 13 );
        protected static readonly BitField sUserOffsetField = new BitField( 14, 15 );
    }

    /// <summary>
    /// Represents all flags that are applicable to a strip chunk.
    /// </summary>
    [Flags]
    public enum StripFlags
    {
        IgnoreLight    = 1 << 0,
        IgnoreSpecular = 1 << 1,
        IgnoreAmbient  = 1 << 2,
        UseAlpha       = 1 << 3,
        DoubleSided    = 1 << 4,
        FlatShaded     = 1 << 5,
        Environment    = 1 << 6
    }

    /// <summary>
    /// Base class for all generic strip chunks with indices of type <typeparamref name="TIndex"/>.
    /// </summary>
    /// <typeparam name="TIndex"></typeparam>
    public abstract class StripChunk<TIndex> : StripChunkBase where TIndex : struct
    {
        /// <summary>
        /// Gets or sets the flags for this strip chunk.
        /// </summary>
        public StripFlags Flags { get; set; }

        /// <summary>
        /// Gets or sets the strips contained within this strip chunk.
        /// </summary>
        public Strip<TIndex>[] Strips { get; set; }

        /// <summary>
        /// Utility function to convert the triangle strip indices into triangle indices.
        /// </summary>
        /// <returns></returns>
        public TIndex[] ToTriangles()
        {
            var triangleCount = Strips.Sum( x => x.Indices.Length - 2 );
            var triangles = new TIndex[triangleCount * 3];
            var triangleBaseIndex = 0;

            foreach ( var strip in Strips )
            {
                var clockwise = !strip.Reversed;
                var a = strip.Indices[0];
                var b = strip.Indices[1];

                for ( int i = 2; i < strip.Indices.Length; i++ )
                {
                    var c = strip.Indices[i];

                    if ( clockwise )
                    {
                        triangles[triangleBaseIndex + 0] = a;
                        triangles[triangleBaseIndex + 1] = b;
                        triangles[triangleBaseIndex + 2] = c;
                    }
                    else
                    {
                        triangles[triangleBaseIndex + 0] = a;
                        triangles[triangleBaseIndex + 1] = c;
                        triangles[triangleBaseIndex + 2] = b;
                    }

                    clockwise = !clockwise;
                    a = b;
                    b = c;
                    triangleBaseIndex += 3;
                }
            }

            return triangles;
        }

        protected override byte GetFlags()
        {
            return (byte)Flags;
        }

        internal override void ReadBody( int size, byte flags, EndianBinaryReader reader )
        {
            Flags = ( StripFlags )flags;
            size = reader.ReadUInt16();
            var flags2 = reader.ReadUInt16();
            var userOffset = sUserOffsetField.Unpack( flags2 );
            var stripCount = sStripCountField.Unpack( flags2 );

            Strips = new Strip<TIndex>[stripCount];
            for ( int i = 0; i < stripCount; i++ )
            {
                var reversed = false;

                var indexCount = reader.ReadInt16();

                if ( indexCount < 0 )
                {
                    indexCount = ( short )-indexCount;
                    reversed = true;
                }

                var triCount = indexCount - 2;
                var indices = new TIndex[indexCount];
                ushort[][] userFlags = null;
                if ( userOffset > 0 )
                    userFlags = new ushort[triCount][];

                for ( int j = 0; j < indexCount; j++ )
                {
                    ReadStripIndex( reader, ref indices[j] );
                    if ( userOffset > 0 && j > 1 )
                    {
                        var triUserFlags = reader.ReadUInt16s( userOffset );
                        userFlags[j - 2] = triUserFlags;
                    }
                }

                var strip = new Strip<TIndex>( reversed, indices, userFlags );
                Strips[i] = strip;
            }
        }

        internal override void WriteBody( EndianBinaryWriter writer )
        {
            var sizePos = writer.Position;
            writer.SeekCurrent( 2 );

            ushort flags2 = 0;
            var userOffset = ( ushort )( Strips != null && Strips.Length > 0 &&
                                          Strips[0].UserFlags != null && Strips[0].UserFlags.Length > 0
                ? Strips[0].UserFlags[0].Length
                : 0 );

            sUserOffsetField.Pack( ref flags2, userOffset );
            sStripCountField.Pack( ref flags2, ( ushort )Strips.Length );
            writer.Write( flags2 );

            foreach ( var strip in Strips )
            {
                var indexCount = strip.Indices.Length;
                if ( strip.Reversed )
                    indexCount = -indexCount;

                writer.Write( ( short )indexCount );

                for ( int i = 0; i < strip.Indices.Length; i++ )
                {
                    WriteStripIndex( writer, ref strip.Indices[i] );

                    if ( i > 1 && strip.UserFlags != null )
                        writer.Write( strip.UserFlags[i - 2] );
                }
            }

            var endPos = writer.Position;
            var size = endPos - sizePos - 2;
            var sizeBy2 = size / 2;
            writer.SeekBegin( sizePos );
            writer.Write( ( ushort )sizeBy2 );
            writer.SeekBegin( endPos );
        }

        protected abstract void ReadStripIndex( EndianBinaryReader reader, ref TIndex index );
        protected abstract void WriteStripIndex( EndianBinaryWriter writer, ref TIndex index );
    }

    public class StripChunk : StripChunk<StripIndex>
    {
        public override ChunkType Type => ChunkType.Strip;

        protected override void ReadStripIndex( EndianBinaryReader reader, ref StripIndex index )
        {
            index.Index = reader.ReadUInt16();
        }

        protected override void WriteStripIndex( EndianBinaryWriter writer, ref StripIndex index )
        {
            writer.Write( index.Index );
        }
    }

    public class VolumeTristripChunk : Chunk16
    {
        public override ChunkType Type => ChunkType.VolumeTristrip;

        public byte Flags { get; set; }

        public ushort[] Data { get; set; }

        protected override byte GetFlags()
        {
            return Flags;
        }

        internal override void ReadBody( int size, byte flags, EndianBinaryReader reader )
        {
            Flags = flags;
            size = reader.ReadUInt16();
            Data = reader.ReadUInt16s( size );
        }

        internal override void WriteBody( EndianBinaryWriter writer )
        {
            writer.Write( (ushort)Data.Length );
            writer.Write( Data );
        }
    }

    public class Strip2Chunk : StripChunk<StripIndex2>
    {
        public override ChunkType Type => ChunkType.Strip2;

        protected override void ReadStripIndex( EndianBinaryReader reader, ref StripIndex2 index )
        {
            index.Index = reader.ReadUInt16();
        }

        protected override void WriteStripIndex( EndianBinaryWriter writer, ref StripIndex2 index )
        {
            writer.Write( index.Index );
        }
    }

    public class StripUVNChunk : StripChunk<StripIndexUVN>
    {
        public override ChunkType Type => ChunkType.StripUVN;

        protected override void ReadStripIndex( EndianBinaryReader reader, ref StripIndexUVN index )
        {
            index.Index = reader.ReadUInt16();
            index.UV = reader.ReadVector2Int16();
        }

        protected override void WriteStripIndex( EndianBinaryWriter writer, ref StripIndexUVN index )
        {
            writer.Write( index.Index );
            writer.Write( index.UV );
        }
    }

    public class StripUVHChunk : StripChunk<StripIndexUVH>
    {
        public override ChunkType Type => ChunkType.StripUVH;

        protected override void ReadStripIndex( EndianBinaryReader reader, ref StripIndexUVH index )
        {
            index.Index = reader.ReadUInt16();
            index.UV = reader.ReadVector2Int16();
        }

        protected override void WriteStripIndex( EndianBinaryWriter writer, ref StripIndexUVH index )
        {
            writer.Write( index.Index );
            writer.Write( index.UV );
        }
    }

    public class StripVNChunk : StripChunk<StripIndexVN>
    {
        public override ChunkType Type => ChunkType.StripVN;

        protected override void ReadStripIndex( EndianBinaryReader reader, ref StripIndexVN index )
        {
            index.Index = reader.ReadUInt16();
            index.Normal = reader.ReadVector3Int16();
        }

        protected override void WriteStripIndex( EndianBinaryWriter writer, ref StripIndexVN index )
        {
            writer.Write( index.Index );
            writer.Write( index.Normal );
        }
    }

    public class StripUVNVNChunk : StripChunk<StripIndexUVNVN>
    {
        public override ChunkType Type => ChunkType.StripUVNVN;

        protected override void ReadStripIndex( EndianBinaryReader reader, ref StripIndexUVNVN index )
        {
            index.Index = reader.ReadUInt16();
            index.UV = reader.ReadVector2Int16();
            index.Normal = reader.ReadVector3Int16();
        }

        protected override void WriteStripIndex( EndianBinaryWriter writer, ref StripIndexUVNVN index )
        {
            writer.Write( index.Index );
            writer.Write( index.UV );
            writer.Write( index.Normal );
        }
    }


    public class StripUVHVNChunk : StripChunk<StripIndexUVHVN>
    {
        public override ChunkType Type => ChunkType.StripUVHVN;

        protected override void ReadStripIndex( EndianBinaryReader reader, ref StripIndexUVHVN index )
        {
            index.Index = reader.ReadUInt16();
            index.UV = reader.ReadVector2Int16();
            index.Normal = reader.ReadVector3Int16();
        }

        protected override void WriteStripIndex( EndianBinaryWriter writer, ref StripIndexUVHVN index )
        {
            writer.Write( index.Index );
            writer.Write( index.UV );
            writer.Write( index.Normal );
        }
    }

    public class StripD8Chunk : StripChunk<StripIndexD8>
    {
        public override ChunkType Type => ChunkType.StripD8;

        protected override void ReadStripIndex( EndianBinaryReader reader, ref StripIndexD8 index )
        {
            index.Index = reader.ReadUInt16();
            index.Color = reader.ReadColor();
        }

        protected override void WriteStripIndex( EndianBinaryWriter writer, ref StripIndexD8 index )
        {
            writer.Write( index.Index );
            writer.Write( index.Color );
        }
    }

    public class StripUVND8Chunk : StripChunk<StripIndexUVND8>
    {
        public override ChunkType Type => ChunkType.StripUVND8;

        protected override void ReadStripIndex( EndianBinaryReader reader, ref StripIndexUVND8 index )
        {
            index.Index = reader.ReadUInt16();
            index.UV = reader.ReadVector2Int16();
            index.Color = reader.ReadColor();
        }

        protected override void WriteStripIndex( EndianBinaryWriter writer, ref StripIndexUVND8 index )
        {
            writer.Write( index.Index );
            writer.Write( index.UV );
            writer.Write( index.Color );
        }
    }

    public class StripUVHD8Chunk : StripChunk<StripIndexUVHD8>
    {
        public override ChunkType Type => ChunkType.StripUVHD8;

        protected override void ReadStripIndex( EndianBinaryReader reader, ref StripIndexUVHD8 index )
        {
            index.Index = reader.ReadUInt16();
            index.UV = reader.ReadVector2Int16();
            index.Color = reader.ReadColor();
        }

        protected override void WriteStripIndex( EndianBinaryWriter writer, ref StripIndexUVHD8 index )
        {
            writer.Write( index.Index );
            writer.Write( index.UV );
            writer.Write( index.Color );
        }
    }

    public class StripUVN2Chunk : StripChunk<StripIndexUVN2>
    {
        public override ChunkType Type => ChunkType.StripUVN2;

        protected override void ReadStripIndex( EndianBinaryReader reader, ref StripIndexUVN2 index )
        {
            index.Index = reader.ReadUInt16();
            index.UV = reader.ReadVector2Int16();
            index.UV2 = reader.ReadVector2Int16();
        }

        protected override void WriteStripIndex( EndianBinaryWriter writer, ref StripIndexUVN2 index )
        {
            writer.Write( index.Index );
            writer.Write( index.UV );
            writer.Write( index.UV2 );
        }
    }


    public class StripUVH2Chunk : StripChunk<StripIndexUVH2>
    {
        public override ChunkType Type => ChunkType.StripUVH2;

        protected override void ReadStripIndex( EndianBinaryReader reader, ref StripIndexUVH2 index )
        {
            index.Index = reader.ReadUInt16();
            index.UV = reader.ReadVector2Int16();
            index.UV2 = reader.ReadVector2Int16();
        }

        protected override void WriteStripIndex( EndianBinaryWriter writer, ref StripIndexUVH2 index )
        {
            writer.Write( index.Index );
            writer.Write( index.UV );
            writer.Write( index.UV2 );
        }
    }
}
