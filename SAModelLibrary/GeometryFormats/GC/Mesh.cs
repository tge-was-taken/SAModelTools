using System;
using System.Collections.Generic;
using System.Diagnostics;
using SAModelLibrary.IO;

namespace SAModelLibrary.GeometryFormats.GC
{
    public class Mesh : ISerializableObject
    {
        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <summary>
        /// Gets or sets the list of mesh parameters.
        /// </summary>
        public List<Param> Parameters { get; set; }

        /// <summary>
        /// Gets or sets the list of GX display lists.
        /// </summary>
        public List<GXDisplayList> DisplayLists { get; set; }

        /// <summary>
        /// Creates a new empty instance of <see cref="Mesh"/>.
        /// </summary>
        public Mesh()
        {
            Parameters   = new List<Param>();
            DisplayLists = new List<GXDisplayList>();
        }

        public void Read( EndianBinaryReader reader, MeshContext context )
        {
            var meshParamListOffset = reader.ReadInt32();
            var meshParamCount      = reader.ReadInt32();
            var displayListOffset   = reader.ReadInt32();
            var displayListSize     = reader.ReadInt32();

            reader.ReadAtOffset( meshParamListOffset, () =>
            {
                Parameters = new List<Param>();
                for ( int i = 0; i < meshParamCount; i++ )
                {
                    var type = ( MeshStateParamType ) reader.ReadInt32();
                    Param param;

                    switch ( type )
                    {
                        case MeshStateParamType.IndexAttributeFlags:
                            param = new IndexAttributeFlagsParam();
                            break;
                        case MeshStateParamType.Lighting:
                            param = new LightingParams();
                            break;
                        case MeshStateParamType.BlendAlpha:
                            param = new BlendAlphaParam();
                            break;
                        case MeshStateParamType.AmbientColor:
                            param = new AmbientColorParam();
                            break;
                        case MeshStateParamType.Texture:
                            param = new TextureParams();
                            break;
                        case MeshStateParamType.MipMap:
                            param = new MipMapParams();
                            break;
                        default:
                            param = new UnknownParam();
                            break;
                    }

                    param.ReadBody( type, reader );
                    Parameters.Add( param );
                }
            } );

            // Hack(TGE): look up index attributes flag in params to parse display lists
            foreach ( var param in Parameters )
            {
                if ( param.Type == MeshStateParamType.IndexAttributeFlags )
                    context.IndexAttributeFlags = ( ( IndexAttributeFlagsParam )param ).Flags;
            }

            reader.ReadAtOffset( displayListOffset, () =>
            {
                DisplayLists = new List<GXDisplayList>();
                var endPosition = reader.Position + displayListSize;
                while ( reader.ReadByte() != 0 && reader.Position < endPosition )
                {
                    reader.SeekCurrent( -1 );
                    var displayList = reader.ReadObject<GXDisplayList>( context.IndexAttributeFlags );
                    DisplayLists.Add( displayList );
                }
            });
        }

        public void Write( EndianBinaryWriter writer, MeshContext context )
        {
            writer.ScheduleWriteListOffset( Parameters, 16, x => x.Write( writer ) );
            writer.Write( Parameters.Count );

            // Hack(TGE): look up index attributes flag in params to parse display lists
            foreach ( var param in Parameters )
            {
                if ( param.Type == MeshStateParamType.IndexAttributeFlags )
                    context.IndexAttributeFlags = ( ( IndexAttributeFlagsParam )param ).Flags;
            }

            // Make sure to make a local copy of it for the display list write function because it's executed later
            var indexAttributeFlags = context.IndexAttributeFlags;

            var displayListSizeOffset = writer.Position + 4;
            writer.ScheduleWriteOffsetAligned( 16, () =>
            {
                var displayListStart = writer.Position;

                // Write display lists
                foreach ( var displayList in DisplayLists )
                    writer.WriteObject( displayList, indexAttributeFlags );

                writer.Write( ( byte ) 0 );
                writer.WriteAlignmentPadding( 16 );

                // Calculate & write display list size
                var displayListEnd = writer.Position;
                var displayListSize = displayListEnd - displayListStart;
                writer.SeekBegin( displayListSizeOffset );
                writer.Write( ( int ) displayListSize );
                writer.SeekBegin( displayListEnd );
            } );
            writer.Write( 0 ); // display list size
        }

        void ISerializableObject.Read( EndianBinaryReader reader, object context ) => Read( reader, ( MeshContext ) context );

        void ISerializableObject.Write( EndianBinaryWriter writer, object context ) => Write( writer, ( MeshContext ) context );
    }
}