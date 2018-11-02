using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SAModelLibrary.Maths;

namespace SAModelLibrary.GeometryFormats.Basic
{
    public class BasicAssimpExporter : AssimpExporter
    {
        public static readonly BasicAssimpExporter Animated = new BasicAssimpExporter
        {
            AttachMeshesToParentNode = false,
            RemoveNodes              = false
        };

        public static readonly BasicAssimpExporter Static = new BasicAssimpExporter
        {
            AttachMeshesToParentNode = false,
            RemoveNodes              = true
        };

        private struct Vertex
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Color Color;
            public Vector2 UV;
        }

        private Dictionary<Material, int> mConvertedMaterialCache;

        public BasicAssimpExporter()
        {
            mConvertedMaterialCache = new Dictionary<Material, int>();
        }

        protected override void Initialize( Node rootNode )
        {
            mConvertedMaterialCache = new Dictionary<Material, int>();
        }

        protected override void ConvertGeometry( IGeometry iGeometry, ref Matrix4x4 nodeWorldTransform )
        {
            if ( iGeometry.Format != GeometryFormat.Basic && iGeometry.Format != GeometryFormat.BasicDX )
                throw new InvalidOperationException();

            var geometry = ( Geometry )iGeometry;
            var materialIndexMap = new int[Math.Max( 1, geometry.Materials.Length )];

            if ( geometry.HasMaterials )
            {
                for ( var i = 0; i < geometry.Materials.Length; i++ )
                {
                    // Convert material
                    var material = geometry.Materials[i];
                    if ( !mConvertedMaterialCache.TryGetValue( material, out var aiMaterialIndex ) )
                    {
                        var textureName = material.UseTexture ? FormatTextureName( material.TextureId ) : null;
                        var aiMaterial = CreateMaterial( material.Diffuse, material.Specular, Color.Gray,     textureName, material.ClampU,
                                                         material.ClampV,  material.FlipU,    material.FlipV, material.UseAlpha );

                        aiMaterialIndex = Scene.MaterialCount;
                        Scene.Materials.Add( aiMaterial );
                        mConvertedMaterialCache[material] = aiMaterialIndex;
                    }

                    materialIndexMap[i] = aiMaterialIndex;
                }
            }
            else
            {
                materialIndexMap[ 0 ] = GetNoMaterialMaterialIndex();
            }

            foreach ( var mesh in geometry.Meshes )
            {
                var aiMesh = new Assimp.Mesh();

                // Convert mesh
                var hasNormals = geometry.HasNormals || mesh.HasNormals;
                var triangleIndices = mesh.ToTriangles();
                var vertices = new List<Vertex>();
                for ( var i = 0; i < triangleIndices.Length; i += 3 )
                {
                    var aiFace = new Assimp.Face();

                    for ( int j = 0; j < 3; j++ )
                    {
                        var index = triangleIndices[i + j];
                        var vertex = new Vertex { Position = geometry.VertexPositions[index.VertexIndex] };

                        if ( hasNormals )
                            vertex.Normal = geometry.HasNormals ? geometry.VertexNormals[index.VertexIndex] : index.Normal;

                        if ( mesh.HasColors )
                            vertex.Color = index.Color;

                        if ( mesh.HasUVs )
                            vertex.UV = UVCodec.Decode255( index.UV );

                        var vertexIndex = vertices.IndexOf( vertex );
                        if ( vertexIndex == -1 )
                        {
                            vertexIndex = vertices.Count;
                            vertices.Add( vertex );
                        }

                        aiFace.Indices.Add( vertexIndex );
                    }

                    aiMesh.Faces.Add( aiFace );
                }

                aiMesh.Vertices.AddRange( vertices.Select( x => ToAssimp( x.Position ) ) );

                if ( hasNormals )
                    aiMesh.Normals.AddRange( vertices.Select( x => ToAssimp( x.Normal ) ) );

                if ( mesh.HasColors )
                    aiMesh.VertexColorChannels[0].AddRange( vertices.Select( x => ToAssimp( x.Color ) ) );

                if ( mesh.HasUVs )
                    aiMesh.TextureCoordinateChannels[0].AddRange( vertices.Select( x => ToAssimp( x.UV ) ) );

                // Set mesh material index
                aiMesh.MaterialIndex = materialIndexMap[ Math.Min( mesh.MaterialId, materialIndexMap.Length - 1 ) ];

                // Add mesh to scene
                Scene.Meshes.Add( aiMesh );
            }
        }
    }
}
