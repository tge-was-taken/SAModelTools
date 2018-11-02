using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using SAModelLibrary.Exceptions;
using SAModelLibrary.IO;
using SAModelLibrary.Maths;

namespace SAModelLibrary.GeometryFormats.GC
{
    /// <summary>
    /// Represents a GC/Wii optimized geometry format.
    /// </summary>
    public class Geometry : IGeometry
    {
        public GeometryFormat Format => GeometryFormat.GC;

        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <summary>
        /// Gets or sets the list of vertex attribute buffers.
        /// </summary>
        public List<VertexAttributeBuffer> VertexBuffers { get; set; }

        /// <summary>
        /// Gets or sets the list of opaque meshes.
        /// </summary>
        public List<Mesh> OpaqueMeshes { get; set; }

        /// <summary>
        /// Gets or sets the list of translucent meshes.
        /// </summary>
        public List<Mesh> TranslucentMeshes { get; set; }

        /// <summary>
        /// Gets or sets the bounding sphere of this geometry.
        /// </summary>
        public BoundingSphere Bounds { get; set; }

        /// <summary>
        /// Creates a new empty instance of <see cref="Geometry"/>.
        /// </summary>
        public Geometry()
        {
            VertexBuffers = new List<VertexAttributeBuffer>();
            OpaqueMeshes = new List<Mesh>();
            TranslucentMeshes = new List<Mesh>();
        }

        /// <summary>
        /// Validates whether or not the given data can possibly be valid geometry data.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static bool Validate( EndianBinaryReader reader )
        {
            var startPosition = reader.Position;

            try
            {
                var vertexAttributeListOffset = reader.ReadInt32();
                var field04                   = reader.ReadInt32();
                var opaqueMeshListOffset      = reader.ReadInt32();
                var translucentMeshListOffset = reader.ReadInt32();
                var opaqueMeshCount           = reader.ReadInt16();
                var translucentMeshCount      = reader.ReadInt16();

                // Must have vertex buffers & should be valid
                if ( vertexAttributeListOffset == 0 || !reader.IsValidOffset( vertexAttributeListOffset ) )
                    return false;

                // Check if the opaque and translucent mesh count together don't make a valid offset
                //if ( reader.IsValidOffset( ( int )opaqueMeshCount | ( int )translucentMeshCount << 16 ) )
                //    return false;
                if ( field04 != 0 )
                    return false;

                // Validate opaque mesh list offset
                if ( !reader.IsValidOffset( opaqueMeshListOffset ) )
                    return false;

                // If opaque mesh list is null then there should be no opaque meshes.
                if ( opaqueMeshListOffset == 0 && opaqueMeshCount != 0 )
                    return false;

                // Sanity check mesh count
                if ( opaqueMeshCount < 0 || opaqueMeshCount >= 10000 )
                    return false;

                // Validate translucent mesh list offset
                if ( !reader.IsValidOffset( translucentMeshListOffset ) )
                    return false;

                // If translucent mesh list is null then there should be no translucent meshes.
                if ( translucentMeshListOffset == 0 && translucentMeshCount != 0 )
                    return false;

                // Sanity check mesh count
                if ( translucentMeshCount < 0 || translucentMeshCount >= 10000 )
                    return false;

                return true;
            }
            finally
            {
                reader.Position = startPosition;
            }
        }

        public void Read( EndianBinaryReader reader, object context = null )
        {
            reader.ReadOffset( ReadVertexAttributes );
            var field04 = reader.ReadInt32();
            Debug.Assert( field04 == 0 );
            var opaqueMeshListOffset = reader.ReadInt32();
            var translucentMeshListOffset = reader.ReadInt32();
            var opaqueMeshCount = reader.ReadInt16();
            var translucentMeshCount = reader.ReadInt16();
            Bounds = reader.ReadBoundingSphere();

            reader.ReadAtOffset( opaqueMeshListOffset, () => OpaqueMeshes = ReadMeshes( reader, opaqueMeshCount ) );
            reader.ReadAtOffset( translucentMeshListOffset, () => TranslucentMeshes = ReadMeshes( reader, translucentMeshCount ) );
        }

        public void Write( EndianBinaryWriter writer, object context = null )
        {
            writer.ScheduleWriteOffsetAligned( 16, () => WriteVertexAttributes( writer ) );
            writer.Write( 0 ); // field04
            writer.ScheduleWriteListOffset( OpaqueMeshes, 16, new MeshContext() );
            writer.ScheduleWriteListOffset( TranslucentMeshes, 16, new MeshContext() );
            writer.Write( ( short )OpaqueMeshes.Count );
            writer.Write( ( short )TranslucentMeshes.Count );
            writer.Write( Bounds );
        }

        private void ReadVertexAttributes( EndianBinaryReader reader )
        {
            VertexBuffers = new List<VertexAttributeBuffer>();

            while ( true )
            {
                var type = ( VertexAttributeType )reader.ReadByte();
                if ( type == VertexAttributeType.End )
                    break;

                var elementSize = reader.ReadByte();
                var elementCount = reader.ReadUInt16();
                var field04 = reader.ReadInt32();
                var dataOffset = reader.ReadUInt32();
                var dataSize = reader.ReadUInt32();

                VertexAttributeBuffer buffer;

                switch ( type )
                {
                    case VertexAttributeType.Position:
                    case VertexAttributeType.Normal:
                        {
                            Vector3[] elements = null;
                            reader.ReadAtOffset( dataOffset, () => elements = reader.ReadVector3s( elementCount ) );

                            if ( type == VertexAttributeType.Position )
                                buffer = new VertexPositionBuffer( elements );
                            else
                                buffer = new VertexNormalBuffer( elements );
                        }
                        break;
                    case VertexAttributeType.Color:
                        {
                            Color[] elements = null;
                            reader.ReadAtOffset( dataOffset, () =>
                            {
                                var endianness = reader.Endianness;
                                reader.Endianness = Endianness.Big;
                                elements = reader.ReadColors( elementCount );
                                reader.Endianness = endianness;
                            } );
                            buffer = new VertexColorBuffer( elements );
                        }
                        break;
                    case VertexAttributeType.UV:
                        {
                            Vector2<short>[] elements = null;
                            reader.ReadAtOffset( dataOffset, () => elements = reader.ReadVector2Int16s( elementCount ) );
                            buffer = new VertexUVBuffer( elements );
                        }
                        break;
                    default:
                        throw new InvalidGeometryDataException( $"Attempted to read invalid/unknown vertex attribute: {type}" );
                }

                Debug.Assert( elementSize == buffer.ElementSize );
                Debug.Assert( elementCount == buffer.ElementCount );
                Debug.Assert( field04 == buffer.Field04 );
                Debug.Assert( dataSize == buffer.DataSize );

                VertexBuffers.Add( buffer );
            }
        }

        private static List<Mesh> ReadMeshes( EndianBinaryReader reader, int meshCount )
        {
            var context = new MeshContext();
            var list = new List<Mesh>( meshCount );
            for ( int i = 0; i < meshCount; i++ )
            {
                var mesh = reader.ReadObject<Mesh>( context );
                list.Add( mesh );
            }

            return list;
        }

        private void WriteVertexAttributes( EndianBinaryWriter writer )
        {
            foreach ( var buffer in VertexBuffers )
            {
                writer.Write( ( byte ) buffer.Type );
                writer.Write( buffer.ElementSize );
                writer.Write( buffer.ElementCount );
                writer.Write( buffer.Field04 );

                switch ( buffer.Type )
                {
                    case VertexAttributeType.Position:
                    case VertexAttributeType.Normal:
                        writer.ScheduleWriteObjectOffset( ( ( VertexAttributeBuffer<Vector3> ) buffer ).Elements, 16, writer.Write );
                        break;
                    case VertexAttributeType.Color:

                        writer.ScheduleWriteObjectOffset( ( ( VertexAttributeBuffer<Color> )buffer ).Elements, 16, x =>
                        {
                            var endianness = writer.Endianness;
                            writer.Endianness = Endianness.Big;
                            writer.Write( x );
                            writer.Endianness = endianness;
                        } );
                        break;
                    case VertexAttributeType.UV:
                        writer.ScheduleWriteObjectOffset( ( ( VertexAttributeBuffer<Vector2<short>> )buffer ).Elements, 16, writer.Write );
                        break;
                }

                writer.Write( buffer.DataSize );
            }

            // Write end entry
            writer.Write( ( byte ) VertexAttributeType.End );
            writer.Write( ( byte ) 0 );
            writer.Write( ( short ) 0 );
            writer.Write( 0 );
            writer.Write( 0 );
            writer.Write( 0 );
        }
    }

    public class MeshContext
    {
        public IndexAttributeFlags IndexAttributeFlags { get; set; }
    }
}
