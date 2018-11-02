using System;
using System.Diagnostics;
using SAModelLibrary.IO;
using SAModelLibrary.Maths;

namespace SAModelLibrary.GeometryFormats.GC
{
    public abstract class Param
    {
        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        public abstract MeshStateParamType Type { get; }

        internal abstract void ReadBody( MeshStateParamType type, EndianBinaryReader reader );

        internal void Write( EndianBinaryWriter writer )
        {
            writer.Write( ( int )Type );
            WriteBody( writer );
        }

        protected abstract void WriteBody( EndianBinaryWriter writer );
    }

    public class IndexAttributeFlagsParam : Param
    {
        public override MeshStateParamType Type => MeshStateParamType.IndexAttributeFlags;

        public IndexAttributeFlags Flags { get; set; }

        public IndexAttributeFlagsParam()
        {
        }

        public IndexAttributeFlagsParam( IndexAttributeFlags flags )
        {
            Flags = flags;
        }

        internal override void ReadBody( MeshStateParamType type, EndianBinaryReader reader )
        {
            Flags = ( IndexAttributeFlags )reader.ReadInt32();
        }

        protected override void WriteBody( EndianBinaryWriter writer )
        {
            writer.Write( ( int )Flags );
        }
    }

    public class LightingParams : Param
    {
        public override MeshStateParamType Type => MeshStateParamType.Lighting;

        public ushort Value1 { get; set; }

        public ushort Value2 { get; set; }

        public LightingParams()
        {
        }

        public LightingParams( Preset preset )
        {
            switch ( preset )
            {
                case Preset.Colors:
                    Value1 = 0x0b11;
                    break;
                case Preset.Normals:
                    Value1 = 0x0011;
                    break;
            }

            Value2 = 1;
        }

        protected override void WriteBody( EndianBinaryWriter writer )
        {
            if ( writer.Endianness == Endianness.Little )
            {
                writer.Write( Value1 );
                writer.Write( Value2 );
            }
            else
            {
                writer.Write( Value2 );
                writer.Write( Value1 );
            }
        }

        internal override void ReadBody( MeshStateParamType type, EndianBinaryReader reader )
        {
            if ( reader.Endianness == Endianness.Little )
            {
                Value1 = reader.ReadUInt16();
                Value2 = reader.ReadUInt16();
            }
            else
            {
                Value2 = reader.ReadUInt16();
                Value1 = reader.ReadUInt16();
            }
        }

        public enum Preset
        {
            Colors,
            Normals
        }
    }

    public class BlendAlphaParam : Param
    {
        public override MeshStateParamType Type => MeshStateParamType.BlendAlpha;

        public BlendAlphaFlags Flags { get; set; }

        public BlendAlphaParam()
        {
            Flags = BlendAlphaFlags.Bit8 | BlendAlphaFlags.Bit10 | BlendAlphaFlags.Bit13;
        }

        protected override void WriteBody( EndianBinaryWriter writer )
        {
            writer.Write( ( ushort ) Flags );
            writer.Write( ( ushort ) 0 );
        }

        internal override void ReadBody( MeshStateParamType type, EndianBinaryReader reader )
        {
            Flags = ( BlendAlphaFlags ) reader.ReadUInt16();
            var unused = reader.ReadInt16();
            Debug.Assert( unused == 0 );
        }
    }

    [Flags]
    public enum BlendAlphaFlags
    {
        Bit0  = 1 << 0,
        Bit1  = 1 << 1,
        Bit2  = 1 << 2,
        Bit3  = 1 << 3,
        Bit4  = 1 << 4,
        Bit5  = 1 << 5,
        Bit6  = 1 << 6,
        Bit7  = 1 << 7,
        Bit8  = 1 << 8,
        Bit9  = 1 << 9,
        Bit10 = 1 << 10,
        Bit11 = 1 << 11,
        Bit12 = 1 << 12,
        Bit13 = 1 << 13,
        UseAlpha = 1 << 14,
        Bit15 = 1 << 15,
    }


    public class AmbientColorParam : Param
    {
        public override MeshStateParamType Type => MeshStateParamType.AmbientColor;

        public Color Color { get; set; }

        public AmbientColorParam()
        {
            Color = new Color( 178, 178, 178 );
        }

        protected override void WriteBody( EndianBinaryWriter writer )
        {
            var endianness = writer.Endianness;
            writer.Endianness = Endianness.Big;
            writer.Write( Color );
            writer.Endianness = endianness;
        }

        internal override void ReadBody( MeshStateParamType type, EndianBinaryReader reader )
        {
            var endianness = reader.Endianness;
            reader.Endianness = Endianness.Big;
            Color             = reader.ReadColor();
            reader.Endianness = endianness;
        }
    }


    public class TextureParams : Param
    {
        public override MeshStateParamType Type => MeshStateParamType.Texture;

        public ushort TextureId { get; set; }

        public TileMode TileMode { get; set; }

        public TextureParams()
        {        
        }

        public TextureParams(ushort textureId, TileMode tileMode = TileMode.WrapU | TileMode.WrapV )
        {
            TextureId = textureId;
            TileMode = tileMode;
        }

        internal override void ReadBody( MeshStateParamType type, EndianBinaryReader reader )
        {
            if ( reader.Endianness == Endianness.Little )
            {
                TextureId = reader.ReadUInt16();
                TileMode = ( TileMode )reader.ReadUInt16();
            }
            else
            {
                TileMode = ( TileMode )reader.ReadUInt16();
                TextureId = reader.ReadUInt16();
            }
        }

        protected override void WriteBody( EndianBinaryWriter writer )
        {
            if ( writer.Endianness == Endianness.Little )
            {
                writer.Write( TextureId );
                writer.Write( (ushort)TileMode );
            }
            else
            {
                writer.Write( ( ushort )TileMode );
                writer.Write( TextureId );
            }
        }
    }

    [Flags]
    public enum TileMode
    {
        WrapU = 1 << 0,
        MirrorU = 1 << 1,
        WrapV = 1 << 2,
        MirrorV = 1 << 3,
    }

    public class MipMapParams : Param
    {
        public override MeshStateParamType Type => MeshStateParamType.MipMap;

        public ushort Value1 { get; set; }

        public ushort Value2 { get; set; }

        public MipMapParams()
        {
            Value1 = 0x104a;
            Value2 = 0x0000;
        }

        protected override void WriteBody( EndianBinaryWriter writer )
        {
            if ( writer.Endianness == Endianness.Little )
            {
                writer.Write( Value1 );
                writer.Write( Value2 );
            }
            else
            {
                writer.Write( Value2 );
                writer.Write( Value1 );
            }
        }

        internal override void ReadBody( MeshStateParamType type, EndianBinaryReader reader )
        {
            if ( reader.Endianness == Endianness.Little )
            {
                Value1 = reader.ReadUInt16();
                Value2 = reader.ReadUInt16();
            }
            else
            {
                Value2 = reader.ReadUInt16();
                Value1 = reader.ReadUInt16();
            }
        }
    }

    public class UnknownParam : Param
    {
        private MeshStateParamType mType;

        public override MeshStateParamType Type => mType;

        public ushort Value1 { get; set; }

        public ushort Value2 { get; set; }

        public UnknownParam()
        {
            
        }

        public UnknownParam(ushort value1, ushort value2)
        {
            Value1 = value1;
            Value2 = value2;
        }

        protected override void WriteBody( EndianBinaryWriter writer )
        {
            if ( writer.Endianness == Endianness.Little )
            {
                writer.Write( Value1 );
                writer.Write( Value2 );
            }
            else
            {
                writer.Write( Value2 );
                writer.Write( Value1 );
            }
        }

        internal override void ReadBody( MeshStateParamType type, EndianBinaryReader reader )
        {
            mType = type;

            if ( reader.Endianness == Endianness.Little )
            {
                Value1 = reader.ReadUInt16();
                Value2 = reader.ReadUInt16();
            }
            else
            {
                Value2 = reader.ReadUInt16();
                Value1 = reader.ReadUInt16();
            }
        }
    }

    public enum MeshStateParamType
    {
        IndexAttributeFlags = 1,
        Lighting = 2,
        BlendAlpha = 4,
        AmbientColor = 5,
        Texture = 8,
        MipMap = 10,
    }
}