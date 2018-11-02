using System.Numerics;
using SAModelLibrary.IO;

namespace SAModelLibrary.SA2.SOC
{
    public class Material : ISerializableObject
    {
        public string     SourceFilePath   { get; set; }
        public long       SourceOffset     { get; set; }
        public Endianness SourceEndianness { get; set; }

        public string Name { get; set; }

        public Vector4 Ambient { get; set; }

        public Vector4 Diffuse { get; set; }

        public Vector4 Specular { get; set; }

        public Vector4 Emission { get; set; }

        public string TextureName { get; set; }

        public Material()
        {
            Name        = "NoName";
            Ambient     = Vector4.UnitW;
            Diffuse     = Vector4.UnitW;
            Specular    = Vector4.Zero;
            Emission    = Vector4.UnitW;
            TextureName = "NoName";
        }

        void ISerializableObject.Read( EndianBinaryReader reader, object context )
        {
            Name        = reader.ReadString( StringBinaryFormat.PrefixedLength32 );
            Ambient     = reader.ReadVector4();
            Diffuse     = reader.ReadVector4();
            Specular    = reader.ReadVector4();
            Emission    = reader.ReadVector4();
            TextureName = reader.ReadString( StringBinaryFormat.PrefixedLength32 );
        }

        void ISerializableObject.Write( EndianBinaryWriter writer, object context )
        {
            writer.Write( Name, StringBinaryFormat.PrefixedLength32 );
            writer.Write( Ambient );
            writer.Write( Diffuse );
            writer.Write( Specular );
            writer.Write( Emission );
            writer.Write( TextureName, StringBinaryFormat.PrefixedLength32 );
        }
    }
}