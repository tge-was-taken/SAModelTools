using SAModelLibrary.IO;

namespace SAModelLibrary.SA2
{
    /// <summary>
    /// Represents a model in the land table.
    /// </summary>
    public class LandModelSA2 : ILandModel
    {
        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <inheritdoc />
        public BoundingSphere Bounds { get; set; }

        /// <inheritdoc />
        public Node RootNode { get; set; }

        /// <summary>
        /// Gets or sets Field14. Purpose unknown.
        /// </summary>
        public int Field14 { get; set; }

        /// <summary>
        /// Gets or sets Field18. Purpose unknown.
        /// </summary>
        public int Field18 { get; set; }

        /// <inheritdoc />
        public SurfaceFlags Flags { get; set; }

        /// <summary>
        /// Initialize a new instance of <see cref="LandModelSA2"/> with default values.
        /// </summary>
        public LandModelSA2()
        {
        }

        private void Read( EndianBinaryReader reader )
        {
            Bounds = reader.ReadBoundingSphere();
            RootNode       = reader.ReadObjectOffset<Node>();
            Field14        = reader.ReadInt32();
            Field18        = reader.ReadInt32();
            Flags          = ( SurfaceFlags )reader.ReadInt32();
        }

        private void Write( EndianBinaryWriter writer )
        {
            writer.Write( Bounds );
            writer.ScheduleWriteObjectOffset( RootNode );
            writer.Write( Field14 );
            writer.Write( Field18 );
            writer.Write( (uint)Flags );
        }

        void ISerializableObject.Read( EndianBinaryReader  reader, object context ) => Read( reader );
        void ISerializableObject.Write( EndianBinaryWriter writer, object context ) => Write( writer );
    }
}