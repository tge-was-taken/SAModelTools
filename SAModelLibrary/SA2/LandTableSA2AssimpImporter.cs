using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FraGag.Compression;
using PuyoTools.Modules.Archive;
using PuyoTools.Modules.Texture;
using SAModelLibrary.GeometryFormats;
using SAModelLibrary.GeometryFormats.GC;
using SAModelLibrary.Maths;
using SAModelLibrary.Utils;
using Color = SAModelLibrary.Maths.Color;
using Basic = SAModelLibrary.GeometryFormats.Basic;

namespace SAModelLibrary.SA2
{
    public class LandTableSA2AssimpImporter
    {
        private class MaterialBuildInfo
        {
            public Color Ambient;
            public Color Diffuse;
            public Color Specular;
            public float Exponent;
            public SrcAlphaOp SourceAlpha;
            public DstAlphaOp DestinationAlpha;
            public bool ClampU;
            public bool ClampV;
            public bool FlipU;
            public bool FlipV;
            public short TextureId;
            public bool SuperSample;
            public FilterMode FilterMode;
            public MipMapDAdjust MipMapDAdjust;
            public bool UseAlpha;
        }


        private struct MeshRenderState
        {
            // 0
            public ushort Param0Value1;
            public ushort Param0Value2;

            // 1
            public IndexAttributeFlags IndexFlags;

            // 2
            public ushort LightingValue1;
            public ushort LightingValue2;

            // 3
            public ushort Param3Value1;
            public ushort Param3Value2;

            // 4
            public BlendAlphaFlags BlendAlphaFlags;

            // 5
            public Color AmbientColor;

            // 6
            public ushort Param6Value1;
            public ushort Param6Value2;

            // 7
            public ushort Param7Value1;
            public ushort Param7Value2;

            // 8
            public ushort TextureId;
            public TileMode TileMode;

            // 9
            public ushort Param9Value1;
            public ushort Param9Value2;

            // 10

            public ushort MipMapValue1;
            public ushort MipMapValue2;
        }

        public LandTableSA2 Import( string path, string texturePakPath = null )
        {
            var landTable = new LandTableSA2();
            var aiScene = AssimpHelper.ImportScene( path, false );
            var meshNodes = FindMeshNodes( aiScene.RootNode );

            // Convert textures & build materials
            var convertMaterialsAndTextures = ConvertMaterialsAndTextures( Path.GetDirectoryName( path ), aiScene.Materials, texturePakPath );
            landTable.TexturePakFileName = Path.GetFileNameWithoutExtension( texturePakPath );
            landTable.Textures = convertMaterialsAndTextures.TextureReferences;

            var displayModels = new List<LandModelSA2>();
            var collisionModels = new List<LandModelSA2>();
            foreach ( var aiMeshNode in meshNodes )
            {
                if ( !aiMeshNode.Name.Contains( "hkRB" ) )
                {
                    displayModels.Add( ConvertDisplayModel( aiScene, convertMaterialsAndTextures.MaterialBuildInfos, aiMeshNode ) );
                }
                else
                {
                    collisionModels.Add( ConvertCollisionModel( aiScene, convertMaterialsAndTextures.MaterialBuildInfos, aiMeshNode ) );
                    displayModels.Add( ConvertDisplayModel( aiScene, convertMaterialsAndTextures.MaterialBuildInfos, aiMeshNode ) );
                    //var collisionModel = new LandModelSA2();
                    //collisionModel.Flags |= SurfaceFlags.Collidable;
                    //collisionModel.RootNode = ConvertNode( aiMeshNode, aiScene.Meshes, convertMaterialsAndTextures.MaterialBuildInfos,
                    //                                       ConvertCollisionGeometry );

                    //var collisionGeometry = ( Basic.Geometry )collisionModel.RootNode.Geometry;
                    //collisionModel.Bounds = collisionGeometry.Bounds;

                    //collisionModels.Add( collisionModel );
                }
            }

            landTable.Models.AddRange( displayModels );
            landTable.Models.AddRange( collisionModels );

            return landTable;
        }

        private static LandModelSA2 ConvertDisplayModel( Assimp.Scene aiScene, List<MaterialBuildInfo> materialBuildInfos, Assimp.Node aiMeshNode )
        {
            var displayModel = new LandModelSA2();
            displayModel.Flags |= SurfaceFlags.Visible;
            displayModel.RootNode = ConvertNode( aiMeshNode, aiScene.Meshes, materialBuildInfos, ConvertDisplayGeometry );
            var displayGeometry = ( Geometry )displayModel.RootNode.Geometry;
            displayModel.Bounds = displayGeometry.Bounds;

            //var min = new Vector3( float.MaxValue, float.MaxValue, float.MaxValue );
            //var max = new Vector3( float.MinValue, float.MinValue, float.MinValue );
            //var modelWorldTransform = displayModel.RootNode.WorldTransform;
            //foreach ( var node in displayModel.RootNode.EnumerateAllNodes() )
            //{
            //    if ( node.Geometry == null )
            //        continue;

            //    var geometry = ( ( Geometry ) node.Geometry );
            //    var positionBuffer = ( VertexPositionBuffer )geometry.VertexBuffers.Single( x => x.Type == VertexAttributeType.Position );
            //    foreach ( var vertexPosition in positionBuffer.Elements )
            //    {
            //        var position = vertexPosition;
            //        min.X = Math.Min( min.X, position.X );
            //        min.Y = Math.Min( min.Y, position.Y );
            //        min.Z = Math.Min( min.Z, position.Z );
            //        max.X = Math.Max( max.X, position.X );
            //        max.Y = Math.Max( max.Y, position.Y );
            //        max.Z = Math.Max( max.Z, position.Z );
            //    }
            //}

            //displayModel.Bounds = Bounds.Calculate( min, max );
            return displayModel;
        }

        private static LandModelSA2 ConvertCollisionModel( Assimp.Scene aiScene, List<MaterialBuildInfo> materialBuildInfos, Assimp.Node aiMeshNode )
        {
            var collisionModel = new LandModelSA2();
            collisionModel.Flags |= SurfaceFlags.Collidable;
            collisionModel.RootNode = ConvertNode( aiMeshNode, aiScene.Meshes, materialBuildInfos,
                                                   ConvertCollisionGeometry );

            var collisionGeometry = ( Basic.Geometry )collisionModel.RootNode.Geometry;
            collisionModel.Bounds = collisionGeometry.Bounds;

            return collisionModel;
        }

        private static List<Assimp.Node> FindMeshNodes( Assimp.Node aiSceneRootNode )
        {
            var meshNodes = new List<Assimp.Node>();

            void FindMeshNodesRecursively( Assimp.Node aiParentNode )
            {
                foreach ( var aiNode in aiParentNode.Children )
                {
                    if ( aiNode.HasMeshes )
                        meshNodes.Add( aiNode );
                    else
                        FindMeshNodesRecursively( aiNode );
                }
            }

            FindMeshNodesRecursively( aiSceneRootNode );

            return meshNodes;
        }

        private static ( List<MaterialBuildInfo> MaterialBuildInfos, TextureReferenceList TextureReferences ) ConvertMaterialsAndTextures( string baseDirectory, List<Assimp.Material> aiMaterials, string texturePakPath )
        {
            var textureArchive = new GvmArchive();
            var textureArchiveStream = new MemoryStream();
            var textureArchiveWriter = ( GvmArchiveWriter )textureArchive.Create( textureArchiveStream );
            var textureIdLookup = new Dictionary<string, int>( StringComparer.InvariantCultureIgnoreCase );
            var textureReferences = new TextureReferenceList();
            var dontConvert = false;
            var alphaTextureIds = new HashSet<int>();

            if ( texturePakPath != null && File.Exists( texturePakPath ) )
            {
                var extension = Path.GetExtension( texturePakPath );
                var fileStream = ( Stream )File.OpenRead( texturePakPath );
                if ( extension.Equals( ".prs", StringComparison.InvariantCultureIgnoreCase ) )
                {
                    try
                    {
                        var decompressedFileStream = new MemoryStream();
                        Prs.Decompress( fileStream, decompressedFileStream );
                        fileStream.Dispose();
                        fileStream = decompressedFileStream;
                    }
                    catch ( Exception )
                    {
                        // Not compressed
                    }

                    fileStream.Position = 0;
                }

                var existingTextureArchive = new GvmArchive();
                var existingTextureArchiveReader = ( GvmArchiveReader )existingTextureArchive.Open( fileStream );
                for ( var i = 0; i < existingTextureArchiveReader.Entries.Count; i++ )
                {
                    var entry = existingTextureArchiveReader.Entries[i];

                    // Make copy of entry stream
                    var entryStreamCopy = new MemoryStream();
                    entry.Open().CopyTo( entryStreamCopy );
                    entryStreamCopy.Position = 0;

                    var texture = new VrSharp.GvrTexture.GvrTexture( entryStreamCopy );
                    Console.WriteLine( texture.GlobalIndex );
                    entryStreamCopy.Position = 0;

                    // Clean entry name from the added extension
                    var entryName = Path.ChangeExtension( entry.Name, null );

                    textureArchiveWriter.CreateEntry( entryStreamCopy, entryName );
                    textureIdLookup[entryName] = i;
                }
            }

            var materials = new List<MaterialBuildInfo>();

            foreach ( var aiMaterial in aiMaterials )
            {
                var texturePath = aiMaterial.TextureDiffuse.FilePath ?? string.Empty;
                var textureName = Path.GetFileNameWithoutExtension( texturePath ).ToLowerInvariant();

                if ( !textureIdLookup.TryGetValue( textureName, out var textureId ) )
                {
                    textureReferences.Add( new TextureReference( textureName ) );
                    textureId = textureIdLookup[textureName] = textureIdLookup.Count;

                    var textureFullPath = Path.GetFullPath( Path.Combine( baseDirectory, texturePath ) );
                    if ( !dontConvert && File.Exists( textureFullPath ) )
                    {
                        // Convert texture
                        var texture = new GvrTexture { GlobalIndex = ( uint )( 1 + textureId ) };

                        var textureStream = new MemoryStream();
                        var textureOriginalBitmap = new Bitmap( textureFullPath );
                        var textureBitmap = new Bitmap( textureOriginalBitmap,
                                                        new Size( textureOriginalBitmap.Width / 2, textureOriginalBitmap.Height / 2 ) );
                        //var textureBitmap = new Bitmap( 32, 32 );
                        texture.Write( textureBitmap, textureStream );
                        textureStream.Position = 0;

                        // Add it
                        textureArchiveWriter.CreateEntry( textureStream, textureName );

                        //if ( HasTransparency( textureBitmap ) )
                        //{
                        //    alphaTextureIds.Add( textureId );
                        //}
                    }
                }

                var material = new MaterialBuildInfo
                {
                    Ambient          = AssimpHelper.FromAssimp( aiMaterial.ColorAmbient ),
                    Diffuse          = AssimpHelper.FromAssimp( aiMaterial.ColorDiffuse ),
                    Specular         = AssimpHelper.FromAssimp( aiMaterial.ColorSpecular ),
                    ClampU           = aiMaterial.TextureDiffuse.WrapModeU == Assimp.TextureWrapMode.Clamp,
                    ClampV           = aiMaterial.TextureDiffuse.WrapModeV == Assimp.TextureWrapMode.Clamp,
                    FlipU            = aiMaterial.TextureDiffuse.WrapModeU == Assimp.TextureWrapMode.Mirror,
                    FlipV            = aiMaterial.TextureDiffuse.WrapModeV == Assimp.TextureWrapMode.Mirror,
                    DestinationAlpha = DstAlphaOp.InverseDst,
                    Exponent         = 0,
                    FilterMode       = FilterMode.Trilinear,
                    MipMapDAdjust    = MipMapDAdjust.D050,
                    SourceAlpha      = SrcAlphaOp.Src,
                    SuperSample      = false,
                    TextureId        = ( short ) textureId,
                    UseAlpha         = alphaTextureIds.Contains( textureId )
                };

                materials.Add( material );
            }

            if ( !dontConvert )
            {
                // Write texture archive to file
                textureArchiveWriter.Flush();
                textureArchiveStream.Position = 0;

                if ( texturePakPath != null )
                {
                    // Compress it.
                    var textureArchivePrsStream = new MemoryStream();
                    Prs.Compress( textureArchiveStream, textureArchivePrsStream );

                    // Save compressed file.
                    textureArchivePrsStream.Position = 0;
                    using ( var outFile = File.Create( texturePakPath ) )
                        textureArchivePrsStream.CopyTo( outFile );

                }
            }

            return ( materials, textureReferences );
        }

        private static Node ConvertNode( Assimp.Node aiNode, List<Assimp.Mesh> aiMeshes, List<MaterialBuildInfo> materialBuildInfos, ConvertGeometryDelegate convertGeometry )
        {
            Node ConvertHierarchyNodeRecursively( Assimp.Node curAiNode, ref Node previousSibling, Node parent, ref Assimp.Matrix4x4 parentNodeWorldTransform )
            {
                var nodeWorldTransform = curAiNode.Transform * parentNodeWorldTransform;

                curAiNode.Transform.Decompose( out var scale, out var rotation, out var translation );

                // Create node
                //var node = new Node( AssimpHelper.FromAssimp( translation ), AngleVector.FromQuaternion( AssimpHelper.FromAssimp( rotation ) ),
                //                     AssimpHelper.FromAssimp( scale ), parent );
                var node = new Node( Vector3.Zero, AngleVector.Zero, Vector3.One, parent );

                if ( curAiNode.HasMeshes )
                {
                    Console.WriteLine( curAiNode.Name );
                    node.Geometry = convertGeometry( curAiNode, nodeWorldTransform, aiMeshes, materialBuildInfos );
                }

                // Set sibling (next) reference of previous
                if ( previousSibling != null )
                    previousSibling.Sibling = node;

                previousSibling = node;

                if ( curAiNode.HasChildren )
                {
                    Node childPreviousSibling = null;
                    foreach ( var aiChildNode in curAiNode.Children )
                    {
                        var childNode = ConvertHierarchyNodeRecursively( aiChildNode, ref childPreviousSibling, node, ref nodeWorldTransform );

                        // Make sure to set the 'first child' reference if we haven't already
                        if ( node.Child == null )
                            node.Child = childNode;
                    }
                }

                return node;
            }

            // Dummy!
            Node dummyNode = null;
            var identityMatrix = Assimp.Matrix4x4.Identity;
            var rootNode = ConvertHierarchyNodeRecursively( aiNode, ref dummyNode, null, ref identityMatrix );

            foreach ( var node in rootNode.EnumerateAllNodes() )
                node.OptimizeFlags();

            return rootNode;
        }

        private delegate IGeometry ConvertGeometryDelegate( Assimp.Node             curAiNode, Assimp.Matrix4x4 nodeWorldTransform,
                                                           List<Assimp.Mesh>       aiMeshes,
                                                           List<MaterialBuildInfo> materialBuildInfos );

        private static IGeometry ConvertDisplayGeometry( Assimp.Node curAiNode, Assimp.Matrix4x4 nodeWorldTransform, List<Assimp.Mesh> aiMeshes, List<MaterialBuildInfo> materialBuildInfos )
        {
            var nodeInverseWorldTransform = nodeWorldTransform;
            nodeInverseWorldTransform.Inverse();
            var nodeInverseTransposeWorldTransform = nodeInverseWorldTransform;
            nodeInverseTransposeWorldTransform.Transpose();

            var geometry = new Geometry();

            // Convert meshes
            var vertexPositions = new List<Assimp.Vector3D>();
            var vertexNormals = new List<Assimp.Vector3D>();
            var vertexUVs = new List<Assimp.Vector3D>();
            var vertexColors = new List<Assimp.Color4D>();
            var lastRenderState = new MeshRenderState();

            foreach ( var aiMeshIndex in curAiNode.MeshIndices )
            {
                var aiMesh = aiMeshes[aiMeshIndex];
                var material = materialBuildInfos[aiMesh.MaterialIndex];
                var mesh = new Mesh();
                var renderState = new MeshRenderState();

                renderState.IndexFlags = IndexAttributeFlags.HasPosition;
                var useColors = false;
                var hasColors = aiMesh.HasVertexColors( 0 );
                var hasUVs = aiMesh.HasTextureCoords( 0 );
                var hasNormals = aiMesh.HasNormals;

                if ( hasColors || !hasNormals )
                {
                    renderState.IndexFlags |= IndexAttributeFlags.HasColor;
                    useColors = true;
                }
                else
                {
                    renderState.IndexFlags |= IndexAttributeFlags.HasNormal;
                }

                if ( hasUVs )
                {
                    renderState.IndexFlags |= IndexAttributeFlags.HasUV;
                }

                // Convert faces
                var triangleIndices = new Index[aiMesh.FaceCount * 3];
                for ( var i = 0; i < aiMesh.Faces.Count; i++ )
                {
                    var aiFace = aiMesh.Faces[i];

                    for ( var j = 0; j < 3; j++ )
                    {
                        var triangleIndicesIndex = ( i * 3 ) + 2 - j;

                        if ( j >= aiFace.IndexCount )
                        {
                            triangleIndices[triangleIndicesIndex] = triangleIndices[triangleIndicesIndex + 1];
                            continue;
                        }

                        int aiFaceIndex = aiFace.Indices[j];

                        var position = aiMesh.Vertices[aiFaceIndex];
                        var positionIndex = vertexPositions.AddUnique( position );
                        if ( positionIndex > byte.MaxValue )
                            renderState.IndexFlags |= IndexAttributeFlags.Position16BitIndex;

                        var normalIndex = 0;
                        var colorIndex = 0;
                        var uvIndex = 0;

                        if ( useColors )
                        {
                            var color = hasColors ? aiMesh.VertexColorChannels[0][aiFaceIndex] : new Assimp.Color4D();
                            colorIndex = vertexColors.AddUnique( color );

                            if ( colorIndex > byte.MaxValue )
                                renderState.IndexFlags |= IndexAttributeFlags.Color16BitIndex;
                        }
                        else
                        {
                            var normal = aiMesh.Normals[aiFaceIndex];
                            normalIndex = vertexNormals.AddUnique( normal );

                            if ( normalIndex > byte.MaxValue )
                                renderState.IndexFlags |= IndexAttributeFlags.Normal16BitIndex;
                        }

                        if ( hasUVs )
                        {
                            var uv = aiMesh.TextureCoordinateChannels[0][aiFaceIndex];
                            uvIndex = vertexUVs.AddUnique( uv );

                            if ( uvIndex > byte.MaxValue )
                                renderState.IndexFlags |= IndexAttributeFlags.UV16BitIndex;
                        }

                        triangleIndices[triangleIndicesIndex] = new Index
                        {
                            PositionIndex = ( ushort )positionIndex,
                            NormalIndex = ( ushort )normalIndex,
                            ColorIndex = ( ushort )colorIndex,
                            UVIndex = ( ushort )uvIndex
                        };
                    }
                }

                // Build display list
                var displayList = new GXDisplayList( GXPrimitive.Triangles, triangleIndices );
                mesh.DisplayLists.Add( displayList );

                // Set up render params
                if ( renderState.IndexFlags != lastRenderState.IndexFlags )
                    mesh.Parameters.Add( new IndexAttributeFlagsParam( renderState.IndexFlags ) );

                // Set up render lighting params
                {
                    if ( useColors )
                    {
                        renderState.LightingValue1 = 0x0b11;
                    }
                    else
                    {
                        renderState.LightingValue2 = 0x0011;
                    }

                    renderState.LightingValue2 = 1;

                    if ( renderState.LightingValue1 != lastRenderState.LightingValue1 ||
                         renderState.LightingValue2 != lastRenderState.LightingValue2 )
                    {
                        mesh.Parameters.Add( new LightingParams()
                        {
                            Value1 = renderState.LightingValue1,
                            Value2 = renderState.LightingValue2
                        } );
                    }
                }

                // Set up render texture params
                {
                    renderState.TextureId = ( ushort )material.TextureId;
                    renderState.TileMode = TileMode.WrapU | TileMode.WrapV;

                    if ( renderState.TextureId != lastRenderState.TextureId ||
                         renderState.TileMode != lastRenderState.TileMode )
                    {
                        mesh.Parameters.Add( new TextureParams( renderState.TextureId, renderState.TileMode ) );
                    }
                }

                // Set up render mipmap params
                {
                    renderState.MipMapValue1 = 0x104a;
                    renderState.MipMapValue2 = 0;

                    if ( renderState.MipMapValue1 != lastRenderState.MipMapValue1 ||
                         renderState.MipMapValue2 != lastRenderState.MipMapValue2 )
                    {
                        mesh.Parameters.Add( new MipMapParams { Value1 = renderState.MipMapValue1, Value2 = renderState.MipMapValue2 } );
                    }
                }

                //if ( material.UseAlpha )
                //{
                //    mesh.Parameters.Add( new BlendAlphaParam() { Flags = BlendAlphaFlags.UseAlpha } );
                //    geometry.TranslucentMeshes.Add( mesh );
                //}
                //else
                //{
                //    geometry.OpaqueMeshes.Add( mesh );
                //}

                geometry.OpaqueMeshes.Add( mesh );
                lastRenderState = renderState;
            }

            // Build vertex buffers
            if ( vertexPositions.Count > 0 )
            {
                Debug.Assert( vertexPositions.Count <= ushort.MaxValue );
                var localVertexPositions = vertexPositions.Select( x =>
                {
                    Assimp.Unmanaged.AssimpLibrary.Instance.TransformVecByMatrix4( ref x, ref nodeWorldTransform );
                    return AssimpHelper.FromAssimp( x );
                } ).ToArray();
                geometry.VertexBuffers.Add( new VertexPositionBuffer( localVertexPositions ) );
                geometry.Bounds = BoundingSphere.Calculate( localVertexPositions );
            }

            if ( vertexNormals.Count > 0 )
            {
                Debug.Assert( vertexNormals.Count <= ushort.MaxValue );
                geometry.VertexBuffers.Add( new VertexNormalBuffer( vertexNormals.Select( x =>
                {
                    Assimp.Unmanaged.AssimpLibrary.Instance.TransformVecByMatrix4( ref x, ref nodeInverseTransposeWorldTransform );
                    return AssimpHelper.FromAssimp( x );
                } ).ToArray() ) );
            }

            if ( vertexColors.Count > 0 )
            {
                Debug.Assert( vertexColors.Count <= ushort.MaxValue );
                geometry.VertexBuffers.Add( new VertexColorBuffer( vertexColors.Select( AssimpHelper.FromAssimp ).ToArray() ) );
            }

            if ( vertexUVs.Count > 0 )
            {
                Debug.Assert( vertexUVs.Count <= ushort.MaxValue );
                geometry.VertexBuffers.Add( new VertexUVBuffer( vertexUVs.Select( x =>
                                                                                      UVCodec.Encode255( AssimpHelper
                                                                                                              .FromAssimpAsVector2( x ) ) )
                                                                         .ToArray() ) );
            }

            return geometry;
        }

        private static IGeometry ConvertCollisionGeometry( Assimp.Node             curAiNode, Assimp.Matrix4x4 nodeWorldTransform,
                                                           List<Assimp.Mesh>       aiMeshes,
                                                           List<MaterialBuildInfo> materialBuildInfos )
        {
            var vertices = new List<Assimp.Vector3D>();
            var triangles = new List<Basic.Triangle>();

            foreach ( var aiMeshIndex in curAiNode.MeshIndices )
            {
                var aiMesh = aiMeshes[ aiMeshIndex ];

                for ( var i = 0; i < aiMesh.Faces.Count; i++ )
                {
                    var aiFace = aiMesh.Faces[ i ];
                    var triangle = new Basic.Triangle();
                    var flip = false;

                    if ( !flip )
                    {
                        triangle.A = ( ushort )vertices.AddUnique( aiMesh.Vertices[aiFace.Indices[0]] );
                        triangle.B = aiFace.IndexCount > 1 ? ( ushort )vertices.AddUnique( aiMesh.Vertices[aiFace.Indices[1]] ) : triangle.A;
                        triangle.C = aiFace.IndexCount > 2 ? ( ushort )vertices.AddUnique( aiMesh.Vertices[aiFace.Indices[2]] ) : triangle.B;
                    }
                    else
                    {
                        triangle.C = ( ushort )vertices.AddUnique( aiMesh.Vertices[aiFace.Indices[0]] );
                        triangle.B = aiFace.IndexCount > 1 ? ( ushort )vertices.AddUnique( aiMesh.Vertices[aiFace.Indices[1]] ) : triangle.A;
                        triangle.A = aiFace.IndexCount > 2 ? ( ushort )vertices.AddUnique( aiMesh.Vertices[aiFace.Indices[2]] ) : triangle.B;
                    }

                    triangles.Add( triangle );
                }
            }

            var geometry = new Basic.Geometry
            {
                Meshes = new[]
                {
                    new Basic.Mesh
                    {
                        PrimitiveType = PrimitiveType.Triangles,
                        Primitives    = triangles.Cast<Basic.IPrimitive>().ToArray(),
                    }
                },
                VertexPositions = vertices.Select( x =>
                                                  {
                                                      Assimp.Unmanaged.AssimpLibrary.Instance.TransformVecByMatrix4( ref x, ref nodeWorldTransform );
                                                      return AssimpHelper.FromAssimp( x );
                                                  }
                                                 ).ToArray()
            };

            geometry.Bounds = BoundingSphere.Calculate( geometry.VertexPositions );

            return geometry;
        }

        // https://stackoverflow.com/questions/3064854/determine-if-alpha-channel-is-used-in-an-image/39013496#39013496
        private static Boolean HasTransparency( Bitmap bitmap )
        {
            // not an alpha-capable color format.
            if ( ( bitmap.Flags & ( Int32 )ImageFlags.HasAlpha ) == 0 )
                return false;
            // Indexed formats. Special case because one index on their palette is configured as THE transparent color.
            if ( bitmap.PixelFormat == PixelFormat.Format8bppIndexed || bitmap.PixelFormat == PixelFormat.Format4bppIndexed )
            {
                ColorPalette pal = bitmap.Palette;
                // Find the transparent index on the palette.
                Int32 transCol = -1;
                for ( int i = 0; i < pal.Entries.Length; i++ )
                {
                    System.Drawing.Color col = pal.Entries[i];
                    if ( col.A != 255 )
                    {
                        // Color palettes should only have one index acting as transparency. Not sure if there's a better way of getting it...
                        transCol = i;
                        break;
                    }
                }
                // none of the entries in the palette have transparency information.
                if ( transCol == -1 )
                    return false;
                // Check pixels for existence of the transparent index.
                Int32 colDepth = Image.GetPixelFormatSize( bitmap.PixelFormat );
                BitmapData data = bitmap.LockBits( new Rectangle( 0, 0, bitmap.Width, bitmap.Height ), ImageLockMode.ReadOnly, bitmap.PixelFormat );
                Int32 stride = data.Stride;
                Byte[] bytes = new Byte[bitmap.Height * stride];
                Marshal.Copy( data.Scan0, bytes, 0, bytes.Length );
                bitmap.UnlockBits( data );
                if ( colDepth == 8 )
                {
                    // Last line index.
                    Int32 lineMax = bitmap.Width - 1;
                    for ( Int32 i = 0; i < bytes.Length; i++ )
                    {
                        // Last position to process.
                        Int32 linepos = i % stride;
                        // Passed last image byte of the line. Abort and go on with loop.
                        if ( linepos > lineMax )
                            continue;
                        Byte b = bytes[i];
                        if ( b == transCol )
                            return true;
                    }
                }
                else if ( colDepth == 4 )
                {
                    // line size in bytes. 1-indexed for the moment.
                    Int32 lineMax = bitmap.Width / 2;
                    // Check if end of line ends on half a byte.
                    Boolean halfByte = bitmap.Width % 2 != 0;
                    // If it ends on half a byte, one more needs to be processed.
                    // We subtract in the other case instead, to make it 0-indexed right away.
                    if ( !halfByte )
                        lineMax--;
                    for ( Int32 i = 0; i < bytes.Length; i++ )
                    {
                        // Last position to process.
                        Int32 linepos = i % stride;
                        // Passed last image byte of the line. Abort and go on with loop.
                        if ( linepos > lineMax )
                            continue;
                        Byte b = bytes[i];
                        if ( ( b & 0x0F ) == transCol )
                            return true;
                        if ( halfByte && linepos == lineMax ) // reached last byte of the line. If only half a byte to check on that, abort and go on with loop.
                            continue;
                        if ( ( ( b & 0xF0 ) >> 4 ) == transCol )
                            return true;
                    }
                }
                return false;
            }
            if ( bitmap.PixelFormat == PixelFormat.Format32bppArgb || bitmap.PixelFormat == PixelFormat.Format32bppPArgb )
            {
                BitmapData data = bitmap.LockBits( new Rectangle( 0, 0, bitmap.Width, bitmap.Height ), ImageLockMode.ReadOnly, bitmap.PixelFormat );
                Byte[] bytes = new Byte[bitmap.Height * data.Stride];
                Marshal.Copy( data.Scan0, bytes, 0, bytes.Length );
                bitmap.UnlockBits( data );
                for ( Int32 p = 3; p < bytes.Length; p += 4 )
                {
                    if ( bytes[p] != 255 )
                        return true;
                }
                return false;
            }
            // Final "screw it all" method. This is pretty slow, but it won't ever be used, unless you
            // encounter some really esoteric types not handled above, like 16bppArgb1555 and 64bppArgb.
            for ( Int32 i = 0; i < bitmap.Width; i++ )
            {
                for ( Int32 j = 0; j < bitmap.Height; j++ )
                {
                    if ( bitmap.GetPixel( i, j ).A != 255 )
                        return true;
                }
            }
            return false;
        }
    }
}
