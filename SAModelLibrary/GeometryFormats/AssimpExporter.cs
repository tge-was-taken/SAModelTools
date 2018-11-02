using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using SAModelLibrary.Maths;

namespace SAModelLibrary.GeometryFormats
{
    public abstract class AssimpExporter
    {
        private static Dictionary<GeometryFormat, Func<AssimpExporter>> sExporterFactory = new Dictionary<GeometryFormat, Func<AssimpExporter>>()
        {
            { GeometryFormat.Chunk, () => new Chunk.ChunkAssimpExporter() }
        };

        /// <summary>
        /// Gets the instance of the scene being exported to.
        /// </summary>
        protected Assimp.Scene Scene { get; set; }

        /// <summary>
        /// Gets the index of the current node that is being converted.
        /// </summary>
        protected int NodeIndex { get; set; }

        /// <summary>
        /// Gets the texture names associated with the current model. Can be null.
        /// </summary>
        protected List<string> TextureNames { get; set; }

        /// <summary>
        /// Gets or sets whether to remove the nodes and just keep the meshes.
        /// </summary>
        public bool RemoveNodes { get; set; } = false;

        /// <summary>
        /// Gets or sets whether meshes should be attached to their parent node, or if a new node for them should be created.
        /// Ignored if <see cref="RemoveNodes"/> is <see langword="true"/>.
        /// </summary>
        public bool AttachMeshesToParentNode { get; set; } = false;

        protected AssimpExporter()
        {
        }

        public void Export( Node rootNode, string filePath, TextureReferenceList textureNames )
        {
            TextureNames = textureNames.Select( x => x.Name ).ToList();
            Export( rootNode, filePath );
        }

        public void Export( Node rootNode, string filePath, List<string> textureNames )
        {
            TextureNames = textureNames;
            Export( rootNode, filePath );
        }

        public void Export( Node rootNode, string filePath )
        {
            Scene = CreateDefaultScene();
            Initialize( rootNode );
            ConvertRootNode( rootNode );
            ExportCollada( Scene, filePath );
        }

        protected abstract void Initialize( Node rootNode );

        protected void ConvertRootNode( Node rootNode )
        {
            NodeIndex = -1;
            var identityMatrix = Matrix4x4.Identity;
            ConvertNodes( rootNode, Scene.RootNode, ref identityMatrix );
        }

        protected void ConvertNodes( Node node, Assimp.Node aiParentNode, ref Matrix4x4 parentNodeWorldTransform )
        {
            while ( node != null )
            {
                ++NodeIndex;

                Assimp.Node aiNode = null;
                if ( !RemoveNodes )
                {
                    // Only convert the node if we're actually keeping them.
                    aiNode = new Assimp.Node( FormatNodeName( NodeIndex ), aiParentNode ) { Transform = ToAssimp( node.Transform ) };
                    aiParentNode.Children.Add( aiNode );
                }

                var nodeWorldTransform = node.Transform * parentNodeWorldTransform;

                var geometry = node.Geometry;
                if ( geometry != null )
                {
                    var meshCountBefore = Scene.Meshes.Count;

                    ConvertGeometry( geometry, ref nodeWorldTransform );

                    var meshCountAfter = Scene.Meshes.Count;
                    var addedMeshCount = meshCountAfter - meshCountBefore;
                    Debug.Assert( addedMeshCount >= 0 );

                    var attachMeshesToParentNode = AttachMeshesToParentNode && aiNode != null;

                    // Add 'mesh' nodes for the newly added meshes
                    for ( int i = 0; i < addedMeshCount; i++ )
                    {
                        var aiMeshIndex = meshCountBefore + i;
                        var aiMesh = Scene.Meshes[ aiMeshIndex ];

                        if ( attachMeshesToParentNode )
                        {
                            aiNode.MeshIndices.Add( aiMeshIndex );
                        }
                        else
                        {
                            var aiMeshNode = new Assimp.Node( FormatMeshName( aiMeshIndex ) ) { Transform = ToAssimp( nodeWorldTransform ) };
                            aiMeshNode.MeshIndices.Add( aiMeshIndex );
                            Scene.RootNode.Children.Add( aiMeshNode );

                            if ( !RemoveNodes && !aiMesh.HasBones )
                            {
                                // Add weights to keep the animation hierarchy intact
                                var aiBone = new Assimp.Bone { Name = aiNode.Name, };

                                for ( int j = 0; j < aiMesh.VertexCount; j++ )
                                    aiBone.VertexWeights.Add( new Assimp.VertexWeight( j, 1f ) );

                                aiMesh.Bones.Add( aiBone );
                            }
                        }
                    }
                }

                if ( node.Child != null )
                    ConvertNodes( node.Child, aiNode, ref nodeWorldTransform );

                node = node.Sibling;
            }
        }

        protected abstract void ConvertGeometry( IGeometry iGeometry, ref Matrix4x4 nodeWorldTransform );

        /// <summary>
        /// TODO remove this crap
        /// </summary>
        /// <param name="aiScene"></param>
        /// <param name="textures"></param>
        /// <param name="exporter"></param>
        /// <param name="iGeometry"></param>
        /// <param name="nodeWorldTransform"></param>
        protected void ConvertGeometry( Assimp.Scene aiScene, List<string> textures, AssimpExporter exporter, IGeometry iGeometry, ref Matrix4x4 nodeWorldTransform )
        {
            exporter.Scene = aiScene;
            exporter.TextureNames = textures;
            exporter.ConvertGeometry( iGeometry, ref nodeWorldTransform );
        }

        protected string FormatTextureName( int textureId )
        {
            return FormatTextureName( textureId, TextureNames );
        }

        protected int GetNoMaterialMaterialIndex()
        {
            var aiMaterialIndex = Scene.Materials.FindIndex( x => x.Name == "no_material" );
            if ( aiMaterialIndex == -1 )
            {
                aiMaterialIndex = Scene.Materials.Count;
                Scene.Materials.Add( new Assimp.Material() { Name = "no_material" } );
            }

            return aiMaterialIndex;
        }

        protected Assimp.Material CreateMaterial( Color diffuse, Color specular, Color ambient, string textureName, bool clampU, bool clampV, bool flipU, bool flipV, bool useAlpha )
        {
            var textureFilePath = textureName != null ? $"{textureName}.png" : null;
            var aiMaterial = new Assimp.Material
            {
                Name              = FormatMaterialName( textureName, Scene.Materials.Count( x => x.TextureDiffuse.FilePath == textureFilePath ) ),
                ColorDiffuse      = ToAssimp( diffuse ),
                ColorSpecular     = ToAssimp( specular ),
                ColorAmbient      = ToAssimp( ambient ),
                Shininess         = 0,
                ShininessStrength = 0,
                Reflectivity      = 0,
            };

            if ( textureName != null )
            {
                aiMaterial.TextureDiffuse = new Assimp.TextureSlot
                {
                    TextureType = Assimp.TextureType.Diffuse,
                    FilePath    = textureFilePath,
                    WrapModeU =
                        clampU ? Assimp.TextureWrapMode.Clamp :
                        flipU  ? Assimp.TextureWrapMode.Mirror :
                                 Assimp.TextureWrapMode.Wrap,
                    WrapModeV =
                        clampV ? Assimp.TextureWrapMode.Clamp :
                        flipV  ? Assimp.TextureWrapMode.Mirror :
                                 Assimp.TextureWrapMode.Wrap,
                };

                if ( useAlpha )
                {
                    aiMaterial.TextureOpacity = new Assimp.TextureSlot
                    {
                        TextureType = Assimp.TextureType.Opacity,
                        FilePath    = textureFilePath,
                        WrapModeU =
                            clampU ? Assimp.TextureWrapMode.Clamp :
                            flipU  ? Assimp.TextureWrapMode.Mirror :
                                     Assimp.TextureWrapMode.Wrap,
                        WrapModeV =
                            clampV ? Assimp.TextureWrapMode.Clamp :
                            flipV  ? Assimp.TextureWrapMode.Mirror :
                                     Assimp.TextureWrapMode.Wrap,
                    };
                }
            }

            return aiMaterial;
        }

        protected static Assimp.Vector3D ToAssimp( Vector3 value )
        {
            return new Assimp.Vector3D( value.X, value.Y, value.Z );
        }

        protected static Assimp.Vector3D ToAssimp( Vector2 value )
        {
            return new Assimp.Vector3D( value.X, value.Y, 0 );
        }

        protected static Assimp.Matrix4x4 ToAssimp( Matrix4x4 matrix )
        {
            return new Assimp.Matrix4x4( matrix.M11, matrix.M21, matrix.M31, matrix.M41,
                                         matrix.M12, matrix.M22, matrix.M32, matrix.M42,
                                         matrix.M13, matrix.M23, matrix.M33, matrix.M43,
                                         matrix.M14, matrix.M24, matrix.M34, matrix.M44 );
        }

        protected static Assimp.Color4D ToAssimp( Color value )
        {
            return new Assimp.Color4D( value.R / 255f,
                                       value.G / 255f,
                                       value.B / 255f,
                                       value.A / 255f );
        }

        protected static Assimp.Scene CreateDefaultScene()
        {
            var aiScene = new Assimp.Scene { RootNode = new Assimp.Node( "RootNode" ) };
            return aiScene;
        }

        protected static void ExportCollada( Assimp.Scene aiScene, string path )
        {
            using ( var aiContext = new Assimp.AssimpContext() )
                aiContext.ExportFile( aiScene, path, "collada", Assimp.PostProcessSteps.JoinIdenticalVertices | Assimp.PostProcessSteps.FlipUVs | Assimp.PostProcessSteps.GenerateSmoothNormals );
        }

        protected static string FormatNodeName( int nodeIndex )
        {
            return $"node_{nodeIndex}";
        }

        protected static string FormatMaterialName(string textureName, int materialIndex)
        {
            return $"{textureName}_material_{materialIndex}";
        }

        protected static string FormatTextureName( int textureId, List<string> textureNames )
        {
            return textureNames != null && textureId < textureNames.Count ? textureNames[ textureId ] : $"texture_{textureId}";
        }

        protected virtual string FormatMeshName( int meshIndex )
        {
            return $"mesh_{meshIndex}";
        }
    }
}
