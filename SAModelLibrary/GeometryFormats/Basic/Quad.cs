using SAModelLibrary.IO;

namespace SAModelLibrary.GeometryFormats.Basic
{
    /// <summary>
    /// Quad primitive type consisting out of 4 vertices.
    /// </summary>
    public struct Quad : IPrimitive
    {
        /// <inheritdoc />
        public PrimitiveType PrimitiveType => PrimitiveType.Quads;

        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <summary>
        /// First vertex index.
        /// </summary>
        public ushort A;

        /// <summary>
        /// Second vertex index.
        /// </summary>
        public ushort B;

        /// <summary>
        /// Third vertex index.
        /// </summary>
        public ushort C;

        /// <summary>
        /// Fourth vertex index.
        /// </summary>
        public ushort D;

        /// <inheritdoc />
        public void Read( EndianBinaryReader reader, object context = null )
        {
            A = reader.ReadUInt16();
            B = reader.ReadUInt16();
            C = reader.ReadUInt16();
            D = reader.ReadUInt16();
        }

        /// <inheritdoc />
        public void Write( EndianBinaryWriter writer, object context = null )
        {
            writer.Write( A );
            writer.Write( B );
            writer.Write( C );
            writer.Write( D );
        }
    }
}