using SAModelLibrary.IO;

namespace SAModelLibrary.GeometryFormats.Basic
{
    /// <summary>
    /// Represents a triangle strip primitive type. 
    /// 2 base vertices are used to construct a new triangle for every vertex that follows.
    /// </summary>
    public class Strip : IPrimitive
    {
        /// <inheritdoc />
        public PrimitiveType PrimitiveType => PrimitiveType.Strips;

        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <summary>
        /// Gets or sets if the initial winding order of the strip is reversed.
        /// </summary>
        public bool Reversed { get; set; }

        /// <summary>
        /// Gets or sets the vertex indices to be used in the strip.
        /// </summary>
        public ushort[] Indices { get; set; }

        /// <inheritdoc />
        public void Read( EndianBinaryReader reader, object context = null )
        {
            var indexCount = ( int ) reader.ReadInt16();
            if ( ( indexCount & 0x8000 ) != 0 )
            {
                Reversed   =  true;
                indexCount &= 0x7FFF;
            }

            Indices = reader.ReadUInt16s( indexCount );
        }

        /// <inheritdoc />
        public void Write( EndianBinaryWriter writer, object context = null )
        {
            var indexCount = Indices.Length;
            if ( Reversed )
                indexCount |= 0x8000;

            writer.Write( ( short ) indexCount );
            writer.Write( Indices );
        }
    }
}