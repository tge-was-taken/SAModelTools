using System;
using System.Collections.Generic;
using System.Diagnostics;
using SAModelLibrary.Exceptions;
using SAModelLibrary.IO;
using SAModelLibrary.Utils;

namespace SAModelLibrary.GeometryFormats.Chunk
{
    /// <summary>
    /// Chunk geometry format. Vertex chunk list contains vertex data, polygon chunk list contains triangle and material data.
    /// </summary>
    public class Geometry : IGeometry
    {
        private static readonly EndChunk16 sEndChunk16 = new EndChunk16();
        private static readonly EndChunk32 sEndChunk32 = new EndChunk32();

        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <inheritdoc />
        public GeometryFormat Format => GeometryFormat.Chunk;

        /// <summary>
        /// Gets the list of vertex chunks in the vertex chunk list.
        /// </summary>
        public List<VertexChunk> VertexList { get; }

        /// <summary>
        /// Gets the list of chunks in the polygon chunk list.
        /// </summary>
        public List<Chunk16> PolygonList { get; }

        /// <summary>
        /// Gets or sets the bounding sphere of this geometry.
        /// </summary>
        public BoundingSphere BoundingSphere { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Geometry"/> that is completely empty.
        /// </summary>
        public Geometry()
        {
            VertexList = new List<VertexChunk>();
            PolygonList = new List<Chunk16>();
        }

        public static bool Validate( EndianBinaryReader reader )
        {
            var start = reader.Position;

            try
            {
                var vertexListOffset  = reader.ReadInt32();
                var polygonListOffset = reader.ReadInt32();
                if ( ( vertexListOffset == 0 && polygonListOffset == 0 ) ||
                     vertexListOffset < 0 || polygonListOffset < 0 ||
                     ( vertexListOffset + reader.BaseOffset ) > reader.Length ||
                     ( polygonListOffset + reader.BaseOffset ) > reader.Length )
                    return false;

                if ( !reader.IsValidOffset( vertexListOffset ) )
                    return false;

                if ( !reader.IsValidOffset( polygonListOffset ) )
                    return false;

                bool success = true;
                if ( vertexListOffset != 0 )
                {
                    success = false;
                    reader.ReadAtOffset( vertexListOffset, () =>
                    {
                        while ( true )
                        {
                            short     size;
                            byte      flags;
                            ChunkType type;
                            if ( reader.Endianness == Endianness.Big )
                            {
                                size  = reader.ReadInt16();
                                flags = reader.ReadByte();
                                type  = ( ChunkType )reader.ReadByte();
                            }
                            else
                            {
                                type  = ( ChunkType )reader.ReadByte();
                                flags = reader.ReadByte();
                                size  = reader.ReadInt16();
                            }

                            if ( size < 0 || type < ChunkType.VertexSH || type > ChunkType.VertexN32UF )
                                return;

                            var chunkEndPos = reader.Position + ( size * 4 );
                            if ( chunkEndPos > reader.Length )
                                return;

                            success = true;

                            if ( type == ChunkType.End )
                                break;
                        }
                    } );
                }

                if ( false && polygonListOffset != 0 )
                {
                    success = true;
                    reader.ReadAtOffset( polygonListOffset, () =>
                    {
                        while ( true )
                        {
                            var header = reader.ReadUInt16();
                            var type = ( ChunkType )( byte )header;
                            if ( type == ChunkType.Null )
                                continue;

                            switch ( type )
                            {
                                case ChunkType.BlendAlpha:
                                case ChunkType.MipmapDAdjust:
                                case ChunkType.SpecularExponent:
                                case ChunkType.CachePolygonList:
                                case ChunkType.DrawPolygonList:
                                    break;
                                case ChunkType.TextureId:
                                case ChunkType.TextureId2:
                                    reader.SeekCurrent( 2 );
                                    break;
                                case ChunkType.MaterialDiffuse:
                                case ChunkType.MaterialAmbient:
                                case ChunkType.MaterialDiffuseAmbient:
                                case ChunkType.MaterialSpecular:
                                case ChunkType.MaterialDiffuseSpecular:
                                case ChunkType.MaterialAmbientSpecular:
                                case ChunkType.MaterialDiffuseAmbientSpecular:
                                case ChunkType.MaterialBump:
                                case ChunkType.MaterialDiffuse2:
                                case ChunkType.MaterialAmbient2:
                                case ChunkType.MaterialDiffuseAmbient2:
                                case ChunkType.MaterialSpecular2:
                                case ChunkType.MaterialDiffuseSpecular2:
                                case ChunkType.MaterialAmbientSpecular2:
                                case ChunkType.MaterialDiffuseAmbientSpecular2:
                                case ChunkType.Strip:
                                case ChunkType.StripUVN:
                                case ChunkType.StripUVH:
                                case ChunkType.StripVN:
                                case ChunkType.StripUVNVN:
                                case ChunkType.StripUVHVN:
                                case ChunkType.StripD8:
                                case ChunkType.StripUVND8:
                                case ChunkType.StripUVHD8:
                                case ChunkType.Strip2:
                                case ChunkType.StripUVN2:
                                case ChunkType.StripUVH2:
                                case ChunkType.VolumeTristrip:
                                    {
                                        var size         = reader.ReadInt16();
                                        var actualSize   = size * 2;
                                        var nextChunkPos = reader.Position + actualSize;
                                        if ( size <= 0 || nextChunkPos > reader.Length )
                                        {
                                            success = false;
                                            return;
                                        }
                                        else
                                        {
                                            reader.SeekBegin( nextChunkPos );
                                        }
                                    }
                                    break;
                                case ChunkType.End:
                                    return;
                                default:
                                    success = false;
                                    return;
                            }
                        }
                    } );
                }

                return success;
            }
            finally
            {
                reader.Position = start;
            }
        }

        void ISerializableObject.Read( EndianBinaryReader reader, object context )
        {
            reader.ReadOffset( ReadVertexList );
            reader.ReadOffset( ReadPolygonList );
            BoundingSphere = reader.ReadBoundingSphere();
        }

        private void ReadVertexList( EndianBinaryReader reader )
        {
            while ( true )
            {
                short size;
                byte flags;
                ChunkType type;
                if ( reader.Endianness == Endianness.Big )
                {
                    size = reader.ReadInt16();
                    flags = reader.ReadByte();
                    type = ( ChunkType ) reader.ReadByte();
                }
                else
                {
                    type  = ( ChunkType )reader.ReadByte();
                    flags = reader.ReadByte();
                    size  = reader.ReadInt16();
                }

                if ( type == ChunkType.End )
                    break;

                var realSize = size * 4;
                var chunkEndPos = reader.Position + realSize;
                VertexChunk chunk;

                switch ( type )
                {
                    case ChunkType.VertexSH:
                        chunk = new VertexSHChunk();
                        break;
                    case ChunkType.VertexNSH:
                        chunk = new VertexNSHChunk();
                        break;
                    case ChunkType.VertexXYZ:
                        chunk = new VertexXYZChunk();
                        break;
                    case ChunkType.VertexD8888:
                        chunk = new VertexD8888Chunk();
                        break;
                    case ChunkType.VertexUF:
                        chunk = new VertexUF32Chunk();
                        break;
                    case ChunkType.VertexNF:
                        chunk = new VertexNF32Chunk();
                        break;
                    case ChunkType.VertexD565S565:
                        chunk = new VertexD565S565Chunk();
                        break;
                    case ChunkType.VertexD4444S565:
                        chunk = new VertexD4444S565Chunk();
                        break;
                    case ChunkType.VertexD16S16:
                        chunk = new VertexD16S16Chunk();
                        break;
                    case ChunkType.VertexN:
                        chunk = new VertexNChunk();
                        break;
                    case ChunkType.VertexND8888:
                        chunk = new VertexND8888Chunk();
                        break;
                    case ChunkType.VertexNUF:
                        chunk = new VertexNUF32Chunk();
                        break;
                    case ChunkType.VertexNNF:
                        chunk = new VertexNNFChunk();
                        break;
                    case ChunkType.VertexND565S565:
                        chunk = new VertexND565S565Chunk();
                        break;
                    case ChunkType.VertexND4444S565:
                        chunk = new VertexND4444S565Chunk();
                        break;
                    case ChunkType.VertexND16S16:
                        chunk = new VertexND16S16Chunk();
                        break;
                    case ChunkType.VertexN32:
                        chunk = new VertexN32Chunk();
                        break;
                    case ChunkType.VertexN32D8888:
                        chunk = new VertexN32D8888Chunk();
                        break;
                    case ChunkType.VertexN32UF:
                        chunk = new VertexN32UFChunk();
                        break;
                    default:
                        throw new InvalidGeometryDataException( $"Found non-vertex chunk in vertex list: {type}" );
                }

                chunk.ReadBody( realSize, flags, reader );
                VertexList.Add( chunk );

                reader.SeekBegin( chunkEndPos );
            }
        }

        private void ReadPolygonList( EndianBinaryReader reader )
        {
            while ( true )
            {
                var header = reader.ReadUInt16();
                var flags = ( byte ) ( header >> 8 );
                var type = ( ChunkType ) ( byte )header;
                if ( type == ChunkType.End )
                    break;
                else if ( type == ChunkType.Null )
                    continue;

                Chunk16 chunk;

                switch ( type )
                {
                    case ChunkType.BlendAlpha:
                        chunk = new BlendAlphaChunk();
                        break;
                    case ChunkType.MipmapDAdjust:
                        chunk = new MipmapDAdjustChunk();
                        break;
                    case ChunkType.SpecularExponent:
                        chunk = new SpecularExponentChunk();
                        break;
                    case ChunkType.CachePolygonList:
                        chunk = new CachePolygonListChunk();
                        break;
                    case ChunkType.DrawPolygonList:
                        chunk = new DrawPolygonListChunk();
                        break;
                    case ChunkType.TextureId:
                        chunk = new TextureIdChunk();
                        break;
                    case ChunkType.TextureId2:
                        chunk = new TextureId2Chunk();
                        break;
                    case ChunkType.MaterialDiffuse:
                        chunk = new MaterialDiffuseChunk();
                        break;
                    case ChunkType.MaterialAmbient:
                        chunk = new MaterialAmbientChunk();
                        break;
                    case ChunkType.MaterialDiffuseAmbient:
                        chunk = new MaterialDiffuseAmbientChunk();
                        break;
                    case ChunkType.MaterialSpecular:
                        chunk = new MaterialSpecularChunk();
                        break;
                    case ChunkType.MaterialDiffuseSpecular:
                        chunk = new MaterialDiffuseSpecularChunk();
                        break;
                    case ChunkType.MaterialAmbientSpecular:
                        chunk = new MaterialAmbientSpecularChunk();
                        break;
                    case ChunkType.MaterialDiffuseAmbientSpecular:
                        chunk = new MaterialDiffuseAmbientSpecularChunk();
                        break;
                    case ChunkType.MaterialBump:
                        chunk = new MaterialBumpChunk();
                        break;
                    case ChunkType.MaterialDiffuse2:
                        chunk = new MaterialDiffuse2Chunk();
                        break;
                    case ChunkType.MaterialAmbient2:
                        chunk = new MaterialAmbient2Chunk();
                        break;
                    case ChunkType.MaterialDiffuseAmbient2:
                        chunk = new MaterialDiffuseAmbient2Chunk();
                        break;
                    case ChunkType.MaterialSpecular2:
                        chunk = new MaterialSpecular2Chunk();
                        break;
                    case ChunkType.MaterialDiffuseSpecular2:
                        chunk = new MaterialDiffuseSpecular2Chunk();
                        break;
                    case ChunkType.MaterialAmbientSpecular2:
                        chunk = new MaterialAmbientSpecular2Chunk();
                        break;
                    case ChunkType.MaterialDiffuseAmbientSpecular2:
                        chunk = new MaterialDiffuseAmbientSpecular2Chunk();
                        break;
                    case ChunkType.Strip:
                        chunk = new StripChunk();
                        break;
                    case ChunkType.StripUVN:
                        chunk = new StripUVNChunk();
                        break;
                    case ChunkType.StripUVH:
                        chunk = new StripUVHChunk();
                        break;
                    case ChunkType.StripVN:
                        chunk = new StripVNChunk();
                        break;
                    case ChunkType.StripUVNVN:
                        chunk = new StripUVNVNChunk();
                        break;
                    case ChunkType.StripUVHVN:
                        chunk = new StripUVHVNChunk();
                        break;
                    case ChunkType.StripD8:
                        chunk = new StripD8Chunk();
                        break;
                    case ChunkType.StripUVND8:
                        chunk = new StripUVND8Chunk();
                        break;
                    case ChunkType.StripUVHD8:
                        chunk = new StripUVHD8Chunk();
                        break;
                    case ChunkType.Strip2:
                        chunk = new Strip2Chunk();
                        break;
                    case ChunkType.StripUVN2:
                        chunk = new StripUVN2Chunk();
                        break;
                    case ChunkType.StripUVH2:
                        chunk = new StripUVH2Chunk();
                        break;
                    case ChunkType.VolumeTristrip:
                        chunk = new VolumeTristripChunk();
                        break;
                    default:
                        throw new NotImplementedException( $"Found unexpected chunk type in polygon list: {type}" );
                }

                chunk.ReadBody( -1, flags, reader );
                PolygonList.Add( chunk );
            }
        }

        void ISerializableObject.Write( EndianBinaryWriter writer, object context )
        {
            writer.ScheduleWriteOffsetAligned( 4, () =>
            {
                VertexList.ForEach( x => x?.Write( writer ) );
                sEndChunk32.Write( writer );
            });
            writer.ScheduleWriteOffsetAligned( 4, () =>
            {
                PolygonList.ForEach( x =>
                {
                    x.Write( writer );
                    writer.WriteAlignmentPadding( 4 );
                });
                sEndChunk16.Write( writer );
            });
            writer.Write( BoundingSphere );
        }
    }

    /// <summary>
    /// Abstract class for chunks with a 16-bit header.
    /// </summary>
    public abstract class Chunk16 : Chunk
    {
        internal override void Write( EndianBinaryWriter writer )
        {
            writer.Write( ( ushort ) ( GetFlags() << 8 | ( byte ) Type ) );
            WriteBody( writer );
        }
    }

    /// <summary>
    /// Abstract class for chunks with a 32-bit header.
    /// </summary>
    public abstract class Chunk32 : Chunk
    {
        internal override void Write( EndianBinaryWriter writer )
        {
            // Skip header
            var headerPos = writer.Position;
            writer.Position += 4;
            var bodyStartPos = writer.Position;

            // Write body
            WriteBody( writer );
            var bodyEndPos = writer.Position;

            // Calculate body size
            var bodySize = bodyEndPos - bodyStartPos;
            var headerBodySize = ( ushort ) ( bodySize / 4 );

            // Write chunk header
            writer.SeekBegin( headerPos );

            if ( writer.Endianness == Endianness.Big )
            {
                writer.Write( headerBodySize );
                writer.Write( GetFlags() );
                writer.Write( ( byte ) Type );
            }
            else
            {
                writer.Write( ( byte ) Type );
                writer.Write( GetFlags() );
                writer.Write( headerBodySize );
            }

            writer.SeekBegin( bodyEndPos );
        }
    }

    /// <summary>
    /// Null chunk used for alignment.
    /// </summary>
    public class NullChunk16 : Chunk16
    {
        public override ChunkType Type => ChunkType.Null;

        protected override byte GetFlags() => 0;

        internal override void ReadBody( int size, byte flags, EndianBinaryReader reader ){}

        internal override void WriteBody( EndianBinaryWriter writer ){}

        internal override void Write( EndianBinaryWriter writer )
        {
            writer.Write( ( short ) 0 );
        }
    }

    /// <summary>
    /// Null chunk used for alignment.
    /// </summary>
    public class NullChunk32 : Chunk32
    {
        public override ChunkType Type => ChunkType.Null;

        protected override byte GetFlags() => 0;

        internal override void ReadBody( int size, byte flags, EndianBinaryReader reader ) { }

        internal override void WriteBody( EndianBinaryWriter writer ) { }

        internal override void Write( EndianBinaryWriter writer )
        {
            writer.Write( 0 );
        }
    }

    /// <summary>
    /// Chunk that signifies the end of a chunk list.
    /// </summary>
    public class EndChunk16 : Chunk16
    {
        public override ChunkType Type => ChunkType.End;

        protected override byte GetFlags() => 0;

        internal override void ReadBody( int size, byte flags, EndianBinaryReader reader ) { }

        internal override void WriteBody( EndianBinaryWriter writer ) { }
    }

    /// <summary>
    /// Chunk that signifies the end of a chunk list.
    /// </summary>
    public class EndChunk32 : Chunk32
    {
        public override ChunkType Type => ChunkType.End;

        protected override byte GetFlags() => 0;

        internal override void ReadBody( int size, byte flags, EndianBinaryReader reader ) { }

        internal override void WriteBody( EndianBinaryWriter writer ) { }
    }

    public class BlendAlphaChunk : Chunk16
    {
        private static readonly BitField sSrcAlphaField = new BitField( 0, 2 );
        private static readonly BitField sDstAlphaField = new BitField( 3, 5 );
        private static readonly BitField sUnusedField = new BitField( 6, 7 );

        public override ChunkType Type => ChunkType.BlendAlpha;

        public SrcAlphaOp SourceAlpha { get; set; }

        public DstAlphaOp DestinationAlpha { get; set; }

        protected override byte GetFlags()
        {
            byte packed = 0;
            sSrcAlphaField.Pack( ref packed, ( byte ) SourceAlpha );
            sDstAlphaField.Pack( ref packed, ( byte ) DestinationAlpha );
            return packed;
        }

        internal override void ReadBody( int size, byte flags, EndianBinaryReader reader )
        {
            SourceAlpha = ( SrcAlphaOp ) sSrcAlphaField.Unpack( flags );
            DestinationAlpha = ( DstAlphaOp ) sDstAlphaField.Unpack( flags );
            Debug.Assert( sUnusedField.Unpack( flags ) == 0, "Unused bits in blend alpha chunk flags are used" );
            Debug.Assert( GetFlags() == flags );
        }

        internal override void WriteBody( EndianBinaryWriter writer ){}
    }

    public class MipmapDAdjustChunk : Chunk16
    {
        private static readonly BitField sDAdjustField = new BitField( 0, 3 );

        public override ChunkType Type => ChunkType.MipmapDAdjust;

        public MipMapDAdjust DAdjust { get; set; }

        protected override byte GetFlags()
        {
            byte packed = 0;
            sDAdjustField.Pack( ref packed, (byte)DAdjust );
            return packed;
        }

        internal override void ReadBody( int size, byte flags, EndianBinaryReader reader )
        {
            DAdjust = ( MipMapDAdjust )sDAdjustField.Unpack( flags );
            Debug.Assert( GetFlags() == flags );
        }

        internal override void WriteBody( EndianBinaryWriter writer ) {}
    }

    public class SpecularExponentChunk : Chunk16
    {
        private static readonly BitField sExponentField = new BitField( 0, 4 );

        public override ChunkType Type => ChunkType.SpecularExponent;

        public byte Exponent { get; set; }

        protected override byte GetFlags()
        {
            byte packed = 0;
            sExponentField.Pack( ref packed, Exponent );
            return packed;
        }

        internal override void ReadBody( int size, byte flags, EndianBinaryReader reader )
        {
            Exponent = sExponentField.Unpack( flags );
            Debug.Assert( GetFlags() == flags );
        }

        internal override void WriteBody( EndianBinaryWriter writer ) { }
    }

    public abstract class PolygonListReferenceChunk : Chunk16
    {
        public byte CacheIndex { get; set; }

        protected override byte GetFlags()
        {
            return CacheIndex;
        }

        internal override void ReadBody( int size, byte flags, EndianBinaryReader reader )
        {
            CacheIndex = flags;
        }

        internal override void WriteBody( EndianBinaryWriter writer ) { }
    }

    public class CachePolygonListChunk : PolygonListReferenceChunk
    {
        public override ChunkType Type => ChunkType.CachePolygonList;
    }

    public class DrawPolygonListChunk : PolygonListReferenceChunk
    {
        public override ChunkType Type => ChunkType.DrawPolygonList;
    }

    public class TextureIdChunk : Chunk16
    {
        private static readonly BitField sMipmapDAdjustField = new BitField( 0, 3 );
        private static readonly BitField sClampUField = new BitField( 4, 4 );
        private static readonly BitField sClampVField = new BitField( 5, 5 );
        private static readonly BitField sFlipUField = new BitField( 6, 6 );
        private static readonly BitField sFlipVField = new BitField( 7, 7 );

        private static readonly BitField sIdField = new BitField( 0, 12 );
        private static readonly BitField sSuperSampleField = new BitField( 13, 13 );
        private static readonly BitField sFilterModeField = new BitField( 14, 15 );

        public override ChunkType Type => ChunkType.TextureId;

        /// <summary>
        /// Gets or sets the mipmap D Adjust value.
        /// </summary>
        public MipMapDAdjust MipMapDAdjust { get; set; }

        /// <summary>
        /// Gets or sets whether to clamp the U texture coordinate.
        /// </summary>
        public bool ClampU { get; set; }

        /// <summary>
        /// Gets or sets whether to clamp the V texture coordinate.
        /// </summary>
        public bool ClampV { get; set; }

        /// <summary>
        /// Gets or sets whether to flip the U texture coordinate.
        /// </summary>
        public bool FlipU { get; set; }

        /// <summary>
        /// Gets or sets whether to flip the V texture coordinate.
        /// </summary>
        public bool FlipV { get; set; }

        /// <summary>
        /// Gets or sets the texture id.
        /// </summary>
        public short Id { get; set; }

        /// <summary>
        /// Gets or sets whether the texture should be super sampled or not.
        /// </summary>
        public bool SuperSample { get; set; }

        /// <summary>
        /// Gets or sets the texture filter mode.
        /// </summary>
        public FilterMode FilterMode { get; set; }

        public TextureIdChunk()
        {
            
        }

        public TextureIdChunk(short id)
        {
            MipMapDAdjust = MipMapDAdjust.D050;
            ClampU = false;
            ClampV = false;
            FlipU = false;
            FlipV = false;
            Id = id;
            SuperSample = false;
            FilterMode = FilterMode.Bilinear;
        }

        protected override byte GetFlags()
        {
            return PackFlags();
        }

        internal override void ReadBody( int size, byte flags, EndianBinaryReader reader )
        {
            UnpackFlags( flags );
            UnpackFlags2( reader.ReadUInt16() );
        }

        internal override void WriteBody( EndianBinaryWriter writer )
        {
            ushort flags2 = PackFlags2();
            writer.Write( flags2 );
        }

        private void UnpackFlags(byte flags)
        {
            MipMapDAdjust = ( MipMapDAdjust )sMipmapDAdjustField.Unpack( flags );
            ClampU        = sClampUField.Unpack( flags ) == 1;
            ClampV        = sClampVField.Unpack( flags ) == 1;
            FlipU         = sFlipUField.Unpack( flags ) == 1;
            FlipV         = sFlipVField.Unpack( flags ) == 1;
            Debug.Assert( PackFlags() == flags );
        }

        private byte PackFlags()
        {
            byte packed = 0;
            sMipmapDAdjustField.Pack( ref packed, ( byte )MipMapDAdjust );
            sClampUField.Pack( ref packed, ( byte )( ClampU ? 1 : 0 ) );
            sClampVField.Pack( ref packed, ( byte )( ClampV ? 1 : 0 ) );
            sFlipUField.Pack( ref packed, ( byte )( FlipU ? 1 : 0 ) );
            sFlipVField.Pack( ref packed, ( byte )( FlipV ? 1 : 0 ) );
            return packed;
        }

        private void UnpackFlags2( ushort flags2 )
        {
            Id          = ( short )sIdField.Unpack( flags2 );
            SuperSample = sSuperSampleField.Unpack( flags2 ) == 1;
            FilterMode  = ( FilterMode )sFilterModeField.Unpack( flags2 );
            Debug.Assert( PackFlags2() == flags2 );
        }

        private ushort PackFlags2()
        {
            ushort flags2 = 0;
            sIdField.Pack( ref flags2, ( ushort )Id );
            sSuperSampleField.Pack( ref flags2, ( ushort )( SuperSample ? 1 : 0 ) );
            sFilterModeField.Pack( ref flags2, ( ushort )FilterMode );
            return flags2;
        }
    }

    public class TextureId2Chunk : TextureIdChunk
    {
        public override ChunkType Type => ChunkType.TextureId2;
    }
}
