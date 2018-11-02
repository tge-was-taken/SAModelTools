using System.Collections.Generic;
using SAModelLibrary.IO;
using SAModelLibrary.SA2;
using GC = SAModelLibrary.GeometryFormats.GC;
using Basic = SAModelLibrary.GeometryFormats.Basic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using SAModelLibrary.Maths;

namespace SAModelLibrary.SA1
{
    /// <summary>
    /// Represents the structure of a land table used in SA1, a table containing info about the models that make up a stage.
    /// </summary>
    public class LandTableSA1 : ILandTable
    {
        /// <inheritdoc />
        public string SourceFilePath { get; set; }

        /// <inheritdoc />
        public long SourceOffset { get; set; }

        /// <inheritdoc />
        public Endianness SourceEndianness { get; set; }

        /// <inheritdoc />
        public short ModelCount => ( short )Models.Count;

        /// <summary>
        /// Gets or sets the value of Field04.
        /// </summary>
        public int Field04 { get; set; }

        /// <inheritdoc />
        public float CullRange { get; set; }

        /// <summary>
        /// Gets or sets the list of models contained within the land table.
        /// </summary>
        public List<LandModelSA1> Models { get; set; }

        /// <inheritdoc />
        public string TexturePakFileName { get; set; }

        /// <inheritdoc />
        public TextureReferenceList Textures { get; set; }

        IEnumerable<ILandModel> ILandTable.Models => Models;

        /// <summary>
        /// Initialize a new empty instance of <see cref="LandTableSA1"/> with default values.
        /// </summary>
        public LandTableSA1()
        {
            Models = new List<LandModelSA1>();
        }

        /// <summary>
        /// Converts the SA1 land table to SA2 format.
        /// </summary>
        /// <param name="texturePakFileName">The filename of the texture pak to use.</param>
        /// <returns></returns>
        public LandTableSA2 ConvertToSA2Format( string texturePakFileName )
        {
            var sa2LandTable = new LandTableSA2();

            foreach ( var sa1Model in Models )
            {
                if ( sa1Model.Flags.HasFlag( SurfaceFlags.Visible ) )
                {
                    var newModel = new LandModelSA2
                    {
                        Bounds = sa1Model.Bounds,
                        Flags = SurfaceFlags.Visible | sa1Model.Flags & ~( SurfaceFlags.Collidable ),
                        RootNode = new Node
                        {
                            Flags = sa1Model.RootNode.Flags,
                            Geometry = ConvertBasicToGCGeometry( ( Basic.Geometry )sa1Model.RootNode.Geometry ),
                            Rotation = sa1Model.RootNode.Rotation,
                            Scale = sa1Model.RootNode.Scale,
                            Translation = sa1Model.RootNode.Translation,
                        }
                    };
                    sa2LandTable.Models.Add( newModel );
                }

                if ( sa1Model.Flags.HasFlag( SurfaceFlags.Collidable ) )
                {
                    var newCollisionModel = new LandModelSA2
                    {
                        Bounds = sa1Model.Bounds,
                        Flags = sa1Model.Flags & ~( SurfaceFlags.Visible | ~( SurfaceFlags.Water | SurfaceFlags.Collidable ) ),
                        RootNode = sa1Model.RootNode,
                    };
                    var geometry = ( Basic.Geometry )newCollisionModel.RootNode.Geometry;
                    geometry.UsesDXLayout = false;

                    sa2LandTable.Models.Add( newCollisionModel );
                }
            }

            sa2LandTable.Models = sa2LandTable.Models.OrderBy( x => x.Flags.HasFlag( SurfaceFlags.Collidable ) ).ToList();
            sa2LandTable.CullRange = CullRange;
            sa2LandTable.TexturePakFileName = texturePakFileName;
            sa2LandTable.Textures = Textures;

            return sa2LandTable;
        }

        private static GC.Geometry ConvertBasicToGCGeometry( Basic.Geometry basicGeometry )
        {
            var geometry = new GC.Geometry();
            geometry.VertexBuffers.Add( new GC.VertexPositionBuffer( basicGeometry.VertexPositions ) );

            var positionFlags = GC.IndexAttributeFlags.HasPosition;
            if ( basicGeometry.VertexCount > byte.MaxValue )
                positionFlags |= GC.IndexAttributeFlags.Position16BitIndex;

            var indexFlagsParams = new List<GC.IndexAttributeFlagsParam>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2<short>>();
            var colors = new List<Color>();

            foreach ( var basicMesh in basicGeometry.Meshes )
            {
                var basicMaterial = basicGeometry.Materials[basicMesh.MaterialId];
                var mesh = new GC.Mesh();

                var indexFlags = positionFlags;
                var useColors = false;
                var hasColors = basicMesh.Colors != null;
                var hasUVs = basicMesh.UVs != null;
                var hasVertexNormals = basicGeometry.VertexNormals != null;
                var hasNormals = hasVertexNormals || basicMesh.Normals != null;

                if ( hasColors || !hasNormals )
                {
                    indexFlags |= GC.IndexAttributeFlags.HasColor;
                    useColors = true;
                }
                else
                {
                    indexFlags |= GC.IndexAttributeFlags.HasNormal;
                }

                if ( hasUVs )
                {
                    indexFlags |= GC.IndexAttributeFlags.HasUV;
                }

                //Debug.Assert( indexFlags == ( GC.IndexAttributeFlags.HasPosition | GC.IndexAttributeFlags.HasNormal ) ||
                //              indexFlags == ( GC.IndexAttributeFlags.Position16BitIndex | GC.IndexAttributeFlags.HasPosition | GC.IndexAttributeFlags.Normal16BitIndex | GC.IndexAttributeFlags.HasNormal ) ||
                //              indexFlags == ( GC.IndexAttributeFlags.HasPosition | GC.IndexAttributeFlags.HasColor ) ||
                //              indexFlags == ( GC.IndexAttributeFlags.Position16BitIndex | GC.IndexAttributeFlags.HasPosition | GC.IndexAttributeFlags.Color16BitIndex | GC.IndexAttributeFlags.HasColor ) ||
                //              indexFlags == ( GC.IndexAttributeFlags.HasPosition | GC.IndexAttributeFlags.HasNormal | GC.IndexAttributeFlags.HasUV ) ||
                //              indexFlags == ( GC.IndexAttributeFlags.Position16BitIndex | GC.IndexAttributeFlags.HasPosition | GC.IndexAttributeFlags.Normal16BitIndex | GC.IndexAttributeFlags.HasNormal | GC.IndexAttributeFlags.HasUV ) ||
                //              indexFlags == ( GC.IndexAttributeFlags.HasPosition | GC.IndexAttributeFlags.HasColor | GC.IndexAttributeFlags.HasUV ) ||
                //              indexFlags == ( GC.IndexAttributeFlags.Position16BitIndex | GC.IndexAttributeFlags.HasPosition | GC.IndexAttributeFlags.Color16BitIndex | GC.IndexAttributeFlags.HasColor | GC.IndexAttributeFlags.HasUV ) ||
                //              indexFlags == ( GC.IndexAttributeFlags.HasPosition | GC.IndexAttributeFlags.HasNormal | GC.IndexAttributeFlags.UV16BitIndex | GC.IndexAttributeFlags.HasUV ) ||
                //              indexFlags == ( GC.IndexAttributeFlags.Position16BitIndex | GC.IndexAttributeFlags.HasPosition | GC.IndexAttributeFlags.Normal16BitIndex | GC.IndexAttributeFlags.HasNormal | GC.IndexAttributeFlags.UV16BitIndex | GC.IndexAttributeFlags.HasUV ) ||
                //              indexFlags == ( GC.IndexAttributeFlags.HasPosition | GC.IndexAttributeFlags.HasColor | GC.IndexAttributeFlags.UV16BitIndex | GC.IndexAttributeFlags.HasUV ) ||
                //              indexFlags == ( GC.IndexAttributeFlags.Position16BitIndex | GC.IndexAttributeFlags.HasPosition | GC.IndexAttributeFlags.Color16BitIndex | GC.IndexAttributeFlags.HasColor | GC.IndexAttributeFlags.UV16BitIndex | GC.IndexAttributeFlags.HasUV ) );

                // Set up parameters
                var indexFlagsParam = new GC.IndexAttributeFlagsParam( indexFlags );
                mesh.Parameters.Add( indexFlagsParam );
                indexFlagsParams.Add( indexFlagsParam );

                if ( useColors )
                    mesh.Parameters.Add( new GC.LightingParams( GC.LightingParams.Preset.Colors ) );
                else
                    mesh.Parameters.Add( new GC.LightingParams( GC.LightingParams.Preset.Normals ) );

                mesh.Parameters.Add( new GC.TextureParams( ( ushort )( basicMaterial.TextureId ) ) );
                mesh.Parameters.Add( new GC.MipMapParams() );

                // Build display list
                var basicTriangles = basicMesh.ToTriangles();
                for ( int i = 0; i < basicTriangles.Length; i += 3 )
                {
                    var temp = basicTriangles[i];
                    basicTriangles[i] = basicTriangles[i + 2];
                    basicTriangles[i + 2] = temp;
                }

                var displayListIndices = new GC.Index[basicTriangles.Length];
                for ( int i = 0; i < basicTriangles.Length; i++ )
                {
                    var index = new GC.Index();
                    var basicIndex = basicTriangles[i];
                    index.PositionIndex = basicIndex.VertexIndex;

                    if ( useColors )
                    {
                        var color = hasColors ? basicIndex.Color : Color.White;
                        var colorIndex = colors.IndexOf( color );
                        if ( colorIndex == -1 )
                        {
                            colorIndex = colors.Count;
                            colors.Add( color );
                        }

                        index.ColorIndex = ( ushort )colorIndex;
                    }
                    else
                    {
                        var normal = hasVertexNormals ? basicGeometry.VertexNormals[basicIndex.VertexIndex] : basicIndex.Normal;
                        var normalIndex = normals.IndexOf( normal );
                        if ( normalIndex == -1 )
                        {
                            normalIndex = normals.Count;
                            normals.Add( normal );
                        }

                        index.NormalIndex = ( ushort )normalIndex;
                    }

                    if ( hasUVs )
                    {
                        var uv = basicIndex.UV;
                        var uvIndex = uvs.IndexOf( uv );
                        if ( uvIndex == -1 )
                        {
                            uvIndex = uvs.Count;
                            uvs.Add( uv );
                        }

                        index.UVIndex = ( ushort )uvIndex;
                    }

                    displayListIndices[i] = index;
                }

                var displayList = new GC.GXDisplayList( GC.GXPrimitive.Triangles, displayListIndices );
                mesh.DisplayLists.Add( displayList );
                geometry.OpaqueMeshes.Add( mesh );
            }

            if ( normals.Count > 0 )
            {
                if ( normals.Count > byte.MaxValue )
                {
                    foreach ( var param in indexFlagsParams )
                    {
                        if ( param.Flags.HasFlag( GC.IndexAttributeFlags.HasNormal ) )
                            param.Flags |= GC.IndexAttributeFlags.Normal16BitIndex;
                    }
                }

                geometry.VertexBuffers.Add( new GC.VertexNormalBuffer( normals.ToArray() ) );
            }

            if ( colors.Count > 0 )
            {
                if ( colors.Count > byte.MaxValue )
                {
                    foreach ( var param in indexFlagsParams )
                    {
                        if ( param.Flags.HasFlag( GC.IndexAttributeFlags.HasColor ) )
                            param.Flags |= GC.IndexAttributeFlags.Color16BitIndex;
                    }
                }

                geometry.VertexBuffers.Add( new GC.VertexColorBuffer( colors.ToArray() ) );
            }

            if ( uvs.Count > 0 )
            {
                if ( uvs.Count > byte.MaxValue )
                {
                    foreach ( var param in indexFlagsParams )
                    {
                        if ( param.Flags.HasFlag( GC.IndexAttributeFlags.HasUV ) )
                            param.Flags |= GC.IndexAttributeFlags.UV16BitIndex;
                    }
                }

                geometry.VertexBuffers.Add( new GC.VertexUVBuffer( uvs.ToArray() ) );
            }

            geometry.Bounds = basicGeometry.Bounds;
            return geometry;
        }

        private void Read( EndianBinaryReader reader )
        {
            var modelCount = reader.ReadInt16();
            var animCount  = reader.ReadInt16();
            Field04   = reader.ReadInt32();
            CullRange = reader.ReadSingle();
            reader.ReadOffset( () =>
            {
                for ( int i = 0; i < modelCount; i++ )
                    Models.Add( reader.ReadObject<LandModelSA1>() );

            } );
            var animListOffset = reader.ReadInt32();
            TexturePakFileName = reader.ReadStringOffset();
            Textures           = reader.ReadObjectOffset<TextureReferenceList>();
        }

        private void Write( EndianBinaryWriter writer )
        {
            writer.Write( ModelCount );
            writer.Write( ( short ) 0 );
            writer.Write( Field04 );
            writer.Write( CullRange );
            writer.ScheduleWriteListOffset( Models );
            writer.Write( ( int ) 0 );
            writer.ScheduleWriteStringOffset( TexturePakFileName );
            writer.ScheduleWriteObjectOffset( Textures );
        }

        void ISerializableObject.Read( EndianBinaryReader reader, object context ) => Read( reader );
        void ISerializableObject.Write( EndianBinaryWriter writer, object context ) => Write( writer );
    }
}