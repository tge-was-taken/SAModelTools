using SAModelLibrary.IO;

namespace SAModelLibrary.GeometryFormats.Chunk
{
    /// <summary>
    /// Common interface for all chunks.
    /// </summary>
    public interface IChunk
    {
        /// <summary>
        /// Gets the chunk type of this chunk.
        /// </summary>
        ChunkType Type { get; }
    }

    /// <summary>
    /// Abstract class for all chunks.
    /// </summary>
    public abstract class Chunk : IChunk
    {
        /// <inheritdoc />
        public abstract ChunkType Type { get; }

        /// <summary>
        /// Gets the flag byte to write to the chunk header.
        /// </summary>
        protected abstract byte GetFlags();

        /// <summary>
        /// Read method for chunks.
        /// <remark>The reader is positioned *after* the chunk header, so do not read any of the chunk header values with this.</remark>
        /// </summary>
        /// <param name="size">Size of the chunk data. -1 if not present.</param>
        /// <param name="flags">Flags associated with the chunk being read.</param>
        /// <param name="reader"></param>
        internal abstract void ReadBody( int size, byte flags, EndianBinaryReader reader );

        /// <summary>
        /// Write method for chunks. Writes the header and the body of the chunk.
        /// </summary>
        /// <param name="writer"></param>
        internal abstract void Write( EndianBinaryWriter writer );

        /// <summary>
        /// Write method for chunks. Writes the body of the chunk.
        /// <remark>The writer is positioned *after* the chunk header, so do not write any of the chunk header values with this.</remark>
        /// </summary>
        /// <param name="writer"></param>
        internal abstract void WriteBody( EndianBinaryWriter writer );
    }
}