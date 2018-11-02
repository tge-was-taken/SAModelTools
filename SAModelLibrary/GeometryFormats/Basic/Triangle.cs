using SAModelLibrary.IO;

namespace SAModelLibrary.GeometryFormats.Basic
{
    /// <summary>
    /// Represents a triangle primitive type. 3 vertices are used to form a triangle.
    /// </summary>
    public struct Triangle : IPrimitive
    {
        /// <inheritdoc />
        public PrimitiveType PrimitiveType => PrimitiveType.Triangles;

        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <summary>
        /// The first vertex index.
        /// </summary>
        public ushort A;

        /// <summary>
        /// The second vertex index.
        /// </summary>
        public ushort B;

        /// <summary>
        /// The third vertex index.
        /// </summary>
        public ushort C;

        /// <inheritdoc />
        public void Read( EndianBinaryReader reader, object context = null )
        {
            A = reader.ReadUInt16();
            B = reader.ReadUInt16();
            C = reader.ReadUInt16();
        }

        /// <inheritdoc />
        public void Write( EndianBinaryWriter writer, object context = null )
        {
            writer.Write( A );
            writer.Write( B );
            writer.Write( C );
        }
    }
}