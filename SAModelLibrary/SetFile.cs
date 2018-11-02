using System.Collections.Generic;
using System.Numerics;
using SAModelLibrary.IO;
using SAModelLibrary.Maths;

namespace SAModelLibrary
{
    public class SetFile : ISerializableObject
    {
        public string SourceFilePath { get; set; }
        public long SourceOffset { get; set; }
        public Endianness SourceEndianness { get; set; }

        public List<SetObject> Objects { get; set; }

        void ISerializableObject.Read( EndianBinaryReader reader, object context )
        {
            var objectCount = reader.ReadInt32();
            reader.SeekCurrent( 28 );

            Objects = new List<SetObject>( objectCount );
            for ( int i = 0; i < objectCount; i++ )
                Objects.Add( reader.ReadObject<SetObject>() );
        }

        void ISerializableObject.Write( EndianBinaryWriter writer, object context )
        {
            writer.Write( Objects.Count );
            writer.WriteAlignmentPadding( 32 );
            foreach ( var setObject in Objects )
                writer.WriteObject( setObject );
        }
    }

    public class SetObject : ISerializableObject
    {
        public string SourceFilePath { get; set; }
        public long SourceOffset { get; set; }
        public Endianness SourceEndianness { get; set; }

        public ushort Type { get; set; }

        public AngleVector Rotation { get; set; }

        public Vector3 Position { get; set; }

        public float Value1 { get; set; }

        public float Value2 { get; set; }

        public float Value3 { get; set; }

        void ISerializableObject.Read( EndianBinaryReader reader, object context )
        {
            Type = reader.ReadUInt16();
            Rotation = new AngleVector( reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16() );
            Position = reader.ReadVector3();
            Value1 = reader.ReadSingle();
            Value2 = reader.ReadSingle();
            Value3 = reader.ReadSingle();
        }

        void ISerializableObject.Write( EndianBinaryWriter writer, object context )
        {
            writer.Write( Type );
            writer.Write( ( ushort ) Rotation.X );
            writer.Write( ( ushort ) Rotation.Y );
            writer.Write( ( ushort ) Rotation.Z );
            writer.Write( Position );
            writer.Write( Value1 );
            writer.Write( Value2 );
            writer.Write( Value3 );
        }
    }
}
