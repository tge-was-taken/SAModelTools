using SAModelLibrary.GeometryFormats.Basic;
using SAModelLibrary.IO;

namespace SAModelLibrary
{
    /// <summary>
    /// Represents a death zone model used in stages.
    /// </summary>
    public class DeathZone : ISerializableObject
    {
        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <summary>
        /// Gets or sets the flags associated with the death zone.
        /// </summary>
        public uint Flags { get; set; }

        /// <summary>
        /// Gets or sets the root node of the death zone's model hierarchy.
        /// </summary>
        public Node RootNode { get; set; }

        void ISerializableObject.Read( EndianBinaryReader reader, object context )
        {
            Flags = reader.ReadUInt32();
            RootNode = reader.ReadObjectOffset<Node>( new NodeReadContext( GeometryFormat.Basic ) );
        }

        void ISerializableObject.Write( EndianBinaryWriter writer, object context )
        {
            writer.Write( Flags );
            writer.ScheduleWriteObjectOffset( RootNode );
        }
    }
}
