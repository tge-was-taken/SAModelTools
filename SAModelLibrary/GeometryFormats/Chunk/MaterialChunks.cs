using System.Diagnostics;
using SAModelLibrary.IO;
using SAModelLibrary.Maths;
using SAModelLibrary.Utils;

namespace SAModelLibrary.GeometryFormats.Chunk
{
    public abstract class MaterialChunk : Chunk16
    {
        private static readonly BitField sDstAlphaField = new BitField( 0, 2 );
        private static readonly BitField sSrcAlphaField = new BitField( 3, 5 );
        private static readonly BitField sUnusedField = new BitField( 6, 7 );

        public SrcAlphaOp SourceAlpha { get; set; }

        public DstAlphaOp DestinationAlpha { get; set; }

        protected MaterialChunk()
        {
            SourceAlpha = SrcAlphaOp.Src;
            DestinationAlpha = DstAlphaOp.InverseDst;
        }

        protected override byte GetFlags()
        {
            byte flags = 0;
            sSrcAlphaField.Pack( ref flags, ( byte )SourceAlpha );
            sDstAlphaField.Pack( ref flags, ( byte )DestinationAlpha );
            return flags;
        }

        internal override void ReadBody( int size, byte flags, EndianBinaryReader reader )
        {
            SourceAlpha = ( SrcAlphaOp )sSrcAlphaField.Unpack( flags );
            DestinationAlpha = ( DstAlphaOp )sDstAlphaField.Unpack( flags );
            Debug.Assert( sUnusedField.Unpack( flags ) == 0, "Unused bits in material flags are used" );
            size = reader.ReadUInt16();
            var actualSize = size * 2;

            ReadMaterialData( actualSize, reader );
        }

        internal override void WriteBody( EndianBinaryWriter writer )
        {
            var sizePos = writer.Position;
            writer.SeekCurrent( 2 );

            WriteMaterialData( writer );

            var endPos = writer.Position;
            var size = endPos - sizePos - 2;
            var sizeBy2 = size / 2;
            writer.SeekBegin( sizePos );
            writer.Write( ( ushort )sizeBy2 );
            writer.SeekBegin( endPos );
        }

        protected abstract void ReadMaterialData( int size, EndianBinaryReader reader );
        protected abstract void WriteMaterialData( EndianBinaryWriter writer );
    }

    public class MaterialDiffuseChunk : MaterialChunk
    {
        public override ChunkType Type => ChunkType.MaterialDiffuse;

        public Color Diffuse { get; set; }

        protected override void ReadMaterialData( int size, EndianBinaryReader reader )
        {
            if ( size >= 4 )
                Diffuse = reader.ReadColor();
        }

        protected override void WriteMaterialData( EndianBinaryWriter writer )
        {
            writer.Write( Diffuse );
        }
    }

    public class MaterialDiffuse2Chunk : MaterialDiffuseChunk
    {
        public override ChunkType Type => ChunkType.MaterialDiffuse2;
    }

    public class MaterialAmbientChunk : MaterialChunk
    {
        public override ChunkType Type => ChunkType.MaterialAmbient;

        public Color Ambient { get; set; }

        protected override void ReadMaterialData( int size, EndianBinaryReader reader )
        {
            if ( size >= 4 )
                Ambient = reader.ReadColor();
        }

        protected override void WriteMaterialData( EndianBinaryWriter writer )
        {
            writer.Write( Ambient );
        }
    }

    public class MaterialAmbient2Chunk : MaterialAmbientChunk
    {
        public override ChunkType Type => ChunkType.MaterialAmbient2;
    }

    public class MaterialDiffuseAmbientChunk : MaterialChunk
    {
        public override ChunkType Type => ChunkType.MaterialDiffuseAmbient;
        public Color Diffuse { get; set; }

        public Color Ambient { get; set; }

        protected override void ReadMaterialData( int size, EndianBinaryReader reader )
        {
            if ( size >= 4 )
                Diffuse = reader.ReadColor();

            if ( size >= 8 )
                Ambient = reader.ReadColor();
        }

        protected override void WriteMaterialData( EndianBinaryWriter writer )
        {
            writer.Write( Diffuse );
            writer.Write( Ambient );
        }
    }

    public class MaterialDiffuseAmbient2Chunk : MaterialDiffuseAmbientChunk
    {
        public override ChunkType Type => ChunkType.MaterialDiffuseAmbient2;
    }

    public class MaterialSpecularChunk : MaterialChunk
    {
        public override ChunkType Type => ChunkType.MaterialSpecular;

        public Color Specular { get; set; }

        protected override void ReadMaterialData( int size, EndianBinaryReader reader )
        {
            if ( size >= 4 )
                Specular = reader.ReadColor();
        }

        protected override void WriteMaterialData( EndianBinaryWriter writer )
        {
            writer.Write( Specular );
        }
    }

    public class MaterialSpecular2Chunk : MaterialSpecularChunk
    {
        public override ChunkType Type => ChunkType.MaterialSpecular2;
    }

    public class MaterialDiffuseSpecularChunk : MaterialChunk
    {
        public override ChunkType Type => ChunkType.MaterialDiffuseSpecular;
        public Color Diffuse { get; set; }

        public Color Specular { get; set; }

        protected override void ReadMaterialData( int size, EndianBinaryReader reader )
        {
            if ( size >= 4 )
                Diffuse = reader.ReadColor();

            if ( size >= 8 )
                Specular = reader.ReadColor();
        }

        protected override void WriteMaterialData( EndianBinaryWriter writer )
        {
            writer.Write( Diffuse );
            writer.Write( Specular );
        }
    }

    public class MaterialDiffuseSpecular2Chunk : MaterialDiffuseSpecularChunk
    {
        public override ChunkType Type => ChunkType.MaterialDiffuseSpecular2;
    }

    public class MaterialAmbientSpecularChunk : MaterialChunk
    {
        public override ChunkType Type => ChunkType.MaterialAmbientSpecular;
        public Color Ambient { get; set; }

        public Color Specular { get; set; }

        protected override void ReadMaterialData( int size, EndianBinaryReader reader )
        {
            if ( size >= 4 )
                Ambient = reader.ReadColor();

            if ( size >= 8 )
                Specular = reader.ReadColor();
        }

        protected override void WriteMaterialData( EndianBinaryWriter writer )
        {
            writer.Write( Ambient );
            writer.Write( Specular );
        }
    }

    public class MaterialAmbientSpecular2Chunk : MaterialAmbientSpecularChunk
    {
        public override ChunkType Type => ChunkType.MaterialAmbientSpecular2;
    }

    public class MaterialDiffuseAmbientSpecularChunk : MaterialChunk
    {
        public override ChunkType Type => ChunkType.MaterialDiffuseAmbientSpecular;

        public Color Diffuse { get; set; }

        public Color Ambient { get; set; }

        public Color Specular { get; set; }

        protected override void ReadMaterialData( int size, EndianBinaryReader reader )
        {
            if ( size >= 4 )
                Diffuse = reader.ReadColor();

            if ( size >= 8 )
                Ambient = reader.ReadColor();

            if ( size >= 12 )
                Specular = reader.ReadColor();
        }

        protected override void WriteMaterialData( EndianBinaryWriter writer )
        {
            writer.Write( Diffuse );
            writer.Write( Ambient );
            writer.Write( Specular );
        }
    }

    public class MaterialDiffuseAmbientSpecular2Chunk : MaterialDiffuseAmbientSpecularChunk
    {
        public override ChunkType Type => ChunkType.MaterialDiffuseAmbientSpecular2;
    }

    public class MaterialBumpChunk : MaterialChunk
    {
        public override ChunkType Type => ChunkType.MaterialBump;

        public short DX { get; set; }

        public short DY { get; set; }

        public short DZ { get; set; }

        public short UX { get; set; }

        public short UY { get; set; }

        public short UZ { get; set; }

        protected override void ReadMaterialData( int size, EndianBinaryReader reader )
        {
            DX = reader.ReadInt16();
            DY = reader.ReadInt16();
            DZ = reader.ReadInt16();
            UX = reader.ReadInt16();
            UY = reader.ReadInt16();
            UZ = reader.ReadInt16();
        }

        protected override void WriteMaterialData( EndianBinaryWriter writer )
        {
            writer.Write( DX );
            writer.Write( DY );
            writer.Write( DZ );
            writer.Write( UX );
            writer.Write( UY );
            writer.Write( UZ );
        }
    }
}
