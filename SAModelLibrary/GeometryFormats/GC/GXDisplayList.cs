using System;
using System.Collections.Generic;
using System.Diagnostics;
using SAModelLibrary.Exceptions;
using SAModelLibrary.IO;
using SAModelLibrary.Utils;

namespace SAModelLibrary.GeometryFormats.GC
{
    /// <summary>
    /// Represents a GX primitive display list.
    /// </summary>
    public class GXDisplayList : ISerializableObject
    {
        private static readonly BitField sVertexStreamIndexField = new BitField( 0, 2 );
        private static readonly BitField sPrimitiveTypeField     = new BitField( 3, 7 );

        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <summary>
        /// Gets or sets the vertex stream index. 
        /// Only supported value is 0.
        /// </summary>
        public byte VertexStreamIndex { get; private set; }

        /// <summary>
        /// Gets the GX primitive type of this display list.
        /// </summary>
        public GXPrimitive PrimitiveType { get; private set; }

        /// <summary>
        /// Gets the indices stored within the display list.
        /// </summary>
        public Index[] Indices { get; private set; }

        /// <summary>
        /// Creates a new empty display list. Internal use only.
        /// </summary>
        public GXDisplayList()
        {
        }

        /// <summary>
        /// Create a new display list given a primitive type and an array of indices to use. Mind that the face order is flipped!.
        /// </summary>
        /// <param name="primitiveType"></param>
        /// <param name="indices"></param>
        public GXDisplayList( GXPrimitive primitiveType, Index[] indices )
        {
            if ( indices == null )
                throw new ArgumentNullException( nameof( indices ), "Indices array can not be null" );

            if ( indices.Length > ushort.MaxValue )
                throw new ArgumentException( "Exceeded max supported number of indices", nameof( indices ) );

            switch ( primitiveType )
            {
                case GXPrimitive.Triangles:
                    if ( ( indices.Length % 3 ) != 0 )
                        throw new ArgumentException( "Number of indices is not a multiple of 3 (as required for a triangle)", nameof( indices ) );
                    break;
                case GXPrimitive.TriangleStrip:
                    if ( indices.Length < 3 )
                        throw new ArgumentException( "Indices do not make a valid triangle strip (less than 3 indices)", nameof( indices ) );
                    break;
                case GXPrimitive.TriangleFan:
                    if ( indices.Length < 2 )
                        throw new ArgumentException( "Indices do not make a valid triangle fan (less than 2 indices)", nameof( indices ) );
                    break;
                case GXPrimitive.Quads:
                    if ( ( indices.Length % 4 ) != 0 )
                        throw new ArgumentException( "Number of indices is not a multiple of 4 (as required for a quad)", nameof( indices ) );
                    break;
                default:
                    throw new ArgumentOutOfRangeException( nameof( primitiveType ), primitiveType, "Unsupported/invalid primitive type specified" );
            }

            PrimitiveType = primitiveType;
            Indices       = indices;
        }

        /// <summary>
        /// Utility function to convert the indices into triangle indices and flips them.
        /// </summary>
        /// <returns></returns>
        public Index[] ToTriangles()
        {
            // TODO(TGE): Should flipping be done here or in the read/write code?
            // Flipping a triangle strip is not trivial.

            switch ( PrimitiveType )
            {
                case GXPrimitive.Triangles:
                    {
                        var indices = new Index[Indices.Length];
                        for ( int i = 0; i < Indices.Length; i += 3 )
                        {
                            indices[i]     = Indices[i + 2];
                            indices[ i + 1 ] = Indices[ i + 1 ];
                            indices[ i + 2 ] = Indices[ i ];
                        }

                        return indices;
                    }

                case GXPrimitive.TriangleStrip:
                    {
                        var indices = new List<Index>();
                        var a = Indices[ 0 ];
                        var b = Indices[ 1 ];
                        var clockwise = true;

                        for ( int i = 2; i < Indices.Length; i++ )
                        {
                            var c = Indices[i];

                            if ( clockwise )
                            {
                                indices.Add( a ); 
                                indices.Add( b ); 
                                indices.Add( c );  
                            }
                            else
                            {
                                indices.Add( a );
                                indices.Add( c );
                                indices.Add( b );
                            }

                            a = b;
                            b = c;
                            clockwise = !clockwise;
                        }

                        for ( int i = 0; i < indices.Count; i += 3 )
                        {
                            var temp = indices[i];
                            indices[i] = indices[i + 2];
                            indices[i + 2] = temp;
                        }

                        return indices.ToArray();
                    }

                case GXPrimitive.TriangleFan:
                    {
                        var indices = new List<Index>();
                        var center  = Indices[0];
                        for ( int i = 1; i < Indices.Length - 1; i++ )
                        {
                            indices.Add( center );
                            indices.Add( Indices[i - 1] );
                            indices.Add( Indices[i] );
                        }
                        return indices.ToArray();
                    }

                case GXPrimitive.Quads:
                    {
                        var indices = new List<Index>();
                        for ( int i = 0; i < Indices.Length; i += 4 )
                        {
                            indices.Add( Indices[i] );
                            indices.Add( Indices[i + 1] );
                            indices.Add( Indices[i + 2] );

                            indices.Add( Indices[i + 2] );
                            indices.Add( Indices[i + 3] );
                            indices.Add( Indices[i] );
                        }
                        return indices.ToArray();
                    }

                default:
                    throw new InvalidOperationException( $"Can't convert primitive type {PrimitiveType} to triangles!" );
            }
        }

        private void Read( EndianBinaryReader reader, IndexAttributeFlags flags )
        {
            // always big endian
            var endianness = reader.Endianness;
            reader.Endianness = Endianness.Big;

            UnpackFlags( reader.ReadByte() );
            var indexCount = reader.ReadUInt16();

            Indices = new Index[indexCount];
            for ( int i = 0; i < indexCount; i++ )
            {
                ref var index = ref Indices[ i ];

                if ( flags.HasFlag( IndexAttributeFlags.HasPosition ) )
                    index.PositionIndex = flags.HasFlag( IndexAttributeFlags.Position16BitIndex ) ? reader.ReadUInt16() : reader.ReadByte();
                else
                    index.PositionIndex = ushort.MaxValue;

                if ( flags.HasFlag( IndexAttributeFlags.HasNormal ) )
                    index.NormalIndex = flags.HasFlag( IndexAttributeFlags.Normal16BitIndex ) ? reader.ReadUInt16() : reader.ReadByte();
                else
                    index.NormalIndex = ushort.MaxValue;

                if ( flags.HasFlag( IndexAttributeFlags.HasColor ) )
                    index.ColorIndex = flags.HasFlag( IndexAttributeFlags.Color16BitIndex ) ? reader.ReadUInt16() : reader.ReadByte();
                else
                    index.ColorIndex = ushort.MaxValue;

                if ( flags.HasFlag( IndexAttributeFlags.HasUV ) )
                    index.UVIndex = flags.HasFlag( IndexAttributeFlags.UV16BitIndex ) ? reader.ReadUInt16() : reader.ReadByte();
                else
                    index.UVIndex = ushort.MaxValue;
            }

            // restore endianness
            reader.Endianness = endianness;
        }

        private void Write( EndianBinaryWriter writer, IndexAttributeFlags flags )
        {
            // always big endian
            var endianness = writer.Endianness;
            writer.Endianness = Endianness.Big;

            writer.Write( PackFlags() );
            writer.Write( ( ushort ) Indices.Length );

            foreach ( var index in Indices )
            {
                if ( flags.HasFlag( IndexAttributeFlags.HasPosition ) )
                {
                    if ( flags.HasFlag( IndexAttributeFlags.Position16BitIndex ) )
                        writer.Write( index.PositionIndex );
                    else
                        writer.Write( ( byte )index.PositionIndex );
                }

                if ( flags.HasFlag( IndexAttributeFlags.HasNormal ) )
                {
                    if ( flags.HasFlag( IndexAttributeFlags.Normal16BitIndex ) )
                        writer.Write( index.NormalIndex );
                    else
                        writer.Write( ( byte )index.NormalIndex );
                }

                if ( flags.HasFlag( IndexAttributeFlags.HasColor ) )
                {
                    if ( flags.HasFlag( IndexAttributeFlags.Color16BitIndex ) )
                        writer.Write( index.ColorIndex );
                    else
                        writer.Write( ( byte )index.ColorIndex );
                }

                if ( flags.HasFlag( IndexAttributeFlags.HasUV ) )
                {
                    if ( flags.HasFlag( IndexAttributeFlags.UV16BitIndex ) )
                        writer.Write( index.UVIndex );
                    else
                        writer.Write( ( byte )index.UVIndex );
                }
            }

            // restore endianness
            writer.Endianness = endianness;
        }

        private void UnpackFlags( byte flags )
        {
            VertexStreamIndex = sVertexStreamIndexField.Unpack( flags );
            PrimitiveType     = ( GXPrimitive )( sPrimitiveTypeField.Unpack( flags ) << sPrimitiveTypeField.From );
            Debug.Assert( PackFlags() == flags );
            Debug.Assert( VertexStreamIndex == 0 );

            if ( PrimitiveType != GXPrimitive.Quads && PrimitiveType != GXPrimitive.TriangleFan && PrimitiveType != GXPrimitive.Triangles &&
                 PrimitiveType != GXPrimitive.TriangleStrip )
                throw new InvalidGeometryDataException();
        }

        private byte PackFlags()
        {
            byte flags = 0;
            sVertexStreamIndexField.Pack( ref flags, VertexStreamIndex );
            sPrimitiveTypeField.Pack( ref flags, (byte)( ((byte)PrimitiveType) >> sPrimitiveTypeField.From ) );
            return flags;
        }

        void ISerializableObject.Read( EndianBinaryReader reader, object context ) => Read( reader, ( IndexAttributeFlags ) context );

        void ISerializableObject.Write( EndianBinaryWriter writer, object context ) => Write( writer, ( IndexAttributeFlags ) context );
    }
}