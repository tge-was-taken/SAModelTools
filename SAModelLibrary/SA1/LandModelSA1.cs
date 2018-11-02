using SAModelLibrary.IO;

namespace SAModelLibrary.SA1
{
    /// <summary>
    /// Represents a land model used in SA1/SADX land tables.
    /// </summary>
    public class LandModelSA1 : ILandModel
    {
        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <inheritdoc />
        public BoundingSphere Bounds { get; set; }

        /// <summary>
        /// Gets or sets the value of Field10.
        /// </summary>
        public int Field10 { get; set; }

        /// <summary>
        /// Gets or sets the value of Field14.
        /// </summary>
        public int Field14 { get; set; }

        /// <inheritdoc />
        public Node RootNode { get; set; }

        /// <summary>
        /// Gets or sets the value of Field1C.
        /// </summary>
        public int Field1C { get; set; }

        /// <inheritdoc />
        public SurfaceFlags Flags { get; set; }

        /// <summary>
        /// Initializes a new empty instance of <see cref="LandModelSA1"/>.
        /// </summary>
        public LandModelSA1()
        {
            
        }

        void ISerializableObject.Read( EndianBinaryReader reader, object context )
        {
            Bounds   = reader.ReadBoundingSphere();
            Field10  = reader.ReadInt32();
            Field14  = reader.ReadInt32();
            RootNode = reader.ReadObjectOffset<Node>( new NodeReadContext( GeometryFormat.BasicDX ) );
            Field1C  = reader.ReadInt32();
            Flags    = ( SurfaceFlags )reader.ReadInt32();
        }

        void ISerializableObject.Write( EndianBinaryWriter writer, object context )
        {
            writer.Write( Bounds );
            writer.Write( Field10 );
            writer.Write( Field14 );
            writer.ScheduleWriteObjectOffset( RootNode );
            writer.Write( Field1C );
            writer.Write( ( int ) Flags );
        }
    }
}