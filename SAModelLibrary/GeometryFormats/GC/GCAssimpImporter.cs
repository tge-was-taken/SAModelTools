using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FraGag.Compression;
using PuyoTools.Modules.Archive;
using PuyoTools.Modules.Texture;
using SAModelLibrary.Maths;
using SAModelLibrary.Utils;
using Color = SAModelLibrary.Maths.Color;

namespace SAModelLibrary.GeometryFormats.GC
{
    public class GCAssimpImporter
    {
        private class MaterialBuildInfo
        {
            public Color         Ambient;
            public Color         Diffuse;
            public Color         Specular;
            public float         Exponent;
            public SrcAlphaOp    SourceAlpha;
            public DstAlphaOp    DestinationAlpha;
            public bool          ClampU;
            public bool          ClampV;
            public bool          FlipU;
            public bool          FlipV;
            public short         TextureId;
            public bool          SuperSample;
            public FilterMode    FilterMode;
            public MipMapDAdjust MipMapDAdjust;
        }


        private struct MeshRenderState
        {
            // 0
            public ushort Param0Value1;
            public ushort Param0Value2;

            // 1
            public IndexAttributeFlags IndexFlags;

            // 2
            public ushort LightingParam1;
            public ushort LightingParam2;

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
            public ushort   TextureId;
            public TileMode TileMode;

            // 9
            public ushort Param9Value1;
            public ushort Param9Value2;

            // 10
            public ushort MipMapParam1;
            public ushort MipMapParam2;
        }

        public Node Import( string path, string texturePakPath = null )
        {
            // Import scene.
            var aiScene = AssimpHelper.ImportScene( path, false );
            var materialBuildInfos = BuildProceduralMaterials( Path.GetDirectoryName( path ), aiScene.Materials, texturePakPath );
            var rootNode = ConvertNode( aiScene.RootNode, aiScene.Meshes, materialBuildInfos );
            return rootNode;
        }

        private static List<MaterialBuildInfo> BuildProceduralMaterials( string baseDirectory, List<Assimp.Material> aiMaterials, string texturePakPath )
        {
            var textureArchive = new GvmArchive();
            var textureArchiveStream = new MemoryStream();
            var textureArchiveWriter = ( GvmArchiveWriter )textureArchive.Create( textureArchiveStream );
            var textureIdLookup = new Dictionary<string, int>( StringComparer.InvariantCultureIgnoreCase );

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
                var textureName = Path.GetFileNameWithoutExtension( aiMaterial.TextureDiffuse.FilePath ).ToLowerInvariant();
                if ( !textureIdLookup.TryGetValue( textureName, out var textureId ) )
                {
                    textureId = textureIdLookup[textureName] = textureIdLookup.Count;
                    var texturePath = Path.GetFullPath( Path.Combine( baseDirectory, aiMaterial.TextureDiffuse.FilePath ) );
                    if ( File.Exists( texturePath ) )
                    {
                        // Convert texture
                        var texture = new GvrTexture { GlobalIndex = ( uint )( 1 + textureId ) };

                        var textureStream = new MemoryStream();
                        var textureBitmap = new Bitmap( texturePath );
                        texture.Write( textureBitmap, textureStream );
                        textureStream.Position = 0;

                        // Add it
                        textureArchiveWriter.CreateEntry( textureStream, textureName );
                    }
                }

                var material = new MaterialBuildInfo
                {
                    Ambient = AssimpHelper.FromAssimp( aiMaterial.ColorAmbient ),
                    Diffuse = AssimpHelper.FromAssimp( aiMaterial.ColorDiffuse ),
                    Specular = AssimpHelper.FromAssimp( aiMaterial.ColorSpecular ),
                    ClampU = aiMaterial.TextureDiffuse.WrapModeU == Assimp.TextureWrapMode.Clamp,
                    ClampV = aiMaterial.TextureDiffuse.WrapModeV == Assimp.TextureWrapMode.Clamp,
                    FlipU = aiMaterial.TextureDiffuse.WrapModeU == Assimp.TextureWrapMode.Mirror,
                    FlipV = aiMaterial.TextureDiffuse.WrapModeV == Assimp.TextureWrapMode.Mirror,
                    DestinationAlpha = DstAlphaOp.InverseDst,
                    Exponent = 0,
                    FilterMode = FilterMode.Trilinear,
                    MipMapDAdjust = MipMapDAdjust.D050,
                    SourceAlpha = SrcAlphaOp.Src,
                    SuperSample = false,
                    TextureId = ( short )textureId,
                };

                materials.Add( material );
            }

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

            return materials;
        }

        private static Node ConvertNode( Assimp.Node aiNode, List<Assimp.Mesh> aiMeshes, List<MaterialBuildInfo> materialBuildInfos )
        {
            Node ConvertHierarchyNodeRecursively( Assimp.Node curAiNode, ref Node previousSibling, Node parent, ref Assimp.Matrix4x4 parentNodeWorldTransform )
            {
                var nodeWorldTransform = curAiNode.Transform * parentNodeWorldTransform;
                var nodeInverseWorldTransform = nodeWorldTransform;
                nodeInverseWorldTransform.Inverse();

                curAiNode.Transform.Decompose( out var scale, out var rotation, out var translation );

                // Create node
                var node = new Node( AssimpHelper.FromAssimp( translation ), AngleVector.FromQuaternion( AssimpHelper.FromAssimp( rotation ) ),
                                     AssimpHelper.FromAssimp( scale ), parent );

                if ( curAiNode.HasMeshes )
                {
                    var geometry = new Geometry();

                    // Convert meshes
                    var vertexPositions = new List<Assimp.Vector3D>();
                    var vertexNormals = new List<Assimp.Vector3D>();
                    var vertexUVs = new List<Assimp.Vector3D>();
                    var vertexColors = new List<Assimp.Color4D>();
                    var lastRenderState = new MeshRenderState();

                    foreach ( var aiMeshIndex in curAiNode.MeshIndices )
                    {
                        var aiMesh = aiMeshes[ aiMeshIndex ];
                        var material = materialBuildInfos[ aiMesh.MaterialIndex ];
                        var mesh = new Mesh();
                        var renderState = new MeshRenderState();

                        renderState.IndexFlags = IndexAttributeFlags.HasPosition | IndexAttributeFlags.Position16BitIndex;
                        var useColors = false;
                        var hasColors = aiMesh.HasVertexColors( 0 );
                        var hasUVs = aiMesh.HasTextureCoords( 0 );
                        var hasNormals = aiMesh.HasNormals;

                        if ( hasColors || !hasNormals )
                        {
                            renderState.IndexFlags |= IndexAttributeFlags.HasColor | IndexAttributeFlags.Color16BitIndex;
                            useColors  =  true;
                        }
                        else
                        {
                            renderState.IndexFlags |= IndexAttributeFlags.HasNormal | IndexAttributeFlags.Normal16BitIndex;
                        }

                        if ( hasUVs )
                        {
                            renderState.IndexFlags |= IndexAttributeFlags.HasUV | IndexAttributeFlags.UV16BitIndex;
                        }

                        // Convert faces
                        var triangleIndices = new Index[aiMesh.FaceCount * 3];
                        for ( var i = 0; i < aiMesh.Faces.Count; i++ )
                        {
                            var aiFace = aiMesh.Faces[ i ];
                            Debug.Assert( aiFace.IndexCount == 3 );

                            for ( var j = 0; j < aiFace.Indices.Count; j++ )
                            {
                                int aiFaceIndex   = aiFace.Indices[ j ];

                                var position = aiMesh.Vertices[aiFaceIndex];
                                var positionIndex = vertexPositions.IndexOf( position );
                                if ( positionIndex == -1 )
                                {
                                    positionIndex = vertexPositions.Count;
                                    vertexPositions.Add( position );
                                }

                                var normalIndex = 0;
                                var colorIndex  = 0;
                                var uvIndex     = 0;

                                if ( useColors )
                                {
                                    var color = hasColors ? aiMesh.VertexColorChannels[ 0 ][ aiFaceIndex ] : new Assimp.Color4D();
                                    colorIndex = vertexColors.IndexOf( color );
                                    if ( colorIndex == -1 )
                                    {
                                        colorIndex = vertexColors.Count;
                                        vertexColors.Add( color );
                                    }
                                }
                                else
                                {
                                    var normal = aiMesh.Normals[ aiFaceIndex ];
                                    normalIndex = vertexNormals.IndexOf( normal );
                                    if ( normalIndex == -1 )
                                    {
                                        normalIndex = vertexNormals.Count;
                                        vertexNormals.Add( normal );
                                    }
                                }

                                if ( hasUVs )
                                {
                                    var uv = aiMesh.TextureCoordinateChannels[ 0 ][ aiFaceIndex ];
                                    uvIndex = vertexUVs.IndexOf( uv );
                                    if ( uvIndex == -1 )
                                    {
                                        uvIndex = vertexUVs.Count;
                                        vertexUVs.Add( uv );
                                    }
                                }

                                triangleIndices[ ( i * 3 ) + j ] = new Index
                                {
                                    PositionIndex = ( ushort )positionIndex,
                                    NormalIndex   = ( ushort )normalIndex,
                                    ColorIndex    = ( ushort )colorIndex,
                                    UVIndex       = ( ushort )uvIndex
                                };
                            }
                        }

                        // Build display list
                        var displayList = new GXDisplayList( GXPrimitive.Triangles, triangleIndices );
                        mesh.DisplayLists.Add( displayList );

                        // Set up render params
                        var indexFlagsParam = new IndexAttributeFlagsParam( renderState.IndexFlags );
                        mesh.Parameters.Add( indexFlagsParam );

                        if ( useColors )
                            mesh.Parameters.Add( new LightingParams( LightingParams.Preset.Colors ) );
                        else
                            mesh.Parameters.Add( new LightingParams( LightingParams.Preset.Normals ) );

                        mesh.Parameters.Add( new TextureParams( ( ushort )( material.TextureId ) ) );
                        mesh.Parameters.Add( new MipMapParams() );
                        geometry.OpaqueMeshes.Add( mesh );
                    }

                    // Build vertex buffers
                    if ( vertexPositions.Count > 0 )
                    {
                        geometry.VertexBuffers.Add( new VertexPositionBuffer( vertexPositions.Select( x =>
                        {
                            Assimp.Unmanaged.AssimpLibrary.Instance.TransformVecByMatrix4( ref x, ref nodeInverseWorldTransform );
                            return AssimpHelper.FromAssimp( x );
                        } ).ToArray() ) );
                    }

                    if ( vertexNormals.Count > 0 )
                    {
                        nodeInverseWorldTransform.Transpose();

                        geometry.VertexBuffers.Add( new VertexNormalBuffer( vertexNormals.Select( x =>
                        {
                            Assimp.Unmanaged.AssimpLibrary.Instance.TransformVecByMatrix4( ref x, ref nodeInverseWorldTransform );
                            return AssimpHelper.FromAssimp( x );
                        } ).ToArray() ) );
                    }

                    if ( vertexColors.Count > 0 )
                    {
                        geometry.VertexBuffers.Add( new VertexColorBuffer( vertexColors.Select( AssimpHelper.FromAssimp ).ToArray() ) );
                    }

                    if ( vertexUVs.Count > 0 )
                    {
                        geometry.VertexBuffers.Add( new VertexUVBuffer( vertexUVs.Select( x =>
                                                                                              UVCodec.Encode1023( AssimpHelper
                                                                                                                      .FromAssimpAsVector2( x ) ) )
                                                                                 .ToArray() ) );
                    }
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
            Node dummy = null;
            var identity = Assimp.Matrix4x4.Identity;
            return ConvertHierarchyNodeRecursively( aiNode, ref dummy, null, ref identity );
        }
    }
}
