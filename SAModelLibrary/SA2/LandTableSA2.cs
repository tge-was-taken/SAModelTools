using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SAModelLibrary.IO;

namespace SAModelLibrary.SA2
{
    /// <summary>
    /// Represents the structure of a land table, a table containing info about the models that make up a stage.
    /// </summary>
    public class LandTableSA2 : ILandTable
    {
        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <summary>
        /// Gets the number of models in the land table.
        /// </summary>
        public short ModelCount => ( short ) Models.Count;

        /// <summary>
        /// Gets the number of visible models in the land table.
        /// </summary>
        public short VisibleModelCount => ( short ) Models.Count( x => !x.Flags.HasFlag( SurfaceFlags.Collidable ) );

        /// <summary>
        /// Gets or sets the value of Field04.
        /// </summary>
        public short Field04 { get; set; }

        /// <summary>
        /// Gets or sets the value of Field04.
        /// </summary>
        public short Field06 { get; set; }

        /// <summary>
        /// Gets or sets the value of Field08. Maybe a flag?
        /// </summary>
        public int Field08 { get; set; }

        /// <summary>
        /// Gets or sets the model cull range.
        /// </summary>
        public float CullRange { get; set; }

        /// <summary>
        /// Gets or sets the models contained within the land table.
        /// </summary>
        public List<LandModelSA2> Models { get; set; }

        /// <summary>
        /// Gets or sets the texture package file name.
        /// </summary>
        public string TexturePakFileName { get; set; }

        /// <summary>
        /// Gets or sets the list of texture references.
        /// </summary>
        public TextureReferenceList Textures { get; set; }

        IEnumerable<ILandModel> ILandTable.Models => Models;

        /// <summary>
        /// Initialize a new empty instance of <see cref="LandTableSA2"/> with default values.
        /// </summary>
        public LandTableSA2()
        {
            Field04 = 0; // constant
            Field06 = -1; // constant
            Field08 = 1; // constant
            CullRange = 3000f; // almost always this value
            Models = new List<LandModelSA2>();
            Textures = new TextureReferenceList();
        }

        public LandTableSA2( string filepath )
        {
            using ( var reader = new EndianBinaryReader( filepath, Endianness.Little ) )
            {
                //SourceEndianness = DetectEndianness( reader );
                Read( reader );
            }
        }

        public LandTableSA2( Stream stream, bool leaveOpen )
        {
            using ( var reader = new EndianBinaryReader( stream, leaveOpen, Endianness.Little ) )
            {
                //SourceEndianness = DetectEndianness( reader );
                Read( reader );
            }
        }

        public void Save( string filepath ) => Save( filepath, SourceEndianness );

        public void Save( string filepath, Endianness endianness )
        {
            using ( var writer = new EndianBinaryWriter( filepath, endianness ) )
            {
                Write( writer );
            }
        }

        private void Read( EndianBinaryReader reader )
        {
            var modelCount = reader.ReadInt16();
            var visibleModelCount = reader.ReadInt16(); // can be -1, see objLandTable0000
            Field04 = reader.ReadInt16();
            Field06 = reader.ReadInt16();
            Field08 = reader.ReadInt32();
            CullRange = reader.ReadSingle();
            reader.ReadOffset( () =>
            {
                Models = new List<LandModelSA2>( modelCount );
                for ( int i = 0; i < modelCount; i++ )
                    Models.Add( reader.ReadObject<LandModelSA2>() );

                var actualVisibleModelCount = Models.Count( x => !x.Flags.HasFlag( SurfaceFlags.Collidable ) );
                if ( visibleModelCount != -1 && !( visibleModelCount > modelCount ) ) // 1 land table is bugged
                    Debug.Assert( visibleModelCount == actualVisibleModelCount );

            } );
            reader.ReadOffset( () => throw new NotImplementedException() );

            // Hack(TGE): strings are stored in the rdata segment, and thus the base offset is different.
            // Maybe solve this with an address resolver function in the reader.
            var baseOffset  = reader.BaseOffset;
            var baseOffset2 = baseOffset;
            if ( baseOffset == -0x10002000 )
                baseOffset2 = -0x10001200;

            reader.BaseOffset = baseOffset2;
            TexturePakFileName = reader.ReadStringOffset();
            reader.BaseOffset = baseOffset;

            Textures = reader.ReadObjectOffset<TextureReferenceList>( baseOffset2 );
        }

        private void Write( EndianBinaryWriter writer )
        {
            writer.Write( ModelCount );
            writer.Write( VisibleModelCount );
            writer.Write( Field04 );
            writer.Write( Field06 );
            writer.Write( Field08 );
            writer.Write( CullRange );
            writer.ScheduleWriteObjectOffset( Models, 16, x => x.ForEach( y => writer.WriteObject( y ) ) );
            writer.Write( 0 );
            writer.ScheduleWriteObjectOffset( TexturePakFileName, 4, x => writer.Write( x, StringBinaryFormat.NullTerminated ) );
            writer.ScheduleWriteObjectOffset( Textures );
        }

        void ISerializableObject.Read( EndianBinaryReader reader, object context ) => Read( reader );

        void ISerializableObject.Write( EndianBinaryWriter writer, object context ) => Write( writer );
    }
}
