using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SAModelLibrary.Exceptions;
using SAModelLibrary.IO;
using SAModelLibrary.Maths;
using SAModelLibrary.Utils;

namespace SAModelLibrary.SA2.SOC
{
    public class Model : ISerializableObject
    {
        private const string MAGIC = "SMB001";

        private static readonly string[] sOriginalMaterialNames =
        {
            "lambert2SG1",
            "lambert4",
            "lambert3",
            "lambert5",
        };

        private static readonly string[] sOriginalTextureNames =
        {
            "pic1.dds",
            "pic2.dds",
            "pic.dds",
            "pic3.dds",
        };

        public string     SourceFilePath   { get; set; }
        public long       SourceOffset     { get; set; }
        public Endianness SourceEndianness { get; set; }

        public string Name { get; set; }

        public List<Geometry> Geometries { get; private set; }

        public Model()
        {
            Name       = "NoName";
            Geometries = new List<Geometry>();
        }

        public Model( string filepath )
        {
            using ( var reader = new EndianBinaryReader( filepath, Endianness.Little ) )
                Read( reader );
        }

        public void Save( string filepath )
        {
            using ( var writer = new EndianBinaryWriter( filepath, Endianness.Little ) )
                Write( writer );
        }

        public void ExportCollada( string filepath )
        {
            var aiScene = AssimpHelper.CreateDefaultScene();

            foreach ( var geometry in Geometries )
            {
                for ( var meshIndex = 0; meshIndex < geometry.Meshes.Count; meshIndex++ )
                {
                    var aiMeshNode = new Assimp.Node( geometry.Meshes.Count > 1 ? $"{geometry.Name}_mesh_{meshIndex}" : geometry.Name,
                                                      aiScene.RootNode );
                    aiScene.RootNode.Children.Add( aiMeshNode );

                    var mesh   = geometry.Meshes[ meshIndex ];
                    var aiMesh = new Assimp.Mesh();

                    var aiMaterial = new Assimp.Material
                    {
                        Name              = mesh.Material.Name,
                        //ColorDiffuse      = AssimpHelper.ToAssimp( mesh.Material.Diffuse ),
                        //ColorSpecular     = AssimpHelper.ToAssimp( mesh.Material.Specular ),
                        //ColorAmbient      = AssimpHelper.ToAssimp( mesh.Material.Ambient ),
                        Shininess         = 0,
                        ShininessStrength = 0,
                        Reflectivity      = 0,
                        TextureDiffuse = new Assimp.TextureSlot
                        {
                            TextureType = Assimp.TextureType.Diffuse,
                            FilePath    = mesh.Material.TextureName,
                            WrapModeU   = Assimp.TextureWrapMode.Wrap,
                            WrapModeV   = Assimp.TextureWrapMode.Wrap,
                        }
                    };

                    aiMesh.MaterialIndex = aiScene.MaterialCount;
                    aiScene.Materials.Add( aiMaterial );

                    foreach ( var vertex in mesh.Vertices )
                    {
                        aiMesh.Vertices.Add( AssimpHelper.ToAssimp( vertex.Position ) );
                        aiMesh.Normals.Add( AssimpHelper.ToAssimp( vertex.Normal ) );
                        aiMesh.VertexColorChannels[ 0 ].Add( AssimpHelper.ToAssimp( vertex.Color ) );
                        aiMesh.TextureCoordinateChannels[ 0 ].Add( AssimpHelper.ToAssimp( vertex.UV ) );
                    }

                    for ( int i = 0; i < mesh.Indices.Length; i += 3 )
                    {
                        var aiFace = new Assimp.Face();
                        for ( int j = 0; j < 3; j++ )
                            aiFace.Indices.Add( mesh.Indices[i + j] );

                        aiMesh.Faces.Add( aiFace );
                    }

                    aiMeshNode.MeshIndices.Add( aiScene.MeshCount );
                    aiScene.Meshes.Add( aiMesh );
                }
            }

            AssimpHelper.ExportCollada( aiScene, filepath );
        }

        public static Model Import( string filepath, bool conformanceMode = true )
        {
            var model = new Model();
            Geometry geometry = null;

            if ( conformanceMode )
            {
                geometry = new Geometry { Name = "polySurfaceShape6" };
                model.Geometries.Add( geometry );
            }

            var aiScene = AssimpHelper.ImportScene( filepath, false );

            void ConvertNode( Assimp.Node aiNode )
            {
                if ( aiNode.HasMeshes )
                {
                    var nodeWorldTransform = AssimpHelper.CalculateWorldTransform( aiNode );

                    if ( !conformanceMode )
                        geometry = new Geometry { Name = aiNode.Name };

                    foreach ( var aiMeshIndex in aiNode.MeshIndices )
                    {
                        var aiMesh = aiScene.Meshes[ aiMeshIndex ];
                        var aiMaterial = aiScene.Materials[ aiMesh.MaterialIndex ];

                        var mesh = new Mesh();

                        mesh.Material.Name = aiMaterial.Name;
                        //mesh.Material.Ambient = new Vector4( aiMaterial.ColorAmbient.R, aiMaterial.ColorAmbient.G, aiMaterial.ColorAmbient.B, aiMaterial.ColorAmbient.A );
                        //mesh.Material.Diffuse = new Vector4( aiMaterial.ColorDiffuse.R, aiMaterial.ColorDiffuse.G, aiMaterial.ColorDiffuse.B,aiMaterial.ColorDiffuse.A );
                        //mesh.Material.Specular = new Vector4( aiMaterial.ColorSpecular.R, aiMaterial.ColorSpecular.G, aiMaterial.ColorSpecular.B, aiMaterial.ColorSpecular.A );
                        mesh.Material.TextureName =
                            Path.ChangeExtension( Path.GetFileNameWithoutExtension( aiMaterial.TextureDiffuse.FilePath ), "dds" );

                        mesh.Vertices = new Vertex[aiMesh.VertexCount];
                        for ( int i = 0; i < mesh.Vertices.Length; i++ )
                        {
                            ref var vertex = ref mesh.Vertices[ i ];
                            vertex.Position = Vector3.Transform( AssimpHelper.FromAssimp( aiMesh.Vertices[ i ] ), nodeWorldTransform );
                            vertex.Normal = aiMesh.HasNormals ? Vector3.TransformNormal( AssimpHelper.FromAssimp( aiMesh.Normals[ i ] ), nodeWorldTransform ) : new Vector3();
                            vertex.Color = aiMesh.HasVertexColors( 0 )
                                ? AssimpHelper.FromAssimp( aiMesh.VertexColorChannels[ 0 ][ i ] )
                                : Color.White;
                            vertex.UV = aiMesh.HasTextureCoords( 0 )
                                ? AssimpHelper.FromAssimpAsVector2( aiMesh.TextureCoordinateChannels[ 0 ][ i ] )
                                : new Vector2();
                        }

                        mesh.Indices = aiMesh.GetIndices();
                        geometry.Meshes.Add( mesh );
                    }

                    if ( !conformanceMode )
                        model.Geometries.Add( geometry );
                }

                foreach ( var aiChildNode in aiNode.Children )
                    ConvertNode( aiChildNode );
            }

            ConvertNode( aiScene.RootNode );

            if ( conformanceMode )
            {
                var meshes = new List<Mesh>( geometry.Meshes );

                var meshMap = new List<int>();
                var nextUniqueNewMeshIndex = sOriginalMaterialNames.Length;
                for ( int i = 0; i < meshes.Count; i++ )
                {
                    var newMeshIndex = Array.IndexOf( sOriginalMaterialNames, meshes[ i ].Material.Name );
                    if ( newMeshIndex == -1 )
                        newMeshIndex = nextUniqueNewMeshIndex++;

                    meshMap.Add( newMeshIndex );
                }

                geometry.Meshes.Clear();
                for ( int i = 0; i < nextUniqueNewMeshIndex; i++ )
                {
                    if ( !meshMap.Contains( i ) )
                    {
                        geometry.Meshes.Add( new Mesh
                        {
                            Indices  = new int[0],
                            Vertices = new Vertex[0],
                            Material = new Material
                            {
                                Name        = sOriginalMaterialNames[ i % sOriginalMaterialNames.Length ],
                                TextureName = sOriginalTextureNames[ i % sOriginalTextureNames.Length ]
                            }
                        } );
                    }
                    else
                    {
                        geometry.Meshes.Add( null );
                    }
                }

                for ( int i = 0; i < meshMap.Count; i++ )
                    geometry.Meshes[meshMap[i]] = meshes[i];
            }

            return model;
        }

        private void Read( EndianBinaryReader reader )
        {
            var magic = reader.ReadString( StringBinaryFormat.PrefixedLength32 );
            if ( magic != MAGIC )
                throw new InvalidFileFormatException( "Header magic value does not match the expected value" );

            Name = reader.ReadString( StringBinaryFormat.PrefixedLength32 );
            var geometryCount = reader.ReadInt32();
            Geometries = new List<Geometry>( geometryCount );
            for ( int i = 0; i < geometryCount; i++ )
                Geometries.Add( reader.ReadObject<Geometry>() );
        }

        private void Write( EndianBinaryWriter writer )
        {
            writer.Write( MAGIC, StringBinaryFormat.PrefixedLength32 );
            writer.Write( Name,  StringBinaryFormat.PrefixedLength32 );
            writer.Write( Geometries.Count );
            foreach ( var geometry in Geometries )
                writer.WriteObject( geometry );
        }

        void ISerializableObject.Read( EndianBinaryReader reader, object context ) => Read( reader );
        void ISerializableObject.Write( EndianBinaryWriter writer, object context ) => Write( writer );
    }
}