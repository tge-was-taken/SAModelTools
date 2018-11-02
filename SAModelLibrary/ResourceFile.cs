using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAModelLibrary.IO;
using SAModelLibrary.SA2;

namespace SAModelLibrary
{
    public class ResourceFile
    {
        private readonly Dictionary<Type, ResourceType> sTypeToResourceType = new Dictionary<Type, ResourceType>()
        {
            { typeof( LandTableSA2 ), ResourceType.LandTable }
        };

        public ISerializableObject Resource { get; set; }

        public ResourceFile()
        {
        }

        public ResourceFile( ISerializableObject resource )
        {
            Resource = resource;
        }

        public ResourceFile( string filepath )
        {
            using ( var reader = new EndianBinaryReader( filepath, Endianness.Little ) )
                Read( reader );
        }

        public void Save( string filepath )
        {
            using ( var writer = new EndianBinaryWriter( filepath, Endianness.Little ) )
                Write( writer );
        }

        private void Read( EndianBinaryReader reader )
        {
            reader.SeekCurrent( 4 );
            var resourceType = ( ResourceType )reader.ReadInt32();
            var dataSize = reader.ReadInt32();
            var relocationTableSize = reader.ReadInt32();
            var relocationTableOffset = reader.ReadInt32();

            reader.SeekBegin( 32 );
            reader.BaseOffset = 32;
            switch ( resourceType )
            {
                case ResourceType.LandTable:
                    Resource = reader.ReadObject<LandTableSA2>();
                    break;
            }
        }

        private void Write( EndianBinaryWriter writer )
        {
            // Skip header
            var headerPos = writer.Position;
            writer.SeekCurrent( 32 );

            // Write data
            writer.BaseOffset = 32;
            var dataStart = writer.Position;
            Resource.Write( writer );
            writer.PerformScheduledWrites();
            writer.WriteAlignmentPadding( 16 );
            var dataEnd = writer.Position;
            var dataSize = dataEnd - dataStart;      

            // Write header
            writer.SeekBegin( headerPos );
            writer.Write( "RES\0", StringBinaryFormat.FixedLength, 4 );
            writer.Write( ( int ) sTypeToResourceType[ Resource.GetType() ] );
            writer.Write( ( int ) dataSize );
            writer.Write( ( int ) writer.OffsetPositions.Count );
            writer.Write( ( int ) dataEnd );

            // Write relocation table
            writer.SeekBegin( dataEnd );
            foreach ( var position in writer.OffsetPositions )
                writer.Write( ( int ) position - 32 );
        }

        public static T Load<T>( string filepath ) where T : ISerializableObject
        {
            var resFile = new ResourceFile( filepath );
            return ( T ) resFile.Resource;
        }

        public static void Save( ISerializableObject res, string filepath )
        {
            var resFile = new ResourceFile( res );
            resFile.Save( filepath );
        }
    }

    public enum ResourceType
    {
        LandTable,
    }
}
