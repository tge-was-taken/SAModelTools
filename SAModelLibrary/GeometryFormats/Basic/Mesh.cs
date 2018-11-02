using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using SAModelLibrary.IO;
using SAModelLibrary.Maths;
using SAModelLibrary.Utils;

namespace SAModelLibrary.GeometryFormats.Basic
{
    /// <summary>
    /// Represents a mesh that consists out of a material id to use, a list of primitive and any extra data to be used for them.
    /// </summary>
    public class Mesh : ISerializableObject
    {
        private static readonly BitField sMaterialIdField = new BitField( 0, 13 );
        private static readonly BitField sPrimitiveTypeField = new BitField( 14, 15 );

        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <summary>
        /// Gets or sets the id of the material this mesh uses.
        /// </summary>
        public short MaterialId { get; set; }

        /// <summary>
        /// Gets or sets the primitive type.
        /// </summary>
        public PrimitiveType PrimitiveType { get; set; }

        /// <summary>
        /// Gets or sets the primitives.
        /// </summary>
        public IPrimitive[] Primitives { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        public uint[] Attributes { get; set; }

        /// <summary>
        /// Gets or sets the per-index vertex normals.
        /// </summary>
        public Vector3[] Normals { get; set; }

        /// <summary>
        /// Gets or sets the per-index vertex colors.
        /// </summary>
        public Color[] Colors { get; set; }

        /// <summary>
        /// Gets or sets the per-index uvs.
        /// </summary>
        public Vector2<short>[] UVs { get; set; }

        /// <summary>
        /// Gets whether the mesh has per-index vertex normals or not.
        /// </summary>
        public bool HasNormals => Normals != null;

        /// <summary>
        /// Gets whether the mesh has per-index vertex colors or not.
        /// </summary>
        public bool HasColors => Colors != null;

        /// <summary>
        /// Gets whether the mesh has  per-index UVs or not.
        /// </summary>
        public bool HasUVs => UVs != null;

        public void Read( EndianBinaryReader reader, object context = null )
        {
            UnpackMaterialIdAndPrimitiveType( reader.ReadUInt16() );

            var primitiveCount = reader.ReadUInt16();
            var primitiveIndexCount = 0;
            reader.ReadOffset( () =>
            {
                Primitives = new IPrimitive[primitiveCount];

                for ( int i = 0; i < primitiveCount; i++ )
                {
                    IPrimitive primitive;

                    switch ( PrimitiveType )
                    {
                        case PrimitiveType.Triangles:
                            primitive = reader.ReadObject<Triangle>();
                            primitiveIndexCount += 3;
                            break;
                        case PrimitiveType.Quads:
                            primitive = reader.ReadObject<Quad>();
                            primitiveIndexCount += 4;
                            break;
                        case PrimitiveType.NGons:
                            //primitive = null;
                            //throw new NotImplementedException();
                            primitive           =  reader.ReadObject<Strip>();
                            primitiveIndexCount += ( ( Strip )primitive ).Indices.Length;
                            break;
                        case PrimitiveType.Strips:
                            primitive = reader.ReadObject<Strip>();
                            primitiveIndexCount += ( ( Strip )primitive ).Indices.Length;
                            break;
                        default:
                            throw new InvalidOperationException();
                    }

                    Primitives[i] = primitive;
                }
            } );
            reader.ReadOffset( () => throw new NotImplementedException() );
            reader.ReadOffset( () => Normals = reader.ReadVector3s( primitiveIndexCount ) );

            var colorsOffset = reader.ReadInt32();
            if ( reader.IsValidOffset( colorsOffset ) )
                reader.ReadAtOffset( colorsOffset, () => Colors = reader.ReadColors( primitiveIndexCount ) );

            reader.ReadOffset( () => UVs = reader.ReadVector2Int16s( primitiveIndexCount ) );

            //if ( Colors != null )
            //    Debugger.Break();

            var usesDXLayout = ( bool )context;
            if ( usesDXLayout )
            {
                var unused = reader.ReadInt32();
                if ( unused != 0 )
                {
                    throw new NotImplementedException( "Mesh DX unused field is not 0: " + unused );
                }
            }
        }

        public void Write( EndianBinaryWriter writer, object context = null )
        {
            writer.Write( PackMaterialIdAndPrimitiveType() );
            writer.Write( ( ushort )Primitives.Length );
            writer.ScheduleWriteArrayOffset( Primitives, 16, x => x.Write( writer ) );
            writer.Write( 0 ); // attributes
            writer.ScheduleWriteArrayOffset( Normals, 16, writer.Write );
            writer.ScheduleWriteArrayOffset( Colors, 16, writer.Write );
            writer.ScheduleWriteArrayOffset( UVs, 16, writer.Write );

            var usesDXLayout = ( bool )context;
            if ( usesDXLayout )
                writer.Write( 0 ); // unused
        }

        /// <summary>
        /// Utility method to convert the primitives to triangle vertex indices.
        /// </summary>
        /// <returns></returns>
        public Index[] ToTriangles()
        {
            switch ( PrimitiveType )
            {
                case PrimitiveType.Triangles:
                    {
                        var triangles = new Index[Primitives.Length * 3];
                        var triangleIndex = 0;
                        for ( var i = 0; i < Primitives.Length; i++ )
                        {
                            var triangle = ( Triangle )Primitives[i];
                            triangles[triangleIndex + 0] = GetIndex( triangle.A, triangleIndex + 0 );
                            triangles[triangleIndex + 1] = GetIndex( triangle.B, triangleIndex + 1 );
                            triangles[triangleIndex + 2] = GetIndex( triangle.C, triangleIndex + 2 );
                            triangleIndex += 3;
                        }

                        return triangles;
                    }

                case PrimitiveType.Quads:
                    {
                        var triangles = new Index[( Primitives.Length * 2 ) * 3];
                        var triangleIndex = 0;
                        for ( var i = 0; i < Primitives.Length; i++ )
                        {
                            var quad = ( Quad )Primitives[i];
                            triangles[triangleIndex] = GetIndex( quad.A, ( i * 4 ) + 0 );
                            triangles[triangleIndex + 1] = GetIndex( quad.B, ( i * 4 ) + 1 );
                            triangles[triangleIndex + 2] = GetIndex( quad.C, ( i * 4 ) + 2 );
                            triangles[triangleIndex + 3] = triangles[triangleIndex + 2];
                            triangles[triangleIndex + 4] = GetIndex( quad.D, ( i * 4 ) + 3 );
                            triangles[triangleIndex + 5] = triangles[triangleIndex];
                            triangleIndex += 6;
                        }

                        return triangles;
                    }

                case PrimitiveType.NGons:
                case PrimitiveType.Strips:
                    {
                        var triangleCount = Primitives.Sum( x => ( ( Strip )x ).Indices.Length - 2 );
                        var triangles = new Index[triangleCount * 3];
                        var triangleIndex = 0;
                        var stripIndex = 0;

                        foreach ( Strip strip in Primitives )
                        {
                            var clockwise = !strip.Reversed;
                            var a = GetIndex( strip.Indices[ 0 ], stripIndex + 0 );
                            var b = GetIndex( strip.Indices[ 1 ], stripIndex + 1 );

                            for ( int i = 2; i < strip.Indices.Length; i++ )
                            {
                                var c = GetIndex( strip.Indices[ i ], stripIndex + i );

                                if ( clockwise )
                                {
                                    triangles[triangleIndex + 0] = a;
                                    triangles[triangleIndex + 1] = b;
                                    triangles[triangleIndex + 2] = c;
                                }
                                else
                                {
                                    triangles[triangleIndex + 0] = a;
                                    triangles[triangleIndex + 1] = c;
                                    triangles[triangleIndex + 2] = b;
                                }

                                clockwise = !clockwise;
                                a = b;
                                b = c;
                                triangleIndex += 3;

                            }

                            stripIndex += strip.Indices.Length;
                        }

                        return triangles;
                    }

                default:
                    throw new InvalidOperationException( "Invalid primitive type" );
            }
        }

        private Index GetIndex( int vertexIndex, int i )
        {
            Index index = new Index() { VertexIndex = (ushort)vertexIndex };

            if ( Normals != null )
                index.Normal = Normals[i];

            if ( Colors != null )
                index.Color = Colors[i];

            if ( UVs != null )
                index.UV = UVs[i];

            return index;
        }

        private void UnpackMaterialIdAndPrimitiveType( ushort materialIdAndPrimitiveType )
        {
            MaterialId = ( short )sMaterialIdField.Unpack( materialIdAndPrimitiveType );
            PrimitiveType = ( PrimitiveType )sPrimitiveTypeField.Unpack( materialIdAndPrimitiveType );
        }

        private ushort PackMaterialIdAndPrimitiveType()
        {
            ushort materialIdAndPrimitiveType = 0;
            sMaterialIdField.Pack( ref materialIdAndPrimitiveType, ( ushort )MaterialId );
            sPrimitiveTypeField.Pack( ref materialIdAndPrimitiveType, ( ushort )PrimitiveType );
            return materialIdAndPrimitiveType;
        }
    }

    public struct Index
    {
        public ushort VertexIndex;
        public Vector3 Normal;
        public Color Color;
        public Vector2<short> UV;
    }
}