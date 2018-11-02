using SAModelLibrary.IO;

namespace SAModelLibrary
{
    public class TextureReference : ISerializableObject
    {
        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <summary>
        /// Gets or sets the name of the texture being referred to.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of Field04.
        /// </summary>
        public int Field04 { get; set; }

        /// <summary>
        /// Gets or sets the value of Field08.
        /// </summary>
        public int Field08 { get; set; }

        public TextureReference()
        {
        }

        public TextureReference( string name )
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"{Name} {Field04} {Field08}";
        }

        private void Read( EndianBinaryReader reader )
        {
            Name    = reader.ReadStringOffset();
            Field04 = reader.ReadInt32();
            Field08 = reader.ReadInt32();
        }

        private void Write( EndianBinaryWriter writer )
        {
            writer.ScheduleWriteObjectOffset( Name, 4, x => writer.Write( x, StringBinaryFormat.NullTerminated ) );
            writer.Write( Field04 );
            writer.Write( Field08 );
        }

        void ISerializableObject.Read( EndianBinaryReader reader, object context ) => Read( reader );

        void ISerializableObject.Write( EndianBinaryWriter writer, object context ) => Write( writer );
    }
}