using System.Collections.Generic;
using SAModelLibrary.IO;

namespace SAModelLibrary.SA2.SOC
{
    public class Geometry : ISerializableObject
    {
        public string     SourceFilePath   { get; set; }
        public long       SourceOffset     { get; set; }
        public Endianness SourceEndianness { get; set; }

        public string Name { get; set; }

        public List<Mesh> Meshes { get; private set; }

        public Geometry()
        {
            Name   = "NoName";
            Meshes = new List<Mesh>();
        }

        void ISerializableObject.Read( EndianBinaryReader reader, object context )
        {
            Name = reader.ReadString( StringBinaryFormat.PrefixedLength32 );
            var meshCount = reader.ReadInt32();
            Meshes = new List<Mesh>( meshCount );
            for ( int i = 0; i < meshCount; i++ )
                Meshes.Add( reader.ReadObject<Mesh>() );
        }

        void ISerializableObject.Write( EndianBinaryWriter writer, object context )
        {
            writer.Write( Name, StringBinaryFormat.PrefixedLength32 );
            writer.Write( Meshes.Count );
            foreach ( var mesh in Meshes )
                writer.WriteObject( mesh );
        }
    }
}